using System;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupEMosaicMaker.Model
{
    public class ImageManipulator
    {
        #region Data members

        private double dpiX;
        private double dpiY;
        private WriteableBitmap modifiedImage;

        #endregion

        #region Constructors

        public ImageManipulator()
        {
            this.modifiedImage = null;
            this.dpiX = 0;
            this.dpiY = 0;
        }

        #endregion

        #region Methods

        private void createMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight, int blockSize)
        {

            var currentPixel = 0;
            var currentPixelMax = blockSize;
            for (var blockHeight = 0; blockHeight < imageHeight / blockSize; blockHeight++)
            {
                for (var blockWidth = 0; blockWidth < imageWidth / blockSize; blockWidth++)
                {
                    var totalRed = 0;
                    var totalBlue = 0;
                    var totalGreen = 0;
                    for (var i = currentPixel; i < currentPixelMax; i++)
                    {
                        for (var j = currentPixel; j < currentPixelMax; j++)
                        {
                            var pixelColor = this.getPixelBgra8(sourcePixels, i, j, imageWidth, imageHeight);
                            totalRed += pixelColor.R;
                            totalBlue += pixelColor.B;
                            totalGreen += pixelColor.G;
                        }
                    }

                    for (var i = currentPixel; i < currentPixelMax; i++)
                    {
                        for (var j = currentPixel; j < currentPixelMax; j++)
                        {
                            var pixelColor = this.getPixelBgra8(sourcePixels, i, j, imageWidth, imageHeight);

                            pixelColor.R = BitConverter.GetBytes(totalRed / blockSize)[0];
                            pixelColor.B = BitConverter.GetBytes(totalBlue / blockSize)[0];
                            pixelColor.G = BitConverter.GetBytes(totalGreen / blockSize)[0];
                            //pixelColor.R = 255;
                            this.setPixelBgra8(sourcePixels, i, j, pixelColor, imageWidth, imageHeight);
                        }
                    }

                    currentPixel += blockSize;
                    currentPixelMax += blockSize;
                }
            }


        }

        private Color getPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        private void setPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }

        #endregion
    }
}