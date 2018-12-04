using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
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

        public static readonly string[] FileTypes = {".jpeg", ".jpg", ".gif", ".bmp", ".png"};

        public MainPageViewModel ViewModel;

        #endregion

        #region Constructors

        public MainPage()
        {
            this.InitializeComponent();

            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;

            ApplicationView.PreferredLaunchViewSize = new Size(bounds.Width, bounds.Height);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

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

            //  StorageFolder folder;
            //  try
            //  {
            var folder = await openPicker.PickSingleFolderAsync();
            //  }
            //  catch (NullReferenceException)
            //  {
            //      folder = null;
            //  }

            return folder;
        }

        public static async Task<StorageFile> SelectSaveImageFile()
        {
            var fileSavePicker = new FileSavePicker {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "image"
            };
            foreach (var fileType in FileTypes)
            {
                fileSavePicker.FileTypeChoices.Add(fileType + " file", new List<string> {fileType});
            }
            var file = await fileSavePicker.PickSaveFileAsync();
            return file;
        }

        #endregion
    }
}