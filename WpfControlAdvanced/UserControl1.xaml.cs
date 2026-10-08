using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace WpfControlAdvanced
{
	/// <summary>
	/// Advanced embeddable synoptic control demonstrating:
	/// <list type="bullet">
	///   <item><description><see cref="IPersistable"/> — save/restore the control state in AppVision workspaces</description></item>
	///   <item><description><see cref="ICommandable"/> — add actions to the menu of the AppVision container</description></item>
	///   <item><description>Async data loading (synoptics, areas, variables, licence info)</description></item>
	///   <item><description>Variable read/write, alarm raise/acknowledge and event subscription</description></item>
	/// </list>
	/// </summary>
	public partial class UserControl1 : UserControl, IPersistable, ICommandable
	{
		/// <summary>Initialises the control (used when the control is placed on a synoptic).</summary>
		public UserControl1()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Parameterised constructor called by the AppVision client when the control is loaded as a document with arguments:
		/// <c>Client.Load(0, "main", "WpfControlAdvanced.dll#WpfControlAdvanced.UserControl1", "tabIndex=2")</c>.
		/// It is also how the client restores the control from a workspace: it calls this constructor
		/// with the arguments saved by <see cref="GetEnv"/>.
		/// </summary>
		/// <param name="args">Document arguments, here "name=value" strings.</param>
		public UserControl1(params string[] args) : this()
		{
			Log("Constructor parameters: " + args.Join());
			ApplyArgs(args);
		}

		/// <summary>
		/// Populates all UI panels with live server data once the control is visible.
		/// Runs asynchronously to avoid blocking the UI thread during network calls.
		/// </summary>
		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			// no server in the Visual Studio designer or in the configurator
			if (!Sdk.Server.IsConnected)
				return;

			// Subscribe to events and changes of variable value (balanced by UserControl_Unloaded)
			Sdk.Server.VariableManager.StateChanged += VariableManager_StateChanged;
			Sdk.Server.AlarmManager.AlarmChanged += AlarmManager_AlarmChanged;
			Sdk.Server.EventManager.EventChanged += EventManager_EventChanged;

			try
			{
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
				// list of synoptics (server request, awaited without blocking the UI)
				synoptics.ItemsSource = from fileinfo in await Sdk.Server.FileInfoManager.GetRows()
										where fileinfo.Type == FileInfoType.Synoptic
										orderby fileinfo.Name
										select fileinfo;
				// list of top level areas (client cache)
				areaTree.ItemsSource = Sdk.Server.AreaManager.GetRows().Where(area => area.TreeLevel == 0);
				// list of variables, except nodes (client cache)
				variableList.ItemsSource = Sdk.Server.VariableManager.GetStates()
					.Where(state => state.Row?.Type != VariableType.Node)
					.OrderBy(state => state.Name)
					.ToList();

				// a variable selected by the constructor arguments or a restored workspace
				if (_pendingVariable != null)
				{
					variableList.SelectedValue = _pendingVariable;
					_pendingVariable = null;
				}
			}
			catch (Exception ex)
			{
				Log("Loading failed: " + ex.Message);
			}
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

		/// <summary>
		/// Adds an entry on top of the rolling log.
		/// The AppVision client normally raises the SDK events on the UI thread, but not in every
		/// configuration (notifications received through a web service port): marshal when needed.
		/// </summary>
		void Log(object obj)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.InvokeAsync(() => Log(obj));
				return;
			}

			logs.Items.Insert(0, obj);

			if (logs.Items.Count > 15) // rolling log
				logs.Items.RemoveAt(15);
		}


		/// <summary>
		/// Opens the selected synoptic in the main container of the current window using
		/// <see cref="Prysm.AppVision.SDK.IClient.Load(string, string[])"/>.
		/// </summary>
		private void OpenSynoptic(object sender, RoutedEventArgs e)
		{
			var filename = (string)synoptics.SelectedValue;
			if (filename != null)
			{
				// Sdk.Client is null when the control is not hosted by the AppVision client
				Sdk.Client?.Load(filename);
			}
		}

		/// <summary>
		/// Raises an alarm (severity 99) in AppVision and stores its ID for later acknowledgement.
		/// </summary>
		private async void TriggerAlarm(object sender, RoutedEventArgs e)
		{
			try
			{
				// AddAlarm2(severity, state, description, info, area, groups, ...) returns the alarm id
				alarmId = await Sdk.Server.AlarmManager.AddAlarm2(99, "My alarm", "Triggered from " + nameof(UserControl1));
			}
			catch (Exception ex)
			{
				Log("AddAlarm2 failed: " + ex.Message);
			}
		}

		/// <summary>Acknowledges the alarm that was raised by <see cref="TriggerAlarm"/>.</summary>
		private async void AcknowledgeAlarm(object sender, RoutedEventArgs e)
		{
			if (alarmId == 0)
				return;
			try
			{
				if (!await Sdk.Server.AlarmManager.AcknowledgeAlarm(alarmId))
					Log("Alarm " + alarmId + " not acknowledged (already acknowledged, ended, or not allowed)");
			}
			catch (Exception ex)
			{
				Log("AcknowledgeAlarm failed: " + ex.Message);
			}
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
				try
				{
					Sdk.Server.VariableManager.Set(varName, variableValue.Text);
				}
				catch (Exception ex)
				{
					Log("Set failed: " + ex.Message);
				}
			}
		}

		#region IPersistable

		// IPersistable is used when the control is the document of an AppVision container, i.e. loaded with
		// Client.Load(..., "WpfControlAdvanced.dll#WpfControlAdvanced.UserControl1"), not when it is placed on a synoptic.
		// When a workspace is saved, the container stores GetEnv(): <document name="DocumentName"><arg>...</arg></document>.
		// When it is restored, the container loads DocumentName again and passes the <arg> values to the
		// 'params string[]' constructor. SetEnv() is kept consistent for hosts that call it directly.

		/// <summary>Name of the document: what the container must load to recreate this control.</summary>
		public string DocumentName => "WpfControlAdvanced.dll#WpfControlAdvanced.UserControl1";

		/// <summary>Current state of the control as document arguments ("name=value").</summary>
		public string[] DocumentArgs => new[]
		{
			"tabIndex=" + tabControl.SelectedIndex,
			"selectedVariable=" + variableList.SelectedValue,
		};

		/// <summary>Serializes the state of the control for a workspace.</summary>
		public XElement GetEnv()
		{
			return new XElement("document",
				new XAttribute("name", DocumentName),
				DocumentArgs.Select(arg => new XElement("arg", arg)));
		}

		/// <summary>Restores a state serialized by <see cref="GetEnv"/>.</summary>
		public Task SetEnv(XElement environment)
		{
			ApplyArgs(environment?.Elements("arg").Select(arg => arg.Value).ToArray());
			return Task.CompletedTask;
		}

		/// <summary>Applies "tabIndex=" and "selectedVariable=" arguments.</summary>
		void ApplyArgs(string[] args)
		{
			var tabIndex = Helper.GetParameter("tabIndex", args);
			if (!string.IsNullOrEmpty(tabIndex))
				tabControl.SelectedIndex = tabIndex.ToInt();

			var variable = Helper.GetParameter("selectedVariable", args);
			if (!string.IsNullOrEmpty(variable))
			{
				if (variableList.ItemsSource != null)
					variableList.SelectedValue = variable;
				else
					_pendingVariable = variable; // applied when the list is loaded
			}
		}
		string _pendingVariable;

		#endregion

		#region ICommandable

		// By implementing ICommandable, this control adds its own actions to the menu of the AppVision container
		// that displays it (the menu button at the top right corner of the container).

		public Task<IEnumerable<RemoteCommand>> GetCommands() => Task.FromResult<IEnumerable<RemoteCommand>>(new[]
		{
			// action example: display a message dialog
			new RemoteCommand("About WpfControlAdvanced...", "msg", "Hello from WpfControlAdvanced!"),
			// submenu example: toggle check mark
			new RemoteCommand("Sub menu", new []
			{
				new RemoteCommand("item 1", "check1", isCheckable: true, isChecked: item1Checked),
				new RemoteCommand("item 2", "check2", isCheckable: true, isChecked: !item1Checked),
			})
		});

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

		#endregion

		/// <summary>
		/// Public method called by a script of the synoptic hosting this control (see the "Direct method invoke"
		/// button of the sample configuration): <c>UserControl1.SampleFunction(999);</c>
		/// The control is reachable from the synoptic scripts through its name (x:Name) in the synoptic.
		/// </summary>
		public string SampleFunction(int arg1)
		{
			int tabId = (tabControl.SelectedIndex + 1) % tabControl.Items.Count;
			tabControl.SelectedIndex = tabId;
			return "dummy return value";
		}
	}
}
