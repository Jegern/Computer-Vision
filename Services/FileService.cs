using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Laboratory_work_1.Services;

public class FileService
{
    public BitmapImage? Image { get; set; }
    
    public bool Open(string filePath)
    {
        var (width, height) = GetSizeOfPicture(filePath);
        if (width > 1600 || height > 900) return false;
        Image = new BitmapImage(new Uri(filePath));
        return true;
    }
    
    private static (int, int) GetSizeOfPicture(string fileName)
    {
        using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        var bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
        var width = bitmapFrame.PixelWidth;
        var height = bitmapFrame.PixelHeight;
        return (width, height);
    }
 
    public void Save(string filePath, BitmapImage image)
    {
        BitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));

        using var fileStream = new FileStream(filePath, FileMode.Create);
        encoder.Save(fileStream);
    }
}