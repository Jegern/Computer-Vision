using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Laboratory_work_1;

public static class Tools
{
    public static BitmapSource CreateImage(byte[] pixels, double width, double height)
    {
        return BitmapSource.Create(
            (int) width,
            (int) height,
            96,
            96,
            PixelFormats.Bgr32, 
            null,
            pixels,
            (int) width * 4);
    }

    public static int[] GetHistogram(byte[] bytes)
    {
        var histogram = new int[256];
        for (var i = 0; i < bytes.Length; i += 4)
            histogram[(byte) GetPixelIntensity(bytes, i)]++;
        return histogram;
    }

    public static double GetPixelIntensity(byte[] bytes, int index = 0) =>
        (bytes[index] + bytes[index + 1] + bytes[index + 2]) / 3.0;

    public static ArraySegment<byte> GetPixel(byte[] bytes, int index) => new(bytes, index, 4);
    

    private static readonly byte[] GrayPixel = new byte[3];

    public static byte[] GetGrayPixel(byte value)
    {
        GrayPixel[0] = value;
        GrayPixel[1] = value;
        GrayPixel[2] = value;
        return GrayPixel;
    }

    public static void SetPixel(ArraySegment<byte> left, IReadOnlyList<byte> right)
    {
        for (var i = 0; i < right.Count; i++)
            left[i] = right[i];
    }
}