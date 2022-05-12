using System.Linq;
using System.Collections.Generic;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using MathNet.Numerics.Statistics;

namespace Laboratory_work_1.ViewModels;

public class HistogramViewModel : ViewModel
{
    #region Fields

    private int[]? _histogram;

    public ISeries[] Series { get; set; } =
    {
        new ColumnSeries<int>
        {
            TooltipLabelFormatter = point => $"Цвет {point.SecondaryValue} ({point.PrimaryValue})"
        }
    };

    public Axis[] XAxes { get; set; } =
    {
        new()
        {
            TextSize = 0,
            ShowSeparatorLines = false
        }
    };

    public Axis[] YAxes { get; set; } =
    {
        new()
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

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public HistogramViewModel()
    {
    }

    public HistogramViewModel(ViewModelStore store) : base(store)
    {
        store.HistogramChanged += Histogram_OnChanged;
    }

    #region Event Subscription

    private void Histogram_OnChanged(int[] histogram)
    {
        Histogram = histogram;
        XAxes.First().Name = $"Количество пиков: {BaselinePeakFinding(histogram).Count(x => x)}";
        XAxes.First().NameTextSize = 15;
    }

    private static IEnumerable<bool> DispersionPeakFinding(
        IReadOnlyList<int> signals,
        int lag = 30,
        double threshold = 9.0,
        double influence = 0.5)
    {
        var peaks = new bool[signals.Count];
        var processedSignals = new double[signals.Count];

        for (var i = lag; i < signals.Count; i++)
        {
            var avg = processedSignals.Skip(i - lag).Take(lag).Mean();
            var std = processedSignals.Skip(i - lag).Take(lag).StandardDeviation();
            if (signals[i] - avg > threshold * std)
            {
                peaks[i] = true;
                processedSignals[i] = influence * signals[i] + (1 - influence) * processedSignals[i - 1];
            }
            else
            {
                processedSignals[i] = signals[i];
            }
        }

        return peaks;
    }

    private static IEnumerable<bool> BaselinePeakFinding(IReadOnlyCollection<int> signals)
    {
        var smoothInput = new double[signals.Count];
        for (var i = 2; i < signals.Count; i++)
            smoothInput[i] = signals.Skip(i - 2).Take(2).Average();

        var peaks = new bool[signals.Count];
        int? peakIndex = null;
        double? peakValue = null;
        var baseline = smoothInput.Average();

        for (var i = 0; i < smoothInput.Length; i++)
        {
            var value = smoothInput[i];
            if (smoothInput[i] > baseline)
            {
                if (value <= peakValue) continue;
                peakIndex = i;
                peakValue = value;
            }
            else if (value < baseline && peakIndex != null)
            {
                peaks[(int) peakIndex] = true;
                peakIndex = null;
                peakValue = null;
            }
        }

        if (peakIndex != null)
            peaks[(int) peakIndex] = true;

        return peaks;
    }

    #endregion
}