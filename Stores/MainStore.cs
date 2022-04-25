using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Laboratory_work_1.Stores;

public class Store
{
    public event Action<bool>? PictureLoaded;
    public event Action<BitmapSource?>? PictureChanged;
    public event Action<Point>? MousePositionChanged;
    public event Action<bool>? MagnifierWindowLoaded;
    public event Action<BitmapSource?>? MagnifierWindowChanged;

    public void TriggerPictureLoadEvent() => PictureLoaded?.Invoke(true);
    public void TriggerPictureEvent(BitmapSource? source) => PictureChanged?.Invoke(source);
    public void TriggerMousePositionEvent(Point point) => MousePositionChanged?.Invoke(point);
    public void TriggerMagnifierWindowLoadEvent() => MagnifierWindowLoaded?.Invoke(true);
    public void TriggerMagnifierWindowEvent(BitmapSource? source) => MagnifierWindowChanged?.Invoke(source);
}