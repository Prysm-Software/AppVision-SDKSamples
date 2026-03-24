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
	/// Connection settings are persisted across sessions via <see cref="Properties.Settings"/>.
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Loads the AppVision colour theme before initialising WPF controls
		/// so that all styled elements use the correct brand colours from startup.
		/// </summary>
		public MainWindow()
		{
			// RColor.Load(true) applies the AppVision WPF theme to the application resources.
			RColor.Load(true);

			InitializeComponent();
		}

		/// <summary>
		/// Connects to AppVision using credentials from <see cref="Properties.Settings"/>,
		/// subscribes to all alarm notifications and updates the status bar.
		/// Uses the <see cref="Sdk.Server"/> singleton so any other UI component in the
		/// same process can access the server without passing a reference.
		/// </summary>
		async void BtnConnect_Click(object sender, RoutedEventArgs e)
		{
			Cursor = Cursors.Wait;
			var settings = Properties.Settings.Default;
			try
			{
				Sdk.Server.Open(settings.host);
				Sdk.Server.Login(settings.user, settings.pwd);
				// LoadDatabase populates the local cache used by managers (VariableManager, etc.).
				await Sdk.Server.LoadDatabase(true);
				Sdk.Server.AlarmManager.AlarmChanged += AlarmManager_AlarmChanged;
				Sdk.Server.AddFilterNotifications("All");
				Sdk.Server.StartNotifications(true, false);

				// GetOption reads a named configuration option from the AppVision server.
				txtStatus.Text = Sdk.Server.UserManager.CurrentUser.FullName + " connected to " + Sdk.Server.GetOption("App.Main.ProjectDescription");
				panelLogin.Visibility = Visibility.Collapsed;
				panelLogout.Visibility = Visibility.Visible;
				settings.Save();
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Exception: " + ex);
				MessageBox.Show(ex.GetInnerMessages());
			}
			Cursor = null;
		}
		
		/// <summary>
		/// Unsubscribes from alarm events and cleanly disconnects from AppVision.
		/// Always unsubscribe before calling <c>Close()</c> to avoid callback-after-dispose.
		/// </summary>
		void BtnDisconnect_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Sdk.Server.AlarmManager.AlarmChanged -= AlarmManager_AlarmChanged;
				Sdk.Server.Close();
			}
			catch { }
			panelLogin.Visibility = Visibility.Visible;
			panelLogout.Visibility = Visibility.Collapsed;
		}


		/// <summary>
		/// Called when the status of any alarm changes.
		/// Extend this handler to display or react to alarm notifications.
		/// </summary>
		void AlarmManager_AlarmChanged(Prysm.AppVision.Data.AlarmInfo obj)
		{
			//Trace("Alarm "+obj.Alarm.Description);
		}
	}
}
