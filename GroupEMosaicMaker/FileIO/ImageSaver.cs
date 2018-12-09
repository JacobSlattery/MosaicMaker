using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using GroupEMosaicMaker.Model;

namespace GroupEMosaicMaker.FileIO
{
    /// <summary>
    ///     the class that handles saving images
    /// </summary>
    public class ImageSaver
    {
        #region Methods

        /// <summary>
        ///     Saves the image.
        /// </summary>
        /// <param name="saveFile">The save file.</param>
        /// <param name="bitMapToSave">The bit map to save.</param>
        /// <param name="imageData">The image data.</param>
        public static async Task SaveImage(StorageFile saveFile, WriteableBitmap bitMapToSave, Image imageData)
        {
            var stream = await saveFile.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

            var pixelStream = bitMapToSave.PixelBuffer.AsStream();
            var pixels = new byte[pixelStream.Length];
            await pixelStream.ReadAsync(pixels, 0, pixels.Length);

            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                (uint) bitMapToSave.PixelWidth,
                (uint) bitMapToSave.PixelHeight, imageData.Decoder.DpiX, imageData.Decoder.DpiY, pixels);
            await encoder.FlushAsync();

            stream.Dispose();
        }

        #endregion
    }
}