using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        #endregion
    }
}