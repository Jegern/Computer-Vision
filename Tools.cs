using System.Windows;

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
}