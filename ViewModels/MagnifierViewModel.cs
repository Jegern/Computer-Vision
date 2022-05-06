using System;
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
    private uint _size = 11;
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

    public uint Size
    {
        get => _size;
        set => Set(ref _size, value);
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

    public MagnifierViewModel(ViewModelStore? store) : base(store)
    {
        if (store is not null) store.MousePositionChanged += MousePosition_OnChanged;
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
        if (Location.X - Size / 2.0 < 0 ||
            Location.X + Size / 2.0 > Picture!.PixelWidth) return;
        if (Location.Y - Size / 2.0 < 0 ||
            Location.Y + Size / 2.0 > Picture!.PixelHeight) return;

        var size = (int) Size;
        var magnifierPixels = Tools.GetPixelBytes(
            Picture!,
            (int) (Location.X - Size / 2.0),
            (int) (Location.Y - Size / 2.0),
            size,
            size);
        Window = Tools.CreateImage(
            Picture!,
            magnifierPixels,
            size,
            size);
    }

    private void UpdateMagnifierInfo()
    {
        var magnifierWindowBytes = Tools.GetPixelBytes(Window!);
        var magnifierWindowLength = Window!.PixelWidth * Window!.PixelHeight;
        Mean = GetMagnifierMean(magnifierWindowBytes, magnifierWindowLength);
        Deviation = GetMagnifierDeviation(magnifierWindowBytes, magnifierWindowLength);
        Median = GetMagnifierMedian(magnifierWindowBytes, magnifierWindowLength);
    }

    /// <summary>
    /// The sum of the intensity of all pixels divided by the number of pixels
    /// </summary>
    private static double GetMagnifierMean(byte[] bytes, int length)
    {
        var intesivitySum = 0d;
        for (var i = 0; i < length; i++)
            intesivitySum += Tools.GetPixelIntensity(bytes, i * 4);
        return Math.Round(intesivitySum / length, 3);
    }

    /// <summary>
    /// The square root of the sum of the squares of
    /// the difference in pixel intensity and the average intensity of all pixels
    /// </summary>
    private static double GetMagnifierDeviation(byte[] bytes, int length)
    {
        var magnifierMean = GetMagnifierMean(bytes, length);
        var differenceSquareSum = 0d;
        for (var i = 0; i < length; i++)
            differenceSquareSum += Math.Pow(Tools.GetPixelIntensity(bytes, i * 4) - magnifierMean, 2);
        return Math.Round(Math.Sqrt(differenceSquareSum / length), 3);
    }

    /// <summary>
    /// The median of the intensity of all pixels
    /// </summary>
    private static double GetMagnifierMedian(byte[] bytes, int length)
    {
        var intensityList = new List<double>();
        for (var i = 0; i < length; i += 4)
            intensityList.Add(Tools.GetPixelIntensity(bytes, i * 4));
        return Math.Round(Tools.GetMedianFromList(intensityList), 3);
    }

    #endregion
}