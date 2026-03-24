using Alturos.Yolo;
using Alturos.Yolo.Model;
using Prysm.AppVideo;
using Prysm.AppVision.FFmpeg;
using Prysm.AppVision.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

#pragma warning disable 1998

namespace PlayerMetadata
{
	/// <summary>
	/// Metadata overlay provider that runs YOLO object detection on every decoded video frame
	/// and draws bounding boxes directly on the AppVision-supplied <see cref="Canvas"/>.
	/// <para>
	/// Implements <see cref="IPlayerOverlay"/>: AppVision manages the camera tile and passes a
	/// <see cref="Canvas"/> to <see cref="Play(Canvas)"/>; this class handles decoding and
	/// annotation independently using <see cref="FFmpegVideoPlayerCtrl"/>.
	/// </para>
	/// </summary>
	public class PlayerMetadata : IPlayerOverlay
	{
		/// <summary>YOLO v3/v4 inference wrapper (model located automatically by <see cref="ConfigurationDetector"/>).</summary>
		YoloWrapper yolo;

		/// <summary>Off-screen FFmpeg decoder — decodes the video file without a visible UI element.</summary>
		FFmpegVideoPlayerCtrl media;

		/// <summary>
		/// WPF canvas supplied by AppVision on <see cref="Play(Canvas)"/>.
		/// Set to <c>null</c> on <see cref="Disconnect"/> to stop any in-flight drawing.
		/// </summary>
		Canvas drawingCanvas;

		/// <summary>Fully-qualified URL of the video source, built in <see cref="Connect"/>.</summary>
		string url;

		/// <inheritdoc/>
		public event EventHandler StatusChanged;

		/// <inheritdoc/>
		public PlayerStatus Status => throw new NotImplementedException();

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
        /// Initialises the YOLO inference engine and the off-screen FFmpeg decoder,
        /// then wires their event callbacks.
        /// </summary>
        public PlayerMetadata()
        {
            // ConfigurationDetector automatically locates the YOLO cfg/weights/names files.
            yolo = new YoloWrapper(new ConfigurationDetector().Detect());
            media = new FFmpegVideoPlayerCtrl();
            // ImageDecoded is raised on every decoded frame — YOLO detection runs here.
            media.NewImageDecoded += ImageDecoded;
            // Log FFmpeg errors to the debug output instead of surfacing them to the user.
            media.ErrorOccured += err => Debug.WriteLine(err);
            // Watch for end-of-file to loop the demo video continuously.
            media.StatusChanged += Media_StatusChanged;
        }

   
        /// <summary>
        /// Prepares the decoder to read a video file stored in the AppVision configuration directory.
        /// </summary>
        /// <param name="cameraName">Display name of the camera (unused in this sample).</param>
        /// <param name="address">
        /// Relative path of the video file inside the AppVision client config folder.
        /// The full URL is built as <c>{ServerHostname}/Config/Param/Client/{address}</c>.
        /// </param>
        /// <param name="parameters">Optional driver-specific parameters (unused in this sample).</param>
        public async Task Connect(string cameraName, string address, string parameters)
        {
            // EnableSynchro = true: decode at real-time speed (x1) rather than as fast as possible.
            media.EnableSynchro = true;
            // Build the absolute URL: the video file is served from the AppVision server's
            // client configuration directory over HTTP.
            url = Sdk.Server.Hostname + "Config/Param/Client/" + address;
        }

        /// <summary>Legacy two-parameter overload; forwards to the three-parameter version.</summary>
        [Obsolete]
        public Task Connect(string address, string parameters) => Connect("", address, parameters);


        /// <summary>
        /// Stops the decoder and clears the canvas reference.
        /// AppVision calls this when the tile is closed or the camera is reassigned.
        /// </summary>
        public async Task Disconnect()
        {
            // Nullify first so that any in-flight ImageDecoded callback exits early.
            drawingCanvas = null;
            media.Stop();
        }


        /// <summary>
        /// Starts live decoding and overlay rendering on the supplied canvas.
        /// AppVision calls this when the tile becomes visible or the user presses Play.
        /// </summary>
        /// <param name="drawingCanvas">
        /// WPF canvas managed by AppVision on which bounding boxes will be drawn.
        /// </param>
        public async Task Play(Canvas drawingCanvas)
        {
            this.drawingCanvas = drawingCanvas;
            media.Play(url);
        }

        /// <summary>
        /// Playback mode is not supported by this overlay provider.
        /// Calling this method stops the decoder and clears the canvas.
        /// </summary>
        /// <param name="drawingCanvas">Canvas supplied by AppVision (ignored).</param>
        /// <param name="start">Requested playback start time (ignored).</param>
        public async Task Play(Canvas drawingCanvas, DateTime start)
        {
            // This sample only supports live mode — stop decoding if playback is requested.
            media.Stop();
            this.drawingCanvas = null;
        }
        /// <summary>
        /// Playback seek position — no-op because this overlay does not support recorded video.
        /// </summary>
        public DateTime PlaybackPosition { set { } }

        /// <summary>
        /// Handles decoder status changes. When the video file reaches its end,
        /// playback is restarted from the beginning to simulate a continuous live feed.
        /// </summary>
        /// <param name="status">New status reported by the FFmpeg decoder.</param>
        private void Media_StatusChanged(FFmpegPlayerStatus status)
        {
            // Loop the demo video file indefinitely.
            if (status == FFmpegPlayerStatus.EndOfFile)
                media.Play(url);
        }


        /// <summary>
        /// Called by the FFmpeg decoder each time a new video frame is available.
        /// Rate-limits YOLO inference to at most 3 frames per second, encodes the frame as BMP,
        /// runs detection on a background thread, then refreshes the canvas overlay.
        /// </summary>
        /// <param name="bmp">Decoded video frame as a WPF <see cref="BitmapSource"/>.</param>
        /// <param name="time">Presentation timestamp of the frame within the stream.</param>
        async void ImageDecoded(BitmapSource bmp, TimeSpan time)
        {
            // Bail out immediately if the canvas was cleared (e.g. during Disconnect).
            if (drawingCanvas == null) return;

            // Limit inference to ~3 analyses per second (333 ms minimum interval)
            // to avoid overloading the CPU with back-to-back YOLO calls.
            if (Environment.TickCount - lastdetection < 333) return;
            lastdetection = Environment.TickCount;

            // Encode the WPF BitmapSource to a raw BMP byte array that Alturos.Yolo can consume.
            IEnumerable<YoloItem> items = null;
            var mem = new MemoryStream();
            var enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmp));
            enc.Save(mem);

            // Run the YOLO inference on a thread-pool thread to keep the UI responsive.
            await Task.Run((Action)delegate
            {
                sw.Restart();
                items = yolo.Detect(mem.ToArray());
                Debug.WriteLine($"{items.Count()} items in {sw.ElapsedMilliseconds} ms");
                mem.Dispose();
            });

            // Back on the UI thread: resize the canvas to match the video frame,
            // remove stale bounding boxes, then draw the new detections.
            drawingCanvas.Children.Clear();
            drawingCanvas.Height = bmp.Height;
            drawingCanvas.Width = bmp.Width;
            DrawMetadata(items);
        }


        /// <summary>
        /// Draws a coloured bounding box and a label for each detected object on the canvas.
        /// Each distinct object type receives a consistent colour chosen from the fixed palette.
        /// </summary>
        /// <remarks>
        /// <see cref="YoloItem"/> fields used:
        /// <list type="bullet">
        ///   <item><description><c>Type</c>       — class label, e.g. "Person", "Car"</description></item>
        ///   <item><description><c>Confidence</c> — detection score in [0.0, 1.0]</description></item>
        ///   <item><description><c>X</c>, <c>Y</c>, <c>Width</c>, <c>Height</c> — bounding box in pixels</description></item>
        /// </list>
        /// </remarks>
        /// <param name="items">Detected objects returned by <see cref="YoloWrapper.Detect"/>.</param>
        void DrawMetadata(IEnumerable<YoloItem> items)
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
                drawingCanvas.Children.Add(box);

                // Label the box with the class name and confidence score.
                var txt = new TextBlock
                {
                    FontSize = 36,
                    Text = $"{item.Type}  {item.Confidence:n2}",
                    Foreground = color,
                };
                Canvas.SetLeft(txt, item.X);
                Canvas.SetTop(txt, item.Y);
                drawingCanvas.Children.Add(txt);
            }
        }


        /// <summary>Measures YOLO inference duration per frame for debug logging.</summary>
        Stopwatch sw = new Stopwatch();
        /// <summary>
        /// <see cref="Environment.TickCount"/> value at the last accepted frame,
        /// used to enforce the 3-fps inference rate limit.
        /// </summary>
        long lastdetection = Environment.TickCount;
        /// <summary>Fixed colour palette — one colour is assigned per detected object type.</summary>
        Brush[] palette = new Brush[] { Brushes.Lime, Brushes.Blue, Brushes.Orange, Brushes.Violet, Brushes.Aqua, Brushes.Pink, Brushes.YellowGreen };
        /// <summary>Maps each object-type label to its assigned palette colour.</summary>
        Dictionary<string, Brush> colors = new Dictionary<string, Brush>();
        /// <summary>Round-robin index into <see cref="palette"/> for new object types.</summary>
        int paletteIdx; 
    }
}
