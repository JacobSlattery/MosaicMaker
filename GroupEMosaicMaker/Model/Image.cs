using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace GroupEMosaicMaker.Model
{
    /// <summary>
    /// The class that keeps track of the properties for a image object and performs actions on the image
    /// </summary>
    public class Image
    {
        #region Properties

        /// <summary>
        /// Gets or sets the source pixels.
        /// </summary>
        /// <value>
        /// The source pixels.
        /// </value>
        public byte[] SourcePixels { get; set; }

        /// <summary>
        /// Gets or sets the decoder.
        /// </summary>
        /// <value>
        /// The decoder.
        /// </value>
        public BitmapDecoder Decoder { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail.
        /// </summary>
        /// <value>
        /// The thumbnail.
        /// </value>
        public BitmapImage Thumbnail { get; set; }

        /// <summary>
        /// Gets or sets the modified pixels.
        /// </summary>
        /// <value>
        /// The modified pixels.
        /// </value>
        public byte[] ModifiedPixels { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <param name="decoder">The decoder.</param>
        /// <param name="thumbnail">The thumbnail.</param>
        public Image(byte[] pixels, BitmapDecoder decoder, BitmapImage thumbnail)
        {
            this.SourcePixels = pixels;
            this.Decoder = decoder;
            this.Thumbnail = thumbnail;
        }


        #endregion

        /// <summary>
        /// Resizes the image.
        /// </summary>
        /// <param name="size">The size.</param>
        public async Task ResizeImage(int size)
        {
            var transform = new BitmapTransform {
                ScaledWidth = Convert.ToUInt32(size),
                ScaledHeight = Convert.ToUInt32(size) 
            };

            var pixelData = await this.Decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage
            );

            this.ModifiedPixels = pixelData.DetachPixelData();

        }
    }
}