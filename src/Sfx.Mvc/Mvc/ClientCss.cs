using System.IO;
using System.Collections.Generic;
using Sfx.Templates;

namespace Sfx.Mvc
{
	public sealed class ClientCss : IRenderizable
	{
		private List<string> files;

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
					context.Writer.WriteLine("<link rel='stylesheet' href='{0}' />", file);
				}
			}
		}
	}
}

