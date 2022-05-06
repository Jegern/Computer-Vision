using System;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class ChromacityViewModel : ViewModel
{
    #region Fields

    private byte? _binaryThreshold;
    private byte? _cutStart;
    private byte? _cutEnd;
    private int? _binaryFlat;

    public byte? BinaryThreshold
    {
        get => _binaryThreshold;
        set => Set(ref _binaryThreshold, value);
    }

    public byte? CutStart
    {
        get => _cutStart;
        set => Set(ref _cutStart, value);
    }

    public byte? CutEnd
    {
        get => _cutEnd;
        set => Set(ref _cutEnd, value);
    }

    public int? BinaryFlat
    {
        get => _binaryFlat;
        set => Set(ref _binaryFlat, value);
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public ChromacityViewModel()
    {
    }

    public ChromacityViewModel(ViewModelStore? store) : base(store)
    {
        BinaryTransformationCommand = new Command(
            BinaryTransformationCommand_OnExecuted,
            BinaryTransformationCommand_CanExecute);
        IntensityRangeCutCommand = new Command(
            IntensityRangeCutCommand_OnExecuted,
            IntensityRangeCutCommand_CanExecute);
        BinaryFlatCutCommand = new Command(
            BinaryFlatCutCommand_OnExecuted,
            BinaryFlatCutCommand_CanExecute);
        LogarithmicTransformationCommand = new Command(
            LogarithmicTransformationCommand_OnExecuted,
            LogarithmicTransformationCommand_CanExecute);
        PowerTransformationCommand = new Command(
            PowerTransformationCommand_OnExecuted,
            PowerTransformationCommand_CanExecute);
    }

    #region Commands

    #region BinaryTransformationCommand

    public Command? BinaryTransformationCommand { get; }

    private bool BinaryTransformationCommand_CanExecute(object? parameter) =>
        Picture is not null &&
        BinaryThreshold is not null;

    private void BinaryTransformationCommand_OnExecuted(object? parameter)
    {
        for (var i = 0; i < PictureBytes!.Length; i += 4)
        {
            var intensity = Tools.GetPixelIntensity(PictureBytes, i);
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, i),
                Tools.GetGrayPixel((byte) (intensity <= BinaryThreshold ? 0 : 255)));
        }

        Store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region IntensityRangeCutCommand

    public Command? IntensityRangeCutCommand { get; }

    private bool IntensityRangeCutCommand_CanExecute(object? parameter) =>
        Picture is not null &&
        CutStart is not null &&
        CutEnd is not null;

    private void IntensityRangeCutCommand_OnExecuted(object? parameter)
    {
        for (var i = 0; i < PictureBytes!.Length; i += 4)
        {
            var intensity = Tools.GetPixelIntensity(PictureBytes, i);
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, i),
                Tools.GetGrayPixel((byte) (CutStart <= intensity && intensity <= CutEnd ? 0 : 255)));
        }

        Store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region BinaryFlatCutCommand

    public Command? BinaryFlatCutCommand { get; }

    private bool BinaryFlatCutCommand_CanExecute(object? parameter) =>
        Picture is not null &&
        BinaryFlat is >= 0 and <= 7;

    private void BinaryFlatCutCommand_OnExecuted(object? parameter)
    {
        for (var i = 0; i < PictureBytes!.Length; i += 4)
        {
            var intensity = Tools.GetPixelIntensity(PictureBytes, i);
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, i),
                Tools.GetGrayPixel((byte) ((((int) intensity >> (int) BinaryFlat!) & 1) * 255)));
        }

        Store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region LogarithmicTransformationCommand

    public Command? LogarithmicTransformationCommand { get; }

    private bool LogarithmicTransformationCommand_CanExecute(object? parameter) => Picture is not null;

    private void LogarithmicTransformationCommand_OnExecuted(object? parameter)
    {
        for (var i = 0; i < PictureBytes!.Length; i += 4)
        {
            var intensity = Tools.GetPixelIntensity(PictureBytes, i);
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, i),
                Tools.GetGrayPixel((byte) (41 * Math.Log(1 + intensity))));
        }

        Store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #region PowerTransformationCommand

    public Command? PowerTransformationCommand { get; }

    private bool PowerTransformationCommand_CanExecute(object? parameter) => Picture is not null;

    private void PowerTransformationCommand_OnExecuted(object? parameter)
    {
        for (var i = 0; i < PictureBytes!.Length; i += 4)
        {
            var intensity = Tools.GetPixelIntensity(PictureBytes, i);
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, i),
                Tools.GetGrayPixel((byte) (Math.Pow(intensity, 2) / 255)));
        }

        Store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #endregion
}