using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Sfx.Mvc
{
    // Most of this class is sourced from the MONO project in:
    //
    // System.Web.HttpRequest.cs 
    //
    // 
    // Author:
    //	Miguel de Icaza (miguel@novell.com)
    //	Gonzalo Paniagua Javier (gonzalo@novell.com)
    //      Marek Habersack <mhabersack@novell.com>
    //

    //
    // Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
    // Copyright (C) 2011-2012 Xamarin, Inc (http://xamarin.com)
    //
    // Permission is hereby granted, free of charge, to any person obtaining
    // a copy of this software and associated documentation files (the
    // "Software"), to deal in the Software without restriction, including
    // without limitation the rights to use, copy, modify, merge, publish,
    // distribute, sublicense, and/or sell copies of the Software, and to
    // permit persons to whom the Software is furnished to do so, subject to
    // the following conditions:
    // 
    // The above copyright notice and this permission notice shall be
    // included in all copies or substantial portions of the Software.
    // 
    // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
    // LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
    // OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
    // WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    
    
    //
    // Stream-based multipart handling.
    //
    // In this incarnation deals with an HttpInputStream as we are now using
    // IntPtr-based streams instead of byte [].   In the future, we will also
    // send uploads above a certain threshold into the disk (to implement
    // limit-less HttpInputFiles). 
    //

    // Also some code from 
    // https://github.com/ServiceStack/ServiceStack/blob/master/src/ServiceStack/WebHost.EndPoints/Extensions/HttpListenerRequestWrapper.cs
    // 

	sealed class HttpMultipartParser
    {
        private readonly HttpListenerRequest request;

        public Dictionary<string, string> Form { get; set; }
        public Dictionary<string, HttpPostedFile> Files { get; set; }

        public HttpMultipartParser(HttpListenerRequest request)
        {
            this.request = request;
            this.LoadMultiPart();
        }

        void LoadMultiPart()
        {
            this.Form = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.Files = new Dictionary<string, HttpPostedFile>(StringComparer.OrdinalIgnoreCase);

            var contentType = this.request.ContentType;

            if (contentType == null)
            {
                return;
            }

            string boundary = GetParameter(contentType, "; boundary=");

            if (boundary == null)
            {
                return;
            }

            Stream input = GetSubStream(this.request.InputStream);

            //DB: 30/01/11 - Hack to get around non-seekable stream and received HTTP request
            //Not ending with \r\n?
            var ms = new MemoryStream(32 * 1024);
            input.CopyTo(ms);
            input = ms;
            ms.WriteByte((byte)'\r');
            ms.WriteByte((byte)'\n');

            input.Position = 0;

            //Uncomment to debug
            //var content = new StreamReader(ms).ReadToEnd();
            //Console.WriteLine(boundary + "::" + content);
            //input.Position = 0;

            HttpMultipart multi_part = new HttpMultipart(input, boundary, this.request.ContentEncoding);

            HttpMultipart.Element e;
            while ((e = multi_part.ReadNextElement()) != null)
            {
                if (e.Filename == null)
                {
                    byte[] copy = new byte[e.Length];

                    input.Position = e.Start;
                    input.Read(copy, 0, (int)e.Length);

                    Form.Add(e.Name, this.request.ContentEncoding.GetString(copy));
                }
                else if(e.Length > 0)
                {
                    //
                    // We use a substream, as in 2.x we will support large uploads streamed to disk,
                    //
                    HttpPostedFile sub = new HttpPostedFile(e.Filename, e.ContentType, input, e.Start, e.Length);
                    Files.Add(e.Name, sub);
                }
            }
        }

        static internal string GetParameter(string header, string attr)
        {
            int ap = header.IndexOf(attr);
            if (ap == -1)
                return null;

            ap += attr.Length;
            if (ap >= header.Length)
                return null;

            char ending = header[ap];
            if (ending != '"')
                ending = ' ';

            int end = header.IndexOf(ending, ap + 1);
            if (end == -1)
                return (ending == '"') ? null : header.Substring(ap);

            return header.Substring(ap + 1, end - ap - 1);
        }

        // GetSubStream returns a 'copy' of the InputStream with Position set to 0.
        static Stream GetSubStream(Stream stream)
        {
            if (stream is MemoryStream)
            {
                MemoryStream other = (MemoryStream)stream;
                return new MemoryStream(other.GetBuffer(), 0, (int)other.Length, false, true);
            }

            return stream;
        }

        //
        // Stream-based multipart handling.
        //
        // In this incarnation deals with an HttpInputStream as we are now using
        // IntPtr-based streams instead of byte [].   In the future, we will also
        // send uploads above a certain threshold into the disk (to implement
        // limit-less HttpInputFiles). 
        //

        class HttpMultipart
        {
            public class Element
            {
                public string ContentType;
                public string Name;
                public string Filename;
                public long Start;
                public long Length;

                public override string ToString()
                {
                    return "ContentType " + ContentType + ", Name " + Name + ", Filename " + Filename + ", Start " +
                        Start.ToString() + ", Length " + Length.ToString();
                }
            }

            Stream data;
            string boundary;
            byte[] boundary_bytes;
            byte[] buffer;
            bool at_eof;
            Encoding encoding;
            StringBuilder sb;

            const byte HYPHEN = (byte)'-', LF = (byte)'\n', CR = (byte)'\r';

            // See RFC 2046 
            // In the case of multipart entities, in which one or more different
            // sets of data are combined in a single body, a "multipart" media type
            // field must appear in the entity's header.  The body must then contain
            // one or more body parts, each preceded by a boundary delimiter line,
            // and the last one followed by a closing boundary delimiter line.
            // After its boundary delimiter line, each body part then consists of a
            // header area, a blank line, and a body area.  Thus a body part is
            // similar to an RFC 822 message in syntax, but different in meaning.

            public HttpMultipart(Stream data, string b, Encoding encoding)
            {
                this.data = data;
                boundary = b;
                boundary_bytes = encoding.GetBytes(b);
                buffer = new byte[boundary_bytes.Length + 2]; // CRLF or '--'
                this.encoding = encoding;
                sb = new StringBuilder();
            }

            string ReadLine()
            {
                // CRLF or LF are ok as line endings.
                bool got_cr = false;
                int b = 0;
                sb.Length = 0;
                while (true)
                {
                    b = data.ReadByte();
                    if (b == -1)
                    {
                        return null;
                    }

                    if (b == LF)
                    {
                        break;
                    }
                    got_cr = (b == CR);
                    sb.Append((char)b);
                }

                if (got_cr)
                    sb.Length--;

                return sb.ToString();

            }

            static string GetContentDispositionAttribute(string l, string name)
            {
                int idx = l.IndexOf(name + "=\"");
                if (idx < 0)
                    return null;
                int begin = idx + name.Length + "=\"".Length;
                int end = l.IndexOf('"', begin);
                if (end < 0)
                    return null;
                if (begin == end)
                    return "";
                return l.Substring(begin, end - begin);
            }

            string GetContentDispositionAttributeWithEncoding(string l, string name)
            {
                int idx = l.IndexOf(name + "=\"");
                if (idx < 0)
                    return null;
                int begin = idx + name.Length + "=\"".Length;
                int end = l.IndexOf('"', begin);
                if (end < 0)
                    return null;
                if (begin == end)
                    return "";

                string temp = l.Substring(begin, end - begin);
                byte[] source = new byte[temp.Length];
                for (int i = temp.Length - 1; i >= 0; i--)
                    source[i] = (byte)temp[i];

                return encoding.GetString(source);
            }

            bool ReadBoundary()
            {
                try
                {
                    string line = ReadLine();

                    while (line == "")
                        line = ReadLine();
                    if (line == null || line[0] != '-' || line[1] != '-')
                        return false;

                    if (!line.EndsWith(boundary, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                catch
                {
                }

                return false;
            }

            string ReadHeaders()
            {
                string s = ReadLine();
                if (s == "")
                    return null;

                return s;
            }

            bool CompareBytes(byte[] orig, byte[] other)
            {
                for (int i = orig.Length - 1; i >= 0; i--)
                    if (orig[i] != other[i])
                        return false;

                return true;
            }

            long MoveToNextBoundary()
            {
                long retval = 0;
                bool got_cr = false;

                int state = 0;
                int c = data.ReadByte();
                while (true)
                {
                    if (c == -1)
                        return -1;

                    if (state == 0 && c == LF)
                    {
                        retval = data.Position - 1;
                        if (got_cr)
                            retval--;
                        state = 1;
                        c = data.ReadByte();
                    }
                    else if (state == 0)
                    {
                        got_cr = (c == CR);
                        c = data.ReadByte();
                    }
                    else if (state == 1 && c == '-')
                    {
                        c = data.ReadByte();
                        if (c == -1)
                            return -1;

                        if (c != '-')
                        {
                            state = 0;
                            got_cr = false;
                            continue; // no ReadByte() here
                        }

                        int nread = data.Read(buffer, 0, buffer.Length);
                        int bl = buffer.Length;
                        if (nread != bl)
                            return -1;

                        if (!CompareBytes(boundary_bytes, buffer))
                        {
                            state = 0;
                            data.Position = retval + 2;
                            if (got_cr)
                            {
                                data.Position++;
                                got_cr = false;
                            }
                            c = data.ReadByte();
                            continue;
                        }

                        if (buffer[bl - 2] == '-' && buffer[bl - 1] == '-')
                        {
                            at_eof = true;
                        }
                        else if (buffer[bl - 2] != CR || buffer[bl - 1] != LF)
                        {
                            state = 0;
                            data.Position = retval + 2;
                            if (got_cr)
                            {
                                data.Position++;
                                got_cr = false;
                            }
                            c = data.ReadByte();
                            continue;
                        }
                        data.Position = retval + 2;
                        if (got_cr)
                            data.Position++;
                        break;
                    }
                    else
                    {
                        // state == 1
                        state = 0; // no ReadByte() here
                    }
                }

                return retval;
            }

            public Element ReadNextElement()
            {
                if (at_eof || ReadBoundary())
                    return null;

                Element elem = new Element();
                string header;
                while ((header = ReadHeaders()) != null)
                {
                    if (header.StartsWith("Content-Disposition:", StringComparison.OrdinalIgnoreCase))
                    {
                        elem.Name = GetContentDispositionAttribute(header, "name");
                        elem.Filename = StripPath(GetContentDispositionAttributeWithEncoding(header, "filename"));
                    }
                    else if (header.StartsWith("Content-Type:", StringComparison.OrdinalIgnoreCase))
                    {
                        elem.ContentType = header.Substring("Content-Type:".Length).Trim();
                    }
                }

                long start = data.Position;
                elem.Start = start;
                long pos = MoveToNextBoundary();
                if (pos == -1)
                    return null;

                elem.Length = pos - start;
                return elem;
            }

            static string StripPath(string path)
            {
                if (path == null || path.Length == 0)
                    return path;

                if (path.IndexOf(":\\") != 1 && !path.StartsWith("\\\\"))
                    return path;
                return path.Substring(path.LastIndexOf('\\') + 1);
            }
        }
    }

}