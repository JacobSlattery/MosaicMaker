using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;

namespace GroupEMosaicMaker.Model
{
    public class Panel
    {

        public static void FillPanelWithAverageColor(byte[] sourceBytes, ICollection<int> indexes)
        {
            var average = getPanelAverageColor(sourceBytes, indexes);//Color.FromArgb(255, 25, 25, 255);
            fillPanelWithColor(sourceBytes, indexes, average);
        }


        private static Color getPanelAverageColor(byte[] sourceBytes, ICollection<int> indexes)
        {
            var totalA = 0;
            var totalR = 0;
            var totalG = 0;
            var totalB = 0;
            var pixels = new Collection<byte>();
            foreach (var index in indexes)
            {
                var newindex = index * 4;
                pixels.Add(sourceBytes[newindex]);
                pixels.Add(sourceBytes[newindex+1]);
                pixels.Add(sourceBytes[newindex+2]);
                pixels.Add(sourceBytes[newindex+3]);
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

        private static void fillPanelWithColor(byte[] sourceBytes, ICollection<int> indexes, Color color)
        {
            foreach (var index in indexes)
            {
                var newindex = index * 4;
                sourceBytes[newindex + 3] = color.A;
                sourceBytes[newindex + 2] = color.R;
                sourceBytes[newindex + 1] = color.G;
                sourceBytes[newindex + 0] = color.B;
            }
        }

        private static Collection<Color> getColorForEachPixel(Collection<byte> pixels)
        {
            var colorCollection = new Collection<Color>();
            for (var index = 0; index < pixels.Count(); index += 4)
            {
                var valueA = pixels[index + 3];
                var valueR = pixels[index + 2];
                var valueG = pixels[index + 1];
                var valueB = pixels[index + 0];
                colorCollection.Add(Color.FromArgb(valueA, valueR, valueG, valueB));
            }

            return colorCollection;
        }

    }
}
