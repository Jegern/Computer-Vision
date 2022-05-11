using Laboratory_work_1.ViewModels;
using Laboratory_work_1.ViewModels.Store;

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
            var store = new ViewModelStore();
            DataContext = new MainViewModel(store);
            InitializePixelInfoViewModel(store);
            InitializeMagnifierViewModel(store);
            InitializeImageManagementViewModel(store);
            InitializeChromacityViewModel(store);
            InitializeAntiAliasingViewModel(store);
            InitializeMaskingViewModel(store);
            InitializeBorderDetectionViewModel(store);
            InitializeSegmentationViewModel(store);
            InitializeHistogramViewModel(store);
            InitializeAdaptiveThresholdingViewModel(store);
        }

        private void InitializePixelInfoViewModel(ViewModelStore store)
        {
            var pixelInfoViewModel = new PixelInfoViewModel(store);
            PixelInfoControl.DataContext = pixelInfoViewModel;
            PixelInfoMenuItem.DataContext = pixelInfoViewModel;
        }

        private void InitializeMagnifierViewModel(ViewModelStore store)
        {
            var magnifierViewModel = new MagnifierViewModel(store);
            MagnifierControl.DataContext = magnifierViewModel;
            MagnifierMenuItem.DataContext = magnifierViewModel;
        }
        
        private void InitializeImageManagementViewModel(ViewModelStore store)
        {
            var imageManagementViewModel = new ImageManagementViewModel(store);
            ImageManagementControl.DataContext = imageManagementViewModel;
            ImageManagementMenuItem.DataContext = imageManagementViewModel;
        }
        
        private void InitializeChromacityViewModel(ViewModelStore store)
        {
            var сhromacityViewModel = new ChromacityViewModel(store);
            ChromacityControl.DataContext = сhromacityViewModel;
            ChromacityMenuItem.DataContext = сhromacityViewModel;
        }
        
        private void InitializeAntiAliasingViewModel(ViewModelStore store)
        {
            var antiAliasingViewModel = new AntiAliasingViewModel(store);
            AntiAliasingControl.DataContext = antiAliasingViewModel;
            AntiAliasingMenuItem.DataContext = antiAliasingViewModel;
        }
        
        private void InitializeMaskingViewModel(ViewModelStore store)
        {
            var maskingViewModel = new MaskingViewModel(store);
            MaskingControl.DataContext = maskingViewModel;
            MaskingMenuItem.DataContext = maskingViewModel;
        }
        
        private void InitializeBorderDetectionViewModel(ViewModelStore store)
        {
            var borderDetectionViewModel = new BorderDetectionViewModel(store);
            BorderDetectionControl.DataContext = borderDetectionViewModel;
            BorderDetectionMenuItem.DataContext = borderDetectionViewModel;
        }
        
        private void InitializeSegmentationViewModel(ViewModelStore store)
        {
            var segmentationViewModel = new SegmentationViewModel(store);
            SegmentationControl.DataContext = segmentationViewModel;
            SegmentationMenuItem.DataContext = segmentationViewModel;
        }
        
        private void InitializeHistogramViewModel(ViewModelStore store)
        {
            var histogramViewModel = new HistogramViewModel(store);
            HistogramControl.DataContext = histogramViewModel;
            HistogramMenuItem.DataContext = histogramViewModel;
        }
        
        private void InitializeAdaptiveThresholdingViewModel(ViewModelStore store)
        {
            var adaptiveThresholdingViewModel = new AdaptiveThresholdingViewModel(store);
            AdaptiveThresholdingControl.DataContext = adaptiveThresholdingViewModel;
            AdaptiveThresholdingMenuItem.DataContext = adaptiveThresholdingViewModel;
        }
    }
}