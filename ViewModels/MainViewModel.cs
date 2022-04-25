using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    private Point? _pictureMousePosition;

    public BitmapImage? Picture
    {
        get => _picture;
        set => Set(ref _picture, value);
    }

    public Point? PictureMousePosition
    {
        get => _pictureMousePosition;
        set
        {
            Set(ref _pictureMousePosition, value);
            UpdatePixelLocation();
            UpdateMagnifierLocation();
        }
    }

    #region PixelInfo

    private Visibility _pixelInfoVisibility = Visibility.Collapsed;
    private Point? _pixelLocation;
    private byte? _pixelRed;
    private byte? _pixelGreen;
    private byte? _pixelBlue;
    private byte? _pixelIntensivity;

    public Visibility PixelInfoVisibility
    {
        get => _pixelInfoVisibility;
        set => Set(ref _pixelInfoVisibility, value);
    }

    private void UpdatePixelLocation()
    {
        PixelLocation = PictureMousePosition;
    }

    public Point? PixelLocation
    {
        get => _pixelLocation;
        set
        {
            Set(ref _pixelLocation, value);
            UpdatePixelRgb();
        }
    }

    private void UpdatePixelRgb()
    {
        var pixelColor = Tools.GetPixelColor(Picture!, (Point) PictureMousePosition!);
        PixelRed = pixelColor.R;
        PixelGreen = pixelColor.G;
        PixelBlue = pixelColor.B;
        PixelIntensivity = (byte) ((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
    }

    public byte? PixelRed
    {
        get => _pixelRed;
        set => Set(ref _pixelRed, value);
    }

    public byte? PixelGreen
    {
        get => _pixelGreen;
        set => Set(ref _pixelGreen, value);
    }

    public byte? PixelBlue
    {
        get => _pixelBlue;
        set => Set(ref _pixelBlue, value);
    }

    public byte? PixelIntensivity
    {
        get => _pixelIntensivity;
        set => Set(ref _pixelIntensivity, value);
    }

    #endregion

    #region Magnifier

    private Visibility _magnifierVisibility = Visibility.Collapsed;
    private Point? _magnifierLocation;
    private BitmapSource? _magnifierWindow;
    private int _magnifierSize = 11;

    public Visibility MagnifierVisibility
    {
        get => _magnifierVisibility;
        set => Set(ref _magnifierVisibility, value);
    }

    private void UpdateMagnifierLocation()
    {
        MagnifierLocation = PictureMousePosition;
    }

    public Point? MagnifierLocation
    {
        get => _magnifierLocation;
        set
        {
            Set(ref _magnifierLocation, value);
            UpdateMagnifierWindow();
        }
    }

    private void UpdateMagnifierWindow()
    {
        if (MagnifierSize == 0) return;
        if (MagnifierLocation!.Value.X - MagnifierSize / 2.0 < 0 ||
            MagnifierLocation!.Value.X + MagnifierSize / 2.0 > Picture!.PixelWidth) return;
        if (MagnifierLocation!.Value.Y - MagnifierSize / 2.0 < 0 ||
            MagnifierLocation!.Value.Y + MagnifierSize / 2.0 > Picture!.PixelHeight) return;

        var magnifierPixels = Tools.GetPixels(
            Picture!,
            (int) (MagnifierLocation!.Value.X - MagnifierSize / 2.0),
            (int) (MagnifierLocation!.Value.Y - MagnifierSize / 2.0),
            MagnifierSize,
            MagnifierSize);
        MagnifierWindow = Tools.CreateImage(
            Picture!,
            magnifierPixels,
            MagnifierSize,
            MagnifierSize);
    }

    public BitmapSource? MagnifierWindow
    {
        get => _magnifierWindow;
        set => Set(ref _magnifierWindow, value);
    }

    public int MagnifierSize
    {
        get => _magnifierSize;
        set => Set(ref _magnifierSize, value);
    }

    #endregion

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
            _dialogService.ShowError("Приложение не поддерживает картинки больше 1600x900");
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

    #region MouseMoveCommand

    public Command MouseMoveCommand { get; }

    private bool MouseMoveCommand_CanExecute(object parameter) =>
        Picture is not null &&
        ((MouseEventArgs) parameter).Source is Image;

    private void MouseMoveCommand_OnExecuted(object parameter)
    {
        var e = (MouseEventArgs) parameter;
        var position = e.GetPosition((Image) e.Source);
        position.X = Math.Min(position.X, Picture!.PixelWidth - 1);
        position.Y = Math.Min(position.Y, Picture!.PixelHeight - 1);
        PictureMousePosition = position;
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
        MouseMoveCommand = new Command(MouseMoveCommand_OnExecuted, MouseMoveCommand_CanExecute);
        PixelInfoCommand = new Command(PixelInfoCommand_OnExecuted, PixelInfoCommand_CanExecute);
        MagnifierCommand = new Command(MagnifierCommand_OnExecuted, MagnifierCommand_CanExecute);
        MagnifierInfoCommand = new Command(MagnifierInfoCommand_OnExecuted, MagnifierInfoCommand_CanExecute);
        ImageManagementCommand = new Command(ImageManagementCommand_OnExecuted, ImageManagementCommand_CanExecute);
    }
}