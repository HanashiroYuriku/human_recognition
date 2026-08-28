using human_recognition.Application.Common.Interfaces.PersonDetection;
using human_recognition.Domain.Entities;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace human_recognition.Infrastructure.ExternalServices;

public class YoloxDetector : IPersonDetector, IDisposable
{
    // ONNX inference session used to execute the model
    private readonly InferenceSession _session;

    // Standard input width and height expected by the YOLO model
    private readonly int _inputWidth = 640;
    private readonly int _inputHeight = 640;

    public YoloxDetector(string modelPath)
    {
        var options = new SessionOptions();

        // Configure the session to use the CPU execution provider
        options.AppendExecutionProvider_CPU();
        _session = new InferenceSession(modelPath, options);
    }

    public PersonDetectionResult CheckForPerson(string imagePath, string imageDirectory)
    {
        // Initialize the result object with the original image path
        var result = new PersonDetectionResult { OriginalImagePath = imagePath };

        // Load the image from the specified path in color mode (BGR)
        using var image = Cv2.ImRead(imagePath, ImreadModes.Color);
        if (image.Empty()) return result;

        // --- Letterboxing (Image Preprocessing) ---
        // Calculate the scale ratio to resize the image while maintaining its original aspect ratio
        float ratio = Math.Min((float)_inputWidth / image.Width, (float)_inputHeight / image.Height);
        int newWidth = (int)(image.Width * ratio);
        int newHeight = (int)(image.Height * ratio);

        // Resize the original image based on the calculated dimensions
        using var resized = new Mat();
        Cv2.Resize(image, resized, new Size(newWidth, newHeight));

        // Create a padded image canvas with a neutral gray background (114, 114, 114)
        using var padded = new Mat(_inputHeight, _inputWidth, MatType.CV_8UC3, new Scalar(114, 114, 114));

        // Calculate the top and left offsets to center the resized image on the padded canvas
        int top = (_inputHeight - newHeight) / 2;
        int left = (_inputWidth - newWidth) / 2;

        // Copy the resized image into the Region of Interest (ROI) of the padded canvas
        using var roi = new Mat(padded, new Rect(left, top, newWidth, newHeight));
        resized.CopyTo(roi);

        // --- Inference ---
        // Convert the processed OpenCV Mat into an ONNX-compatible tensor
        var inputTensor = ExtractPixels(padded);
        var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("images", inputTensor) };

        // Execute the ONNX model with the prepared input tensor
        using var results = _session.Run(inputs);

        // Retrieve the output tensor containing bounding boxes, scores, and class predictions
        var output = results.First().AsTensor<float>();

        bool personFound = false;
        float highestScore = 0f;

        // The number of predicted bounding boxes is stored in the second dimension of the output tensor
        int numBoxes = output.Dimensions[1];

        var boxes = new List<Rect>();
        var scores = new List<float>();

        // Minimum confidence score required to consider a detection valid
        const float confidenceThreshold = 0.25f;

        // Iterate through all predicted bounding boxes from the model output
        for (int i = 0; i < numBoxes; i++)
        {
            // Extract bounding box spatial coordinates (center X, center Y, width, height)
            float cx = output[0, i, 0];
            float cy = output[0, i, 1];
            float w = output[0, i, 2];
            float h = output[0, i, 3];

            // In YOLOX, the final detection score is the product of the objectness score and the class probability score
            float objScore = output[0, i, 4];
            float clsScore = output[0, i, 5];
            float personScore = objScore * clsScore;

            // Discard detections that fall below the predefined confidence threshold
            if (personScore < confidenceThreshold)
                continue;

            // Scale the coordinates back to the original, unpadded image dimensions
            float originalCx = (cx - left) / ratio;
            float originalCy = (cy - top) / ratio;
            float originalW = w / ratio;
            float originalH = h / ratio;

            // Calculate the top-left X and Y coordinates required for the OpenCV Rect structure
            int x = (int)(originalCx - originalW / 2);
            int y = (int)(originalCy - originalH / 2);

            boxes.Add(new Rect(x, y, (int)originalW, (int)originalH));
            scores.Add(personScore);
        }

        // --- Post-Processing (NMS & Annotation) ---
        // Proceed if at least one valid bounding box was found
        if (boxes.Count > 0)
        {
            // Apply Non-Maximum Suppression (NMS) to eliminate overlapping bounding boxes for the same object
            CvDnn.NMSBoxes(boxes, scores, scoreThreshold: 0.25f, nmsThreshold: 0.50f, out int[] indices);

            // Iterate over the indices kept after NMS filtering
            foreach (int idx in indices)
            {
                personFound = true;
                Rect box = boxes[idx];
                float score = scores[idx];

                // Track the highest confidence score among all confirmed detections
                if (score > highestScore) highestScore = score;

                // Draw a red bounding box and the confidence score text on the original image
                Cv2.Rectangle(image, box, Scalar.Red, 2);
                Cv2.PutText(image, $"{score:F2}", new Point(box.X, box.Y - 5), HersheyFonts.HersheySimplex, 0.6, Scalar.Red, 2);
            }

            // Save the annotated image back to the file system to visually confirm the detection
            if (personFound)
            {
                Cv2.ImWrite(imagePath, image);
            }
        }

        // Populate and return the final detection result
        result.HasPerson = personFound;
        result.ConfidenceScore = highestScore;

        return result;
    }

    private DenseTensor<float> ExtractPixels(Mat image)
    {
        // Create a 4D tensor with shape [batch_size, channels, height, width]
        var tensor = new DenseTensor<float>([1, 3, _inputHeight, _inputWidth]);

        for (int y = 0; y < _inputHeight; y++)
        {
            for (int x = 0; x < _inputWidth; x++)
            {
                var vec3b = image.At<Vec3b>(y, x);

                // YOLOX expects channels in BGR order. 
                // Unlike some standard YOLO models, YOLOX requires raw pixel values (0-255) without normalization (no division by 255.0f).
                tensor[0, 0, y, x] = vec3b.Item0; // Blue channel
                tensor[0, 1, y, x] = vec3b.Item1; // Green channel
                tensor[0, 2, y, x] = vec3b.Item2; // Red channel
            }
        }

        return tensor;
    }

    public void Dispose()
    {
        // Release the ONNX inference session resources to prevent memory leaks
        _session?.Dispose();
    }
}