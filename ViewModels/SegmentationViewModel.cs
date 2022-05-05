using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class SegmentationViewModel : ViewModel
{
    #region Fields

    private readonly ViewModelStore? _store;
    private BitmapSource? _picture;
    private byte[]? _pictureBytes;
    private Visibility _visibility = Visibility.Collapsed;

    private int? _areaPercentage;
    private int? _kNeighbours;

    private BitmapSource? Picture
    {
        get => _picture;
        set => Set(ref _picture, value);
    }

    private byte[]? PictureBytes
    {
        get => _pictureBytes;
        set
        {
            if (Set(ref _pictureBytes, value))
                FillHistogram();
        }
    }

    private void FillHistogram()
    {
        Array.Clear(Histogram, 0, Histogram.Length);
        for (var i = 0; i < PictureBytes!.Length; i += 4)
            Histogram[(byte) Tools.GetPixelIntensity(PictureBytes, i)]++;
    }

    private int[] Histogram { get; } = new int[256];

    public Visibility Visibility
    {
        get => _visibility;
        set => Set(ref _visibility, value);
    }

    public int? AreaPercentage
    {
        get => _areaPercentage;
        set => Set(ref _areaPercentage, value);
    }

    public int? KNeighbours
    {
        get => _kNeighbours;
        set => Set(ref _kNeighbours, value);
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public SegmentationViewModel()
    {
    }

    public SegmentationViewModel(ViewModelStore? store)
    {
        if (store is null) return;

        store.PictureChanged += Picture_OnChanged;
        store.PictureBytesChanged += PictureBytes_OnChanged;
        _store = store;

        SegmentationCommand = new Command(
            SegmentationCommand_OnExecuted,
            SegmentationCommand_CanExecute);
        PTileCommand = new Command(
            PTileCommand_OnExecuted,
            PTileCommand_CanExecute);
        HistogramDependentCommand = new Command(
            HistogramDependentCommand_OnExecuted,
            HistogramDependentCommand_CanExecute);
        KMeansCommand = new Command(
            KMeansCommand_OnExecuted,
            KMeansCommand_CanExecute);
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

    #region SegmentationCommand

    public Command? SegmentationCommand { get; }

    private bool SegmentationCommand_CanExecute(object? parameter) => Picture is not null;

    private void SegmentationCommand_OnExecuted(object? parameter)
    {
        Visibility = Visibility is Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    #endregion

    #region PTileCommand

    public Command? PTileCommand { get; }

    private bool PTileCommand_CanExecute(object? parameter) =>
        Picture is not null &&
        AreaPercentage is not null;

    private void PTileCommand_OnExecuted(object? parameter)
    {
        var sum = Histogram.Sum();
        var remainder = sum;
        int threshold;
        for (threshold = 0; threshold < 256; threshold++)
        {
            remainder -= Histogram[threshold];
            if (100 * remainder / sum < AreaPercentage) break;
        }

        ThresholdingSegmentation(threshold);
        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    private void ThresholdingSegmentation(int threshold)
    {
        for (var i = 0; i < PictureBytes!.Length; i += 4)
        {
            var intensity = Tools.GetPixelIntensity(PictureBytes, i);
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, i),
                Tools.GetGrayPixel((byte) (intensity <= threshold ? 0 : 255)));
        }
    }

    #endregion

    #region HistogramDependentCommand

    public Command? HistogramDependentCommand { get; }

    private bool HistogramDependentCommand_CanExecute(object? parameter) => Picture is not null;

    private void HistogramDependentCommand_OnExecuted(object? parameter)
    {
        var threshold = 127;
        var newThreshold = CalculateNewThreshold(threshold);

        while (Math.Abs(threshold - newThreshold) != 0)
        {
            threshold = newThreshold;
            newThreshold = CalculateNewThreshold(threshold);
        }

        ThresholdingSegmentation(threshold);
        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    private int CalculateNewThreshold(int threshold)
    {
        var leftSum = 0;
        for (var i = 0; i < threshold; i++)
            leftSum += Histogram[i] * i;
        var leftMean = leftSum / Histogram.Take(threshold).Sum();

        var rightSum = 0;
        for (var i = threshold; i < 256; i++)
            rightSum += Histogram[i] * i;
        var rightMean = rightSum / Histogram.Take(new Range(threshold, 256)).Sum();

        return (leftMean + rightMean) / 2;
    }

    #endregion

    #region KMeansCommand

    public Command? KMeansCommand { get; }

    private bool KMeansCommand_CanExecute(object? parameter) =>
        Picture is not null &&
        KNeighbours >= 2;

    private void KMeansCommand_OnExecuted(object? parameter)
    {
        var centroids = InitializeCentroids((int) KNeighbours!);
        var newCentroids = CalculateNewCentroids(centroids);
        while (!centroids.SequenceEqual(newCentroids))
        {
            newCentroids.CopyTo(centroids, 0);
            if (newCentroids[1] == 1) newCentroids[1] = 1;
            newCentroids = CalculateNewCentroids(centroids);
        }

        var thresholds = new int[centroids.Length - 1];
        for (var i = 0; i < thresholds.Length; i++)
            thresholds[i] = (centroids[i] + centroids[i + 1]) / 2;

        var palette = InitializeCentroids(centroids.Length, 0, 255);
        for (var i = 0; i < PictureBytes!.Length; i += 4)
        {
            var intensity = Tools.GetPixelIntensity(PictureBytes, i);
            var thresholdIndex = thresholds.Length;
            for (var j = 0; j < thresholds.Length; j++)
                if (intensity < thresholds[j])
                {
                    thresholdIndex = j;
                    break;
                }

            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, i),
                Tools.GetGrayPixel((byte) palette[thresholdIndex]));
        }

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    private int[] InitializeCentroids(int count, int? first = null, int? last = null)
    {
        var centroids = new int[count];
        centroids[0] = first ?? GetHistogramMin();
        centroids[^1] = last ?? GetHistogramMax();
        for (var i = 0; i < centroids.Length; i++)
            centroids[i] = centroids[0] + i * (centroids[^1] - centroids[0]) / (centroids.Length - 1);
        return centroids;
    }

    private int GetHistogramMin()
    {
        var min = 0;
        for (; min < Histogram.Length; min++)
            if (Histogram[min] > 0)
                break;
        return min;
    }

    private int GetHistogramMax()
    {
        var max = 255;
        for (; max >= 0; max--)
            if (Histogram[max] > 0)
                break;
        return max;
    }

    private int[] CalculateNewCentroids(IReadOnlyList<int> centroids)
    {
        var newCentroids = new int[centroids.Count];
        var centroidBorders = new int[centroids.Count + 1];
        centroidBorders[^1] = 255;
        for (var i = 1; i < centroidBorders.Length - 1; i++)
            centroidBorders[i] = (centroids[i] + centroids[i - 1]) / 2;
        for (var i = 0; i < centroids.Count; i++)
            newCentroids[i] = (int) GetMeanOfHistogram(centroidBorders[i], centroidBorders[i + 1]);

        return newCentroids;
    }

    private double GetMeanOfHistogram(int start, int end)
    {
        var sum = 0d;
        var counter = 0d;
        for (var i = start; i < end; i++)
        {
            sum += Histogram[i] * i;
            counter += Histogram[i];
        }

        return sum != 0 ? sum / counter : start;
    }

    public static int IndexOfClosestToTarget(IReadOnlyList<int> array, int target)
    {
        var closestIndex = 0;
        var minDifference = int.MaxValue;
        for (var i = 0; i < array.Count; i++)
        {
            var difference = Math.Abs(array[i] - target);
            if (minDifference <= difference) continue;
            minDifference = difference;
            closestIndex = i;
        }

        return closestIndex;
    }

    #endregion

    #endregion
}