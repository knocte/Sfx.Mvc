using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;
using System.Text;

namespace Sfx.Mvc
{    	
	delegate bool RawRequestHandler(HttpListenerContext context);

	class HttpServerBase
	{
        HttpListener listener;
        List<Thread> workerThreads;
        bool run;

        public int Port { get; private set; }
		public string RootDirectory { get; private set; }
		public Logger EventLogger { get; set; }
        public int Threads { get; set; }
		
		public event RawRequestHandler OnRequest;

		public HttpServerBase(int port, string rootDirectory)
		{			
			this.Port = port;
			this.RootDirectory = rootDirectory;
			this.Threads = Environment.ProcessorCount * 2;
		}
        
        public void Start()
        {
            this.Start(true);
		}
        
        /// <summary>
        /// Arranca el servidor. Si wait es true espera a que finalice el servidor.
        /// </summary>
        public void Start(bool wait)
        {
            if (!HttpListener.IsSupported)
            {
				throw new ApplicationException("HttpListener is not supported on this platform.");
            }

			this.PrintLog("Starting Sfx Server....");

            try
            {
                this.listener = new HttpListener();
				this.listener.IgnoreWriteExceptions = true;
                ServicePointManager.DefaultConnectionLimit = 5000;
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.MaxServicePoints = 5000;
                
                this.listener.Prefixes.Add(string.Format("http://+:{0}/", this.Port));
                this.listener.Start();
                run = true;

                this.workerThreads = new List<Thread>();

				var threads = this.Threads > 0 ? this.Threads : Environment.ProcessorCount * 2;

				for (int i = 0; i < threads; i++)
                {
                    var thread = new Thread(new ThreadStart(ListenerThread));
                    this.workerThreads.Add(thread);
                    thread.IsBackground = true;
					thread.Start();
                }

				this.PrintLog("Webserver started");

                if (wait)
                {
                    foreach (var t in this.workerThreads)
                    {
                        t.Join();
                    }
                }
            } 
            catch (Exception ex)
            {
				this.PrintLog(ex.ToString());
                throw;
            }
        }

		public void Wait()
        {
            if (run)
            {
                foreach (var t in this.workerThreads)
                {
                    t.Join();
                }
            }
        }

        public void Stop()
		{
			this.PrintLog("Stopping S Server....");
			this.run = false;

			try
			{
				this.listener.Stop();
				this.listener.Close();
			}
			catch (ObjectDisposedException)
			{
				// TODO: salta en mono, no en .net
			}
        }

        void ListenerThread ()
		{
			while(this.run && listener.IsListening)
			{
				try
				{
					var context = listener.GetContext();

					try
					{
						ProcessRequest(context);
					}
					catch(Exception ex)
					{
						Console.WriteLine(ex);
					}

					try
					{
						context.Response.Close();
						context.Response.OutputStream.Close();
						context.Response.OutputStream.Dispose();
					}
					catch(Exception ex)
					{
						Console.WriteLine(ex);
					}
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}

        void ProcessRequest(HttpListenerContext context)
		{				
			context.Response.Headers["Server"] = "Sfx 1.1";
			context.Response.KeepAlive = false;

			try
			{					
				bool handled = false;

				// Intentar servir la petición si procesar
				if(this.OnRequest != null)
				{
					handled = this.OnRequest(context);
				}
					
				// intentar servir un archivo físico.
				if(!handled)
				{
					handled = ServeFile(context.Request.Url, context.Response);
				}
                
				// si llega aqui es un 404
				if(!handled)
				{
					ServeError("Page not found", HttpStatus.NOT_FOUND, context.Response);
				}
			}
			catch(Exception ex)
			{
				var errorMessage = ex.ToString();
				this.PrintLog(errorMessage);                	
				errorMessage = errorMessage.Replace("\n", "<br />");
				ServeError(errorMessage, HttpStatus.INTERNAL_ERRROR, context.Response);
			}
		}

		bool ServeFile(Uri url, HttpListenerResponse httpResponse)
		{		
			if(this.RootDirectory == null)
			{
				// no servir nada físico si no se ha establecido www root
				return false;
			}

			var path = url.LocalPath;

			// IMPORTANTE: Validación de seguridad!
			if(path.Contains(".."))
			{
				// Proteger contra path traversal. Directamente no servir nada
				// que contenga "escalar" directorios.
				return false;
			}

			// todas las rutas deben especificarse absolutas desde www root.
			if(path[0] != '/')
			{
				return false;
			}

			// el archivo por defecto es index.html
			if(Path.GetFileName(path) == string.Empty)
			{
				path += "index.html";
			}

			// hacer la ruta absoluta
			path = Path.Combine(this.RootDirectory, path.TrimStart('/'));

			return ServeFile(path, httpResponse);
		}                                

		static bool ServeFile(string path, HttpListenerResponse httpResponse)
		{		
			if(!File.Exists(path))
			{
				return false;
			}

			// http://stackoverflow.com/a/13386573/4264
			using(var fs = File.OpenRead(path))
			{
				httpResponse.StatusCode = (int)HttpStatusCode.OK;
				httpResponse.ContentLength64 = fs.Length;
				httpResponse.SendChunked = false;
				httpResponse.ContentType = GetContentType(Path.GetExtension(path));

				if(httpResponse.ContentType == "application/octet-stream")
				{
					httpResponse.AddHeader("Content-disposition", "attachment; filename=" +
						Path.GetFileName(path));
				}

				byte[] buffer = new byte[64 * 1024];
				int read;
				using(BinaryWriter bw = new BinaryWriter(httpResponse.OutputStream))
				{
					while((read = fs.Read(buffer, 0, buffer.Length)) > 0)
					{
						bw.Write(buffer, 0, read);
						bw.Flush(); //seems to have no effect
					}
				}
			}

			return true;
		}

		static void ServeError(string text, int httpStatus, HttpListenerResponse httpResponse)
		{
			var buffer = Encoding.UTF8.GetBytes(text);
			httpResponse.ContentType = GetContentType(".html");
			httpResponse.StatusCode = httpStatus;
			httpResponse.ContentLength64 = buffer.LongLength;
			httpResponse.OutputStream.Write(buffer, 0, buffer.Length);
		}  

		static string GetContentType(string extension)
		{
			switch (extension.ToLower())
			{
				case ".mp3": return "audio/mpeg3";
				case ".htm": return "text/html";
				case ".html": return "text/html";
				case ".css": return "text/css";
				case ".js": return "text/javascript";
				case ".bmp": return "image/ms-bmp";
				case ".png": return "image/png";
				case ".jpeg": return "image/jpeg";
				case ".jpg": return "image/jpeg";
				case ".gif": return "image/gif";
				case ".swf": return "application/x-shockwave-flash";
				case ".json": return "application/json";
				default: return "application/octet-stream";
			}
		}

		void PrintLog(string value)
		{
			if(this.EventLogger != null)
			{
				this.EventLogger.Print(value);
			}
			else
			{
				Console.WriteLine(value);
			}
		}
    }

	// El código de estado de una respuesta HTTP
	public static class HttpStatus
	{
		public const int OK = 200;
		public const int MOVED_PERMANENTLY = 301;
		public const int FOUND = 302;
		public const int SEE_OTHER = 303;
		public const int TEMPORARY_REDIRECT = 307;
		public const int NOT_FOUND = 404;
		public const int INTERNAL_ERRROR = 500;
	}
}








































