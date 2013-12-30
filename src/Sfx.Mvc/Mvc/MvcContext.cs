using System;
using System.IO;
using Sfx.Collections;

namespace Sfx.Mvc
{
	public class MvcContext
	{
		Map<HttpCookie> responseCookies;
		StringMap responseHeaders;
		User user;
		Sessions sessions;

		public User User
		{
			get
			{
				if(this.user == null)
				{
					this.user = this.sessions.Load(this, true);
				}
				return this.user;
			}
			set { this.user = value; }
		}

		public Route Route { get; set; }
		public HttpRequest Request { get; set; }
		public Settings Settings  { get; set; }

		public string RootDirectory 
		{
			get { return this.Request.RootDirectory; } 
		}

		public StringMap ResponseHeaders
		{
			get
			{
				if(this.responseHeaders == null)
				{
					this.responseHeaders = new StringMap();
				}
				return this.responseHeaders; 
			}
			set{ this.responseHeaders = value; }
		}

		public Map<HttpCookie> ResponseCookies
		{
			get
			{
				if(this.responseCookies == null)
				{
					this.responseCookies = new Map<HttpCookie>();
				}
				return this.responseCookies; 
			}
			set{ this.responseCookies = value; }
		}

		
		public MvcContext(HttpRequest request, Settings settings, Sessions sessions)
		{
			this.sessions = sessions;
			var route = Route.Parse(request.Url, settings);
			this.Route = route;
			this.Request = request;	
			this.Settings = this.Settings;
		}

		public void DeleteCookie(string name)
		{
			var cookie = this.Request.Cookies[name];			
			if(cookie != null)
			{
				cookie.Expiration = DateTime.Now.AddDays(-1);
				this.ResponseCookies[name] = cookie;
			}
		}

		public string MapPath(string path)
		{
			if(path == null)
			{
				throw new ArgumentNullException();
			}

			if(path[0] == '/')
			{
				path = path.Substring(1);
			}

			return Path.Combine(this.RootDirectory, path);
		}

		public void SaveSession()
		{
			this.sessions.Save(this, true);
		}

		public void Logout()
		{
			this.sessions.DeleteSession(this.User);
			this.DeleteCookie(Sessions.CookieName);
		}
	}
}



































