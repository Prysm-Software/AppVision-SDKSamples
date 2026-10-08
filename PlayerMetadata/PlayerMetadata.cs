using Prysm.AppVideo;
using Prysm.AppVision.FFmpeg;
using Prysm.AppVision.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

#pragma warning disable 1998

namespace PlayerMetadata
{
	/// <summary>
	/// Metadata overlay provider that runs an object detection (Tiny YOLOv2) on the video and draws bounding boxes
	/// on the <see cref="Canvas"/> that AppVision lays over the camera tile.
	/// <para>
	/// Implements <see cref="IPlayerOverlay"/>. It is declared on the camera variable with the
	/// <c>CreateOverlayPlayer</c> parameter, next to the video player declared with <c>CreateControlPlayer</c>.
	/// AppVision calls <see cref="Connect"/>, then <see cref="Play(Canvas)"/> once the video is displayed.
	/// </para>
	/// <para>
	/// For the demonstration, this class decodes the video a second time with its own off-screen decoder
	/// (<see cref="FFmpegVideoPlayerCtrl"/>) and runs the detection on these frames. A real provider would rather
	/// receive its metadata from the video system or the analytics server (ONVIF metadata stream, VMS SDK events...).
	/// </para>
	/// </summary>
	public class PlayerMetadata : IPlayerOverlay
	{
		/// <summary>Object detector, created in <see cref="Connect"/> and released in <see cref="Disconnect"/>.</summary>
		TinyYoloDetector detector;

		/// <summary>Off-screen FFmpeg decoder — decodes the video without a visible UI element.</summary>
		readonly FFmpegVideoPlayerCtrl media;

		/// <summary>
		/// WPF canvas supplied by AppVision on <see cref="Play(Canvas)"/>.
		/// Set to <c>null</c> on <see cref="Disconnect"/> to stop any in-flight drawing.
		/// </summary>
		Canvas drawingCanvas;

		/// <summary>Video source decoded by <see cref="media"/>, built in <see cref="Connect"/>.</summary>
		string url;

		/// <summary>Detection running on a thread-pool thread, or <c>null</c>.</summary>
		Task detection;

		/// <inheritdoc/>
		public event EventHandler StatusChanged;

		/// <inheritdoc/>
		public PlayerStatus Status
		{
			get => _status;
			private set
			{
				if (_status == value)
					return;
				_status = value;
				StatusChanged?.Invoke(this, EventArgs.Empty);
			}
		}
		PlayerStatus _status = PlayerStatus.Stopped;

		/// <summary>
		/// Returns <c>false</c>: this overlay is a pure WPF element, not an ActiveX component.
		/// </summary>
		public bool IsActiveX => false;

		/// <summary>
		/// Returns <c>false</c>: reconnection is handled internally by looping on end-of-file
		/// (see <see cref="Media_StatusChanged"/>).
		/// </summary>
		public bool HostMustHandleAutoReconnection => false;


		/// <summary>
		/// Creates the off-screen FFmpeg decoder and wires its events.
		/// Keep the constructor light: an exception here would prevent the camera from being displayed at all,
		/// whereas an exception in <see cref="Connect"/> is only reported as an overlay error.
		/// </summary>
		public PlayerMetadata()
		{
			media = new FFmpegVideoPlayerCtrl();
			// Raised on the UI thread for every decoded frame — the detection is started from there.
			media.NewImageDecoded += ImageDecoded;
			// Log FFmpeg errors to the debug output instead of surfacing them to the user.
			media.ErrorOccured += err => Debug.WriteLine(err);
			// Watch for end-of-file to loop the demo video continuously.
			media.StatusChanged += Media_StatusChanged;
		}


		/// <summary>
		/// Loads the detection model and prepares the decoder.
		/// </summary>
		/// <param name="cameraName">Name of the camera variable (unused in this sample).</param>
		/// <param name="address">
		/// Value of the <c>Overlay.Address</c> parameter of the camera variable (not its Address field).
		/// A relative path is a file of the configuration, downloaded from the server:
		/// <c>{server}/Config/Param/Client/{address}</c>. An absolute path or a URL is used as is.
		/// </param>
		/// <param name="parameters">Parameters of the camera variable starting with <c>Overlay.</c>, without this prefix
		/// (unused in this sample).</param>
		public async Task Connect(string cameraName, string address, string parameters)
		{
			address = address?.Trim().Trim('"');
			if (string.IsNullOrEmpty(address))
				throw new ArgumentException("Overlay.Address parameter missing on the camera variable");

			if (address.Contains("://") || System.IO.Path.IsPathRooted(address))
				url = address;
			else
				// The video file is served by the AppVision server from the client configuration directory (Param\Client).
				// Hostname ends with a slash, e.g. http://localhost/
				url = Sdk.Server.Hostname + "Config/Param/Client/" + address.Replace('\\', '/');

			// EnableSynchro = true: decode at real-time speed (x1) rather than as fast as possible.
			media.EnableSynchro = true;

			// The model file is next to the DLL, in the AppVision Bin folder.
			// Loading takes about a second: keep it off the UI thread.
			if (detector == null)
				detector = await Task.Run(() => new TinyYoloDetector(
					System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tinyyolov2-8.onnx")));

			Status = PlayerStatus.Connected;
		}

		/// <summary>Legacy two-parameter overload, required by <see cref="IPlayerVideoDevice"/>; forwards to the three-parameter version.</summary>
		[Obsolete]
		public Task Connect(string address, string parameters) => Connect("", address, parameters);


		/// <summary>
		/// Stops the decoder, clears the canvas reference and releases the detector (native memory).
		/// AppVision calls this when the tile is closed or shows another camera.
		/// </summary>
		public async Task Disconnect()
		{
			// Nullify first so that any in-flight ImageDecoded callback exits early.
			drawingCanvas = null;
			media.Stop();

			// Never dispose the engine while a detection is still using it.
			try
			{
				if (detection != null)
					await detection;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Detection failed: " + ex.Message);
			}
			detection = null;

			detector?.Dispose();
			detector = null;
			Status = PlayerStatus.Stopped;
		}


		/// <summary>
		/// Starts live decoding and overlay rendering on the supplied canvas.
		/// AppVision calls this once the live video is displayed.
		/// </summary>
		/// <param name="drawingCanvas">
		/// WPF canvas managed by AppVision on which bounding boxes will be drawn.
		/// </param>
		public async Task Play(Canvas drawingCanvas)
		{
			this.drawingCanvas = drawingCanvas;
			media.Play(url);
			Status = PlayerStatus.Playing;
		}

		/// <summary>
		/// Playback mode is not supported by this overlay provider.
		/// Calling this method stops the decoder and clears the canvas.
		/// </summary>
		/// <param name="drawingCanvas">Canvas supplied by AppVision.</param>
		/// <param name="start">Requested playback start time (ignored).</param>
		public async Task Play(Canvas drawingCanvas, DateTime start)
		{
			// This sample only supports live mode — stop decoding if playback is requested.
			this.drawingCanvas = null;
			media.Stop();
			drawingCanvas?.Children.Clear();
			Status = PlayerStatus.Stopped;
		}

		/// <summary>
		/// Timestamp of the frame displayed in playback, set by AppVision for each frame.
		/// No-op because this overlay does not support recorded video.
		/// </summary>
		public DateTime PlaybackPosition { set { } }

		/// <summary>
		/// Handles decoder status changes. When the video file reaches its end,
		/// playback is restarted from the beginning to simulate a continuous live feed.
		/// </summary>
		/// <param name="status">New status reported by the FFmpeg decoder.</param>
		private void Media_StatusChanged(FFmpegPlayerStatus status)
		{
			// Loop the demo video file indefinitely, unless the overlay was stopped in the meantime.
			if (status == FFmpegPlayerStatus.EndOfFile)
				media.Dispatcher.InvokeAsync(() =>
				{
					if (drawingCanvas != null)
						media.Play(url);
				});
		}


		/// <summary>
		/// Called by the FFmpeg decoder, on the UI thread, each time a new video frame is available.
		/// Runs at most one detection at a time and at most 3 per second: the frame is resized to the input size
		/// of the model, the detection runs on a thread-pool thread, then the canvas is refreshed back on the UI thread.
		/// </summary>
		/// <param name="bmp">Decoded video frame as a WPF <see cref="BitmapSource"/>.</param>
		/// <param name="time">Presentation timestamp of the frame within the stream.</param>
		async void ImageDecoded(BitmapSource bmp, TimeSpan time)
		{
			// Bail out immediately if the overlay was stopped (e.g. during Disconnect).
			var canvas = drawingCanvas;
			var engine = detector;
			if (canvas == null || engine == null) return;

			// One detection at a time: frames arriving while the CPU is busy are simply skipped.
			if (detection != null && !detection.IsCompleted) return;

			// Limit inference to ~3 analyses per second (333 ms minimum interval).
			if (Environment.TickCount - lastdetection < 333) return;
			lastdetection = Environment.TickCount;

			// Resize the frame to the input of the model and copy its pixels (B, G, R, A bytes).
			// The decoder reuses its bitmap for the next frame: the copy must be made now, on the UI thread.
			int size = TinyYoloDetector.InputSize;
			var resized = new FormatConvertedBitmap(
				new TransformedBitmap(bmp, new ScaleTransform(size / (double)bmp.PixelWidth, size / (double)bmp.PixelHeight)),
				PixelFormats.Bgra32, null, 0);
			var pixels = new byte[size * size * 4];
			resized.CopyPixels(pixels, size * 4, 0);
			int width = bmp.PixelWidth, height = bmp.PixelHeight;

			try
			{
				// Run the inference on a thread-pool thread to keep the UI responsive.
				var task = Task.Run(() =>
				{
					var sw = Stopwatch.StartNew();
					var result = engine.Detect(pixels, width, height);
					Debug.WriteLine($"{result.Count} items in {sw.ElapsedMilliseconds} ms");
					return result;
				});
				detection = task;
				var items = await task;

				// Back on the UI thread. The overlay may have been stopped or moved to another canvas meanwhile.
				if (canvas != drawingCanvas)
					return;

				// Size the canvas like the video frame (AppVision scales it over the image),
				// remove stale bounding boxes, then draw the new detections.
				canvas.Children.Clear();
				canvas.Width = width;
				canvas.Height = height;
				DrawMetadata(canvas, items);
			}
			catch (Exception ex)
			{
				// async void: an exception escaping here would crash the client.
				Debug.WriteLine("Detection failed: " + ex.Message);
			}
		}


		/// <summary>
		/// Draws a coloured bounding box and a label for each detected object on the canvas.
		/// Each distinct object type receives a consistent colour chosen from the fixed palette.
		/// </summary>
		/// <remarks>
		/// <see cref="Detection"/> fields used:
		/// <list type="bullet">
		///   <item><description><c>Type</c>       — class label, e.g. "person", "car"</description></item>
		///   <item><description><c>Confidence</c> — detection score in [0.0, 1.0]</description></item>
		///   <item><description><c>X</c>, <c>Y</c>, <c>Width</c>, <c>Height</c> — bounding box in pixels of the frame</description></item>
		/// </list>
		/// </remarks>
		/// <param name="canvas">Canvas supplied by AppVision.</param>
		/// <param name="items">Detected objects returned by <see cref="TinyYoloDetector.Detect"/>.</param>
		void DrawMetadata(Canvas canvas, IEnumerable<Detection> items)
		{
			foreach (var item in items)
			{
				// Assign a stable colour per object type so the same class always looks the same.
				if (!colors.TryGetValue(item.Type, out Brush color))
					colors[item.Type] = color = palette[paletteIdx++ % palette.Length];

				// Draw the bounding rectangle with no fill so the video remains visible underneath.
				var box = new Rectangle
				{
					Height = item.Height,
					Width = item.Width,
					Fill = null,
					StrokeThickness = 2,
					Stroke = color,
				};
				Canvas.SetLeft(box, item.X);
				Canvas.SetTop(box, item.Y);
				canvas.Children.Add(box);

				// Label the box with the class name and confidence score.
				var txt = new TextBlock
				{
					FontSize = 36,
					Text = $"{item.Type}  {item.Confidence:n2}",
					Foreground = color,
				};
				Canvas.SetLeft(txt, item.X);
				Canvas.SetTop(txt, item.Y);
				canvas.Children.Add(txt);
			}
		}


		/// <summary>
		/// <see cref="Environment.TickCount"/> value at the last accepted frame,
		/// used to enforce the 3-fps inference rate limit.
		/// </summary>
		long lastdetection = Environment.TickCount;
		/// <summary>Fixed colour palette — one colour is assigned per detected object type.</summary>
		readonly Brush[] palette = new Brush[] { Brushes.Lime, Brushes.Blue, Brushes.Orange, Brushes.Violet, Brushes.Aqua, Brushes.Pink, Brushes.YellowGreen };
		/// <summary>Maps each object-type label to its assigned palette colour.</summary>
		readonly Dictionary<string, Brush> colors = new Dictionary<string, Brush>();
		/// <summary>Round-robin index into <see cref="palette"/> for new object types.</summary>
		int paletteIdx;
	}
}
