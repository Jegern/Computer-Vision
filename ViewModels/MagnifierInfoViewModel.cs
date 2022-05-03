using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class MagnifierInfoViewModel : ViewModel
{
    #region Fields

    private Visibility _visibility = Visibility.Collapsed;
    private Visibility _magnifierVisibility = Visibility.Collapsed;
    private BitmapSource? _window;
    private double _mean;
    private double _deviation;
    private double _median;

    public Visibility Visibility
    {
        get => _visibility;
        set => Set(ref _visibility, value);
    }

    private BitmapSource? Window
    {
        get => _window;
        set => Set(ref _window, value);
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
    public MagnifierInfoViewModel()
    {
        
    }

    public MagnifierInfoViewModel(ViewModelStore? store)
    {
        if (store is null) return;
        
        store.MagnifierVisibilityChanged += MagnifierVisibility_OnChanged;
        store.MagnifierWindowChanged += MagnifierWindow_OnChanged;

        MagnifierInfoCommand = new Command(MagnifierInfoCommand_OnExecuted, MagnifierInfoCommand_CanExecute);
    }

    

    #region Event Subscription
    
    private void MagnifierVisibility_OnChanged(Visibility visibility)
    {
        _magnifierVisibility = visibility;
        if (visibility is Visibility.Collapsed)
            Visibility = visibility;
    }

    private void MagnifierWindow_OnChanged(BitmapSource? source)
    {
        Window = source;
        UpdateMagnifierInfo();
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

    #region MagnifierInfoCommand

    public Command? MagnifierInfoCommand { get; }

    private bool MagnifierInfoCommand_CanExecute(object? parameter) => _magnifierVisibility is Visibility.Visible;

    private void MagnifierInfoCommand_OnExecuted(object? parameter)
    {
        Visibility = Visibility is Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    #endregion
}