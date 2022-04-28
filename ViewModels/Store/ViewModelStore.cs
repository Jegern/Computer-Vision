using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Laboratory_work_1.ViewModels.Store;

public class ViewModelStore
{
    public event Action<BitmapSource?>? PictureChanged;
    public event Action<Point>? MousePositionChanged;
    public event Action<Visibility>? MagnifierVisibilityChanged;
    public event Action<BitmapSource?>? MagnifierWindowChanged;

    public void TriggerPictureEvent(BitmapSource? source) => PictureChanged?.Invoke(source);
    public void TriggerMousePositionEvent(Point point) => MousePositionChanged?.Invoke(point);
    public void TriggerMagnifierVisibility(Visibility visibility) => MagnifierVisibilityChanged?.Invoke(visibility);
    public void TriggerMagnifierWindowEvent(BitmapSource? source) => MagnifierWindowChanged?.Invoke(source);
}