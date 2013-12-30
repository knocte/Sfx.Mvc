using System;
using Sfx.Mvc;

namespace Demo
{
	public class MainController : Controller
	{
		public HttpResponse Index()
		{
			return this.Redirect("/admin");
		}
	}
}

