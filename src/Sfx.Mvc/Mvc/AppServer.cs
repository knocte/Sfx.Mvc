using System;
using System.IO;
using System.Net;
using System.Text;
using System.Diagnostics;
using Sfx.Collections;
using Sfx.Templates;

namespace Sfx.Mvc
{
	public sealed class AppServer
	{
		readonly Sessions sessions;
		readonly Settings settings;
		HttpServerBase httpServer;
		string rootDirectory;
		Logger accessLogger;
		bool printErrors;
		Dispatcher dispatcher;

		public AppServer(Settings settings)
		{
			this.settings = settings;
			this.sessions = new Sessions(settings);
			this.Init();
		}

		MvcContext CreateContext(HttpRequest request)
		{
			var context = new MvcContext(request, this.settings, this.sessions);
			context.Route = Route.Parse(request.Url, this.settings);
			context.Settings = this.settings;
			context.Request = request;	
			return context;
		}

		public void Start()
		{
			this.httpServer.Start();
		}

		void Init()
		{
			this.dispatcher = new Dispatcher();
			this.dispatcher.Settings = settings;

			var port = settings.HttpPort;
			this.rootDirectory = settings.RootDirectory;
			if(this.rootDirectory == null)
			{
				throw new ApplicationException("RootDirectory is not set");
			}

			var publicDir = Path.Combine(this.rootDirectory, "Public");
			this.httpServer = new HttpServerBase(port, publicDir);
			this.httpServer.OnRequest += HandleOnRequest;

			var logFile = settings.Values["accessLog"] as string;
			if(logFile != null)
			{
				accessLogger = new Logger(logFile);
			}

			this.printErrors = settings.Values["printErrors"] as bool? ?? false;
		}

		bool HandleOnRequest(HttpListenerContext context)
		{
			Stopwatch sw = null;
			if(this.accessLogger != null)
			{
				sw = new Stopwatch();
				sw.Start();
			}

			var path = context.Request.Url.LocalPath;
			var extension = Path.GetExtension(path).ToLower();
			if(extension.Length > 0)
			{
				// ignorar peticiones con extensión, son archivos estáticos
				return false;
			}

			HttpRequest request = null;
			HttpResponse response = null;
			try
			{
				request = RequestBuilder.BuildRequest(path, context.Request);
				request.RootDirectory = this.rootDirectory;

				var mvcContext = this.CreateContext(request);
				response = this.dispatcher.Invoke(mvcContext);
			}
			catch(Exception e)
			{
				Logger.Default.Print(e);
				try
				{
					response = ServeError(e);
				}
				catch
				{
					response = new InternalErrorResponse();
				}
			}

			if(response == null)
			{
				return false;
			}

			ServeMvcResponse(context.Response, response);

			if(this.accessLogger != null)
			{
				sw.Stop();
				this.accessLogger.Print(string.Concat(request.Method, " ", request.Url.OriginalString, " ", sw.Elapsed.TotalMilliseconds.ToString("f"), "m"));
			}

			return true;
		}

		HttpResponse ServeError(Exception ex)
		{
			var error = this.printErrors ? ex.ToString() : "<h1>Internal Error</h1>";
			var template = LoadErrorTemplate();
			if(template != null)
			{
				error = template.Render(error);
			}
			return new HtmlResponse(error);
		}

		Template LoadErrorTemplate()
		{
			var path = Path.Combine(this.rootDirectory, "Views/error.html");
			if(File.Exists(path))
			{
				return Template.ParseFile(path);
			}
			return null;
		}

		static bool ServeMvcResponse(HttpListenerResponse httpResponse, HttpResponse mvcResponse)
		{
			var status = mvcResponse.Status;
			httpResponse.StatusCode = status;
			httpResponse.ContentType = mvcResponse.ContentType;
			httpResponse.RedirectLocation = mvcResponse.RedirectUrl;

			// headers
			if (mvcResponse.Headers.Count > 0)
			{
				foreach (var header in mvcResponse.Headers)
				{
					httpResponse.AppendHeader(header.Key as String, header.Value as String);
				}
			}

			// cookies
			var cookies = mvcResponse.Cookies;
			if(cookies != null && cookies.Count > 0)
			{
				foreach (var cookie in cookies.Values)
				{
					var cookieHeader = GetHeaderValue(cookie);
					httpResponse.Headers.Add("Set-Cookie", cookieHeader);
				}
			}

			// body
			var body = mvcResponse.Body;
			if (body != null)
			{
				byte[] buffer;

				var byteArrayBody = body as byte[];                
				if (byteArrayBody != null)
				{
					buffer = byteArrayBody;
				}
				else
				{
					// si no son bytes tratarlo siempre como un string.
					buffer = Encoding.UTF8.GetBytes(body.ToString());
				}

				httpResponse.ContentLength64 = buffer.LongLength;
				httpResponse.OutputStream.Write(buffer, 0, buffer.Length);
			}

			return true;
		}

		static string GetHeaderValue(HttpCookie cookie)
		{
			var sb = new StringBuilder();			
			sb.AppendFormat("{0}={1};path={2}", cookie.Name, cookie.Value, "/");

			if (cookie.Expiration.HasValue)
			{
				sb.AppendFormat(";expires={0}", cookie.Expiration.Value.ToString("R"));
			}

			var domain = cookie.Domain;
			if (domain != null)
			{
				sb.AppendFormat(";domain={0}", domain);
			}

			if (cookie.Secure)
			{
				sb.Append(";Secure");
			}

			if (cookie.HttpOnly)
			{
				sb.Append(";HttpOnly");
			}

			return sb.ToString();
		} 
	}
}




























