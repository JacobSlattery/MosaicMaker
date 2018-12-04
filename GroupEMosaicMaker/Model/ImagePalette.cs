using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;

namespace GroupEMosaicMaker.Model
{
    public class ImagePalette
    {
        #region Data members

        public ICollection<Image> OriginalImages;

        public IDictionary<Color, Image> AverageColorDictionary;



        #endregion

        #region Constructors

        public ImagePalette()
        {
            this.OriginalImages = new Collection<Image>();
            this.AverageColorDictionary = new Dictionary<Color, Image>();
        }

        #endregion

        #region Methods

        public void AddImage(Image image)
        {
            this.OriginalImages.Add(image);
            this.FindAverageColorsForImagesInPalette(image);
        }

        public void ClearPalette()
        {
            this.OriginalImages.Clear();
        }

        public void FindAverageColorsForImagesInPalette(Image image)
        {
           // var averageColorsInPalette = new Dictionary<Color, Image>();

         //   foreach (var image in this.OriginalImages)
           // {
                var indexes = IndexMapper.Box(0, 50, 50, 50);
                IndexMapper.ConvertEachIndexToMatchOffset(indexes, 4);
                var color = Painter.GetAverageColor(image.SourcePixels, indexes);
               // averageColorsInPalette.Add(color, image);
            this.AverageColorDictionary.Add(color, image);
          //  }

           // return averageColorsInPalette;
        }

        #endregion
    }
}