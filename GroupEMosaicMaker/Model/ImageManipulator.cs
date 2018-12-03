using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            foreach (var index in this.getGridStartingPoints(blockSize))
            {
                var indexes = IndexMapper.Grid(index, blockSize, (int)this.ImageWidth, (int)this.ImageHeight);
                IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                Painter.FillWithColor(this.SourcePixels, indexes, Color.FromArgb(255, 255, 255, 255));
            }
        }

        //public void DrawGrid(int blockSize)
        //{
        //    foreach (var index in this.getBlockStartingPoints(blockSize))
        //    {
        //        var indexes = IndexMapper.Grid(blockSize, (int)this.ImageWidth, (int)this.ImageHeight);
        //        IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
        //        Painter.FillWithColor(this.SourcePixels, indexes, Color.FromArgb(255, 255, 255, 255));
        //    }
        //}

        public void CreatePictureMosaic(int blockSize, ImagePalette palette)
        {
        }

        public void CreateSolidBlockMosaic(int blockSize)
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