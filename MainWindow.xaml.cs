using System.Windows;
using System.Windows.Media.Imaging;

namespace Laboratory_work_1
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
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