using Prysm.AppVision.Common;
using Prysm.AppVision.SDK;
using Prysm.Logger;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DriverAdvanced
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
		/// Registers an assembly resolution fallback that looks in the parent directories.
		/// This allows the driver executable to be placed in a sub-folder of the AppVision
		/// Bin directory (Bin\Drivers\DriverAdvanced\DriverAdvanced.exe, or Bin\DriverAdvanced\...)
		/// while still resolving the shared AppVision assemblies (appSDK.dll, appCommon.dll,
		/// appData.dll, App.Logger.dll, App.Utility.dll...) from Bin\, without copying them.
		/// </summary>
		/// <remarks>
		/// The handler must be registered before any AppVision type is used: hence the static constructor
		/// of the App class, which runs before the WPF startup code.
		/// </remarks>
		static App() => AppDomain.CurrentDomain.AssemblyResolve += LookForAssemblyInParentDir;

		/// <summary>
		/// Attempts to load an assembly from the parent directory, then the grand-parent directory,
		/// of the driver executable.
		/// Called automatically by the CLR when an assembly cannot be found in the normal probing paths.
		/// </summary>
		static Assembly LookForAssemblyInParentDir(object sender, ResolveEventArgs a)
		{
			var dir = AppDomain.CurrentDomain.BaseDirectory;
			var name = a.Name.Split(',')[0] + ".dll";
			foreach (var relativeDir in new[] { "..", @"..\.." })
			{
				var path = Path.GetFullPath(Path.Combine(dir, relativeDir, name));
				if (File.Exists(path))
					return Assembly.LoadFrom(path);
			}
			return null;
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

			// When AppVision stops the protocol (configurator, server shutdown, protocol restart),
			// ControllerManager raises the Closed event. We use it to close the driver window.
			// SDK events are raised on background threads: Dispatcher.BeginInvoke goes back to the UI thread.
			_driver.AppServer.ControllerManager.Closed += () => Dispatcher.BeginInvoke((Action)Driver_Closing);

			// ControllerManager.Restart is raised when the SDK loses the connection to the server
			// (keep-alive failure); the SDK then closes the session. Since AppVision 4.8.5 the server
			// restarts a protocol itself (close, then new process) and relaunches the "Autostart"
			// protocols whose driver stopped answering: the driver just exits, it must not relaunch itself.
			_driver.AppServer.ControllerManager.Restart += () => Dispatcher.BeginInvoke((Action)(() =>
			{
				Trace.Error("Connection to the AppVision server lost");
				Driver_Closing();
			}));
			_driver.Start();
		}

		/// <summary>
		/// Called (on the UI thread) when AppVision stops the protocol or when the connection is lost.
		/// Stops the driver and closes the window, which ends the process.
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
			Dispatcher.Invoke(Driver_Closing);     // the exception may come from a background thread
		}
	}
}
