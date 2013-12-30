using System;

namespace Sfx.Mvc
{
	public sealed class HttpCookie
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public DateTime? Expiration { get; set; }
		public string Domain { get; set; }
		public bool HttpOnly { get; set; }
		public bool Secure { get; set; }
	}
}

