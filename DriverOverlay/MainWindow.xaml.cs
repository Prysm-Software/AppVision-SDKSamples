using Prysm.AppVision.Common;
using Prysm.AppVision.SDK;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using static System.Math;

namespace DriverOverlay
{
	/// <summary>
	/// WPF driver that animates a geo-referenced overlay icon on an AppVision GIS synoptic.
	/// A <see cref="DispatcherTimer"/> computes a Lissajous trajectory at 2 Hz and calls
	/// <c>ClientManager.UpdateOverlay</c> to move the icon and leave a 20-dot position trail.
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>AppVision driver server connection handle.</summary>
		AppServerForDriver _appServer;
		/// <summary>Timer that drives the 2 Hz position update loop.</summary>
		DispatcherTimer _timer;

		/// <summary>Id of the moving target overlay.</summary>
		const string TargetId = "target";
		/// <summary>Prefix of the trail dot overlay ids ("tail_0", "tail_1", ...).</summary>
		const string TailPrefix = "tail_";
		/// <summary>Number of trail dots kept on the map.</summary>
		const int TailLength = 20;

		public MainWindow()
		{
			InitializeComponent();
			Title = Helper.ProductDescription + " v" + Helper.ProductVersion;

			_appServer = new AppServerForDriver();

			// SDK events are raised on background threads: go back to the UI thread before closing the window.
			// Closed : AppVision stops the protocol. Restart: the connection to the server is lost.
			_appServer.ControllerManager.Closed += () => Dispatcher.BeginInvoke((Action)Close);
			_appServer.ControllerManager.Restart += () => Dispatcher.BeginInvoke((Action)Close);
		}

		#region Load
		/// <summary>
		/// Parses command-line arguments, connects to AppVision, then starts
		/// the animation timer.
		/// Expected: <c>DriverOverlay.exe protocolName[@hostname]</c>
		/// </summary>
		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			string[] args = Environment.GetCommandLineArgs();
			if (args == null || args.Length < 2)
			{
				MessageBox.Show("Usage: DriverOverlay.exe protocolName[@hostname]", Title);
				Close();
				return;
			}
			string[] ss = args[1].Split('@');
			var protocolName = ss[0];
			var serverHostname = (ss.Length > 1) ? ss[1] : "";

			try
			{
				Trace("Connecting to AppVision server...");

				// Log into AppVision server as a driver using the protocol name.
				// Note: an empty hostname will connect to localhost.
				// Login throws if the protocol does not exist (Configurator > Protocols).
				_appServer.Open(serverHostname);
				_appServer.Login(protocolName);

				// No variable subscription here, but StartNotifications is still required:
				// it is the channel used by AppVision to send the Close command to the driver.
				_appServer.StartNotifications(false);
				_appServer.ProtocolSynchronized();
				Trace("Connected");

				// Overlays are kept by the server: remove the trail left by a previous run.
				await RemoveOverlaysAsync(TailPrefix + "*");

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
		/// <summary>Stops the animation timer, removes the overlays and disconnects from AppVision.</summary>
		private void Window_Closed(object sender, EventArgs e)
		{
			try
			{
				_timer?.Stop();

				if (_appServer.IsConnected)
				{
					// Remove the overlays of this driver, otherwise they stay on the maps.
					// Run on a worker thread and wait (2 s max): the window is closing.
					Task.Run(async () =>
					{
						await RemoveOverlaysAsync(TargetId);
						await RemoveOverlaysAsync(TailPrefix + "*");
					}).Wait(2000);

					// Disconnect from AppVision server
					_appServer.Close();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Close: " + ex.Message);    // the process is exiting
			}
		}
		#endregion

		/// <summary>
		/// Removes the overlays whose id matches <paramref name="mask"/> ('*' wildcard),
		/// one by one: <c>GetOverlays("Name=...")</c> lists them, <c>RemoveOverlay(id)</c> deletes each of them.
		/// </summary>
		/// <remarks>
		/// Removing by exact id is the reliable way: a mask passed to <c>RemoveOverlay</c> is not expanded
		/// by the AppVision 4.10 server (only one matching overlay is removed from the clients).
		/// </remarks>
		async Task RemoveOverlaysAsync(string mask)
		{
			try
			{
				var overlays = await _appServer.ClientManager.GetOverlays("Name=" + mask);
				foreach (var o in overlays ?? Enumerable.Empty<Prysm.AppVision.Data.OverlayItem>())
					await _appServer.ClientManager.RemoveOverlay(o.Name);
			}
			catch (Exception ex)
			{
				Trace("RemoveOverlays: " + ex.Message);
			}
		}

		/// <summary>
		/// Timer callback — advances the step counter and recalculates the target position
		/// in metres relative to the origin.
		/// </summary>
		private void OnTick(object sender, EventArgs e)
		{
			// Lissajous figure: X = cos(t)*50m, Y = sin(2t)*50m, one lap every 63 steps (~31 s)
			_step++;
			var t = _step * 0.1;
			_xMeters = Cos(t) * 50;
			_yMeters = Sin(2 * t) * 50;

			MoveTargetPosition();
		}
		int _step;
		double _xMeters, _yMeters;

		readonly Point _originLocation = new Point(5.370101, 43.493548);    // X=longitude, Y=latitude (Fr Aix Antelios)

		/// <summary>
		/// Converts the current metric offset from the origin into geographic coordinates
		/// and pushes the new position to AppVision with <c>ClientManager.UpdateOverlay</c>.
		/// Also maintains a 20-dot position trail: one overlay per step, the oldest one is removed.
		/// </summary>
		private void MoveTargetPosition()
		{
			try
			{
				var location = new Point(
					_originLocation.X + Helper.GetOffsetLongitude(_xMeters, _originLocation.Y),
					_originLocation.Y + Helper.GetOffsetLatitude(_yMeters));

				Trace($"move target to {location}");

				// Create or update the main icon overlay (same id each time: the icon moves).
				// Symbol: a symbol of the configuration (Param\Client\DriverOverlay\target.xaml).
				// Synoptic "@geo": the overlay is shown on every geographical (GIS) synoptic.
				// UpdateOverlay/RemoveOverlay return a Task: they are sent without waiting here.
				_ = _appServer.ClientManager.UpdateOverlay(TargetId, "DriverOverlay\\target.xaml", "@geo", location);

				// Trail: a new dot per step, shown on the synoptic "DriverOverlay\map.xaml" only.
				// The symbol can also be given as a XAML string.
				_ = _appServer.ClientManager.UpdateOverlay(TailPrefix + _step, tailSymbol, "DriverOverlay\\map.xaml", location);
				_ = _appServer.ClientManager.RemoveOverlay(TailPrefix + (_step - TailLength));
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
				if (_appServer.CurrentProtocol != null)      // logged in
					_appServer.AlarmManager.AddAlarmExt(10, text, _appServer.CurrentProtocol.Description);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error: " + ex.Message);    // never let error reporting crash the driver
			}
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
				text = DateTime.Now.ToString("HH:mm:ss.fff ") + text;
				this.listBox1.Items.Add(text);
				listBox1.ScrollIntoView(text);
			});
		}
		#endregion
	}
}
