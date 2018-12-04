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

namespace GroupEMosaicMaker.ViewModel
{
    /// <summary>
    /// The main paige view model
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class MainPageViewModel : INotifyPropertyChanged
    {
        #region Data members

        private StorageFile sourceFile;
        private WriteableBitmap displayImage;
        private WriteableBitmap resultImage;
        private bool grid;
        private bool squareMosaic;
        private bool triangleMosaic;
        private bool pictureMosaic;
        private int blockSize;
        private int sliderMaximum;
        private ImageManipulator manipulatorForGridImage;
        private ImageManipulator manipulatorForResultImage;
        private readonly ImageLoader imageLoader;
        private Image imageWithGrid;
        private Image imageWithMosaic;

        private WriteableBitmap currentImage;
        private WriteableBitmap currentImageWithGrid;
        private ObservableCollection<Image> imagePalette;
        private Image selectedImage;
        private ImagePalette palette;

        private bool zoomImage;
        private bool scaleImage;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [zoom image].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [zoom image]; otherwise, <c>false</c>.
        /// </value>
        public bool ZoomImage
        {
            get => this.zoomImage;
            set
            {
                this.zoomImage = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [scale image].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [scale image]; otherwise, <c>false</c>.
        /// </value>
        public bool ScaleImage
        {
            get => this.scaleImage;
            set
            {
                this.scaleImage = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the selected image.
        /// </summary>
        /// <value>
        /// The selected image.
        /// </value>
        public Image SelectedImage
        {
            get => this.selectedImage;
            set
            {
                this.selectedImage = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the image palette.
        /// </summary>
        /// <value>
        /// The image palette.
        /// </value>
        public ObservableCollection<Image> ImagePalette
        {
            get => this.imagePalette;
            set
            {
                this.imagePalette = value;
                this.PictureMosaicCommand.OnCanExecuteChanged();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the source file.
        /// </summary>
        /// <value>
        /// The source file.
        /// </value>
        public StorageFile SourceFile
        {
            get => this.sourceFile;
            set
            {
                this.sourceFile = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the display image.
        /// </summary>
        /// <value>
        /// The display image.
        /// </value>
        public WriteableBitmap DisplayImage
        {
            get => this.displayImage;
            set
            {
                this.displayImage = value;
                this.ConvertCommand.OnCanExecuteChanged();
                this.PictureMosaicCommand.OnCanExecuteChanged();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the result image.
        /// </summary>
        /// <value>
        /// The result image.
        /// </value>
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

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MainPageViewModel"/> is grid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if grid; otherwise, <c>false</c>.
        /// </value>
        public bool Grid
        {
            get => this.grid;
            set
            {
                this.grid = value;
                if (this.SourceFile != null)
                {
                    this.updateGrid();
                    this.updateDisplayImage();
                }
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [square mosaic].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [square mosaic]; otherwise, <c>false</c>.
        /// </value>
        public bool SquareMosaic
        {
            get => this.squareMosaic;
            set
            {
                this.squareMosaic = value;
                if (this.SourceFile != null)
                {
                    this.updateGrid();
                    this.updateDisplayImage();
                }
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [triangle mosaic].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [triangle mosaic]; otherwise, <c>false</c>.
        /// </value>
        public bool TriangleMosaic
        {
            get => this.triangleMosaic;
            set
            {
                this.triangleMosaic = value;
                if (this.SourceFile != null)
                {
                    this.updateGrid();
                    this.updateDisplayImage();
                }
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [picture mosaic].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [picture mosaic]; otherwise, <c>false</c>.
        /// </value>
        public bool PictureMosaic
        {
            get => this.pictureMosaic;
            set
            {
                this.pictureMosaic = value;
                if (this.SourceFile != null)
                {
                    this.updateGrid();
                    this.updateDisplayImage();
                }
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the size of the block.
        /// </summary>
        /// <value>
        /// The size of the block.
        /// </value>
        public int BlockSize
        {
            get => this.blockSize;
            set
            {
                this.blockSize = value;
                if (this.SourceFile != null)
                {
                    this.updateGrid();
                    this.updateDisplayImage();
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the slider maximum.
        /// </summary>
        /// <value>
        /// The slider maximum.
        /// </value>
        public int SliderMaximum
        {
            get => this.sliderMaximum;
            set
            {
                this.sliderMaximum = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Updates the grid.
        /// </summary>
        private void updateGrid()
        {
            this.manipulatorForGridImage = new ImageManipulator(this.imageWithGrid.Decoder.PixelWidth,
                this.imageWithGrid.Decoder.PixelHeight, this.imageWithGrid.SourcePixels);
            this.manipulatorForGridImage.DrawGrid(this.BlockSize, this.TriangleMosaic);
            this.currentImageWithGrid = new WriteableBitmap((int) this.imageWithGrid.Decoder.PixelWidth,
                (int) this.imageWithGrid.Decoder.PixelHeight);
            this.writeStreamOfPixels(this.currentImageWithGrid,
                this.manipulatorForGridImage.RetrieveModifiedPixels());
        }

        /// <summary>
        ///     Gets or sets the save to file command.
        /// </summary>
        public RelayCommand SaveToFileCommand { get; set; }

        /// <summary>
        ///     Gets or sets the load file command.
        /// </summary>
        public RelayCommand LoadFileCommand { get; set; }

        /// <summary>
        /// Gets or sets the convert command.
        /// </summary>
        /// <value>
        /// The convert command.
        /// </value>
        public RelayCommand ConvertCommand { get; set; }

        /// <summary>
        /// Gets or sets the load image palette command.
        /// </summary>
        /// <value>
        /// The load image palette command.
        /// </value>
        public RelayCommand LoadImagePaletteCommand { get; set; }

        /// <summary>
        /// Gets or sets the picture mosaic command.
        /// </summary>
        /// <value>
        /// The picture mosaic command.
        /// </value>
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
            this.imageLoader = new ImageLoader();
            this.imageWithGrid = null;
            this.imageWithMosaic = null;
            this.imagePalette = new ObservableCollection<Image>();
            this.palette = new ImagePalette();
        }

        #endregion

        #region Methods        

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
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

        private void loadProperties()
        {
            this.DisplayImage = null;
            this.currentImageWithGrid = null;
            this.currentImage = null;
            this.ResultImage = null;
            this.SourceFile = null;
            this.BlockSize = 25;
            this.Grid = false;
            this.SquareMosaic = true;
            this.TriangleMosaic = false;
            this.SliderMaximum = 100;
        }

        private bool canCreatePictureMosaic(object obj)
        {
            return this.DisplayImage != null && this.ImagePalette.Count != 0;
        }

        private async void createPictureMosaic(object obj)
        {
           
            this.imageWithMosaic = await this.imageLoader.LoadImage(this.SourceFile);

            var width = (int)this.imageWithMosaic.Decoder.PixelWidth;
            var height = (int)this.imageWithMosaic.Decoder.PixelHeight;
            var pixels = this.imageWithMosaic.SourcePixels;

            this.manipulatorForResultImage = new ImageManipulator((uint)width, (uint)height, pixels);
            
            await this.manipulatorForResultImage.CreatePictureMosaic(this.BlockSize, this.palette);
            this.ResultImage = new WriteableBitmap(width, height);
            this.writeStreamOfPixels(this.ResultImage, this.manipulatorForResultImage.RetrieveModifiedPixels());
        }

        private bool canLoadPalette(object obj)
        {
            return this.imagePalette != null;
        }

        private async void loadPalette(object obj)
        {
            this.ImagePalette.Clear();
            this.palette.ClearPalette();
            var folder = await MainPage.SelectImagePaletteFolder();
            if (folder != null)
            {
                this.palette = await this.imageLoader.LoadImages(folder);
                this.ImagePalette = this.palette.OriginalImages.ToObservableCollection();
            }
            
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
            this.imageWithGrid = await this.imageLoader.LoadImage(this.SourceFile);

            var width = (int) this.imageWithGrid.Decoder.PixelWidth;
            var height = (int) this.imageWithGrid.Decoder.PixelHeight;
            var pixels = this.imageWithGrid.SourcePixels;
            this.currentImage = new WriteableBitmap(width, height);
            this.writeStreamOfPixels(this.currentImage, pixels);

            this.manipulatorForGridImage = new ImageManipulator((uint) width,
                (uint) height, pixels);
            this.manipulatorForResultImage = new ImageManipulator((uint)width, (uint)height, pixels);

            this.manipulatorForGridImage.DrawGrid(this.BlockSize, this.TriangleMosaic);
            this.currentImageWithGrid = new WriteableBitmap(width, height);
            this.writeStreamOfPixels(this.currentImageWithGrid, this.manipulatorForGridImage.RetrieveModifiedPixels());

            if (width > height)
            {
                this.SliderMaximum = width;
            }
            else
            {
                this.SliderMaximum = height;
            }
            this.OnPropertyChanged();
            this.updateDisplayImage();
        }

        private async Task handleCreatingMosaic()
        {
            this.imageWithMosaic = await this.imageLoader.LoadImage(this.SourceFile);

            var width = (int) this.imageWithMosaic.Decoder.PixelWidth;
            var height = (int) this.imageWithMosaic.Decoder.PixelHeight;
            var pixels = this.imageWithMosaic.SourcePixels;

            this.manipulatorForResultImage = new ImageManipulator((uint) width, (uint) height, pixels);
            if (this.SquareMosaic)
            {
                this.manipulatorForResultImage.CreateSquareMosaic(this.BlockSize);
            }
            else if (this.PictureMosaic)
            {
                await this.manipulatorForResultImage.CreatePictureMosaic(this.BlockSize, this.palette);
            }
            else
            {
                this.manipulatorForResultImage.CreateTriangleMosaic(this.BlockSize);
            }
            

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