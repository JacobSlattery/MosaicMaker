using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        /// <summary>
        ///     The image width
        /// </summary>
        private uint ImageWidth { get; }

        /// <summary>
        ///     The image Height
        /// </summary>
        private uint ImageHeight { get; }

        /// <summary>
        ///     The image pixels
        /// </summary>
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

        /// <summary>
        ///     Creates a picture mosaic with the specified block size and image palette
        /// </summary>
        /// <param name="blockSize"> the block size </param>
        /// <param name="palette"> the palette to use</param>
        /// <param name="juxtaposition"> whether to juxtaposition the picture to prevent patterns</param>
        /// ///
        /// <param name="cycle">
        ///     whether to cycle through available photos to ensure no photo used used more that once more than
        ///     any other
        /// </param>
        public async Task CreatePictureMosaic(int blockSize, ImagePalette palette, bool juxtaposition, bool cycle)
        {
            if (palette.AverageColorDictionary.Count == 0)
            {
                throw new ArgumentException("There must be a minimum of one picture in the palette.", nameof(palette));
            }

            if (juxtaposition && cycle && palette.AverageColorDictionary.Count < 1)
            {
                throw new ArgumentException(
                    "There must be at least two pictures in the palette to both cycle thorough images and prevent juxtaposition",
                    nameof(palette));
            }

            var colors = palette.AverageColorDictionary;
            var cycleRemoved = new Collection<Image>();
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

                Image imageToUse = null;

                if (juxtaposition)
                {
                    if (index > imageByteWidth)
                    {
                        aboveImage = previous[aboveImageIndex];
                        aboveImageIndex++;
                    }

                    imageToUse = this.getRandomImage(indexes, palette, aboveImage, previous, disqualified,
                        cycleRemoved);
                }

                if (cycle)
                {
                    if (imageToUse == null)
                    {
                        imageToUse = findClosestMatch(colors, averageColor, cycleRemoved);
                    }

                    cycleRemoved.Add(imageToUse);

                    if (cycleRemoved.Count == colors.Count)
                    {
                        cycleRemoved.Clear();
                    }
                }

                if (imageToUse == null)
                {
                    imageToUse = findClosestMatch(colors, averageColor, cycleRemoved);
                }

                var task = await Task.Factory.StartNew(async () =>
                {
                    await imageToUse.ResizeImage(blockSize);
                    Painter.FillBlockWithPicture(this.SourcePixels, imageToUse.ModifiedPixels, indexes);
                });

                tasks.Add(task);

            }

            foreach (var task in tasks)
            {
                await task;
            }
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
                    IndexMapper.Triangle(index, blockSize, (int)this.ImageWidth, (int)this.ImageHeight);
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
                var indexes = IndexMapper.Box(index, blockSize, (int)this.ImageWidth, (int)this.ImageHeight);
                IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                Painter.FillWithAverageColor(this.SourcePixels, indexes);
            }
        }

        /// <summary>
        ///     Converts the image to black and white
        /// </summary>
        public void ConvertImageToBlackAndWhite()
        {
            Painter.ConvertToBlackAndWhite(this.SourcePixels, (int)this.ImageWidth, (int)this.ImageHeight);
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

        private Image getRandomImage(int[] indexes, ImagePalette palette, Image aboveImage, Collection<Image> previous,
            Collection<Image> disqualified, ICollection<Image> cycleRemoved)
        {
            Collection<Image> images;
            var averageColor = Painter.GetAverageColor(this.SourcePixels, indexes);
            var imagesToAvoid = disqualified.Concat(cycleRemoved).ToList();

            var randomSelectionSize = 8;

            var disqualifiedSize = 6;

            if (aboveImage != null)
            {
                imagesToAvoid.Add(aboveImage);
                images = palette.FindMultipleImagesClosestToColor(averageColor, randomSelectionSize, imagesToAvoid);
                imagesToAvoid.RemoveAt(imagesToAvoid.Count - 1);
            }
            else
            {
                images = palette.FindMultipleImagesClosestToColor(averageColor, randomSelectionSize, imagesToAvoid);
            }

            if (disqualified.Count > disqualifiedSize)
            {
                disqualified.RemoveAt(0);
            }

            var imageToUse = this.chooseRandom(images);
            disqualified.Add(imageToUse);
            previous.Add(imageToUse);
            return imageToUse;
        }

        private static Image findClosestMatch(IDictionary<Color, Collection<Image>> colors, Color averageColor,
            Collection<Image> disqualified)
        {
            Image imageToUse = null;
            var difference = StartingDifferenceBetweenColors;
            foreach (var color in colors.Keys)
            {
                var currentDifference = calculateDifferenceBetweenColors(averageColor, color);
                if (currentDifference <= difference)
                {
                    var image = colors[color][0];
                    if (!disqualified.Contains(image))
                    {
                        difference = currentDifference;
                        imageToUse = image;
                    }
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

        #endregion
    }
}