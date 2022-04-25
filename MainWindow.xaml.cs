using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.ViewModels;

namespace Laboratory_work_1
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeViewModels();
        }

        private void InitializeViewModels()
        {
            var store = new Store();
            DataContext = new MainViewModel(store);
            InitializePixelInfoViewModel(store);
            InitializeMagnifierViewModel(store);
        }

        private void InitializePixelInfoViewModel(Store store)
        {
            var pixelInfoViewModel = new PixelInfoViewModel(store);
            PixelInfoControl.DataContext = pixelInfoViewModel;
            PixelInfoMenuItem.DataContext = pixelInfoViewModel;
        }

        private void InitializeMagnifierViewModel(Store store)
        {
            var magnifierViewModel = new MagnifierViewModel(store);
            MagnifierControl.DataContext = magnifierViewModel;
            MagnifierMenuItem.DataContext = magnifierViewModel;
        }
    }
}