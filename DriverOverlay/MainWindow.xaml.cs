using Prysm.AppVision.Common;
using Prysm.AppVision.SDK;
using System;
using System.Windows;
using System.Windows.Threading;
using static System.Math;

namespace DriverOverlay
{
	public partial class MainWindow : Window
	{
		AppServerForDriver _appServer;
		DispatcherTimer _timer;

		public MainWindow()
		{
			InitializeComponent();
			Title = Helper.ProductDescription + " v" + Helper.ProductVersion;

			_appServer = new AppServerForDriver();
			_appServer.ControllerManager.Closed += Close; // Close driver window;
		}

		#region Load
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			string[] args = Environment.GetCommandLineArgs();
			if (args == null || args.Length < 2)
			{
				MessageBox.Show("Usage: DriverOverlay protocol_name");
				return;
			}
			string[] ss = args[1].Split('@');
			var protocolName = ss[0];
			var serverHostname = (ss.Length > 1) ? ss[1] : "";

			try
			{
				Trace("Connecting to AppVision server...");

				// Log into AppVision server as a driver using the protocol name.
				// Note: an empty hostname will connect to localhost 
				_appServer.Open(serverHostname);
				_appServer.Login(protocolName);

				// {protocolName} does not exist in AppVision configuration. See Configurator/Protocols
				if (_appServer.CurrentProtocol == null)
					throw new Exception("Invalid protocol " + protocolName);

				_appServer.StartNotifications(false);
				_appServer.ProtocolSynchronized();
				Trace("Connected");

				_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(.5) };
				_timer.Tick += OnTick;
				_timer.Start();
			}
			catch (Exception ex)
			{
				Error("Loaded: " + ex.Message);
				Close();
			}
		}
		#endregion
		#region Window_Closed()
		private void Window_Closed(object sender, EventArgs e)
		{
			try
			{
				_timer?.Stop();

				// Disconnect from AppVision server
				if (_appServer.IsConnected)
					_appServer.Close();
			}
			catch { }
		}
		#endregion
		private void OnTick(object sender, EventArgs e)
		{
			// Loitering around origin location
			_t += 0.1;
			if (_t > 2 * PI) _t = 0;
			_xMeters = Cos(_t) * 50;
			_yMeters = Sin(2 * _t) * 50;
			
			MoveTargetPosition();
		}
		double _t, _xMeters, _yMeters;

		readonly Point _originLocation = new Point(5.370101, 43.493548);    // X=longitude, Y=latitude (Fr Aix Antelios)
		
		private void MoveTargetPosition()
		{
			try
			{
				// Delete batch of overlays
				if (_t == 0)
					_appServer.ClientManager.RemoveOverlay("tail_*");

				var location = new Point(
					_originLocation.X + Helper.GetOffsetLongitude(_xMeters, _originLocation.Y),
					_originLocation.Y + Helper.GetOffsetLatitude(_yMeters));
				
				Trace($"move target to {location}");

				// Create or update a overlay
				// Note: "@geo" keyword will display the overlay on all geographical synoptics 
				_appServer.ClientManager.UpdateOverlay("item_id", "DriverOverlay\\target.xaml", "@geo", location);
				
				// Display a list of spot to visualize the previous positions
				_appServer.ClientManager.UpdateOverlay("tail_" + _t, tailSymbol, "DriverOverlay\\map.xaml", location);
				_appServer.ClientManager.RemoveOverlay("tail_" + Round(_t - 2, 1)); // track is 20 dots long
			}
			catch (Exception ex)
			{
				Trace("Exception: " + ex);
			}
		}

		// Note: overlay symbols can also be defined with a XAML string
		const string tailSymbol = @"<Ellipse xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:t='http://schemas.telerik.com/2008/xaml/presentation'
									Fill='#FF00B050' Width='10' Height='10' HorizontalAlignment='Center' VerticalAlignment='Center' t:MapLayer.BaseZoomLevel='19' />";

		#region Trace,Error,Log
		private void Error(string text)
		{
			try
			{
				TraceWindow("Error: " + text);
				_appServer.Log("Error: " + text);
				if (_appServer.IsConnected)
					_appServer.AlarmManager.AddAlarmExt(10, text, _appServer.CurrentProtocol.Description);
			}
			catch { }
		}
		void Trace(string text)
		{
			TraceWindow(text);
			if (_appServer.LogLevel > 0)
				_appServer.Log(text);
		}
		void TraceWindow(string text)
		{
			Dispatcher.BeginInvoke((Action)delegate
			{
				while (listBox1.Items.Count > 250)
					listBox1.Items.RemoveAt(0);
				text = DateTime.Now.ToString("hh:mm:ss.fff ") + text;
				this.listBox1.Items.Add(text);
				listBox1.ScrollIntoView(text);
			});
		}
		#endregion
	}
}
