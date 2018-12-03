using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using GroupEMosaicMaker.Extension;
using GroupEMosaicMaker.FileIO;
using GroupEMosaicMaker.Model;
using GroupEMosaicMaker.Utility;
using System;

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
        private ImageManipulator manipulatorForGridImage;
        private ImageManipulator manipulatorForResultImage;
        //private readonly ImageStorage imageStorageForGrid;
        //private readonly ImageStorage imageStorageForMosaic;
        private readonly ImageLoader imageLoader;
        private Image imageWithGrid;
        private Image imageWithMosaic;

        private WriteableBitmap currentImage;
        private WriteableBitmap currentImageWithGrid;
        private ObservableCollection<Image> imagePalette;
        private Image selectedImage;
        private ImagePalette palette;

        #endregion

        #region Properties

        public Image SelectedImage
        {
            get => this.selectedImage;
            set
            {
                this.selectedImage = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<Image> ImagePalette
        {
            get => this.imagePalette;
            set
            {
                this.imagePalette = value;
                this.OnPropertyChanged();
            }
        }

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
                this.ConvertCommand.OnCanExecuteChanged();
                this.OnPropertyChanged();
            }
        }

        public WriteableBitmap ResultImage
        {
            get => this.resultImage;
            set
            {
                this.resultImage = value;
                this.SaveToFileCommand.OnCanExecuteChanged();
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
                if (this.SourceFile != null)
                {
                    this.manipulatorForGridImage = new ImageManipulator(this.imageWithGrid.Decoder.PixelWidth,
                    this.imageWithGrid.Decoder.PixelHeight, this.imageWithGrid.SourcePixels);
                    this.manipulatorForGridImage.DrawGrid(this.BlockSize);
                    this.currentImageWithGrid = new WriteableBitmap((int) this.imageWithGrid.Decoder.PixelWidth, (int) this.imageWithGrid.Decoder.PixelHeight);
                    this.writeStreamOfPixels(this.currentImageWithGrid,
                    this.manipulatorForGridImage.RetrieveModifiedPixels());
                    this.updateDisplayImage();
                }
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

        public RelayCommand ConvertCommand { get; set; }

        public RelayCommand LoadImagePaletteCommand { get; set; }

        public RelayCommand PictureMosaicCommand { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MainPageViewModel" /> class.
        /// </summary>
        public MainPageViewModel()
        {
            this.loadCommands();
            this.loadProperties();
            //this.imageStorageForGrid = new ImageStorage();
            // this.imageStorageForMosaic = new ImageStorage();
            this.imageLoader = new ImageLoader();
            this.imageWithGrid = null;
            this.imageWithMosaic = null;
            this.imagePalette = new ObservableCollection<Image>();
            this.palette = new ImagePalette();
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
            this.ConvertCommand = new RelayCommand(this.convert, this.canConvert);
            this.LoadImagePaletteCommand = new RelayCommand(this.loadPalette, this.canLoadPalette);
            this.PictureMosaicCommand = new RelayCommand(this.createPictureMosaic, this.canCreatePictureMosaic);
        }

        private bool canCreatePictureMosaic(object obj)
        {
            return true;
        }

        private void createPictureMosaic(object obj)
        {
            this.manipulatorForResultImage.CreatePictureMosaic(this.BlockSize, this.palette);
        }

        private bool canLoadPalette(object obj)
        {
            return this.imagePalette != null;
        }

        private async void loadPalette(object obj)
        {
            var folder = await MainPage.SelectImagePaletteFolder();
            var palette = await this.imageLoader.LoadImages(folder);
            this.palette = palette;
            this.ImagePalette = palette.Images.ToObservableCollection();
        }

        private void loadProperties()
        {
            this.DisplayImage = null;
            this.currentImageWithGrid = null;
            this.currentImage = null;
            this.ResultImage = null;
            this.SourceFile = null;
            this.BlockSize = 5;
            this.Grid = false;
        }

        private bool canConvert(object obj)
        {
            return this.DisplayImage != null;
        }

        private async void convert(object obj)
        {
            await this.handleCreatingMosaic();
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
            // await this.imageStorageForGrid.CreateImage(this.SourceFile);
            this.imageWithGrid = await this.imageLoader.LoadImage(this.SourceFile);

            // var width = (int) this.imageStorageForGrid.Decoder.PixelWidth;
            //   var height = (int) this.imageStorageForGrid.Decoder.PixelHeight;
            // var pixels = this.imageStorageForGrid.SourcePixels;
            var width = (int) this.imageWithGrid.Decoder.PixelWidth;
            var height = (int) this.imageWithGrid.Decoder.PixelHeight;
            var pixels = this.imageWithGrid.SourcePixels;
            this.currentImage = new WriteableBitmap(width, height);
            this.writeStreamOfPixels(this.currentImage, pixels);

            this.manipulatorForGridImage = new ImageManipulator((uint) width,
                (uint) height, pixels);
            this.manipulatorForResultImage = new ImageManipulator((uint)width, (uint)height, pixels);

            this.manipulatorForGridImage.DrawGrid(this.BlockSize);
            this.currentImageWithGrid = new WriteableBitmap(width, height);
            this.writeStreamOfPixels(this.currentImageWithGrid, this.manipulatorForGridImage.RetrieveModifiedPixels());
            this.updateDisplayImage();
        }

        private async Task handleCreatingMosaic()
        {
            //  await this.imageStorageForMosaic.CreateImage(this.SourceFile);
            this.imageWithMosaic = await this.imageLoader.LoadImage(this.SourceFile);

            //var width = (int) this.imageStorageForMosaic.Decoder.PixelWidth;
            //var height = (int) this.imageStorageForMosaic.Decoder.PixelHeight;
            //var pixels = this.imageStorageForMosaic.SourcePixels;
            var width = (int) this.imageWithMosaic.Decoder.PixelWidth;
            var height = (int) this.imageWithMosaic.Decoder.PixelHeight;
           // var pixels = this.imageWithMosaic.SourcePixels;

           // this.manipulatorForResultImage = new ImageManipulator((uint) width, (uint) height, pixels);
            this.manipulatorForResultImage.CreateSolidBlockMosaic(this.BlockSize);

            this.ResultImage = new WriteableBitmap(width, height);
            this.writeStreamOfPixels(this.ResultImage, this.manipulatorForResultImage.RetrieveModifiedPixels());
        }

        private async void writeStreamOfPixels(WriteableBitmap bitMap, byte[] sourcePixels)
        {
            using (var writeStream = bitMap.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
            }
        }

        private async void saveWritableBitmap()
        {
            var saveFile = await MainPage.SelectSaveImageFile();
            if (saveFile != null)
            {
                await ImageSaver.SaveImage(saveFile, this.ResultImage, this.imageWithMosaic);
            }
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
            return this.ResultImage != null;
        }

        #endregion
    }
}