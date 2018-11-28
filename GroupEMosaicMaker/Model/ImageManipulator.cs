using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupEMosaicMaker.Model
{
    public class ImageManipulator
    {

        private uint ImageWidth { get; }
        private uint ImageHeight { get; }

        private byte[] SourcePixels { get; }

        public ImageManipulator(uint width, uint height, byte[] sourcePixels)
        {
            this.ImageWidth = width;
            this.ImageHeight = height;
            this.SourcePixels = (byte[])sourcePixels.Clone();
        }
        #region Methods

        public byte[] RetrieveModifiedPixels()
        {
            return this.SourcePixels;
        }

        public void DrawGrid(int blockSize)
        {
            for (var i = 0; i < this.ImageHeight; i++)
            {
                for (var j = 0; j < this.ImageWidth; j += blockSize)
                {
                    var pixelColor = Color.FromArgb(255, 255, 255, 255);
                    setPixelBgra8(this.SourcePixels, i, j, pixelColor, this.ImageWidth, this.ImageHeight);
                 //   setPixelBgra8(this.SourcePixels, i, j + 1, pixelColor, this.ImageWidth, this.ImageHeight);
                }
            }

            for (var i = 0; i < this.ImageHeight; i += blockSize)
            {
                for (var j = 0; j < this.ImageWidth; j++)
                {
                    var pixelColor = Color.FromArgb(255, 255, 255, 255);
                    setPixelBgra8(this.SourcePixels, i, j, pixelColor, this.ImageWidth, this.ImageHeight);
                  //  setPixelBgra8(this.SourcePixels, i + 1, j, pixelColor, this.ImageWidth, this.ImageHeight);
                }
            }
        }

        public void CreateMosaic(int blockSize)
        {
            var currentPixelHeight = 0;
            var currentPixelMaxHeight = blockSize;
            var currentPixelWidth = 0;
            var currentPixelMaxWidth = blockSize;
            var maxForBlockHeight = Convert.ToInt32(Math.Round(Convert.ToDecimal(this.ImageHeight / blockSize))) + 1;
            var maxForBlockWidth = Convert.ToInt32(Math.Round(Convert.ToDecimal(this.ImageWidth / blockSize))) + 1;
            for (var blockHeight = 0;
                blockHeight < maxForBlockHeight;
                blockHeight++)
            {
                for (var blockWidth = 0;
                    blockWidth < maxForBlockWidth;
                    blockWidth++)
                {
                    this.averagePixelColors(currentPixelHeight, currentPixelMaxHeight, currentPixelWidth, currentPixelMaxWidth);

                    currentPixelWidth += blockSize;
                    if (currentPixelMaxWidth + blockSize > this.ImageWidth)
                    {
                        currentPixelMaxWidth += (Convert.ToInt32(this.ImageWidth) - currentPixelMaxWidth);
                    }
                    else
                    {
                        currentPixelMaxWidth += blockSize;
                    }

                }
                currentPixelHeight += blockSize;
                if (currentPixelMaxHeight + blockSize > this.ImageHeight)
                {
                    currentPixelMaxHeight += (Convert.ToInt32(this.ImageHeight) - currentPixelMaxHeight);
                }
                else
                {
                    currentPixelMaxHeight += blockSize;
                }
                currentPixelWidth = 0;
                currentPixelMaxWidth = blockSize;
            }
        }

        private void averagePixelColors(int currentPixelHeight, int currentPixelMaxHeight, int currentPixelWidth,
            int currentPixelMaxWidth)
        {
            var totalRed = 0;
            var totalBlue = 0;
            var totalGreen = 0;
            var pixelCounter = 0;

            for (var i = currentPixelHeight; i < currentPixelMaxHeight; i++)
            {
                for (var j = currentPixelWidth; j < currentPixelMaxWidth; j++)
                {
                    var pixelColor = getPixelBgra8(this.SourcePixels, i, j, this.ImageWidth, this.ImageHeight);
                    totalRed += pixelColor.R;
                    totalBlue += pixelColor.B;
                    totalGreen += pixelColor.G;
                    pixelCounter++;
                }
            }

            for (var i = currentPixelHeight; i < currentPixelMaxHeight; i++)
            {
                for (var j = currentPixelWidth; j < currentPixelMaxWidth; j++)
                {
                    var pixelColor = getPixelBgra8(this.SourcePixels, i, j, this.ImageWidth, this.ImageHeight);
                    pixelColor.R = (byte) (totalRed / pixelCounter);
                    pixelColor.B = (byte) (totalBlue / pixelCounter);
                    pixelColor.G = (byte) (totalGreen / pixelCounter);
                    setPixelBgra8(this.SourcePixels, i, j, pixelColor, this.ImageWidth, this.ImageHeight);
                }
            }
        }

        private static byte[] getPixelAt(byte[] pixels, int x, int y, uint width)
        {
            var offset = (x * (int)width + y) * 4;
            return new[] { pixels[offset], pixels[offset + 1], pixels[offset + 2], pixels[offset + 3] };
        }

        private static Color getPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        private static void setPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }

        #endregion
    }
}