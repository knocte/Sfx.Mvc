using System;
using Sfx.JSON;

namespace Sfx.Mvc
{
	public class MvcController : Controller<MvcContext>
	{
	}

	public abstract class Controller<T> where T : MvcContext
	{
		ViewData viewData;

		public T Context { get; set; }

		public ViewData ViewData
		{ 
			get
			{
				if(viewData == null)
				{
					viewData = new ViewData(this.Context);
				}

				return viewData;
			}
		}

		public HttpRequest Request 
		{
			get{ return this.Context.Request; }
		}

		public Route Route 
		{
			get{ return this.Context.Route; }
		}

		public RedirectResponse Redirect(string path)
		{
			return new RedirectResponse(path);
		}

		public HttpResponse TextResponse(string text)
		{
			return new ContentResponse(text, "text/plain");
		}

		public InternalErrorResponse InternalError(string error)
		{
			return new InternalErrorResponse(error);
		}		

		public HtmlResponse Html(string html)
		{
			return new HtmlResponse(html);
		}	

		public NotFoundResponse NotFound()
		{
			return new NotFoundResponse();
		}

		public ForbiddenResponse Forbidden()
		{
			return new ForbiddenResponse();
		}

		public HtmlResponse View()
		{
			return View(null);
		}

		public JsonResponse Json(object value)
		{
			return new JsonResponse(Sfx.JSON.Json.Serialize(value));
		}
		
		/// <summary>
		/// Renders the view.
		/// ./ se refiere a serverRoot/
		/// ~/ se refiere a serverRoot/views/
		/// </summary>
		public HtmlResponse View(string viewPath)
		{
			var html = ViewRenderer<T>.Render(viewPath, this.ViewData, this.Context);
			return this.Html(html);
		}

		public void AddError(string key, string error)
		{
			this.ViewData.Errors[key] = error;
		}
	}
}

















