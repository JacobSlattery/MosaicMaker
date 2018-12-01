using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI;

namespace GroupEMosaicMaker.Model
{
    public class Panel
    {

        public static void FillPanelWithAverageColor(byte[] sourceBytes, ICollection<int> indexes)
        {
            var average = getPanelAverageColor(sourceBytes, indexes);
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

        private static void fillPanelWithColor(byte[] sourceBytes, ICollection<int> indexes, Color color)
        {
            foreach (var index in indexes)
            {
                sourceBytes[index + 3] = color.A;
                sourceBytes[index + 2] = color.R;
                sourceBytes[index + 1] = color.G;
                sourceBytes[index + 0] = color.B;
            }
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

    }
}
