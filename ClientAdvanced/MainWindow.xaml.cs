using Prysm.AppVision.Common;
using Prysm.AppVision.SDK;
using Prysm.AppVision.UI;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ClientAdvanced
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			RColor.Load(true);

			InitializeComponent();
		}

		async void BtnConnect_Click(object sender, RoutedEventArgs e)
		{
			Cursor = Cursors.Wait;
			var settings = Properties.Settings.Default;
			try
			{
				Sdk.Server.Open(settings.host);
				Sdk.Server.Login(settings.user, settings.pwd);
				await Sdk.Server.LoadDatabase(true);
				Sdk.Server.AlarmManager.AlarmChanged += AlarmManager_AlarmChanged;
				Sdk.Server.AddFilterNotifications("All");
				Sdk.Server.StartNotifications(true, false);

				txtStatus.Text = Sdk.Server.UserManager.CurrentUser.FullName + " connected to " + Sdk.Server.GetOption("App.Main.ProjectDescription");
				panelLogin.Visibility = Visibility.Collapsed;
				panelLogout.Visibility = Visibility.Visible;
				settings.Save();

				//await c1.Load("@Timetables");
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Exception: " + ex);
				MessageBox.Show(ex.GetInnerMessages());
			}
			Cursor = null;
		}
		
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


		void AlarmManager_AlarmChanged(Prysm.AppVision.Data.AlarmInfo obj)
		{
			//Trace("Alarm "+obj.Alarm.Description);
		}
	}
}
