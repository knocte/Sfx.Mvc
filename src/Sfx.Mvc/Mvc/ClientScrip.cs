using System.IO;
using System.Collections.Generic;
using Sfx.Templates;
using Sfx.JSON;

namespace Sfx.Mvc
{
	public sealed class ClientScript : IRenderizable
	{
		private List<string> files;		
		private List<string> scripts;
		private Dictionary<string, object> variables;

		public Dictionary<string, object> Variables
		{
			get
			{
				if (this.variables == null) 
				{
					this.variables = new Dictionary<string, object> ();
				}
				return this.variables;
			}
		}
		
		public List<string> Scripts
		{
			get
			{
				if(this.scripts == null)
				{
					this.scripts = new List<string>();
				}
				return this.scripts;
			}
		}

		public List<string> Files
		{
			get
			{
				if(this.files == null)
				{
					this.files = new List<string>();
				}
				return this.files;
			}
		}

		public void Render(RenderContext context)
		{	
			if (this.files != null)
			{
				foreach (string file in this.files)
				{
					context.Writer.WriteLine("<script type='text/javascript' src='{0}'></script>", file);
				}
			}

			if (this.variables != null) 
			{				
				context.Writer.WriteLine("<script type='text/javascript'>var scriptVars=");
				context.Writer.WriteLine(Json.Serialize(variables));				
				context.Writer.WriteLine("</script>");
			}

			if(scripts != null)
			{
				context.Writer.WriteLine("<script type='text/javascript'>");
				foreach(string script in scripts)
				{
					
					context.Writer.WriteLine(script);
					
				}
				context.Writer.WriteLine("</script>");
			}
		}
	}
}



























