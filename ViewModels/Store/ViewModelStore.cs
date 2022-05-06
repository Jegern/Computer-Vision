using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Laboratory_work_1.ViewModels.Store;

public class ViewModelStore
{
    public event Action<BitmapSource>? PictureChanged;
    public event Action<byte[]>? PictureBytesChanged;
    public event Action<byte[]>? AntiAliasingPictureBytesChanged;
    public event Action<Point>? MousePositionChanged;

    public void TriggerPictureEvent(BitmapSource source) => PictureChanged?.Invoke(source);
    public void TriggerPictureBytesEvent(byte[] bytes) => PictureBytesChanged?.Invoke(bytes);

    public void TriggerPictureBytesEvent(BitmapSource source, byte[] bytes)
        => PictureChanged?.Invoke(Tools.CreateImage(source, bytes));

    public void TriggerAntiAliasingPictureBytesEvent(byte[] bytes) => AntiAliasingPictureBytesChanged?.Invoke(bytes);
    public void TriggerMousePositionEvent(Point point) => MousePositionChanged?.Invoke(point);
}