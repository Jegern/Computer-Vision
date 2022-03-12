using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Laboratory_work_1
{
    public partial class MainWindow
    {
        private readonly Frame[] _listOfFrames;
        private byte[] _slidingWindowPixels = null!;

        public MainWindow()
        {
            InitializeComponent();
            _listOfFrames = new[]
            {
                PixelInfoFrame,
                SlidingWindowFrame,
                SlidingWindowInfoFrame,
                BrightnessProfileFrame,
                ImagePropertiesFrame
            };
        }

        private static void ChangeFrameContent(Frame frame, Page? page)
        {
            frame.Content = page;
            frame.NavigationService.RemoveBackEntry();
        }

        private void Frame_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            var loadedFrame = (Frame) sender;
            ResizingToolboxColumnCondition(loadedFrame.Tag);
            ResizeToolboxRow(loadedFrame);
            ChangeFrameTag(loadedFrame);
        }

        private void ResizingToolboxColumnCondition(object frameTag)
        {
            if (CountNonEmptyFrameContents() == 0)
                ResizeToolboxColumn(0);
            else if (CountNonEmptyFrameContents() == 1 &&
                     (string) frameTag == "Hidden")
                ResizeToolboxColumn(200);
        }

        private int CountNonEmptyFrameContents()
        {
            return _listOfFrames.Count(frame => frame.Content is not null);
        }

        private void ResizeToolboxColumn(int width)
        {
            Width += width == 0 ? -200 : width;
            ToolsColumn.Width = new GridLength(width);
            Getters.CenterWindowOnScreen(this);
        }

        private void ResizeToolboxRow(Frame loadedFrame)
        {
            var resizingRow = FrameGrid.RowDefinitions[Array.IndexOf(_listOfFrames, loadedFrame)];
            resizingRow.Height = new GridLength((string) loadedFrame.Tag == "Visible" ? 0 : 1, GridUnitType.Star);
        }

        private static void ChangeFrameTag(ContentControl frame)
        {
            frame.Tag = frame.Content is null ? "Hidden" : "Visible";
        }

        private void ToggleSlidingWindowCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ChangeFrameContent(SlidingWindowFrame,
                SlidingWindowFrame.Content is null
                    ? new SlidingWindow()
                    : null);
        }

        private void MainImage_OnMouseMove(object sender, MouseEventArgs e)
        {
            var pixelPosition = e.MouseDevice.GetPosition(sender as Image);

            if (PixelInfoFrame.Content is not null)
                UpdatePixelInfo(pixelPosition);
            if (SlidingWindowFrame.Content is not null)
                UpdateSlidingWindow(pixelPosition);
            if (SlidingWindowInfoFrame.Content is not null)
                UpdateSlidingWindowInfo(pixelPosition);
        }

        private void UpdatePixelInfo(Point pixelPosition)
        {
            var pixelInfo = (PixelInfo) PixelInfoFrame.Content;
            UpdateCoordinate(pixelInfo.Coordinates, pixelPosition);
            UpdateRgb((BitmapImage) MainImage.Source, pixelInfo, pixelPosition);
        }

        private static void UpdateCoordinate(TextBlock coordinates, Point pixelPosition)
        {
            coordinates.Text = $"{(int) pixelPosition.X}, {(int) pixelPosition.Y}";
        }

        private static void UpdateRgb(BitmapSource source, PixelInfo pixelInfo, Point pixelPosition)
        {
            var pixelColor = GetPixelColor(source, pixelPosition);
            pixelInfo.RedValue.Text = pixelColor.R.ToString();
            pixelInfo.GreenValue.Text = pixelColor.G.ToString();
            pixelInfo.BlueValue.Text = pixelColor.B.ToString();
            pixelInfo.BrightnessValue.Text = ((pixelColor.R + pixelColor.G + pixelColor.B) / 3).ToString();
        }

        private static Color GetPixelColor(BitmapSource source, Point pixelPosition)
        {
            var pixelBytes = GetPixels(
                source,
                (int) pixelPosition.X,
                (int) pixelPosition.Y,
                1,
                1);
            return Color.FromRgb(pixelBytes[0], pixelBytes[1], pixelBytes[2]);
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

        private void UpdateSlidingWindow(Point pixelPosition)
        {
            _slidingWindowPixels = GetSlidingWindowPixels((BitmapImage) MainImage.Source, pixelPosition);

            var slidingWindowImage = ((SlidingWindow) SlidingWindowFrame.Content).SlidingWindowImage;
            UpdateSlidingWindowImage((BitmapImage) MainImage.Source, slidingWindowImage, pixelPosition);
        }

        private static byte[] GetSlidingWindowPixels(BitmapSource source, Point pixelPosition)
        {
            var ((x, y), (width, height)) = GetSlidingWindowLocationAndSize(source, pixelPosition);
            return GetPixels(source, x, y, width, height);
        }

        private static ((int X, int Y), (int Width, int Height)) GetSlidingWindowLocationAndSize(
            BitmapSource source,
            Point pixelPosition)
        {
            var location = (X: (int) pixelPosition.X - 5, Y: (int) pixelPosition.Y - 5);
            var size = (Width: 11, Height: 11);

            if (pixelPosition.X - 5 < 0)
            {
                size.Width += location.X;
                location.X = 0;
            }

            if (pixelPosition.Y - 5 < 0)
            {
                size.Height += location.Y;
                location.Y = 0;
            }

            if (location.X + 11 > source.PixelWidth)
                size.Width = source.PixelWidth - location.X;
            if (location.Y + 11 > source.PixelHeight)
                size.Height = source.PixelHeight - location.Y;

            return (location, size);
        }

        private static void UpdateSlidingWindowImage(BitmapSource source, Image slidingWindowImage, Point pixelPosition)
        {
            var slidingWindowPixels = GetSlidingWindowPixels(source, pixelPosition);
            var (_, (width, height)) = GetSlidingWindowLocationAndSize(source, pixelPosition);
            slidingWindowImage.Source = CreateImageFromPixels(source, slidingWindowPixels, width, height, width * 4);
        }

        internal static BitmapSource CreateImageFromPixels(BitmapSource source,
            byte[] pixels,
            int width,
            int height,
            int stride)
        {
            return BitmapSource.Create(
                width,
                height,
                source.DpiX,
                source.DpiY,
                source.Format,
                source.Palette,
                pixels,
                stride);
        }

        private void ToggleSlidingWindowInfoCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ChangeFrameContent(SlidingWindowInfoFrame,
                SlidingWindowInfoFrame.Content is null
                    ? new SlidingWindowInfo()
                    : null);
        }

        private void UpdateSlidingWindowInfo(Point pixelPosition)
        {
            var slidingWindowInfo = (SlidingWindowInfo) SlidingWindowInfoFrame.Content;
            UpdateCoordinate(slidingWindowInfo.Coordinates, pixelPosition);
            UpdateMath(slidingWindowInfo);
        }

        private void UpdateMath(SlidingWindowInfo slidingWindowInfo)
        {
            slidingWindowInfo.MeanValue.Text = GetSlidingWindowMean()
                .ToString(CultureInfo.CurrentCulture);
            slidingWindowInfo.StandardDeviationValue.Text = GetSlidingWindowStandardDeviation()
                .ToString(CultureInfo.CurrentCulture);
            slidingWindowInfo.MedianValue.Text = GetSlidingWindowMedian()
                .ToString(CultureInfo.CurrentCulture);
        }

        private double GetSlidingWindowMean()
        {
            var brightnessSum = 0.0;
            for (var i = 0; i < _slidingWindowPixels.Length; i += 4)
                brightnessSum += GetPixelBrightness(_slidingWindowPixels, i);
            return Math.Round(GetMeanFromSum(brightnessSum), 3);
        }

        private double GetMeanFromSum(double sum)
        {
            return sum / (_slidingWindowPixels.Length / 4.0);
        }

        internal static double GetPixelBrightness(byte[] source, int index)
        {
            //Находится среднее значение между красным, зеленым и синим каналом
            return source.Skip(index).Take(3).Select(x => (int) x).Sum() / 3.0;
        }

        private double GetSlidingWindowStandardDeviation()
        {
            var differenceSquareSum = 0.0;
            var meanBrightness = GetSlidingWindowMean();
            for (var i = 0; i < _slidingWindowPixels.Length; i += 4)
                //Из яркости одного пикселя вычетается средняя яркость изображения и возводится в квадрат
                differenceSquareSum += Math.Pow(GetPixelBrightness(_slidingWindowPixels, i) - meanBrightness, 2);
            return Math.Round(Math.Sqrt(GetMeanFromSum(differenceSquareSum)), 3);
        }

        private double GetSlidingWindowMedian()
        {
            var brightnessList = new List<double>();
            for (var i = 0; i < _slidingWindowPixels.Length; i += 4)
                //Сборка всех яркостей в один List
                brightnessList.Add(GetPixelBrightness(_slidingWindowPixels, i));
            return Math.Round(GetMedianFromList(brightnessList), 3);
        }

        private static double GetMedianFromList(IEnumerable<double> source)
        {
            var data = source.OrderBy(x => x).ToArray();
            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
            return data[data.Length / 2];
        }

        private void ToggleBrightnessProfileCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ChangeFrameContent(BrightnessProfileFrame,
                BrightnessProfileFrame.Content is null
                    ? new BrightnessProfile(this)
                    : null);
        }

        private void ToggleImagePropertiesCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ChangeFrameContent(ImagePropertiesFrame,
                ImagePropertiesFrame.Content is null
                    ? new ImageProperties(this)
                    : null);
        }
    }
}