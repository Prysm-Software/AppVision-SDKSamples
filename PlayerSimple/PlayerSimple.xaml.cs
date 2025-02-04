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
	public partial class PlayerSimple : PlayerBaseControl, IPlayerLive
	{
		private string url;

		public PlayerSimple()
		{
			InitializeComponent();

			player.FrameRendered += ImageDecoded;
			player.ErrorOccured += (err, ex) => SetError(err);
		}

		public override async Task Connect(string cameraName, string address, string parameters)
		{
			url = address;
		}

		public override Task Disconnect() => Stop();

		public override async Task Play()
		{
			await player.PlayAsync(new AVPlayerUri
			{
				Uri = url
			},
			new PlayerOptions
			{
				UseHardwareDecoding = false,
				Rendering = RenderingMethod.Software,
			});
		}

		public override Task Stop()
		{
			player.Stop();
			return Task.CompletedTask;
		}

		private void ImageDecoded(object sender, Prysm.FFmpeg.Next.FrameEventArgs e)
		{
			// WARNING: e.Frame can be null if the RenderingMethod is different than software.
			var bmp = (e.Frame as SoftwareVideoFrame)?.Image as BitmapSource;

			FireFrameDecoded(e.Timestamp.Date, bmp);
		}
	}
}
