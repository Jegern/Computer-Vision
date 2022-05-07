using System;
using System.Collections.Generic;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class AntiAliasingViewModel : ViewModel
{
    #region Fields

    private byte[]? _antiAliasingPictureBytes;
    private bool _rectangleFilter3X3;
    private bool _rectangleFilter5X5;
    private bool _medianFilter3X3;
    private bool _medianFilter5X5;
    private double? _gaussianFilterSigma;
    private bool _sigmaFilter3X3;
    private bool _sigmaFilter5X5;
    private double? _sigmaFilterSigma;

    private byte[]? AntiAliasingPictureBytes
    {
        get => _antiAliasingPictureBytes;
        set
        {
            if (Set(ref _antiAliasingPictureBytes, value))
                Store?.TriggerAntiAliasingPictureBytesEvent(AntiAliasingPictureBytes!);
        }
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

    public AntiAliasingViewModel(ViewModelStore? store) : base(store)
    {
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

    #region Commands

    #region RectangleFilterCommand

    public Command? RectangleFilterCommand { get; }

    private bool RectangleFilterCommand_CanExecute(object? parameter) =>
        !PictureSize.IsEmpty &&
        (RectangleFilter3X3 || RectangleFilter5X5);

    private void RectangleFilterCommand_OnExecuted(object? parameter)
    {
        AntiAliasingPictureBytes = (byte[]) PictureBytes!.Clone();
        var width = (int) PictureSize.Width;
        var height = (int) PictureSize.Height;
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
                    if (x + j < 0 | x + j >= width) continue;
                    var windowPixelIndex = (y + i) * width * 4 + (x + j) * 4;
                    var windowPixelIntensity = Tools.GetPixelIntensity(AntiAliasingPictureBytes, windowPixelIndex);
                    sum += windowPixelIntensity;
                    counter++;
                }
            }

            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, y * width * 4 + x * 4),
                Tools.GetGrayPixel((byte) (sum / counter)));
        }

        Store?.TriggerPictureBytesEvent(PictureBytes!, PictureSize);
    }

    #endregion

    #region MedianFilterCommand

    public Command? MedianFilterCommand { get; }

    private bool MedianFilterCommand_CanExecute(object? parameter) =>
        !PictureSize.IsEmpty &&
        (MedianFilter3X3 || MedianFilter5X5);

    private void MedianFilterCommand_OnExecuted(object? parameter)
    {
        AntiAliasingPictureBytes = (byte[]) PictureBytes!.Clone();
        var width = (int) PictureSize.Width;
        var height = (int) PictureSize.Height;
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
                    if (x + j < 0 | x + j >= width) continue;
                    var windowPixelIndex = (y + i) * width * 4 + (x + j) * 4;
                    windowPixelList.Add(Tools.GetPixelIntensity(AntiAliasingPictureBytes, windowPixelIndex));
                }
            }

            windowPixelList.Sort();

            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, y * width * 4 + x * 4),
                Tools.GetGrayPixel((byte) windowPixelList[windowPixelList.Count / 2]));
        }

        Store?.TriggerPictureBytesEvent(PictureBytes!, PictureSize);
    }

    #endregion

    #region GaussianFilterCommand

    public Command? GaussianFilterCommand { get; }

    private bool GaussianFilterCommand_CanExecute(object? parameter) =>
        !PictureSize.IsEmpty &&
        GaussianFilterSigma is not null;

    private void GaussianFilterCommand_OnExecuted(object? parameter)
    {
        AntiAliasingPictureBytes = (byte[]) PictureBytes!.Clone();
        var width = (int) PictureSize.Width;
        var height = (int) PictureSize.Height;
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
                    if (x + j < 0 | x + j >= width) continue;
                    var windowPixelIndex = (y + i) * width * 4 + (x + j) * 4;
                    var windowPixelIntensity = Tools.GetPixelIntensity(AntiAliasingPictureBytes, windowPixelIndex);
                    var w = 1 / (Math.Exp((i * i + j * j) / (2 * sigma * sigma)) * (2 * Math.PI * sigma * sigma));
                    sum += w * windowPixelIntensity;
                }
            }

            var intensity = Tools.GetPixelIntensity(AntiAliasingPictureBytes, y * width * 4 + x * 4);
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, y * width * 4 + x * 4),
                Tools.GetGrayPixel((byte) (sum <= 255 ? sum : intensity)));
        }

        Store?.TriggerPictureBytesEvent(PictureBytes!, PictureSize);
    }

    #endregion

    #region SigmaFilterCommand

    public Command? SigmaFilterCommand { get; }

    private bool SigmaFilterCommand_CanExecute(object? parameter) => 
        !PictureSize.IsEmpty &&
        (SigmaFilter3X3 || SigmaFilter5X5);

    private void SigmaFilterCommand_OnExecuted(object? parameter)
    {
        AntiAliasingPictureBytes = (byte[]) PictureBytes!.Clone();
        var width = (int) PictureSize.Width;
        var height = (int) PictureSize.Height;
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
                    if (x + j < 0 | x + j >= width) continue;
                    var windowPixelIndex = (i + y) * width * 4 + (j + x) * 4;
                    var windowPixelIntensity = Tools.GetPixelIntensity(AntiAliasingPictureBytes, windowPixelIndex);
                    var midPixelIndex = y * width * 4 + x * 4;
                    var midPixelIntensity = Tools.GetPixelIntensity(AntiAliasingPictureBytes, midPixelIndex);
                    if (Math.Abs(midPixelIntensity - windowPixelIntensity) >= sigma) continue;
                    sum += windowPixelIntensity;
                    counter++;
                }
            }

            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, y * width * 4 + x * 4),
                Tools.GetGrayPixel((byte) (sum / counter)));
        }

        Store?.TriggerPictureBytesEvent(PictureBytes!, PictureSize);
    }

    #endregion

    #endregion
}