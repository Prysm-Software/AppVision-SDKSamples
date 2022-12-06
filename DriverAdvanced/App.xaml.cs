using DriverAdvanced;
using Prysm.AppVision.Common;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Prysm.AppVision.SDK
{
	public partial class App : Application
	{
		Driver _driver;
		Window _window;

		protected override void OnStartup(StartupEventArgs e)
		{
			// Set invariant culture
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

			AppDomain.CurrentDomain.UnhandledException += (s, ex) => UnhandledException((Exception)ex.ExceptionObject);
			DispatcherUnhandledException += (s, ex) => UnhandledException(ex.Exception);

			var args = Environment.GetCommandLineArgs();
			if (args.Length < 2)
			{
				MessageBox.Show($"Usage: {Assembly.GetEntryAssembly().GetName().Name} protocolName[@hostName]", Helper.ProductName, MessageBoxButton.OK, MessageBoxImage.Information);
				Shutdown(1);
				return;
			}

			_window = new MainWindow();
			_window.Closing += Window_Closing;
			_window.CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, (s, a) => _driver.Refresh("")));
			_window.CommandBindings.Add(new CommandBinding(ApplicationCommands.CorrectionList, (s, a) => _driver.Download("", "")));
			_window.Show();

			_driver = new Driver();
			_driver.AppServer.ControllerManager.Closed += Driver_Closing;
			_driver.Start();
		}

		void Driver_Closing()
		{
			_driver.Stop();
			_window.Closing -= Window_Closing;
			_window.Close();
		}

		void Window_Closing(object sender, CancelEventArgs e)
		{
			if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
			{
				MessageBox.Show(RT.Get("No_user_stop"), Helper.ProductName, MessageBoxButton.OK, MessageBoxImage.Warning);
				e.Cancel = true;
			}
			else
				_driver?.Stop();
		}

		void UnhandledException(Exception ex)
		{
			Trace.Error(ex);
			Shutdown();
			MessageBox.Show(ex?.Message ?? "Fatal error", Helper.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
		}
		new async void Shutdown()
		{
			Trace.Error("Closing driver in 5 seconds...");
			await Task.Delay(5000);
			Driver_Closing();
		}
	}
}
