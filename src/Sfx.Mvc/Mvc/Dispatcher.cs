using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Sfx.Collections;

namespace Sfx.Mvc
{    
	/// <summary>
	/// The Dispatcher is responsible for invoking Mvc request calls.
	/// </summary>
	public sealed class Dispatcher
	{
		readonly object syncLock = new object();
		Map<ActionInfo> actions;

		public Settings Settings { get; set; }

		public Map<ActionInfo> Actions
		{
			get
			{
				if(actions == null)
				{
					InitializeActions();
				}
				return actions;
			}
		}

		public HttpResponse Invoke(MvcContext context)
		{
			var path = context.Route.ToString();

			ActionInfo action;
			if(Actions.TryGetValue(path, out action))
			{		
				var response = Invoke(action, context);
				ApplyResponseHeaders(response, context);
				return response;
			}

			return null;
		}

		void ApplyResponseHeaders(HttpResponse response, MvcContext context)
		{
			foreach(var header in context.ResponseHeaders)
			{
				response.Headers[header.Key] = header.Value;
			}

			foreach(var cookie in context.ResponseCookies)
			{
				response.Cookies[cookie.Key] = cookie.Value;
			}
		}

		HttpResponse Invoke(ActionInfo action, MvcContext context)
		{
			// Ejecuta las validaciones de seguridad, HttpPost, etc...
			var response = ExecuteFilters(action, context);
			if(response != null)
			{
				return response;
			}

			var method = action.MethodInfo;
			if(method.IsStatic)
			{
				var parameters = new object[] { context };
				return method.Invoke(null, parameters) as HttpResponse;
			}
			else
			{
				var controller = Activator.CreateInstance(action.ControllerType) as Controller;
				if(controller == null)
				{
					return null;
				}

				controller.Context = context;

				try
				{
					response = method.Invoke(controller, null) as HttpResponse;
				}
				catch(TargetInvocationException ex)
				{
					// interesa únicamente el origen de la excepción
					throw ex.InnerException;
				}

				return response;
			}
		}

		static HttpResponse ExecuteFilters(ActionInfo action, MvcContext context)
		{
			// Execute controller filters
			foreach(FilterAttribute filter in action.ControllerType.GetCustomAttributes(typeof(FilterAttribute), true))
			{
				var response = filter.Execute(context);
				if(response != null)
				{
					return response;
				}
			}
			// Execute action filters
			foreach(FilterAttribute filter in action.MethodInfo.GetCustomAttributes(typeof(FilterAttribute), true))
			{
				var response = filter.Execute(context);
				if(response != null)
				{
					return response;
				}
			}
			return null;
		}

		void InitializeActions()
		{			
			const BindingFlags flags = BindingFlags.IgnoreCase | 
										BindingFlags.Static | 
										BindingFlags.Instance | 
										BindingFlags.Public | 
										BindingFlags.FlattenHierarchy;

			lock(syncLock)
			{
				actions = new Map<ActionInfo>();

				foreach(var assembly in LoadAssemblies())
				{
					foreach(var type in assembly.GetTypes())
					{
						if(IsController(type))
						{
							foreach(var method in type.GetMethods(flags))
							{
								if(IsAction(method))
								{
									var path = GetPath(type, method);
									var action = new ActionInfo();
									action.ControllerType = type;
									action.MethodInfo = method;
									actions.Add(path, action);
								}
							}
						}
					}
				}
			}
		}

		static string GetPath(Type type, MethodInfo info)
		{
			const string ControllerSufix = "Controller";
			var controller = type.Name.Substring(0, type.Name.Length - ControllerSufix.Length);
			var action = info.Name;
			return string.Concat("/", controller, "/", action).ToLower();
		}

		// Sirve para almacenar el tipo porque si es un controller abstracto
		// no se instancia correctamente llamando a action.DeclaringType
		public sealed class ActionInfo
		{
			public MethodInfo MethodInfo { get; set; }
			public Type ControllerType { get; set; }
		}

		static bool IsAction(MethodInfo method)
		{
			// una acción tiene que devolver HttpResponse
			if(!typeof(HttpResponse).IsAssignableFrom(method.ReturnType))
			{
				return false;
			}
			// una acción puede tener un único argumento HttpRequest o ninguno si es estática
			var parameters = method.GetParameters();
			var length = parameters.Length;
			if(length > 1)
			{
				return false;
			}
			else
				if(length == 1)
				{
					if(!method.IsStatic)
					{
						return false;
					}
					if(!typeof(MvcContext).IsAssignableFrom(parameters[0].ParameterType))
					{
						return false;
					}
				}
			return true;
		}

		static bool IsController(Type type)
		{
			if(!type.IsPublic)
			{
				return false;
			}
			if(!typeof(Controller).IsAssignableFrom(type))
			{
				return false;
			}
			if(type.IsAbstract && !type.IsSealed)
			{
				return false;
			}
			return true;
		}

		static List<Assembly> LoadAssemblies()
		{
			var assemblies = new List<Assembly>();
			var root = GetExecutingDirectory();
			foreach(var file in Directory.GetFiles(root))
			{
				var extension = Path.GetExtension(file).ToLower();
				if(!extension.Equals(".exe") && !extension.Equals(".dll"))
				{
					continue;
				}
				if(file.EndsWith("sqlite3.dll", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				if(file.EndsWith("mysql.data.dll", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				try
				{
					assemblies.Add(Assembly.LoadFrom(file));
				}
				catch(BadImageFormatException)
				{
					// no hacer nada pq es código nativo
				}
			}
			return assemblies;
		}

		static string GetExecutingDirectory()
		{
			//Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			return Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
		}
	}
}
    
