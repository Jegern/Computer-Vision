using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.ViewModels.Base;

namespace Laboratory_work_1.ViewModels;

public class PixelInfoViewModel : ViewModel
{
    public PixelInfoViewModel()
    {
    }

    public PixelInfoViewModel(Store store)
    {
        store.PictureChanged += Picture_OnChanged;
        store.PixelInfoVisibilityChanged += PixelInfoVisibility_OnChanged;
        store.MousePositionChanged += PixelInfoLocation_OnChanged;
    }

    private void Picture_OnChanged(BitmapImage? source)
    {
        Picture = source;
    }

    private void PixelInfoVisibility_OnChanged(Visibility visibility)
    {
        PixelInfoVisibility = visibility;
    }

    private void PixelInfoLocation_OnChanged(Point? point)
    {
        if (point is null) return;

        var roundedPoint = new Point((int) point.Value.X, (int) point.Value.Y);
        PixelLocation = roundedPoint;
        UpdatePixelRgb(roundedPoint);
    }

    private void UpdatePixelRgb(Point point)
    {
        var pixelColor = Tools.GetPixelColor(Picture, point);
        PixelRed = pixelColor.R;
        PixelGreen = pixelColor.G;
        PixelBlue = pixelColor.B;
        PixelIntensivity = (byte) ((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
    }

    private BitmapImage? _picture;
    private Visibility _pixelInfoVisibility = Visibility.Collapsed;
    private Point _pixelLocation;
    private byte? _pixelRed;
    private byte? _pixelGreen;
    private byte? _pixelBlue;
    private byte? _pixelIntensivity;

    private BitmapImage? Picture
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

    public byte? PixelRed
    {
        get => _pixelRed;
        set => Set(ref _pixelRed, value);
    }

    public byte? PixelGreen
    {
        get => _pixelGreen;
        set => Set(ref _pixelGreen, value);
    }

    public byte? PixelBlue
    {
        get => _pixelBlue;
        set => Set(ref _pixelBlue, value);
    }

    public byte? PixelIntensivity
    {
        get => _pixelIntensivity;
        set => Set(ref _pixelIntensivity, value);
    }
}