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
			Trace.Info(Environment.CommandLine.HidePasswords());

			// Create the SDK gateway. No network connection is made at this point.
			AppServer = new AppServerForDriver();

			// Log every variable state change received from AppVision (useful for debugging).
			AppServer.VariableManager.StateChanged += s => Trace.Info($">app> {s.Name}={s.Value}");

			// Note: restart and stop requests are handled in App.xaml.cs with AppServer.ControllerManager.Closed
			// and .Restart. AppServerForDriver.Restart is obsolete and never raised: do not use it.

			// AppVision sends a download command (e.g. push config to an AppControl controller).
			AppServer.Download += Download;

			// AppVision sends a refresh command (e.g. resync all device states).
			AppServer.Refresh += Refresh;

			// Write the Trace entries to the driver log file through AppServer.Log():
			// C:\ProgramData\AppVision 4\LogFiles\<year>\<product>\<protocol>_<yyMM>.log
			// Entries are filtered by the "Log level" of the protocol (Configurator > Protocols > Parameters).
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
				// AppVision launches this driver with: DriverAdvanced.exe protocolName[@hostname] [-password=...]
				// (the password of the protocol, if any, is read by Login from the command line).
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
				// Login throws if the protocol does not exist (Configurator > Protocols).
				AppServer.Login(protocolName);

				// Optional: refuse to run a driver built after the end of the AppVision warranty
				// of the site (license). BuildInfo.Date is generated at each build by BuildInfo.tt.
				if (!AppServer.CheckWarrantyDate(BuildInfo.Date))
					throw new Exception("This driver was built after the end of the AppVision warranty of this site.");

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
				// Multiple masks are joined with "|" (logical OR) in ONE call: a new "$V." filter
				// replaces the previous one. An empty filter is ignored (no variable notification).
				// todo: adjust the filter if you need a different scope (e.g. a single specific variable).
				var filter = deviceVars.Select(v => "$V." + v.Name + ".*").Join("|");
				AppServer.AddFilterNotifications(filter);

				// Additionally, restrict state change notifications to variables with the
				// "Command" flag checked (StateType=1). These are the variables that operators
				// or AppVision rules write to in order to send commands to the driver.
				AppServer.AddFilterNotifications("Variable.StateType=1");

				// Begin receiving notifications (variable changes and the Close/Download/Refresh commands).
				// false: do not send the current values of the subscribed variables now (true sends them at once).
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

				Trace.Info(_devices.Count + " device(s) running");
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
		/// Called when AppVision closes the protocol, when the connection to the server is lost,
		/// or when the user closes the window with Ctrl held (see App.xaml.cs).
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
		/// <param name="varNames">Comma-separated names of the device variables to download (e.g. "Access.C001").</param>
		/// <param name="parameter">What to download, e.g. "Holidays,Timetables,Rights".</param>
		internal void Download(string varNames, string parameter)
		{
			Trace.Info($"Download {varNames} {parameter}");
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
		/// <param name="parameter">Parameter of the refresh request (empty when started from the driver window).</param>
		public void Refresh(string parameter)
		{
			Trace.Info($"Refresh {parameter}");
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
				Trace.Error(ex, $"[{row?.Name}]");

				// AddAlarmExt parameters:
				//   severity    : 1 (low) .. 100 (high), here 10
				//   state       : short text shown in the State column of the alarm list
				//   description : description of the object in alarm (here the variable's full description)
				//   info        : additional information (here the full exception text)
				//   areaName    : area of the alarm (for area-based filtering and user rights)
				//   groupNames  : groups of the alarm (for group-based filtering)
				// For a protocol-level error (row == null), the protocol description is used instead.
				AppServer.AlarmManager.AddAlarmExt(10, ex.Message, row?.FullDescription ?? AppServer.CurrentProtocol?.Description, ex.ToString(), row?.AreaDescription, row?.FullGroupDescriptions);
			}
			catch (Exception ex2)
			{
				// never let error reporting crash the driver
				Trace.Error(ex2);
			}
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
		/// <param name="info">Optional free-text information attached to the value (Info column of the events).</param>
		/// <param name="operation">
		/// Optional operation flags, comma separated: "@init" (initial value: not archived, no server script),
		/// "@changeonly" (the server ignores the value if it did not change), "@log" (value read from a device log).
		/// </param>
		/// <remarks>
		/// VariableManager.Set is queued and sent asynchronously by the SDK: it does not throw on network errors.
		/// </remarks>
		internal void SetVariableNoCache(string name, object value, string info = null, string operation = null)
		{
			try
			{
				Trace.Info($"<drv< {name}={value}	{info}");
				_cache[name] = value;
				AppServer.VariableManager.Set(name, value, info, operation);
			}
			catch (Exception ex)
			{
				_cache.TryRemove(name, out _);      // send it again next time
				Trace.Error(ex, $"[{name}]");
			}
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

			SetVariableNoCache(name, value, info, operation);       // updates the cache
		}
	}
}
