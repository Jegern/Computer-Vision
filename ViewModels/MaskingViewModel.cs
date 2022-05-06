using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class MaskingViewModel : ViewModel
{
    
    #region Fields

    private byte[]? _antiAliasingPictureBytes;
    private double? _lambda;
    
    private byte[]? AntiAliasingPictureBytes
    {
        get => _antiAliasingPictureBytes;
        set => Set(ref _antiAliasingPictureBytes, value);
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

    public MaskingViewModel(ViewModelStore? store) : base(store)
    {
        if (store is not null) store.AntiAliasingPictureBytesChanged += AntiAliasingPictureBytes_OnChanged;
        
        SoftMaskingCommand = new Command(
            SoftMaskingCommand_OnExecuted,
            SoftMaskingCommand_CanExecute);
    }

    #region Event Subscription

    private void AntiAliasingPictureBytes_OnChanged(byte[] bytes)
    {
        AntiAliasingPictureBytes = bytes;
    }

    #endregion
    
    #region Commands
    
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
        
        Store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion
    
    #endregion
}