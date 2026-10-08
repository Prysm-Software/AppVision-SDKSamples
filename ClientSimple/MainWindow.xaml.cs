using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System;
using System.Linq;
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
			// a second click reconnects: close the previous session first
			Disconnect();

			try
			{
				// AppServer is one of the main classes of the SDK. It handles all communications with the server
				_server = new AppServer
				{
					// Raise the notification events on the WPF UI thread (the AppVision client does the same).
					// Without it, events may arrive on a background thread, for example after an automatic reconnection.
					SynchronizeUI = true,
				};
				_server.AlarmManager.AlarmChanged += AlarmManager_AlarmChanged;
				_server.VariableManager.StateChanged += VariableManager_StateChanged;
				_server.EventManager.EventChanged += EventManager_EventChanged;

				// Connect to the server using a name or IP address with an optional port number. ex: localhost, 192.168.0.10:8080, https://myserver
				Trace("Server.Open...");
				_server.Open(txtHost.Text);

				// Log into the server. Default user/password is "ADM", "123"
				Trace("Server.Login...");
				_server.Login(txtUser.Text, txtPwd.Password);

				// Populate a local cache with the configuration database
				Trace("Server.LoadDatabase...");
				await _server.LoadDatabase(true);

				Trace("Server.StartNotifications...");
				// Subscribe to notifications. Use a filter string to narrow scope (filters add up):
				//   "$V.CR1.*"                          — variables under node CR1
				//   "$A.Area1|$A.Area2"                 — objects of these areas ($G. for groups)
				//   "NotificationType=AlarmRow|EventRow" — only alarms and events
				//   "All"                               — everything (used here for the demo)
				_server.AddFilterNotifications("All");
				// sendInitialValues=false: only changes are notified (GetStates() below gives the current values)
				// useWebService=false: the SDK polls the server, no listening port is opened on this machine
				_server.StartNotifications(false, false);
				Trace("Connected");

				// Retrieve the status of all variables (nodes excluded: they have no value)
				cbxVariables.ItemsSource = _server.VariableManager.GetStates()
					.Where(s => s.Row?.Type != VariableType.Node)
					.OrderBy(s => s.Name)
					.ToList();
				panelSetVar.IsEnabled = true;
			}
			catch (Exception ex)
			{
				Trace("Exception: " + ex.Message);
				Disconnect();
			}
		}

		/// <summary>Called when a new event is added to the AppVision event log.</summary>
		private void EventManager_EventChanged(EventRow evt)
		{
			Trace($"Event {evt.Type} {evt.OriginDesc}: {evt.Description} {evt.StateDesc}");
		}

		/// <summary>Called when the status of an alarm changes (raised, acknowledged, cleared).</summary>
		private void AlarmManager_AlarmChanged(AlarmInfo alr)
		{
			Trace($"Alarm.{alr.Event} {alr.Alarm.Description}");
		}

		/// <summary>Called when the value of any subscribed variable changes.</summary>
		private void VariableManager_StateChanged(VariableState var)
		{
			Trace($"Variable {var.Row?.FullDescription ?? var.Name} = {var.Value}");
		}

		/// <summary>
		/// Appends a timestamped message to the trace list box and to the debug output.
		/// Safe to call from any thread: the list box is only touched on the UI thread.
		/// </summary>
		public void Trace(string s)
		{
			System.Diagnostics.Debug.WriteLine(s);

			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.InvokeAsync(() => Trace(s));
				return;
			}
			listBox.Items.Add(DateTime.Now + " " + s);
			listBox.ScrollIntoView(listBox.Items[listBox.Items.Count - 1]);
		}

		/// <summary>
		/// Writes the value entered in the text box to the variable selected in the combo box.
		/// </summary>
		private void ButtonSetVar_Click(object sender, RoutedEventArgs e)
		{
			// Change the value of a variable. The value is a string converted by the server:
			// Bool "1"/"0", Enum = state number, Double with '.' as decimal separator.
			var value = txtVarValue.Text;
			if (cbxVariables.SelectedItem is VariableState varState)
			{
				try
				{
					_server.VariableManager.Set(varState.Name, value);
				}
				catch (Exception ex)
				{
					Trace("Set failed: " + ex.Message);
				}
			}
		}

		/// <summary>Disconnects from AppVision when the window is closed.</summary>
		private void Window_Closed(object sender, EventArgs e)
		{
			Disconnect();
		}

		/// <summary>
		/// Unsubscribes from the notifications and closes the session (Close() also stops the notifications and logs out).
		/// </summary>
		void Disconnect()
		{
			if (_server == null)
				return;

			_server.AlarmManager.AlarmChanged -= AlarmManager_AlarmChanged;
			_server.VariableManager.StateChanged -= VariableManager_StateChanged;
			_server.EventManager.EventChanged -= EventManager_EventChanged;
			try
			{
				_server.Close();
			}
			catch { }
			_server = null;
			panelSetVar.IsEnabled = false;
		}
	}
}
