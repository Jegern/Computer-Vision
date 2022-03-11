using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using OxyPlot;
using OxyPlot.Series;

namespace Laboratory_work_1;

public partial class BrightnessProfile
{
    private readonly MainWindow _owner;

    public BrightnessProfile(MainWindow owner)
    {
        InitializeComponent();
        _owner = owner;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var source = (BitmapSource) _owner.MainImage.Source;
        var stringNumber = Convert.ToInt32(StringNumber.Text);
        var width = source.PixelWidth;
        var stringBytes = MainWindow.GetPixels(source, 0, stringNumber, width, 1);
        var stringBrightnesses = GetBrightnessesFromBytes(stringBytes);

        var lineSeries = CreatePlotLineSeries(stringBrightnesses);
        DisplayLineSeries(lineSeries);
    }

    private static IReadOnlyList<double> GetBrightnessesFromBytes(byte[] bytes)
    {
        var listOfBrightnesses = new List<double>();
        for (var i = 0; i < bytes.Length; i += 4)
            listOfBrightnesses.Add(MainWindow.GetPixelBrightness(bytes, i));
        return listOfBrightnesses.ToArray();
    }

    private static LineSeries CreatePlotLineSeries(IReadOnlyList<double> points)
    {
        var lineSeries = new LineSeries {MarkerType = MarkerType.Circle};
        for (var i = 0; i < points.Count; i++)
            lineSeries.Points.Add(new DataPoint(i, points[i]));
        return lineSeries;
    }

    private void DisplayLineSeries(Series lineSeries)
    {
        var viewModel = (BrightnessProfileViewModel) DataContext;
        var plotModel = new PlotModel();
        plotModel.Series.Add(lineSeries);
        viewModel.Model = plotModel;
    }
}

public class BrightnessProfileViewModel
{
    public PlotModel Model { get; set; } = null!;
}