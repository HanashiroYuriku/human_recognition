using human_recognition.Application.Common.Interfaces.PersonDetection;
using human_recognition.Domain.Entities;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace human_recognition.Infrastructure.ExternalServices;

public class YoloxDetector : IPersonDetector, IDisposable
{
    private readonly InferenceSession _session;
    private readonly int _inputWidth = 640;  // Standar YOLO11
    private readonly int _inputHeight = 640; // Standar YOLO11

    public YoloxDetector(string modelPath)
    {
        var options = new SessionOptions();
        options.AppendExecutionProvider_CPU();
        _session = new InferenceSession(modelPath, options);
    }

    public PersonDetectionResult CheckForPerson(string imagePath, string imageDirectory)
    {
        var result = new PersonDetectionResult { OriginalImagePath = imagePath };

        using var image = Cv2.ImRead(imagePath, ImreadModes.Color);
        if (image.Empty()) return result;

        // --- START CLAHE (DIMATIKAN SEMENTARA) ---
        // Kita bypass CLAHE agar YOLO11 membaca gambar murni.
        // --- END CLAHE ---

        // --- START LETTERBOXING ---
        float ratio = Math.Min((float)_inputWidth / image.Width, (float)_inputHeight / image.Height);
        int newWidth = (int)(image.Width * ratio);
        int newHeight = (int)(image.Height * ratio);

        using var resized = new Mat();
        Cv2.Resize(image, resized, new Size(newWidth, newHeight));

        using var padded = new Mat(_inputHeight, _inputWidth, MatType.CV_8UC3, new Scalar(114, 114, 114));
        int top = (_inputHeight - newHeight) / 2;
        int left = (_inputWidth - newWidth) / 2;

        using var roi = new Mat(padded, new Rect(left, top, newWidth, newHeight));
        resized.CopyTo(roi);
        // --- END LETTERBOXING ---

        // --- START INFERENCE ---
        var inputTensor = ExtractPixels(padded);
        var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("images", inputTensor) };

        using var results = _session.Run(inputs);
        var output = results.First().AsTensor<float>();

        bool personFound = false;
        float highestScore = 0f;

        int numBoxes = output.Dimensions[2];

var boxes = new List<Rect>();
var scores = new List<float>();

const float confidenceThreshold = 0.20f;

for (int i = 0; i < numBoxes; i++)
{
    float cx = output[0, 0, i];
    float cy = output[0, 1, i];
    float w  = output[0, 2, i];
    float h  = output[0, 3, i];

    // YOLO11 COCO:
    // class 0 = person
    float personScore = output[0, 4, i];

    if (personScore < confidenceThreshold)
        continue;

    float originalCx = (cx - left) / ratio;
    float originalCy = (cy - top) / ratio;
    float originalW  = w / ratio;
    float originalH  = h / ratio;

    int x = (int)(originalCx - originalW / 2);
    int y = (int)(originalCy - originalH / 2);

    int width  = (int)originalW;
    int height = (int)originalH;

    var box = new Rect(
        x,
        y,
        width,
        height
    );

    boxes.Add(box);
    scores.Add(personScore);
}

        // --- NMS & PENGGAMBARAN KOTAK ---
        if (boxes.Count > 0)
        {
            // NMS Threshold dinaikkan sedikit agar kotak yang berhimpitan tidak hilang salah satu
            CvDnn.NMSBoxes(boxes, scores, scoreThreshold: 0.25f, nmsThreshold: 0.50f, out int[] indices);

            foreach (int idx in indices)
            {
                personFound = true;
                Rect box = boxes[idx];
                float score = scores[idx];

                if (score > highestScore) highestScore = score;

                Cv2.Rectangle(image, box, Scalar.Red, 2);
                Cv2.PutText(image, $"{score:F2}", new Point(box.X, box.Y - 5), HersheyFonts.HersheySimplex, 0.6, Scalar.Red, 2);
            }

            if (personFound)
            {
                Cv2.ImWrite(imagePath, image);
            }
        }

        result.HasPerson = personFound;
        result.ConfidenceScore = highestScore;

        return result;
    }

    private DenseTensor<float> ExtractPixels(Mat image)
    {
        var tensor = new DenseTensor<float>([1, 3, _inputHeight, _inputWidth]);

        for (int y = 0; y < _inputHeight; y++)
        {
            for (int x = 0; x < _inputWidth; x++)
            {
                var vec3b = image.At<Vec3b>(y, x);

                // --- PERUBAHAN FATAL UNTUK YOLO11 ---
                // 1. Format harus RGB (bukan BGR seperti YOLOX)
                // 2. Nilai WAJIB dinormalisasi (dibagi 255.0f)
                tensor[0, 0, y, x] = vec3b.Item2 / 255.0f; // R
                tensor[0, 1, y, x] = vec3b.Item1 / 255.0f; // G
                tensor[0, 2, y, x] = vec3b.Item0 / 255.0f; // B
            }
        }
        return tensor;
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}