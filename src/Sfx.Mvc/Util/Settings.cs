using System;
using System.IO;
using Sfx.JSON;
using Sfx.Collections;

namespace Sfx.Mvc
{
	/// <summary>
	/// Helper class that allows to conveniently access the default configuration.
	/// </summary>
	public class Settings
	{
		string rootDirectory;
		string defaultController;
		static Settings defaultSettings;

		public static Settings Default
		{
			get
			{
				if(defaultSettings == null)
				{
					defaultSettings = new Settings();
				}
				return defaultSettings;
			}
		}

		public object this[string key]
		{
			get { return this.Values[key]; }
		}

		public Map Values { get; set; }

		public string RootDirectory
		{
			get
			{
				if(rootDirectory == null)
				{
					rootDirectory = GetRootDirectory();
				}
				return rootDirectory; 
			}
		}

		public string DefaultController
		{
			get
			{
				if(defaultController == null)
				{
					defaultController = Values["defaultController"] as string ?? "main";
				}
				return defaultController; 
			}
		}

		public string CacheBreaker 
		{
			get{ return Values["cacheBreaker"] as string; }
		}

		public string DatabaseType 
		{
			get{ return Values["databaseType"] as string; }
		}

		public string ConnectionString
		{
			get{ return Values["ConnectionString"] as string; }
		}

		public int HttpPort
		{
			get{ return Values["HttpPort"] as int? ?? -1; }
		}

		public bool ShowErrors
		{
			get{ return Values["showErrors"] as bool? ?? false; }
		}

		public Settings()
		{
			Reload();
		}

		public void Reload()
		{
			Values = LoadSettings();
		}

		Map LoadSettings()
		{
			var settingsFile = Path.Combine(GetExecutingDirectory(), "settings.conf");
			if(File.Exists(settingsFile))
			{
				return Json.Deserialize<Map>(File.ReadAllText(settingsFile));
			}
			return new Map();
		}

		string GetExecutingDirectory()
		{
			//Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			return Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
		}

		string GetRootDirectory()
		{
			var settingsRootDir = Values["rootDirectory"] as string;

			if(settingsRootDir != null && settingsRootDir[0] == '/')
			{
				// la ruta es absoluta.
				return settingsRootDir;
			}

			var binDirectory = GetExecutingDirectory();
			if(settingsRootDir != null)
			{
				return string.Concat(binDirectory.TrimEnd('/'), "/", settingsRootDir);
			}

			return binDirectory;
		}
	}
}

