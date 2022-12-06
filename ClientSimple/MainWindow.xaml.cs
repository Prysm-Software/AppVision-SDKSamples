using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System;
using System.Windows;

namespace ClientSimple
{
	public partial class MainWindow : Window
	{
		AppServer _server;

		public MainWindow()
		{
			InitializeComponent();
		}

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
				// Subscribe for all events
				// some possible values are: 
				//		_server.AddFilterNotifications("$V.CR1.*");
				//		_server.AddFilterNotifications("$A.*");
				//		_server.AddFilterNotifications("NotificationType=VariableState");
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

		private void EventManager_EventChanged(Prysm.AppVision.Data.EventRow evt)
		{
			Trace("Event " + evt);
		}

		private void AlarmManager_AlarmChanged(Prysm.AppVision.Data.AlarmInfo alr)
		{
			Trace($"Alarm.{alr.Event} {alr.Alarm.Description}");
		}

		private void VariableManager_StateChanged(Prysm.AppVision.Data.VariableState var)
		{
			Trace($"Variable {var.Row.FullDescription} = {var.Value}");
		}

		public void Trace(string s)
		{
			listBox.Items.Add(DateTime.Now + " " + s);
			System.Diagnostics.Debug.WriteLine(s);
		}

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
