using System;
using Sfx.Mvc;

namespace Demo
{
	[Admin]
	public class AdminController : Controller
	{
		public HttpResponse Index()
		{
			return this.View();	
		}
	}
}

