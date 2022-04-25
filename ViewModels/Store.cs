using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Laboratory_work_1.ViewModels;

public class Store
{
    public event Action<BitmapImage?>? PictureChanged;
    public event Action<Point>? MousePositionChanged; 

    public void TriggerPictureEvent(BitmapImage? source) => PictureChanged?.Invoke(source);

    public void TriggerMousePositionEvent(Point point) => MousePositionChanged?.Invoke(point);
}