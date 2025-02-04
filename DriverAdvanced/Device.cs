using System;
using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using Prysm.Logger;

namespace DriverAdvanced
{
	// $projectname$ device
	public partial class Device
	{
		Driver _driver;
		VariableRow _rootVar;

		public Device(Driver driver, VariableRow deviceVariable)
		{
			_driver = driver;
			_rootVar = deviceVariable;

			Trace.Info($"[{_rootVar.Name}] {_rootVar.Address?.HidePasswords()}");
			Trace.Info($"[{_rootVar.Name}] {_rootVar.FullParameters?.HidePasswords().Trim()}");

			// todo 
			// get $projectname$ device info

			//Uri adrs = Helper.ParseUri(deviceVariable.Address);
			//string param1 = Helper.GetParameter("Param1", deviceVariable.FullParameters, "Param1Default");
			//int param2 = Helper.GetParameter<int>("Param2", deviceVariable.FullParameters);
			// ...


			_driver.SetVariable(_rootVar.Name + ".Comm", (int)CommStatus.Unknown);
		}

		public void Start()
		{
			Trace.Info($"[{_rootVar.Name}] Start");
			_driver.AppServer.VariableManager.StateChanged += VariableChanged;
			_driver.AppServer.VariableManager.RowUpdated += variableUpdated;

			// todo 
			// connect to $projectname$ device
			// start monitoring

			// todo
			// when connected
			Trace.Info($"[{_rootVar.Name}] Connected");
			_driver.SetVariable(_rootVar.Name + ".Comm", (int)CommStatus.Connected);

			// todo
			// if connection is lost
			//Trace.Error("error message");
			//_driver.SetVariable(_rootVar.Name + ".Comm", (int)CommStatus.Fault);
		}

		public void Stop()
		{
			Trace.Info($"[{_rootVar.Name}] Stop");
			_driver.AppServer.VariableManager.StateChanged -= VariableChanged;
			_driver.AppServer.VariableManager.RowUpdated -= variableUpdated;

			// todo 
			// disconnect from $projectname$ device

			// todo
			// when disconnected
			Trace.Info($"[{_rootVar}] Disconnected");
			_driver.SetVariable(_rootVar.Name + ".Comm", (int)CommStatus.Disconnected);
		}


		void VariableChanged(VariableState state)
		{
			if (!state.Name.StartsWith(_rootVar.Name + ".")) // only for this device
				return;
			try
			{
				// todo 
				// transfert command from appvision to the external device
			}
			catch (Exception ex)
			{
				_driver.AddAlarm(ex, state.Row);
			}
		}

		// to be removed if not required
		void variableUpdated(VariableRow row)
		{
			if (!row.Name.StartsWith(_rootVar.Name + ".")) // only for this device
				return;

			Trace.Info($">app> update: {row.Name}");
			try
			{
				// todo 
				// reflect the variable changed in the external device
			}
			catch (Exception ex)
			{
				_driver.AddAlarm(ex, row);
			}
		}
	}
}
