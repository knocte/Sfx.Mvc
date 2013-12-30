using System;
using Sfx.Mvc;

namespace Demo
{
	public class ErrorsController : Controller
	{
		public HtmlResponse Index()
		{
			if(this.Request.Value("error") == "on")
			{
				this.ViewData.Errors["name"] = "El valor es inválido";
			}

			this.ViewData.Model = new { name = "Juan" };
			return this.View("~/uidemo/errors");
		}
	}
}

