using System.IO;

namespace Sfx.Mvc
{
	public interface IHttpPostedFile
	{
		string FileName { get; }
		string ContentType { get; }
		int ContentLength { get; }
		Stream InputStream { get; }
		byte[] GetBytes();
		string GetString();
		void SaveAs(string filename);
	}
}

