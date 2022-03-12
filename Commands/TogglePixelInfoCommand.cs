using System.Windows.Controls;
using Laboratory_work_1.Commands.Base;

namespace Laboratory_work_1.Commands;

public class TogglePixelInfoCommand : Command
{
    public override bool CanExecute(object? parameter) => parameter is Frame;

    public override void Execute(object? parameter)
    {
        if (parameter is not Frame frame) return;
        Getters.ChangeFrameContent(frame,
            frame.Content is null
                ? new PixelInfo()
                : null);
    }
}