using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class ChromacityViewModel : ViewModel
{
    #region Fields

    private readonly ViewModelStore? _store;
    private BitmapSource? _picture;
    private byte[]? _pictureBytes;
    private Visibility _visibility = Visibility.Collapsed;

    private byte? _binaryThreshold;
    private byte? _cutStart;
    private byte? _cutEnd;
    private int? _binaryFlat;

    private BitmapSource? Picture
    {
        get => _picture;
        set => Set(ref _picture, value);
    }

    private byte[]? PictureBytes
    {
        get => _pictureBytes;
        set => Set(ref _pictureBytes, value);
    }

    public Visibility Visibility
    {
        get => _visibility;
        set => Set(ref _visibility, value);
    }

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

    public ChromacityViewModel(ViewModelStore? store)
    {
        if (store is null) return;

        store.PictureChanged += Picture_OnChanged;
        store.PictureBytesChanged += PictureBytes_OnChanged;
        _store = store;

        ChromacityCommand = new Command(
            ChromacityCommand_OnExecuted,
            ChromacityCommand_CanExecute);
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

    #region Event Subscription

    private void Picture_OnChanged(BitmapSource? source)
    {
        Picture = source;
    }

    private void PictureBytes_OnChanged(byte[] bytes)
    {
        PictureBytes = bytes;
    }

    #endregion

    #region Commands

    #region ChromacityCommand

    public Command? ChromacityCommand { get; }

    private bool ChromacityCommand_CanExecute(object? parameter) => Picture is not null;

    private void ChromacityCommand_OnExecuted(object? parameter)
    {
        Visibility = Visibility is Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    #endregion

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

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
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

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
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

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
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

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
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

        _store?.TriggerPictureBytesEvent(Picture!, PictureBytes!);
    }

    #endregion

    #endregion
}