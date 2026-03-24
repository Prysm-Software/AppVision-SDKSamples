using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System;
using System.Windows;

namespace ClientSimple
{
	/// <summary>
	/// Beginner client application that connects to AppVision and subscribes
	/// to all variable, alarm and event notifications.
	/// Also demonstrates how to read and write variable values.
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>AppVision client connection handle — manages server communication.</summary>
		AppServer _server;

		public MainWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Connects to AppVision, loads the configuration database and starts
		/// receiving notifications for all variables, alarms and events.
		/// </summary>
		async void ButtonConnect_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// AppServer is one of the main classes of the SDK. It handles all communications with the server
				_server = new AppServer();
				_server.AlarmManager.AlarmChanged += AlarmManager_AlarmChanged;
				_server.VariableManager.StateChanged += VariableManager_StateChanged;
				_server.EventManager.EventChanged += EventManager_EventChanged;

				// Connect to the server using a name or IP address with an optional port number. ex: http://localhost:80
				Trace("Server.Open...");
				_server.Open(txtHost.Text); 

				// Log into the server. Default user/password is "ADM", "123"
				Trace("Server.Login...");
				_server.Login(txtUser.Text, txtPwd.Text);

				// Populate a local cache with the configuration database
				Trace("Server.LoadDatabase...");
				await _server.LoadDatabase(true);

				Trace("Server.StartNotifications...");
				// Subscribe to notifications. Use a filter string to narrow scope:
				//   "$V.CR1.*"              — all variables under CR1
				//   "$A.*"                  — all alarm changes
				//   "NotificationType=VariableState" — only variable changes
				//   "All"                   — everything (used here for the demo)
				_server.AddFilterNotifications("All");
				_server.StartNotifications(false, false);
				Trace("Connected");

				// Retrieve the status of all variables
				cbxVariables.ItemsSource = _server.VariableManager.GetStates();
				panelSetVar.IsEnabled = true;
			}
			catch (Exception ex)
			{
				Trace("Exception: " + ex.Message);
			}
		}

		/// <summary>Called when a new event is added to the AppVision event log.</summary>
		private void EventManager_EventChanged(Prysm.AppVision.Data.EventRow evt)
		{
			Trace("Event " + evt);
		}

		/// <summary>Called when the status of an alarm changes (raised, acknowledged, cleared).</summary>
		private void AlarmManager_AlarmChanged(Prysm.AppVision.Data.AlarmInfo alr)
		{
			Trace($"Alarm.{alr.Event} {alr.Alarm.Description}");
		}

		/// <summary>Called when the value of any subscribed variable changes.</summary>
		private void VariableManager_StateChanged(Prysm.AppVision.Data.VariableState var)
		{
			Trace($"Variable {var.Row.FullDescription} = {var.Value}");
		}

		/// <summary>Appends a timestamped message to the trace list box and to the debug output.</summary>
		public void Trace(string s)
		{
			listBox.Items.Add(DateTime.Now + " " + s);
			System.Diagnostics.Debug.WriteLine(s);
		}

		/// <summary>
		/// Writes the value entered in the text box to the variable selected in the combo box.
		/// </summary>
		private void ButtonSetVar_Click(object sender, RoutedEventArgs e)
		{
			// Change the value of a variable
			var value = txtVarValue.Text;
			var varState = cbxVariables.SelectedItem as VariableState;
			if (varState != null)
			{
				_server.VariableManager.Set(varState.Name, value);
			}
		}

		/// <summary>Disconnects from AppVision when the window is closed.</summary>
		private void Window_Closed(object sender, EventArgs e)
		{
			try
			{
				_server.Close();
			}
			catch { }
		}
	}
}
