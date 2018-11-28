using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using GroupEMosaicMaker.Model;

namespace GroupEMosaicMaker.FileIO
{
    public class ImageSaver
    {
        public static async Task SaveImage(StorageFile saveFile, WriteableBitmap bitMapToSave, ImageStorage imageData)
        {
            var stream = await saveFile.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

            var pixelStream = bitMapToSave.PixelBuffer.AsStream();
            var pixels = new byte[pixelStream.Length];
            await pixelStream.ReadAsync(pixels, 0, pixels.Length);

            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                (uint)bitMapToSave.PixelWidth,
                (uint)bitMapToSave.PixelHeight, imageData.Decoder.DpiX, imageData.Decoder.DpiY, pixels);
            await encoder.FlushAsync();

            stream.Dispose();
        }
    }
}
