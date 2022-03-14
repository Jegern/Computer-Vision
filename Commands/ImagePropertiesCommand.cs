using System;

namespace Laboratory_work_1.Commands;

internal class ImagePropertiesCommand : Command
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool>? _canExecute;

    public ImagePropertiesCommand(Action<object> execute, Func<object, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public override bool CanExecute(object parameter) => true;

    public override void Execute(object parameter) => _execute(parameter);
}