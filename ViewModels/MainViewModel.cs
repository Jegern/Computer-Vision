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

    private FileService FileService { get; } = new();
    private DialogService DialogService { get; } = new();

    private BitmapSource? _picture;
    private BitmapSource? _originalPicture;
    private Point _pictureMousePosition;
    private string? _title;

    public BitmapSource? Picture
    {
        get => _picture;
        set => Set(ref _picture, value);
    }

    private BitmapSource? OriginalPicture
    {
        get => _originalPicture;
        set => Set(ref _originalPicture, value);
    }

    private new byte[]? PictureBytes
    {
        set => Store?.TriggerPictureBytesEvent(value!);
    }

    private Point PictureMousePosition
    {
        get => _pictureMousePosition;
        set
        {
            if (Set(ref _pictureMousePosition, value))
                Store?.TriggerMousePositionEvent(PictureMousePosition);
        }
    }

    public string? Title
    {
        get => _title;
        set => Set(ref _title, value);
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public MainViewModel()
    {
    }

    public MainViewModel(ViewModelStore? store) : base(store)
    {
        if (store is not null) store.PictureChanged += source => Picture = source;
        
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

    #region Commands

    #region OpenImageCommand

    public Command? OpenImageCommand { get; }

    private bool OpenImageCommand_CanExecute(object? parameter) => true;

    private void OpenImageCommand_OnExecuted(object? parameter)
    {
        if (!DialogService.OpenFileDialog()) return;
        if (FileService.Open(DialogService.FilePath!))
        {
            Picture = FileService.OpenedImage;
            PictureSize = new Size(Picture!.Width, Picture!.Height);
            OriginalPicture = Picture!.Clone();
            PictureBytes = GetPictureBytes();
            Title = FileService.ImageName;
            ResizeAndCenterWindow();
        }
        else
            DialogService.ShowError("Приложение не поддерживает картинки больше 1600x900");
    }

    private byte[] GetPictureBytes()
    {
        var width = (int) PictureSize.Width;
        var height = (int) PictureSize.Height;
        var pixels = new byte[height * width * 4];
        Picture!.CopyPixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
        return pixels;
    }

    private static void ResizeAndCenterWindow()
    {
        var window = Application.Current.MainWindow;
        if (window is null) return;
        window.SizeToContent = SizeToContent.WidthAndHeight;
        window.Top = (SystemParameters.WorkArea.Height - window.Height) / 2;
        window.Left = (SystemParameters.WorkArea.Width - window.Width) / 2;
    }

    #endregion

    #region SaveImageCommand

    public Command? SaveImageCommand { get; }

    private bool SaveImageCommand_CanExecute(object? parameter) => Picture is not null;

    private void SaveImageCommand_OnExecuted(object? parameter)
    {
        if (!DialogService.SaveFileDialog()) return;
        FileService.Save(DialogService.FilePath!, Picture!);
    }

    #endregion

    #region ReturnOriginalImageCommand

    public Command? ReturnOriginalImageCommand { get; }

    private bool ReturnOriginalImageCommand_CanExecute(object? parameter) => OriginalPicture is not null;

    private void ReturnOriginalImageCommand_OnExecuted(object? parameter)
    {
        Picture = OriginalPicture;
        PictureBytes = GetPictureBytes();
    }

    #endregion

    #region MouseMoveCommand

    public Command? MouseMoveCommand { get; }

    private bool MouseMoveCommand_CanExecute(object? parameter) =>
        !PictureSize.IsEmpty &&
        ((MouseEventArgs) parameter!).Source is Image;

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