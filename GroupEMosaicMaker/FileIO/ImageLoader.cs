using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using GroupEMosaicMaker.Model;

namespace GroupEMosaicMaker.FileIO
{
    /// <summary>
    /// The class that handles loading images
    /// </summary>
    public class ImageLoader
    {
        #region Data members

        /// <summary>
        /// The palette
        /// </summary>
        public ImagePalette Palette;

        #endregion

        #region Constructors  
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageLoader"/> class.
        /// </summary>
        public ImageLoader()
        {
            this.Palette = new ImagePalette();
        }

        #endregion

        #region Methods        
        /// <summary>
        /// Loads a single image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns> the workable image</returns>
        public async Task<Image> LoadImage(StorageFile image)
        {
            return await this.createWorkableImage(image);
        }

        /// <summary>
        /// Loads a folder of images.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns> the palette created from the folder</returns>
        public async Task<ImagePalette> LoadImages(StorageFolder folder)
        {
            var images = await folder.GetFilesAsync();
            foreach (var image in images)
            {
                var myImage = await this.createWorkableImage(image);
                this.Palette.AddImage(myImage);
            }

            return this.Palette;
        }

        private async Task<Image> createWorkableImage(StorageFile image)
        {
            var copyBitmapImage = await this.makeACopyOfTheFileToWorkOn(image);
            using (var fileStream = await image.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();

                var thumbnail = await this.makeACopyOfTheFileToWorkOn(image);
                return new Image(sourcePixels, decoder, thumbnail);
            }
        }

        private async Task<BitmapImage> makeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputStream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputStream);
            return newImage;
        }

        #endregion
    }
}