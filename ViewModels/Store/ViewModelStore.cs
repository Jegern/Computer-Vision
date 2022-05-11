using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Laboratory_work_1.ViewModels.Store;

public class ViewModelStore
{
    public event Action<BitmapSource>? PictureChanged;
    public event Action<Size>? PictureSizeChanged; 
    public event Action<byte[]>? PictureBytesChanged;
    public event Action<Point>? MousePositionChanged;
    public event Action<byte[]>? AntiAliasingPictureBytesChanged;
    public event Action<int[]>? HistogramChanged;

    public void TriggerPictureSizeEvent(Size size) => PictureSizeChanged?.Invoke(size);
    public void TriggerPictureBytesEvent(byte[] bytes)
    {
        PictureBytesChanged?.Invoke(bytes);
        HistogramChanged?.Invoke(Tools.GetHistogram(bytes));
    }

    public void TriggerPictureBytesEvent(byte[] bytes, Size size)
    {
        PictureChanged?.Invoke(Tools.CreateImage(bytes, size.Width, size.Height));
        HistogramChanged?.Invoke(Tools.GetHistogram(bytes));
    }

    public void TriggerMousePositionEvent(Point point) => MousePositionChanged?.Invoke(point);
    public void TriggerAntiAliasingPictureBytesEvent(byte[] bytes) => AntiAliasingPictureBytesChanged?.Invoke(bytes);
    public void TriggerHistogramEvent(int[] histogram) => HistogramChanged?.Invoke(histogram);
}