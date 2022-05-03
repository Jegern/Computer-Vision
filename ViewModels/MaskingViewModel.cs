using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class MaskingViewModel : ViewModel
{
    
    #region Fields

    private readonly ViewModelStore? _store;
    private BitmapSource? _picture;
    private byte[]? _pictureBytes;
    private byte[]? _antiAliasingPictureBytes;
    private Visibility _visibility = Visibility.Collapsed;

    private double? _lambda;

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
    
    private byte[]? AntiAliasingPictureBytes
    {
        get => _antiAliasingPictureBytes;
        set => Set(ref _antiAliasingPictureBytes, value);
    }

    public Visibility Visibility
    {
        get => _visibility;
        set => Set(ref _visibility, value);
    }

    public double? Lambda
    {
        get => _lambda;
        set => Set(ref _lambda, value);
    }

    #endregion
    
    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public MaskingViewModel()
    {
    }

    public MaskingViewModel(ViewModelStore? store)
    {
        if (store is null) return;

        store.PictureChanged += Picture_OnChanged;
        store.PictureBytesChanged += PictureBytes_OnChanged;
        store.AntiAliasingPictureBytesChanged += AntiAliasingPictureBytes_OnChanged;
        _store = store;

        MaskingCommand = new Command(
            MaskingCommand_OnExecuted,
            MaskingCommand_CanExecute);
        SoftMaskingCommand = new Command(
            SoftMaskingCommand_OnExecuted,
            SoftMaskingCommand_CanExecute);
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

    private void AntiAliasingPictureBytes_OnChanged(byte[] bytes)
    {
        AntiAliasingPictureBytes = bytes;
    }

    #endregion
    
    #region Commands

    #region MaskingCommand

    public Command? MaskingCommand { get; }

    private bool MaskingCommand_CanExecute(object? parameter) => Picture is not null;

    private void MaskingCommand_OnExecuted(object? parameter)
    {
        Visibility = Visibility is Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    #endregion
    
    #region SoftMaskingCommand

    public Command? SoftMaskingCommand { get; }

    private bool SoftMaskingCommand_CanExecute(object? parameter) => 
        Picture is not null &&
        Lambda is not null &&
        AntiAliasingPictureBytes is not null;

    private void SoftMaskingCommand_OnExecuted(object? parameter)
    {
        var lambda = (double) Lambda!;
        
        for (var i = 0; i < PictureBytes!.Length; i += 4)
        {
            var originalIntensity = Tools.GetPixelIntensity(AntiAliasingPictureBytes!, i);
            var currentIntensity = Tools.GetPixelIntensity(PictureBytes, i);
            var intensity = (1 + lambda) * originalIntensity - lambda * currentIntensity;
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, i),
                Tools.GetGrayPixel((byte) intensity));
        }
        
        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion
    
    #endregion
}