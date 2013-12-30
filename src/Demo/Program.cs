using System;
using Sfx.Mvc;

namespace Sfx.Mvc.Tests
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			//new Sessions(Settings.Default).CreateTables();
			new AppServer(Settings.Default).Start();
		}
	}
}
