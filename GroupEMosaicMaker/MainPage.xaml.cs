﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Capture;
using Windows.Graphics.Imaging;
using Windows.Media.Audio;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using GroupEMosaicMaker.ViewModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GroupEMosaicMaker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Data members

        private double dpiX;
        private double dpiY;
        private WriteableBitmap modifiedImage;

        public MainPageViewModel ViewModel;



        #endregion

        #region Constructors

        public MainPage()
        {
            this.InitializeComponent();

            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;

            ApplicationView.PreferredLaunchViewSize = new Size(bounds.Width, bounds.Height);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            this.modifiedImage = null;
            this.dpiX = 0;
            this.dpiY = 0;

            this.ViewModel = new MainPageViewModel();
            this.DataContext = this.ViewModel;
        }

        #endregion

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            this.saveWritableBitmap();
        }

        private async void saveWritableBitmap()
        {
            var fileSavePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "image"
            };
            fileSavePicker.FileTypeChoices.Add("PNG files", new List<string> { ".png" });
            var savefile = await fileSavePicker.PickSaveFileAsync();

            if (savefile != null)
            {
                var stream = await savefile.OpenAsync(FileAccessMode.ReadWrite);
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                var pixelStream = this.modifiedImage.PixelBuffer.AsStream();
                var pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                    (uint)this.modifiedImage.PixelWidth,
                    (uint)this.modifiedImage.PixelHeight, this.dpiX, this.dpiY, pixels);
                await encoder.FlushAsync();

                stream.Dispose();
            }
        }


        /// <summary>
        ///     Picks the file to open with a file open picker.
        /// </summary>
        /// <returns>
        ///     A <see cref="StorageFile"/>.
        /// </returns>
        public static async Task<StorageFile> PickFileWithOpenPicker()
        {
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            //openPicker.FileTypeFilter.Add(".csv");
            //openPicker.FileTypeFilter.Add(".txt");
            //openPicker.FileTypeFilter.Add(".xml");

            StorageFile file;
            try
            {
                file = await openPicker.PickSingleFileAsync();
            }
            catch (NullReferenceException)
            {
                file = null;
            }

            return file;
        }

        private async void loadButton_Click(object sender, RoutedEventArgs e)
        {
            var sourceImageFile = await this.selectSourceImageFile();
            var copyBitmapImage = await this.MakeACopyOfTheFileToWorkOn(sourceImageFile);

            using (var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform
                {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };

                this.dpiX = decoder.DpiX;
                this.dpiY = decoder.DpiY;

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                    );

                var sourcePixels = pixelData.DetachPixelData();

                //this.giveImageRedTint(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);
                //this.DrawGrid(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);
                this.CreateMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);

                this.modifiedImage = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                using (var writeStream = this.modifiedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    this.imageSource.Source = this.modifiedImage;
                    this.imageDisplay.Source = await this.MakeACopyOfTheFileToWorkOn(sourceImageFile);

                }
            }
        }

        private void giveImageRedTint(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        {
            for (var i = 0; i < imageHeight; i++)
            {
                for (var j = 0; j < imageWidth; j++)
                {
                    var pixelColor = this.getPixelBgra8(sourcePixels, i, j, imageWidth, imageHeight);
                    pixelColor.R = 255;
   
                    this.setPixelBgra8(sourcePixels, i, j, pixelColor, imageWidth, imageHeight);
                }
            }
            
        }


        public void DrawGrid(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        {

            for (var i = 0; i < imageHeight; i++)
            {
                for (var j = 0; j < imageWidth; j += Convert.ToInt32(this.blockSizeTextBox.Text))
                {
                    var pixelColor = this.getPixelBgra8(sourcePixels, i, j, imageWidth, imageHeight);
                    pixelColor.R = 255;
                    pixelColor.B = 255;
                    pixelColor.G = 255;
                    this.setPixelBgra8(sourcePixels, i, j, pixelColor, imageWidth, imageHeight);
                }
            }

            for (var i = 0; i < imageHeight; i += Convert.ToInt32(this.blockSizeTextBox.Text))
            {
                for (var j = 0; j < imageWidth; j++)
                {
                    var pixelColor = this.getPixelBgra8(sourcePixels, i, j, imageWidth, imageHeight);
                    pixelColor.R = 255;
                    pixelColor.B = 255;
                    pixelColor.G = 255;
                    this.setPixelBgra8(sourcePixels, i, j, pixelColor, imageWidth, imageHeight);
                }
            }

        }

        public void CreateMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        {
            var currentPixel = 0;
            var currentPixelMax = Convert.ToInt32(this.blockSizeTextBox.Text);
            for (var blockHeight = 0; blockHeight < (Convert.ToDouble(imageHeight) / Convert.ToDouble(this.blockSizeTextBox.Text)); blockHeight ++)
            {
                for (var blockWidth = 0;
                    blockWidth < (Convert.ToDouble(imageWidth) / Convert.ToDouble(this.blockSizeTextBox.Text));
                    blockWidth++)
                {
                    var totalRed = 0;
                    var totalBlue = 0;
                    var totalGreen = 0;
                    var pixelCounter = 0;

                    for (var i = currentPixel; i < currentPixelMax; i++)
                    {
                        for (var j = currentPixel; j < currentPixelMax; j++)
                        {
                            var pixelColor = this.getPixelBgra8(sourcePixels, i, j, imageWidth, imageHeight);
                            totalRed += pixelColor.R;
                            totalBlue += pixelColor.B;
                            totalGreen += pixelColor.G;
                            pixelCounter++;
                        }
                    }

                    for (var i = currentPixel; i < currentPixelMax; i++)
                    {
                        for (var j = currentPixel; j < currentPixelMax; j++)
                        {
                            var pixelColor = this.getPixelBgra8(sourcePixels, i, j, imageWidth, imageHeight);
                            pixelColor.R = BitConverter.GetBytes(totalRed / pixelCounter)[0];
                            pixelColor.B = BitConverter.GetBytes(totalBlue / pixelCounter)[0];
                            pixelColor.G = BitConverter.GetBytes(totalGreen / pixelCounter)[0];
                            this.setPixelBgra8(sourcePixels, i ,j, pixelColor, imageWidth, imageHeight);
                        }
                    }

                    currentPixel += Convert.ToInt32(this.blockSizeTextBox.Text);
                    currentPixelMax += Convert.ToInt32(this.blockSizeTextBox.Text);

                }
            }

        }



        private async Task<StorageFile> selectSourceImageFile()
        {
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");

            var file = await openPicker.PickSingleFileAsync();

            return file;
        }

        private async Task<BitmapImage> MakeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputstream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputstream);
            return newImage;
        }

        private Color getPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int)width + y) * 4;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        private void setPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height)
        {
            var offset = (x * (int)width + y) * 4;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }
    }
}
