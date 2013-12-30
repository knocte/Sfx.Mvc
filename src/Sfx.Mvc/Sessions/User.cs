using System;
using System.Globalization;
using Sfx.Collections;

namespace Sfx.Mvc
{
	public sealed class User
	{
		public int Id { get; set; } // el registro en la base de datos
		public string SessionId { get; set; }
		public int IdAdmin { get; set; }
		public int IdSuperAdmin { get; set; }
		public int IdCustomer { get; set; }
		public string Name { get; set; }
		public Tenant Tenant { get; set; }
		public CultureInfo Culture { get; set; }
		public TimeZoneInfo TimeZone { get; set; }
		public Map Items { get; set; }
		public DateTime CreateDate { get; set; }

		public bool IsAdmin { get { return this.IdAdmin > 0; } }
		public bool IsSuperAdmin { get { return this.IdSuperAdmin > 0; } }
		public bool IsCustomer { get { return this.IdCustomer > 0; } }

		/// <summary>
		/// Convierte una fecha UTC a la hora local de la sesión
		/// </summary>
		public DateTime ToLocal(DateTime utc)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(utc, this.TimeZone);
		}

		public DateTime ToUtc(DateTime local)
		{
			return TimeZoneInfo.ConvertTimeToUtc(local, this.TimeZone);
		}

		/// <summary>
		/// Traduce el idioma a la cultura de la sesión.
		/// </summary>
		public string T(string value, params object[] args)
		{
			if(value == null)
			{
				return null;
			}

			// TODO: traducir value antes de llamar a format.

			return args.Length > 0 ? string.Format(value, args) : value;
		}
	}
}

