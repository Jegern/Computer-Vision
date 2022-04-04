using System.Windows;
using Microsoft.Win32;

namespace Laboratory_work_1.Services;

public class DialogService
{
    public string? FilePath { get; set; }
 
    public bool OpenFileDialog()
    {
        var openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() == false) return false;
        FilePath = openFileDialog.FileName;
        return true;
    }
 
    public bool SaveFileDialog()
    {
        var saveFileDialog = new SaveFileDialog();
        if (saveFileDialog.ShowDialog() == false) return false;
        FilePath = saveFileDialog.FileName;
        return true;
    }
 
    public void ShowError(string message)
    {
        MessageBox.Show(
            message,
            "Ошибка!",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}