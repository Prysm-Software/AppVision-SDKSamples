using Prysm.AppVideo;
using Prysm.AppVision.FFmpeg;
using Prysm.AppVision.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PlayerSimple
{
    public partial class PlayerSimple : PlayerBaseControl, IPlayerLive
    {
        string url;

        public PlayerSimple()
        {
            InitializeComponent();
            player.NewImageDecoded += ImageDecoded;
            player.ErrorOccured += err => SetError(err);
        }

        public override async Task Connect(string cameraName, string address, string parameters)
        {
            url = address;
        }

        public override async Task Play() => player.Play(url);

        public override async Task Disconnect() => player.Stop();

        private void ImageDecoded(BitmapSource bmp, TimeSpan time)
        {
            FireFrameDecoded(DateTime.Now, bmp);
        }
    }
}