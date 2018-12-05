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
        public IDictionary<Color, Image> AverageColorDictionary;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImagePalette" /> class.
        /// </summary>
        public ImagePalette()
        {
            this.OriginalImages = new Collection<Image>();
            this.AverageColorDictionary = new Dictionary<Color, Image>();
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
            this.FindAverageColorsForImagesInPalette(image);
        }

        public int FindNumberOfImagesInPalette()
        {
            return this.OriginalImages.Count;
        }

        /// <summary>
        ///     Clears the palette.
        /// </summary>
        public void ClearPalette()
        {
            this.OriginalImages.Clear();
        }

        /// <summary>
        ///     Finds the average colors for images in palette.
        /// </summary>
        /// <param name="image">The image.</param>
        public void FindAverageColorsForImagesInPalette(Image image)
        {
            var indexes = IndexMapper.Box(0, 50, 50, 50);
            IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
            var color = Painter.GetAverageColor(image.SourcePixels, indexes);

            this.AverageColorDictionary.Add(color, image);
            
        }

        #endregion
    }
}