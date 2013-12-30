using System;

namespace Sfx.Mvc
{
	/// <summary>
	/// Sirve para interceptar y poder alterar la ejecución de llamadas.
	/// </summary>
	public abstract class FilterAttribute : Attribute
	{
		// Se ejecuta en cada petición. Si devuelve null la ejecución prosigue normalmente.
		// Si devuelve una respuesta la ejecución se corta.
		public abstract HttpResponse Execute(MvcContext context);
	}
}

