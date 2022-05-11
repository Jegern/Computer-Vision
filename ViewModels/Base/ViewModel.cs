using System.Windows;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels.Base;

public abstract class ViewModel : INotifyPropertyChanged
{
    #region PropertyChangedEvent

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion

    #region Fields

    private Size _pictureSize = Size.Empty;
    private byte[]? _pictureBytes;
    private Visibility _visibility = Visibility.Collapsed;

    public ViewModelStore? Store { get; }

    protected Size PictureSize
    {
        get => _pictureSize;
        set
        {
            if (Set(ref _pictureSize, value))
                Store?.TriggerPictureSizeEvent(_pictureSize);
        }
    }

    protected byte[]? PictureBytes
    {
        get => _pictureBytes;
        private set => Set(ref _pictureBytes, value);
    }

    public Visibility Visibility
    {
        get => _visibility;
        set => Set(ref _visibility, value);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    protected ViewModel()
    {
    }

    protected ViewModel(ViewModelStore store)
    {
        Store = store;
        Store.PictureSizeChanged += size => PictureSize = size;
        Store.PictureBytesChanged += bytes => PictureBytes = bytes;

        ChangeVisibilityCommand = new Command(
            ChangeVisibilityCommand_OnExecuted,
            ChangeVisibilityCommand_CanExecute);
    }

    #endregion

    #region ChangeVisibilityCommand

    public Command? ChangeVisibilityCommand { get; init; }

    private bool ChangeVisibilityCommand_CanExecute(object? parameter) => !PictureSize.IsEmpty;

    private void ChangeVisibilityCommand_OnExecuted(object? parameter)
    {
        Visibility = Visibility is Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    #endregion
}