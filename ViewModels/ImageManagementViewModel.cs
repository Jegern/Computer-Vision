using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class ImageManagementViewModel : ViewModel
{
    #region Fields

    private readonly ViewModelStore? _store;
    private BitmapSource? _picture;
    private byte[]? _pictureBytes;
    private Visibility _imageManagementVisibility = Visibility.Collapsed;

    private int _intensity = 127;
    private int _channelCounter;
    private bool _redChecked;
    private bool _greenChecked;
    private bool _blueChecked;
    private bool _horizontalChecked;
    private bool _verticalChecked;
    private bool _horizontalVerticalChecked;
    private bool _diagonalChecked;


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

    public Visibility ImageManagementVisibility
    {
        get => _imageManagementVisibility;
        set => Set(ref _imageManagementVisibility, value);
    }

    public int Intensity
    {
        get => _intensity;
        set => Set(ref _intensity, value);
    }

    private int ChannelCounter
    {
        get => _channelCounter;
        set => Set(ref _channelCounter, value);
    }

    public bool RedChecked
    {
        get => _redChecked;
        set
        {
            if (ChannelCounter == 2 && value) return;
            Set(ref _redChecked, value);
            ChannelCounter += value ? 1 : -1;
        }
    }

    public bool GreenChecked
    {
        get => _greenChecked;
        set
        {
            if (ChannelCounter == 2 && value) return;
            Set(ref _greenChecked, value);
            ChannelCounter += value ? 1 : -1;
        }
    }

    public bool BlueChecked
    {
        get => _blueChecked;
        set
        {
            if (ChannelCounter == 2 && value) return;
            Set(ref _blueChecked, value);
            ChannelCounter += value ? 1 : -1;
        }
    }

    public bool HorizontalChecked
    {
        get => _horizontalChecked;
        set => Set(ref _horizontalChecked, value);
    }

    public bool VerticalChecked
    {
        get => _verticalChecked;
        set => Set(ref _verticalChecked, value);
    }

    public bool HorizontalVerticalChecked
    {
        get => _horizontalVerticalChecked;
        set => Set(ref _horizontalVerticalChecked, value);
    }

    public bool DiagonalChecked
    {
        get => _diagonalChecked;
        set => Set(ref _diagonalChecked, value);
    }

    private bool[,] HorizontalVerticalMask { get; } =
    {
        {false, true, false},
        {true, true, true},
        {false, true, false}
    };

    private bool[,] DiagonalMask { get; } =
    {
        {true, false, true},
        {false, true, false},
        {true, false, true}
    };

    private bool[,] AllMask { get; } =
    {
        {true, true, true},
        {true, true, true},
        {true, true, true}
    };

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public ImageManagementViewModel()
    {
    }

    public ImageManagementViewModel(ViewModelStore? store)
    {
        if (store is null) return;

        store.PictureChanged += Picture_OnChanged;
        store.PictureBytesChanged += PictureBytes_OnChanged;
        _store = store;

        ImageManagementCommand = new Command(ImageManagementCommand_OnExecuted, ImageManagementCommand_CanExecute);
        ValueChangedCommand = new Command(ValueChangedCommand_OnExecuted, ValueChangedCommand_CanExecute);
        BleachCommand = new Command(BleachCommand_OnExecuted, BleachCommand_CanExecute);
        NegativeCommand = new Command(NegativeCommand_OnExecuted, NegativeCommand_CanExecute);
        SwapCommand = new Command(SwapCommand_OnExecuted, SwapCommand_CanExecute);
        SymmetryCommand = new Command(SymmetryCommand_OnExecuted, SymmetryCommand_CanExecute);
        VanishCommand = new Command(VanishCommand_OnExecuted, VanishCommand_CanExecute);
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

    #region ImageManagementCommand

    public Command? ImageManagementCommand { get; }

    private bool ImageManagementCommand_CanExecute(object? parameter) => Picture is not null;

    private void ImageManagementCommand_OnExecuted(object? parameter)
    {
        ImageManagementVisibility = ImageManagementVisibility is Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    #endregion

    #region ValueChangedCommand

    public Command? ValueChangedCommand { get; }

    private bool ValueChangedCommand_CanExecute(object? parameter) => Picture is not null;

    private void ValueChangedCommand_OnExecuted(object? parameter)
    {
        var e = (RoutedPropertyChangedEventArgs<double>) parameter!;
        if (e.OldValue - e.NewValue == 0) return;

        var difference = e.OldValue - e.NewValue;

        for (var i = 0; i < PictureBytes!.Length; i += 4)
        {
            if (0 <= PictureBytes[i + 0] + difference && PictureBytes[i + 0] + difference <= 255)
                PictureBytes[i + 0] = (byte)(PictureBytes[i + 0] + difference);
            if (0 <= PictureBytes[i + 1] + difference && PictureBytes[i + 1] + difference <= 255)
                PictureBytes[i + 1] = (byte)(PictureBytes[i + 1] + difference);
            if (0 <= PictureBytes[i + 2] + difference && PictureBytes[i + 2] + difference <= 255)
                PictureBytes[i + 2] = (byte)(PictureBytes[i + 2] + difference);
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region BleachCommand

    public Command? BleachCommand { get; }

    private bool BleachCommand_CanExecute(object? parameter) => Picture is not null;

    private void BleachCommand_OnExecuted(object? parameter)
    {
        for (var i = 0; i < PictureBytes!.Length; i += 4)
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, i),
                Tools.GetGrayPixel((byte) Tools.GetPixelIntensity(PictureBytes, i)));
        
        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region NegativeCommand

    public Command? NegativeCommand { get; }

    private bool NegativeCommand_CanExecute(object? parameter) => Picture is not null;

    private void NegativeCommand_OnExecuted(object? parameter)
    {
        for (var i = 0; i < PictureBytes!.Length; i += 4)
        {
            PictureBytes[i + 0] = (byte) (255 - PictureBytes[i + 0]);
            PictureBytes[i + 1] = (byte) (255 - PictureBytes[i + 1]);
            PictureBytes[i + 2] = (byte) (255 - PictureBytes[i + 2]);
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region SwapCommand

    public Command? SwapCommand { get; }

    private bool SwapCommand_CanExecute(object? parameter) => Picture is not null && ChannelCounter == 2;

    private void SwapCommand_OnExecuted(object? parameter)
    {
        var firstChannel = RedChecked ? 2 : 1;
        var secondChannel = BlueChecked ? 0 : 1;

        for (var i = 0; i < PictureBytes!.Length; i += 4)
            Tools.Swap(ref PictureBytes[i + firstChannel], ref PictureBytes[i + secondChannel]);
        
        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region SymmetryCommand

    public Command? SymmetryCommand { get; }

    private bool SymmetryCommand_CanExecute(object? parameter) =>
        Picture is not null &&
        (HorizontalChecked || VerticalChecked);

    private void SymmetryCommand_OnExecuted(object? parameter)
    {
        var width = Picture!.PixelWidth;
        var height = Picture!.PixelHeight;

        if (HorizontalChecked)
            for (var i = 0; i < height / 2; i++)
            for (var j = 0; j < width; j++)
                Tools.SwapPixels(
                    Tools.GetPixel(PictureBytes!, i * width * 4 + j * 4),
                    Tools.GetPixel(PictureBytes!, (height - 1 - i) * width * 4 + j * 4));

        if (VerticalChecked)
            for (var i = 0; i < height; i++)
            for (var j = 0; j < width / 2; j++)
                Tools.SwapPixels(
                    Tools.GetPixel(PictureBytes!, i * width * 4 + j * 4),
                    Tools.GetPixel(PictureBytes!, i * width * 4 + (width - j - 1) * 4));

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region VanishCommand

    public Command? VanishCommand { get; }

    private bool VanishCommand_CanExecute(object? parameter) =>
        Picture is not null &&
        (HorizontalVerticalChecked || DiagonalChecked);

    private void VanishCommand_OnExecuted(object? parameter)
    {
        var originalPictureBytes = (byte[]) PictureBytes!.Clone();
        var width = Picture!.PixelWidth;
        var height = Picture!.PixelHeight;
        var mask = HorizontalVerticalChecked switch
        {
            true when DiagonalChecked => AllMask,
            true => HorizontalVerticalMask,
            false => DiagonalMask
        };

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var sum = 0d;
            var counter = 0;
            for (var i = -1; i <= 1; i++)
            {
                if (y + i < 0 | y + i >= height) continue;
                for (var j = -1; j <= 1; j++)
                {
                    if (j + x < 0 | j + x >= width) continue;
                    if (!mask[i + 1, j + 1]) continue;
                    var windowPixelIndex = (y + i) * width * 4 + (x + j) * 4;
                    var windowPixel = Tools.GetPixelIntensity(originalPictureBytes, windowPixelIndex);
                    sum += windowPixel;
                    counter++;
                }
            }

            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, y * width * 4 + x * 4),
                Tools.GetGrayPixel((byte) (sum / counter)));
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #endregion
}