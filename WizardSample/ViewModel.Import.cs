using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WizardSample
{
	public partial class ViewModel : INotifyPropertyChanged
	{
		/// <summary>Returns <see langword="true"/> while the import has not yet committed its database changes.</summary>
		bool CanCancelImport() => _canCancel;
		/// <summary>Sets the cancellation flag; the import loop checks it between entity iterations.</summary>
		void CancelImport() => _canceling = true;

		bool _canceling, _canCancel = true;

		/// <summary>
		/// Performs the full import on a background thread:
		/// creates the protocol row, optionally creates a variable group,
		/// then iterates <see cref="SelectedEntities"/> to build the corresponding AppVision
		/// variables before saving them all in a single batch call.
		/// </summary>
		/// <exception cref="OperationCanceledException">Thrown when the user cancels the import.</exception>
		/// <exception cref="Exception">Thrown when the variable quota is exceeded.</exception>
		async Task Import()
		{
			// the wizard can import several devices in a row ("Import new device"): reset the state of the previous import
			_canceling = false;
			_canCancel = true;
			Success = true;
			Progress = 0;

			// leave UI thread
			await Task.Delay(1).ConfigureAwait(false);

			var protocolName = CreateProtocol();

			if (CreateGroup)
				GetOrCreateGroup(NodeName, NodeDescription);

			#region create variables

			var newVariables = new List<VariableRow>();
			var vargroups = new Dictionary<string, string>(); // key: variableName, value: groupName

			VariableRow varDevice = appServer.VariableManager.GetRowByName(NodeName);
			if (varDevice != null)
			{
				// this code runs on a background thread: show the dialog on the UI thread, owned by the wizard window
				var answer = Application.Current.Dispatcher.Invoke(() => MessageBox.Show(Application.Current.MainWindow,
					NodeName + " already exists.\r\nDo you want to add the entities to this device?", "Confirmation", MessageBoxButton.YesNo));
				if (answer != MessageBoxResult.Yes)
					throw new OperationCanceledException("Import was canceled by user.");
			}
			else
			{
				Log("Create device: " + NodeName);
				varDevice = new VariableRow
				{
					Name = NodeName,
					Description = NodeDescription,
					Type = VariableType.Node,
					Expression = "$P." + protocolName,  // link to the protocol: the driver receives this node and its children
					Address = $"{DeviceUsername}:{DevicePassword}@{DeviceHostname}",
					// <todo> add custom parameter as needed
					//Parameters = "name=value",
				};
				newVariables.Add(varDevice);

				// add root device variable to 
				if (CreateGroup)
					vargroups.Add(varDevice.Name, varDevice.Name);

				var comm = new VariableRow
				{
					Name = varDevice.Name + ".Comm",
					Description = "Communication",
					Type = VariableType.Enum,
					IsAlarm = true,
					StoreValue = true,
					StateDescs = "0:Unknown\n1:Connected:d\n2:Disconnected\n3:Default:a",
					AlarmSeverity = 20,
				};
				newVariables.Add(comm);

				// add the .Comm variable to the Communication group if present
				if (appServer.GroupManager.GetRowByName("Communication") != null)
					vargroups.Add(comm.Name, "Communication");
			}
			#endregion

			var inc = 1.0 / SelectedEntities.Count;

			// create variables for each entities
			foreach (var entity in SelectedEntities)
			{
				Progress += inc;

				if (_canceling)
					throw new OperationCanceledException("Import was canceled by user.");

				var entityName = varDevice.Name + "." + entity.Name.ToAlphaNum("_");

				VariableRow varEntity = appServer.VariableManager.GetRowByName(entityName);
				if (varEntity != null)
				{
					// <todo> update or ignore entities already in the configuration
					Log("Entity already exists: " + entityName);
					continue;
				}

				#region create entity variables
				Log("Create entity: " + entityName);

				// entity node
				varEntity = new VariableRow
				{
					Name = entityName,
					Description = entity.Description,
					Type = VariableType.Node,
					Symbol = EntitySymbol,
				};
				newVariables.Add(varEntity);

				// sample status variable
				newVariables.Add(new VariableRow
				{
					Name = varEntity.Name + ".State",
					Description = "status",
					Type = VariableType.Enum,
					IsAlarm = true,
					StoreValue = true,
					StateDescs = "0:Normal:d\n1:Alarm:a\n2:Tamper:a",
				});

				// sample command variable
				newVariables.Add(new VariableRow
				{
					Name = varEntity.Name + ".Cmd",
					Description = "Command",
					Type = VariableType.Bool,
					StateDescs = "0:Off\n1:On",
					StateType = 1,  // check flag 'Command'
				});

				/* <todo> add more variables if needed 

				// sample video variable
				newVariables.Add(new VariableRow
				{
					Name = varEntity.Name + ".Video",
					Type = VariableType.Video,
				});

				*/

				#endregion
			}

			// <todo> create other type of entities
			// ex: areas, synoptics...


			// beyond this point, canceling import is no longer possible
			_canCancel = false;
			InvokeUI(CommandManager.InvalidateRequerySuggested); // must run on the UI thread to refresh the Cancel button


			Log("Saving variables...");
			if (!appServer.VariableManager.AddVariables(newVariables.ToArray()))
				throw new Exception($"Your license does not allow to create {newVariables.Count} variables.");

			// variables groups relationship
			foreach (var vargroup in vargroups)
				appServer.VariableManager.AddVariableGroup(vargroup.Key, vargroup.Value);

			Log(newVariables.Count + " variables created");

			if (newVariables.Count > 1000)
				Log("WARNING: server is creating variables, it might take several minutes, check with configurator tool.");

			Message = "Import completed.";
		}


		/// <summary>
		/// Ensures the AppVision protocol row exists; creates it (with icon upload) if it does not.
		/// The protocol name is sanitised to alphanumeric characters before use.
		/// </summary>
		/// <returns>The protocol name actually used (sanitised).</returns>
		private string CreateProtocol()
		{
			// safe protocol name
			var protocolName = ProtocolName.ToAlphaNum();

			var protocol = appServer.ProtocolManager.GetRowByName(protocolName);
			if (protocol == null)
			{
				Log("Create protocol: " + protocolName);

				var iconPath = $"Images/{protocolName}.png";
				try
				{
					var icon = Application.GetResourceStream(new Uri("/Resources/logo.png", UriKind.Relative));
					appServer.FileManager.PutFile("Param/Client/" + iconPath, icon.Stream).Wait();
				}
				catch (Exception ex)
				{
					Log("Failed to upload protocol icon: " + ex.Message);
					iconPath = null;
				}

				protocol = new ProtocolRow
				{
					Name = protocolName,
					Description = "Protocol " + ProtocolName,
					// path of the driver executable, relative to the AppVision 'Bin' folder of the server,
					// e.g. $@"Drivers\{protocolName}\appDriver{protocolName}.exe" if the driver has its own sub-folder
					Executable = $"appDriver{protocolName}.exe",
					Icon = iconPath
				};
				if (!appServer.ProtocolManager.AddProtocol(protocol))
					throw new Exception("Failed to create protocol " + protocolName);
			}
			return protocolName;
		}

		/// <summary>
		/// Returns the variable group matching <paramref name="groupName"/>, creating it first if it does not exist.
		/// </summary>
		/// <param name="groupName">Unique name for the group (also used as the AppVision group identifier).</param>
		/// <param name="groupDescription">Human-readable description stored with the group.</param>
		/// <returns>The existing or newly created <see cref="GroupRow"/>.</returns>
		private GroupRow GetOrCreateGroup(string groupName, string groupDescription)
        {
            var group = appServer.GroupManager.GetRowByName(groupName);
            if (group == null)
            {
                Log("Create group " + groupName);
                group = new GroupRow
                {
                    Name = groupName,
                    Description = groupDescription
                };
                appServer.GroupManager.AddGroup(group);
            }
            return group;
        }
    }
}
