using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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


        public void DrawGrid(int blockSize, bool includeDiagonalLine)
        {
            foreach (var index in this.getBlockStartingPoints(blockSize))
            {
                var indexes = IndexMapper.Grid(index, blockSize, (int)this.ImageWidth, (int)this.ImageHeight, includeDiagonalLine);
                IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                Painter.FillWithColor(this.SourcePixels, indexes, Color.FromArgb(255, 255, 255, 255));
            }
        }

        /// <summary>
        /// Creates a picture mosaic with the specified block size and image palette
        /// </summary>
        /// <param name="blockSize"> the block size </param>
        /// <param name="palette"> the palette to use</param>
        public async Task CreatePictureMosaic(int blockSize, ImagePalette palette)
        {
            // var colors = palette.FindAverageColorsForImagesInPalette();
            var colors = palette.AverageColorDictionary;

            foreach (var index in this.getBlockStartingPoints(blockSize))
            {
                var indexes = IndexMapper.Box(index, blockSize, (int)this.ImageWidth, (int)this.ImageHeight);
                IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                var averageColor = Painter.GetAverageColor(this.SourcePixels, indexes);

                Image imageToUse = null;
                var difference = 1000000.0;
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

                if (imageToUse == null)
                {
                    //TODO
                    // Alert user
                }
                else
                {
                    await imageToUse.ResizeImage(blockSize);
                    Painter.FillBlockWithPicture(this.SourcePixels, imageToUse.modifiedPixels, indexes);
                }
                
            }
           
        }

        public void CreateTriangleMosaic(int blockSize)
        {
            foreach (var index in this.getBlockStartingPoints(blockSize))
            {
                var indexMapperData = IndexMapper.Triangle(index, blockSize, (int)this.ImageWidth, (int)this.ImageHeight);
                for (var collectionIndex = 0; collectionIndex < indexMapperData.Count; collectionIndex++)
                {
                    IndexMapper.ConvertEachIndexToMatchOffset(indexMapperData[collectionIndex], 4);
                }
                var left = indexMapperData[0];
                var right = indexMapperData[1];

                if (left.Length > 0)
                {
                    Painter.FillWithAverageColor(this.SourcePixels, left);
                }
                if (right.Length > 0)
                {
                    Painter.FillWithAverageColor(this.SourcePixels, right);
                }
                
                
            }
        }


        /// <summary>
        /// Creates the solid block mosaic with the specified block sizes
        /// </summary>
        /// <param name="blockSize"> the block size to use</param>
        public void CreateSquareMosaic(int blockSize)
        {
            foreach (var index in this.getBlockStartingPoints(blockSize))
            {
                var indexes = IndexMapper.Box(index, blockSize, (int)this.ImageWidth, (int)this.ImageHeight);
                IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                Painter.FillWithAverageColor(this.SourcePixels, indexes);
            }
        }


        private IEnumerable<int> getBlockStartingPoints(int blockSize)
        {
            var startingIndexes = new Collection<int>();
            var currentIndex = 0;
            var verticalJumpSize = blockSize * this.ImageWidth;
            var maxHorizontalBlocks = (int)Math.Ceiling(decimal.Divide(this.ImageWidth, blockSize));
            var maxVerticalBlocks = (int)Math.Ceiling(decimal.Divide(this.ImageHeight, blockSize));

            for (var i = 1; i <= maxVerticalBlocks; i++)
            {
                var heightOffset = (int)(i * verticalJumpSize);
                for (var j = 1; j <= maxHorizontalBlocks; j++)
                {
                    startingIndexes.Add(currentIndex);
                    currentIndex += blockSize;
                }

                currentIndex = heightOffset;
            }

            return startingIndexes;
        }


        private IEnumerable<int> getGridStartingPoints(int blockSize)
        {
            var gridBlockSize = blockSize - 1;
            var startingIndexes = new Collection<int>();
            var currentIndex = 0;
            var verticalJumpSize = blockSize * this.ImageWidth;
            var maxHorizontalBlocks = (int)Math.Ceiling(decimal.Divide(this.ImageWidth, gridBlockSize));
            var maxVerticalBlocks = (int)Math.Ceiling(decimal.Divide(this.ImageHeight, gridBlockSize));

            for (var i = 1; i <= maxVerticalBlocks; i++)
            {
                for (var j = 0; j < maxHorizontalBlocks; j++)
                {
                    startingIndexes.Add(currentIndex);
                    currentIndex += gridBlockSize;
                }
                var heightOffset = (int)(i * verticalJumpSize);
                currentIndex = heightOffset - (i * (int)this.ImageWidth);
            }

            return startingIndexes;
        }

        #endregion
    }
}