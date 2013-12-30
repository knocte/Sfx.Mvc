using System;
using System.Globalization;
using Sfx.Sql;
using Sfx.JSON;
using Sfx.Collections;
using Sfx.Mvc;

namespace Sfx.Mvc
{
	public sealed class Sessions
	{
		static readonly object syncLock = new object();
		readonly Random random = new Random();
		readonly bool persistent;
		readonly string connectionString;
		readonly string databaseType;

		const string sessionIdSeed = "abcdefghjkmnopqrstuvwzxyzABCDEFGHJKLMNOPQRSTUVWXYZ1234567890!·$%&/()?¿¨Ñ~^Ç{}[]ªº.-_+*-";
		public static readonly string CookieName = "sfxSession"; // arbitrario

		public int CookieExpirationDays { get; set; }

		public Sessions(Settings settings)
		{
			CookieExpirationDays = 60;
			persistent = settings["persistentSessions"] as bool? ?? false;
			connectionString = settings["ConnectionString"] as string;
			databaseType = settings["databaseType"] as string;
		}

		Orm CreateOrm()
		{
			return new Orm(DbSession.New(this.databaseType, this.connectionString));
		}

		public void CreateTables()
		{
			using(var orm = CreateOrm())
			{
				var ddl = new DdlQuery("tenant");
				ddl.IfNotExists = true;
				ddl.Columns.Add(new DdlColumn() { Name = "id", Type = DdlType.Int, PrimaryKey = true, Autoincrement = true });
				ddl.Columns.Add(new DdlColumn() { Name = "name", Type = DdlType.String, Length = 25 });
				ddl.Columns.Add(new DdlColumn() { Name = "displayName", Type = DdlType.String, Length = 50 });
				ddl.Columns.Add(new DdlColumn() { Name = "culture", Type = DdlType.String, Length = 5, Nullable = true });
				ddl.Columns.Add(new DdlColumn() { Name = "timeZone", Type = DdlType.String, Length = 20, Nullable = true });
				ddl.Columns.Add(new DdlColumn() { Name = "items", Type = DdlType.Text, Length = 50, Nullable = true });
				orm.ExecuteNonQuery(ddl);
						
				ddl = new DdlQuery("session");
				ddl.IfNotExists = true;
				ddl.Columns.Add(new DdlColumn() { Name = "id", Type = DdlType.Int, PrimaryKey = true, Autoincrement = true });
				ddl.Columns.Add(new DdlColumn() { Name = "sessionId", Type = DdlType.String, Length = 35 });
				ddl.Columns.Add(new DdlColumn() { Name = "name", Type = DdlType.String, Length = 25, Nullable = true });
				ddl.Columns.Add(new DdlColumn() { Name = "tenant", Type = DdlType.String, Length = 25, Nullable = true });
				ddl.Columns.Add(new DdlColumn() { Name = "idAdmin", Type = DdlType.Int });
				ddl.Columns.Add(new DdlColumn() { Name = "idSuperAdmin", Type = DdlType.Int });
				ddl.Columns.Add(new DdlColumn() { Name = "idCustomer", Type = DdlType.Int });
				ddl.Columns.Add(new DdlColumn() { Name = "culture", Type = DdlType.String, Length = 5, Nullable = true });
				ddl.Columns.Add(new DdlColumn() { Name = "createdate", Type = DdlType.DateTime });
				ddl.Columns.Add(new DdlColumn() { Name = "items", Type = DdlType.Text, Length = 50, Nullable = true });
				orm.ExecuteNonQuery(ddl);

//				ddl = new DdlQuery("session");
//				ddl.Operation = DdlOperation.CreateIndex;
//				ddl.Indexes.Add(new Index() { Columns = new string[]{ "sessionId" } });
//				db.ExecuteNonQuery(ddl);
			}
		}

		public User New()
		{
			var user = new User();	
			user.SessionId = GenerateSessionId();		
			user.Culture = CultureInfo.CurrentCulture;
			user.TimeZone = TimeZoneInfo.Local;
			user.CreateDate = DateTime.UtcNow;
			user.Items = new Map();
			return user;
		}
		
		public User Load(MvcContext context, bool createIfNotExists)
		{
			var user = Load(context);
			if(user == null && createIfNotExists)
			{
				user = New();
			}

			return user;
		}

		string GenerateSessionId()
		{            
			string sessionId;

			do
			{
				sessionId = GenerateRandomString(32);
			}
			// iterar hasta encontrar una id que no exista.
			while (Cache.Default[GetCacheKey(sessionId)] != null);

			return sessionId;
		}
		
		string GenerateRandomString(int length)
		{
			int seedLenght = sessionIdSeed.Length;
			char[] result = new char[length];

			for (int i = 0; i < length; i++)
			{
				lock(syncLock)
				{
					var r = random.Next(0, seedLenght);
					result[i] = sessionIdSeed[r];
				}
			}

			return new string(result);
		}

		public void Save(MvcContext context, bool persistent)
		{
			if (context == null)
			{
				throw new ArgumentNullException();
			}

			var user = context.User;

			Cache.Default[GetCacheKey(user.SessionId)] = user;

			SaveInDatabase(user);
			SaveInCookie(context, user, persistent);
		}

		User Load(MvcContext context)
		{
			var cookie = context.Request.Cookies[CookieName];			
			if(cookie == null)
			{
				return null;
			}

			var sessionId = UrlUtil.UrlDecode(cookie.Value);		
			if(string.IsNullOrWhiteSpace(sessionId))
			{
				return null;
			}

			var expirationLimit = DateTime.UtcNow.AddDays(-30);

			var cacheKey = GetCacheKey(sessionId);
			var user = Cache.Default[cacheKey] as User;
			if(user != null)
			{
				if(user.CreateDate < expirationLimit)
				{
					Cache.Default.Remove(cacheKey);
					return null;
				}
				return user;
			}

			if(this.persistent)
			{
				using(var orm = CreateOrm())
				{
					// buscar si no en la base de datos
					var query = Query.SelectAllFrom("session").WhereIsEqual("sessionId", sessionId).Limit(1);
					query.IsEqualOrGreater("createdate", expirationLimit);
					var r = orm.LoadFirst(query);
					return r != null ? ToSession(r) : null;
				}
			}

			return null;
		}

		string GetCacheKey(string sessionId)
		{
			if(string.IsNullOrEmpty(sessionId))
			{
				throw new ArgumentNullException();
			}
			return string.Concat("_session_", sessionId);
		}

		void SaveInCookie(MvcContext context, User user, bool isPersistent)
		{
			var cookie = new HttpCookie();
			cookie.Name = CookieName;
			cookie.Value = UrlUtil.UrlEncode(user.SessionId); // solo mandar caracteres válidos a la cookie
			cookie.HttpOnly = true;

			// si es persistente especifica la fecha, si no dura lo que la sesión del navegador.
			if (isPersistent)
			{
				cookie.Expiration = DateTime.UtcNow.AddDays(CookieExpirationDays);
			}

			context.ResponseCookies.Add(CookieName, cookie);
		}

		public void DeleteSession(User user)
		{
			if (string.IsNullOrEmpty(user.SessionId))
			{
				throw new ArgumentNullException();
			}
									
			Cache.Default.Remove(GetCacheKey(user.SessionId));

			if(this.persistent)
			{
				var q = Query.DeleteFrom("session").WhereIsEqual("sessionId", user.SessionId);
				using(var orm = CreateOrm())
				{
					orm.ExecuteNonQuery(q);
				}
			}
		}

		// TODO: implementar
		Tenant foo;
		public Tenant LoadTenant(string name)
		{
			if(foo == null)
			{ 
				foo = new Tenant() {
					Culture = new System.Globalization.CultureInfo("es-ES"),
					TimeZone = TimeZoneInfo.Local
				};
			}
			return foo;
		}

		Tenant ToTenant(Record r)
		{
			var tenant = new Tenant();
			tenant.Name = r.GetString("name");
			tenant.DisplayName = r.GetString("displayName");
			var cultureName = r.GetString("culture");
			tenant.Culture = cultureName != null ? new CultureInfo(cultureName) : CultureInfo.CurrentCulture;
			var timeZone = r.GetString("timeZone");
			tenant.TimeZone = timeZone != null ? TimeZoneInfo.FindSystemTimeZoneById(timeZone) : TimeZoneInfo.Local;
			var items = r.GetString("items");
			tenant.Items = !string.IsNullOrEmpty(items) ? Json.Deserialize<Map>(items) : new Map();
			return tenant;
		}

		void SaveInDatabase(User user)
		{
			var isNew = user.Id == 0;

			var query = new Query("session");
			query.AddValue("name", user.Name);
			query.AddValue("idAdmin", user.IdAdmin);
			query.AddValue("idSuperAdmin", user.IdSuperAdmin);
			query.AddValue("idCustomer", user.IdCustomer);

			if(user.Tenant != null)
			{
				query.AddValue("tenant", user.Tenant.Name);
			}

			query.AddValue("culture", user.Culture.Name);
			query.AddValue("createdate", user.CreateDate);
			query.AddValue("items", Json.Serialize(user.Items));

			if(isNew)
			{
				query.AddValue("sessionId", user.SessionId);
				query.DbOperation = DbOperation.Insert;
			}
			else
			{
				query.DbOperation = DbOperation.Update;
				query.WhereIsEqual("sessionId", user.SessionId);
			}

			if(this.persistent)
			{
				using(var orm = CreateOrm())
				{
					orm.ExecuteNonQuery(query);
					if(isNew)
					{
						user.Id = Convert.ToInt32(orm.DbSession.LastInsertedId);
					}
				}
			}
		}

		User ToSession(Record r)
		{
			var user = new User();
			user.Id = r.GetInt("id");
			user.SessionId = r.GetString("sessionId");
			user.Name = r.GetString("name");
			user.IdAdmin = r.GetInt("idAdmin");
			user.IdSuperAdmin = r.GetInt("idSuperAdmin");
			user.IdCustomer = r.GetInt("idCustomer");
			
			var tenant = LoadTenant(r.GetString("tenant"));
			user.Tenant = tenant;
			user.Culture = GetCulture(r.GetString("culture"), tenant);
			user.TimeZone = GetTimeZone(tenant);
			user.CreateDate = r.GetDateTime("createdate");

			var items = r.GetString("items");
			user.Items = !string.IsNullOrEmpty(items) ? Json.Deserialize<Map>(items) : new Map();

			return user;
		}

		CultureInfo GetCulture(string cultureName, Tenant tenant)
		{
			if(cultureName != null)
			{
				return new CultureInfo(cultureName);
			}
			return tenant.Culture ?? CultureInfo.CurrentCulture;
		}

		TimeZoneInfo GetTimeZone( Tenant tenant)
		{
			return tenant.TimeZone ?? TimeZoneInfo.Local;
		}
	}
}








































