using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Stores;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;

namespace Laboratory_work_1.ViewModels;

public class MagnifierInfoViewModel : ViewModel
{
    #region Fields

    private Visibility _magnifierInfoVisibility = Visibility.Collapsed;
    private BitmapSource? _magnifierWindow;
    private double _magnifierMean;
    private double _magnifierDeviation;
    private double _magnifierMedian;

    public Visibility MagnifierInfoVisibility
    {
        get => _magnifierInfoVisibility;
        set => Set(ref _magnifierInfoVisibility, value);
    }

    private BitmapSource? MagnifierWindow
    {
        get => _magnifierWindow;
        set => Set(ref _magnifierWindow, value);
    }

    public double MagnifierMean
    {
        get => _magnifierMean;
        set => Set(ref _magnifierMean, value);
    }

    public double MagnifierDeviation
    {
        get => _magnifierDeviation;
        set => Set(ref _magnifierDeviation, value);
    }

    public double MagnifierMedian
    {
        get => _magnifierMedian;
        set => Set(ref _magnifierMedian, value);
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public MagnifierInfoViewModel()
    {
        
    }

    public MagnifierInfoViewModel(Store? store)
    {
        if (store is null) return;
        
        store.MagnifierWindowChanged += MagnifierWindow_OnChanged;

        MagnifierInfoCommand = new Command(MagnifierInfoCommand_OnExecuted, MagnifierInfoCommand_CanExecute);
    }

    #region Event Subscription

    private void MagnifierWindow_OnChanged(BitmapSource? magnifierWindow)
    {
        MagnifierWindow = magnifierWindow;
        UpdateMagnifierInfo();
    }

    private void UpdateMagnifierInfo()
    {
        
    }

    #endregion

    #region MagnifierInfoCommand

    public Command? MagnifierInfoCommand { get; }

    private bool MagnifierInfoCommand_CanExecute(object? parameter) => MagnifierWindow is not null;

    private void MagnifierInfoCommand_OnExecuted(object? parameter)
    {
        MagnifierInfoVisibility =
            MagnifierInfoVisibility is Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion
}