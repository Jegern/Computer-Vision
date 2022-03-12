using Laboratory_work_1.Commands.Base;
using Microsoft.Win32;

namespace Laboratory_work_1.Commands;

public class OpenImageCommand : Command
{
    public override bool CanExecute(object? parameter) => parameter is MainWindow;

    public override void Execute(object? parameter)
    {
        if (parameter is not MainWindow mainWindow) return;
        var openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() == true)
            Getters.CheckResolutionThenLoadImageFromFileName(openFileDialog.FileName, mainWindow);
    }
}