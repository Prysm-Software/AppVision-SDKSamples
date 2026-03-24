using DriverAdvanced;
using Prysm.AppVision.Common;
using Prysm.Logger;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Prysm.AppVision.SDK
{
	/// <summary>
	/// WPF application entry point for the DriverAdvanced driver.
	/// Manages the application lifecycle: startup, shutdown, and unhandled exception handling.
	/// Hosts the <see cref="Driver"/> instance and the <see cref="MainWindow"/> diagnostic UI.
	/// </summary>
	public partial class App : Application
	{
		Driver _driver;
		Window _window;

		/// <summary>
		/// Registers an assembly resolution fallback that looks in the parent directory.
		/// This allows the driver executable to be placed in a sub-folder of the AppVision
		/// Bin directory (e.g. Bin\DriverAdvanced\DriverAdvanced.exe) while still resolving
		/// the shared SDK assemblies (appSDK.dll, appCommon.dll, appData.dll) from Bin\.
		/// </summary>
		static App() => AppDomain.CurrentDomain.AssemblyResolve += LookForAssemblyInParentDir;

		/// <summary>
		/// Attempts to load an assembly from the parent directory of the driver executable.
		/// Called automatically by the CLR when an assembly cannot be found in the normal probing paths.
		/// </summary>
		static System.Reflection.Assembly LookForAssemblyInParentDir(object sender, ResolveEventArgs a)
		{
			Console.WriteLine(a.Name);
			var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			var ass = $@"{dir}\..\..\{a.Name.Split(',')[0]}.dll";
			return File.Exists(ass) ? Assembly.LoadFrom(ass) : null;
		}

		/// <summary>
		/// Application startup. Validates the command-line arguments, wires up global error handlers,
		/// creates the diagnostic window, and starts the driver.
		/// </summary>
		protected override void OnStartup(StartupEventArgs e)
		{
			// Use invariant culture so that number and date formatting is consistent
			// regardless of the locale of the machine where the driver is deployed.
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

			// Catch any unhandled exception from background threads or the WPF dispatcher.
			// Both handlers delegate to UnhandledException() which logs, waits 5 s, then closes.
			AppDomain.CurrentDomain.UnhandledException += (s, ex) => UnhandledException((Exception)ex.ExceptionObject);
			DispatcherUnhandledException += (s, ex) => UnhandledException(ex.Exception);

			// Validate command-line: the protocol name (and optional host) must be provided.
			// AppVision passes this when it launches the driver; without it the driver cannot login.
			var args = Environment.GetCommandLineArgs();
			if (args.Length < 2)
			{
				MessageBox.Show($"Usage: {Assembly.GetEntryAssembly().GetName().Name} protocolName[@hostName]", Helper.ProductName, MessageBoxButton.OK, MessageBoxImage.Information);
				Shutdown(1);
				return;
			}

			// Create and show the diagnostic window before starting the driver so that
			// log messages emitted during startup are visible to the user.
			_window = new MainWindow();
			_window.Closing += Window_Closing;

			// Bind the WPF "Refresh" command (F5 / toolbar button) to driver Refresh.
			_window.CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, (s, a) => _driver.Refresh("")));

			// Bind the WPF "CorrectionList" command to driver Download (AppControl scenarios).
			_window.CommandBindings.Add(new CommandBinding(ApplicationCommands.CorrectionList, (s, a) => _driver.Download("", "")));
			_window.Show();

			// Instantiate and start the driver. Driver.Start() connects to AppVision and
			// launches all device instances.
			_driver = new Driver();

			// When AppVision closes the connection (e.g. server shutdown, protocol removed),
			// ControllerManager raises the Closed event. We use it to close the driver window.
			_driver.AppServer.ControllerManager.Closed += Driver_Closing;
			_driver.Start();
		}

		/// <summary>
		/// Called when AppVision closes the connection to this driver (server shutdown,
		/// protocol disabled, etc.). Stops the driver and closes the window.
		/// </summary>
		void Driver_Closing()
		{
			_driver.Stop();
			_window.Closing -= Window_Closing;
			_window.Close();
		}

		/// <summary>
		/// Intercepts the user's attempt to close the window.
		/// Closing without holding Ctrl is blocked intentionally: in production the driver
		/// should only be stopped by AppVision, not by an operator accidentally closing the window.
		/// Closing with Ctrl held confirms the intent and stops the driver cleanly.
		/// </summary>
		void Window_Closing(object sender, CancelEventArgs e)
		{
			if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
			{
				// RT.Get("No_user_stop") returns a localised warning message from AppVision resources.
				MessageBox.Show(RT.Get("No_user_stop"), Helper.ProductName, MessageBoxButton.OK, MessageBoxImage.Warning);
				e.Cancel = true;
			}
			else
				_driver?.Stop();
		}

		/// <summary>
		/// Handles any unhandled exception (from the dispatcher or background threads).
		/// Logs the error, schedules a delayed shutdown (5 s), and shows a message box.
		/// The 5-second delay gives AppVision time to detect the disconnection and restart
		/// the driver automatically according to the protocol restart policy.
		/// </summary>
		void UnhandledException(Exception ex)
		{
			Trace.Error(ex);
			Shutdown();
			MessageBox.Show(ex?.Message ?? "Fatal error", Helper.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
		}

		/// <summary>
		/// Waits 5 seconds, then stops the driver and closes the window.
		/// The delay is intentional: it prevents a rapid restart loop when a persistent
		/// error condition causes the driver to fail immediately on each launch.
		/// </summary>
		new async void Shutdown()
		{
			Trace.Error("Closing driver in 5 seconds...");
			await Task.Delay(5000);
			Driver_Closing();
		}
	}
}
