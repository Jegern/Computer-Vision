using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class BorderDetectionViewModel : ViewModel
{
    #region Fields

    private readonly ViewModelStore? _store;
    private BitmapSource? _picture;
    private byte[]? _pictureBytes;
    private Visibility _visibility = Visibility.Collapsed;

    private int? _hessianThreshold;
    private int? _harrisThreshold;
    private bool _sobelOperator3X3;
    private bool _sobelOperator5X5;
    private bool _sobelOperator7X7;
    private int? _sobelThreshold;

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

    public Visibility Visibility
    {
        get => _visibility;
        set => Set(ref _visibility, value);
    }

    public int? HessianThreshold
    {
        get => _hessianThreshold;
        set => Set(ref _hessianThreshold, value);
    }

    public int? HarrisThreshold
    {
        get => _harrisThreshold;
        set => Set(ref _harrisThreshold, value);
    }

    public bool SobelOperator3X3
    {
        get => _sobelOperator3X3;
        set => Set(ref _sobelOperator3X3, value);
    }

    public bool SobelOperator5X5
    {
        get => _sobelOperator5X5;
        set => Set(ref _sobelOperator5X5, value);
    }

    public bool SobelOperator7X7
    {
        get => _sobelOperator7X7;
        set => Set(ref _sobelOperator7X7, value);
    }

    public int? SobelThreshold
    {
        get => _sobelThreshold;
        set => Set(ref _sobelThreshold, value);
    }

    private int[,] SobelMask3X3 { get; } =
    {
        { -1, -2, -1 },
        { 0, 0, 0 },
        { 1, 2, 1 }
    };

    private int[,] SobelMask5X5 { get; } =
    {
        { 2, 3, 4, 3, 2 },
        { 1, 2, 3, 2, 1 },
        { 0, 0, 0, 0, 0 },
        { -1, -2, -3, -2, -1 },
        { -2, -3, -4, -3, -2 }
    };

    private int[,] SobelMask7X7 { get; } =
    {
        { -3, -4, -5, -6, -5, -4, -3 },
        { -2, -3, -4, -5, -4, -3, -2 },
        { -1, -2, -3, -4, -3, -2, -1 },
        { 0, 0, 0, 0, 0, 0, 0 },
        { 1, 2, 3, 4, 3, 2, 1 },
        { 2, 3, 4, 5, 4, 3, 2 },
        { 3, 4, 5, 6, 5, 4, 3 }
    };

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public BorderDetectionViewModel()
    {
    }

    public BorderDetectionViewModel(ViewModelStore? store)
    {
        if (store is null) return;

        store.PictureChanged += Picture_OnChanged;
        store.PictureBytesChanged += PictureBytes_OnChanged;
        _store = store;

        BorderDetectionCommand = new Command(
            BorderDetectionCommand_OnExecuted,
            BorderDetectionCommand_CanExecute);
        HessianOperatorCommand = new Command(
            HessianOperatorCommand_OnExecuted,
            HessianOperatorCommand_CanExecute);
        HarrisOperatorCommand = new Command(
            HarrisOperatorCommand_OnExecuted,
            HarrisOperatorCommand_CanExecute);
        SobelOperatorCommand = new Command(
            SobelOperatorCommand_OnExecuted,
            SobelOperatorCommand_CanExecute);
        LogOperatorCommand = new Command(
            LogOperatorCommand_OnExecuted,
            LogOperatorCommand_CanExecute);
        DogOperatorCommand = new Command(
            DogOperatorCommand_OnExecuted,
            DogOperatorCommand_CanExecute);
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

    #region BorderDetectionCommand

    public Command? BorderDetectionCommand { get; }

    private bool BorderDetectionCommand_CanExecute(object? parameter) => Picture is not null;

    private void BorderDetectionCommand_OnExecuted(object? parameter)
    {
        Visibility = Visibility is Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    #endregion

    #region HessianOperatorCommand

    public Command? HessianOperatorCommand { get; }

    private bool HessianOperatorCommand_CanExecute(object? parameter) => 
        Picture is not null &&
        HessianThreshold is not null;

    private void HessianOperatorCommand_OnExecuted(object? parameter)
    {
        var originalPictureBytes = (byte[])PictureBytes!.Clone();
        var width = Picture!.PixelWidth;
        var height = Picture!.PixelHeight;

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            if (y - 1 < 0 | y + 1 >= height) continue;
            if (x - 1 < 0 | x + 1 >= width) continue;
            var index = y * width * 4 + x * 4;
            var ixx = Tools.GetPixelIntensity(originalPictureBytes, index + 4) +
                      Tools.GetPixelIntensity(originalPictureBytes, index - 4)
                      - 2 * Tools.GetPixelIntensity(originalPictureBytes, index);
            var ixy = Tools.GetPixelIntensity(originalPictureBytes, index + 4) +
                      Tools.GetPixelIntensity(originalPictureBytes, index + 4 * width)
                      - 2 * Tools.GetPixelIntensity(originalPictureBytes, index);
            var iyy = Tools.GetPixelIntensity(originalPictureBytes, index - 4 * width) +
                      Tools.GetPixelIntensity(originalPictureBytes, index + 4 * width)
                      - 2 * Tools.GetPixelIntensity(originalPictureBytes, index);
            var lambdas = SolveQuadraticEquation(new[,] { { ixx, ixy }, { ixy, iyy } });
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, index),
                Tools.GetGrayPixel((byte)(Math.Abs(lambdas[0]) > HessianThreshold &&
                                          Math.Abs(lambdas[1]) > HessianThreshold ? 0 : 255)));
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    private static double[] SolveQuadraticEquation(double[,] matrix)
    {
        const double a = 1.0;
        var b = -(matrix[0, 0] + matrix[1, 1]);
        var c = -matrix[0, 1] * matrix[1, 0] + matrix[0, 0] * matrix[1, 1];
        var d = b * b - 4 * a * c;
        var x1 = (-b - Math.Sqrt(d)) / (2 * a);
        var x2 = (-b + Math.Sqrt(d)) / (2 * a);
        return new[] { x1, x2 };
    }

    #endregion

    #region HarrisOperatorCommand

    public Command? HarrisOperatorCommand { get; }

    private bool HarrisOperatorCommand_CanExecute(object? parameter) => 
        Picture is not null &&
        HarrisThreshold is not null;

    private void HarrisOperatorCommand_OnExecuted(object? parameter)
    {
        var originalPictureBytes = (byte[])PictureBytes!.Clone();
        var width = Picture!.PixelWidth;
        var height = Picture!.PixelHeight;

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            if (y - 1 < 0 | y + 1 >= height) continue;
            if (x - 1 < 0 | x + 1 >= width) continue;
            var index = y * width * 4 + x * 4;
            var ix = Tools.GetPixelIntensity(originalPictureBytes, index + 4) -
                     Tools.GetPixelIntensity(originalPictureBytes, index);
            var iy = Tools.GetPixelIntensity(originalPictureBytes, index + 4 * width) -
                     Tools.GetPixelIntensity(originalPictureBytes, index);
            var l2X = ix * ix;
            var lxy = ix * iy;
            var l2Y = iy * iy;
            var lambdas = SolveQuadraticEquation(new[,] { { l2X, lxy }, { lxy, l2Y } });
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, index),
                Tools.GetGrayPixel((byte)(0.05 * lambdas[1] > HarrisThreshold ? 0 : 255)));
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region SobelOperatorCommand

    public Command? SobelOperatorCommand { get; }

    private bool SobelOperatorCommand_CanExecute(object? parameter) =>
        Picture is not null &&
        SobelThreshold is not null &&
        (SobelOperator3X3 || SobelOperator5X5 || SobelOperator7X7);

    private void SobelOperatorCommand_OnExecuted(object? parameter)
    {
        var originalPictureBytes = (byte[])PictureBytes!.Clone();
        var width = Picture!.PixelWidth;
        var height = Picture!.PixelHeight;

        var mask = SobelOperator3X3 switch
        {
            true => SobelMask3X3,
            false when SobelOperator5X5 => SobelMask5X5,
            false => SobelMask7X7
        };
        var radius = SobelOperator3X3 switch
        {
            true => 1,
            false when SobelOperator5X5 => 2,
            false => 3
        };

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var gx = 0d;
            var gy = 0d;
            for (var i = -radius; i <= radius; i++)
            {
                if (y + i < 0 | y + i >= height) continue;
                for (var j = -radius; j <= radius; j++)
                {
                    if (x + j < 0 | x + j >= width) continue;
                    var windowPixelIndex = (i + y) * width * 4 + (j + x) * 4;
                    gx += Tools.GetPixelIntensity(originalPictureBytes, windowPixelIndex) *
                          mask[i + radius, j + radius];
                    gy += Tools.GetPixelIntensity(originalPictureBytes, windowPixelIndex) *
                          mask[j + radius, i + radius];
                }
            }

            var f = Math.Sqrt(gx * gx + gy * gy);
            var threshold = SobelThreshold;
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, y * width * 4 + x * 4),
                Tools.GetGrayPixel((byte)(f > threshold ? 0 : 255)));
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region LogOperatorCommand

    public Command? LogOperatorCommand { get; }

    private bool LogOperatorCommand_CanExecute(object? parameter) => Picture is not null;

    private void LogOperatorCommand_OnExecuted(object? parameter)
    {
        var originalPictureBytes = (byte[])PictureBytes!.Clone();
        var width = Picture!.PixelWidth;
        var height = Picture!.PixelHeight;
        var labmdaSquare = Math.Pow(0.5 / Math.Sqrt(2), 2);

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            if (y - 1 < 0 | y + 1 >= height) continue;
            if (x - 1 < 0 | x + 1 >= width) continue;
            var index = y * width * 4 + x * 4;
            var ixx = Tools.GetPixelIntensity(originalPictureBytes, index + 4) +
                      Tools.GetPixelIntensity(originalPictureBytes, index - 4)
                      - 2 * Tools.GetPixelIntensity(originalPictureBytes, index);
            var iyy = Tools.GetPixelIntensity(originalPictureBytes, index - 4 * width) +
                      Tools.GetPixelIntensity(originalPictureBytes, index + 4 * width)
                      - 2 * Tools.GetPixelIntensity(originalPictureBytes, index);
            var g = 1 / (2 * Math.PI * Math.Pow(labmdaSquare, 2)) *
                    ((ixx * ixx + iyy * iyy - 2 * labmdaSquare) / labmdaSquare) *
                    Math.Pow(Math.E, -(ixx * ixx + iyy * iyy) / (2 * labmdaSquare));

            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, index),
                Tools.GetGrayPixel((byte)(g == 0 ? 0 : 255)));
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region DogOperatorCommand

    public Command? DogOperatorCommand { get; }

    private bool DogOperatorCommand_CanExecute(object? parameter) => Picture is not null;

    private void DogOperatorCommand_OnExecuted(object? parameter)
    {
        var width = Picture!.PixelWidth;
        var height = Picture!.PixelHeight;
        var vanishBytes = GaussianFilter((byte[])PictureBytes!.Clone(), 0.5);
        var vanishBytesAlpha = GaussianFilter((byte[])PictureBytes!.Clone(), 0.5 * 1.6);

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            if (y - 1 < 0 | y + 1 >= height) continue;
            if (x - 1 < 0 | x + 1 >= width) continue;
            var index = y * width * 4 + x * 4;
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes!, index),
                Tools.GetGrayPixel((byte)(vanishBytes[index] == vanishBytesAlpha[index] ? 0 : 255)));
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    private byte[] GaussianFilter(byte[] bytes, double sigma)
    {
        var originalPictureBytes = (byte[])bytes.Clone();
        var width = Picture!.PixelWidth;
        var height = Picture!.PixelHeight;
        var k = (int)Math.Ceiling((6 * sigma - 2) / 2);
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
                    if (x + j < 0 | x + j >= width) continue;
                    var windowPixelIndex = (y + i) * width * 4 + (x + j) * 4;
                    var windowPixelIntensity = Tools.GetPixelIntensity(originalPictureBytes, windowPixelIndex);
                    var w = 1 / (Math.Exp((i * i + j * j) / (2 * sigma * sigma)) * (2 * Math.PI * sigma * sigma));
                    sum += w * windowPixelIntensity;
                }
            }

            var intensity = Tools.GetPixelIntensity(originalPictureBytes, y * width * 4 + x * 4);
            Tools.SetPixel(
                Tools.GetPixel(bytes, y * width * 4 + x * 4),
                Tools.GetGrayPixel((byte)(sum <= 255 ? sum : intensity)));
        }

        return bytes;
    }

    #endregion

    #endregion
}