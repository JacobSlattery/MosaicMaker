using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupEMosaicMaker.Model
{
    /// <summary>
    /// The class in charge of painting pixels
    /// </summary>
    public class Painter
    {
        #region Methods

        /// <summary>
        /// Fills the indexes that map to the specified source bytes with the average color.
        /// </summary>
        /// <param name="sourceBytes">The source bytes.</param>
        /// <param name="indexes">The indexes.</param>
        public static void FillWithAverageColor(byte[] sourceBytes, ICollection<int> indexes)
        {
            var average = GetAverageColor(sourceBytes, indexes);
            FillWithColor(sourceBytes, indexes, average);
        }

        /// <summary>
        /// Fills the block with the specified picture picture.
        /// </summary>
        /// <param name="sourceBytes">The source bytes.</param>
        /// <param name="modifiedBytes">The modified bytes of the picture to be used.</param>
        /// <param name="indexes">The indexes.</param>
        public static void FillBlockWithPicture(byte[] sourceBytes, byte[] modifiedBytes, IEnumerable<int> indexes)
        {
            var counter = 0;
            foreach (var index in indexes)
            {
                sourceBytes[index + 3] = modifiedBytes[counter + 3];
                sourceBytes[index + 2] = modifiedBytes[counter + 2];
                sourceBytes[index + 1] = modifiedBytes[counter + 1];
                sourceBytes[index + 0] = modifiedBytes[counter + 0];
                counter += 4;
            }

        }

        /// <summary>
        /// Fills the source bytes with the specified color.
        /// </summary>
        /// <param name="sourceBytes">The source bytes.</param>
        /// <param name="indexes">The indexes.</param>
        /// <param name="color">The color.</param>
        public static void FillWithColor(byte[] sourceBytes, IEnumerable<int> indexes, Color color)
        {
            foreach (var index in indexes)
            {
                sourceBytes[index + 3] = color.A;
                sourceBytes[index + 2] = color.R;
                sourceBytes[index + 1] = color.G;
                sourceBytes[index + 0] = color.B;
            }
        }

        /// <summary>
        /// Gets the average color.
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
                pixels.Add(sourceBytes[index]);
                pixels.Add(sourceBytes[index + 1]);
                pixels.Add(sourceBytes[index + 2]);
                pixels.Add(sourceBytes[index + 3]);
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
            for (var index = 0; index < pixelBytes.Count(); index += 4)
            {
                var valueA = pixelBytes[index + 3];
                var valueR = pixelBytes[index + 2];
                var valueG = pixelBytes[index + 1];
                var valueB = pixelBytes[index + 0];
                colorCollection.Add(Color.FromArgb(valueA, valueR, valueG, valueB));
            }

            return colorCollection;
        }

        #endregion
    }
}