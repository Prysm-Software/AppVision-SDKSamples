using DriverAdvanced.Properties;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Prysm.AppVision.Common;
using Prysm.Logger;

namespace Prysm.AppVision.SDK
{
	/// <summary>
	/// Diagnostic window for the DriverAdvanced driver.
	/// Displays a live, scrollable log with configurable log level and maximum line count.
	/// The window title shows the AppVision product version and the protocol name so that
	/// multiple driver windows can be distinguished on the taskbar.
	/// </summary>
	/// <remarks>
	/// Log output is written by the Trace class (Prysm.Logger). Two appenders are attached:
	///   - LogBox  : writes to this WPF window (filtered to the configured LogLevel).
	///   - AppServerLogAppender : forwards entries to the AppVision server log (see Driver.cs).
	/// Log level and max line count are persisted in user settings (Settings.Default).
	/// </remarks>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Initialises the window, validates that the protocol name argument was supplied,
		/// sets up the log appender, and restores persisted UI settings.
		/// </summary>
		public MainWindow()
		{
			// The driver cannot function without a protocol name argument.
			// This guard should never trigger in production (App.xaml.cs checks first),
			// but it is kept here as a safety net when running directly from the debugger.
			var args = Environment.GetCommandLineArgs();
			if (args.Length < 2)
				throw new Exception($"Usage: {Assembly.GetEntryAssembly().GetName().Name} protocolName[@hostName]");

			// Build the window title: "AppVision 4.x - MyProtocolName"
			Title = Helper.ProductName + " v" + Helper.ProductVersion + " - " + Environment.GetCommandLineArgs().Skip(1).FirstOrDefault();

			Trace.Info("*********** START ****************");
			Trace.Info($"{Helper.ProductName} v{Helper.ProductVersion}");
			Trace.Info("**************************************");

			// Load AppVision localised labels (used by RT.Get() throughout the application).
			// The file AppLab.txt is located in the AppVision resources folder.
			try { RT.Load(Path.Combine(Helper.PathResources, "AppLab.txt")); }
			catch { }

			DataContext = this;
			InitializeComponent();

			// Restore persisted log settings from the last session.
			LogBox.MaxCount = Settings.Default.MaxCount;
			LogBox.LogLevel = (LogLevel)Settings.Default.LogLevel;

			// Register the LogBox control as a log appender so that all Trace.Info/Error/...
			// calls are displayed in real time in the window.
			Trace.Logger.AddAppender(LogBox, LogLevel.All, "wpf");
		}

		/// <summary>
		/// Persists the current log settings when the window is closed.
		/// </summary>
		void Window_Closed(object sender, EventArgs e)
		{
			Settings.Default.MaxCount = LogBox.MaxCount;
			Settings.Default.LogLevel = (int)LogBox.LogLevel;
			Settings.Default.Save();
		}
	}
}
