using System;
using System.Linq;
using System.Collections.Generic;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class SegmentationViewModel : ViewModel
{
    #region Fields

    private double[]? _histogram;
    private int? _areaPercentage;
    private int? _kNeighbours;

    private double[]? Histogram
    {
        get => _histogram;
        set => Set(ref _histogram, value);
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

    public SegmentationViewModel(ViewModelStore? store) : base(store)
    {
        if (store is not null) store.HistogramChanged += histogram => Histogram = histogram;
        
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

    #region Commands

    #region PTileCommand

    public Command? PTileCommand { get; }

    private bool PTileCommand_CanExecute(object? parameter) =>
        !PictureSize.IsEmpty &&
        AreaPercentage is not null;

    private void PTileCommand_OnExecuted(object? parameter)
    {
        var sum = Histogram!.Sum();
        var remainder = sum;
        int threshold;
        for (threshold = 0; threshold < 256; threshold++)
        {
            remainder -= Histogram![threshold];
            if (100 * remainder / sum < AreaPercentage) break;
        }

        ThresholdingSegmentation(threshold);
        Store?.TriggerPictureBytesEvent(PictureBytes!, PictureSize);
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

    private bool HistogramDependentCommand_CanExecute(object? parameter) => !PictureSize.IsEmpty;

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
        Store?.TriggerPictureBytesEvent(PictureBytes!, PictureSize);
    }

    private int CalculateNewThreshold(int threshold)
    {
        var leftSum = 0;
        for (var i = 0; i < threshold; i++)
            leftSum += (int) Histogram![i] * i;
        var leftMean = leftSum / (int) Histogram!.Take(threshold).Sum();

        var rightSum = 0;
        for (var i = threshold; i < 256; i++)
            rightSum += (int) Histogram![i] * i;
        var rightMean = rightSum / (int) Histogram!.Take(new Range(threshold, 256)).Sum();

        return (leftMean + rightMean) / 2;
    }

    #endregion

    #region KMeansCommand

    public Command? KMeansCommand { get; }

    private bool KMeansCommand_CanExecute(object? parameter) =>
        !PictureSize.IsEmpty &&
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

        Store?.TriggerPictureBytesEvent(PictureBytes!, PictureSize);
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
        for (; min < Histogram!.Length; min++)
            if (Histogram[min] > 0)
                break;
        return min;
    }

    private int GetHistogramMax()
    {
        var max = 255;
        for (; max >= 0; max--)
            if (Histogram![max] > 0)
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
            sum += Histogram![i] * i;
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