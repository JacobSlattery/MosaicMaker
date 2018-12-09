using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;

namespace GroupEMosaicMaker.Model
{
    /// <summary>
    ///     Keeps track of a list of images and performs actions on them
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
        ///     Removes the image.
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

        public void ChangeToNewCollection(ICollection<Image> images)
        {
            this.AverageColorDictionary.Clear();
            this.OriginalImages.Clear();
            foreach (var image in images)
            {
                this.AddImage(image);
            }
        }

        public Collection<Image> FindMultipleImagesClosestToColor(Color color, int imageCount,
            ICollection<Image> disqualified)
        {
            var disqualifiedImages = this.ensureDisqualifiedIsNotTooRestrictive(disqualified);

            var images = new Collection<Image>();
            var orderedAvailableColors = this.MostSimilarColorsInDictionaryTo(color);

            var currentIndex = 0;
            while (images.Count < imageCount)
            {
                var currentColor = orderedAvailableColors[currentIndex];
                this.addImagesCloseToColor(imageCount, currentColor, images, disqualifiedImages);

                if (currentIndex < orderedAvailableColors.Length - 1)
                {
                    currentIndex++;
                }
                else if (images.Count == 0 && disqualifiedImages.Count > 0)
                {
                    currentIndex = 0;
                    disqualifiedImages.RemoveAt(0);
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

        private static double calculateDifferenceBetweenColors(Color averageColor, Color color)
        {
            return Math.Pow((color.R - averageColor.R) * .3, 2) +
                   Math.Pow((color.G - averageColor.G) * .59, 2) +
                   Math.Pow((color.B - averageColor.B) * .11, 2);
        }

        private List<Image> ensureDisqualifiedIsNotTooRestrictive(IEnumerable<Image> disqualified)
        {
            var disqualifiedImages = disqualified.ToList();
            if (disqualifiedImages.Count > this.AverageColorDictionary.Count && this.AverageColorDictionary.Count != 0)
            {
                for (var i = this.AverageColorDictionary.Count - 1; i < disqualifiedImages.Count; i++)
                {
                    disqualifiedImages.RemoveAt(i);
                }
            }

            return disqualifiedImages;
        }

        private void addImagesCloseToColor(int imageCount, Color currentColor, ICollection<Image> images,
            ICollection<Image> disqualifiedImages)
        {
            foreach (var image in this.AverageColorDictionary[currentColor])
            {
                if (images.Count < imageCount && !images.Contains(image) && !disqualifiedImages.Contains(image))
                {
                    images.Add(image);
                }
            }
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