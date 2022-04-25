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

            var store = new Store();
            DataContext = new MainViewModel(store);
            PixelInfo.DataContext = new PixelInfoViewModel(store);
        }

        internal static byte[] GetPixels(BitmapSource source, int x = 0, int y = 0, int width = 0, int height = 0)
        {
            if (width == 0)
                width = source.PixelWidth;
            if (height == 0)
                height = source.PixelHeight;
            var stride = width * 4;
            var pixels = new byte[height * width * 4];
            source.CopyPixels(new Int32Rect(x, y, width, height), pixels, stride, 0);
            return pixels;
        }
    }
}