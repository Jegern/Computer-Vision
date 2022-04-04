using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Laboratory_work_1
{
    /// <summary>
    /// Логика взаимодействия для ImageProperties.xaml
    /// </summary>
    public partial class ImageProperties : Page
    {
        private MainWindow _owner;
        private BitmapSource _original;

        public ImageProperties(MainWindow owner)
        {
            InitializeComponent();
            _owner = owner;
            _original = (BitmapSource) _owner.Picture.Source;
        }

        private void Brightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_owner != null)
            {
                // deterime the brightness to add
                byte brightnessValue = (byte) Brightness.Value;
                BitmapSource orig = _original;
                ChangeBrightness((BitmapSource) _owner.Picture.Source, brightnessValue, orig);
            }
        }

        private void ChangeBrightness(BitmapSource source, byte brightnessValue, BitmapSource original)
        {
            var imagePixels = MainWindow.GetPixels(source);
            var origPixels = MainWindow.GetPixels(original);

            for (int i = 0; i < imagePixels.Length; i++)
            {
                if ((i + 1) % 4 == 0)
                {
                    imagePixels[i - 3] = (byte) (origPixels[i - 3] * brightnessValue / 127);
                    imagePixels[i - 2] = (byte) (origPixels[i - 2] * brightnessValue / 127);
                    imagePixels[i - 1] = (byte) (origPixels[i - 1] * brightnessValue / 127);
                }
                else
                {
                    continue;
                }
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
            if (Intensivity != null)
            {
                // deterime the brightness to add
                byte intensivityValue = (byte) Intensivity.Value;

                ChangeIntensivity((BitmapSource) _owner.Picture.Source, intensivityValue);
            }
        }

        private void ChangeIntensivity(BitmapSource source, byte intensivityValue)
        {
            var imagePixels = MainWindow.GetPixels(source);

            for (int i = 0; i < imagePixels.Length; i++)
            {
                if ((i + 1) % 4 == 0)
                {
                    imagePixels[i] = intensivityValue;
                }
                else
                {
                    continue;
                }
            }

            ReplaceImage(source, imagePixels);
        }

        private void Bleach_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource source = (BitmapSource) _owner.Picture.Source;

            var imagePixels = MainWindow.GetPixels(source);

            for (int i = 0; i < imagePixels.Length; i += 4)
            {
                int coef = (imagePixels[i] + imagePixels[i + 1] + imagePixels[i + 2]) / 3;

                imagePixels[i] = (byte) coef;
                imagePixels[i + 1] = (byte) coef;
                imagePixels[i + 2] = (byte) coef;
            }

            ReplaceImage(source, imagePixels);
        }

        private void Negative_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource source = (BitmapSource) _owner.Picture.Source;

            var imagePixels = MainWindow.GetPixels(source);

            for (int i = 0; i < imagePixels.Length; i += 4)
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
            var imagePixels = MainWindow.GetPixels(source);
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
            var red = (bool) RedExchange.IsChecked ? 1 : 0;
            var green = (bool) GreenExchange.IsChecked ? 1 : 0;
            var blue = (bool) BlueExchange.IsChecked ? 1 : 0;
            int[] chanels = {red, green, blue};
            return chanels;
        }

        private void SimmetricalImage_Click(object sender, RoutedEventArgs e)
        {
            var source = (BitmapSource) _owner.Picture.Source;
            var imageBytes = MainWindow.GetPixels(source);
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


            //BitmapSource source = (BitmapSource)_owner.MainImage.Source;

            //var imagePixels = MainWindow.GetPixels(source);
            //var temp = imagePixels[0];
            //var width = source.PixelWidth * 4;
            //for (int i = 0; i < imagePixels.Length; i += width)
            //{
            //    for (int j = 0; j < width / 2; j += 4)
            //    {
            //        temp = imagePixels[i + j];
            //        imagePixels[i + j] = imagePixels[width - j];
            //        imagePixels[width - j] = temp;
            //        temp = imagePixels[i + j + 1];
            //        imagePixels[i + j + 1] = imagePixels[width - j + 1];
            //        imagePixels[width - j + 1] = temp;
            //        temp = imagePixels[i + j + 2];
            //        imagePixels[i + j + 2] = imagePixels[width - j + 2];
            //        imagePixels[width - j + 2] = temp;
            //    }
            //}

            //ReplaceImage(source, imagePixels);
        }

        private void Vanish_Click(object sender, RoutedEventArgs e)
        {
            var source = (BitmapSource) _owner.Picture.Source;
            var imageBytes = MainWindow.GetPixels(source);
            var copyBytes = MainWindow.GetPixels(source);
            var width = source.PixelWidth;
            var height = source.PixelHeight;
            int neighbour_left = 0;
            int neighbour_right = 0;
            int neighbour_top = 0;
            int neighbour_bottom = 0;
            for (int i = 0; i < imageBytes.Length; i++)
            {
                int count = 0;
                try
                {
                    neighbour_left = imageBytes[i - 4];
                    count++;
                }
                catch
                {
                }

                try
                {
                    neighbour_right = imageBytes[i + 4];
                    count++;
                }
                catch
                {
                }

                try
                {
                    neighbour_top = imageBytes[i - 4 * width];
                    count++;
                }
                catch
                {
                }

                try
                {
                    neighbour_bottom = imageBytes[i + 4 * width];
                    count++;
                }
                catch
                {
                }

                imageBytes[i] = (byte) ((copyBytes[neighbour_left] +
                                         copyBytes[neighbour_right] +
                                         copyBytes[neighbour_top] +
                                         copyBytes[neighbour_bottom])
                                        / count);
            }

            ReplaceImage(source, imageBytes);
        }
    }
}