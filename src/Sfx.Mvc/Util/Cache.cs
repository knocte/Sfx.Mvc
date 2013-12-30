using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Sfx.Mvc
{
	public sealed class Cache
	{
		readonly MemoryCache internalCache;
		static Cache defaultCache;

		public static Cache Default
		{
			get
			{
				if(defaultCache == null)
				{
					defaultCache = new Cache();
				}
				return defaultCache;
			}
		}

		public Cache()
		{
			internalCache = new MemoryCache("_sfxCache", null);
		}

		public bool ContainsKey(string key)
		{
			return this.internalCache.Contains(key, null);
		}

		public void Add(string key, object value, int minutes)
		{
			var policy = new CacheItemPolicy();
			policy.SlidingExpiration = new TimeSpan(0, minutes, 0);
			internalCache.Add(new CacheItem(key, value), policy);
		}   

		public void Add(string key, object value, DateTime expiration)
		{
			var policy = new CacheItemPolicy();
			policy.AbsoluteExpiration = expiration;
			internalCache.Add(new CacheItem(key, value), policy);
		}      

		public int Count
		{
			get { return this.internalCache.Count(); }
		}

		public IEnumerable<string> Keys
		{
			get{ return this.internalCache.Select(t => t.Key); }
		}

		public object this[string key]
		{
			get { return internalCache[key]; }
			set { internalCache[key] = value; }        
		}

		public void Remove(string key)
		{
			internalCache.Remove(key);
		}

		public void Clear()
		{
			foreach (var item in this.internalCache)
			{
				internalCache.Remove((string)item.Key);
			}
		}
	}
}
