using System;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;
using OpenCvSharp;

namespace Laboratory_work_1.ViewModels;

public class ColorSegmentationViewModel : ViewModel
{
    #region Fields

    private int? _numberOfColors;
    
    public int? NumberOfColors
    {
        get => _numberOfColors;
        set => Set(ref _numberOfColors, value);
    }
    
    #endregion

    public ColorSegmentationViewModel()
    {
    }

    public ColorSegmentationViewModel(ViewModelStore store) : base(store)
    {
        KmeansCommand = new Command(
            KmeansCommand_OnExecuted,
            KmeansCommand_CanExecute);
    }

    #region Commands

    #region KmeansCommand

    public Command? KmeansCommand { get; }

    private bool KmeansCommand_CanExecute(object? parameter) =>
        !PictureSize.IsEmpty;
    
    private void KmeansCommand_OnExecuted(object? parameter)
    {
        var mat = Mat.FromArray(PictureBytes!);
        var matResult = mat.Clone();
        Store?.TriggerPictureBytesEvent(PictureBytes!, PictureSize);
        int colors = (int) NumberOfColors!;

        using Mat points = new Mat();
        using Mat labels = new Mat();
        using (Mat centers = new Mat())
        {
            int width = mat.Cols;
            int height = mat.Rows;

            points.Create(width * height, 1, MatType.CV_32FC3);
            centers.Create(colors, 1, points.Type());
            matResult.Create(height, width, matResult.Type());

            // Input Image Data
            int i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++, i++)
                {
                    Vec3f vec3F = new Vec3f
                    {
                        Item0 = mat.At<Vec3b>(y, x).Item0,
                        Item1 = mat.At<Vec3b>(y, x).Item1,
                        Item2 = mat.At<Vec3b>(y, x).Item2
                    };

                    points.Set(i, vec3F);
                }
            }

            // Criteria:
            // – Stop the algorithm iteration if specified accuracy, epsilon, is reached.
            // – Stop the algorithm after the specified number of iterations, MaxIter.
            var criteria = new TermCriteria(type: CriteriaTypes.MaxIter, maxCount: 10, epsilon: 1.0);

            // Finds centers of clusters and groups input samples around the clusters.
            Cv2.Kmeans(data: points, k: colors, bestLabels: labels, criteria: criteria, attempts: 3, flags: KMeansFlags.PpCenters, centers: centers);

            // Output Image Data
            i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++, i++)
                {
                    int index = labels.Get<int>(i);

                    Vec3b vec3B = new Vec3b();

                    int firstComponent = Convert.ToInt32(Math.Round(centers.At<Vec3f>(index).Item0));
                    firstComponent = firstComponent > 255 ? 255 : firstComponent < 0 ? 0 : firstComponent;
                    vec3B.Item0 = Convert.ToByte(firstComponent);

                    int secondComponent = Convert.ToInt32(Math.Round(centers.At<Vec3f>(index).Item1));
                    secondComponent = secondComponent > 255 ? 255 : secondComponent < 0 ? 0 : secondComponent;
                    vec3B.Item1 = Convert.ToByte(secondComponent);

                    int thirdComponent = Convert.ToInt32(Math.Round(centers.At<Vec3f>(index).Item2));
                    thirdComponent = thirdComponent > 255 ? 255 : thirdComponent < 0 ? 0 : thirdComponent;
                    vec3B.Item2 = Convert.ToByte(thirdComponent);

                    matResult.Set(y, x, vec3B);
                }
            }
        }
        var imageBytes = matResult.ToArray();
        for (var i = 0; i < PictureBytes!.Length; i += 4)
        {
            Tools.SetPixel(
                Tools.GetPixel(PictureBytes, i),
                Tools.GetPixel(imageBytes, i));
        }
    }

    #endregion

    #endregion
}