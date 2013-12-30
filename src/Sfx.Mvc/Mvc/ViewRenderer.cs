using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Text;
using Sfx.Templates;

namespace Sfx.Mvc
{
	public static class ViewRenderer<T> where T : MvcContext
	{
		/// <summary>
		/// Renders the view.
		/// ./ se refiere a serverRoot/
		/// ~/ se refiere a serverRoot/views/
		/// </summary>
		public static string Render(string viewPath, ViewData viewData, T context)
		{
			var defaultDir = string.Concat("~/", context.Route.Controller);
			var path = MapViewPath(viewPath, defaultDir, context);			
			var template = Template.Parse(File.ReadAllText(path));
			
			AddBuiltInFunctions(template, viewData, context);
			AddCustomFunctions(template, viewData);

			// los includes toman el directorio de la vista como por defecto.
			var includesDefaultDir = Path.GetDirectoryName(path);
			LoadIncludes(template, includesDefaultDir, viewData, context);

			return template.Render(viewData);
		}

		static void LoadIncludes(Template template, string defaultDir, ViewData viewData, T context)
		{
			string[] includes;
			int includesLength;

			do
			{
				includes = template.Templates.Keys.ToArray();
				includesLength = includes.Length;

				foreach(var includePath in includes)
				{
					var fullPath = MapViewPath(includePath, defaultDir, context);
					template.ParseInclude(includePath, File.ReadAllText(fullPath));
				}
			}
			// al parsear puede que se añadan nuevos includes. 
			// Se deben evitar includes dentro de includes por simplicidad, pero es posible.
			while(includesLength < template.Templates.Count);
		}

		static void AddBuiltInFunctions(Template template, ViewData viewData, T context)
		{
			template.AddFunction("now", () => DateTime.Now);
			template.AddFunction("geterrors", () => GetErrors(viewData));
			template.AddFunction("getError", (string s) => viewData.Errors[s]);
			template.AddFunction("getErrorClass", (string s) => viewData.Errors[s] != null ? "error" : null);
			template.AddFunction("hasError", (string s) => viewData.Errors[s] != null);
			template.AddFunction("getValue", (string s) => GetValue(s, viewData, context));
		}

		static object GetValue(string key, ViewData viewData, T context)
		{
			object value = context.Request[key];
			if(value == null)
			{
				value = GetModelValue(viewData.Model, key);
			}

			return value;
		}

		static object GetModelValue (object model, string key)
		{	
			if(key == null || model == null)
			{
				return null;
			}

			var dic = model as IDictionary;
			if(dic != null)
			{
				return dic[key];
			}

			// si no es un diccionario obtener la propiedad
			var property = model.GetType().GetProperty(key);
			if(property == null)
			{
				//throw new SException("The property %v does not exist", key);
				return null;
			}

			return property.GetValue(model, null);
		}

		static string GetErrors(ViewData viewData)
		{
			if(viewData.IsValid)
			{
				return null;
			}

			var w = new StringBuilder();
			w.Append("<ul>");
			foreach(var error in viewData.Errors.Values)
			{
				w.Append("<li>");
				w.Append(error);
				w.Append("</li>");
			}
			w.Append("</ul>");

			return w.ToString();
		}

		static void AddCustomFunctions(Template template, ViewData viewData)
		{
			foreach(var func in viewData.Functions)
			{
				template.AddFunction(func.Key, func.Value);
			}
		}

		static string MapViewPath(string viewPath, string defaultDir, T context)
		{
			var route = context.Route;
			if(viewPath == null)
			{
				// especificar la ruta por defecto si no se proporciona.
				viewPath = string.Concat(defaultDir, "/", route.Action, ".html");
			}
			// si no tiene directorio especificado
			else if(viewPath.IndexOf('/') == -1)
			{
				viewPath = string.Concat(defaultDir, "/", viewPath);
			}

			var path = viewPath;
			if(path.StartsWith("~/", StringComparison.Ordinal))
			{
				path = Path.Combine(Path.Combine(context.RootDirectory, "Views"), path.Substring(2));
			}

			if(path.StartsWith("./", StringComparison.Ordinal))
			{
				path = Path.Combine(context.RootDirectory, path.Substring(2));
			}

			if(!Path.HasExtension(path))
			{
				path += ".html";
			}

			if(!File.Exists(path))
			{
				throw new ApplicationException("View not found: " + viewPath);
			}

			return path;
		}
	}
}







