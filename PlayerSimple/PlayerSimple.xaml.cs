using Prysm.AppVideo;
using Prysm.FFmpeg.Next;
using Prysm.FFmpeg.Next.Frames;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PlayerSimple
{
	/// <summary>
	/// Minimal live-video player: decodes the camera address (RTSP, HTTP, local file...) with the AppVision
	/// FFmpeg player (<c>App.FFmpeg.dll</c>) and reports each rendered frame to AppVision.
	/// <para>Implements <see cref="IPlayerLive"/> only: no playback, PTZ or export.</para>
	/// </summary>
	public partial class PlayerSimple : PlayerBaseControl, IPlayerLive
	{
		/// <summary>Stream address received in <see cref="Connect"/>.</summary>
		string url;

		/// <summary>
		/// Initialises the control and wires the FFmpeg player events.
		/// </summary>
		public PlayerSimple()
		{
			InitializeComponent();

			// Tell AppVision that images arrive (see OnFrameRendered).
			player.FrameRendered += OnFrameRendered;
			// Display FFmpeg errors (stream unreachable, codec...) in red in the camera tile, for 5 seconds.
			// The event may be raised on a decoding thread: SetError dispatches to the UI thread itself.
			player.ErrorOccured += (message, ex) => _ = SetError(message, 5);
		}

		/// <summary>
		/// Stores the stream address of the camera. Called by AppVision before the first <see cref="Play()"/>,
		/// and again when another camera is shown in the same tile.
		/// </summary>
		/// <param name="cameraName">Name of the camera variable.</param>
		/// <param name="address">Address field of the camera variable (or of its parent node):
		/// RTSP or HTTP URL, file path, or any input accepted by FFmpeg.</param>
		/// <param name="parameters">Parameters field of the camera variable and of its parents (one <c>key=value</c> per line).
		/// Not used by this sample.</param>
		public override Task Connect(string cameraName, string address, string parameters)
		{
			// The connection itself is opened by Play(). The address is read like an FFmpeg command line:
			// keep the quotes around a file path containing spaces ("C:\Videos\cctv 1.mp4"), and options such as
			// -f gdigrab -i desktop are accepted.
			url = address?.Trim();
			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the stream. AppVision calls it when the tile is closed or shows another camera.
		/// </summary>
		public override Task Disconnect() => Stop();

		/// <summary>
		/// Starts the live stream. AppVision calls it after <see cref="Connect"/>, when the tile is displayed.
		/// </summary>
		public override Task Play() => player.PlayAsync(
			new AVPlayerUri { Uri = url },
			new PlayerOptions
			{
				// Pure WPF rendering: AppVision draws its overlays (texts, metadata, digital zoom) over the
				// control, and FrameRendered provides a SoftwareVideoFrame holding the decoded bitmap.
				Rendering = RenderingMethod.Software,
				UseHardwareDecoding = false,
				// Stream lost or not reachable yet: retry automatically (after 2 s, then up to every 30 s).
				AutoReconnect = true,
			});

		/// <summary>
		/// Stops the FFmpeg player immediately.
		/// </summary>
		public override Task Stop()
		{
			player.Stop();
			return Task.CompletedTask;
		}

		/// <summary>
		/// Image of the last decoded frame, used by AppVision for the snapshot command of the camera tile.
		/// Without this override, AppVision takes a screen capture of the tile instead.
		/// </summary>
		public override Task<ImageSource> Snapshot() => player.Snapshot();

		/// <summary>
		/// Fill (stretched) or uniform (aspect ratio kept) display, set by AppVision from the
		/// <c>App.Client.Video.StretchVideo</c> option before <see cref="Connect"/>.
		/// </summary>
		public override bool StretchVideo
		{
			get => player.StretchVideo;
			set => player.StretchVideo = value;
		}

		/// <summary>
		/// Raised by the FFmpeg player for each rendered frame.
		/// <see cref="PlayerBaseControl.FireFrameDecoded(System.DateTime, BitmapSource)"/> tells AppVision that images arrive:
		/// the tile status goes from Connected to Playing, and in playback the timestamp moves the timeline.
		/// </summary>
		private void OnFrameRendered(object sender, Prysm.FFmpeg.Next.FrameEventArgs e)
		{
			// SoftwareVideoFrame because Rendering = Software (a Direct3D frame has no WPF bitmap).
			var bmp = (e.Frame as SoftwareVideoFrame)?.Image as BitmapSource;
			FireFrameDecoded(e.Timestamp.Date, bmp);
		}
	}
}
