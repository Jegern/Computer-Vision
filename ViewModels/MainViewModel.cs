using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Stores;
using Laboratory_work_1.Services;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;

namespace Laboratory_work_1.ViewModels;

public class MainViewModel : ViewModel
{
    #region Fields

    private readonly FileService _fileService = new();
    private readonly DialogService _dialogService = new();
    private readonly Store? _store;
    private BitmapImage? _picture;
    private Point _pictureMousePosition;

    public BitmapImage? Picture
    {
        get => _picture;
        set
        {
            Set(ref _picture, value);
            _store?.TriggerPictureEvent(_picture);
        } 
    }

    private Point PictureMousePosition
    {
        set
        {
            Set(ref _pictureMousePosition, value);
            _store?.TriggerMousePositionEvent(_pictureMousePosition);
        }
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public MainViewModel()
    {
        
    }
    
    public MainViewModel(Store? store)
    {
        if (store is null) return;
        
        _store = store;
        
        OpenImageCommand = new Command(OpenImageCommand_OnExecuted, OpenImageCommand_CanExecute);
        SaveImageCommand = new Command(SaveImageCommand_OnExecuted, SaveImageCommand_CanExecute);
        MouseMoveCommand = new Command(MouseMoveCommand_OnExecuted, MouseMoveCommand_CanExecute);
        ImageManagementCommand = new Command(ImageManagementCommand_OnExecuted, ImageManagementCommand_CanExecute);
    }

    #region Commands

    #region OpenImageCommand

    public Command? OpenImageCommand { get; }

    private bool OpenImageCommand_CanExecute(object? parameter) => true;

    private void OpenImageCommand_OnExecuted(object? parameter)
    {
        if (!_dialogService.OpenFileDialog()) return;
        if (_fileService.Open(_dialogService.FilePath!))
        {
            Picture = _fileService.OpenedImage;
            Tools.ResizeAndCenterWindow(Application.Current.MainWindow);
        }
        else
            _dialogService.ShowError("Приложение не поддерживает картинки больше 1600x900");
    }

    #endregion

    #region SaveImageCommand

    public Command? SaveImageCommand { get; }

    private bool SaveImageCommand_CanExecute(object? parameter) => Picture is not null;

    private void SaveImageCommand_OnExecuted(object? parameter)
    {
        if (!_dialogService.SaveFileDialog()) return;
        _fileService.Save(_dialogService.FilePath!, Picture!);
    }

    #endregion

    #region MouseMoveCommand

    public Command? MouseMoveCommand { get; }

    private bool MouseMoveCommand_CanExecute(object? parameter) =>
        Picture is not null && ((MouseEventArgs) parameter!).Source is Image;

    private void MouseMoveCommand_OnExecuted(object? parameter)
    {
        var e = (MouseEventArgs) parameter!;
        var position = e.GetPosition((Image) e.Source);
        position.X = Math.Min(position.X, Picture!.PixelWidth - 1);
        position.Y = Math.Min(position.Y, Picture!.PixelHeight - 1);
        PictureMousePosition = position;
    }

    #endregion

    #region ImageManagementCommand

    public Command? ImageManagementCommand { get; }

    private bool ImageManagementCommand_CanExecute(object? parameter) => Picture is not null;

    private void ImageManagementCommand_OnExecuted(object? parameter)
    {
    }

    #endregion

    #endregion
}