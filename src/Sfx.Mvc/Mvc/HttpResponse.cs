
using Sfx.Collections;

namespace Sfx.Mvc
{
	public class HttpResponse
	{
		Map<HttpCookie> cookies;
		StringMap headers;

		public int Status { get; set; }
		public string ContentType { get; set; }
		public string RedirectUrl { get; set; }
		public object Body { get; set; }

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
	}

	public sealed class HtmlResponse : HttpResponse
	{
		public HtmlResponse(string body)
		{
			this.Status = 200;
			this.Body = body;
			this.ContentType = "text/html";
		}
	}

	public sealed class JsonResponse : HttpResponse
	{
		public JsonResponse(string body)
		{
			this.Status = 200;
			this.Body = body;
			this.ContentType = "application/json";
		}
	}

	public sealed class ContentResponse : HttpResponse
	{
		public ContentResponse(object body, string contentType)
		{
			this.Status = 200;
			this.Body = body;
			this.ContentType = contentType;
		}
	}

	public sealed class RedirectResponse : HttpResponse
	{
		public RedirectResponse(string path)
		{
			this.Status = 303;
			this.RedirectUrl = path;
		}
	}

	public sealed class InternalErrorResponse : HttpResponse
	{
		public InternalErrorResponse() : this("Internal error")
		{
		}

		public InternalErrorResponse(string error)
		{
			this.Status = 500;
			this.Body = error;
			this.ContentType = "text/plain";
		}
	}

	public sealed class NotFoundResponse : HttpResponse
	{
		public NotFoundResponse()
		{
			this.Status = 404;
			this.Body = "Not found";
			this.ContentType = "text/plain";
		}
	}

	public sealed class ForbiddenResponse : HttpResponse
	{
		public ForbiddenResponse()
		{
			this.Status = 401;
			this.Body = "Forbidden";
			this.ContentType = "text/plain";
		}
	}
}


























