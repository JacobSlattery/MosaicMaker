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
using System.Collections.Generic;
using GroupEMosaicMaker.View;

namespace GroupEMosaicMaker.ViewModel
{
    /// <summary>
    /// The main paige view model
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class MainPageViewModel : INotifyPropertyChanged
    {
        #region Data members

        private const int DefaultGridSetting = 25;
        private const int DefaultLastUsedBlockSize = 0;

        private StorageFile sourceFile;
        private WriteableBitmap displayImage;
        private WriteableBitmap resultImage;
        private bool grid;
        private bool squareMosaic;
        private bool triangleMosaic;
        private bool pictureMosaic;
        private int blockSize;
        private ImageManipulator manipulatorForGridImage;
        private ImageManipulator manipulatorForResultImage;
        private readonly ImageLoader imageLoader;
        private Image propertiesOfImageWithGrid;
        private Image propertiesOfImageWithMosaic;

        private WriteableBitmap currentImage;
        private WriteableBitmap currentImageWithGrid;
        private ObservableCollection<Image> imagePalette;
        private Image selectedImage;
        private ImagePalette palette;

        private bool zoomImage;

        private int countOfImagesInPalette;

        private int lastUsedBlockSizeForSquareMosaic;
        private int lastUsedBlockSizeForPictureMosaic;
        private int lastUsedBlockSizeForTriangleMosaic;
        private bool blackAndWhiteCreated;

        private bool randomize;
        private bool lastRandomizeSelection;

        private bool useSelectedImages;

        private bool useEachImageOnce;
        private bool lastUseEachImageOnceSelection;

        #endregion

        #region Properties

        public bool UseEachImageOnce
        {
            get => this.useEachImageOnce;
            set
            {
                this.useEachImageOnce = value;
                this.ConvertCommand.OnCanExecuteChanged();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets and sets whether or not the use selected images was selected  
        /// </summary>
        /// <value>
        /// <c> true</c> if [use selected images]; otherwise, <c>false</c>
        /// </value>
        public bool UseSelectedImages
        {
            get => this.useSelectedImages;
            set
            {
                this.useSelectedImages = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets and sets whether or not the randomize was selected  
        /// </summary>
        /// <value>
        /// <c> true</c> if [randomize]; otherwise, <c>false</c>
        /// </value>
        public bool Randomize
        {
            get => this.randomize;
            set
            {
                this.randomize = value;
                this.ConvertCommand.OnCanExecuteChanged();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets and sets whether or not the black and white image was created 
        /// </summary>
        /// <value>
        ///   <c>true</c> if [black and white created]; otherwise, <c>false</c>.
        /// </value>
        public bool BlackAndWhiteCreated
        {
            get => this.blackAndWhiteCreated;
            set
            {
                this.blackAndWhiteCreated = value;
                this.ConvertCommand.OnCanExecuteChanged();
                this.ConvertToBlackAndWhiteCommand.OnCanExecuteChanged();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the number of images in the image palette 
        /// </summary>
        /// <value> the count of images in the palette</value>
        public int CountOfImagesInPalette
        {
            get => this.countOfImagesInPalette;
            set
            {
                this.countOfImagesInPalette = value;
                this.OnPropertyChanged();
            }
        }

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
                this.RemoveImageFromPaletteCommand.OnCanExecuteChanged();
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
                this.ConvertCommand.OnCanExecuteChanged();
                this.RemoveImageFromPaletteCommand.OnCanExecuteChanged();
                this.ClearImagePaletteCommand.OnCanExecuteChanged();
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
                this.ConvertToBlackAndWhiteCommand.OnCanExecuteChanged();
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
                    this.ConvertCommand.OnCanExecuteChanged();
                    this.updateDisplayImage();
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Updates the grid.
        /// </summary>
        private void updateGrid()
        {
            this.manipulatorForGridImage = new ImageManipulator(this.propertiesOfImageWithGrid.Decoder.PixelWidth,
                this.propertiesOfImageWithGrid.Decoder.PixelHeight, this.propertiesOfImageWithGrid.SourcePixels);
            this.manipulatorForGridImage.DrawGrid(this.BlockSize, this.TriangleMosaic);
            this.currentImageWithGrid = new WriteableBitmap((int)this.propertiesOfImageWithGrid.Decoder.PixelWidth,
                (int)this.propertiesOfImageWithGrid.Decoder.PixelHeight);
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
        /// Gets or sets the convert to black and white command.
        /// </summary>
        /// <value>
        /// The convert to black and white command.
        /// </value>
        public RelayCommand ConvertToBlackAndWhiteCommand { get; set; }

        /// <summary>
        /// Gets or sets the add image to palette command.
        /// </summary>
        /// <value>
        /// The add image to palette command.
        /// </value>
        public RelayCommand AddImageToPaletteCommand { get; set; }

        /// <summary>
        /// Gets or sets the remove image from palette command.
        /// </summary>
        /// <value>
        /// The remove image from palette command.
        /// </value>
        public RelayCommand RemoveImageFromPaletteCommand { get; set; }

        /// <summary>
        /// Gets or sets the clear image palette command.
        /// </summary>
        /// <value>
        /// The clear image palette command.
        /// </value>
        public RelayCommand ClearImagePaletteCommand { get; set; }


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
            this.propertiesOfImageWithGrid = null;
            this.propertiesOfImageWithMosaic = null;
            this.imagePalette = new ObservableCollection<Image>();
            this.palette = new ImagePalette();
        }

        #endregion

        #region Methods     

        /// <summary>
        /// Adds the selected images to the image palette
        /// </summary>
        /// <param name="images"> the images to be added to the palette</param>
        public void AddSelectedImages(IList<object> images)
        {
            this.palette.ClearPalette();
            var selectedImages = new List<Image>();
            foreach (Image current in images)
            {
                selectedImages.Add(current);
            }
            foreach (var current in selectedImages)
            {
                this.palette.AddImage(current);
            }
        }

        /// <summary>
        /// Adds all the images in the image palette to the palette 
        /// </summary>
        public void AddAllImages()
        {
            foreach (var image in this.ImagePalette)
            {
                this.palette.AddImage(image);
            }

        }

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
            this.ConvertToBlackAndWhiteCommand = new RelayCommand(this.convertToBlackAndWhite, this.canCanConvertToBlackAndWhite);
            this.AddImageToPaletteCommand = new RelayCommand(this.addImageToPalette, this.canAddImageToPalette);
            this.RemoveImageFromPaletteCommand = new RelayCommand(this.removeImageFromPalette, this.canRemoveImageFromPalette);
            this.ClearImagePaletteCommand = new RelayCommand(this.clearImagePalette, this.canClearImagePalette);
        }

        private bool canClearImagePalette(object obj)
        {
            return this.ImagePalette.Count != 0;
        }

        private void clearImagePalette(object obj)
        {
            this.resetImagePalette();
        }

        private bool canRemoveImageFromPalette(object obj)
        {
            return this.SelectedImage != null;
        }

        private void removeImageFromPalette(object obj)
        {
            this.palette.RemoveImage(this.SelectedImage);
            this.CountOfImagesInPalette = this.palette.OriginalImages.Count;
            this.ImagePalette = this.palette.OriginalImages.ToObservableCollection();

        }

        private bool canAddImageToPalette(object obj)
        {
            return true;
        }

        private async void addImageToPalette(object obj)
        {
            var file = await MainPage.SelectImageForPalette();
            if (file != null)
            {
                var image = await this.imageLoader.LoadImage(file);
                this.palette.AddImage(image);
                this.CountOfImagesInPalette = this.palette.OriginalImages.Count;
                this.ImagePalette = this.palette.OriginalImages.ToObservableCollection();
            }
        }

        private bool canCanConvertToBlackAndWhite(object obj)
        {
            return this.ResultImage != null && !this.BlackAndWhiteCreated;
        }

        private void convertToBlackAndWhite(object obj)
        {
            this.manipulatorForResultImage.ConvertImageToBlackAndWhite();
            this.ResultImage = new WriteableBitmap((int)this.propertiesOfImageWithMosaic.Decoder.PixelWidth, (int)this.propertiesOfImageWithMosaic.Decoder.PixelHeight);
            this.writeStreamOfPixels(this.ResultImage, this.manipulatorForResultImage.RetrieveModifiedPixels());
            this.BlackAndWhiteCreated = true;
        }

        private void loadProperties()
        {
            this.DisplayImage = null;
            this.currentImageWithGrid = null;
            this.currentImage = null;
            this.ResultImage = null;
            this.SourceFile = null;
            this.BlockSize = DefaultGridSetting;
            this.Grid = false;
            this.SquareMosaic = true;
            this.randomize = false;
        }

        private bool canLoadPalette(object obj)
        {
            return this.imagePalette != null;
        }

        private async void loadPalette(object obj)
        {
            this.resetImagePalette();
            var folder = await MainPage.SelectImagePaletteFolder();
            if (folder != null)
            {
                this.palette = await this.imageLoader.LoadImages(folder);
                this.CountOfImagesInPalette = this.palette.OriginalImages.Count;
                this.ImagePalette = this.palette.OriginalImages.ToObservableCollection();
            }
            
        }

        private void resetImagePalette()
        {
            this.palette.ClearPalette();
            this.palette.AverageColorDictionary.Clear();
            this.CountOfImagesInPalette = this.palette.OriginalImages.Count;
            this.ImagePalette = this.palette.OriginalImages.ToObservableCollection();
        }

        private bool canConvert(object obj)
        {
            if (this.SquareMosaic)
            {
               
                return (this.DisplayImage != null && this.BlockSize != this.lastUsedBlockSizeForSquareMosaic) || this.BlackAndWhiteCreated;
                
            } else if (this.PictureMosaic)
            {
                return (this.DisplayImage != null && this.ImagePalette.Count != 0 && this.BlockSize != this.lastUsedBlockSizeForPictureMosaic) 
                       || (this.BlackAndWhiteCreated && this.ImagePalette.Count != 0 || this.Randomize != this.lastRandomizeSelection || this.UseEachImageOnce != this.lastUseEachImageOnceSelection);
            }
            else
            { 
                return (this.DisplayImage != null && this.BlockSize != this.lastUsedBlockSizeForTriangleMosaic) || this.BlackAndWhiteCreated;
            }
        }

        private async void convert(object obj)
        {
            await this.handleCreatingMosaic();
            this.BlackAndWhiteCreated = false;
            this.UseSelectedImages = false;

        }

        private async void loadFile(object obj)
        {
            var file = await MainPage.SelectSourceImageFile();

            if (file != null)
            {
                this.SourceFile = file;
                await this.handleNewImageFile();
                this.resetLastUsedBlockSizes();

                this.ResultImage = null;
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
            this.propertiesOfImageWithGrid = await this.imageLoader.LoadImage(this.SourceFile);


            var width = (int)this.propertiesOfImageWithGrid.Decoder.PixelWidth;
            var height = (int)this.propertiesOfImageWithGrid.Decoder.PixelHeight;
            var pixels = this.propertiesOfImageWithGrid.SourcePixels;
            this.currentImage = new WriteableBitmap(width, height);
            this.writeStreamOfPixels(this.currentImage, pixels);

            this.manipulatorForGridImage = new ImageManipulator((uint)width,
                (uint)height, pixels);
            this.manipulatorForResultImage = new ImageManipulator((uint)width, (uint)height, pixels);


            this.manipulatorForGridImage.DrawGrid(this.BlockSize, this.TriangleMosaic);
            this.currentImageWithGrid = new WriteableBitmap(width, height);
            this.writeStreamOfPixels(this.currentImageWithGrid, this.manipulatorForGridImage.RetrieveModifiedPixels());
            this.updateDisplayImage();
        }

        private async Task handleCreatingMosaic()
        {
            this.resetLastUsedBlockSizes();
            this.propertiesOfImageWithMosaic = await this.imageLoader.LoadImage(this.SourceFile);

            var width = (int)this.propertiesOfImageWithMosaic.Decoder.PixelWidth;
            var height = (int)this.propertiesOfImageWithMosaic.Decoder.PixelHeight;
            var pixels = this.propertiesOfImageWithMosaic.SourcePixels;

            this.manipulatorForResultImage = new ImageManipulator((uint)width, (uint)height, pixels);
            if (this.SquareMosaic)
            {
                this.manipulatorForResultImage.CreateSquareMosaic(this.BlockSize);
                this.lastUsedBlockSizeForSquareMosaic = this.BlockSize;
            }
            else if (this.PictureMosaic)
            {
                if (this.Randomize)
                {
                    await this.manipulatorForResultImage.CreatePictureMosaic(this.BlockSize, this.palette, this.Randomize);
                    this.lastUsedBlockSizeForPictureMosaic = this.BlockSize;
                    this.lastRandomizeSelection = this.Randomize;
                }
                else if (this.UseEachImageOnce)
                {
                    await this.manipulatorForResultImage.CreatePictureMosaicUsingEachImageAtleastOnce(this.BlockSize,
                        this.palette);
                    this.lastUsedBlockSizeForPictureMosaic = this.BlockSize;
                    this.lastUseEachImageOnceSelection = this.UseEachImageOnce;
                }
                else
                {
                    await this.manipulatorForResultImage.CreatePictureMosaic(this.BlockSize, this.palette, false);
                    this.lastUsedBlockSizeForPictureMosaic = this.BlockSize;
                    this.lastRandomizeSelection = this.Randomize;
                    this.lastUseEachImageOnceSelection = this.UseEachImageOnce;
                }
                
            }
            else
            {
                this.manipulatorForResultImage.CreateTriangleMosaic(this.BlockSize);
                this.lastUsedBlockSizeForTriangleMosaic = this.BlockSize;
            }

            this.ResultImage = new WriteableBitmap(width, height);
            this.writeStreamOfPixels(this.ResultImage, this.manipulatorForResultImage.RetrieveModifiedPixels());

        }

        private void resetLastUsedBlockSizes()
        {
            this.lastUsedBlockSizeForPictureMosaic = DefaultLastUsedBlockSize;
            this.lastUsedBlockSizeForSquareMosaic = DefaultLastUsedBlockSize;
            this.lastUsedBlockSizeForTriangleMosaic = DefaultLastUsedBlockSize;
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
                await ImageSaver.SaveImage(saveFile, this.ResultImage, this.propertiesOfImageWithMosaic);
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