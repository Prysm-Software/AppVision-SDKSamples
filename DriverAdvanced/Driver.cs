using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Prysm.AppVision.SDK;
using Prysm.AppVision.Data;
using Prysm.AppVision.Common;
using Prysm.Logger;
using Prysm.Utility.Logger;

namespace DriverAdvanced
{
	/// <summary>
	/// Core driver class. Manages the connection to the AppVision server and the lifecycle
	/// of all <see cref="Device"/> instances (one per linked variable).
	/// </summary>
	/// <remarks>
	/// This class is split across two files via the partial keyword:
	///   Driver.cs  - protocol-level logic (server connection, device discovery, alarms, variable I/O)
	///   Device.cs  - device-level logic (per-device connection, commands, data push)
	/// </remarks>
	public partial class Driver
	{
		/// <summary>
		/// The single gateway object used to communicate with the AppVision server.
		/// All SDK operations (variable read/write, alarms, notifications) go through this instance.
		/// </summary>
		internal AppServerForDriver AppServer;

		/// <summary>List of active device instances, one per linked variable.</summary>
		List<Device> _devices = new List<Device>();

		/// <summary>
		/// Value cache used by <see cref="SetVariable"/> to suppress redundant updates.
		/// Keyed by variable name, value is the last value successfully sent to AppVision.
		/// </summary>
		ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

		/// <summary>
		/// Initialises the AppVision server gateway and wires up server-level events.
		/// The actual connection to AppVision is opened later in <see cref="Start"/>.
		/// </summary>
		public Driver()
		{
			Trace.Info(Environment.CommandLine);

			// Create the SDK gateway. No network connection is made at this point.
			AppServer = new AppServerForDriver();

			// Log every variable state change received from AppVision (useful for debugging).
			AppServer.VariableManager.StateChanged += s => Trace.Info($">app> {s.Name}={s.Value}");

			// AppVision requests a full restart of the driver (e.g. after a configuration reload).
			AppServer.Restart += () => { Stop(); Start(); };

			// AppVision sends a download command (e.g. push config to an AppControl controller).
			AppServer.Download += Download;

			// AppVision sends a refresh command (e.g. resync all device states).
			AppServer.Refresh += Refresh;

			// Forward driver log entries to the AppVision server log so they appear
			// in the AppVision Manager diagnostic console alongside other server logs.
			Trace.Logger.AddAppender(new AppServerLogAppender(AppServer), LogLevel.All, "app");
		}

		/// <summary>
		/// Connects to the AppVision server, registers the protocol, discovers linked variables,
		/// configures notification filters, and starts all <see cref="Device"/> instances.
		/// </summary>
		public void Start()
		{
			try
			{
				// AppVision launches this driver with: DriverAdvanced.exe protocolName[@hostname]
				// Split the argument to separate the protocol name from the optional server host.
				var args = Environment.GetCommandLineArgs();
				var a = args[1].Split('@');
				var protocolName = a[0];
				var hostName = (a.Length > 1) ? a[1] : ""; // empty = connect to local machine

				// Open a TCP connection to the AppVision server and authenticate as a driver.
				Trace.Info($"Connecting to server {hostName}");
				AppServer.Open(hostName);

				// Login tells AppVision which protocol this driver manages.
				// AppVision will then route all events for that protocol to this process.
				AppServer.Login(protocolName);

				// Ensure this driver binary was compiled within the SDK warranty period.
				// If the warranty has expired the driver is not allowed to run.
				if (!AppServer.CheckWarrantyDate(BuildInfo.Date))
					throw new Exception("This driver was built after your warranty has expied.");

				// --- Read protocol-level settings from AppVision configuration ---
				// The protocol row (AppServer.CurrentProtocol) holds global settings shared
				// by all devices of this protocol: address, timeout, retry count, custom parameters.
				// todo: uncomment and adapt the examples below as required.

				//IPEndPoint host = Helper.ParseEndPoint(AppServer.CurrentProtocol.Address, 80);
				//int timeout = AppServer.CurrentProtocol.Timeout;
				//int retry = AppServer.CurrentProtocol.MaxRetry;
				//string param1 = Helper.GetParameter("Param1", AppServer.CurrentProtocol.Parameters, "Param1Default");
				//int param2 = Helper.GetParameter<int>("Param2", AppServer.CurrentProtocol.Parameters);

				// Retrieve all variables whose Source = "$P.<protocolName>".
				// Each returned VariableRow represents one device that this driver must manage.
				var deviceVars = AppServer.GetVariablesByProtocol();
				if (deviceVars.Length == 0)
					Trace.Info("This protocol has no device");

				// Build a notification filter so AppVision only sends us events we care about.
				// "$V.Device1.*" matches all sub-variables of Device1 (e.g. Device1.Comm, Device1.Temperature, ...).
				// Multiple filters are joined with "|" (logical OR).
				// todo: adjust the filter if you need a different scope (e.g. a single specific variable).
				var filter = deviceVars.Select(v => "$V." + v.Name + ".*").Join("|");
				AppServer.AddFilterNotifications(filter);

				// Additionally, restrict state change notifications to variables with the
				// "Command" flag checked (StateType=1). These are the variables that operators
				// or AppVision rules write to in order to send commands to the driver.
				AppServer.AddFilterNotifications("Variable.StateType=1");

				// Begin receiving notification callbacks. Pass false for synchronous (inline) delivery.
				AppServer.StartNotifications(false);

				// Signal to AppVision that this driver has finished its initialisation and is
				// ready to process commands. AppVision will mark the protocol as "synchronized".
				AppServer.ProtocolSynchronized();
				Trace.Info($"Protocol {protocolName} synchronized");

				// Instantiate and start one Device object per linked variable.
				foreach (var var in deviceVars)
				{
					try
					{
						var device = new Device(this, var);
						device.Start();
						_devices.Add(device);
					}
					catch (Exception ex)
					{
						// A single device failure must not prevent the other devices from starting.
						AddAlarm(ex, var);
					}
				}

				// Display a summary in the MainWindow status bar.
				Trace.StatusText = _devices.Count + " device(s) running";
			}
			catch (Exception ex)
			{
				// Choose one of the three error strategies below:
				//AddAlarm(ex);       // raise an alarm in AppVision and keep running
				//throw;              // propagate - App.xaml.cs will close the driver in 5 s
				Trace.Error(ex.Message); // log and keep running (no alarm raised)
			}
		}

		/// <summary>
		/// Stops all <see cref="Device"/> instances and disconnects from the AppVision server.
		/// Called on graceful shutdown (Ctrl+close) or before a restart.
		/// </summary>
		public void Stop()
		{
			Trace.Info("Stopping");
			try
			{
				// Stop each device cleanly so it can set its Comm status to Disconnected
				// and release its own resources (sockets, timers, threads, etc.).
				foreach (var device in _devices)
					device.Stop();

				_devices.Clear();

				// Close the connection to AppVision only if it is still open.
				if (AppServer.IsConnected)
					AppServer.Close();

				Trace.Info("Stopped");
			}
			catch (Exception ex)
			{
				Trace.Error(ex);
			}
		}

		/// <summary>
		/// Called by AppVision when a download command is issued for this protocol.
		/// Typically used in AppControl scenarios to push a configuration snapshot
		/// to a physical controller.
		/// Remove this method if your system does not support download.
		/// </summary>
		internal void Download(string command, string parameters)
		{
			Trace.Info("Download");
			try
			{
				// todo: push configuration/data to the external system.
			}
			catch (Exception ex)
			{
				AddAlarm(ex);
			}
		}

		/// <summary>
		/// Called by AppVision when a refresh command is issued for this protocol.
		/// Use this to request a full resynchronisation of all device states,
		/// for example after a communication interruption.
		/// Remove this method if your system does not support refresh.
		/// </summary>
		public void Refresh(string message)
		{
			Trace.Info("Refresh");
			try
			{
				// todo: re-read all device states and call SetVariable for each value.
			}
			catch (Exception ex)
			{
				AddAlarm(ex);
			}
		}

		/// <summary>
		/// Raises a structured alarm in AppVision and logs the error.
		/// </summary>
		/// <param name="ex">The exception that caused the alarm.</param>
		/// <param name="row">
		/// The variable row associated with the error (provides area, group, and description
		/// context for the alarm). Pass null for protocol-level errors.
		/// </param>
		internal void AddAlarm(Exception ex, VariableRow row = null)
		{
			try
			{
#if MAX_ALARMS_COUNT_PER_MIN
				// Optional guard: limit the number of alarms raised per minute to avoid
				// flooding the AppVision alarm list when a device is generating continuous errors.
				// Enable by defining the MAX_ALARMS_COUNT_PER_MIN compilation symbol.
				if (DateTime.UtcNow < _alrDate.AddMinutes(1))
				{
					if (_alrCount++ >= 60)
					{
						if (_alrCount == 61)
						{
							string msg = $"Maximum alarms count reach. {AppServer?.CurrentProtocol.Name} alarms will be disabled for the next minute";
							Trace.Error(msg);
							AppServer.AlarmManager.AddAlarmExt(92, "Too many errors", AppServer?.CurrentProtocol.Name, msg);
						}
						return;
					}
				}
				else
				{
					_alrDate = DateTime.UtcNow;
					_alrCount = 0;
				}
#endif
				Trace.Error(ex, $"[{row.Name}]");

				// AddAlarmExt parameters:
				//   alarmCode        : numeric code used to identify the alarm type in AppVision rules
				//   shortMessage     : brief description shown in the alarm list
				//   objectDescription: the variable's full description (displayed in context)
				//   detailMessage    : full exception text (shown in alarm detail view)
				//   areaDescription  : the area path of the variable (for area-based filtering)
				//   groupDescriptions: group membership of the variable (for group-based filtering)
				AppServer.AlarmManager.AddAlarmExt(10, ex.Message, row?.FullDescription, ex.ToString(), row?.AreaDescription, row?.FullGroupDescriptions);
			}
			catch { }
		}
#if MAX_ALARMS_COUNT_PER_MIN
		DateTime _alrDate;
		int _alrCount;
#endif

		/// <summary>
		/// Sends a variable value to AppVision unconditionally, bypassing the value cache.
		/// Use this when you need to force-refresh a value even if it has not changed.
		/// </summary>
		/// <param name="name">Full variable name (e.g. "Device1.Temperature").</param>
		/// <param name="value">New value to set.</param>
		/// <param name="info">Optional free-text information attached to the value (visible in logs).</param>
		/// <param name="operation">Optional operation qualifier (e.g. "ack", "reset").</param>
		internal void SetVariableNoCache(string name, object value, string info = null, string operation = null)
		{
			try
			{
				Trace.Info($"<drv< {name}={value}	{info}");
				AppServer.VariableManager.Set(name, value, info, operation);
			}
			catch { }
		}

		/// <summary>
		/// Sends a variable value to AppVision only if the value has changed since the last call.
		/// This cache prevents AppVision from being updated with identical values, which would
		/// generate unnecessary database writes and event traffic on the server.
		/// </summary>
		/// <param name="name">Full variable name (e.g. "Device1.Temperature").</param>
		/// <param name="value">New value to set.</param>
		/// <param name="info">Optional free-text information attached to the value.</param>
		/// <param name="operation">Optional operation qualifier.</param>
		internal void SetVariable(string name, object value, string info = null, string operation = null)
		{
			// Compare against the cached value. Skip the update if nothing has changed.
			object val;
			if (_cache.TryGetValue(name, out val))
				if (value?.Equals(val) == true)
					return;

			_cache[name] = value;
			SetVariableNoCache(name, value, info, operation);
		}
	}
}
