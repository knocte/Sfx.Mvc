using System;
using System.Collections.Generic;
using Sfx.Collections;

namespace Sfx.Mvc
{	
	public sealed class ViewData
	{
		StringMap errors;
		Flash flash;
		readonly MvcContext context;
		Map<Delegate> functions;
		ClientScript clientScript;

		public object Model { get; set; }

		public Route Route
		{
			get{ return this.context.Route; }
		}

		public HttpRequest Request
		{
			get{ return this.context.Request; }
		}

		public StringMap Errors 
		{
			get
			{
				if(this.errors == null)
				{
					this.errors = new StringMap();
				}
				return this.errors;
			}
		}

		public bool IsValid
		{
			get{ return this.errors == null || this.errors.Count == 0; }			
		}

		public bool IsNotValid
		{
			get{ return !this.IsValid; }			
		}

		public Flash Flash
		{
			get
			{
				if(this.flash == null)
				{
					this.flash = GetFlash();
				}
				return this.flash;
			}
		}

		public ClientScript ClientScript
		{
			get
			{
				if(this.clientScript == null)
				{
					this.clientScript = new ClientScript();
				}
				return this.clientScript;
			}
		}

		public string CacheBreaker 
		{
			get{ return this.context.Settings.CacheBreaker; }
		}

		public Map<Delegate> Functions 
		{
			get
			{
				if(this.functions == null)
				{
					this.functions = new Map<Delegate>();
				}
				return functions;
			}
		}

		public ViewData(MvcContext context)
		{
			this.context = context;
		}

		public void AddError(string error)
		{
			this.AddError(string.Empty, error);
		}

		public void AddError(string key, string error)
		{
			this.Errors[key] = error;
		}

		public void AddFunction(string name, Func<object> func)
		{
			this.Functions.Add(name, func);
		}

		public void AddFunction<T1>(string name, Func<T1, object> func)
		{
			this.Functions.Add(name, func);
		}

		public void AddFunction<T1, T2>(string name, Func<T1, T2, object> func)
		{
			this.Functions.Add(name, func);
		}

		public void AddFunction<T1, T2, T3>(string name, Func<T1, T2, T3, object> func)
		{
			this.Functions.Add(name, func);
		}

		public void AddFunction<T1, T2, T3, T4>(string name, Func<T1, T2, T3, T4, object> func)
		{
			this.Functions.Add(name, func);
		}

		/// <summary>
		/// Obtiene un mensaje establecido en la petición anterior y lo elimina.
		/// </summary>
		Flash GetFlash()
		{
			var cookieName = "flash";
			var cookie = this.Request.Cookies[cookieName];			
			if(cookie != null)
			{
				var text = UrlUtil.DecodeParameter(cookie.Value);
				var i = text.IndexOf(':');

				var flashMessage = new Flash();
				flashMessage.Type = text.Substring(0, i);
				flashMessage.Text = text.Substring(i + 1);

				// eliminar la cookie para que no vuelva a mostrarse
				this.context.DeleteCookie(cookieName);

				return flashMessage;
			}
			return null;
		}

		/// <summary>
		/// Guarda una cookie con un mensaje para mostrarse en la próxima petición. Se elimina en cuanto se lee.
		/// </summary>
		public void SetFlash(string type, string text)
		{
			if(type == null || text == null)
			{
				throw new ArgumentNullException();
			}

			var cookie = new HttpCookie();		
			cookie.Name = "flash";
			cookie.Value = UrlUtil.EncodeParameter(string.Concat(type, ": ", text));
			cookie.HttpOnly = true;
			cookie.Secure = false;
			this.context.ResponseCookies.Add(cookie.Name, cookie);
		}
	}

	public sealed class Flash
	{
		public string Text { get; set; }
		public string Type { get; set; }
	}
}





























