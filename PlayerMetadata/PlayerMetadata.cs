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
    public class PlayerMetadata : IPlayerOverlay
    {
        YoloWrapper yolo;
        FFmpegVideoPlayerCtrl media;
        Canvas drawingCanvas;
        string url;

        public event EventHandler StatusChanged;

        public PlayerStatus Status => throw new NotImplementedException();

        public bool IsActiveX => false;



        public PlayerMetadata()
        {
            yolo = new YoloWrapper(new ConfigurationDetector().Detect());
            media = new FFmpegVideoPlayerCtrl();
            media.NewImageDecoded += ImageDecoded;
            media.ErrorOccured += err => Debug.WriteLine(err);
            media.StatusChanged += Media_StatusChanged;
        }

   
        public async Task Connect(string cameraName, string address, string parameters)
        {
            // assume we're using a video file from the config directory as our images source
            // and we want to decode it at x1 speed
            media.EnableSynchro = true;
            url = Sdk.Server.Hostname + "Config/Param/Client/" + address;
        }

        [Obsolete]
        public Task Connect(string address, string parameters) => Connect("", address, parameters);


        public async Task Disconnect()
        {
            drawingCanvas = null;
            media.Stop();
        }


        public async Task Play(Canvas drawingCanvas)
        {
            this.drawingCanvas = drawingCanvas;
            media.Play(url);
        }

        // this sample does not draw metadata in playback mode.
        public async Task Play(Canvas drawingCanvas, DateTime start)
        {
            media.Stop();
            this.drawingCanvas = null;
        }
        public DateTime PlaybackPosition { set { } }

        // loop back to beginning at the end of file 
        private void Media_StatusChanged(FFmpegPlayerStatus status)
        {
            if (status == FFmpegPlayerStatus.EndOfFile)
                media.Play(url);
        }


        async void ImageDecoded(BitmapSource bmp, TimeSpan time)
        {
            if (drawingCanvas == null) return;

            // no more than 3 analysis per sec
            if (Environment.TickCount - lastdetection < 333) return;
            lastdetection = Environment.TickCount;


            IEnumerable<YoloItem> items = null;
            var mem = new MemoryStream();
            var enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmp));
            enc.Save(mem);

            await Task.Run((Action)delegate
            {
                sw.Restart();
                items = yolo.Detect(mem.ToArray());
                Debug.WriteLine($"{items.Count()} items in {sw.ElapsedMilliseconds} ms");
                mem.Dispose();
            });

            drawingCanvas.Children.Clear();
            drawingCanvas.Height = bmp.Height;
            drawingCanvas.Width = bmp.Width;
            DrawMetadata(items);
        }


        //items[0].Type -> "Person, Car, ..."
        //items[0].Confidence -> 0.0 (low) -> 1.0 (high)
        //items[0].X -> bounding box
        //items[0].Y -> bounding box
        //items[0].Width -> bounding box
        //items[0].Height -> bounding box
        void DrawMetadata(IEnumerable<YoloItem> items)
        {
            foreach (var item in items)
            {
                // pick one color per type of object
                if (!colors.TryGetValue(item.Type, out Brush color))
                    colors[item.Type] = color = palette[paletteIdx++ % palette.Length];

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


        Stopwatch sw = new Stopwatch();
        long lastdetection = Environment.TickCount;
        Brush[] palette = new Brush[] { Brushes.Lime, Brushes.Blue, Brushes.Orange, Brushes.Violet, Brushes.Aqua, Brushes.Pink, Brushes.YellowGreen };
        Dictionary<string, Brush> colors = new Dictionary<string, Brush>();
        int paletteIdx; 
    }
}
