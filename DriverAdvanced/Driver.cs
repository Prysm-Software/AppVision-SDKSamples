using Prysm.AppVision.SDK;
using Prysm.AppVision.Data;
using Prysm.AppVision.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Prysm.AppVision.Common.Logger;

namespace DriverAdvanced
{
	public partial class Driver
	{
		internal AppServerForDriver AppServer; // unique instance of AppVision server
		List<Device> _devices = new List<Device>();
		ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

		public Driver()
		{
			Trace.Info(Environment.CommandLine);

			AppServer = new AppServerForDriver();
			AppServer.VariableManager.StateChanged += s => Trace.Info($">app> {s.Name}={s.Value}");
			AppServer.Restart += () => { Stop(); Start(); };
			AppServer.Download += Download;
			AppServer.Refresh += Refresh;
			Trace.Logger.LogEvent += it => AppServer.Log((int)it.Level, it.Message);
		}

		public void Start()
		{
			try
			{
				// command line: protocolName[@hostname]
				var args = Environment.GetCommandLineArgs();
				var a = args[1].Split('@');
				var protocolName = a[0];
				var hostName = (a.Length > 1) ? a[1] : "";

				// connect to AppVision server
				Trace.Info($"Connecting to server {hostName}");
				AppServer.Open(hostName);
				AppServer.Login(protocolName);

                // todo
                // get protocol settings

                //IPEndPoint host = Helper.ParseEndPoint(AppServer.CurrentProtocol.Address, 80);
                //int timeout = AppServer.CurrentProtocol.Timeout;
                //int retry = AppServer.CurrentProtocol.MaxRetry;
                //string param1 = Helper.GetParameter("Param1", AppServer.CurrentProtocol.Parameters, "Param1Default");
                //int param2 = Helper.GetParameter<int>("Param2", AppServer.CurrentProtocol.Parameters);
                // ...

                var deviceVars = AppServer.GetVariablesByProtocol();
				if (deviceVars.Length == 0)
					Trace.Info("This protocol has no device");

				// register for server notifications 
				var filter = deviceVars.Select(v => "$V." + v.Name + ".*").Join("|"); // todo adjust filter as required
				AppServer.AddFilterNotifications(filter);
				AppServer.AddFilterNotifications("Variable.StateType=1"); // only varaible with "command" flag checked
				AppServer.StartNotifications(false);
				AppServer.ProtocolSynchronized();
				Trace.Info($"Protocol {protocolName} synchronized");

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
						AddAlarm(ex, var);
					}
				}

				// display some info the the bottom status bar. Ex:
				Trace.StatusText = _devices.Count + " device(s) running";
			}
			catch (Exception ex)
			{
				//AddAlarm(ex);	// driver keeps running
				throw;			// driver will close in 5s
			}
		}

		public void Stop()
		{
			Trace.Info("Stopping");
			try
			{
				foreach (var device in _devices)
					device.Stop();

				_devices.Clear();

				if (AppServer.IsConnected)
					AppServer.Close();

				Trace.Info("Stopped");
			}
			catch (Exception ex)
			{
				Trace.Error(ex);
			}
		}

		// to be removed if not required
		internal void Download(string command, string parameters)
		{
			Trace.Info("Download");
			try
			{
				// todo 
				// download data to controller (AppControl only)
			}
			catch (Exception ex)
			{
				AddAlarm(ex);
			}
		}

		// to be removed if not required
		public void Refresh(string message)
		{
			Trace.Info("Refresh");
			try
			{
				// todo
				// implement Refresh command
			}
			catch (Exception ex)
			{
				AddAlarm(ex);
			}
		}

		internal void AddAlarm(Exception ex, VariableRow row = null)
		{
			try
			{
// to be removed if not required
#if MAX_ALARMS_COUNT_PER_MIN 
				// limit alarms per minute to prevent server overflow
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
				AppServer.AlarmManager.AddAlarmExt(10, ex.Message, row?.FullDescription, ex.ToString(), row?.AreaDescription, row?.GroupsDescription);
			}
			catch { }
		}
		DateTime _alrDate;
		int _alrCount;


		internal void SetVariableNoCache(string name, object value, string info = null, string operation = null)
		{
			try
			{
				Trace.Info($"<drv< {name}={value}	{info}");
				AppServer.VariableManager.Set(name, value, info, operation);
			}
			catch { }
		}
		internal void SetVariable(string name, object value, string info = null, string operation = null)
		{
			// do not send identical values. 
			object val;
			if (_cache.TryGetValue(name, out val))
				if (value?.Equals(val) == true)
					return;

			_cache[name] = value;
			SetVariableNoCache(name, value, info, operation);
		}
	}
}