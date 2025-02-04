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
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			var args = Environment.GetCommandLineArgs();
			if (args.Length < 2)
				throw new Exception($"Usage: {Assembly.GetEntryAssembly().GetName().Name} protocolName[@hostName]");

			Title = Helper.ProductName + " v" + Helper.ProductVersion + " - " + Environment.GetCommandLineArgs().Skip(1).FirstOrDefault();

			Trace.Info("*********** START ****************");
			Trace.Info($"{Helper.ProductName} v{Helper.ProductVersion}");
			Trace.Info("**************************************");

			// load appvision labels
			try { RT.Load(Path.Combine(Helper.PathResources, "AppLab.txt")); }
			catch { }

			DataContext = this;
			InitializeComponent();

			LogBox.MaxCount = Settings.Default.MaxCount;
			LogBox.LogLevel = (LogLevel)Settings.Default.LogLevel;

			Trace.Logger.AddAppender(LogBox, LogLevel.All, "wpf");
		}

		void Window_Closed(object sender, EventArgs e)
		{
			Settings.Default.MaxCount = LogBox.MaxCount;
			Settings.Default.LogLevel = (int)LogBox.LogLevel;
			Settings.Default.Save();
		}
	}
}
