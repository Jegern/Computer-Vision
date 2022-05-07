using System.Drawing;
using System.Windows;
using Laboratory_work_1.ViewModels;

namespace Laboratory_work_1.Views;

public partial class Histogram
{
    
    private HistogramViewModel? ViewModel { get; set; }

    public Histogram()
    {
        InitializeComponent();
    }

    private void Histogram_OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel = (HistogramViewModel) DataContext;
        if (ViewModel.Store is not null) ViewModel.Store.HistogramChanged += Histogram_OnChanged;
        WpfPlot.Configuration.Pan = false;
        WpfPlot.Configuration.Zoom = false;
        WpfPlot.Plot.XAxis.IsVisible = false;
        WpfPlot.Plot.XAxis2.IsVisible = false;
        WpfPlot.Plot.YAxis.IsVisible = false;
        WpfPlot.Plot.YAxis2.IsVisible = false;
    }

    private void Histogram_OnChanged(double[] histogram)
    {
        WpfPlot.Plot.Clear();
        WpfPlot.Plot.AddBar(histogram, Color.Black);
        WpfPlot.Refresh();
    }
}