using System;
using System.Collections.Generic;
using System.Linq;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace Laboratory_work_1.ViewModels;

public class HistogramViewModel : ViewModel
{
    private int[]? _histogram;
    private string _peaks = string.Empty;

    public ISeries[] Series { get; set; } =
    {
        new ColumnSeries<int>
        {
            TooltipLabelFormatter = point => $"Цвет {point.SecondaryValue} ({point.PrimaryValue})"
        }
    };

    public IEnumerable<Axis> XAxes { get; set; } = new[]
    {
        new Axis
        {
            TextSize = 0,
        }
    };

    public IEnumerable<Axis> YAxes { get; set; } = new[]
    {
        new Axis
        {
            TextSize = 0
        }
    };

    private int[]? Histogram
    {
        get => _histogram;
        set
        {
            if (Set(ref _histogram, value))
                Series[0].Values = Histogram;
        }
    }

    public string Peaks
    {
        get => _peaks;
        set => Set(ref _peaks, value);
    }

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public HistogramViewModel()
    {
    }

    public HistogramViewModel(ViewModelStore? store) : base(store)
    {
        if (store is not null) store.HistogramChanged += Histogram_OnChanged;
    }

    private void Histogram_OnChanged(int[] histogram)
    {
        Histogram = histogram;
        var output = ZScore.StartAlgo(histogram, 30, 5.0, 0.0);
        Peaks = $"Количество приков: {output.Signals?.Count(x => x == 1).ToString()!}";
    }
}

public class ZScoreOutput
{
    public List<int>? Input;
    public List<int>? Signals;
    public List<double>? AvgFilter;
    public List<double>? FilteredStddev;
}

public static class ZScore
{
    public static ZScoreOutput StartAlgo(int[] input, int lag, double threshold, double influence)
    {
        // init variables!
        var signals = new int[input.Length];
        var filteredY = new List<int>(input).ToArray();
        var avgFilter = new double[input.Length];
        var stdFilter = new double[input.Length];

        var initialWindow = new List<int>(filteredY).Skip(0).Take(lag).ToList();

        avgFilter[lag - 1] = Mean(initialWindow);
        stdFilter[lag - 1] = StdDev(initialWindow);

        for (var i = lag; i < input.Length; i++)
        {
            if (Math.Abs(input[i] - avgFilter[i - 1]) > threshold * stdFilter[i - 1])
            {
                signals[i] = (input[i] > avgFilter[i - 1]) ? 1 : -1;
                filteredY[i] = (int) (influence * input[i] + (1 - influence) * filteredY[i - 1]);
            }
            else
            {
                signals[i] = 0;
                filteredY[i] = input[i];
            }

            // Update rolling average and deviation
            var slidingWindow = new List<int>(filteredY).Skip(i - lag).Take(lag + 1).ToList();

            avgFilter[i] = Mean(slidingWindow);
            stdFilter[i] = StdDev(slidingWindow);
        }

        // Copy to convenience class 
        var result = new ZScoreOutput
        {
            Input = input.ToList(),
            AvgFilter = new List<double>(avgFilter),
            Signals = new List<int>(signals),
            FilteredStddev = new List<double>(stdFilter)
        };

        return result;
    }

    private static double Mean(IEnumerable<int> list)
    {
        // Simple helper function! 
        return list.Average();
    }

    private static double StdDev(IReadOnlyCollection<int> values)
    {
        double ret = 0;
        if (!values.Any()) return ret;
        var avg = values.Average();
        var sum = values.Sum(d => Math.Pow(d - avg, 2));
        ret = Math.Sqrt(sum / (values.Count - 1));

        return ret;
    }
}