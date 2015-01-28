using System;
using System.IO;
using System.Reflection;

namespace LyncChatLogger
{
	public static class DebugLog
	{
		private static readonly string DEBUG_LOG_PATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/debug.log";

		public static void Write(string text, params object[] args)
		{
			using (var writer = new StreamWriter(DEBUG_LOG_PATH, true))
			{
				writer.Write("[{0} {1}] ", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString());
				writer.WriteLine(text, args);
			}
		}
	}
}
