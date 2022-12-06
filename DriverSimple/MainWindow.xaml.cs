using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DriverSimple
{
	public partial class MainWindow : Window
	{
		AppServerForDriver _appServer;
		VariableRow _protocolVariable;

		public MainWindow()
		{
			InitializeComponent();
			Title = Helper.ProductDescription + " v" + Helper.ProductVersion;

			_appServer = new AppServerForDriver();
			_appServer.VariableManager.StateChanged += VariableManager_StateChanged;
			_appServer.ControllerManager.Closed += CloseRequested;
			_appServer.ControllerManager.Restart += RestartRequested;
		}

		#region Load
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// CommandLine: DriverSample.exe protocolName[@hostname]

			string[] args = Environment.GetCommandLineArgs();

			if (args == null || args.Length < 2)
			{
				MessageBox.Show("Usage: DriverSample.exe protocol_name");
				return;
			}
			string[] ss = args[1].Split('@');
			var protocolName = ss[0];
			var serverHostname = (ss.Length > 1) ? ss[1] : "";

			try
			{
				Trace("Connecting to AppVision server...");

				// Log into AppVision server as a driver using the protocol name.
				// Note: an empty hostname will connect to localhost 
				_appServer.Open(serverHostname);
				_appServer.Login(protocolName);

				// {protocolName} does not exist in AppVision configuration. See Configurator/Protocols
				if (_appServer.CurrentProtocol == null)
					throw new Exception("Invalid protocol " + protocolName);

				// One node variable must have its parameter 'Source' set to '$P.{protocolName}'
				_protocolVariable = _appServer.GetVariablesByProtocol(protocolName).FirstOrDefault();
				if (_protocolVariable == null)
					throw new Exception("No variable linked to protocol " + protocolName);

				// Subscribe to received the change of value for Output variables
				_appServer.AddFilterNotifications($"$V.{_protocolVariable.Name}.Output*");
				_appServer.StartNotifications(false);   // true to receive the output initial values
				_appServer.ProtocolSynchronized();
				Trace("Connected");


				// Set the initial value of Input variables.
				// At driver startup, '@init' flag is used to set initial values without storing the change in the database as if it was an actual devce event.
				for (int i = 1; i <= 8; i++)
				{
					_appServer.VariableManager.Set(_protocolVariable.Name + ".Input" + i, 0, operation: "@init");
					Trace("Reset Input" + i);
				}
			}
			catch (Exception ex)
			{
				Error(ex.Message);
				Close();
			}
		}
		#endregion

		#region Close

		void RestartRequested()
		{
			_restart = true;
			Close();
		}
		void CloseRequested()
		{
			Close(); // Close driver window
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			try
			{
				// Disconnect from AppVision server
				if (_appServer.IsConnected)
					_appServer.Close();
			}
			catch
			{ }
			// If a restart has been requested by the server, relaunch the driver with the same command line arguments
			if (_restart)
			{
				var args = Environment.GetCommandLineArgs();
				Process.Start(args[0], args.Skip(1).Join(" "));
			}
		}
		bool _restart = false;

		#endregion

		// Handle change of variable values received from AppVision server
		//
		// in this demo driver, we toggle RadioButtons
		// in a real driver, the command should be reflected in the external system or device
		void VariableManager_StateChanged(VariableState state)
		{
			Dispatcher.BeginInvoke((Action)delegate
			{
				Trace($"variable changed: {state.Name} = {state.Value}");

				var radioBtn = outputs.Children.OfType<RadioButton>().FirstOrDefault(btn => btn.Name == state.Name.GetShortName());
				if (radioBtn != null)
				{
					radioBtn.IsChecked = state.Value.ToBool();
				}
			});
		}

		// Set variable values in AppVision server
		//
		// in this demo driver, we simulate IO events with CheckBoxes
		// in a real driver, the system or device events should be reflected in AppVision using variables
		private void Inputs_Click(object sender, RoutedEventArgs e)
		{
			var checkBox = (CheckBox)e.Source;
			var variableName = _protocolVariable.Name + "." + checkBox.Name;

			Trace($"Set {variableName}: {checkBox.IsChecked}");

			_appServer.VariableManager.Set(variableName, (bool)checkBox.IsChecked);
			// ex:     VariableManager.Set("DrvSampleVar.Input1", true);
		}

		
		private void SetBuffer_Click(object sender, RoutedEventArgs e)
		{
			var rand = new Random();
			var buffer = string.Join(" ", Enumerable.Range(0, 25).Select(i => rand.Next()).ToArray());

			Trace("Set Buffer: " + buffer);

			_appServer.VariableManager.Set(_protocolVariable.Name + ".Buffer", buffer);
			// ex:     VariableManager.Set("DrvSampleVar.Buffer", "12 35 2 78 456 -85 0 99");

		}

		private void SetEvent_Click(object sender, RoutedEventArgs e)
		{
			// Set a custom event in AppVision server
			_appServer.EventManager.AddEvent("test", "Sample Event");
		}

		private void TriggerAlarm_Click(object sender, RoutedEventArgs e)
		{
			// Trigger a custom alarm in AppVision server
			_appServer.AlarmManager.AddAlarmExt(1, "test", "Sample Alarm");
		}

		#region Trace,Error,Log
		private void Error(string text)
		{
			try
			{
				TraceWindow("Error: " + text);
				_appServer.Log("Error: " + text);
				if (_appServer.IsConnected)
					_appServer.AlarmManager.AddAlarmExt(10, text, _appServer.CurrentProtocol.Description);
			}
			catch { }
		}

		void Trace(string text)
		{
			TraceWindow(text);
			if (_appServer.LogLevel > 0)
				_appServer.Log(text);
		}

		void TraceWindow(string text)
		{
			Dispatcher.BeginInvoke((Action)delegate
			{
				while (listBox1.Items.Count > 250)
					listBox1.Items.RemoveAt(0);

				text = DateTime.Now.ToString("hh:mm:ss.fff ") + text;
				this.listBox1.Items.Add(text);
				listBox1.ScrollIntoView(text);
			});
		}
		#endregion
	}
}
