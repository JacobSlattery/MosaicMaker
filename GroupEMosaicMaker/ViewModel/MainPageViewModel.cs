using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
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
        #region Data members

        private StorageFile sourceFile;
        private WriteableBitmap displayImage;
        private WriteableBitmap resultImage;
        private bool grid;
        private int blockSize;
        private ImageManipulator manipulator;
        private double dpiX;
        private double dpiY;

        #endregion

        #region Properties

        private WriteableBitmap currentImage;
        public WriteableBitmap currentImageWithGrid;

        public StorageFile SourceFile
        {
            get => this.sourceFile;
            set
            {
                this.sourceFile = value;
                this.OnPropertyChanged();
            }
        }

        public WriteableBitmap DisplayImage
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
                if (this.grid != value)
                {
                    this.grid = value;
                    this.updateDisplayImage();
                    this.OnPropertyChanged();
                }
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

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MainPageViewModel" /> class.
        /// </summary>
        public MainPageViewModel()
        {
            this.loadCommands();
            this.loadProperties();
            this.manipulator = new ImageManipulator();
        }

        #endregion

        #region Methods

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void loadCommands()
        {
            this.LoadFileCommand = new RelayCommand(this.loadFile, this.canLoadFile);
            this.SaveToFileCommand = new RelayCommand(this.saveToFile, this.canSaveToFile);
        }

        private void loadProperties()
        {
            this.DisplayImage = null;
            this.currentImageWithGrid = null;
            this.currentImage = null;
            this.ResultImage = null;
            this.SourceFile = null;
            this.BlockSize = 100;
            this.Grid = false;
            
        }

        private async void loadFile(object obj)
        {
            var file = await MainPage.SelectSourceImageFile();

            if (file != null)
            {
                this.SourceFile = file;
                await this.handleNewImageFile();
            }
        }

        private void updateDisplayImage()
        {
            if (this.Grid)
            {
                this.DisplayImage = this.currentImageWithGrid;
            }
            else
            {
                this.DisplayImage = this.currentImage;
            }
        }

        private async Task handleNewImageFile()
        {
            var copyBitmapImage = await this.MakeACopyOfTheFileToWorkOn(this.SourceFile);
            var copyBitmapImage2 = await this.MakeACopyOfTheFileToWorkOn(this.SourceFile);
            using (var fileStream = await this.SourceFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform {
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

                
                this.currentImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                await this.writeStreamOfPixels(this.currentImage, sourcePixels);


                ImageManipulator.DrawGrid(sourcePixels, decoder.PixelWidth, decoder.PixelHeight, this.BlockSize);
                this.currentImageWithGrid = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);

                await this.writeStreamOfPixels(this.currentImageWithGrid, sourcePixels);
               
                this.updateDisplayImage();
            }
            await this.handleCreatingMosaic(copyBitmapImage2);
        }

        private async Task handleCreatingMosaic(BitmapImage copyBitmapImage2)
        {
            using (var fileStream = await this.SourceFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage2.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage2.PixelHeight)
                };

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();

                ImageManipulator.CreateMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight, this.BlockSize);
                this.ResultImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                await this.writeStreamOfPixels(this.ResultImage, sourcePixels);
            }
        }

        private async Task writeStreamOfPixels(WriteableBitmap bitMap, byte[] sourcePixels)
        {
            using (var writeStream = bitMap.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
            }
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

                var pixelStream = this.DisplayImage.PixelBuffer.AsStream();
                var pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                    (uint)this.DisplayImage.PixelWidth,
                    (uint)this.DisplayImage.PixelHeight, this.dpiX, this.dpiY, pixels);
                await encoder.FlushAsync();

                stream.Dispose();
            }
        }

        private Color getPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        private void setPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            pixels[offset + 3] = color.A;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }

        private void saveToFile(object obj)
        {
            this.saveWritableBitmap();
        }

        private bool canLoadFile(object obj)
        {
            return true;
        }

        private bool canSaveToFile(object obj)
        {
            return true;
        }

        #endregion
    }
}