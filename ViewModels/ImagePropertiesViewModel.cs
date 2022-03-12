using System.Windows.Input;
using Laboratory_work_1.Commands;

namespace Laboratory_work_1.ViewModels;

public class ImagePropertiesViewModel : ViewModel
{
    public ImagePropertiesViewModel()
    {
        DecolorizationCommand = new ImagePropertiesCommand(
            DecolorizationCommand_OnExecuted,
            DecolorizationCommand_CanExecute);
    }

    #region DecolorizationCommand

    public ICommand DecolorizationCommand { get; }

    private static bool DecolorizationCommand_CanExecute(object? parameter) => true;

    private static void DecolorizationCommand_OnExecuted(object? parameter)
    {
        
    }

    #endregion
}