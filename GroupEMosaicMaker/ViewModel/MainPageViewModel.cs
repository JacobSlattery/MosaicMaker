using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using GroupEMosaicMaker.FileIO;
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
        private ImageManipulator manipulatorForGridImage;
        private ImageManipulator manipulatorForResultImage;
        private readonly ImageStorage imageStorageForGrid;
        private readonly ImageStorage imageStorageForMosaic;

        private WriteableBitmap currentImage;
        private WriteableBitmap currentImageWithGrid;

        #endregion

        #region Properties

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
                    this.manipulatorForGridImage = new ImageManipulator(this.imageStorageForGrid.Decoder.PixelWidth,
                        this.imageStorageForGrid.Decoder.PixelHeight, this.imageStorageForGrid.SourcePixels);
                    this.manipulatorForGridImage.DrawGrid(this.BlockSize);
                    this.currentImageWithGrid = new WriteableBitmap((int) this.imageStorageForGrid.Decoder.PixelWidth,
                        (int) this.imageStorageForGrid.Decoder.PixelHeight);
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

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MainPageViewModel" /> class.
        /// </summary>
        public MainPageViewModel()
        {
            this.loadCommands();
            this.loadProperties();
            this.imageStorageForGrid = new ImageStorage();
            this.imageStorageForMosaic = new ImageStorage();
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
            await this.imageStorageForGrid.CreateImage(this.SourceFile);

            var width = (int) this.imageStorageForGrid.Decoder.PixelWidth;
            var height = (int) this.imageStorageForGrid.Decoder.PixelHeight;
            var pixels = this.imageStorageForGrid.SourcePixels;
            this.currentImage = new WriteableBitmap(width, height);
            this.writeStreamOfPixels(this.currentImage, pixels);

            this.manipulatorForGridImage = new ImageManipulator((uint) width,
                (uint) height, pixels);

            this.manipulatorForGridImage.DrawGrid(this.BlockSize);
            this.currentImageWithGrid = new WriteableBitmap(width, height);
            this.writeStreamOfPixels(this.currentImageWithGrid, this.manipulatorForGridImage.RetrieveModifiedPixels());
            this.updateDisplayImage();
        }

        private async Task handleCreatingMosaic()
        {
            await this.imageStorageForMosaic.CreateImage(this.SourceFile);

            var width = (int) this.imageStorageForMosaic.Decoder.PixelWidth;
            var height = (int) this.imageStorageForMosaic.Decoder.PixelHeight;
            var pixels = this.imageStorageForMosaic.SourcePixels;
            this.manipulatorForResultImage = new ImageManipulator((uint) width, (uint) height, pixels);
            this.manipulatorForResultImage.CreateMosaic(this.BlockSize);

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
                await ImageSaver.SaveImage(saveFile, this.ResultImage, this.imageStorageForMosaic);
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