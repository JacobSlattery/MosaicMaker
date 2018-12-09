using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GroupEMosaicMaker.ViewModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GroupEMosaicMaker.View
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        #region Data members

        /// <summary>
        ///     The file types
        /// </summary>
        public static readonly string[] FileTypes = {".jpg", ".bmp", ".png"};

        /// <summary>
        ///     The file types for saving
        /// </summary>
        public static List<string> FileTypesForSaving;

        private static StorageFile sourceFile;

        /// <summary>
        ///     The view model/
        /// </summary>
        public MainPageViewModel ViewModel;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MainPage" /> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(1202, 960);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            sourceFile = null;
            FileTypesForSaving = new List<string>();
            this.ViewModel = new MainPageViewModel(this.imagePaletteGridView);
            DataContext = this.ViewModel;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Picks the file to open with a file open picker.
        /// </summary>
        /// <returns>
        ///     A <see cref="StorageFile" />.
        /// </returns>
        /// <summary>
        ///     Picks the file to open with a file open picker.
        /// </summary>
        /// <returns>
        ///     A <see cref="StorageFile" />.
        /// </returns>
        public static async Task<StorageFile> SelectSourceImageFile()
        {
            FileTypesForSaving.Clear();
            var openPicker = new FileOpenPicker {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            foreach (var fileType in FileTypes)
            {
                openPicker.FileTypeFilter.Add(fileType);
            }

            try
            {
                sourceFile = await openPicker.PickSingleFileAsync();
                FileTypesForSaving.Add(sourceFile.FileType);
            }
            catch (NullReferenceException)
            {
                sourceFile = null;
            }
            catch (Exception)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Invalid File",
                    IsPrimaryButtonEnabled = true,
                    DefaultButton = ContentDialogButton.Primary,
                    PrimaryButtonText = "Ok"
                    
                };
                await dialog.ShowAsync();
            }

            return sourceFile;
        }

        /// <summary>
        ///     Picks a file to open and select for the image palette
        /// </summary>
        /// <returns> the file</returns>
        public static async Task<StorageFile> SelectImageForPalette()
        {
            var openPicker = new FileOpenPicker {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            foreach (var fileType in FileTypes)
            {
                openPicker.FileTypeFilter.Add(fileType);
            }

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

        /// <summary>
        ///     Picks the folder to open with a file open picker.
        /// </summary>
        /// <returns>
        ///     A <see cref="StorageFile" />.
        /// </returns>
        /// <summary>
        ///     Picks the file to open with a file open picker.
        /// </summary>
        /// <returns>
        ///     A <see cref="StorageFile" />.
        /// </returns>
        public static async Task<StorageFolder> SelectImagePaletteFolder()
        {
            var openPicker = new FolderPicker {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            foreach (var fileType in FileTypes)
            {
                openPicker.FileTypeFilter.Add(fileType);
            }

            StorageFolder folder;
            try
            {
                folder = await openPicker.PickSingleFolderAsync();
            }
            catch (NullReferenceException)
            {
                folder = null;
            }

            return folder;
        }

        /// <summary>
        ///     Selects the save image file.
        /// </summary>
        /// <returns> the save file</returns>
        public static async Task<StorageFile> SelectSaveImageFile()
        {
            createSaveFileTypes();

            var fileSavePicker = new FileSavePicker {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "image"
            };
            addFileTypesForSaving(fileSavePicker);

            var file = await fileSavePicker.PickSaveFileAsync();

            return file;
        }

        private static void addFileTypesForSaving(FileSavePicker fileSavePicker)
        {
            foreach (var fileType in FileTypesForSaving)
            {
                fileSavePicker.FileTypeChoices.Add(fileType + " file", new List<string> {fileType});
            }
        }

        private static void createSaveFileTypes()
        {
            foreach (var fileType in FileTypes)
            {
                if (!FileTypesForSaving.Contains(fileType))
                {
                    FileTypesForSaving.Add(fileType);
                }
            }
        }

        private void scaleToFitRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            this.displayScrollView.ChangeView(0, this.displayScrollView.VerticalOffset, 1.0f);
            this.resultScrollView.ChangeView(0, this.resultScrollView.VerticalOffset, 1.0f);
        }

        #endregion
    }
}