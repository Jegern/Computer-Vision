using System.Collections.Generic;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class AdaptiveThresholdingViewModel : ViewModel
{
    #region Fields

    private int? _threshold;
    private int? _radius;

    public int? Threshold
    {
        get => _threshold;
        set => Set(ref _threshold, value);
    }
    
    public int? Radius
    {
        get => _radius;
        set => Set(ref _radius, value);
    }

    #endregion

    public AdaptiveThresholdingViewModel()
    {
    }

    public AdaptiveThresholdingViewModel(ViewModelStore? store) : base(store)
    {
        MeanThresholdingCommand = new Command(
            MeanThresholdingCommand_OnExecuted,
            MeanThresholdingCommand_CanExecute);
        MedianThresholdingCommand = new Command(
            MedianThresholdingCommand_OnExecuted,
            MedianThresholdingCommand_CanExecute);
        MinMaxThresholdingCommand = new Command(
            MinMaxThresholdingCommand_OnExecuted,
            MinMaxThresholdingCommand_CanExecute);
    }

    #region Commands

    #region MeanThresholdingCommand

    public Command? MeanThresholdingCommand { get; }

    private bool MeanThresholdingCommand_CanExecute(object? parameter) =>
        !PictureSize.IsEmpty &&
        Radius > 0 &&
        Threshold > 0;

    private void MeanThresholdingCommand_OnExecuted(object? parameter)
    {
        var originalPictureBytes = (byte[]) PictureBytes!.Clone();
        var width = (int) PictureSize.Width;
        var height = (int) PictureSize.Height;
        var radius = (int) Radius!;

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
                    var windowPixelIntensity = Tools.GetPixelIntensity(originalPictureBytes, windowPixelIndex);
                    sum += windowPixelIntensity;
                    counter++;
                }
            }

            var c = sum / counter;
            var index = y * width * 4 + x * 4;
            var intensity = Tools.GetPixelIntensity(originalPictureBytes, index);
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes!, index),
                Tools.GetGrayPixel((byte) (intensity - c > Threshold ? 0 : 255)));
        }

        Store?.TriggerPictureBytesEvent(PictureBytes!, PictureSize);
    }

    #endregion

    #region MedianThresholdingCommand

    public Command? MedianThresholdingCommand { get; }

    private bool MedianThresholdingCommand_CanExecute(object? parameter) =>
        !PictureSize.IsEmpty &&
        Radius > 0 &&
        Threshold > 0;

    private void MedianThresholdingCommand_OnExecuted(object? parameter)
    {
        var originalPictureBytes = (byte[]) PictureBytes!.Clone();
        var width = (int) PictureSize.Width;
        var height = (int) PictureSize.Height;
        var radius = (int) Radius!;
        var side = 2 * radius + 1;
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
                    windowPixelList.Add(Tools.GetPixelIntensity(originalPictureBytes, windowPixelIndex));
                }
            }

            windowPixelList.Sort();

            var c = windowPixelList[windowPixelList.Count / 2];
            var index = y * width * 4 + x * 4;
            var intensity = Tools.GetPixelIntensity(originalPictureBytes, index);
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes!, index),
                Tools.GetGrayPixel((byte) (intensity - c > Threshold ? 0 : 255)));
        }

        Store?.TriggerPictureBytesEvent(PictureBytes!, PictureSize);
    }

    #endregion

    #region MinMaxThresholdingCommand

    public Command? MinMaxThresholdingCommand { get; }

    private bool MinMaxThresholdingCommand_CanExecute(object? parameter) =>
        !PictureSize.IsEmpty &&
        Radius > 0 &&
        Threshold > 0;

    private void MinMaxThresholdingCommand_OnExecuted(object? parameter)
    {
        var originalPictureBytes = (byte[]) PictureBytes!.Clone();
        var width = (int) PictureSize.Width;
        var height = (int) PictureSize.Height;
        var radius = (int) Radius!;

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var min = 0d;
            var max = 0d;
            for (var i = -radius; i <= radius; i++)
            {
                if (y + i < 0 | y + i >= height) continue;
                for (var j = -radius; j <= radius; j++)
                {
                    if (x + j < 0 | x + j >= width) continue;
                    var windowPixelIndex = (y + i) * width * 4 + (x + j) * 4;
                    var windowPixelIntensity = Tools.GetPixelIntensity(originalPictureBytes, windowPixelIndex);
                    if (min > windowPixelIndex) min = windowPixelIntensity;
                    if (max < windowPixelIntensity) max = windowPixelIntensity;
                }
            }

            var c = (min + max) / 2;
            var index = y * width * 4 + x * 4;
            var intensity = Tools.GetPixelIntensity(originalPictureBytes, index);
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes!, index),
                Tools.GetGrayPixel((byte) (intensity - c > Threshold ? 255 : 0)));
        }

        Store?.TriggerPictureBytesEvent(PictureBytes!, PictureSize);
    }

    #endregion

    #endregion
}