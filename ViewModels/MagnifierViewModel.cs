using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Stores;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;

namespace Laboratory_work_1.ViewModels;

public class MagnifierViewModel : ViewModel
{
    #region Fields

    private readonly Store? _store;
    private BitmapSource? _picture;
    private Visibility _magnifierVisibility = Visibility.Collapsed;
    private Point _magnifierLocation;
    private BitmapSource? _magnifierWindow;
    private int _magnifierSize = 11;

    private BitmapSource? Picture
    {
        get => _picture;
        set => Set(ref _picture, value);
    }

    public Visibility MagnifierVisibility
    {
        get => _magnifierVisibility;
        set => Set(ref _magnifierVisibility, value);
    }

    private Point MagnifierLocation
    {
        get => _magnifierLocation;
        set => Set(ref _magnifierLocation, value);
    }

    public BitmapSource? MagnifierWindow
    {
        get => _magnifierWindow;
        set
        {
            Set(ref _magnifierWindow, value); 
            _store?.TriggerMagnifierWindowEvent(_magnifierWindow);
        }
    }

    public int MagnifierSize
    {
        get => _magnifierSize;
        set => Set(ref _magnifierSize, value);
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public MagnifierViewModel()
    {
        
    }

    public MagnifierViewModel(Store? store)
    {
       if (store is null) return;
        
        _store = store;
        store.PictureChanged += Picture_OnChanged;
        store.MousePositionChanged += MousePosition_OnChanged;
        
        MagnifierCommand = new Command(MagnifierCommand_OnExecuted, MagnifierCommand_CanExecute);
    }

    #region Event Subscription

    private void Picture_OnChanged(BitmapSource? picture)
    {
        Picture = picture;
    }

    private void MousePosition_OnChanged(Point point)
    {
        if (MagnifierVisibility is Visibility.Collapsed) return;
        
        MagnifierLocation = point;
        UpdateMagnifierWindow();
    }

    private void UpdateMagnifierWindow()
    {
        if (MagnifierSize == 0) return;
        if (MagnifierLocation.X - MagnifierSize / 2.0 < 0 ||
            MagnifierLocation.X + MagnifierSize / 2.0 > Picture!.PixelWidth) return;
        if (MagnifierLocation.Y - MagnifierSize / 2.0 < 0 ||
            MagnifierLocation.Y + MagnifierSize / 2.0 > Picture!.PixelHeight) return;

        var magnifierPixels = Tools.GetPixels(
            Picture!,
            (int) (MagnifierLocation.X - MagnifierSize / 2.0),
            (int) (MagnifierLocation.Y - MagnifierSize / 2.0),
            MagnifierSize,
            MagnifierSize);
        MagnifierWindow = Tools.CreateImage(
            Picture!,
            magnifierPixels,
            MagnifierSize,
            MagnifierSize);
    }

    #endregion

    #region MagnifierCommand

    public Command? MagnifierCommand { get; }

    private bool MagnifierCommand_CanExecute(object? parameter) => Picture is not null;

    private void MagnifierCommand_OnExecuted(object? parameter)
    {
        MagnifierVisibility = MagnifierVisibility is Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion
}