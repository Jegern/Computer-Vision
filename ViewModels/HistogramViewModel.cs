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
    private byte[]? _pictureBytes;
    private Visibility _visibility = Visibility.Collapsed;

    private ISeries[] _series =
    {
        new ColumnSeries<int>
        {
            Values = new int[256]
        }
    };

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
        Series.First().Values = Histogram;
    }

    private int[] Histogram { get; } = new int[256];

    public Visibility Visibility
    {
        get => _visibility;
        set => Set(ref _visibility, value);
    }

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

    public HistogramViewModel(ViewModelStore? store)
    {
        if (store is null) return;

        store.PictureBytesChanged += PictureBytes_OnChanged;

        HistogramCommand = new Command(
            HistogramCommand_OnExecuted,
            HistogramCommand_CanExecute);
    }

    #region Event Subscription

    private void PictureBytes_OnChanged(byte[] bytes)
    {
        PictureBytes = bytes;
    }

    #endregion

    #region Commands

    #region HistogramCommand

    public Command? HistogramCommand { get; }

    private bool HistogramCommand_CanExecute(object? parameter) => PictureBytes is not null;

    private void HistogramCommand_OnExecuted(object? parameter)
    {
        Visibility = Visibility is Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    #endregion

    #endregion
}