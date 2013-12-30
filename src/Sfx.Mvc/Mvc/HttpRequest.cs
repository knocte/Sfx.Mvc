using System;
using Sfx.Collections;

namespace Sfx.Mvc
{
	public sealed class HttpRequest
	{
		StringMap postValues;
		StringMap queryValues;
		StringMap headers;
		Map<IHttpPostedFile> files;
		Map<HttpCookie> cookies;		

		public HttpRequest(string url)
		{
			this.Url = new Uri(url);
			this.queryValues = UrlUtil.ParseQueryString(this.Url.Query);
		}

		public Uri Url { get; set; }
		public string RootDirectory { get; set; }
		public string Method { get; set; }
		public string[] UserLanguages { get; set; }
		public string UserAgent { get; set; }
		public Uri UrlReferrer { get; set; }

		/// <summary>
		/// Obtiene los valores enviados por POST
		/// </summary>
		public StringMap PostValues
		{
			get
			{
				if(this.postValues == null)
				{
					this.postValues = new StringMap();
				}
				return this.postValues; 
			}
			set{ this.postValues = value; }
		}

		/// <summary>
		/// Obtiene los valores enviados por GET
		/// </summary>
		public StringMap QueryValues
		{
			get
			{
				if(this.queryValues == null)
				{
					this.queryValues = new StringMap();
				}
				return this.queryValues; 
			}
			set{ this.queryValues = value; }
		}

		public StringMap Headers
		{
			get
			{
				if(this.headers == null)
				{
					this.headers = new StringMap();
				}
				return this.headers; 
			}
			set{ this.headers = value; }
		}

		public Map<IHttpPostedFile> Files
		{
			get
			{
				if(this.files == null)
				{
					this.files = new Map<IHttpPostedFile>();
				}
				return this.files; 
			}
			set{ this.files = value; }
		}

		public Map<HttpCookie> Cookies
		{
			get
			{
				if(this.cookies == null)
				{
					this.cookies = new Map<HttpCookie>();
				}
				return this.cookies; 
			}
			set{ this.cookies = value; }
		}

		public string this[string key]
		{
			get { return this.Value(key); }
		}

		/// <summary>
		/// Devuelve true si la petición esta hecha por el método POST.
		/// </summary>
		public bool IsPost
		{
			get { return this.Method != null && this.Method.ToUpper() == "POST"; }
		}

		/// <summary>
		/// Devuelve si la petición esta hecha desde localhost
		/// </summary>
		public bool IsLocalhost
		{
			get
			{  
				var host = this.Url.Host;
				return host == "127.0.0.1" || host.Equals("localhost", StringComparison.OrdinalIgnoreCase);
			}
		}

		/// <summary>
		/// Obtiene el valor del parámetro enviado por GET
		/// </summary>
		public string Query(string key)
		{
			return this.queryValues != null ? this.queryValues[key] as String : null;
		}

		/// <summary>
		/// Obtiene el valor del parámetro enviado por POST
		/// </summary>
		public string Post(string key)
		{
			return this.postValues != null ? this.postValues[key] as String : null;
		}

		/// <summary>
		/// Obtiene el valor del parámetro enviado por POST o por GET
		/// </summary>
		public string Value(string key)
		{
			return Post(key) ?? Query(key);
		}
	}
}
























