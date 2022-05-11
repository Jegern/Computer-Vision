using System.Collections.Generic;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace Laboratory_work_1.ViewModels;

public class HistogramViewModel : ViewModel
{
    private int[]? _histogram;

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

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public HistogramViewModel()
    {
    }

    public HistogramViewModel(ViewModelStore store) : base(store)
    {
        if (store is not null) store.HistogramChanged += histogram => Histogram = histogram;
    }

    #region Commands

    #endregion
}