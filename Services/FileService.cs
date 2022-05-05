using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Laboratory_work_1.Services;

public class FileService
{
    public BitmapImage? OpenedImage { get; private set; }
    public string? ImageName { get; private set; }
    
    public bool Open(string filePath)
    {
        var (imageWidth, imageHeight) = GetSizeOfPicture(filePath);
        if (imageWidth > 1600 || imageHeight > 900) return false;
        OpenedImage = new BitmapImage(new Uri(filePath));
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
 
    public void Save(string filePath, BitmapSource source)
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