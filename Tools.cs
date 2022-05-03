using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Laboratory_work_1.ViewModels.Base;

namespace Laboratory_work_1;

public static class Tools
{
    public static void ResizeAndCenterWindow(Window? window)
    {
        if (window is null) return;

        window.SizeToContent = SizeToContent.WidthAndHeight;
        window.Top = (SystemParameters.WorkArea.Height - window.Height) / 2;
        window.Left = (SystemParameters.WorkArea.Width - window.Width) / 2;
    }

    public static Color GetPixelColor(BitmapSource? source, Point position)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        var bytes = GetPixelBytes(
            source,
            (int) position.X,
            (int) position.Y,
            1,
            1);
        return Color.FromRgb(bytes[2], bytes[1], bytes[0]);
    }

    public static byte[] GetPixelBytes(BitmapSource source)
    {
        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var stride = width * 4;
        var pixels = new byte[height * width * 4];
        source.CopyPixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        return pixels;
    }

    public static byte[] GetPixelBytes(BitmapSource source, byte[] pixels)
    {
        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var stride = width * 4;
        source.CopyPixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        return pixels;
    }

    public static byte[] GetPixelBytes(BitmapSource source, int x, int y, int width, int height)
    {
        var stride = width * 4;
        var pixels = new byte[height * width * 4];
        source.CopyPixels(new Int32Rect(x, y, width, height), pixels, stride, 0);
        return pixels;
    }

    public static BitmapSource CreateImage(BitmapSource source, byte[] pixels)
    {
        var width = source.PixelWidth;
        var height = source.PixelHeight;

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

    public static BitmapSource CreateImage(BitmapSource source, byte[] pixels, int width, int height)
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

    public static double GetPixelIntensivity(byte[] bytes, int index) =>
        (bytes[index] + bytes[index + 1] + bytes[index + 2]) / 3.0;


    public static double GetMedianFromList(IEnumerable<double> source)
    {
        var data = source.OrderBy(x => x).ToArray();
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        return data[data.Length / 2];
    }

    public static ArraySegment<byte> GetPixel(byte[] bytes, int index) => new(bytes, index, 3);

    private static readonly byte[] GrayPixel = new byte[3];

    // public static byte[] GetGrayPixel(byte value) => new[] {value, value, value};
    public static byte[] GetGrayPixel(byte value)
    {
        GrayPixel[0] = value;
        GrayPixel[1] = value;
        GrayPixel[2] = value;
        return GrayPixel;
    }


    public static void SetPixel(ArraySegment<byte> left, IReadOnlyList<byte> right)
    {
        for (var i = 0; i < left.Count; i++)
            left[i] = right[i];
    }

    public static void Swap<T>(ref T left, ref T right)
    {
        (left, right) = (right, left);
    }

    public static void SwapPixels(ArraySegment<byte> left, ArraySegment<byte> right)
    {
        for (var i = 0; i < left.Count; i++)
            (left[i], right[i]) = (right[i], left[i]);
    }
}