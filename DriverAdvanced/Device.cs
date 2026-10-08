using System;
using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using Prysm.Logger;

namespace DriverAdvanced
{
	/// <summary>
	/// Represents one external device managed by this driver.
	/// Each linked variable (Source = "$P.&lt;protocolName&gt;") in AppVision maps to one Device instance.
	/// </summary>
	/// <remarks>
	/// Responsibilities:
	///   - Parse the device address and parameters from the AppVision variable configuration.
	///   - Open and close the connection to the physical or virtual external device.
	///   - Forward commands received from AppVision to the external device (VariableChanged).
	///   - Push data received from the external device back to AppVision (SetVariable).
	///   - Maintain the Comm status variable so AppVision knows the connection health.
	/// </remarks>
	public partial class Device
	{
		/// <summary>Reference to the driver, used to call SetVariable and AddAlarm.</summary>
		Driver _driver;

		/// <summary>
		/// The AppVision variable row that represents this device.
		/// Its Name is the root variable name (e.g. "Device1").
		/// Its Address and FullParameters hold the device connection settings.
		/// </summary>
		VariableRow _rootVar;

		/// <summary>
		/// Initialises the device by parsing its configuration from the AppVision variable row.
		/// The actual connection to the external device is opened in <see cref="Start"/>.
		/// </summary>
		/// <param name="driver">The parent driver instance.</param>
		/// <param name="deviceVariable">
		/// The AppVision variable row for this device.
		/// Its Address field typically holds the network address (IP, URL, COM port, etc.).
		/// Its FullParameters field holds additional named parameters (key=value pairs).
		/// </param>
		public Device(Driver driver, VariableRow deviceVariable)
		{
			_driver = driver;
			_rootVar = deviceVariable;

			// Log the raw address and parameters for troubleshooting.
			// HidePasswords() redacts any password tokens before writing to the log.
			Trace.Info($"[{_rootVar.Name}] {_rootVar.Address?.HidePasswords()}");
			Trace.Info($"[{_rootVar.Name}] {_rootVar.FullParameters?.HidePasswords().Trim()}");

			// --- Parse device-level connection settings ---
			// The Address field usually contains the network endpoint of the device.
			// The FullParameters field contains additional named settings in "key=value" format.
			// todo: uncomment and adapt the examples below to match your device configuration.

			//Uri adrs = Helper.ParseUri(deviceVariable.Address);
			//string param1 = Helper.GetParameter("Param1", deviceVariable.FullParameters, "Param1Default");
			//int param2 = Helper.GetParameter<int>("Param2", deviceVariable.FullParameters);

			// Set the initial communication status to Unknown while the device is not yet connected.
			// AppVision uses this value to display the connection state in the variable list.
			_driver.SetVariable(_rootVar.Name + ".Comm", (int)CommStatus.Unknown);
		}

		/// <summary>
		/// Opens the connection to the external device and starts monitoring its state.
		/// Subscribes to AppVision variable events so that commands from operators or
		/// rules are forwarded to the device.
		/// Called once per device at driver startup (and again after a restart).
		/// </summary>
		public void Start()
		{
			Trace.Info($"[{_rootVar.Name}] Start");

			// Subscribe to AppVision variable state changes (commands).
			// VariableChanged is called when an operator or a rule writes to a variable
			// owned by this device that has the "Command" flag checked.
			_driver.AppServer.VariableManager.StateChanged += VariableChanged;

			// Subscribe to AppVision variable row updates (configuration changes).
			// variableUpdated is called when a variable's metadata (description, parameters, etc.)
			// is modified in the AppVision Configurator while the driver is running.
			// Remove this subscription if your driver does not need to react to config changes.
			_driver.AppServer.VariableManager.RowUpdated += variableUpdated;

			// todo: establish the connection to the external device here.
			// For example: open a TCP socket, connect via SDK, start a polling timer, etc.

			// todo: report the result of the connection attempt:
			//   on success  -> CommStatus.Connected  (see below)
			//   on failure  -> CommStatus.Fault + AddAlarm

			// Simulate a successful connection for the sample.
			Trace.Info($"[{_rootVar.Name}] Connected");
			_driver.SetVariable(_rootVar.Name + ".Comm", (int)CommStatus.Connected);

			// todo: when an established connection is lost unexpectedly, set Fault and raise an alarm:
			//Trace.Error("error message");
			//_driver.SetVariable(_rootVar.Name + ".Comm", (int)CommStatus.Fault);
		}

		/// <summary>
		/// Closes the connection to the external device and unsubscribes from AppVision events.
		/// Called at driver shutdown or before a restart.
		/// </summary>
		public void Stop()
		{
			Trace.Info($"[{_rootVar.Name}] Stop");

			// Unsubscribe from AppVision events to avoid receiving callbacks after Stop.
			_driver.AppServer.VariableManager.StateChanged -= VariableChanged;
			_driver.AppServer.VariableManager.RowUpdated -= variableUpdated;

			// todo: close the connection to the external device and release resources.

			// Inform AppVision that this device is intentionally disconnected.
			Trace.Info($"[{_rootVar.Name}] Disconnected");
			_driver.SetVariable(_rootVar.Name + ".Comm", (int)CommStatus.Disconnected);
		}

		/// <summary>
		/// Called by AppVision when a variable owned by this device changes state.
		/// This is the main entry point for commands flowing from AppVision to the external device.
		/// Only variables with the "Command" flag checked trigger this event (see notification filter
		/// in Driver.cs: "Variable.StateType=1").
		/// </summary>
		/// <param name="state">
		/// The new variable state. Key properties:
		///   state.Name  - full variable name (e.g. "Device1.Output1")
		///   state.Value - new value set by the operator or rule
		///   state.Row   - the full variable configuration row
		/// </param>
		void VariableChanged(VariableState state)
		{
			// Filter: only process variables that belong to this device.
			// Variable names follow the hierarchy: "<DeviceName>.<SubVariable>".
			if (!state.Name.StartsWith(_rootVar.Name + "."))
				return;
			try
			{
				// todo: read state.Name and state.Value and forward the command to the external device.
				// Example:
				//   if (state.Name == _rootVar.Name + ".Output1")
				//       _myDevice.SetOutput(1, (bool)state.Value);

				// Sample simulation (remove it): the device "executes" a command <x>.Cmd and reports
				// the result in <x>.Status, e.g. DriverAdvanced.DeviceA.Sensor01.Cmd -> .Sensor01.Status
				// in the configuration provided with the sample.
				if (state.Name.EndsWith(".Cmd"))
					_driver.SetVariable(state.Name.Substring(0, state.Name.Length - ".Cmd".Length) + ".Status", state.Value.ToBool() ? 1 : 0);
			}
			catch (Exception ex)
			{
				_driver.AddAlarm(ex, state.Row);
			}
		}

		/// <summary>
		/// Called by AppVision when a variable's configuration row is updated in the Configurator
		/// while the driver is running (e.g. address or parameter change).
		/// Use this to reconnect with new settings without restarting the whole driver.
		/// Remove this method and its subscription in <see cref="Start"/>/<see cref="Stop"/>
		/// if your driver does not need to react to live configuration changes.
		/// </summary>
		/// <param name="row">The updated variable row.</param>
		void variableUpdated(VariableRow row)
		{
			// Filter: only process rows that belong to this device.
			if (!row.Name.StartsWith(_rootVar.Name + "."))
				return;

			Trace.Info($">app> update: {row.Name}");
			try
			{
				// todo: reflect the configuration change in the external device.
				// For example, if the address changed: reconnect with the new endpoint.
			}
			catch (Exception ex)
			{
				_driver.AddAlarm(ex, row);
			}
		}
	}
}
