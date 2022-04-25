using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Laboratory_work_1.Views
{
    public partial class ImageManagement
    {
        private readonly MainWindow _owner;
        private readonly BitmapSource _original;

        public ImageManagement(MainWindow owner)
        {
            InitializeComponent();
            _owner = owner;
            _original = (BitmapSource) _owner.Picture.Source;
        }

        private void Brightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_owner is null) return;
            
            var brightnessValue = (byte) Brightness.Value;
            ChangeBrightness((BitmapSource) _owner.Picture.Source, brightnessValue, _original);
        }

        private void ChangeBrightness(BitmapSource source, byte brightnessValue, BitmapSource original)
        {
            var imagePixels = Tools.GetPixels(source);
            var origPixels = Tools.GetPixels(original);

            for (var i = 0; i < imagePixels.Length; i++)
            {
                if ((i + 1) % 4 != 0) continue;
                imagePixels[i - 3] = (byte) (origPixels[i - 3] * brightnessValue / 127);
                imagePixels[i - 2] = (byte) (origPixels[i - 2] * brightnessValue / 127);
                imagePixels[i - 1] = (byte) (origPixels[i - 1] * brightnessValue / 127);
            }

            ReplaceImage(source, imagePixels);
        }

        private void ReplaceImage(BitmapSource source, byte[] imagePixels)
        {
            _owner.Picture.Source = BitmapSource.Create(
                source.PixelWidth,
                source.PixelHeight,
                source.DpiX,
                source.DpiY,
                source.Format,
                source.Palette,
                imagePixels,
                source.PixelWidth * 4);
        }

        private void Intensivity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Intensivity == null) return;
            var intensivityValue = (byte) Intensivity.Value;

            ChangeIntensivity((BitmapSource) _owner.Picture.Source, intensivityValue);
        }

        private void ChangeIntensivity(BitmapSource source, byte intensivityValue)
        {
            var imagePixels = Tools.GetPixels(source);

            for (var i = 0; i < imagePixels.Length; i++)
                if ((i + 1) % 4 == 0)
                    imagePixels[i] = intensivityValue;

            ReplaceImage(source, imagePixels);
        }

        private void Bleach_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource source = (BitmapSource) _owner.Picture.Source;

            var imagePixels = Tools.GetPixels(source);

            for (var i = 0; i < imagePixels.Length; i += 4)
            {
                var coef = (imagePixels[i] + imagePixels[i + 1] + imagePixels[i + 2]) / 3;

                imagePixels[i] = (byte) coef;
                imagePixels[i + 1] = (byte) coef;
                imagePixels[i + 2] = (byte) coef;
            }

            ReplaceImage(source, imagePixels);
        }

        private void Negative_Click(object sender, RoutedEventArgs e)
        {
            var source = (BitmapSource) _owner.Picture.Source;

            var imagePixels = Tools.GetPixels(source);

            for (var i = 0; i < imagePixels.Length; i += 4)
            {
                imagePixels[i] = (byte) Math.Abs(imagePixels[i] - 255);
                imagePixels[i + 1] = (byte) Math.Abs(imagePixels[i + 1] - 255);
                imagePixels[i + 2] = (byte) Math.Abs(imagePixels[i + 2] - 255);
            }

            ReplaceImage(source, imagePixels);
        }

        private void SwapChanels_Click(object sender, RoutedEventArgs e)
        {
            var source = (BitmapSource) _owner.Picture.Source;
            var imagePixels = Tools.GetPixels(source);
            var chanels = FindChanelsToSwap();

            for (var i = 0; i < imagePixels.Length; i += 4)
            {
                var temp = (chanels[0] == 1) ? imagePixels[i] : imagePixels[i + 1];

                if (temp == imagePixels[i + 1])
                {
                    imagePixels[i + 1] = imagePixels[i + 2];
                    imagePixels[i + 2] = temp;
                }
                else if (chanels[1] == 0)
                {
                    imagePixels[i] = imagePixels[i + 2];
                    imagePixels[i + 2] = temp;
                }
                else
                {
                    imagePixels[i] = imagePixels[i + 1];
                    imagePixels[i + 1] = temp;
                }
            }

            ReplaceImage(source, imagePixels);
        }

        private int[] FindChanelsToSwap()
        {
            var red = RedExchange.IsChecked == true ? 1 : 0;
            var green = GreenExchange.IsChecked == true ? 1 : 0;
            var blue = BlueExchange.IsChecked == true ? 1 : 0;
            int[] chanels = {red, green, blue};
            return chanels;
        }

        private void SimmetricalImage_Click(object sender, RoutedEventArgs e)
        {
            var source = (BitmapSource) _owner.Picture.Source;
            var imageBytes = Tools.GetPixels(source);
            var width = source.PixelWidth;
            var height = source.PixelHeight;
            if (Vertical.IsChecked == true)
            {
                for (var i = 0; i < height; i++)
                for (var j = 0; j < width / 2; j++)
                {
                    var tempR = imageBytes[i * width * 4 + j * 4 + 0];
                    var tempG = imageBytes[i * width * 4 + j * 4 + 1];
                    var tempB = imageBytes[i * width * 4 + j * 4 + 2];
                    imageBytes[i * width * 4 + j * 4 + 0] = imageBytes[i * width * 4 + (width - j - 1) * 4 + 0];
                    imageBytes[i * width * 4 + j * 4 + 1] = imageBytes[i * width * 4 + (width - j - 1) * 4 + 1];
                    imageBytes[i * width * 4 + j * 4 + 2] = imageBytes[i * width * 4 + (width - j - 1) * 4 + 2];
                    imageBytes[i * width * 4 + (width - j - 1) * 4 + 0] = tempR;
                    imageBytes[i * width * 4 + (width - j - 1) * 4 + 1] = tempG;
                    imageBytes[i * width * 4 + (width - j - 1) * 4 + 2] = tempB;
                }
            }
            else
            {
                for (var i = 0; i < height / 2; i++)
                for (var j = 0; j < width; j++)
                {
                    var tempR = imageBytes[i * width * 4 + j * 4 + 0];
                    var tempG = imageBytes[i * width * 4 + j * 4 + 1];
                    var tempB = imageBytes[i * width * 4 + j * 4 + 2];
                    imageBytes[i * width * 4 + j * 4 + 0] = imageBytes[(height - 1 - i) * width * 4 + j * 4 + 0];
                    imageBytes[i * width * 4 + j * 4 + 1] = imageBytes[(height - 1 - i) * width * 4 + j * 4 + 1];
                    imageBytes[i * width * 4 + j * 4 + 2] = imageBytes[(height - 1 - i) * width * 4 + j * 4 + 2];
                    imageBytes[(height - 1 - i) * width * 4 + j * 4 + 0] = tempR;
                    imageBytes[(height - 1 - i) * width * 4 + j * 4 + 1] = tempG;
                    imageBytes[(height - 1 - i) * width * 4 + j * 4 + 2] = tempB;
                }
            }

            ReplaceImage(source, imageBytes);
        }

        private void Vanish_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}