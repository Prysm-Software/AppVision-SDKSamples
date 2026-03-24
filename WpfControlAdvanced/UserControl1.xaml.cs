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
	/// <summary>
	/// Advanced embeddable synoptic control demonstrating:
	/// <list type="bullet">
	///   <item><description><see cref="IPersistable"/> — save/restore control state in AppVision workspaces</description></item>
	///   <item><description><see cref="ICommandable"/> — expose actions to AppVision context menus</description></item>
	///   <item><description>Async data loading (synoptics, areas, variables, licence info)</description></item>
	///   <item><description>Variable read/write, alarm raise/acknowledge and event subscription</description></item>
	/// </list>
	/// </summary>
	public partial class UserControl1 : UserControl, IPersistable, ICommandable
	{
		/// <summary>Initialises the control.</summary>
		public UserControl1()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Parameterised constructor called by AppVision when a control URL contains arguments.
		/// </summary>
		/// <param name="args">
		/// Optional key=value strings passed via
		/// <c>Client.Load(0, "main", "WpfControlAdvanced.dll#...", "arg1", "argN")</c>.
		/// </param>
		public UserControl1(params string[] args) : this()
		{
			Log("Constructor parameters: " + args.Join());
			// it is possible to pass parameters at 
			// ex:
			// Client.Load(0, "main", "WpfControlAdvanced.dll#WpfControlAdvanced.UserControl1", "arg1", "argN");
		}

		/// <summary>
		/// Populates all UI panels with live server data once the control is visible.
		/// Runs asynchronously to avoid blocking the UI thread during network calls.
		/// </summary>
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

		/// <summary>
		/// Unsubscribes from all server events when the control is removed from the visual tree.
		/// Always unsubscribe here to prevent memory leaks and callbacks after disposal.
		/// </summary>
		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			Sdk.Server.VariableManager.StateChanged -= VariableManager_StateChanged;
			Sdk.Server.AlarmManager.AlarmChanged -= AlarmManager_AlarmChanged;
			Sdk.Server.EventManager.EventChanged -= EventManager_EventChanged;
		}

		/// <summary>Called when a new event is added to the AppVision event log.</summary>
		private void EventManager_EventChanged(EventRow @event) => Log(@event);

		/// <summary>Called when the status of an alarm changes.</summary>
		private void AlarmManager_AlarmChanged(AlarmInfo alarm) => Log(alarm);

		/// <summary>Called when the value of any subscribed variable changes.</summary>
		private void VariableManager_StateChanged(VariableState state) => Log(state);

		void Log(object obj)
		{
			logs.Items.Insert(0, obj);

			if (logs.Items.Count > 15) // rolling log
				logs.Items.RemoveAt(15);
		}


		/// <summary>
		/// Opens the selected synoptic file in a new AppVision window using
		/// <see cref="Prysm.AppVision.SDK.IClient.Load(string)"/>.
		/// </summary>
		private void OpenSynoptic(object sender, RoutedEventArgs e)
		{
			var filename = (string)synoptics.SelectedValue;
			if (filename != null)
			{
				Sdk.Client.Load(filename);
			}
		}

		/// <summary>
		/// Raises a numbered alarm in AppVision and stores its ID for later acknowledgement.
		/// </summary>
		private async void TriggerAlarm(object sender, RoutedEventArgs e)
		{
			alarmId = await Sdk.Server.AlarmManager.AddAlarm2(99, "My alarm", "Triggered from " + nameof(UserControl1));
		}

		/// <summary>Acknowledges the alarm that was raised by <see cref="TriggerAlarm"/>.</summary>
		private void AcknowledgeAlarm(object sender, RoutedEventArgs e)
		{
			Sdk.Server.AlarmManager.AcknowledgeAlarm(alarmId);
		}

		long alarmId;


		/// <summary>Reads and displays the value of the variable selected in the list.</summary>
		private void VariableList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var variable = (VariableState)variableList.SelectedItem;
			// ToStringInvariant() ensures doubles use '.' as decimal separator regardless of locale.
			variableValue.Text = variable?.Value?.ToStringInvariant();
		}

		/// <summary>Writes the text box value to the selected variable in AppVision.</summary>
		private void SetVariableValue(object sender, RoutedEventArgs e)
		{
			var varName = (string)variableList.SelectedValue;
			if (varName != null)
			{
				Sdk.Server.VariableManager.Set(varName, variableValue.Text);
			}
		}

		#region IPersistable

		// By implementing IPersistable, this control can save and restore its internal state
		// in AppVision workspaces. AppVision calls GetEnv() when saving a workspace and
		// SetEnv() when restoring it.

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

		// By implementing ICommandable, this control exposes named actions to AppVision's
		// right-click context menu and to remote script automation.

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
