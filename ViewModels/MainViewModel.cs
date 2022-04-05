using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Services;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;

namespace Laboratory_work_1.ViewModels;

public class MainViewModel : ViewModel
{
    private readonly FileService _fileService = new();
    private readonly DialogService _dialogService = new();


    #region Fields

    private BitmapImage? _picture;
    private Visibility _pixelInfoVisibility = Visibility.Collapsed;
    private Visibility _magnifierVisibility = Visibility.Collapsed;

    public BitmapImage? Picture
    {
        get => _picture;
        set => Set(ref _picture, value);
    }
    
    public Visibility PixelInfoVisibility
    {
        get => _pixelInfoVisibility;
        set => Set(ref _pixelInfoVisibility, value);
    }
    
    public Visibility MagnifierVisibility
    {
        get => _magnifierVisibility;
        set => Set(ref _magnifierVisibility, value);
    }
    
    #endregion

    #region Commands

    #region OpenImageCommand

    public Command OpenImageCommand { get; }

    private bool OpenImageCommand_CanExecute(object parameter) => true;

    private void OpenImageCommand_OnExecuted(object parameter)
    {
        if (!_dialogService.OpenFileDialog()) return;
        if (_fileService.Open(_dialogService.FilePath!))
        {
            Picture = _fileService.OpenedImage;
            Tools.ResizeAndCenterWindow(Application.Current.MainWindow);
        }
        else
            _dialogService.ShowError("Разрешение картинки не может быть больше 1600x900");
    }

    #endregion

    #region SaveImageCommand

    public Command SaveImageCommand { get; }

    private bool SaveImageCommand_CanExecute(object parameter) => Picture is not null;

    private void SaveImageCommand_OnExecuted(object parameter)
    {
        if (!_dialogService.SaveFileDialog()) return;
        _fileService.Save(_dialogService.FilePath!, Picture!);
    }

    #endregion

    #region PixelInfoCommand

    public Command PixelInfoCommand { get; }

    private bool PixelInfoCommand_CanExecute(object parameter) => Picture is not null;

    private void PixelInfoCommand_OnExecuted(object parameter)
    {
        PixelInfoVisibility = PixelInfoVisibility is Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion

    #region MagnifierCommand

    public Command MagnifierCommand { get; }

    private bool MagnifierCommand_CanExecute(object parameter) => Picture is not null;

    private void MagnifierCommand_OnExecuted(object parameter)
    {
        MagnifierVisibility = MagnifierVisibility is Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion

    #region MagnifierInfoCommand

    public Command MagnifierInfoCommand { get; }

    private bool MagnifierInfoCommand_CanExecute(object parameter) => Picture is not null;

    private void MagnifierInfoCommand_OnExecuted(object parameter)
    {
        
    }

    #endregion

    #region ImageManagementCommand

    public Command ImageManagementCommand { get; }

    private bool ImageManagementCommand_CanExecute(object parameter) => Picture is not null;

    private void ImageManagementCommand_OnExecuted(object parameter)
    {
        
    }

    #endregion

    #endregion

    public MainViewModel()
    {
        OpenImageCommand = new Command(OpenImageCommand_OnExecuted, OpenImageCommand_CanExecute);
        SaveImageCommand = new Command(SaveImageCommand_OnExecuted, SaveImageCommand_CanExecute);
        PixelInfoCommand = new Command(PixelInfoCommand_OnExecuted, PixelInfoCommand_CanExecute);
        MagnifierCommand = new Command(MagnifierCommand_OnExecuted, MagnifierCommand_CanExecute);
        MagnifierInfoCommand = new Command(MagnifierInfoCommand_OnExecuted, MagnifierInfoCommand_CanExecute);
        ImageManagementCommand = new Command(ImageManagementCommand_OnExecuted, ImageManagementCommand_CanExecute);
    }
}