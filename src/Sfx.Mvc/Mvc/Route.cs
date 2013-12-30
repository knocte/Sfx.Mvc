using System;
using System.Linq;

namespace Sfx.Mvc
{    
	// Representa una ruta mvc
	public sealed class Route
	{
		public string Controller;
		public string Action;

		/// <summary>
		/// Todo lo que pase de module/controller/action son parámetros.
		/// Sirve por ejemplo para meter url's bonitas tipo: /blog/posts/9454/how-to-write-modules.
		/// En ese caso 9454 y how-to-write-modules serían parámetros.
		/// </summary>
		public string[] Parameters;

		public bool TryGetParameterValue(int index, out string value)
		{
			if(this.Parameters != null && this.Parameters.Length > index)
			{
				value = this.Parameters[index];
				return true;
			}

			value = null;
			return false;
		}

		public static Route Parse(Uri url, Settings settings)
		{
			var route = new Route();

			var path = url.AbsolutePath;
			var parts = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();	

			switch(parts.Count)
			{
				case 0:
					route.Controller = settings.DefaultController;
					route.Action = "index";
					break;

				case 1:
					route.Controller = parts[0];
					route.Action = "index";
					break;

				default:
					route.Controller = parts[0];
					route.Action = parts[1];
					break;
			}

			if(parts.Count > 2)
			{
				// todo lo que pase de module/controller/action son parámetros
				route.Parameters = parts.Skip(2).ToArray();
			}

			return route;
		}

		public override string ToString()
		{
			return string.Concat("/", this.Controller.ToLower(), "/", this.Action.ToLower());
		}
	}
}























