using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class MagnifierViewModel : ViewModel
{
    #region Fields

    private BitmapSource? _window;
    private Point _location;
    private byte[] _windowBytes = new byte[11 * 11 * 4];
    private int _windowSide = 11;
    private double _mean;
    private double _deviation;
    private double _median;

    private Point Location
    {
        get => _location;
        set => Set(ref _location, value);
    }

    public BitmapSource? Window
    {
        get => _window;
        set => Set(ref _window, value);
    }

    private byte[] WindowBytes
    {
        get => _windowBytes;
        set => Set(ref _windowBytes, value);
    }

    public int WindowSide
    {
        get => _windowSide;
        set
        {
            if (Set(ref _windowSide, value))
                WindowBytes = new byte[_windowSide * _windowSide * 4];
        }
    }

    public double Mean
    {
        get => _mean;
        set => Set(ref _mean, value);
    }

    public double Deviation
    {
        get => _deviation;
        set => Set(ref _deviation, value);
    }

    public double Median
    {
        get => _median;
        set => Set(ref _median, value);
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public MagnifierViewModel()
    {
    }

    public MagnifierViewModel(ViewModelStore store) : base(store)
    {
        store.MousePositionChanged += MousePosition_OnChanged;
    }

    #region Event Subscription

    private void MousePosition_OnChanged(Point point)
    {
        if (Visibility is Visibility.Collapsed) return;

        Location = point;
        UpdateMagnifierWindow();
        UpdateMagnifierInfo();
    }

    private void UpdateMagnifierWindow()
    {
        var width = (int) PictureSize.Width;
        var height = (int) PictureSize.Height;
        var i = (int) (Location.Y - WindowSide / 2.0);
        var j = (int) (Location.X - WindowSide / 2.0);
        for (var y = i; y < i + WindowSide; y++)
        for (var x = j; x < j + WindowSide; x++)
        {
            var magnifierPixelIndex = ((y - i) * WindowSide + x - j) * 4;
            if (y < 0 | y >= height || x < 0 | x >= width)
            {
                Tools.SetPixel(
                    Tools.GetPixel(WindowBytes, magnifierPixelIndex),
                    Tools.GetGrayPixel(0));
            }
            else
            {
                var windowPixelIndex = y * width * 4 + x * 4;
                Tools.SetPixel(
                    Tools.GetPixel(WindowBytes, magnifierPixelIndex),
                    Tools.GetPixel(PictureBytes!, windowPixelIndex));
            }
        }

        Window = Tools.CreateImage(WindowBytes, WindowSide, WindowSide);
    }

    private void UpdateMagnifierInfo()
    {
        Mean = GetMagnifierMean();
        Deviation = GetMagnifierDeviation();
        Median = GetMagnifierMedian();
    }

    /// <summary>
    /// The sum of the intensity of all pixels divided by the number of pixels
    /// </summary>
    private double GetMagnifierMean()
    {
        var intesivitySum = 0d;
        for (var i = 0; i < WindowBytes.Length; i += 4)
            intesivitySum += Tools.GetPixelIntensity(WindowBytes, i);
        return Math.Round(intesivitySum * 4 / WindowBytes.Length, 3);
    }

    /// <summary>
    /// The square root of the sum of the squares of
    /// the difference in pixel intensity and the average intensity of all pixels
    /// </summary>
    private double GetMagnifierDeviation()
    {
        var magnifierMean = GetMagnifierMean();
        var differenceSquareSum = 0d;
        for (var i = 0; i < WindowBytes.Length; i += 4)
            differenceSquareSum += Math.Pow(Tools.GetPixelIntensity(WindowBytes, i) - magnifierMean, 2);
        return Math.Round(Math.Sqrt(differenceSquareSum * 4 / WindowBytes.Length), 3);
    }

    /// <summary>
    /// The median of the intensity of all pixels
    /// </summary>
    private double GetMagnifierMedian()
    {
        var intensityList = new List<double>();
        for (var i = 0; i < WindowBytes.Length; i += 4)
            intensityList.Add(Tools.GetPixelIntensity(WindowBytes, i));
        return Math.Round(GetMedianFromList(intensityList), 3);
    }

    private static double GetMedianFromList(IEnumerable<double> list)
    {
        var data = list.OrderBy(x => x).ToArray();
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        return data[data.Length / 2];
    }

    #endregion
}