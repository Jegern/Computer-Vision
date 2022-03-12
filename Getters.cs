using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Laboratory_work_1;

public static class Getters
{
    public static (int, int) GetSizeOfPicture(string fileName)
    {
        using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        var bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
        var width = bitmapFrame.PixelWidth;
        var height = bitmapFrame.PixelHeight;
        return (width, height);
    }

    public static void CheckResolutionThenLoadImageFromFileName(string fileName, MainWindow mainWindow)
    {
        var (width, height) = Getters.GetSizeOfPicture(fileName);
        if (width <= 1600 && height <= 900)
            LoadImage(fileName, mainWindow);
        else
            ShowWrongResolutionMessage();
    }

    private static void LoadImage(string fileName, MainWindow mainWindow)
    {
        mainWindow.MainImage.Source = new BitmapImage(new Uri(fileName));
        mainWindow.SizeToContent = SizeToContent.WidthAndHeight;
        CenterWindowOnScreen(mainWindow);
    }
    
    public static void CenterWindowOnScreen(Window window)
    {
        window.Top = (SystemParameters.WorkArea.Height - window.Height) / 2;
        window.Left = (SystemParameters.WorkArea.Width - window.Width) / 2;
    }

    private static void ShowWrongResolutionMessage()
    {
        MessageBox.Show(
            "Разрешение картинки не может быть больше 1600x900.",
            "Ошибка!",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}