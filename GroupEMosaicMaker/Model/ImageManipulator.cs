using System;
using System.Diagnostics;
using Windows.UI;

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
                }
            }

            for (var i = 0; i < this.ImageHeight; i += blockSize)
            {
                for (var j = 0; j < this.ImageWidth; j++)
                {
                    var pixelColor = Color.FromArgb(255, 255, 255, 255);
                    setPixelBgra8(this.SourcePixels, i, j, pixelColor, this.ImageWidth, this.ImageHeight);
                }
            }
        }

        public void CreatePictureMosaic(int blockSize, ImagePalette palette)
        {
            
        }
        public void CreateSolidBlockMosaic(int blockSize)
        {
            var currentIndex = 0;
            var verticalJumpSize = blockSize * this.ImageWidth;
            var maxHorizontalBlocks = (int) Math.Ceiling(decimal.Divide(this.ImageWidth, blockSize));
            var maxVerticalBlocks = (int) Math.Ceiling(decimal.Divide(this.ImageHeight, blockSize));



            for (var i = 1; i <= maxVerticalBlocks; i++)
            {
                var heightOffset = (int)(i * verticalJumpSize);
                for (var j = 1; j <= maxHorizontalBlocks; j++)
                {
                    Debug.WriteLine(currentIndex);
                    var indexes = ImageMapper.CalculateIndexBox(currentIndex, blockSize, blockSize, (int)this.ImageWidth, (int)this.ImageHeight);
                    ImageMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                    Panel.FillPanelWithAverageColor(this.SourcePixels, indexes);
                    currentIndex += blockSize;
                }
                currentIndex = heightOffset;
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