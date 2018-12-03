using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupEMosaicMaker.Model
{
    public class Image
    {
        #region Properties

        public byte[] SourcePixels { get; set; }
        public BitmapDecoder Decoder { get; set; }

        public BitmapImage Thumbnail { get; set; }

        #endregion

        #region Constructors

        public Image(byte[] pixels, BitmapDecoder decoder, BitmapImage thumbnail)
        {
            this.SourcePixels = pixels;
            this.Decoder = decoder;
            this.Thumbnail = thumbnail;
        }

        #endregion

        public async Task ResizeImage(int size)
        {
            var transform = new BitmapTransform {
                ScaledWidth = Convert.ToUInt32(size),
                ScaledHeight = Convert.ToUInt32(size) 
            };

            var pixelData = await this.Decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage
            );

            this.SourcePixels = pixelData.DetachPixelData();

        }
    }
}