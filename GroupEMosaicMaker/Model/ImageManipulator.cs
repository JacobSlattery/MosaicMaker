using System;
using Windows.UI;

namespace GroupEMosaicMaker.Model
{
    public class ImageManipulator
    {
        #region Properties

        private uint ImageWidth { get; }
        private uint ImageHeight { get; }

        private byte[] SourcePixels { get; }

        #endregion

        #region Constructors

        public ImageManipulator(uint width, uint height, byte[] sourcePixels)
        {
            this.ImageWidth = width;
            this.ImageHeight = height;
            this.SourcePixels = (byte[]) sourcePixels.Clone();
        }

        #endregion

        #region Methods

        public byte[] RetrieveModifiedPixels()
        {
            return this.SourcePixels;
        }

        public void DrawGrid(int blockSize)
        {
            var indexes = IndexMapper.Grid(blockSize, (int)this.ImageWidth, (int)this.ImageHeight);
            IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
            Painter.FillWithColor(this.SourcePixels, indexes, Color.FromArgb(255, 255, 255, 255));
        }

        /// <summary>
        /// Creates a picture mosaic with the specified block size and image palette
        /// </summary>
        /// <param name="blockSize"> the block size </param>
        /// <param name="palette"> the palette to use</param>
        public void CreatePictureMosaic(int blockSize, ImagePalette palette)
        {
            var colors = palette.FindAverageColorsForImagesInPalette();

            var currentIndex = 0;
            var verticalJumpSize = blockSize * this.ImageWidth;
            var maxHorizontalBlocks = (int)Math.Ceiling(decimal.Divide(this.ImageWidth, blockSize));
            var maxVerticalBlocks = (int)Math.Ceiling(decimal.Divide(this.ImageHeight, blockSize));

            for (var i = 1; i <= maxVerticalBlocks; i++)
            {
                var heightOffset = (int)(i * verticalJumpSize);
                for (var j = 1; j <= maxHorizontalBlocks; j++)
                {
                    var indexes = IndexMapper.CalculateIndexBox(currentIndex, blockSize, blockSize,
                        (int)this.ImageWidth, (int)this.ImageHeight);
                    IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                    var averageColor = Panel.getPanelAverageColor(this.SourcePixels, indexes);

                    Image imageToUse;
                    var difference = 5000.0;
                    foreach (var color in colors.Keys)
                    {
                        var currentDifference = Math.Pow(((color.R - averageColor.R) * .3), 2) + 
                                                Math.Pow(((color.G - averageColor.G) * .59), 2) +  
                                                Math.Pow(((color.B - averageColor.B) * .11), 2);
                        if (currentDifference <= difference)
                        {
                            difference = currentDifference;
                            imageToUse = colors[color];
                        }
                    }

                    currentIndex += blockSize;
                }

                currentIndex = heightOffset;
            }
        }


        /// <summary>
        /// Creates the solid block mosaic with the specified block sizes
        /// </summary>
        /// <param name="blockSize"> the block size to use</param>
        public void CreateSolidBlockMosaic(int blockSize)
        {
            var currentIndex = 0;
            var verticalJumpSize = blockSize * this.ImageWidth;
            var maxHorizontalBlocks = (int) Math.Ceiling(decimal.Divide(this.ImageWidth, blockSize));
            var maxVerticalBlocks = (int) Math.Ceiling(decimal.Divide(this.ImageHeight, blockSize));

            for (var i = 1; i <= maxVerticalBlocks; i++)
            {
                var heightOffset = (int) (i * verticalJumpSize);
                for (var j = 1; j <= maxHorizontalBlocks; j++)
                {
                    var indexes = IndexMapper.Box(currentIndex, blockSize, blockSize,
                        (int) this.ImageWidth, (int) this.ImageHeight);
                    IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                    Painter.FillWithAverageColor(this.SourcePixels, indexes);
                    currentIndex += blockSize;
                }

                currentIndex = heightOffset;
            }
        }

        #endregion
    }
}