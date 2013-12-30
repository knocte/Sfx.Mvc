using System;

namespace Sfx.Mvc
{
	// Sirve para lanzar excepciones con un mensaje válido para mostrar directamente a los usuarios de la aplicación.
	public sealed class SfxException : Exception
	{
		public string PublicMessage { get; private set; }

		public SfxException(string publicMessage, string debugMessage, Exception inner) : base(debugMessage, inner)
		{
			this.PublicMessage = publicMessage;
		}

		public SfxException(string publicMessage, string debugMessage) : base(debugMessage)
		{
			this.PublicMessage = publicMessage;
		}

		public SfxException(string debugMessage, Exception inner) : base(debugMessage, inner)
		{
		}
	}
}

