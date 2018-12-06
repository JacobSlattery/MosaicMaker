using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;

namespace GroupEMosaicMaker.Model
{
    /// <summary>
    /// Keeps track of a list of images and performs actions on them
    /// </summary>
    public class ImagePalette
    {
        #region Data members

        /// <summary>
        ///     The original images
        /// </summary>
        public ICollection<Image> OriginalImages;

        /// <summary>
        ///     The average color dictionary
        /// </summary>
        public IDictionary<Color, Collection<Image>> AverageColorDictionary;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImagePalette" /> class.
        /// </summary>
        public ImagePalette()
        {
            this.OriginalImages = new Collection<Image>();
            this.AverageColorDictionary = new Dictionary<Color, Collection<Image>>();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds the image.
        /// </summary>
        /// <param name="image">The image.</param>
        public void AddImage(Image image)
        {
            this.OriginalImages.Add(image);
            this.addImageToAverageColorDictionary(image);
        }

        /// <summary>
        ///     Clears the palette.
        /// </summary>
        public void ClearPalette()
        {
            this.OriginalImages.Clear();
        }



        //public Image[] FindMultipleImagesFor(Color color, int imageCount)
        //{
        //    var colors = new Color[imageCount];

        //    foreach (var currentColor in this.AverageColorDictionary.Keys)
        //    {
        //        for (var index = 0; index < colors.Length; index++)
        //        {
        //            var arrayColor = colors[index];
        //            if (arrayColor.Equals(null))
        //            {
        //                colors[index] = currentColor;
        //            }
        //            else if (this.isColorCloser(currentColor, arrayColor, color))
        //            {
        //                colors[index] = currentColor;
        //            }
        //        }
        //    }

        //}


        private bool isColorCloser(Color primary, Color other, Color baseColor)
        {
            return (calculateDifferenceBetweenColors(primary, baseColor) <
                    calculateDifferenceBetweenColors(other, baseColor));
        }

        private static double calculateDifferenceBetweenColors(Color averageColor, Color color)
        {
            return Math.Pow((color.R - averageColor.R) * .3, 2) +
                   Math.Pow((color.G - averageColor.G) * .59, 2) +
                   Math.Pow((color.B - averageColor.B) * .11, 2);
        }

        private void addImageToAverageColorDictionary(Image image)
        {
            var color = Painter.GetAverageColor(image.SourcePixels);

            if (!this.AverageColorDictionary.ContainsKey(color))
            {
                this.AverageColorDictionary.Add(color, new Collection<Image>());
            }
            this.AverageColorDictionary[color].Add(image);

        }

        #endregion
    }
}