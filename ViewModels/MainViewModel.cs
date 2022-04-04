using System.Windows.Media.Imaging;
using Laboratory_work_1.Services;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;

namespace Laboratory_work_1.ViewModels;

public class MainViewModel : ViewModel
{
    private readonly FileService _fileService = new();
    private readonly DialogService _dialogService = new();


    private BitmapImage? _picture;

    public BitmapImage? Picture
    {
        get => _picture;
        set => Set(ref _picture, value);
    }

    #region Commands

    #region OpenImageCommand

    public Command OpenImageCommand { get; }

    private bool OpenImageCommand_CanExecute(object parameter) => true;

    private void OpenImageCommand_OnExecuted(object parameter)
    {
        if (!_dialogService.OpenFileDialog()) return;
        if (_fileService.Open(_dialogService.FilePath!))
            Picture = _fileService.OpenedImage;
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

    #endregion

    public MainViewModel()
    {
        OpenImageCommand = new Command(OpenImageCommand_OnExecuted, OpenImageCommand_CanExecute);
        SaveImageCommand = new Command(SaveImageCommand_OnExecuted, SaveImageCommand_CanExecute);
    }
}