using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

#pragma warning disable CS1998

namespace WpfControlAdvanced
{
	// Demo WPF control to be displayed in AppVision client
	public partial class UserControl1 : UserControl, IPersistable, ICommandable
	{
		public UserControl1()
		{
			InitializeComponent();
		}

		public UserControl1(params string[] args) : this()
		{
			Log("Constructor parameters: " + args.Join());
			// it is possible to pass parameters at 
			// ex:
			// Client.Load(0, "main", "WpfControlAdvanced.dll#WpfControlAdvanced.UserControl1", "arg1", "argN");
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (!Sdk.Server.IsConnected) 
				return;

			// At load time, retrieve some data from AppVision server:

			// configuration data
			projectName.Text = Sdk.Server.GetOption("App.Main.ProjectDescription");
			// server status
			serverVersion.Text = Sdk.Server.ControllerManager.ServerState.Version;
			// server address
			serverAdrs.Text = Sdk.Server.Hostname;
			// licence data
			serialNumber.Text = Sdk.Server.GetLicenseInfo("SN");
			// connected user
			userName.Text = Sdk.Server.UserManager.CurrentUser?.FullName;
			// parent container name
			container.Text = this.GetAncestor<IContainer>()?.Name;
			// list of synoptics
			synoptics.ItemsSource = from fileinfo in await Sdk.Server.FileInfoManager.GetRows()
									where fileinfo.Type == FileInfoType.Synoptic
									select fileinfo;
			// list of top level areas
			areaTree.ItemsSource = Sdk.Server.AreaManager.GetRows().Where(area => area.TreeLevel == 0);
			// list of variables (except nodes)
			variableList.ItemsSource = Sdk.Server.VariableManager.GetStates();

			// Subscribe to events and changes of variable value
			Sdk.Server.VariableManager.StateChanged += VariableManager_StateChanged;
			Sdk.Server.AlarmManager.AlarmChanged += AlarmManager_AlarmChanged;
			Sdk.Server.EventManager.EventChanged += EventManager_EventChanged;
		}

		// Release subscriptions
		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			Sdk.Server.VariableManager.StateChanged -= VariableManager_StateChanged;
			Sdk.Server.AlarmManager.AlarmChanged -= AlarmManager_AlarmChanged;
			Sdk.Server.EventManager.EventChanged -= EventManager_EventChanged;
		}

		// This callback is invoked for each server event
		private void EventManager_EventChanged(EventRow @event) => Log(@event);

		// This callback is invoked everytime the status of an alarm changes
		private void AlarmManager_AlarmChanged(AlarmInfo alarm) => Log(alarm);

		// This callback is invoked everytime the value of a variable changes
		private void VariableManager_StateChanged(VariableState state) => Log(state);

		void Log(object obj)
		{
			logs.Items.Insert(0, obj);

			if (logs.Items.Count > 15) // rolling log
				logs.Items.RemoveAt(15);
		}


		// Open the selected synoptic in a new window
		private void OpenSynoptic(object sender, RoutedEventArgs e)
		{
			var filename = (string)synoptics.SelectedValue;
			if (filename != null)
			{
				Sdk.Client.Load(filename);
			}
		}

		// Raise and acknowledge an alarm in AppVision
		private async void TriggerAlarm(object sender, RoutedEventArgs e)
		{
			alarmId = await Sdk.Server.AlarmManager.AddAlarm2(99, "My alarm", "Triggered from " + nameof(UserControl1));
		}

		private void AcknowledgeAlarm(object sender, RoutedEventArgs e)
		{
			Sdk.Server.AlarmManager.AcknowledgeAlarm(alarmId);
		}

		long alarmId;


		// Get and set variable value
		private void VariableList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var variable = (VariableState)variableList.SelectedItem;

			variableValue.Text = variable?.Value?.ToStringInvariant(); // doubles are formated using invariant culture
		}

		private void SetVariableValue(object sender, RoutedEventArgs e)
		{
			var varName = (string)variableList.SelectedValue;
			if (varName != null)
			{
				Sdk.Server.VariableManager.Set(varName, variableValue.Text);
			}
		}

		#region IPersistable

		// By implementing the IPersistable inteface, this control can save and restore its internal state in AppVision workspaces.

		public string[] DocumentArgs => new[]
		{
			tabControl.SelectedIndex.ToString(),
			variableList.SelectedValue?.ToString()
		};

		public string DocumentName => "WpfControlAdvanced.dll#WpfControlAdvanced.UserControl1";

		public XElement GetEnv()
		{
			return new XElement("document",
				new XAttribute("name", DocumentName),
				new XElement("arg", "tabIndex=" + tabControl.SelectedIndex),
				new XElement("arg", "selectedVariable=" + variableList.SelectedValue));
		}

		public async Task SetEnv(XElement environment)
		{
			foreach (var arg in environment.Elements("arg"))
			{
				if (arg.Value.StartsWith("tabIndex=")) tabControl.SelectedIndex = arg.Value.Substring("tabIndex=".Length).ToInt();
				if (arg.Value.StartsWith("selectedVariable=")) variableList.SelectedValue = arg.Value.Substring("selectedVariable=".Length);
			}
		}

		#endregion

		#region ICommandable

		// By implementing the ICommandable interface, this control can be remotely controled

		public async Task<IEnumerable<RemoteCommand>> GetCommands() => new[]
		{
			// action example: display a message dialog
			new RemoteCommand("About WpfControlAdvanced...", "msg", "Hello from WpfControlAdvanced!"), 
			// submenu example: toggle check mark
			new RemoteCommand("Sub menu", new []
			{
				new RemoteCommand("item 1", "check1", isCheckable: true, isChecked:item1Checked),
				new RemoteCommand("item 2", "check2", isCheckable: true, isChecked:!item1Checked),
			})
		};

		public void ExecuteCommand(string command, string parameters)
		{
			switch (command)
			{
				case "msg":
					MessageBox.Show(parameters, "ICommandable demo");
					break;
				case "check1": item1Checked = true; break;
				case "check2": item1Checked = false; break;
			}
		}
		bool item1Checked = true;

		public async Task InitMenu(ItemsControl menu) { } // obsolete

		#endregion

		// This function will be called by its AppVision host to demonstrate how this component can be controlled from AppVision client
		public string SampleFunction(int arg1)
		{
			int tabId = (tabControl.SelectedIndex + 1) % 4;
			tabControl.SelectedIndex = tabId;
			return "dummy return value";
		}
	}
}
