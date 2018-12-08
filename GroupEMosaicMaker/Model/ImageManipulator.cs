using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace GroupEMosaicMaker.Model
{
    /// <summary>
    ///     The class in charge of manipulating an image
    /// </summary>
    public class ImageManipulator
    {
        #region Data members

        private const double StartingDifferenceBetweenColors = double.PositiveInfinity;

        #endregion

        #region Properties

        private uint ImageWidth { get; }
        private uint ImageHeight { get; }

        private byte[] SourcePixels { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImageManipulator" /> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="sourcePixels">The source pixels.</param>
        public ImageManipulator(uint width, uint height, byte[] sourcePixels)
        {
            this.ImageWidth = width;
            this.ImageHeight = height;
            this.SourcePixels = (byte[]) sourcePixels.Clone();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Retrieves the modified pixels.
        /// </summary>
        /// <returns>the modified pixels.</returns>
        public byte[] RetrieveModifiedPixels()
        {
            return this.SourcePixels;
        }

        /// <summary>
        ///     Draws the grid.
        /// </summary>
        /// <param name="blockSize">Size of the block.</param>
        /// <param name="includeDiagonalLine">if set to <c>true</c> [include diagonal line].</param>
        public void DrawGrid(int blockSize, bool includeDiagonalLine)
        {
            foreach (var index in this.getBlockStartingPoints(blockSize))
            {
                var indexes = IndexMapper.Grid(index, blockSize, (int) this.ImageWidth, (int) this.ImageHeight,
                    includeDiagonalLine);
                IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                Painter.FillWithColor(this.SourcePixels, indexes, Color.FromArgb(255, 255, 255, 255));
            }
        }

        private Image chooseRandom(Collection<Image> images)
        {
            if (images.Count == 0)
            {
                throw new ArgumentException(nameof(images));
            }

            var min = 0;
            var max = images.Count - 1;
            Image chosenImage;
            if (images.Count == 1)
            {
                chosenImage = images[0];
            }
            else
            {
                chosenImage = images[new Random().Next(min, max)];
            }

            return chosenImage;
        }

        public async Task CreatePictureMosaicByCyclingThroughAvailableImages(int blockSize, ImagePalette palette)
        {
           
            var colors = palette.AverageColorDictionary;
            var temporaryColors = new Dictionary<Color, Collection<Image>>();
            this.addImagesToTemporayDictionary(colors, temporaryColors);
            var tasks = new Collection<Task>();
            foreach (var index in this.getBlockStartingPoints(blockSize))
            {
                if (temporaryColors.Count == 0)
                {
                    this.addImagesToTemporayDictionary(colors,temporaryColors);
                }
                var indexes = IndexMapper.Box(index, blockSize, (int) this.ImageWidth, (int) this.ImageHeight);
                IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                var averageColor = Painter.GetAverageColor(this.SourcePixels, indexes);
                var imageToUse = findClosestMatch(temporaryColors, averageColor);
                var task = await Task.Factory.StartNew(async () =>
                {
                    await imageToUse.ResizeImage(blockSize);
                    Painter.FillBlockWithPicture(this.SourcePixels, imageToUse.ModifiedPixels, indexes);
                });

                tasks.Add(task);
                temporaryColors.Remove(imageToUse.AverageColor);

            }

            foreach (var task in tasks)
            {
                await task;
            }
        }

        private void addImagesToTemporayDictionary(IDictionary<Color, Collection<Image>> colors, Dictionary<Color, Collection<Image>> temp)
        {
            foreach (var current in colors.Keys)
            {
                temp.Add(current, colors[current]);
            }
        }

        /// <summary>
        ///     Creates a picture mosaic with the specified block size and image palette
        /// </summary>
        /// <param name="blockSize"> the block size </param>
        /// <param name="palette"> the palette to use</param>
        /// <param name="randomize"> whether to randomize the picture to prevent patterns</param>
        public async Task CreatePictureMosaic(int blockSize, ImagePalette palette, bool randomize)
        {
            var colors = palette.AverageColorDictionary;
            var disqualified = new Collection<Image>();
            var previous = new Collection<Image>();
            var tasks = new Collection<Task>();
            var aboveImageIndex = 0;
            var imageByteWidth = (int)this.ImageWidth * 4;
            Image aboveImage = null;

            foreach (var index in this.getBlockStartingPoints(blockSize))
            {
                var indexes = IndexMapper.Box(index, blockSize, (int)this.ImageWidth, (int)this.ImageHeight);
                IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                var averageColor = Painter.GetAverageColor(this.SourcePixels, indexes);

                Image imageToUse;
                if (randomize)
                {
                    if (index > imageByteWidth)
                    {
                        aboveImage = previous[aboveImageIndex];
                        aboveImageIndex++;
                    }
                    imageToUse = this.getRandomImage(indexes, palette, aboveImage, previous, disqualified);
                }
                else
                {
                    imageToUse = findClosestMatch(colors, averageColor);
                }

                var task = await Task.Factory.StartNew(async () =>
                {
                    await imageToUse.ResizeImage(blockSize);
                    Painter.FillBlockWithPicture(this.SourcePixels, imageToUse.ModifiedPixels, indexes);
                });

                tasks.Add(task);


                //TODO
                //Alert user about possible null image

            }

            foreach (var task in tasks)
            {
                await task;
            }
        }

        private Image getRandomImage(int[] indexes, ImagePalette palette, Image aboveImage, Collection<Image> previous, Collection<Image> disqualified)
        {
            Collection<Image> images;
            var averageColor = Painter.GetAverageColor(this.SourcePixels, indexes);
            const int randomSelectionSize = 8;
            const int disqualifiedSize = 6;

            if (aboveImage != null)
            {
                disqualified.Add(aboveImage);
                images = palette.FindMultipleClosestToColor(averageColor, randomSelectionSize, disqualified);
                disqualified.RemoveAt(disqualified.Count - 1);
            }
            else
            {
                images = palette.FindMultipleClosestToColor(averageColor, randomSelectionSize, disqualified);
            }

            if (disqualified.Count >= disqualifiedSize)
            {
                disqualified.RemoveAt(0);
            }

            var imageToUse = this.chooseRandom(images);
            disqualified.Add(imageToUse);
            previous.Add(imageToUse);
            return imageToUse;
        }

        private static Image findClosestMatch(IDictionary<Color, Collection<Image>> colors, Color averageColor)
        {
            Image imageToUse = null;
            var difference = StartingDifferenceBetweenColors;
            foreach (var color in colors.Keys)
            {
                var currentDifference = calculateDifferenceBetweenColors(averageColor, color);
                if (currentDifference <= difference)
                {
                    difference = currentDifference;
                    imageToUse = colors[color][0];
                }
            }

            return imageToUse;
        }

        private static double calculateDifferenceBetweenColors(Color averageColor, Color color)
        {
            return Math.Pow((color.R - averageColor.R) * .3, 2) +
                   Math.Pow((color.G - averageColor.G) * .59, 2) +
                   Math.Pow((color.B - averageColor.B) * .11, 2);
        }

        /// <summary>
        ///     Creates the triangle mosaic.
        /// </summary>
        /// <param name="blockSize">Size of the block.</param>
        public void CreateTriangleMosaic(int blockSize)
        {
            foreach (var index in this.getBlockStartingPoints(blockSize))
            {
                var indexMapperData =
                    IndexMapper.Triangle(index, blockSize, (int) this.ImageWidth, (int) this.ImageHeight);
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
        ///     Creates the solid block mosaic with the specified block sizes
        /// </summary>
        /// <param name="blockSize"> the block size to use</param>
        public void CreateSquareMosaic(int blockSize)
        {
            foreach (var index in this.getBlockStartingPoints(blockSize))
            {
                var indexes = IndexMapper.Box(index, blockSize, (int) this.ImageWidth, (int) this.ImageHeight);
                IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                Painter.FillWithAverageColor(this.SourcePixels, indexes);
            }
        }

        /// <summary>
        ///     Converts the image to black and white
        /// </summary>
        public void ConvertImageToBlackAndWhite()
        {
            Painter.ConvertToBlackAndWhite(this.SourcePixels, (int) this.ImageWidth, (int) this.ImageHeight);
        }

        private IEnumerable<int> getBlockStartingPoints(int blockSize)
        {
            var startingIndexes = new Collection<int>();
            var currentIndex = 0;
            var verticalJumpSize = blockSize * this.ImageWidth;
            var maxHorizontalBlocks = (int) Math.Ceiling(decimal.Divide(this.ImageWidth, blockSize));
            var maxVerticalBlocks = (int) Math.Ceiling(decimal.Divide(this.ImageHeight, blockSize));

            for (var i = 1; i <= maxVerticalBlocks; i++)
            {
                var heightOffset = (int) (i * verticalJumpSize);
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
            var maxHorizontalBlocks = (int) Math.Ceiling(decimal.Divide(this.ImageWidth, gridBlockSize));
            var maxVerticalBlocks = (int) Math.Ceiling(decimal.Divide(this.ImageHeight, gridBlockSize));

            for (var i = 1; i <= maxVerticalBlocks; i++)
            {
                for (var j = 0; j < maxHorizontalBlocks; j++)
                {
                    startingIndexes.Add(currentIndex);
                    currentIndex += gridBlockSize;
                }

                var heightOffset = (int) (i * verticalJumpSize);
                currentIndex = heightOffset - i * (int) this.ImageWidth;
            }

            return startingIndexes;
        }

        #endregion
    }
}