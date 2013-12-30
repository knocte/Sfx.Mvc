using System;
using System.IO;
using System.Text;
using Sfx.Collections;

namespace Sfx.Mvc
{
    /// <summary>
    /// Utilidades para trabajar con URL's
    /// </summary>
    public static class UrlUtil
	{       
		public static string EncodeParameter(string value)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
		}

		public static string DecodeParameter(string value)
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(value));
		}

		public static string UrlEncode(string text)
		{
			// Sytem.Uri provides reliable parsing
			return Uri.EscapeDataString(text);
		}

		/// <summary>
		/// UrlDecodes a string without requiring System.Web
		/// </summary>
		public static string UrlDecode(string text)
		{
			// http://www.west-wind.com/weblog/posts/2009/Feb/05/Html-and-Uri-String-Encoding-without-SystemWeb
			// pre-process for + sign space formatting since System.Uri doesn't handle it
			// plus literals are encoded as %2b normally so this should be safe
			text = text.Replace("+", " ");
			return Uri.UnescapeDataString(text);
		}

		/// <summary>
		/// Retrieves a value by key from a UrlEncoded string.
		/// </summary>
		public static string GetUrlEncodedKey(string urlEncoded, string key)
		{
			urlEncoded = "&" + urlEncoded + "&";

			int Index = urlEncoded.IndexOf("&" + key + "=",StringComparison.OrdinalIgnoreCase);
			if (Index < 0)
				return "";

			int lnStart = Index + 2 + key.Length;

			int Index2 = urlEncoded.IndexOf("&", lnStart);
			if (Index2 < 0)
				return "";

			return UrlDecode(urlEncoded.Substring(lnStart, Index2 - lnStart));
		}

        /// <summary>
        /// Obtienen el content type del recurso especificado por la URL.
        /// </summary>
        public static string GetContentType(Uri uri)
        {
            var extension = Path.GetExtension(uri.AbsolutePath);
            return GetContentType(extension);
        }
        
        /// <summary>
        /// Obtienen el content type del recurso especificado por la URL.
        /// </summary>
        public static string GetContentType(string extension)
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

        /// <summary>
        /// Obtiene el subdominio de la URL
        /// </summary>
        public static string GetSubdomain(Uri uri)
        {
            if (uri.HostNameType == UriHostNameType.Dns)
            {
                string host = uri.Host;
                if (host.Split('.').Length > 2)
                {
                    int lastIndex = host.LastIndexOf(".");
                    int index = host.LastIndexOf(".", lastIndex - 1);
                    return host.Substring(0, index);
                }
            }

            return null;
        }

		/// <summary>
		/// Modifica o añade si no existe el parámetro a la querystring
		/// </summary>
		public static string SetQueryValue(string url, string param, string value)
		{
			// si no tiene ningun parámetro
			var indexParams = url.IndexOf('?');
			if(indexParams == -1)
			{
				return string.Concat(url, "?", param, "=", value);
			}

			// si el parametro no existe
			var indexParam = url.IndexOf(param + "=", indexParams);
			if(indexParam == -1)
			{
				return string.Concat(url, "&", param, "=", value);
			}

			// si es el último
			var indexEndParam = url.IndexOf("&", indexParam);
			if(indexEndParam == -1)
			{
				return string.Concat(url.Substring(0, indexParam), param, "=", value);
			}

			// El parámetro está entre otros. remplazar
			return string.Concat(url.Substring(0, indexParam), param, "=", value, url.Substring(indexEndParam));
		}

        public static StringMap ParseQueryString(string url)
        {
			var items = new StringMap();

            // parse the querystring if needed
			if (url.Length > 0)
            {
                var querySegments = url.TrimStart('?').Split('&');
                foreach (string segment in querySegments)
                {
					string key;
					string value;

					var index = segment.IndexOf('=');
					if(index == -1)
					{
						key = segment;
						value = string.Empty;
					}
					else
					{
						key = segment.Substring(0, index);
						value = segment.Substring(index + 1);
					}

					items[UrlUtil.UrlDecode(key)] = UrlUtil.UrlDecode(value);
                }
            }

            return items;
        }
    }
}
