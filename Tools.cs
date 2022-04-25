using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Laboratory_work_1;

public class Tools
{
    public static void ResizeAndCenterWindow(Window? window)
    {
        if (window is null) return;
        window.SizeToContent = SizeToContent.WidthAndHeight;
        window.Top = (SystemParameters.WorkArea.Height - window.Height) / 2;
        window.Left = (SystemParameters.WorkArea.Width - window.Width) / 2;
    }
    
    public static Color GetPixelColor(BitmapSource? source, Point? pixelPosition)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (pixelPosition is null) throw new ArgumentNullException(nameof(pixelPosition));
        
        var pixelBytes = GetPixels(
            source,
            (int) pixelPosition.Value.X,
            (int) pixelPosition.Value.Y,
            1,
            1);
        return Color.FromRgb(pixelBytes[2], pixelBytes[1], pixelBytes[0]);
    }
    
    public static byte[] GetPixels(BitmapSource source, int x = 0, int y = 0, int width = 0, int height = 0)
    {
        if (width == 0)
            width = source.PixelWidth;
        if (height == 0)
            height = source.PixelHeight;
        var stride = width * 4;
        var pixels = new byte[height * width * 4];
        source.CopyPixels(new Int32Rect(x, y, width, height), pixels, stride, 0);
        return pixels;
    }
    
    internal static BitmapSource CreateImage(BitmapSource source,
        byte[] pixels,
        int width,
        int height)
    {
        return BitmapSource.Create(
            width,
            height,
            source.DpiX,
            source.DpiY,
            source.Format,
            source.Palette,
            pixels,
            width * 4);
    }

    
    // private static (int Width, int Height) GetSlidingWindowSize(
    //     BitmapSource source,
    //     Point center,
    //     int size)
    // {
    //     var location = (X: (int) center.X - 5, Y: (int) center.Y - 5);
    //     var size = (Width: 11, Height: 11);
    //     if (center.X - 5 < 0)
    //     {
    //         size.Width += location.X;
    //         location.X = 0;
    //     }
    //     if (center.Y - 5 < 0)
    //     {
    //         size.Height += location.Y;
    //         location.Y = 0;
    //     }
    //     if (location.X + 11 > source.PixelWidth)
    //         size.Width = source.PixelWidth - location.X;
    //     if (location.Y + 11 > source.PixelHeight)
    //         size.Height = source.PixelHeight - location.Y;
    //     return (location, size);
    // }

}