using Prysm.AppVideo;
using Prysm.AppVision.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using static System.Math;

namespace PlayerClock
{
    public partial class PlayerClockControl : PlayerBaseControl, IPlayerLive, IPlayerPlayback, IPlayerPtz, IPlayerExport
	{
		const int fps = 5;

		Random _random = new Random(DateTime.Now.Second);
		DispatcherTimer _fakeVideo = new DispatcherTimer();
		DateTime _date;
		DateTime? _playbackEnd;
		

		public PlayerClockControl()
		{
			InitializeComponent();

			// Simulate video frame using a timer
			_fakeVideo.Interval = TimeSpan.FromSeconds(1.0 / fps);
			_fakeVideo.Tick += NewFrame;
			// Display the player status for debug purpose
			tbBottom.Text = Mode + ": " + Status;
		}

		#region IPlayerVideoDevice

		// A camera is about to be displayed.
		// Camera variable is linked to
		// Use info from 'address' and 'param' to identify and connect to the device.
		//*******************************
		// Note: the PlayerClockControl instance could be reused to display other camera. Connect() method could be 
		// called again with different 'address' and 'param'.
		public override async Task Connect(string cameraName, string address, string param)
		{
			this.DebugWriteLine(address);

			// Extract info from variable. ex: 
			var adr = Helper.ParseUri(address ?? "user:pwd@localhost:80");
			var user = adr.GetUsername();
			var pwd = adr.GetPassword();

			tbTop.Text = cameraName;

			// Simulate connection delay
			await Task.Delay(_random.Next(100, 200));
		}

		// Camera is removed
		public override async Task Disconnect()
		{
			this.DebugWriteLine();
			await Task.Delay(_random.Next(100, 200));
			_fakeVideo.Stop();
		}

		// If player has the capability of stretching pictures, then handle the StretchVideo property.
		public override bool StretchVideo
		{
			get { return clock.Stretch == Stretch.Fill; }
			set { clock.Stretch = value ? Stretch.Fill : Stretch.Uniform; }
		}

		#endregion

		#region IPlayerLive

		// Live video is requested
		public override async Task Play()
		{
			this.DebugWriteLine();
			await Stop();

			await Task.Delay(_random.Next(100, 200));

			PlaybackSpeed = 1;
			_fakeVideo.Tag = "live";
			_fakeVideo.Start();
		}
		public override async Task Pause()
		{
			this.DebugWriteLine();
			await Task.Delay(100);
			_fakeVideo.Stop();
		}
		public override async Task Resume()
		{
			this.DebugWriteLine();
			await Task.Delay(100);
			_fakeVideo.Start();
		}


		#endregion

		#region IPlayerPlayback

		// Players supporting video playback should implement IPlayerPlayback interface
		//
		public override async Task<bool> Play(DateTime start, TimeSpan duration, double speed)
		{
			this.DebugWriteLine($"start={start}   duration={duration}   speed={speed}");
			await Stop();

			await Task.Delay(_random.Next(100, 500));

			PlaybackSpeed = speed;
			_date = start;

			if (duration != TimeSpan.Zero)
				_playbackEnd = start + (speed > 0 ? duration : -duration);
			else
				_playbackEnd = null;

			//_mode = PlayerMode.Playback;
			_fakeVideo.Tag = "playback";
			_fakeVideo.Start();
			//Status = PlayerStatus.Playing;

			// return true if there is recorded video at the requested date and time, otherwise false;
			return true;
		}

		public override async Task Stop()
		{
			this.DebugWriteLine();

			_fakeVideo.Stop();
			tbClock.Text = "Stopped";
		}

		public override double PlaybackSpeed
		{
			get { return clock.Speed; }
			set { clock.Speed = value; }
		}

		// Player is able to play video backwards. In this case, speed value is lesser than 0.
		// ex: playing backwards at speed x2  
		// myplayer.Play(DateTime.Now, TimeSpan.Zero, -2.0);
		public override bool CanBackward => true;

		// If picture by picture mode is available, StepForward() and StepBackward() will be called
		public override bool CanGoByStep => true;

		public override async Task StepForward()
		{
			this.DebugWriteLine();
			SetDate(_date.AddSeconds(1.0 / fps));
			await Task.Delay(100);
		}
		public override async Task StepBackward()
		{
			this.DebugWriteLine();
			SetDate(_date.AddSeconds(-1.0 / fps));
			await Task.Delay(100);
		}

		#endregion
		#region IPlayerExport

		// Players that can export video sequence, should implement IPlayerExport interface
		//
		// 
		public override async Task Export(DateTime start, DateTime end, string filePath, string fileName)
		{
			// Optional : report the progress of the video export operation.
			_cancelExport = false;
			for (double i = 0; i < 100; i++)
			{
				await Task.Delay(_random.Next(30, 100));
				FireExportProgress(i, "");

				if (_cancelExport)
					throw new OperationCanceledException();
			}

			File.WriteAllText(
				Path.Combine(filePath, Path.ChangeExtension(fileName, ".txt")),
				$@"Video export demo
				from: {start}
				to:   {end}");
		}
		bool _cancelExport;
		
		// Video export can usually be cancelled
		public override void CancelExport() => _cancelExport = true;

		#endregion

		#region IPlayerPtz

		// Camera supports PTZ commands
		public override bool CanContinuousMove => true;

		// 
		public override async Task ContinuousMove(double panSpeed, double tiltSpeed)
		{

			// PTZ simulation
			DoubleAnimation x = null, y = null;
			if (panSpeed != 0) x = new DoubleAnimation(Sign(panSpeed) * ActualWidth / 2, TimeSpan.FromSeconds(1e3 / Abs(panSpeed)));
			if (tiltSpeed != 0) y = new DoubleAnimation(-Sign(tiltSpeed) * ActualHeight / 2, TimeSpan.FromSeconds(1e3 / Abs(tiltSpeed)));
			panTilt.X = panTilt.X; // retain animated value
			panTilt.Y = panTilt.Y;
			panTilt.BeginAnimation(TranslateTransform.XProperty, x);
			panTilt.BeginAnimation(TranslateTransform.YProperty, y);
		}

		public override async Task ContinuousZoom(double zoomSpeed)
		{

			DoubleAnimation z = null;
			if (zoomSpeed > 0) z = new DoubleAnimation(8, TimeSpan.FromSeconds(2e3 / zoomSpeed)); // zoom up to x8 
			else if (zoomSpeed < 0) z = new DoubleAnimation(1, TimeSpan.FromSeconds(2e3 / -zoomSpeed)); // back to 100%
			zoom.ScaleX = zoom.ScaleX;
			zoom.ScaleY = zoom.ScaleY;
			zoom.BeginAnimation(ScaleTransform.ScaleXProperty, z);
			zoom.BeginAnimation(ScaleTransform.ScaleYProperty, z);
		}


		public override bool CanPresets => true;


		public override async Task<string[]> GetPresets()
		{
			return _presets.Keys.ToArray();
		}
		public override async Task GotoPreset(string preset)
		{
			if (_presets.TryGetValue(preset, out Point3D pos))
			{
				panTilt.X = pos.X;
				panTilt.Y = pos.Y;
				zoom.ScaleX = zoom.ScaleY = pos.Z;
			}
		}

		// Store the current position in an existing preset
		public override async Task SavePreset(string preset)
		{
			if (!_presets.ContainsKey(preset))
				throw new Exception($"Preset '{preset}' not found");

			_presets[preset] = new Point3D(panTilt.X, panTilt.Y, zoom.ScaleX);
		}
		// Add the current position as a new preset
		public override async Task AddPreset(string preset)
		{
			_presets[preset] = new Point3D(panTilt.X, panTilt.Y, zoom.ScaleX);
		}
		// Remove an existing preset
		public override async Task RemovePreset(string preset)
		{
			if (!_presets.Remove(preset))
				throw new Exception($"Preset '{preset}' not found");
		}

		public override async Task GotoHomePosition()
		{
			zoom.ScaleX = zoom.ScaleY = 1;
			panTilt.X = panTilt.Y = 0;
		}
		public override Task SetHomePosition()
		{
			return SetError("This player does not allow setting the home position.", 5);
		}

		// Store presets locally for simulation purpose
		Dictionary<string, Point3D> _presets = new Dictionary<string, Point3D>
		{
			{ "preset #1", new Point3D(50, 20, 1)},
			{ "preset #2", new Point3D(0, 0, 2)},
			{ "preset #3", new Point3D(-20, -10, 1.3)},
		};

		// Same as CanPresets if player supports patterns
		public override bool CanPatterns => false;


		//public override bool CanDigitalZoom => false;


		// If player can handle manual focus or iris, set the CanFocus or CanIris value to true and implement the
		// corresponding methods.
		public override bool CanFocus => true;

		public override async Task Focus(double speed)
		{
			focus.Radius = focus.Radius;
			focus.BeginAnimation(BlurEffect.RadiusProperty, (speed == 0) ? null : new DoubleAnimation(30, TimeSpan.FromSeconds(1e3 / Abs(speed))));
		}
		public override async Task SetAutoFocus()
		{
			focus.Radius = 0;
		}

		#endregion
        
		#region Simulation internals

		void NewFrame(object sender, EventArgs e)
		{
			if ((string)_fakeVideo.Tag == "playback")
			{
				var date = _date.AddSeconds(PlaybackSpeed / fps);
				if (date > DateTime.Now) // no travel to the future
					date = DateTime.Now;
				if (PlaybackSpeed > 0 && date > _playbackEnd ||
					PlaybackSpeed < 0 && date < _playbackEnd) // end of sequence
				{
                    Stop();
					return;
				}
				SetDate(date);
			}
			else // live
			{
				SetDate(DateTime.Now);
			}
		}

		void SetDate(DateTime date)
		{
			_date = date;
			clock.Value = date;
			tbClock.Text = $"{date:d}\r\n{date:HH:mm:ss.fff}";
			tbBottom.Text = Mode + ": " + Status;

			// Notify the latest picture timestamp
			FireFrameDecoded(date);
		}
		#endregion
	}
}
