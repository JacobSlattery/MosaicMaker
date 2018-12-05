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
        private const int BlueIndex = 0;
        private const int GreenIndex = 1;
        private const int RedIndex = 2;
        private const int OpacityIndex = 3;
        private const int StartingByteCounter = 0;
        private const int IncrementForBytes = 4;

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

        private static Color getPixelBgra8(byte[] pixels, int height, int width, uint imageWidth)
        {
            var offset = (height * (int)imageWidth + width) * 4;
            var red = pixels[offset + RedIndex];
            var green = pixels[offset + GreenIndex];
            var blue = pixels[offset + BlueIndex];
            return Color.FromArgb(0, red, green, blue);
        }

        private static void setPixelBgra8(byte[] pixels, int height, int width, Color color, uint imageWidth)
        {
            var offset = (height * (int)imageWidth + width) * 4;
            pixels[offset + RedIndex] = color.R;
            pixels[offset + GreenIndex] = color.G;
            pixels[offset + BlueIndex] = color.B;
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
                sourceBytes[index + OpacityIndex] = modifiedBytes[byteCounter + OpacityIndex];
                sourceBytes[index + RedIndex] = modifiedBytes[byteCounter + RedIndex];
                sourceBytes[index + GreenIndex] = modifiedBytes[byteCounter + GreenIndex];
                sourceBytes[index + BlueIndex] = modifiedBytes[byteCounter + BlueIndex];
                byteCounter += IncrementForBytes;
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
                sourceBytes[index + OpacityIndex] = color.A;
                sourceBytes[index + RedIndex] = color.R;
                sourceBytes[index + GreenIndex] = color.G;
                sourceBytes[index + BlueIndex] = color.B;
            }
        }

        /// <summary>
        ///     Gets the average color.
        /// </summary>
        /// <param name="sourceBytes">The source bytes.</param>
        /// <param name="indexes">The indexes.</param>
        /// <returns></returns>
        public static Color GetAverageColor(byte[] sourceBytes, IEnumerable<int> indexes)

        {
            var totalA = 0;
            var totalR = 0;
            var totalG = 0;
            var totalB = 0;
            var pixels = new Collection<byte>();
            foreach (var index in indexes)
            {
                pixels.Add(sourceBytes[index + BlueIndex]);
                pixels.Add(sourceBytes[index + GreenIndex]);
                pixels.Add(sourceBytes[index + RedIndex]);
                pixels.Add(sourceBytes[index + OpacityIndex]);
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

        private static Collection<Color> getColorForEachPixel(Collection<byte> pixelBytes)
        {
            var colorCollection = new Collection<Color>();
            for (var index = 0; index < pixelBytes.Count(); index += IncrementForBytes)
            {
                var valueA = pixelBytes[index + OpacityIndex];
                var valueR = pixelBytes[index + RedIndex];
                var valueG = pixelBytes[index + GreenIndex];
                var valueB = pixelBytes[index + BlueIndex];
                colorCollection.Add(Color.FromArgb(valueA, valueR, valueG, valueB));
            }

            return colorCollection;
        }

        #endregion
    }
}