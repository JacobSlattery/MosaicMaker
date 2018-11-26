using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupEMosaicMaker.Model
{
    public class ImageManipulator
    {
        public async Task<BitmapImage> DrawGrid(StorageFile file)
        {
            var copyBitmapImage = await this.MakeACopyOfTheFileToWorkOn(file);
            using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();
                for (var i = 0; i < copyBitmapImage.PixelHeight; i += 30)
                {
                    for (var j = 0; j < copyBitmapImage.PixelWidth; j += 30)
                    {
                        var pixelColor = this.getPixelBgra8(sourcePixels, i, j, copyBitmapImage.PixelWidth, copyBitmapImage.PixelHeight);
                        pixelColor.R = 255;
                        pixelColor.B = 255;
                        pixelColor.G = 255;
                        this.setPixelBgra8(sourcePixels, i, j, pixelColor, copyBitmapImage.PixelWidth, copyBitmapImage.PixelHeight);
                    }
                }
            }

            return copyBitmapImage;
        }

        private async Task<BitmapImage> MakeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputStream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputStream);
            return newImage;
        }

        private Color getPixelBgra8(byte[] pixels, int x, int y, int width, int height)
        {
            var offset = (x * (int)width + y) * 4;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        private void setPixelBgra8(byte[] pixels, int x, int y, Color color, int width, int height)
        {
            var offset = (x * (int)width + y) * 4;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }

    
}
