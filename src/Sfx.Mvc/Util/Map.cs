using System;
using System.Linq;
using System.Collections.Generic;

namespace Sfx.Collections
{
	/// <summary>
	/// Diccionario string-object. Por defecto no distingue mayúsculas/minúsculas en la clave
	/// y que devuelve null cuando se pide una clave que no existe en lugar de lanzar una
	/// excepción.
	/// </summary>
    public class Map : Map<object>
    {
        public Map()
        {
        }

        public Map(bool ignoreCase) : base(ignoreCase)
        {
        }
    }

	/// <summary>
	/// Diccionario string-string. Por defecto no distingue mayúsculas/minúsculas en la clave
	/// y que devuelve null cuando se pide una clave que no existe en lugar de lanzar una
	/// excepción.
	/// </summary>
    public sealed class StringMap : Map<string>
    {
        public StringMap()
        {
        }

        public StringMap(bool ignoreCase) : base(ignoreCase)
        {
        }
    }

	/// <summary>
	/// Diccionario string-valor. Por defecto no distingue mayúsculas/minúsculas en la clave
	/// y que devuelve null cuando se pide una clave que no existe en lugar de lanzar una
	/// excepción.
	/// </summary>
	public class Map<T> : Dictionary<string, T>
    {
        public Map() : this (true)
        {
        }

        public Map(bool ignoreCase) : base(ignoreCase ? 
		                                   StringComparer.OrdinalIgnoreCase : 
		                                   StringComparer.Ordinal)
        {
		}

		public new IEnumerable<string> Keys
		{
			get{ return base.Keys.Select(t => t); }
		}

		public KeyValuePair<string, T>[] DebugValue
		{
			get{ return this.ToArray(); }
		}

        public new T this[string key]
        {
            get
            {
                T value;

                if (this.TryGetValue(key, out value))
                {
                    return value;
                }
                else
                {
                    return default(T);
                }
            }
            set
            {
                if (this.ContainsKey(key))
                {
                    base[key] = value;
                }
                else
                {
                    this.Add(key, value);
                }
            }
        }
		
		public static Map<T> Copy(IDictionary<string, T> items)
		{
			var map = new Map<T>();
			map.AddRange(items);
			return map;
		}

		public void AddRange (IDictionary<string, T> items)
		{
			if (items != null)
			{
				foreach (var item in items)
				{
					this.Add (item.Key, item.Value);
				}
			}
        }
    }
}
