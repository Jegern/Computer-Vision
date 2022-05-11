using System;
using System.Linq;
using System.Collections.Generic;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;
using OpenCvSharp;

namespace Laboratory_work_1.ViewModels;

public class SegmentationViewModel : ViewModel
{
    #region Fields

    private int[]? _histogram;
    private int? _areaPercentage;
    private int? _kNeighbours;
    private int? _cannyThreshold;

    private int[]? Histogram
    {
        get => _histogram;
        set => Set(ref _histogram, value);
    }

    public int? AreaPercentage
    {
        get => _areaPercentage;
        set => Set(ref _areaPercentage, value);
    }

    public int? CannyThreshold
    {
        get => _cannyThreshold;
        set => Set(ref _cannyThreshold, value);
    }

    public int? KNeighbours
    {
        get => _kNeighbours;
        set => Set(ref _kNeighbours, value);
    }

    private int[,] SobelMask3X3 { get; } =
    {
        {-1, -2, -1},
        {0, 0, 0},
        {1, 2, 1}
    };

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public SegmentationViewModel()
    {
    }

    public SegmentationViewModel(ViewModelStore store) : base(store)
    {
        store.HistogramChanged += histogram => Histogram = histogram;

        PTileCommand = new Command(
            PTileCommand_OnExecuted,
            PTileCommand_CanExecute);
        HistogramDependentCommand = new Command(
            HistogramDependentCommand_OnExecuted,
            HistogramDependentCommand_CanExecute);
        KMeansCommand = new Command(
            KMeansCommand_OnExecuted,
            KMeansCommand_CanExecute);
        CannyMethodCommand = new Command(
            CannyMethodCommand_OnExecuted,
            CannyMethodCommand_CanExecute);
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

    #region CannyMethodCommand

    public Command? CannyMethodCommand { get; }

    private bool CannyMethodCommand_CanExecute(object? parameter) =>
        !PictureSize.IsEmpty &&
        CannyThreshold > 0;

    private void CannyMethodCommand_OnExecuted(object? parameter)
    {
        var originalPictureBytes = (byte[]) PictureBytes!.Clone();
        var width = (int) PictureSize.Width;
        var height = (int) PictureSize.Height;
        var mask = SobelMask3X3;
        var grad = new double [height, width, 3];

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var gx = 0d;
            var gy = 0d;
            for (var i = -1; i <= 1; i++)
            {
                if (y + i < 0 | y + i >= height) continue;
                for (var j = -1; j <= 1; j++)
                {
                    if (x + j < 0 | x + j >= width) continue;
                    var windowPixelIndex = (i + y) * width * 4 + (j + x) * 4;
                    gx += Tools.GetPixelIntensity(originalPictureBytes, windowPixelIndex) *
                          mask[i + 1, j + 1];
                    gy += Tools.GetPixelIntensity(originalPictureBytes, windowPixelIndex) *
                          mask[j + 1, i + 1];
                }
            }

            var f = Math.Sqrt(gx * gx + gy * gy);
            var direction = ((Math.Round(Math.Atan2(gx, gy) / (Math.PI / 4)) * (Math.PI / 4) - Math.PI / 2) /
                (Math.PI / 4) + 8) % 8;
            var threshold = CannyThreshold;
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, y * width * 4 + x * 4),
                Tools.GetGrayPixel((byte) (f > threshold ? 0 : 255)));
            grad[y, x, 0] = f;
            grad[y, x, 1] = direction;
        }

        var comparison = new double[2];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            if ((int) Tools.GetPixelIntensity(PictureBytes, y * width * 4 + x * 4) == 255) continue;
            if (y - 1 < 0 | y + 1 >= height) continue;
            if (x - 1 < 0 | x + 1 >= width) continue;
            int direct = (int) grad[y, x, 1];
            (comparison[0], comparison[1]) = (direct % 4) switch
            {
                0 => (grad[y, x - 1, 0], grad[y, x + 1, 0]),
                1 => (grad[y - 1, x + 1, 0], grad[y + 1, x - 1, 0]),
                2 => (grad[y + 1, x, 0], grad[y - 1, x, 0]),
                3 => (grad[y - 1, x - 1, 0], grad[y + 1, x + 1, 0]),
                _ => throw new ArgumentOutOfRangeException()
            };
            if (grad[y, x, 0] <= comparison[0] && grad[y, x, 0] <= comparison[1])
            {
                Tools.SetPixel(
                    Tools.GetPixel(PictureBytes, y * width * 4 + x * 4),
                    Tools.GetGrayPixel(255));
            }
        }

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            if ((int) Tools.GetPixelIntensity(PictureBytes, y * width * 4 + x * 4) == 255 ||
                (int) grad[y, x, 2] == 1) continue;
            if (y - 1 < 0 | y + 1 >= height) continue;
            if (x - 1 < 0 | x + 1 >= width) continue;
            grad[y, x, 2] = 1;
            var flag = true;
            var i = 0;
            var j = 0;
            while (flag)
            {
                if (y - i - 1 <= 0 | y + i + 1 >= height | y + i - 1 <= 0 | y - i + 1 >= height) break;
                if (x - j - 1 <= 0 | x + j + 1 >= width | x + j - 1 <= 0 | x - j + 1 >= width) break;
                if ((int) Tools.GetPixelIntensity(PictureBytes, (y + i) * width * 4 + (x + j + 1) * 4) == 0 &&
                    (int) grad[y + i, x + j + 1, 2] == 0)
                {
                    grad[y + i, x + j + 1, 2] = 1;
                    j++;
                }
                else if ((int) Tools.GetPixelIntensity(PictureBytes, (y + i + 1) * width * 4 + (x + j + 1) * 4) == 0 &&
                         (int) grad[y + i + 1, x + j + 1, 2] == 0)
                {
                    grad[y + i + 1, x + j + 1, 2] = 1;
                    i++;
                    j++;
                }
                else if ((int) Tools.GetPixelIntensity(PictureBytes, (y + i + 1) * width * 4 + (x + j) * 4) == 0 &&
                         (int) grad[y + i + 1, x + j, 2] == 0)
                {
                    grad[y + i + 1, x + j, 2] = 1;
                    i++;
                }
                else if ((int) Tools.GetPixelIntensity(PictureBytes, (y + i + 1) * width * 4 + (x + j - 1) * 4) == 0 &&
                         (int) grad[y + i + 1, x + j - 1, 2] == 0)
                {
                    grad[y + i + 1, x + j - 1, 2] = 1;
                    j--;
                    i++;
                }
                else if ((int) Tools.GetPixelIntensity(PictureBytes, (y + i) * width * 4 + (x + j - 1) * 4) == 0 &&
                         (int) grad[y + i, x + j - 1, 2] == 0)
                {
                    grad[y + i, x + j - 1, 2] = 1;
                    j--;
                }
                else if ((int) Tools.GetPixelIntensity(PictureBytes, (y + i - 1) * width * 4 + (x + j - 1) * 4) == 0 &&
                         (int) grad[y + i - 1, x + j - 1, 2] == 0)
                {
                    grad[y + i - 1, x + j - 1, 2] = 1;
                    i--;
                    j--;
                }
                else if ((int) Tools.GetPixelIntensity(PictureBytes, (y + i - 1) * width * 4 + (x + j) * 4) == 0 &&
                         (int) grad[y + i - 1, x + j, 2] == 0)
                {
                    grad[y + i - 1, x + j, 2] = 1;
                    i--;
                }
                else if ((int) Tools.GetPixelIntensity(PictureBytes, (y + i - 1) * width * 4 + (x + j + 1) * 4) == 0 &&
                         (int) grad[y + i - 1, x + j + 1, 2] == 0)
                {
                    grad[y + i - 1, x + j + 1, 2] = 1;
                    i--;
                    j++;
                }
                else if ((int) Tools.GetPixelIntensity(PictureBytes, (y + i) * width * 4 + (x + j) * 4) == 0 &&
                         (int) grad[y + i, x + j, 2] == 1)
                {
                    var flagFill = true;
                    var q = i;
                    var w = j;
                    if (w < 0)
                    {
                        w--;
                    }

                    while (flagFill)
                    {
                        if (y - q - 1 <= 0 | y + q + 1 >= height | y + q - 1 <= 0 | y - q + 1 >= height) break;
                        if (x - w - 1 <= 0 | x + w + 1 >= width | x + w - 1 <= 0 | x - w + 1 >= width) break;
                        if ((int) Tools.GetPixelIntensity(PictureBytes, (y + q) * width * 4 + (x + w + 1) * 4) == 255)
                        {
                            Tools.SetPixel(
                                Tools.GetPixel(PictureBytes, (y + q) * width * 4 + (x + w + 1) * 4),
                                Tools.GetGrayPixel(0));
                            w++;
                        }
                        else if ((int) Tools.GetPixelIntensity(PictureBytes,
                                     (y + q + 1) * width * 4 + (x + w + 1) * 4) ==
                                 255)
                        {
                            Tools.SetPixel(
                                Tools.GetPixel(PictureBytes, (y + q + 1) * width * 4 + (x + w + 1) * 4),
                                Tools.GetGrayPixel(0));
                            q++;
                            w++;
                        }
                        else if ((int) Tools.GetPixelIntensity(PictureBytes, (y + q + 1) * width * 4 + (x + w) * 4) ==
                                 255)
                        {
                            Tools.SetPixel(
                                Tools.GetPixel(PictureBytes, (y + q + 1) * width * 4 + (x + w) * 4),
                                Tools.GetGrayPixel(0));
                            q++;
                        }

                        else if ((int) Tools.GetPixelIntensity(PictureBytes,
                                     (y + q + 1) * width * 4 + (x + w - 1) * 4) ==
                                 255)
                        {
                            Tools.SetPixel(
                                Tools.GetPixel(PictureBytes, (y + q + 1) * width * 4 + (x + w - 1) * 4),
                                Tools.GetGrayPixel(0));
                            w--;
                            q++;
                        }

                        else if ((int) Tools.GetPixelIntensity(PictureBytes, (y + q) * width * 4 + (x + w - 1) * 4) ==
                                 255)
                        {
                            Tools.SetPixel(
                                Tools.GetPixel(PictureBytes, (y + q) * width * 4 + (x + w - 1) * 4),
                                Tools.GetGrayPixel(0));
                            w--;
                        }
                        else if ((int) Tools.GetPixelIntensity(PictureBytes,
                                     (y + q - 1) * width * 4 + (x + w - 1) * 4) ==
                                 255)
                        {
                            Tools.SetPixel(
                                Tools.GetPixel(PictureBytes, (y + q - 1) * width * 4 + (x + w - 1) * 4),
                                Tools.GetGrayPixel(0));
                            w--;
                            q--;
                        }
                        else if ((int) Tools.GetPixelIntensity(PictureBytes, (y + q - 1) * width * 4 + (x + w) * 4) ==
                                 255)
                        {
                            Tools.SetPixel(
                                Tools.GetPixel(PictureBytes, (y + q - 1) * width * 4 + (x + w) * 4),
                                Tools.GetGrayPixel(0));
                            q--;
                        }
                        else if ((int) Tools.GetPixelIntensity(PictureBytes,
                                     (y + q - 1) * width * 4 + (x + w + 1) * 4) ==
                                 255)
                        {
                            Tools.SetPixel(
                                Tools.GetPixel(PictureBytes, (y + q - 1) * width * 4 + (x + w + 1) * 4),
                                Tools.GetGrayPixel(0));
                            w++;
                            q--;
                        }
                        else
                        {
                            flagFill = false;
                            flag = false;
                        }
                    }
                }
                else
                {
                    flag = false;
                }
            }
        }

        Store?.TriggerPictureBytesEvent(PictureBytes!, PictureSize);
    }

    #endregion

    #endregion
}