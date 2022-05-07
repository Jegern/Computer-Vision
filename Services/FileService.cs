using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace Laboratory_work_1.Services;

public class FileService
{
    public BitmapSource? OpenedImage { get; private set; }
    public string? ImageName { get; private set; }

    public bool Open(string filePath)
    {
        var (imageWidth, imageHeight) = GetSizeOfPicture(filePath);
        if (imageWidth > 1600 || imageHeight > 900) return false;
        OpenedImage = Tools.CreateImage(ReadByte(filePath), imageWidth, imageHeight);
        ImageName = Path.GetFileNameWithoutExtension(filePath);
        return true;
    }

    private static (int, int) GetSizeOfPicture(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var bitmapFrame = BitmapFrame.Create(fileStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
        var pixelWidth = bitmapFrame.PixelWidth;
        var pixelHeight = bitmapFrame.PixelHeight;
        return (pixelWidth, pixelHeight);
    }

    private static byte[] ReadByte(string filePath)
    {
        var bitmap = new Bitmap(filePath);
        var bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppRgb);
        var length = bitmapData.Stride * bitmapData.Height;
        var bytes = new byte[length];
        Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
        bitmap.UnlockBits(bitmapData);
        return bytes;
    }

    public static void Save(string filePath, BitmapSource source)
    {
        var fileExtension = Path.GetExtension(filePath);
        BitmapEncoder? encoder = fileExtension switch
        {
            ".png" => new PngBitmapEncoder(),
            ".jpg" => new JpegBitmapEncoder(),
            ".tiff" => new TiffBitmapEncoder(),
            ".bmp" => new BmpBitmapEncoder(),
            _ => null
        };
        if (encoder is null) return;

        using var fileStream = new FileStream(filePath, FileMode.Create);
        encoder.Frames.Add(BitmapFrame.Create(source));
        encoder.Save(fileStream);
    }
}