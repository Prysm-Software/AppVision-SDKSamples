using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System.Collections;
using System.Reflection;
using Prysm.AppControl.SDK;
using System.Collections.Concurrent;
using System.Net;

namespace DriverAppControl
{
	/// <summary>
	/// AppControl driver — connects to AppVision as a protocol driver, manages a set of
	/// access-control devices and bridges events between them and the AppVision server.
	/// </summary>
	class Driver
	{
		#region public events & properties
		/// <summary>AppVision server connection shared by all devices.</summary>
		internal static AppServer Server
		{
			get { return _this._appServer; }
		}
		/// <summary>Returns the hostname of the connected AppVision server.</summary>
		internal static string GetHostname()
		{
			return _this._appServer.Hostname;
		}

		/// <summary>Gets whether the driver thread is running.</summary>
		internal bool IsAlive
		{
			get
			{
				if (_thread == null)
					return false;
				return _thread.IsAlive;
			}
		}
		/// <summary>Raised when the driver thread terminates (normal or after a restart).</summary>
		internal event Action Closed;
		/// <summary>
		/// Delegates for trace and error
		/// </summary>
		internal Action<string> TraceDelegate = null;
		internal Action<string> ErrorDelegate = null;
		#endregion

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="argLine">protocolName@hostname or protocolName</param>
		internal Driver(string argLine)
		{
			_this = this;
			string[] ss = argLine.Split('@');
			_protocolName = ss[0];
			if (ss.Length > 1)
				_serverHostname = ss[1];
			_appServer = new AppServerForDriver();
			_appServer.VariableManager.StateChanged += appServer_VariableStateChanged;
			_appServer.ControllerManager.Restart += appServer_Restart;
			_appServer.ControllerManager.Closed += appServer_Closed;
			_appServer.Download += appServer_Download;
			_appServer.NewModification += appServer_NewModification;

			Device.TraceDelegate = Trace;
			Device.ErrorDelegate = Error;
		}

		#region Start(),Stop()
		/// <summary>Starts the driver on a background thread.</summary>
		internal void Start()
		{
			_thread = new Thread(new ThreadStart(Main));
			_thread.Start();
		}
		/// <summary>Requests an orderly shutdown: stops each device then disconnects from AppVision.</summary>
		internal void Stop()
		{
			_serverStopWaiting = true;
		}
		#endregion

		#region Main()
		/// <summary>
		/// Driver main loop: connects to AppVision, creates devices, processes the event queue
		/// and handles periodic modification downloads until <see cref="Stop"/> is called.
		/// </summary>
		void Main()
		{
			_serverStopWaiting = false;
			try
			{
				// 1. initialisation
				Trace("Server connection ...");
				_appServer.Open(_serverHostname);
				_appServer.Login(_protocolName);		// throws if the protocol does not exist

				// The device index files ({name}_TID.xml, ...) are written in the current directory:
				// here the Param folder of the configuration (the driver must run on the server machine).
				Environment.CurrentDirectory = _appServer.GetPathParam();

				// to get output command from the server: subscribe to the command variables
				// of every controller, e.g. "$V.Access.C001.*.Cmd" (Access.C001.Door1.Cmd, ...)
				var variablesProtocol = _appServer.GetVariablesByProtocol(_protocolName);
				if (variablesProtocol.Length == 0)
					Trace("No variable connected to this protocol");
				var filters = variablesProtocol.Select(v => "$V." + v.Name + ".*.Cmd");
				_appServer.AddFilterNotifications(string.Join("|", filters));
				_appServer.StartNotifications(false);
				_appServer.ProtocolSynchronized();
				Trace("Connection success.");

				// create access controler device objects
				CreateDevices(variablesProtocol);

				// 2. main loop
				bool deviceStopWaiting = false;
				DateTime dateDownloadModifications = DateTime.MinValue;
				for(;;)
				{
					// check ending condition
					if (_serverStopWaiting)
					{
						if (deviceStopWaiting)
						{
							if (IsAllDevicesClosed())
								break;
						}
						else
						{
							CloseDevices();
							deviceStopWaiting = true;
						}
					}

					// periodically download modifications
					if (dateDownloadModifications < DateTime.UtcNow && _newModification)
					{
						_newModification = false;
						dateDownloadModifications = DateTime.UtcNow.AddSeconds(_tempoDownloadModifications);
						try
						{
							DownloadModifications();
						}
						catch (Exception ex)	// e.g. AppControl data service not reachable: keep the driver running
						{
							Error("DownloadModifications: " + ex.Message);
						}
					}

					int count = 0;
					object e;
					while (_events.TryDequeue(out e))
					{
						count++;
						if (e is VariableState)
							OnVariableState(e as VariableState);
						else if (e is AccessControlEventArgs)
							OnAccessControlEvent((AccessControlEventArgs)e);
						else if (e is InputChangedEventArgs)
							OnInputChanged((InputChangedEventArgs)e);
						else if (e is ConnectChangedEventArgs)
							OnConnectChanged((ConnectChangedEventArgs)e);
						else if (e is TraceEventArgs)
							OnTraceEvent((TraceEventArgs)e);
						else if (e is DownloadInfo)
							OnDownload(e as DownloadInfo);
					}
					if (count == 0)
						Thread.Sleep(50);
				}
			}
			catch (Exception ex)
			{
				Error(ex.Message);
				Log(ex.ToString());
			}
			finally
			{
				#region 3. finalisation
				// stop the device threads still running (main loop left on an exception)
				CloseDevices();
				_devices.Clear();

				try
				{
					if (_appServer.IsConnected)
						_appServer.Close();		// also logs the protocol out
				}
				catch (Exception ex)
				{
					Log("Close: " + ex.Message);
				}

				// check restart (connection lost): reconnect with a new set of devices.
				// If the server is still unreachable, Open fails and the driver ends (Closed).
				if (_restart)
				{
					_restart = false;
					Start();
				}
				else
				{
                    _thread = null;
					Log("End");
					if (Closed != null)
						Closed();
				}
				#endregion
			}
		}
		#endregion

		#region CreateDevices(),CloseDevices(),IsAllDevicesClosed()
		/// <summary>
		/// Instantiates one <see cref="Device"/> per variable declared under this protocol
		/// and starts each device's acquisition thread.
		/// </summary>
		void CreateDevices(IEnumerable<VariableRow> variablesProtocol)
		{
			foreach (var v in variablesProtocol)
				try
				{
					Device device = new Device(v.Id, v.Name, v.Address, v.FullParameters, _protocol.Timeout, _protocol.MaxRetry);
					device.ConnectChanged += device_ConnectChanged;
					device.InputChanged += device_InputChanged;
					device.AccessControlEvent += device_AccessControlEvent;
					device.TraceEvent += device_TraceEvent;
					device.Start();	// start acquisition
					_devices.Add(device);
				}
				catch (Exception ex)
				{
					SendInfo(1, v.Description + ": " + ex.Message);
				}
		}
		/// <summary>Requests all devices to stop their acquisition threads.</summary>
		void CloseDevices()
		{
			foreach (var d in _devices)
				try
				{
					d.Stop();
				}
				catch (Exception ex)
				{
					SendInfo(1, d.Name + ": " + ex.Message);
				}
		}
		/// <summary>Returns <see langword="true"/> when every device channel is closed.</summary>
		bool IsAllDevicesClosed()
		{
			foreach (var d in _devices)
				if (d.IsOpened)
					return false;
			return true;
		}
		#endregion

		#region appServer_Restart
		/// <summary>
		/// Raised by the SDK when the connection to the server is lost (keep-alive failure):
		/// sets the restart flag and stops the driver, which then tries to reconnect once.
		/// (Since AppVision 4.8.5 a protocol restart requested by the server is a Close followed by a new process.)
		/// </summary>
		void appServer_Restart()
		{
			_restart = true;
			Stop();
		}
		volatile bool _restart = false;
		/// <summary>AppVision stops the protocol: stops the driver.</summary>
		void appServer_Closed()
		{
			Stop();
		}

		/// <summary>
		/// AppVision signals that the configuration changed (people, badges, rights, timetables, holidays...):
		/// the main loop reads the modification list at most every <see cref="_tempoDownloadModifications"/> seconds.
		/// </summary>
		void appServer_NewModification()
		{
			_newModification = true;
		}
		volatile bool _newModification = true;

		#endregion
		#region appServer_VariableStateChanged()
		/// <summary>Enqueues an incoming variable state change for processing on the driver thread.</summary>
		void appServer_VariableStateChanged(VariableState obj)
		{
			_events.Enqueue(obj);
		}
		/// <summary>
		/// Processes a variable state change: decodes the variable name to identify the target
		/// device and forwards the command (e.g. <c>Door1.Cmd</c>) to that device.
		/// </summary>
		private void OnVariableState(VariableState variableState)
		{
			string val = variableState.Value.ToString();
			Trace("From server: " + variableState.Name + "=" + val);
			// decode and check variable name
			// ex: Access.C001.Door1.Cmd
			var ss = variableState.Name.Split('.');
			if (ss.Length > 2)
			{
				string cmdName = ss[ss.Length - 2] + "." + ss[ss.Length - 1];	// Door1.Cmd
				string devName = variableState.Name.Substring(0, variableState.Name.Length - cmdName.Length - 1);
				var d = GetDeviceByName(devName);
				if (d != null)
					d.SendCommand(new CommandAction { Name = cmdName, Value = val });
			}
		}
		#endregion
		#region appServer_Download()
		/// <summary>Enqueues a full-download request from AppVision for processing on the driver thread.</summary>
		void appServer_Download(string varNames, string parameter)
		{
			_events.Enqueue(new DownloadInfo { VarNames = varNames, Parameter = parameter });
		}
		/// <summary>
		/// Dispatches a full-download command to each device named in <paramref name="di"/>.
		/// </summary>
		private void OnDownload(DownloadInfo di)
		{
			Trace("OnDownload: " + di.VarNames + " " + di.Parameter);
			// Note: varNames contains variable controller names, ex: Access.C001
			foreach (string varName in di.VarNames.Split(','))
			{
				var d = GetDeviceByName(varName);
				if (d != null)
					d.SendCommand(new CommandDownloadInfo { Parameter = di.Parameter });
				else
					Trace("Device unknown: " + varName);
			}
		}
		#endregion

		#region device_ConnectChanged()
		/// <summary>Enqueues a device connection-state change for processing on the driver thread.</summary>
		void device_ConnectChanged(object sender, ConnectChangedEventArgs e)
		{
			_events.Enqueue(e);
		}
		/// <summary>
		/// Publishes the device connection state to AppVision via the <c>&lt;device&gt;.Comm</c> variable.
		/// </summary>
		void OnConnectChanged(ConnectChangedEventArgs e)
		{
			// send variable state with variable .Comm
			string varName = e.DeviceName + ".Comm";
			int comm = (int)e.Connected;
			SendVariable(varName, comm, e.Info);
		}
		#endregion
		#region device_InputChanged()
		/// <summary>Enqueues a device input change (door state/mode) for processing on the driver thread.</summary>
		void device_InputChanged(object sender, InputChangedEventArgs e)
		{
			_events.Enqueue(e);
		}
		/// <summary>
		/// Maps a device input event to an AppVision variable name and publishes the value.
		/// </summary>
		void OnInputChanged(InputChangedEventArgs e)
		{
			string varName;
			switch (e.Type)
			{
				case DeviceEventType.DoorState:
					varName = string.Format("{0}.Door{1}.Status", e.DeviceName, e.InputId);
					break;
				case DeviceEventType.DoorMode:
					varName = string.Format("{0}.Door{1}.Mode", e.DeviceName, e.InputId);
					break;
				default:
					return;
			}
			SendVariable(varName, e.InputValue, "", e.DateUtc);
		}
		#endregion
		#region device_AccessControlEvent()
		/// <summary>Enqueues a badge-read event for processing on the driver thread.</summary>
		void device_AccessControlEvent(object sender, AccessControlEventArgs e)
		{
			_events.Enqueue(e);
		}
		/// <summary>
		/// Publishes a badge-read result (granted/denied, person ID, badge code) to AppVision
		/// via the <c>&lt;device&gt;.Reader1</c> variable.
		/// </summary>
		private void OnAccessControlEvent(AccessControlEventArgs e)
		{
			// same naming as Device._readerNames: <device>.Reader01 .. <device>.Reader04
			string varName = string.Format("{0}.Reader{1:00}", e.DeviceName, e.ReaderId);
			SendVariableBadge(varName, e.Status, e.UserId, e.BadgeId, e.DateUtc);
		}
		#endregion
		#region device_TraceEvent()
		/// <summary>Enqueues a device trace/error message for processing on the driver thread.</summary>
		void device_TraceEvent(object sender, TraceEventArgs e)
		{
			_events.Enqueue(e);
		}
		/// <summary>
		/// Forwards a device trace message to AppVision via the <c>&lt;device&gt;.Trace</c> variable.
		/// </summary>
		void OnTraceEvent(TraceEventArgs e)
		{
			string varName = e.DeviceName + ".Trace";
			SendVariable(varName, e.Text, "", null, e.Severity);
		}
		#endregion

		#region DownloadModifications()
		/// <summary>
		/// Polls AppVision for configuration changes since the last known modification ID and
		/// dispatches incremental download commands to each affected device.
		/// </summary>
		private void DownloadModifications()
		{
			Trace("DownloadModifications");

			// 1. extraction
			Datalayer dl = new Datalayer(_appServer);
			if (_lastModificationId < 0)
			{
				_lastModificationId = dl.GetLastModificationId();	// the first time: goto the last modif
				return;
			}
			var modifs = dl.GetLastModifications(_lastModificationId);
			if (modifs.Length == 0)
				return;
			_lastModificationId = (int)modifs.Last().Id;

			// 2. build entities list to download
			List<int> downloadUsers = new List<int>();			// Person.Id list
			List<int> downloadTimetables = new List<int>();		// VariableRow.Id list
			List<int> downloadVariables = new List<int>();		// VariableRow.Id list
			bool downloadHolidays = false, downloadParameters;
			foreach (var m in modifs)
				OnModification(m, ref downloadUsers, ref downloadTimetables, ref downloadVariables, ref downloadHolidays);

			// 3. dispath modifs to devices
			foreach (var d in _devices)
			{
				downloadParameters = downloadVariables.Contains(d.Id);
				if (downloadUsers.Count > 0 || downloadTimetables.Count > 0 || downloadHolidays || downloadParameters)
					d.SendCommand(new CommandDownloadList { Users = downloadUsers, Timetables = downloadTimetables, Holidays = downloadHolidays, Parameters = downloadParameters });
			}
		}
		int _lastModificationId = -1;
		/// <summary>
		/// Classifies a single modification record and adds the affected entity IDs to the
		/// appropriate download lists.
		/// </summary>
		void OnModification(Prysm.AppControl.SDK.AppVisionHistoDataServiceReference.ModifRow m, ref List<int> downloadUsers, ref List<int> downloadTimetables, ref List<int> downloadVariables, ref bool downloadHolidays)
		{
			Trace(string.Format("OnModif: table={0},type={1},index={2}", m.Table, m.Type, m.SourceId));
			switch (m.Table)
			{
				case "Apc_PersonRights":
				case "Apc_PersonProfiles":
				case "Apc_People":
				case "Apc_Badges":
					if (m.SourceId > 0)
						if (!downloadUsers.Contains(m.SourceId))
							downloadUsers.Add(m.SourceId);
					break;
				case "Apc_AccessProfileRights":
					{
						Datalayer dl = new Datalayer(_appServer);
						var people = dl.GetPeopleByProfileId(m.SourceId);
						foreach (var p in people)
							if (!downloadUsers.Contains(p.Id))
								downloadUsers.Add(p.Id);
					}
					break;
				case "Apc_Categories":
					{
						Datalayer dl = new Datalayer(_appServer);
						var people = dl.GetPeopleByCategoryId(m.SourceId);
						foreach (var p in people)
							if (!downloadUsers.Contains(p.Id))
								downloadUsers.Add(p.Id);
					}
					break;
				case "Apc_Departments":
					{
						Datalayer dl = new Datalayer(_appServer);
						var people = dl.GetPeopleByDepartmentId(m.SourceId);
						foreach (var p in people)
							if (!downloadUsers.Contains(p.Id))
								downloadUsers.Add(p.Id);
					}
					break;
				case "Variables":
					switch (m.Type)
					{
						case 0:	// create
						case 1:	// update
							if (!downloadVariables.Contains(m.SourceId))
								downloadVariables.Add(m.SourceId);
							break;
					}
					break;
				case "Timetables":
					switch (m.Type)
					{
						case 0:	// create
						case 1:	// update
							if (!downloadTimetables.Contains(m.SourceId))
								downloadTimetables.Add(m.SourceId);
							break;
					}
					break;
				case "Holidays":
					downloadHolidays = true;
					break;
			}
		}
		#endregion

		#region SendInfo()
		/// <summary>
		/// Sends an informational message or alarm to AppVision.
		/// </summary>
		/// <param name="severity">0 = informational event, &gt;0 = alarm.</param>
		/// <param name="info">Message text.</param>
		void SendInfo(int severity, string info)
		{
			Trace("SendInfo " + info);
			try
			{
				if (severity > 0)
					_appServer.AlarmManager.AddAlarm2(severity, "", info, "", "", "AppControl");
				else
					_appServer.EventManager.AddEvent("", info, "", "", "AppControl");
			}
			catch (Exception ex)
			{
				Error("SendInfo: " + ex.Message, false);
			}
		}
		#endregion
		#region SendVariable,SendVariableBadge
		/// <summary>Writes a scalar variable value to AppVision.</summary>
		void SendVariable(string varName, object v, string info, DateTimeOffset? date=null, int severity = -1)
		{
			try
			{
				Trace("To server: " + varName + "=" + v);
				_appServer.VariableManager.Set(varName, v, info, null, date ?? DateTimeOffset.MinValue, severity);
			}
			catch (Exception ex)
			{
				Error("VariableManager.Set: " + ex.Message, false);
			}
		}
		/// <summary>Writes a badge-read result to AppVision via <c>VariableManager.SetBadge</c>.</summary>
		void SendVariableBadge(string varName, ReaderStatus status, int personId, string badgeCode, DateTimeOffset date)
		{
			try
			{
				Trace("To server: " + varName + "=" + status);
				_appServer.VariableManager.SetBadge(varName, badgeCode, "", status, personId, date);
			}
			catch (Exception ex)
			{
				Error("VariableManager.SetBadge: " + ex.Message, false);
			}
		}
		#endregion

		#region GetDeviceByName(),GetDeviceByAddress
		/// <summary>Finds a device by its AppVision variable name.</summary>
		Device GetDeviceByName(string name)
		{
			return _devices.Where(v => v.Name == name).FirstOrDefault();
		}
		/// <summary>Finds a device by its network address.</summary>
		Device GetDeviceByAddress(string address)
		{
			return _devices.Where(v => v.Address == address).FirstOrDefault();
		}
		#endregion

		#region Trace(),Error(),Log()
		/// <summary>Logs a diagnostic message; optionally forwards it to AppVision as an event.</summary>
		void Trace(string text, bool sendToServer = false)
		{
			if (_appServer.IsConnected && sendToServer)
				SendInfo(0, text);
			if (_appServer.LogLevel > 0)
				Log(text);
			if (TraceDelegate != null)
				TraceDelegate(text);
		}
		/// <summary>Logs an error message; optionally raises an alarm in AppVision.</summary>
		void Error(string text, bool sendToServer = true)
		{
			if (_appServer.IsConnected && sendToServer)
				SendInfo(1, text);
			Log(text);
			if (ErrorDelegate != null)
				ErrorDelegate(text);
		}
		/// <summary>Writes a line to the AppVision rolling log file.</summary>
		void Log(string line)
		{
			_appServer.Log(line);
		}
		#endregion

		static Driver _this;
		string _serverHostname = "";
		string _protocolName;
		Thread _thread;
		volatile bool _serverStopWaiting;		// set by Stop() from other threads
		ConcurrentQueue<object> _events = new ConcurrentQueue<object>();
		AppServerForDriver _appServer;
		ProtocolRow _protocol
		{
			get { return _appServer.CurrentProtocol; }
		}
		int _tempoDownloadModifications = 5;		// minimum seconds between two incremental downloads
		List<Device> _devices = new List<Device>();	// devices managed by this driver instance
	}

	/// <summary>Event data for a full download request from AppVision.</summary>
	class DownloadInfo
	{
		/// <summary>Comma-separated controller variable names (e.g. <c>Access.C001</c>).</summary>
		internal string VarNames;
		/// <summary>Download type flags (e.g. <c>Holidays,Timetables,Rights</c>).</summary>
		internal string Parameter;
	}
	/// <summary>Command: perform a full download with the given parameter string.</summary>
	class CommandDownloadInfo
	{
		/// <summary>Download type flags forwarded from AppVision.</summary>
		public string Parameter;
	}
	/// <summary>Command: send an action (e.g. door open/close) to the device.</summary>
	class CommandAction
	{
		/// <summary>Composite name of the target input, e.g. <c>Door1.Cmd</c>.</summary>
		public string Name;
		/// <summary>Command value as a string.</summary>
		public string Value;
	}
	/// <summary>Command: perform an incremental download of selected data categories.</summary>
	class CommandDownloadList
	{
		/// <summary>Person IDs to re-download.</summary>
		public List<int> Users;
		/// <summary>Timetable IDs to re-download.</summary>
		public List<int> Timetables;
		/// <summary>Whether to re-download all holiday definitions.</summary>
		public bool Holidays;
		/// <summary>Whether to re-download device parameters.</summary>
		public bool Parameters;
	}
}
