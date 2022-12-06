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
	class Driver
	{
		#region public events & properties
		internal static AppServer Server
		{
			get { return _this._appServer; }
		}
		internal static string GetHostname()
		{
			return _this._appServer.Hostname;
		}

		internal bool IsAlive
		{
			get
			{
				if (_thread == null)
					return false;
				return _thread.IsAlive;
			}
		}
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
		internal void Start()
		{
			_thread = new Thread(new ThreadStart(Main));
			_thread.Start();
		}
		internal void Stop()
		{
			_serverStopWaiting = true;
		}
		#endregion

		#region Main()
		void Main()
		{
			try
			{
				// 1. initialisation
				Trace("Server connection ...");
				_appServer.Open(_serverHostname);
				_appServer.Login(_protocolName);
				if (_appServer.CurrentProtocol == null)
				{ Trace("No protocol " + _protocolName); return; }
				Environment.CurrentDirectory = _appServer.GetPathParam();

				// to get output command from the server
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
				_serverStopWaiting = false;
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
						DownloadModifications();
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
				Trace(ex.ToString());
			}
			finally
			{
				#region 3. finalisation
				try
				{
					if (_appServer.IsConnected)
					{
						_appServer.Logout();
						_appServer.Close();
					}
				}
				catch { }

				// check restart
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
		bool IsAllDevicesClosed()
		{
			foreach (var d in _devices)
				if (d.IsOpened)
					return false;
			return true;
		}
		#endregion

		#region appServer_Restart,appServer_Closed,appServer_NewModification
		void appServer_Restart()
		{
			_restart = true;
			Stop();
		}
		bool _restart = false;
		void appServer_Closed()
		{
			Stop();
		}


		void appServer_NewModification()
		{
			_newModification = true;
		}
		bool _newModification = true;

		#endregion
		#region appServer_VariableStateChanged()
		void appServer_VariableStateChanged(VariableState obj)
		{
			_events.Enqueue(obj);
		}
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
		void appServer_Download(string varNames, string parameter)
		{
			_events.Enqueue(new DownloadInfo { VarNames = varNames, Parameter = parameter });
		}
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
		void device_ConnectChanged(object sender, ConnectChangedEventArgs e)
		{
			_events.Enqueue(e);
		}
		void OnConnectChanged(ConnectChangedEventArgs e)
		{
			// send variable state with variable .Comm
			string varName = e.DeviceName + ".Comm";
			int comm = (int)e.Connected;
			SendVariable(varName, comm, e.Info);
		}
		#endregion
		#region device_InputChanged()
		void device_InputChanged(object sender, InputChangedEventArgs e)
		{
			_events.Enqueue(e);
		}
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
		void device_AccessControlEvent(object sender, AccessControlEventArgs e)
		{
			_events.Enqueue(e);
		}
		private void OnAccessControlEvent(AccessControlEventArgs e)
		{
			string varName = string.Format("{0}.Reader1", e.DeviceName);
			SendVariableBadge(varName, e.Status, e.UserId, e.BadgeId, e.DateUtc);
		}
		#endregion
		#region device_TraceEvent()
		void device_TraceEvent(object sender, TraceEventArgs e)
		{
			_events.Enqueue(e);
		}
		void OnTraceEvent(TraceEventArgs e)
		{
			string varName = e.DeviceName + ".Trace";
			SendVariable(varName, e.Text, "", null, e.Severity);
		}
		#endregion

		#region DownloadModifications()
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
		Device GetDeviceByName(string name)
		{
			return _devices.Where(v => v.Name == name).FirstOrDefault();
		}
		Device GetDeviceByAddress(string address)
		{
			return _devices.Where(v => v.Address == address).FirstOrDefault();
		}
		#endregion

		#region Trace(),Error(),Log()
		void Trace(string text, bool sendToServer = false)
		{
			if (_appServer.IsConnected && sendToServer)
				SendInfo(0, text);
			if (_appServer.LogLevel > 0)
				Log(text);
			if (TraceDelegate != null)
				TraceDelegate(text);
		}
		void Error(string text, bool sendToServer = true)
		{
			if (_appServer.IsConnected && sendToServer)
				SendInfo(1, text);
			Log(text);
			if (ErrorDelegate != null)
				ErrorDelegate(text);
		}
		void Log(string line)
		{
			_appServer.Log(line);
		}
		#endregion

		static Driver _this;
		string _serverHostname = "";
		string _protocolName;
		Thread _thread;
		bool _serverStopWaiting;
		ConcurrentQueue<object> _events = new ConcurrentQueue<object>();
		AppServerForDriver _appServer;
		ProtocolRow _protocol
		{
			get { return _appServer.CurrentProtocol; }
		}
		int _tempoDownloadModifications = 5;		// in second
		List<Device> _devices = new List<Device>();	// devices list attached with this protocol
	}

	class DownloadInfo
	{
		internal string VarNames;
		internal string Parameter;
	}
	class CommandDownloadInfo
	{
		public string Parameter;
	}
	class CommandAction
	{
		public string Name;
		public string Value;
	}
	class CommandDownloadList
	{
		public List<int> Users;
		public List<int> Timetables;
		public bool Holidays;
		public bool Parameters;
	}
}
