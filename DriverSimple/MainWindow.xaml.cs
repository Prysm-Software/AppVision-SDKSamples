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
	/// <summary>
	/// WPF driver demonstrating 8 simulated digital I/O channels, a custom alarm
	/// and a custom event.
	/// <para>
	/// Inputs (CheckBoxes) simulate incoming device signals pushed into AppVision
	/// via <see cref="Prysm.AppVision.SDK.VariableManager.Set"/>.
	/// Outputs (RadioButtons) reflect commands received from AppVision.
	/// </para>
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>AppVision driver server connection handle.</summary>
		AppServerForDriver _appServer;
		/// <summary>The protocol node variable; its sub-variables map to the 8 I/O channels.</summary>
		VariableRow _protocolVariable;

		/// <summary>
		/// Initialises the window, wires AppVision server event callbacks
		/// and sets the title bar to the product version.
		/// </summary>
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
		/// <summary>
		/// Parses the command-line arguments, connects to AppVision and subscribes
		/// to output variable notifications.
		/// Expected command line: <c>DriverSimple.exe protocolName[@hostname]</c>
		/// </summary>
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
				_appServer.AddFilterNotifications($"$V.{_protocolVariable.Name}.*");
				_appServer.StartNotifications(false);   // true to receive the output initial values
				_appServer.ProtocolSynchronized();
				Trace("Connected");


				// Set the initial value of Input variables.
				// The '@init' operation flag sets values without recording them as real device events
				// in the history database (useful for startup state synchronisation).
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

		/// <summary>Stops the driver and triggers a restart when requested by AppVision.</summary>
		void RestartRequested()
		{
			_restart = true;
			Close();
		}
		/// <summary>Closes the driver window when AppVision terminates this driver process.</summary>
		void CloseRequested()
		{
			Close(); // Close driver window
		}

		/// <summary>
		/// Disconnects from AppVision and optionally relaunches the driver process
		/// when a restart was requested by the server.
		/// </summary>
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

		/// <summary>
		/// Called by AppVision when an output variable changes value (e.g. an operator
		/// presses a button in a synoptic).
		/// In a real driver, the command would be forwarded to the physical device.
		/// Here, the corresponding RadioButton is toggled in the UI.
		/// </summary>
		/// <param name="state">New variable state containing name, value and timestamp.</param>
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

		/// <summary>
		/// Simulates an I/O input event: publishes the CheckBox state to AppVision
		/// as a variable value change.
		/// In a real driver this would be triggered by the physical device, not the UI.
		/// </summary>
		private void Inputs_Click(object sender, RoutedEventArgs e)
		{
			var checkBox = (CheckBox)e.Source;
			var variableName = _protocolVariable.Name + "." + checkBox.Name;

			Trace($"Set {variableName}: {checkBox.IsChecked}");

			_appServer.VariableManager.Set(variableName, (bool)checkBox.IsChecked);
			// ex:     VariableManager.Set("DrvSampleVar.Input1", true);
		}

		
		/// <summary>
		/// Sends a space-separated list of 25 random integers to the Buffer variable.
		/// Demonstrates publishing a multi-value payload to AppVision.
		/// </summary>
		private void SetBuffer_Click(object sender, RoutedEventArgs e)
		{
			var rand = new Random();
			var buffer = string.Join(" ", Enumerable.Range(0, 25).Select(i => rand.Next()).ToArray());

			Trace("Set Buffer: " + buffer);

			_appServer.VariableManager.Set(_protocolVariable.Name + ".Buffer", buffer);
			// ex:     VariableManager.Set("DrvSampleVar.Buffer", "12 35 2 78 456 -85 0 99");

		}

		/// <summary>Raises a custom free-text event in the AppVision event log.</summary>
		private void SetEvent_Click(object sender, RoutedEventArgs e)
		{
			// Set a custom event in AppVision server
			_appServer.EventManager.AddEvent("test", "Sample Event");
		}

		/// <summary>Raises a custom numbered alarm in the AppVision alarm table.</summary>
		private void TriggerAlarm_Click(object sender, RoutedEventArgs e)
		{
			// Trigger a custom alarm in AppVision server
			_appServer.AlarmManager.AddAlarmExt(1, "test", "Sample Alarm");
		}

		#region Trace,Error,Log
		/// <summary>
		/// Logs an error to the trace window, to the AppVision server log
		/// and raises an alarm in AppVision so the operator is notified.
		/// </summary>
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

		/// <summary>
		/// Logs a message to the local trace window and, if the server log level
		/// allows it, to the AppVision server log.
		/// </summary>
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
