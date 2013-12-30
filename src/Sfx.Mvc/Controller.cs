using System;

namespace Sfx.Mvc
{
	public class Controller : Controller<MvcContext>
	{
		public User User 
		{
			get{ return this.Context.User; }
		}

		public string T(string value, params object[] args)
		{
			return this.User.T(value, args);
		}

		public void SaveSession()
		{
			this.Context.SaveSession();
		}
	}
}

