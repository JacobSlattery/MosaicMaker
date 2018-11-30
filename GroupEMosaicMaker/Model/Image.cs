using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupEMosaicMaker.Model
{
    public class Image
    {
        public byte[] SourcePixels { get; set; }
        public BitmapDecoder Decoder { get; set; }

        public BitmapImage Thumbnail { get; set; }

        public Image(byte[] pixels, BitmapDecoder decoder, BitmapImage thumbnail)
        {
            this.SourcePixels = pixels;
            this.Decoder = decoder;
            this.Thumbnail = thumbnail;
        }
    }
}
