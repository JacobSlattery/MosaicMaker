using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;

namespace GroupEMosaicMaker.Model
{
    public class Panel
    {

        public static void FillPanelWithAverageColor(byte[] pixels)
        {
            var average = getAverageColorInPanel(pixels);
            convertPanelTo(pixels, average);
        }

        private static void convertPanelTo(byte[] pixels, Color color)
        {
            for (var index=0; index < pixels.Count(); index+=4)
            {
                pixels[index + 3] = color.A;
                pixels[index + 2] = color.R;
                pixels[index + 1] = color.G;
                pixels[index + 0] = color.B;
            }
        }

        private static Color getAverageColorInPanel(byte[] pixels)
        {
            var totalA = 0;
            var totalR = 0;
            var totalG = 0;
            var totalB = 0;
            var colorCollection = getColorForEachPixel(pixels);
            foreach (var currentColor in colorCollection)
            {
                totalA += currentColor.A;
                totalR += currentColor.R;
                totalG += currentColor.G;
                totalB += currentColor.B;
            }
            var newA = Convert.ToInt32(Math.Round(Convert.ToDecimal(totalA / colorCollection.Count)));
            var newR = Convert.ToInt32(Math.Round(Convert.ToDecimal(totalR / colorCollection.Count)));
            var newG = Convert.ToInt32(Math.Round(Convert.ToDecimal(totalG / colorCollection.Count)));
            var newB = Convert.ToInt32(Math.Round(Convert.ToDecimal(totalB / colorCollection.Count)));
            return Color.FromArgb(BitConverter.GetBytes(newA)[0], BitConverter.GetBytes(newR)[0], BitConverter.GetBytes(newG)[0], BitConverter.GetBytes(newB)[0]);
        }

        private static Collection<Color> getColorForEachPixel(byte[] pixels)
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
