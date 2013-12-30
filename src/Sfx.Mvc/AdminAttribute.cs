using System;

namespace Sfx.Mvc
{
	public sealed class AdminAttribute : FilterAttribute
	{
		public const string DefaultLogin = "/account/login";
		
		public override HttpResponse Execute(MvcContext context)
		{
			if(!((MvcContext)context).User.IsAdmin)
			{
				var url = context.Settings["adminLogin"] as string ?? DefaultLogin;
				url = UrlUtil.SetQueryValue(url, "callback", context.Request.Url.AbsolutePath);
				return new RedirectResponse(url);
			}
			return null;
		}
	}

	public sealed class SuperAdminAttribute : FilterAttribute
	{
		public const string DefaultLogin = "/account/superadminlogin";
		
		public override HttpResponse Execute(MvcContext context)
		{
			if(!((MvcContext)context).User.IsAdmin)
			{
				var url = context.Settings["superAdminKogin"] as string ?? DefaultLogin;
				url = UrlUtil.SetQueryValue(url, "callback", context.Request.Url.AbsolutePath);
				return new RedirectResponse(url);
			}
			return null;
		}
	}
}

