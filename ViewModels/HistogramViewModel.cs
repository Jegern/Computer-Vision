using System;
using System.Linq;
using System.Windows;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace Laboratory_work_1.ViewModels;

public class HistogramViewModel : ViewModel
{
    private ISeries[] _series =
    {
        new ColumnSeries<int>
        {
            Values = new int[256]
        }
    };

    private void FillHistogram()
    {
        Array.Clear(Histogram, 0, Histogram.Length);
        for (var i = 0; i < PictureBytes!.Length; i += 4)
            Histogram[(byte) Tools.GetPixelIntensity(PictureBytes, i)]++;
        Series.First().Values = Histogram;
    }

    private int[] Histogram { get; } = new int[256];

    public ISeries[] Series
    {
        get => _series;
        set => Set(ref _series, value);
    }

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public HistogramViewModel()
    {
    }

    public HistogramViewModel(ViewModelStore? store) : base(store)
    {
    }

    #region Commands

    #endregion
}