using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;

namespace GroupEMosaicMaker.Model
{
    public class ImagePalette
    {
        #region Data members

        public ICollection<Image> Images;

        #endregion

        #region Constructors

        public ImagePalette()
        {
            this.Images = new Collection<Image>();
        }

        #endregion

        #region Methods

        public void AddImage(Image image)
        {
            this.Images.Add(image);
        }

        public Dictionary<Color, Image> FindAverageColorsForImagesInPalette()
        {
            var averageColorsInPalette = new Dictionary<Color, Image>();

            foreach (var image in this.Images)
            {
                var indexes = IndexMapper.CalculateIndexBox(0, 50, 50, 50, 50);
                IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                var color = Panel.getPanelAverageColor(image.SourcePixels, indexes);
                averageColorsInPalette.Add(color, image);
            }

            return averageColorsInPalette;
        }

        #endregion
    }
}