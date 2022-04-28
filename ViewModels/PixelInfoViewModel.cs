using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class PixelInfoViewModel : ViewModel
{
    #region Fields

    private BitmapSource? _picture;
    private Visibility _visibility = Visibility.Collapsed;
    private Point _location;
    private byte _red;
    private byte _green;
    private byte _blue;
    private byte _intensivity;
    
    private BitmapSource? Picture
    {
        get => _picture;
        set => Set(ref _picture, value);
    }

    public Visibility Visibility
    {
        get => _visibility;
        set => Set(ref _visibility, value);
    }

    public Point Location
    {
        get => _location;
        set => Set(ref _location, value);
    }

    public byte Red
    {
        get => _red;
        set => Set(ref _red, value);
    }

    public byte Green
    {
        get => _green;
        set => Set(ref _green, value);
    }

    public byte Blue
    {
        get => _blue;
        set => Set(ref _blue, value);
    }

    public byte Intensivity
    {
        get => _intensivity;
        set => Set(ref _intensivity, value);
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public PixelInfoViewModel()
    {
        
    }

    public PixelInfoViewModel(ViewModelStore? store)
    {
        if (store is null) return;
        
        store.PictureChanged += Picture_OnChanged;
        store.MousePositionChanged += MousePosition_OnChanged;
        
        PixelInfoCommand = new Command(PixelInfoCommand_OnExecuted, PixelInfoCommand_CanExecute);
    }

    #region Event Subscription

    private void Picture_OnChanged(BitmapSource? source)
    {
        Picture = source;
    }

    private void MousePosition_OnChanged(Point point)
    {
        if (Visibility is Visibility.Collapsed) return;
        
        Location = point;
        UpdatePixelInfo(point);
    }

    private void UpdatePixelInfo(Point point)
    {
        var pixelColor = Tools.GetPixelColor(Picture, point);
        Red = pixelColor.R;
        Green = pixelColor.G;
        Blue = pixelColor.B;
        Intensivity = (byte) ((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
    }

    #endregion
    
    #region PixelInfoCommand

    public Command? PixelInfoCommand { get; }

    private bool PixelInfoCommand_CanExecute(object? parameter) => Picture is not null;

    private void PixelInfoCommand_OnExecuted(object? parameter)
    {
        Visibility = Visibility is Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    #endregion
}