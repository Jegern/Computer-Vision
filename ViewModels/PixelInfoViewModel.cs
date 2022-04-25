using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Stores;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;

namespace Laboratory_work_1.ViewModels;

public class PixelInfoViewModel : ViewModel
{
    #region Fields

    private BitmapSource? _picture;
    private Visibility _pixelInfoVisibility = Visibility.Collapsed;
    private Point _pixelLocation;
    private byte _pixelRed;
    private byte _pixelGreen;
    private byte _pixelBlue;
    private byte _pixelIntensivity;

    private BitmapSource? Picture
    {
        get => _picture;
        set => Set(ref _picture, value);
    }

    public Visibility PixelInfoVisibility
    {
        get => _pixelInfoVisibility;
        set => Set(ref _pixelInfoVisibility, value);
    }

    public Point PixelLocation
    {
        get => _pixelLocation;
        set => Set(ref _pixelLocation, value);
    }

    public byte PixelRed
    {
        get => _pixelRed;
        set => Set(ref _pixelRed, value);
    }

    public byte PixelGreen
    {
        get => _pixelGreen;
        set => Set(ref _pixelGreen, value);
    }

    public byte PixelBlue
    {
        get => _pixelBlue;
        set => Set(ref _pixelBlue, value);
    }

    public byte PixelIntensivity
    {
        get => _pixelIntensivity;
        set => Set(ref _pixelIntensivity, value);
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public PixelInfoViewModel()
    {
        
    }

    public PixelInfoViewModel(Store? store)
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
        if (PixelInfoVisibility is Visibility.Collapsed) return;
        
        PixelLocation = point;
        UpdatePixelInfo(point);
    }

    private void UpdatePixelInfo(Point point)
    {
        var pixelColor = Tools.GetPixelColor(Picture, point);
        PixelRed = pixelColor.R;
        PixelGreen = pixelColor.G;
        PixelBlue = pixelColor.B;
        PixelIntensivity = (byte) ((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
    }

    #endregion
    
    #region PixelInfoCommand

    public Command? PixelInfoCommand { get; }

    private bool PixelInfoCommand_CanExecute(object? parameter) => Picture is not null;

    private void PixelInfoCommand_OnExecuted(object? parameter)
    {
        PixelInfoVisibility = PixelInfoVisibility is Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion
}