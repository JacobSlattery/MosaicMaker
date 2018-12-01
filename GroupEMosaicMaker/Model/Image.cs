using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupEMosaicMaker.Model
{
    public class Image
    {
        #region Properties

        public byte[] SourcePixels { get; set; }
        public BitmapDecoder Decoder { get; set; }

        public BitmapImage Thumbnail { get; set; }

        #endregion

        #region Constructors

        public Image(byte[] pixels, BitmapDecoder decoder, BitmapImage thumbnail)
        {
            this.SourcePixels = pixels;
            this.Decoder = decoder;
            this.Thumbnail = thumbnail;
        }

        #endregion
    }
}