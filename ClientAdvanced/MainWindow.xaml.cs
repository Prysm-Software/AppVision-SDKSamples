using Prysm.AppVision.Common;
using Prysm.AppVision.SDK;
using Prysm.AppVision.UI;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ClientAdvanced
{
	/// <summary>
	/// Advanced client application demonstrating the <see cref="Sdk"/> singleton pattern,
	/// AppVision colour theme loading, server option retrieval and clean disconnect.
	/// Host and user name are persisted across sessions via <see cref="Properties.Settings"/> (never the password).
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Loads the AppVision colour theme before initialising WPF controls
		/// so that all styled elements use the correct brand colours from startup.
		/// </summary>
		public MainWindow()
		{
			// RColor.Load(lightTheme) loads the AppVision colour palette used by the appUI controls: true = light, false = dark.
			RColor.Load(true);

			// Labels of the AppVision controls ("Alarm.acknowledge" -> "Acknowledge"), shipped as a resource of appClient.
			// Loaded before InitializeComponent: the controls read them when they are created.
			var labels = Application.GetResourceStream(new Uri("/appClient;component/Resources/Applab.txt", UriKind.Relative));
			if (labels != null)
				using (labels.Stream)
					RT.Load(labels.Stream, "en");

			InitializeComponent();
		}

		/// <summary>
		/// Connects to AppVision using the host and user from <see cref="Properties.Settings"/>,
		/// subscribes to all alarm notifications and updates the status bar.
		/// Uses the <see cref="Sdk.Server"/> singleton: the AppVision controls of this window
		/// (areas, cameras, alarms, events) read their data from it.
		/// </summary>
		async void BtnConnect_Click(object sender, RoutedEventArgs e)
		{
			Cursor = Cursors.Wait;
			var settings = Properties.Settings.Default;
			try
			{
				// The appUI controls update themselves from the SDK events: raise them on the UI thread, as the AppVision client does.
				Sdk.Server.SynchronizeUI = true;

				Sdk.Server.Open(settings.host);
				Sdk.Server.Login(settings.user, txtPwd.Password);
				// LoadDatabase populates the local cache used by managers (VariableManager, etc.).
				await Sdk.Server.LoadDatabase(true);
				Sdk.Server.AlarmManager.AlarmChanged += AlarmManager_AlarmChanged;
				Sdk.Server.AddFilterNotifications("All");
				// sendInitialValues=true: the server first sends the current states (variables, protocols, current alarms)
				// useWebService=false: the SDK polls the server (no listening port on this machine)
				Sdk.Server.StartNotifications(true, false);

				// The AppVision controls reload by themselves when the user changes or when the server becomes active or
				// inactive. Here both happen during Login, before LoadDatabase has filled the cache: the controls load
				// an empty cache, and nothing tells them when it is filled. Ask them to reload now, as appClient does
				// after StartNotifications.
				areas.OnUpdateContent();
				cameras.OnUpdateContent();
				alarms.OnUpdateContent();
				events.OnUpdateContent();

				// GetOption reads a named configuration option from the AppVision server.
				txtStatus.Text = Sdk.Server.UserManager.CurrentUser.FullName + " connected to " + Sdk.Server.GetOption("App.Main.ProjectDescription");
				panelLogin.Visibility = Visibility.Collapsed;
				panelLogout.Visibility = Visibility.Visible;
				settings.Save(); // host + user only
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Exception: " + ex);
				Disconnect(); // do not leave a half-open session behind
				MessageBox.Show(ex.GetInnerMessages());
			}
			Cursor = null;
		}

		/// <summary>Disconnect button.</summary>
		void BtnDisconnect_Click(object sender, RoutedEventArgs e)
		{
			Disconnect();
			panelLogin.Visibility = Visibility.Visible;
			panelLogout.Visibility = Visibility.Collapsed;
		}

		/// <summary>Closes the session when the window is closed, so that the server releases it immediately.</summary>
		void Window_Closed(object sender, EventArgs e) => Disconnect();

		/// <summary>
		/// Unsubscribes from alarm events and cleanly disconnects from AppVision.
		/// Always unsubscribe before calling <c>Close()</c> to avoid callback-after-dispose.
		/// <c>Close()</c> also stops the notifications and logs the user out.
		/// </summary>
		void Disconnect()
		{
			Sdk.Server.AlarmManager.AlarmChanged -= AlarmManager_AlarmChanged;
			try
			{
				Sdk.Server.Close();
			}
			catch { }
		}

		/// <summary>
		/// Called when the status of any alarm changes.
		/// Extend this handler to display or react to alarm notifications.
		/// </summary>
		void AlarmManager_AlarmChanged(Prysm.AppVision.Data.AlarmInfo obj)
		{
			Trace.WriteLine($"Alarm.{obj.Event} {obj.Alarm?.Description}");
		}
	}
}
