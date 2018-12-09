using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;

namespace GroupEMosaicMaker.Model
{
    /// <summary>
    ///     The class in charge of painting pixels
    /// </summary>
    public class Painter
    {
        #region Data members

        private const int CutOffForBlackOrWhiteColor = 127;
        private const int TotalColors = 3;
        private const int BlueOffset = 0;
        private const int GreenOffset = 1;
        private const int RedOffset = 2;
        private const int OpacityOffset = 3;
        private const int StartingByteCounter = 0;
        private const int ByteOffset = 4;

        #endregion

        #region Methods

        /// <summary>
        ///     Converts the image to black and white
        /// </summary>
        /// <param name="sourceBytes"> the source bytes being converted</param>
        /// <param name="imageWidth"> the width of the image</param>
        /// <param name="imageHeight"> the height of the image</param>
        public static void ConvertToBlackAndWhite(byte[] sourceBytes, int imageWidth, int imageHeight)
        {
            for (var height = 0; height < imageHeight; height++)
            {
                for (var width = 0; width < imageWidth; width++)
                {
                    var color = getPixelBgra8(sourceBytes, height, width, (uint) imageWidth);
                    var average = (color.R + color.G + color.B) / TotalColors;
                    if (average < CutOffForBlackOrWhiteColor)
                    {
                        setPixelBgra8(sourceBytes, height, width, Colors.Black, (uint) imageWidth);
                    }
                    else
                    {
                        setPixelBgra8(sourceBytes, height, width, Colors.White, (uint) imageWidth);
                    }
                }
            }
        }

        /// <summary>
        ///     Fills the indexes that map to the specified source bytes with the average color.
        /// </summary>
        /// <param name="sourceBytes">The source bytes.</param>
        /// <param name="indexes">The indexes.</param>
        public static void FillWithAverageColor(byte[] sourceBytes, ICollection<int> indexes)
        {
            var average = GetAverageColor(sourceBytes, indexes);
            FillWithColor(sourceBytes, indexes, average);
        }

        /// <summary>
        ///     Fills the block with the specified picture picture.
        /// </summary>
        /// <param name="sourceBytes">The source bytes.</param>
        /// <param name="modifiedBytes">The modified bytes of the picture to be used.</param>
        /// <param name="indexes">The indexes.</param>
        public static void FillBlockWithPicture(byte[] sourceBytes, byte[] modifiedBytes, IEnumerable<int> indexes)
        {
            var byteCounter = StartingByteCounter;
            foreach (var index in indexes)
            {
                sourceBytes[index + RedOffset] = modifiedBytes[byteCounter + RedOffset];
                sourceBytes[index + GreenOffset] = modifiedBytes[byteCounter + GreenOffset];
                sourceBytes[index + BlueOffset] = modifiedBytes[byteCounter + BlueOffset];
                byteCounter += ByteOffset;
            }
        }

        /// <summary>
        ///     Fills the source bytes with the specified color.
        /// </summary>
        /// <param name="sourceBytes">The source bytes.</param>
        /// <param name="indexes">The indexes.</param>
        /// <param name="color">The color.</param>
        public static void FillWithColor(byte[] sourceBytes, IEnumerable<int> indexes, Color color)
        {
            foreach (var index in indexes)
            {
                sourceBytes[index + OpacityOffset] = color.A;
                sourceBytes[index + RedOffset] = color.R;
                sourceBytes[index + GreenOffset] = color.G;
                sourceBytes[index + BlueOffset] = color.B;
            }
        }

        /// <summary>
        ///     Gets the average color.
        /// </summary>
        /// <param name="sourceBytes">The source bytes.</param>
        /// <param name="indexes">The indexes.</param>
        /// <returns></returns>
        public static Color GetAverageColor(byte[] sourceBytes, IEnumerable<int> indexes = null)
        {
            Collection<int> indexCollection;
            if (indexes == null)
            {
                indexCollection = new Collection<int>();
                for (var index = 0; index < sourceBytes.Length; index += ByteOffset)
                {
                    indexCollection.Add(index);
                }
            }
            else
            {
                indexCollection = new Collection<int>(indexes.ToList());
            }

            var totalA = 0;
            var totalR = 0;
            var totalG = 0;
            var totalB = 0;
            var pixels = new Collection<byte>();
            foreach (var index in indexCollection)
            {
                pixels.Add(sourceBytes[index + BlueOffset]);
                pixels.Add(sourceBytes[index + GreenOffset]);
                pixels.Add(sourceBytes[index + RedOffset]);
                pixels.Add(sourceBytes[index + OpacityOffset]);
            }

            var colorCollection = getColorForEachPixel(pixels);
            foreach (var currentColor in colorCollection)
            {
                totalA += currentColor.A;
                totalR += currentColor.R;
                totalG += currentColor.G;
                totalB += currentColor.B;
            }

            var newA = (byte) (totalA / colorCollection.Count);
            var newR = (byte) (totalR / colorCollection.Count);
            var newG = (byte) (totalG / colorCollection.Count);
            var newB = (byte) (totalB / colorCollection.Count);
            return Color.FromArgb(newA, newR, newG, newB);
        }

        private static Color getPixelBgra8(byte[] pixels, int height, int width, uint imageWidth)
        {
            var offset = (height * (int)imageWidth + width) * ByteOffset;
            var red = pixels[offset + RedOffset];
            var green = pixels[offset + GreenOffset];
            var blue = pixels[offset + BlueOffset];
            return Color.FromArgb(0, red, green, blue);
        }

        private static void setPixelBgra8(byte[] pixels, int height, int width, Color color, uint imageWidth)
        {
            var offset = (height * (int)imageWidth + width) * ByteOffset;
            pixels[offset + RedOffset] = color.R;
            pixels[offset + GreenOffset] = color.G;
            pixels[offset + BlueOffset] = color.B;
        }

        private static Collection<Color> getColorForEachPixel(Collection<byte> pixelBytes)
        {
            var colorCollection = new Collection<Color>();
            for (var index = 0; index < pixelBytes.Count; index += ByteOffset)
            {
                var valueA = pixelBytes[index + OpacityOffset];
                var valueR = pixelBytes[index + RedOffset];
                var valueG = pixelBytes[index + GreenOffset];
                var valueB = pixelBytes[index + BlueOffset];
                colorCollection.Add(Color.FromArgb(valueA, valueR, valueG, valueB));
            }

            return colorCollection;
        }

        #endregion
    }
}