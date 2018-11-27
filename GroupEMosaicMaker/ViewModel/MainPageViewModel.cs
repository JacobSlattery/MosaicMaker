using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using GroupEMosaicMaker.Model;
using GroupEMosaicMaker.Utility;

namespace GroupEMosaicMaker.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {

        private Image sourceImage;
        private Image displayImage;
        private WriteableBitmap resultImage;
        private bool grid;
        private int blockSize;
        private ImageManipulator manipulator;
        private double dpiX;
        private double dpiY;


        public Image SourceImage
        {
            get => this.sourceImage;
            set
            {
                this.sourceImage = value;
                this.OnPropertyChanged();
            }
        }

        public Image DisplayImage
        {
            get => this.displayImage;
            set
            {
                this.displayImage = value;
                this.OnPropertyChanged();
            }
        }

        public WriteableBitmap ResultImage
        {
            get => this.resultImage;
            set
            {
                this.resultImage = value;
                this.OnPropertyChanged();
            }
        }

        public bool Grid
        {
            get => this.grid;
            set
            {
                this.grid = value;
                this.OnPropertyChanged();
            }
        }

        public int BlockSize
        {
            get => this.blockSize;
            set
            {
                this.blockSize = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the save to file command.
        /// </summary>
        public RelayCommand SaveToFileCommand { get; set; }

        /// <summary>
        ///     Gets or sets the load file command.
        /// </summary>
        public RelayCommand LoadFileCommand { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MainPageViewModel" /> class.
        /// </summary>
        public MainPageViewModel()
        {
            this.loadCommands();
            this.loadProperties();
            this.manipulator = new ImageManipulator();
        }

        private void loadCommands()
        {
            this.LoadFileCommand = new RelayCommand(this.loadFile, this.canLoadFile);
            this.SaveToFileCommand = new RelayCommand(this.saveToFile, this.canSaveToFile);
        }

        private void loadProperties()
        {
            this.DisplayImage = new Image();
            this.ResultImage = null;
            this.SourceImage = new Image();
            this.BlockSize = 100;
        }

        private async void loadFile(object obj)
        {
            var file = await MainPage.SelectSourceImageFile();

            if (file != null)
            {
                await this.handleNewImage(file);
            }
        }

        private async Task handleNewImage(StorageFile file)
        {
            var sourceImageFile = file;
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
                this.DrawGrid(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);
                //this.CreateMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);

                this.ResultImage = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                using (var writeStream = this.ResultImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    this.SourceImage.Source = this.ResultImage;
                    this.DisplayImage.Source = await this.MakeACopyOfTheFileToWorkOn(sourceImageFile);
                    this.OnPropertyChanged(nameof(this.DisplayImage));
                }
            }
        }

        public void DrawGrid(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        {

            for (var i = 0; i < imageHeight; i++)
            {
                for (var j = 0; j < imageWidth; j++)
                {
                    if (j % 100 == 0)
                    {
                        var pixelColor = Color.FromArgb(255, 255, 255, 255);
                        this.setPixelBgra8(sourcePixels, i, j, pixelColor, imageWidth, imageHeight);
                    }
                }
            }

            //for (var i = 0; i < imageHeight; i++)
            //{
            //    if (i % this.blockSize == 0)
            //    {
            //        for (var j = 0; j < imageWidth; j++)
            //        {
            //            //var pixelColor = this.getPixelBgra8(sourcePixels, i, j, imageWidth, imageHeight);
            //            var pixelColor = Color.FromArgb(255, 255, 255, 255);
            //            this.setPixelBgra8(sourcePixels, i, j, pixelColor, imageWidth, imageHeight);
            //        }
            //    }
            //}

        }

        private async Task<BitmapImage> MakeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputstream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputstream);
            return newImage;
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

        //public void CreateMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        //{
        //    var currentPixel = 0;
        //    var currentPixelMax = Convert.ToInt32(this.blockSizeTextBox.Text);
        //    for (var blockHeight = 0; blockHeight < (Convert.ToDouble(imageHeight) / Convert.ToDouble(this.blockSizeTextBox.Text)); blockHeight++)
        //    {
        //        for (var blockWidth = 0;
        //            blockWidth < (Convert.ToDouble(imageWidth) / Convert.ToDouble(this.blockSizeTextBox.Text));
        //            blockWidth++)
        //        {
        //            var totalRed = 0;
        //            var totalBlue = 0;
        //            var totalGreen = 0;
        //            var pixelCounter = 0;

        //            for (var i = currentPixel; i < currentPixelMax; i++)
        //            {
        //                for (var j = currentPixel; j < currentPixelMax; j++)
        //                {
        //                    var pixelColor = this.getPixelBgra8(sourcePixels, i, j, imageWidth, imageHeight);
        //                    totalRed += pixelColor.R;
        //                    totalBlue += pixelColor.B;
        //                    totalGreen += pixelColor.G;
        //                    pixelCounter++;
        //                }
        //            }

        //            for (var i = currentPixel; i < currentPixelMax; i++)
        //            {
        //                for (var j = currentPixel; j < currentPixelMax; j++)
        //                {
        //                    var pixelColor = this.getPixelBgra8(sourcePixels, i, j, imageWidth, imageHeight);
        //                    pixelColor.R = BitConverter.GetBytes(totalRed / pixelCounter)[0];
        //                    pixelColor.B = BitConverter.GetBytes(totalBlue / pixelCounter)[0];
        //                    pixelColor.G = BitConverter.GetBytes(totalGreen / pixelCounter)[0];
        //                    this.setPixelBgra8(sourcePixels, i, j, pixelColor, imageWidth, imageHeight);
        //                }
        //            }

        //            currentPixel += Convert.ToInt32(this.blockSizeTextBox.Text);
        //            currentPixelMax += Convert.ToInt32(this.blockSizeTextBox.Text);

        //        }
        //    }

        //}

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
            pixels[offset + 3] = color.A;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }


        private void saveToFile(object obj)
        {

        }

        private bool canLoadFile(object obj)
        {
            return true;
        }

        private bool canSaveToFile(object obj)
        {
            return true;
        }

    }
}
