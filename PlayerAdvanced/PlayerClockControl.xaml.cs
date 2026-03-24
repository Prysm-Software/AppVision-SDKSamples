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
	/// <summary>
	/// Full-featured player control used as the AppVision SDK reference implementation.
	/// Renders an analogue clock face as a simulated video feed and demonstrates all four
	/// player integration interfaces:
	/// <list type="bullet">
	///   <item><description><see cref="IPlayerLive"/>     — live stream (real-time clock)</description></item>
	///   <item><description><see cref="IPlayerPlayback"/> — recorded video with variable speed and stepping</description></item>
	///   <item><description><see cref="IPlayerPtz"/>      — pan/tilt/zoom and presets</description></item>
	///   <item><description><see cref="IPlayerExport"/>   — video export with progress reporting</description></item>
	/// </list>
	/// </summary>
	public partial class PlayerClockControl : PlayerBaseControl, IPlayerLive, IPlayerPlayback, IPlayerPtz, IPlayerExport
	{
		/// <summary>Simulated frame rate used for both live and playback ticks (frames per second).</summary>
		const int fps = 5;

		/// <summary>Random number generator for simulating variable connection and seek delays.</summary>
		Random _random = new Random(DateTime.Now.Second);

		/// <summary>Timer that fires <see cref="NewFrame"/> at <see cref="fps"/> Hz to simulate a video stream.</summary>
		DispatcherTimer _fakeVideo = new DispatcherTimer();

		/// <summary>Current clock time — advances each tick in playback mode, tracks wall clock in live mode.</summary>
		DateTime _date;

		/// <summary>
		/// Playback end boundary; <c>null</c> means play until <see cref="Stop"/> is called.
		/// Compared against <see cref="_date"/> in <see cref="NewFrame"/> to detect end-of-sequence.
		/// </summary>
		DateTime? _playbackEnd;
		

		/// <summary>
		/// Initialises the control, configures the simulation timer and shows the initial status.
		/// </summary>
		public PlayerClockControl()
		{
			InitializeComponent();

			// Fire NewFrame at <fps> Hz to simulate incoming video frames.
			_fakeVideo.Interval = TimeSpan.FromSeconds(1.0 / fps);
			_fakeVideo.Tick += NewFrame;
			// Show the current Mode and Status in the bottom status bar for debugging.
			tbBottom.Text = Mode + ": " + Status;
		}

		#region IPlayerVideoDevice

		/// <summary>
		/// Called by AppVision when a camera is assigned to this tile.
		/// Parses the camera address to extract credentials and simulates a connection delay.
		/// </summary>
		/// <remarks>
		/// This method may be called multiple times on the same instance when the operator
		/// reassigns the tile — always reinitialise connection state here.
		/// </remarks>
		/// <param name="cameraName">Human-readable camera name shown at the top of the tile.</param>
		/// <param name="address">
		/// Camera address in the format <c>user:password@host:port</c>.
		/// Parsed with <see cref="Helper.ParseUri"/> to extract credentials and endpoint.
		/// </param>
		/// <param name="param">Optional driver-specific parameters (unused in this sample).</param>
		public override async Task Connect(string cameraName, string address, string param)
		{
			this.DebugWriteLine(address);

			// Parse address to extract username, password and host/port.
			var adr = Helper.ParseUri(address ?? "user:pwd@localhost:80");
			var user = adr.GetUsername();
			var pwd = adr.GetPassword();

			tbTop.Text = cameraName;

			// Simulate a real-world connection handshake delay (100–200 ms).
			await Task.Delay(_random.Next(100, 200));
		}

		/// <summary>
		/// Called by AppVision when the camera tile is closed or the camera is unassigned.
		/// Simulates a teardown delay and stops the frame timer.
		/// </summary>
		public override async Task Disconnect()
		{
			this.DebugWriteLine();
			// Simulate network teardown / device release.
			await Task.Delay(_random.Next(100, 200));
			_fakeVideo.Stop();
		}

		/// <summary>
		/// Gets or sets whether the rendered frame should be stretched to fill the tile.
		/// Maps to the <see cref="Clock.Stretch"/> property on the inner clock control.
		/// </summary>
		/// <value>
		/// <c>true</c> = <see cref="Stretch.Fill"/> (distort to fill);
		/// <c>false</c> = <see cref="Stretch.Uniform"/> (letterbox/pillarbox).
		/// </value>
		public override bool StretchVideo
		{
			get { return clock.Stretch == Stretch.Fill; }
			set { clock.Stretch = value ? Stretch.Fill : Stretch.Uniform; }
		}

		#endregion

		#region IPlayerLive

		/// <summary>
		/// Starts live mode: the clock tracks the real wall-clock time and fires
		/// <see cref="NewFrame"/> at <see cref="fps"/> Hz.
		/// AppVision calls this when the tile becomes visible or the user presses Live.
		/// </summary>
		public override async Task Play()
		{
			this.DebugWriteLine();
			// Ensure any running playback or previous live session is stopped first.
			await Stop();

			// Simulate a connection / buffering delay.
			await Task.Delay(_random.Next(100, 200));

			PlaybackSpeed = 1;
			// Tag the timer so NewFrame knows which branch to execute.
			_fakeVideo.Tag = "live";
			_fakeVideo.Start();
		}
		/// <summary>
		/// Pauses the frame timer without resetting <see cref="_date"/>.
		/// The stream can be resumed from the same position with <see cref="Resume"/>.
		/// </summary>
		public override async Task Pause()
		{
			this.DebugWriteLine();
			await Task.Delay(100);
			_fakeVideo.Stop();
		}
		/// <summary>
		/// Resumes a paused stream from the current <see cref="_date"/> position.
		/// </summary>
		public override async Task Resume()
		{
			this.DebugWriteLine();
			await Task.Delay(100);
			_fakeVideo.Start();
		}


		#endregion

		#region IPlayerPlayback

		/// <summary>
		/// Starts playback from <paramref name="start"/> for <paramref name="duration"/> at
		/// the requested <paramref name="speed"/> (negative value = reverse playback).
		/// </summary>
		/// <param name="start">Playback start date/time.</param>
		/// <param name="duration">
		/// Length of the video segment to play.
		/// Pass <see cref="TimeSpan.Zero"/> to play without a defined end point.
		/// </param>
		/// <param name="speed">
		/// Playback speed multiplier (e.g. 2.0 = double-speed forward, -1.0 = real-time reverse).
		/// </param>
		/// <returns>
		/// <c>true</c> if recorded video is available at <paramref name="start"/>;
		/// <c>false</c> otherwise. This sample always returns <c>true</c>.
		/// </returns>
		public override async Task<bool> Play(DateTime start, TimeSpan duration, double speed)
		{
			this.DebugWriteLine($"start={start}   duration={duration}   speed={speed}");
			await Stop();

			// Simulate seek / buffer-fill delay.
			await Task.Delay(_random.Next(100, 500));

			PlaybackSpeed = speed;
			_date = start;

			// Compute the absolute end time, accounting for reverse playback direction.
			if (duration != TimeSpan.Zero)
				_playbackEnd = start + (speed > 0 ? duration : -duration);
			else
				_playbackEnd = null; // no defined end — play until Stop() is called

			_fakeVideo.Tag = "playback";
			_fakeVideo.Start();

			// Return true: recorded video is assumed available at any requested time.
			return true;
		}

		/// <summary>
		/// Stops both live and playback modes, halts the frame timer and clears the clock label.
		/// </summary>
		public override async Task Stop()
		{
			this.DebugWriteLine();

			_fakeVideo.Stop();
			tbClock.Text = "Stopped";
		}

		/// <summary>
		/// Gets or sets the playback speed multiplier.
		/// Positive = forward, negative = reverse, magnitude = ratio relative to real-time.
		/// Delegates to <see cref="Clock.Speed"/>, which controls the second-hand animation rate.
		/// </summary>
		public override double PlaybackSpeed
		{
			get { return clock.Speed; }
			set { clock.Speed = value; }
		}

		/// <summary>
		/// Returns <c>true</c>: this player supports reverse playback.
		/// To play backwards at speed x2, call <c>Play(start, duration, -2.0)</c>.
		/// </summary>
		public override bool CanBackward => true;

		/// <summary>
		/// Returns <c>true</c>: this player supports frame-by-frame stepping.
		/// AppVision will call <see cref="StepForward"/> and <see cref="StepBackward"/>
		/// when the operator uses the step buttons.
		/// </summary>
		public override bool CanGoByStep => true;

		/// <summary>
		/// Advances the clock by one frame (1/<see cref="fps"/> second) and fires a frame event.
		/// </summary>
		public override async Task StepForward()
		{
			this.DebugWriteLine();
			SetDate(_date.AddSeconds(1.0 / fps));
			await Task.Delay(100);
		}
		/// <summary>
		/// Rewinds the clock by one frame (1/<see cref="fps"/> second) and fires a frame event.
		/// </summary>
		public override async Task StepBackward()
		{
			this.DebugWriteLine();
			SetDate(_date.AddSeconds(-1.0 / fps));
			await Task.Delay(100);
		}

		#endregion
		#region IPlayerExport

		/// <summary>
		/// Simulates a video export by reporting progress in 1% increments and writing
		/// a placeholder text file. A real implementation would write a video container file.
		/// </summary>
		/// <param name="start">Start of the export time range.</param>
		/// <param name="end">End of the export time range.</param>
		/// <param name="filePath">Destination directory for the exported file.</param>
		/// <param name="fileName">Base file name (extension is replaced with <c>.txt</c> in this demo).</param>
		/// <exception cref="OperationCanceledException">
		/// Thrown if <see cref="CancelExport"/> is called while the export is in progress.
		/// </exception>
		public override async Task Export(DateTime start, DateTime end, string filePath, string fileName)
		{
			_cancelExport = false;
			// Report progress from 0 to 99 in 1% steps, simulating encoding time.
			for (double i = 0; i < 100; i++)
			{
				await Task.Delay(_random.Next(30, 100));
				// FireExportProgress notifies the AppVision UI to update the progress bar.
				FireExportProgress(i, "");

				if (_cancelExport)
					throw new OperationCanceledException();
			}

			// Write a stub file instead of a real video — demonstrates the expected output location.
			File.WriteAllText(
				Path.Combine(filePath, Path.ChangeExtension(fileName, ".txt")),
				$@"Video export demo
				from: {start}
				to:   {end}");
		}
		/// <summary>Flag set by <see cref="CancelExport"/> to interrupt an ongoing export loop.</summary>
		bool _cancelExport;

		/// <summary>
		/// Signals the running <see cref="Export"/> operation to stop and throw
		/// <see cref="OperationCanceledException"/>.
		/// </summary>
		public override void CancelExport() => _cancelExport = true;

		#endregion

		#region IPlayerPtz

		/// <summary>Returns <c>true</c>: this player supports continuous pan/tilt movement.</summary>
		public override bool CanContinuousMove => true;

		/// <summary>
		/// Starts a simulated continuous pan/tilt animation.
		/// A real implementation would send velocity commands to the camera hardware.
		/// </summary>
		/// <param name="panSpeed">
		/// Horizontal speed — positive = right, negative = left, 0 = stop.
		/// Magnitude determines the WPF animation duration.
		/// </param>
		/// <param name="tiltSpeed">
		/// Vertical speed — positive = down, negative = up, 0 = stop.
		/// Negated internally to match camera convention (positive tilt = look down).
		/// </param>
		public override async Task ContinuousMove(double panSpeed, double tiltSpeed)
		{
			// Build animations that move the panTilt TranslateTransform toward the frame edge.
			DoubleAnimation x = null, y = null;
			if (panSpeed != 0) x = new DoubleAnimation(Sign(panSpeed) * ActualWidth / 2, TimeSpan.FromSeconds(1e3 / Abs(panSpeed)));
			if (tiltSpeed != 0) y = new DoubleAnimation(-Sign(tiltSpeed) * ActualHeight / 2, TimeSpan.FromSeconds(1e3 / Abs(tiltSpeed)));
			// Read current animated values before BeginAnimation resets them to their base value.
			panTilt.X = panTilt.X;
			panTilt.Y = panTilt.Y;
			panTilt.BeginAnimation(TranslateTransform.XProperty, x);
			panTilt.BeginAnimation(TranslateTransform.YProperty, y);
		}

		/// <summary>
		/// Starts a simulated continuous zoom animation.
		/// Positive speed zooms in toward x8 magnification; negative speed zooms back out to 100%.
		/// </summary>
		/// <param name="zoomSpeed">
		/// Zoom velocity — positive = zoom in, negative = zoom out, 0 = stop.
		/// </param>
		public override async Task ContinuousZoom(double zoomSpeed)
		{
			DoubleAnimation z = null;
			if (zoomSpeed > 0)      z = new DoubleAnimation(8, TimeSpan.FromSeconds(2e3 / zoomSpeed));  // animate toward x8
			else if (zoomSpeed < 0) z = new DoubleAnimation(1, TimeSpan.FromSeconds(2e3 / -zoomSpeed)); // animate back to x1
			// Latch animated values before BeginAnimation resets them to their base value.
			zoom.ScaleX = zoom.ScaleX;
			zoom.ScaleY = zoom.ScaleY;
			zoom.BeginAnimation(ScaleTransform.ScaleXProperty, z);
			zoom.BeginAnimation(ScaleTransform.ScaleYProperty, z);
		}


		/// <summary>Returns <c>true</c>: this player supports named PTZ presets.</summary>
		public override bool CanPresets => true;

		/// <summary>Returns the names of all currently defined PTZ presets.</summary>
		public override async Task<string[]> GetPresets()
		{
			return _presets.Keys.ToArray();
		}
		/// <summary>
		/// Moves the camera to the pan/tilt/zoom position stored under <paramref name="preset"/>.
		/// </summary>
		/// <param name="preset">Name of the preset to go to.</param>
		public override async Task GotoPreset(string preset)
		{
			// Point3D stores (pan, tilt, zoom) as (X, Y, Z).
			if (_presets.TryGetValue(preset, out Point3D pos))
			{
				panTilt.X = pos.X;
				panTilt.Y = pos.Y;
				zoom.ScaleX = zoom.ScaleY = pos.Z;
			}
		}

		/// <summary>
		/// Updates the stored pan/tilt/zoom values of an existing preset to the current position.
		/// </summary>
		/// <param name="preset">Name of the preset to overwrite.</param>
		/// <exception cref="Exception">Thrown if <paramref name="preset"/> does not exist.</exception>
		public override async Task SavePreset(string preset)
		{
			if (!_presets.ContainsKey(preset))
				throw new Exception($"Preset '{preset}' not found");

			_presets[preset] = new Point3D(panTilt.X, panTilt.Y, zoom.ScaleX);
		}
		/// <summary>
		/// Adds the current pan/tilt/zoom position as a new named preset.
		/// If a preset with the same name already exists it is overwritten.
		/// </summary>
		/// <param name="preset">Name of the new preset.</param>
		public override async Task AddPreset(string preset)
		{
			_presets[preset] = new Point3D(panTilt.X, panTilt.Y, zoom.ScaleX);
		}
		/// <summary>Removes an existing preset by name.</summary>
		/// <param name="preset">Name of the preset to remove.</param>
		/// <exception cref="Exception">Thrown if <paramref name="preset"/> does not exist.</exception>
		public override async Task RemovePreset(string preset)
		{
			if (!_presets.Remove(preset))
				throw new Exception($"Preset '{preset}' not found");
		}

		/// <summary>
		/// Resets the camera to the home position: zoom 100%, pan/tilt centred.
		/// </summary>
		public override async Task GotoHomePosition()
		{
			zoom.ScaleX = zoom.ScaleY = 1;
			panTilt.X = panTilt.Y = 0;
		}
		/// <summary>
		/// Setting a custom home position is not supported; reports an error to AppVision.
		/// </summary>
		public override Task SetHomePosition()
		{
			// Demonstrate how to surface a capability error to the AppVision UI.
			return SetError("This player does not allow setting the home position.", 5);
		}

		/// <summary>
		/// In-memory preset store keyed by preset name.
		/// Each value is a <see cref="Point3D"/> where X = pan offset, Y = tilt offset, Z = zoom scale.
		/// Initialised with three demonstration presets.
		/// </summary>
		Dictionary<string, Point3D> _presets = new Dictionary<string, Point3D>
		{
			{ "preset #1", new Point3D(50, 20, 1)},
			{ "preset #2", new Point3D(0, 0, 2)},
			{ "preset #3", new Point3D(-20, -10, 1.3)},
		};

		/// <summary>
		/// Returns <c>false</c>: this player does not support PTZ patrol patterns.
		/// Set to <c>true</c> and implement <c>GetPatterns</c>/<c>GotoPattern</c> to enable.
		/// </summary>
		public override bool CanPatterns => false;


		//public override bool CanDigitalZoom => false;


		/// <summary>
		/// Returns <c>true</c>: this player supports manual focus control.
		/// Set <c>CanIris</c> to <c>true</c> and implement <c>Iris()</c> to also expose iris control.
		/// </summary>
		public override bool CanFocus => true;

		/// <summary>
		/// Simulates manual focus by animating a <see cref="BlurEffect"/> on the clock face.
		/// Non-zero <paramref name="speed"/> blurs toward radius 30; zero stops the animation.
		/// </summary>
		/// <param name="speed">Focus motor speed — positive = focus far, negative = focus near, 0 = stop.</param>
		public override async Task Focus(double speed)
		{
			// Latch the current animated radius before starting a new animation.
			focus.Radius = focus.Radius;
			// Null animation removes the blur effect (speed == 0 means stop focus motor).
			focus.BeginAnimation(BlurEffect.RadiusProperty, (speed == 0) ? null : new DoubleAnimation(30, TimeSpan.FromSeconds(1e3 / Abs(speed))));
		}
		/// <summary>
		/// Triggers auto-focus: immediately clears the blur effect (radius = 0 = sharp image).
		/// </summary>
		public override async Task SetAutoFocus()
		{
			focus.Radius = 0;
		}

		#endregion
        
		#region Simulation internals

		/// <summary>
		/// DispatcherTimer tick handler — produces one simulated video frame per tick.
		/// In playback mode, advances <see cref="_date"/> by <c>PlaybackSpeed / fps</c> seconds
		/// and stops automatically when the end of the requested segment is reached.
		/// In live mode, always uses the current wall-clock time.
		/// </summary>
		void NewFrame(object sender, EventArgs e)
		{
			if ((string)_fakeVideo.Tag == "playback")
			{
				var date = _date.AddSeconds(PlaybackSpeed / fps);
				// Prevent fast-forward from going beyond the current wall time.
				if (date > DateTime.Now)
					date = DateTime.Now;
				// Detect end-of-sequence for both forward and reverse playback.
				if (PlaybackSpeed > 0 && date > _playbackEnd ||
					PlaybackSpeed < 0 && date < _playbackEnd)
				{
					Stop();
					return;
				}
				SetDate(date);
			}
			else // live: always track real time
			{
				SetDate(DateTime.Now);
			}
		}

		/// <summary>
		/// Updates the internal clock position, refreshes all UI labels and calls
		/// <see cref="PlayerBaseControl.FireFrameDecoded(DateTime)"/> so AppVision records
		/// the frame timestamp for the timeline display and recording.
		/// </summary>
		/// <param name="date">The new clock value (frame presentation time).</param>
		void SetDate(DateTime date)
		{
			_date = date;
			clock.Value = date;
			tbClock.Text = $"{date:d}\r\n{date:HH:mm:ss.fff}";
			tbBottom.Text = Mode + ": " + Status;

			// Notify AppVision of the new frame timestamp — drives the recording timeline.
			FireFrameDecoded(date);
		}
		#endregion
	}
}
