using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;

namespace GroupEMosaicMaker.Model
{
    public class ImagePalette
    {
        #region Data members

        public ICollection<Image> OriginalImages;
        public ICollection<Image> AlteredImages;



        #endregion

        #region Constructors

        public ImagePalette()
        {
            this.OriginalImages = new Collection<Image>();
            this.AlteredImages = new Collection<Image>();
        }

        #endregion

        #region Methods

        public void AddImage(Image image)
        {
            this.OriginalImages.Add(image);
        }

        public void ClearPalette()
        {
            this.OriginalImages.Clear();
        }

        public Dictionary<Color, Image> FindAverageColorsForImagesInPalette()
        {
            this.AlteredImages = this.OriginalImages;
            var averageColorsInPalette = new Dictionary<Color, Image>();

            foreach (var image in this.AlteredImages)
            {
                var indexes = IndexMapper.Box(0, 50, 50, 50);
                IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                var color = Painter.GetAverageColor(image.SourcePixels, indexes);
                averageColorsInPalette.Add(color, image);
            }

            return averageColorsInPalette;
        }

        #endregion
    }
}