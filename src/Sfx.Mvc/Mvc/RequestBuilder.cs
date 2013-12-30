using System;
using System.Net;
using System.IO;

namespace Sfx.Mvc
{
	/// <summary>
	/// Convierte una petición Http en una MvcRequest
	/// </summary>
	public static class RequestBuilder
	{
		public static HttpRequest BuildRequest(string path, HttpListenerRequest request)
		{
			// construir la url porque puede ser modificado el path respecto de la petición original
			var uri = request.Url;
			var url = uri.Scheme + "://" + uri.Authority + path + uri.Query;

			var mvcRequest = new HttpRequest(url);
			mvcRequest.Method = request.HttpMethod;
			mvcRequest.UserAgent = request.UserAgent;
			mvcRequest.UserLanguages = request.UserLanguages;
			mvcRequest.UrlReferrer = request.UrlReferrer;

			// Procesar headers
			if (request.Headers.Count > 0)
			{
				foreach (var key in request.Headers.AllKeys)
				{
					mvcRequest.Headers.Add(key, request.Headers[key]);
				}
			}

			// procesar form params y files
			if (request.HttpMethod == "POST")
			{
				ParsePost(request, mvcRequest);
			}

			if (request.Cookies.Count > 0)
			{
				foreach (Cookie c in request.Cookies)
				{
					mvcRequest.Cookies.Add(c.Name, BuildCookie(c));
				}
			}

			return mvcRequest;
		}

		static HttpCookie BuildCookie(Cookie cookie)
		{
			var c = new HttpCookie();
			c.Name = cookie.Name;
			c.Value = cookie.Value;
			c.Domain = cookie.Domain;
			c.Expiration = cookie.Expires;
			c.Secure = cookie.Secure;
			c.Value = cookie.Value;
			c.HttpOnly = cookie.HttpOnly;
			return c;
		}

		static void ParsePost(HttpListenerRequest request, HttpRequest mvcRequest)
		{                
			if (request.ContentType.ToLower().Contains("multipart"))
			{
				ParseMultipart(request, mvcRequest);
			}
			else
			{					
				ParseDefaultPost(request, mvcRequest);
			}
		}

		static void ParseMultipart(HttpListenerRequest request, HttpRequest mvcRequest)
		{                   
			var parser = new HttpMultipartParser(request);                                        
			if (parser.Form.Count > 0)
			{
				foreach (var value in parser.Form)
				{
					mvcRequest.PostValues.Add(value.Key, value.Value);
				}
			}

			if (parser.Files.Count > 0)
			{
				foreach (var item in parser.Files)
				{
					mvcRequest.Files.Add(item.Key, item.Value);
				}
			}
		}

		static void ParseDefaultPost(HttpListenerRequest request, HttpRequest mvcRequest)
		{                   
			using(var reader = new StreamReader(request.InputStream, request.ContentEncoding))
			{
				var text = reader.ReadToEnd();
				if(!string.IsNullOrEmpty(text))
				{
					var parameters = text.Split('&');
					if(parameters.Length > 0)
					{
						foreach(var parameter in parameters)
						{
							if(parameter != string.Empty)
							{
								var items = parameter.Split('=');
								var key = UrlUtil.UrlDecode(items[0]);
								var value = items.Length > 1 ? UrlUtil.UrlDecode(items[1]) : null;
								mvcRequest.PostValues[key] = value;
							}
						}
					}
				}
			}
		}


	}
}

