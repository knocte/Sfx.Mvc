using System;
using Sfx.Mvc;
using Sfx.Sql;

namespace Sfx.Mvc
{
	public static class DbUtil
	{
		public static Orm NewOrm(Settings settings)
		{
			if(settings.DatabaseType == null)
			{
				throw new NullReferenceException("DatabaseType is not set. Check settings.conf");
			}

			if(settings.ConnectionString == null)
			{
				throw new NullReferenceException("ConnectionString is not set. Check settings.conf");
			}

			return new Orm(DbSession.New(settings.DatabaseType, settings.ConnectionString));
		}

		public static Orm NewOrm()
		{
			return NewOrm(Settings.Default);
		}
	}
}

