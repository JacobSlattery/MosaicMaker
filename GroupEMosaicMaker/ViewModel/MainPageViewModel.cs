using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using GroupEMosaicMaker.Utility;

namespace GroupEMosaicMaker.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {

        private Image sourceImage;
        private Image displayImage;
        private Image resultImage;


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

        public Image ResultImage
        {
            get => this.resultImage;
            set
            {
                this.resultImage = value;
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
        }

        private void loadCommands()
        {
            this.LoadFileCommand = new RelayCommand(this.loadFile, this.canLoadFile);
            this.SaveToFileCommand = new RelayCommand(this.saveToFile, this.canSaveToFile);
        }

        private void loadProperties()
        {
            this.displayImage = null;
            this.resultImage = null;
            this.sourceImage = null;
        }

        private async void loadFile(object obj)
        {
            var file = await MainPage.PickFileWithOpenPicker();

            if (file != null)
            {
                this.handleNewImage(file);
            }
        }

        private void handleNewImage(StorageFile file)
        {

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
