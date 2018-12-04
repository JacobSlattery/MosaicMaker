using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using GroupEMosaicMaker.ViewModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GroupEMosaicMaker
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Data members

        public static readonly string[] FileTypes = { ".jpg", ".jpeg", ".gif", ".bmp", ".png"};

        public static List<string> FileTypesForSaving;

        private static StorageFile sourcefile;
        public MainPageViewModel ViewModel;

        #endregion

        #region Constructors

        public MainPage()
        {
            this.InitializeComponent();

            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;

            ApplicationView.PreferredLaunchViewSize = new Size(bounds.Width, bounds.Height);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            sourcefile = null;
            FileTypesForSaving = new List<string>();

            this.ViewModel = new MainPageViewModel();
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
                sourcefile = await openPicker.PickSingleFileAsync();
                FileTypesForSaving.Add(sourcefile.FileType);
            }
            catch (NullReferenceException)
            {
                sourcefile = null;
            }

            return sourcefile;
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

        public static async Task<StorageFile> SelectSaveImageFile()
        {
            createSaveFileTypes();

            var fileSavePicker = new FileSavePicker
            {
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

        #endregion
    }
}