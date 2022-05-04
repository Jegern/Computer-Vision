using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class SegmentationViewModel : ViewModel
{
    #region Fields

    private readonly ViewModelStore? _store;
    private BitmapSource? _picture;
    private byte[]? _pictureBytes;
    private Visibility _visibility = Visibility.Collapsed;

    private int? _areaPercentage;
    private int? _kNeighbours;

    private BitmapSource? Picture
    {
        get => _picture;
        set => Set(ref _picture, value);
    }

    private byte[]? PictureBytes
    {
        get => _pictureBytes;
        set
        {
            if (!Set(ref _pictureBytes, value)) return;
            for (var i = 0; i < PictureBytes!.Length; i += 4)
                Histogram[(byte) Tools.GetPixelIntensity(PictureBytes, i)]++;
        }
    }

    private static int[] Histogram { get; } = new int[256];

    public Visibility Visibility
    {
        get => _visibility;
        set => Set(ref _visibility, value);
    }

    public int? AreaPercentage
    {
        get => _areaPercentage;
        set => Set(ref _areaPercentage, value);
    }
    
    public int? KNeighbours
    {
        get => _kNeighbours;
        set => Set(ref _kNeighbours, value);
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public SegmentationViewModel()
    {
    }

    public SegmentationViewModel(ViewModelStore? store)
    {
        if (store is null) return;

        store.PictureChanged += Picture_OnChanged;
        store.PictureBytesChanged += PictureBytes_OnChanged;
        _store = store;

        SegmentationCommand = new Command(
            SegmentationCommand_OnExecuted,
            SegmentationCommand_CanExecute);
        PTileCommand = new Command(
            PTileCommand_OnExecuted,
            PTileCommand_CanExecute);
        HistogramDependentCommand = new Command(
            HistogramDependentCommand_OnExecuted,
            HistogramDependentCommand_CanExecute);
        KMeansCommand = new Command(
            KMeansCommand_OnExecuted,
            KMeansCommand_CanExecute);
    }

    #region Event Subscription

    private void Picture_OnChanged(BitmapSource? source)
    {
        Picture = source;
    }

    private void PictureBytes_OnChanged(byte[] bytes)
    {
        PictureBytes = bytes;
    }

    #endregion

    #region Commands

    #region SegmentationCommand

    public Command? SegmentationCommand { get; }

    private bool SegmentationCommand_CanExecute(object? parameter) => Picture is not null;

    private void SegmentationCommand_OnExecuted(object? parameter)
    {
        Visibility = Visibility is Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    #endregion

    #region PTileCommand

    public Command? PTileCommand { get; }

    private bool PTileCommand_CanExecute(object? parameter) =>
        Picture is not null &&
        AreaPercentage is not null;

    private void PTileCommand_OnExecuted(object? parameter)
    {
        var sum = Histogram.Sum();
        var remainder = sum;
        int threshold;
        for (threshold = 0; threshold < 256; threshold++)
        {
            remainder -= Histogram[threshold];
            if (100 * remainder / sum < AreaPercentage) break;
        }

        for (var i = 0; i < PictureBytes!.Length; i += 4)
        {
            var intensity = Tools.GetPixelIntensity(PictureBytes, i);
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, i),
                Tools.GetGrayPixel((byte) (intensity <= threshold ? 0 : 255)));
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region HistogramDependentCommand

    public Command? HistogramDependentCommand { get; }

    private bool HistogramDependentCommand_CanExecute(object? parameter) => Picture is not null;

    private void HistogramDependentCommand_OnExecuted(object? parameter)
    {
        var oldThreshold = 127;
        var threshold = CalculateNewThreshold(oldThreshold);

        while (Math.Abs(oldThreshold - threshold) == 0)
        {
            oldThreshold = threshold;
            threshold = CalculateNewThreshold(oldThreshold);
        }

        for (var i = 0; i < PictureBytes!.Length; i += 4)
        {
            var intensity = Tools.GetPixelIntensity(PictureBytes, i);
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, i),
                Tools.GetGrayPixel((byte) (intensity <= threshold ? 0 : 255)));
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    private static int CalculateNewThreshold(int threshold)
    {
        var leftSum = 0;
        for (var i = 0; i < threshold; i++)
            leftSum += Histogram[i] * i;
        var leftMean = leftSum / Histogram.Take(threshold).Sum();

        var rightSum = 0;
        for (var i = threshold; i < 256; i++)
            rightSum += Histogram[i] * i;
        var rightMean = rightSum / Histogram.Take(new Range(threshold, 256)).Sum();

        return (leftMean + rightMean) / 2;
    }

    #endregion

    #region KMeansCommand

    public Command? KMeansCommand { get; }

    private bool KMeansCommand_CanExecute(object? parameter) => Picture is not null;

    private void KMeansCommand_OnExecuted(object? parameter)
    {
        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #endregion
}