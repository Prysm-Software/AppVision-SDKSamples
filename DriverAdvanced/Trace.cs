using Prysm.AppVision.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Prysm.AppVision.SDK
{
	public enum LogLevel
	{
		Error = 0, Info = 1, Debug = 2
	}

	class Trace
	{
		public static event Action<LogEntry> LogEvent;

		public static void Debug(string message) => Log(LogLevel.Debug, message);
		public static void Info(string message) => Log(LogLevel.Info, message);
		public static void Error(string message) => Log(LogLevel.Error, message);
		public static void Debug(Exception ex, string message = null) => Debug(BuildMessageFromEx(ex, message));
		public static void Error(Exception ex, string message = null) => Error(BuildMessageFromEx(ex, message));

		private static string BuildMessageFromEx(Exception ex, string message)
		{
			if (!string.IsNullOrWhiteSpace(message))
				message += ": ";

			return message + ex.Message + "\r\n" + ex;
		}

		public static void Log(LogLevel level, string message)
		{
			var it = new LogEntry(level, message);
			// outputdebugstring
			System.Diagnostics.Trace.WriteLine(it);
			// gui + log file
			LogEvent?.Invoke(it);
		}

		// status bar
		public static string StatusText { get; set; }
	}


	public class LogEntry
	{
		public DateTime Date { get; set; }
		public LogLevel Level { get; set; }
		public string Message { get; set; }

		public LogEntry(LogLevel level, string message)
		{
			Date = DateTime.Now;
			Level = level;
			Message = message;
		}
		public override string ToString()
		{
			return $"{Date:yyyy-MM-dd HH:mm:ss.f}	{Level}	{Message}";
		}
	}
}
