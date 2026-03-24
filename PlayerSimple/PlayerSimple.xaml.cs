using Prysm.AppVideo;
using Prysm.AppVision.Common;
using Prysm.FFmpeg.Next;
using Prysm.FFmpeg.Next.Frames;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PlayerSimple
{
	/// <summary>
	/// Minimal live-video player control that decodes an RTSP/HTTP stream with FFmpeg
	/// using the software rendering pipeline and forwards each decoded frame to AppVision.
	/// <para>Implements <see cref="IPlayerLive"/> only — no playback, PTZ or export support.</para>
	/// </summary>
	public partial class PlayerSimple : PlayerBaseControl, IPlayerLive
	{
		/// <summary>Stream URL provided by AppVision during <see cref="Connect"/>.</summary>
		private string url;

		/// <summary>
		/// Initialises the control and wires the FFmpeg player event callbacks.
		/// </summary>
		public PlayerSimple()
		{
			InitializeComponent();

			// Forward every decoded frame to the AppVision host via ImageDecoded.
			player.FrameRendered += ImageDecoded;
			// Propagate FFmpeg errors to the AppVision error overlay.
			player.ErrorOccured += (err, ex) => SetError(err);
		}

		/// <summary>
		/// Stores the camera stream address provided by AppVision.
		/// Called before the first <see cref="Play"/> and whenever the camera assignment changes.
		/// </summary>
		/// <param name="cameraName">Display name of the camera (used for labelling).</param>
		/// <param name="address">Stream URL (RTSP, HTTP, local file path, …).</param>
		/// <param name="parameters">Optional driver-specific parameters (unused in this sample).</param>
		public override async Task Connect(string cameraName, string address, string parameters)
		{
			// Defer the actual network connection until Play() is called.
			url = address;
		}

		/// <summary>
		/// Stops the stream and releases resources.
		/// AppVision calls this when the tile is closed or the camera is reassigned.
		/// </summary>
		public override Task Disconnect() => Stop();

		/// <summary>
		/// Starts live decoding of the stream stored in <see cref="url"/> using the FFmpeg
		/// software rendering pipeline.
		/// AppVision calls this when the tile becomes visible or the user presses Play.
		/// </summary>
		/// <remarks>
		/// Hardware decoding is intentionally disabled: only the software pipeline produces
		/// a <see cref="SoftwareVideoFrame"/> in the <c>FrameRendered</c> event, which is
		/// required to obtain a usable <see cref="BitmapSource"/> in <see cref="ImageDecoded"/>.
		/// </remarks>
		public override async Task Play()
		{
			await player.PlayAsync(
				new AVPlayerUri { Uri = url },
				new PlayerOptions
				{
					// Software rendering is mandatory: hardware-decoded frames do not
					// expose a SoftwareVideoFrame bitmap in the FrameRendered callback.
					UseHardwareDecoding = false,
					Rendering = RenderingMethod.Software,
				});
		}

		/// <summary>
		/// Stops the FFmpeg decoder immediately.
		/// AppVision calls this before <see cref="Disconnect"/> or when the tile is hidden.
		/// </summary>
		public override Task Stop()
		{
			player.Stop();
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called by the FFmpeg player each time a video frame has been decoded and rendered.
		/// Extracts the <see cref="BitmapSource"/> from the software frame and notifies AppVision
		/// so it can display the image and trigger recording / motion-detection pipelines.
		/// </summary>
		/// <param name="sender">The FFmpeg player control that raised the event.</param>
		/// <param name="e">
		/// Event arguments containing the decoded frame and its presentation timestamp.
		/// </param>
		private void ImageDecoded(object sender, Prysm.FFmpeg.Next.FrameEventArgs e)
		{
			// e.Frame is null when Rendering != Software — the null-conditional cast handles this safely.
			var bmp = (e.Frame as SoftwareVideoFrame)?.Image as BitmapSource;

			// Notify AppVision: the timestamp drives the recording timeline;
			// bmp is the image displayed inside the camera tile.
			FireFrameDecoded(e.Timestamp.Date, bmp);
		}
	}
}
