using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using OxyPlot;

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
        var source = (BitmapSource) _owner.Picture.Source;
        var stringNumber = Convert.ToInt32(StringNumber.Text);
        var width = source.PixelWidth;
        var stringBytes = MainWindow.GetPixels(source, 0, stringNumber, width, 1);
        var stringBrightnesses = GetBrightnessesFromBytes(stringBytes);

        FillPointsWithBrightnessPoints(stringBrightnesses);
    }

    private static IEnumerable<double> GetBrightnessesFromBytes(byte[] bytes)
    {
        var listOfBrightnesses = new List<double>();
        for (var i = 0; i < bytes.Length; i += 4)
            listOfBrightnesses.Add(MainWindow.GetPixelBrightness(bytes, i));
        return listOfBrightnesses.ToArray();
    }

    private void FillPointsWithBrightnessPoints(IEnumerable<double> brightnessPoints)
    {
        var viewModel = (BrightnessProfileViewModel) DataContext;
        var newPoints = brightnessPoints.Select((t, i) => new DataPoint(i, t)).ToList();
        viewModel.Points = newPoints;
    }
}

public class BrightnessProfileViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private List<DataPoint> _points = new();

    public List<DataPoint> Points
    {
        get => _points;
        set
        {
            if (Equals(_points, value)) return;
            _points = value;
            OnPropertyChanged();
        }
    }
}