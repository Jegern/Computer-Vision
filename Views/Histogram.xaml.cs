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
    }

    private void Histogram_OnChanged(double[] histogram)
    {
        Plot.Plot.Clear();
        Plot.Plot.AddBar(histogram);
        Plot.Refresh();
    }
}