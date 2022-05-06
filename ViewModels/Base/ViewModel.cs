using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels.Base;

public abstract class ViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }


    private BitmapSource? _picture;
    private byte[]? _pictureBytes;
    private Visibility _visibility = Visibility.Collapsed;

    protected ViewModelStore? Store { get; init; }

    public BitmapSource? Picture
    {
        get => _picture;
        set
        {
            if (Set(ref _picture, value))
                Store?.TriggerPictureEvent(_picture!);
        }
    }

    protected byte[]? PictureBytes
    {
        get => _pictureBytes;
        set => Set(ref _pictureBytes, value);
    }
    
    public Visibility Visibility
    {
        get => _visibility;
        set => Set(ref _visibility, value);
    }
    
    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    protected ViewModel()
    {
    }

    protected ViewModel(ViewModelStore? store)
    {
        if (store is null) return;
        Store = store;
        Store.PictureChanged += Picture_OnChanged;
        Store.PictureBytesChanged += PictureBytes_OnChanged;

        ChangeVisibilityCommand = new Command(
            ChangeVisibilityCommand_OnExecuted,
            ChangeVisibilityCommand_CanExecute);
    }

    protected virtual void Picture_OnChanged(BitmapSource? source)
    {
        Picture = source;
    }

    protected virtual void PictureBytes_OnChanged(byte[] bytes)
    {
        PictureBytes = bytes;
    }
    
    #region ChangeVisibilityCommand

    public Command? ChangeVisibilityCommand { get; init; }

    private bool ChangeVisibilityCommand_CanExecute(object? parameter) => Picture is not null;

    private void ChangeVisibilityCommand_OnExecuted(object? parameter)
    {
        Visibility = Visibility is Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
        Tools.ResizeAndCenterWindow(Application.Current.MainWindow);
    }

    #endregion
}