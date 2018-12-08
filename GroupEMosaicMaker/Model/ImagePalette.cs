using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        /// Removes the image.
        /// </summary>
        /// <param name="image">The image.</param>
        public void RemoveImage(Image image)
        {
            this.OriginalImages.Remove(image);
            this.AverageColorDictionary.Remove(image.AverageColor);
        }

        /// <summary>
        ///     Clears the palette.
        /// </summary>
        public void ClearPalette()
        {
            this.OriginalImages.Clear();
            this.AverageColorDictionary.Clear();
        }



        public Collection<Image> FindMultipleClosestToColor(Color color, int imageCount, IList<Image> disqualified)
        {
            var disqualifiedImages = disqualified.ToList();
            if (disqualifiedImages.Count > this.AverageColorDictionary.Count)
            {
                for (var i = this.AverageColorDictionary.Count - 1; i < disqualifiedImages.Count; i++)
                {
                    disqualifiedImages.RemoveAt(i);
                }
            }

            var images = new Collection<Image>();
            var orderedAvailableColors = this.MostSimilarColorsInDictionaryTo(color);
            
            var currentIndex = 0;
            while (images.Count < imageCount)
            {
                var currentColor = orderedAvailableColors[currentIndex];
                foreach (var image in this.AverageColorDictionary[currentColor])
                {
                    if (images.Count < imageCount && !images.Contains(image) && !disqualifiedImages.Contains(image))
                    {
                        images.Add(image);
                    }
                }

                if (currentIndex < orderedAvailableColors.Length - 1)
                {
                    currentIndex++;
                }
                else if (images.Count == 0)
                {
                    currentIndex = 0;
                    disqualifiedImages.Clear();
                }
                else
                {
                    break;
                }
                
            }

            return images;
        }

        public Color[] MostSimilarColorsInDictionaryTo(Color color)
        {
            var colors = new Collection<Color>(this.AverageColorDictionary.Keys.ToArray());
            var orderedList = colors.OrderBy(key => calculateDifferenceBetweenColors(key, color));
            return orderedList.ToArray();
        }


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