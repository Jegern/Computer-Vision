using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Services;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class MainViewModel : ViewModel
{
    #region Fields

    private readonly FileService _fileService = new();
    private readonly DialogService _dialogService = new();
    private readonly ViewModelStore? _store;
    private BitmapSource? _picture;
    private BitmapSource? _originalPicture;
    private byte[]? _pictureBytes;
    private Point _pictureMousePosition;

    public BitmapSource? Picture
    {
        get => _picture;
        set
        {
            if (Set(ref _picture, value))
                _store?.TriggerPictureEvent(Picture!);
        }
    }

    private BitmapSource? OriginalPicture
    {
        get => _originalPicture;
        set => Set(ref _originalPicture, value);
    }

    private byte[]? PictureBytes
    {
        set
        {
            if (Set(ref _pictureBytes, value) && Picture is not null)
                _store?.TriggerPictureBytesEvent(Picture);
        }
    }

    private Point PictureMousePosition
    {
        get => _pictureMousePosition;
        set
        {
            if (Set(ref _pictureMousePosition, value))
                _store?.TriggerMousePositionEvent(PictureMousePosition);
        }
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public MainViewModel()
    {
    }

    public MainViewModel(ViewModelStore? store)
    {
        if (store is null) return;

        store.PictureChanged += Picture_OnChanged;
        _store = store;

        OpenImageCommand = new Command(
            OpenImageCommand_OnExecuted, 
            OpenImageCommand_CanExecute);
        SaveImageCommand = new Command(
            SaveImageCommand_OnExecuted, 
            SaveImageCommand_CanExecute);
        ReturnOriginalImageCommand = new Command(
            ReturnOriginalImageCommand_OnExecuted, 
            ReturnOriginalImageCommand_CanExecute);
        MouseMoveCommand = new Command(
            MouseMoveCommand_OnExecuted, 
            MouseMoveCommand_CanExecute);
    }

    #region Event Subscription

    private void Picture_OnChanged(BitmapSource source)
    {
        Picture = source;
    }

    #endregion

    #region Commands

    #region OpenImageCommand

    public Command? OpenImageCommand { get; }

    private bool OpenImageCommand_CanExecute(object? parameter) => true;

    private void OpenImageCommand_OnExecuted(object? parameter)
    {
        if (!_dialogService.OpenFileDialog()) return;
        if (_fileService.Open(_dialogService.FilePath!))
        {
            Picture = _fileService.OpenedImage!;
            OriginalPicture = Picture.Clone();
            PictureBytes = Tools.GetPixelBytes(Picture);
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
    
    #region ReturnOriginalImageCommand

    public Command? ReturnOriginalImageCommand { get; }

    private bool ReturnOriginalImageCommand_CanExecute(object? parameter) => OriginalPicture is not null;

    private void ReturnOriginalImageCommand_OnExecuted(object? parameter)
    {
        Picture = OriginalPicture;
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
        position.X = Math.Round(Math.Min(position.X, Picture!.PixelWidth - 1), 0);
        position.Y = Math.Round(Math.Min(position.Y, Picture!.PixelHeight - 1), 0);
        PictureMousePosition = position;
    }

    #endregion

    #endregion
}