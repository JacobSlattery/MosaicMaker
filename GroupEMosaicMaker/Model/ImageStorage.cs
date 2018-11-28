using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupEMosaicMaker.Model
{
    public class ImageStorage
    {
        public BitmapDecoder Decoder { get; set; }
        public byte[] SourcePixels { get; set; }

        private double dpiX;
        private double dpiY;

        public async Task CreateImage(StorageFile file)
        {
            var copyBitmapImage = await this.MakeACopyOfTheFileToWorkOn(file);
            using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                this.Decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform
                {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };

                this.dpiX = this.Decoder.DpiX;
                this.dpiY = this.Decoder.DpiY;

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

        private async Task<BitmapImage> MakeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputStream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputStream);
            return newImage;
        }
    }
}
