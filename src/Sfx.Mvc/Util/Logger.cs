using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sfx.Mvc
{
	public sealed class Logger
	{
		static Logger defaultLogger;
		static readonly Dictionary<string, object> pathLockers = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		readonly object thisLock = new object();
		readonly StringBuilder buffer;
		readonly int bufferSize;

		public static Logger Default
		{
			get
			{
				if(defaultLogger == null)
				{
					var file = Settings.Default.Values["eventsLog"] as string;
					if(file == null)
					{ 
						file = Path.Combine(Settings.Default.RootDirectory, "default.log");
					}
					defaultLogger = new Logger(file);
				}
				return defaultLogger;
			}
			set { defaultLogger = value; }
		}

		public int RotateFileSize { get; set; }
		public string FileName { get; set; }

		public Logger(string file) : this(file, 0)
		{
		}

		/// bufferSize es el numero de caracteres que soporta el buffer antes de escribir en disco. 
		/// En caso de especificar un buffer, las últimas llamadas pueden no llegar a escribirse nunca, por lo que conviene 
		/// llamar a Flush() al terminar de escribir mensajes.
		public Logger(string file, int bufferSize)
		{
			if(string.IsNullOrWhiteSpace(file))
			{
				throw new ArgumentNullException("file");
			}

			this.FileName = file;
			this.bufferSize = bufferSize;
			this.RotateFileSize = 30000000;
			this.buffer = new StringBuilder();
		}

		public void Print(Exception ex)
		{
			Print(ex, null);
		}

		public void Print(Exception ex, string message, params object[] args)
		{
			if(message != null)
			{
				Print(string.Concat(string.Format(message, args), ". ", ex.ToString(), Environment.NewLine));
			}
			else
			{
				Print(ex.ToString());
			}
		}

		public void Print(string message, params object[] args)
		{
			try
			{
				if (this.bufferSize > 0)
				{
					lock (thisLock)
					{
						this.buffer.Append(FormatMessage(message, args));

						if (this.buffer.Length >= this.bufferSize)
						{
							WriteToFile(this.buffer.ToString());
							this.buffer.Clear();
						}
					}
				}
				else
				{
					lock (thisLock)
					{
						WriteToFile(FormatMessage(message, args));
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		public void Flush()
		{
			lock (thisLock)
			{
				WriteToFile(this.buffer.ToString());
				this.buffer.Clear();
			}
		}

		void WriteToFile(string buffer)
		{
			try
			{
				lock (this.GetLockObject())
				{
					var directory = Path.GetDirectoryName(this.FileName);

					if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}

					var fileInfo = new FileInfo(this.FileName);

					// si el archivo es demasiado grande, archivarlo
					if (fileInfo.Exists && fileInfo.Length > this.RotateFileSize)
					{
						fileInfo.MoveTo(GetArchivedFileName(fileInfo));
					}

					File.AppendAllText(this.FileName, buffer);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error en LogWriter: " + ex);
			}
		}

		private object GetLockObject()
		{
			object pathLocker;
			lock (pathLockers)
			{
				if (!pathLockers.TryGetValue(this.FileName, out pathLocker))
				{
					pathLocker = new object();
					pathLockers.Add(this.FileName, pathLocker);
				}
			}
			return pathLocker;
		}

		static string FormatMessage(string message, params object[] args)
		{
			var sb = new StringBuilder();
			sb.AppendLine();
			sb.Append("[");
			sb.Append(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
			sb.Append("] ");

			if (args.Length > 0)
			{
				sb.AppendFormat(message, args);
			}
			else
			{
				sb.Append(message);
			}

			return sb.ToString();
		}

		static string GetArchivedFileName(FileInfo fileInfo)
		{
			return Path.Combine(fileInfo.DirectoryName,
				Path.GetFileNameWithoutExtension(fileInfo.Name) + "." +
				DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss-ffff") +
				Path.GetExtension(fileInfo.Name));
		}
	}
}




