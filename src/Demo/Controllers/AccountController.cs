using System;
using Sfx.Mvc;

namespace Demo
{
	public class AccountController : Controller
	{
		public HttpResponse Login()
		{
			if(this.Request.IsPost)
			{
				var user = this.Request["user"];			
				var password = this.Request["password"];
				if(this.Validate(user, password))
				{				
					var callback = this.Request["callback"] ?? "/admin";
					return this.Redirect(callback);
				}
			}

			return this.View();	
		}

		public HttpResponse Logout()
		{
			this.Context.Logout();
			return this.Redirect("/admin");
		}

		bool Validate(string user, string password)
		{
			if(string.IsNullOrEmpty(user))
			{
				this.AddError("user",this.T("Introduzca un valor para {0}", this.T("Usuario")));
			}

			if(string.IsNullOrEmpty(password))
			{
				this.AddError("password", this.T("Introduzca un valor para {0}", this.T("Contraseña")));
			}

			if(this.ViewData.IsValid)
			{
				if(user == "test" && password == "test")
				{
					// Aquí cargaría los datos del usuario
					this.User.Name = "Demo Admin";
					this.User.IdAdmin = 1;
					this.SaveSession();
					return true;
				}
				else
				{
					this.AddError(string.Empty, this.T("Usuario o Contraseña incorrectos"));
				}
			}

			return false;
		}
	}
}



















