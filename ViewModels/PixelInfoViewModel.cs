using System.Windows;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class PixelInfoViewModel : ViewModel
{
    #region Fields

    private Point _location;
    private byte _red;
    private byte _green;
    private byte _blue;
    private byte _intensity;

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

    public byte Intensity
    {
        get => _intensity;
        set => Set(ref _intensity, value);
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public PixelInfoViewModel()
    {
        
    }

    public PixelInfoViewModel(ViewModelStore? store) : base(store)
    {
        if (store is not null) store.MousePositionChanged += MousePosition_OnChanged;
    }

    #region Event Subscription

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
        Intensity = (byte) ((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
    }

    #endregion
}