using System;
using System.Globalization;
using Sfx.Collections;

namespace Sfx.Mvc
{
	public sealed class Tenant
	{
		public string Name { get; set; }
		public string DisplayName { get; set; }
		public CultureInfo Culture { get; set; }
		public TimeZoneInfo TimeZone { get; set; }
		public Map Items { get; set; }
	}
}

