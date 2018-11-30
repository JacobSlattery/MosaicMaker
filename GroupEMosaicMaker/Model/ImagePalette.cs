using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupEMosaicMaker.Model
{
    public class ImagePalette
    {
        public ICollection<Image> Images;

        public ImagePalette()
        {
            this.Images = new Collection<Image>();
        }
        public void AddImage(Image image)
        {
            this.Images.Add(image);
        }
    }
}
