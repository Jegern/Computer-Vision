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

    private bool _pTileChecked;
    private bool _approximationsChecked;
    private bool _kMeansChecked;
    private bool _histogramPeaksChecked;

    private BitmapSource? Picture
    {
        get => _picture;
        set => Set(ref _picture, value);
    }

    private byte[]? PictureBytes
    {
        get => _pictureBytes;
        set => Set(ref _pictureBytes, value);
    }

    public Visibility Visibility
    {
        get => _visibility;
        set => Set(ref _visibility, value);
    }

    public bool PTileChecked
    {
        get => _pTileChecked;
        set => Set(ref _pTileChecked, value);
    }

    public bool ApproximationsChecked
    {
        get => _approximationsChecked;
        set => Set(ref _approximationsChecked, value);
    }

    public bool KMeansChecked
    {
        get => _kMeansChecked;
        set => Set(ref _kMeansChecked, value);
    }

    public bool HistogramPeaksChecked
    {
        get => _histogramPeaksChecked;
        set => Set(ref _histogramPeaksChecked, value);
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
        ThresholdMethodCommand = new Command(
            ThresholdMethodCommand_OnExecuted,
            ThresholdMethodCommand_CanExecute);
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

    #region ThresholdMethodCommand

    public Command? ThresholdMethodCommand { get; }

    private bool ThresholdMethodCommand_CanExecute(object? parameter) => Picture is not null;

    private void ThresholdMethodCommand_OnExecuted(object? parameter)
    {
        var histogram = new int[256];
        for (var i = 0; i < PictureBytes!.Length; i += 4)
            histogram[(byte) Tools.GetPixelIntensity(PictureBytes, i)]++;

        double maxSum;
        var sum = maxSum = histogram.Sum();
        var threshold = 0;
        const double p = 0.5;
        for (var i = 0; i < histogram.Length; i++)
        {
            sum -= histogram[i];
            if (!(sum / maxSum < p)) continue;
            threshold = i;
            break;
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

    #endregion
}