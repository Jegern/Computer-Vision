using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class AntiAliasingViewModel : ViewModel
{
    #region Fields

    private readonly ViewModelStore? _store;
    private BitmapSource? _picture;
    private byte[]? _pictureBytes;
    private Visibility _antiAliasingVisibility = Visibility.Collapsed;

    private bool _rectangleFilter3X3;
    private bool _rectangleFilter5X5;
    private bool _medianFilter3X3;
    private bool _medianFilter5X5;
    private double? _gaussianFilterSigma;
    private bool _sigmaFilter3X3;
    private bool _sigmaFilter5X5;
    private double? _sigmaFilterSigma;

    private BitmapSource? Picture
    {
        get => _picture;
        set => Set(ref _picture, value);
    }

    private byte[]? PictureBytes
    {
        get => _pictureBytes;
        set => Set(ref _pictureBytes, value);
    }

    public Visibility AntiAliasingVisibility
    {
        get => _antiAliasingVisibility;
        set => Set(ref _antiAliasingVisibility, value);
    }

    public bool RectangleFilter3X3
    {
        get => _rectangleFilter3X3;
        set => Set(ref _rectangleFilter3X3, value);
    }

    public bool RectangleFilter5X5
    {
        get => _rectangleFilter5X5;
        set => Set(ref _rectangleFilter5X5, value);
    }

    public bool MedianFilter3X3
    {
        get => _medianFilter3X3;
        set => Set(ref _medianFilter3X3, value);
    }

    public bool MedianFilter5X5
    {
        get => _medianFilter5X5;
        set => Set(ref _medianFilter5X5, value);
    }

    public double? GaussianFilterSigma
    {
        get => _gaussianFilterSigma;
        set => Set(ref _gaussianFilterSigma, value);
    }
    
    public bool SigmaFilter3X3
    {
        get => _sigmaFilter3X3;
        set => Set(ref _sigmaFilter3X3, value);
    }

    public bool SigmaFilter5X5
    {
        get => _sigmaFilter5X5;
        set => Set(ref _sigmaFilter5X5, value);
    }

    public double? SigmaFilterSigma
    {
        get => _sigmaFilterSigma;
        set => Set(ref _sigmaFilterSigma, value);
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public AntiAliasingViewModel()
    {
    }

    public AntiAliasingViewModel(ViewModelStore? store)
    {
        if (store is null) return;

        store.PictureChanged += Picture_OnChanged;
        store.PictureBytesChanged += PictureBytes_OnChanged;
        _store = store;

        AntiAliasingCommand = new Command(
            AntiAliasingCommand_OnExecuted,
            AntiAliasingCommand_CanExecute);
        RectangleFilterCommand = new Command(
            RectangleFilterCommand_OnExecuted,
            RectangleFilterCommand_CanExecute);
        MedianFilterCommand = new Command(
            MedianFilterCommand_OnExecuted,
            MedianFilterCommand_CanExecute);
        GaussianFilterCommand = new Command(
            GaussianFilterCommand_OnExecuted,
            GaussianFilterCommand_CanExecute);
        SigmaFilterCommand = new Command(
            SigmaFilterCommand_OnExecuted,
            SigmaFilterCommand_CanExecute);
    }

    #region Event Subscription

    private void Picture_OnChanged(BitmapSource? source)
    {
        Picture = source;
    }

    private void PictureBytes_OnChanged(byte[] bytes)
    {
        PictureBytes = bytes;
    }

    #endregion

    #region Commands

    #region AntiAliasingCommand

    public Command? AntiAliasingCommand { get; }

    private bool AntiAliasingCommand_CanExecute(object? parameter) => Picture is not null;

    private void AntiAliasingCommand_OnExecuted(object? parameter)
    {
        AntiAliasingVisibility = AntiAliasingVisibility is Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    #endregion

    #region RectangleFilterCommand

    public Command? RectangleFilterCommand { get; }

    private bool RectangleFilterCommand_CanExecute(object? parameter) =>
        Picture is not null;

    private void RectangleFilterCommand_OnExecuted(object? parameter)
    {
        var originalPictureBytes = (byte[]) PictureBytes!.Clone();
        var width = Picture!.PixelWidth;
        var height = Picture!.PixelHeight;
        var side = RectangleFilter3X3 ? 3 : 5;
        var radius = side / 2;

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var sum = 0d;
            var counter = 0;
            for (var i = -radius; i <= radius; i++)
            {
                if (y + i < 0 | y + i >= height) continue;
                for (var j = -radius; j <= radius; j++)
                {
                    if (j + x < 0 | j + x >= width) continue;
                    var windowPixelIndex = (y + i) * width * 4 + (x + j) * 4;
                    var windowPixelIntensity = Tools.GetPixelIntensity(originalPictureBytes, windowPixelIndex);
                    sum += windowPixelIntensity;
                    counter++;
                }
            }

            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, y * width * 4 + x * 4),
                Tools.GetGrayPixel((byte) (sum / counter)));
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region MedianFilterCommand

    public Command? MedianFilterCommand { get; }

    private bool MedianFilterCommand_CanExecute(object? parameter) =>
        Picture is not null;

    private void MedianFilterCommand_OnExecuted(object? parameter)
    {
        var originalPictureBytes = (byte[]) PictureBytes!.Clone();
        var width = Picture!.PixelWidth;
        var height = Picture!.PixelHeight;
        var side = MedianFilter3X3 ? 3 : 5;
        var radius = side / 2;
        var windowPixelList = new List<double>(side * side);

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            windowPixelList.Clear();
            for (var i = -radius; i <= radius; i++)
            {
                if (y + i < 0 | y + i >= height) continue;
                for (var j = -radius; j <= radius; j++)
                {
                    if (j + x < 0 | j + x >= width) continue;
                    var windowPixelIndex = (y + i) * width * 4 + (x + j) * 4;
                    windowPixelList.Add(Tools.GetPixelIntensity(originalPictureBytes, windowPixelIndex));
                }
            }

            windowPixelList.Sort();

            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, y * width * 4 + x * 4),
                Tools.GetGrayPixel((byte) windowPixelList[windowPixelList.Count / 2]));
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region GaussianFilterCommand

    public Command? GaussianFilterCommand { get; }

    private bool GaussianFilterCommand_CanExecute(object? parameter) =>
        Picture is not null &&
        GaussianFilterSigma is not null;

    private void GaussianFilterCommand_OnExecuted(object? parameter)
    {
        var originalPictureBytes = (byte[]) PictureBytes!.Clone();
        var width = Picture!.PixelWidth;
        var height = Picture!.PixelHeight;
        var sigma = (double) GaussianFilterSigma!;
        var k = (int) Math.Ceiling((6 * sigma - 2) / 2);
        var side = 2 * k + 1;
        var radius = side / 2;

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var sum = 0d;
            for (var i = -radius; i <= radius; i++)
            {
                if (y + i < 0 | y + i >= height) continue;
                for (var j = -radius; j <= radius; j++)
                {
                    if (j + x < 0 | j + x >= width) continue;
                    var windowPixelIndex = (y + i) * width * 4 + (x + j) * 4;
                    var windowPixelIntensity = Tools.GetPixelIntensity(originalPictureBytes, windowPixelIndex);
                    var w = 1 / (Math.Exp((i * i + j * j) / (2 * sigma * sigma)) * (2 * Math.PI * sigma * sigma));
                    sum += w * windowPixelIntensity;
                }
            }

            var intensity = Tools.GetPixelIntensity(originalPictureBytes, y * width * 4 + x * 4);
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, y * width * 4 + x * 4),
                Tools.GetGrayPixel((byte) (sum <= 255 ? sum : intensity)));
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region SigmaFilterCommand

    public Command? SigmaFilterCommand { get; }

    private bool SigmaFilterCommand_CanExecute(object? parameter) => Picture is not null;

    private void SigmaFilterCommand_OnExecuted(object? parameter)
    {
        var originalPictureBytes = (byte[]) PictureBytes!.Clone();
        var width = Picture!.PixelWidth;
        var height = Picture!.PixelHeight;
        var side = SigmaFilter3X3 ? 3 : 5;
        var radius = side / 2;
        var sigma = (double) SigmaFilterSigma!;

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var counter = 0;
            var sum = 0d;
            for (var i = -radius; i <= radius; i++)
            {
                if (y + i < 0 | y + i >= height) continue;
                for (var j = -radius; j <= radius; j++)
                {
                    if (j + x < 0 | j + x >= width) continue;
                    var windowPixelIndex = (i + y) * width * 4 + (j + x) * 4;
                    var windowPixelIntensity = Tools.GetPixelIntensity(originalPictureBytes, windowPixelIndex);
                    var midPixelIndex = y * width * 4 + x * 4;
                    var midPixelIntensity = Tools.GetPixelIntensity(originalPictureBytes, midPixelIndex);
                    if (Math.Abs(midPixelIntensity - windowPixelIntensity) >= sigma) continue;
                    sum += windowPixelIntensity;
                    counter++;
                }
            }

            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, y * width * 4 + x * 4),
                Tools.GetGrayPixel((byte) (sum / counter)));
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #endregion
}