using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class MagnifierViewModel : ViewModel
{
    #region Fields

    private readonly ViewModelStore? _store;
    private Visibility _visibility = Visibility.Collapsed;
    private BitmapSource? _picture;
    private BitmapSource? _window;
    private Point _location;
    private int _size = 11;

    private BitmapSource? Picture
    {
        get => _picture;
        set => Set(ref _picture, value);
    }

    public Visibility Visibility
    {
        get => _visibility;
        set
        {
            if (Set(ref _visibility, value))
                _store?.TriggerMagnifierVisibility(_visibility);
        }
    }

    private Point Location
    {
        get => _location;
        set => Set(ref _location, value);
    }

    public BitmapSource? Window
    {
        get => _window;
        set
        {
            if (Set(ref _window, value))
                _store?.TriggerMagnifierWindowEvent(_window);
        }
    }

    public int Size
    {
        get => _size;
        set => Set(ref _size, value);
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public MagnifierViewModel()
    {
    }

    public MagnifierViewModel(ViewModelStore? store)
    {
        if (store is null) return;

        _store = store;
        store.PictureChanged += Picture_OnChanged;
        store.MousePositionChanged += MousePosition_OnChanged;

        MagnifierCommand = new Command(MagnifierCommand_OnExecuted, MagnifierCommand_CanExecute);
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
        UpdateMagnifierWindow();
    }

    private void UpdateMagnifierWindow()
    {
        if (Size == 0) return;
        if (Location.X - Size / 2.0 < 0 ||
            Location.X + Size / 2.0 > Picture!.PixelWidth) return;
        if (Location.Y - Size / 2.0 < 0 ||
            Location.Y + Size / 2.0 > Picture!.PixelHeight) return;

        var magnifierPixels = Tools.GetPixelBytes(
            Picture!,
            (int) (Location.X - Size / 2.0),
            (int) (Location.Y - Size / 2.0),
            Size,
            Size);
        Window = Tools.CreateImage(
            Picture!,
            magnifierPixels,
            Size,
            Size);
    }

    #endregion

    #region MagnifierCommand

    public Command? MagnifierCommand { get; }

    private bool MagnifierCommand_CanExecute(object? parameter) => Picture is not null;

    private void MagnifierCommand_OnExecuted(object? parameter)
    {
        Visibility = Visibility is Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    #endregion
}