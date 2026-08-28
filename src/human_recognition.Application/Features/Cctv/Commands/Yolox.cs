using Cortex.Mediator.Commands;
using FluentValidation;
using human_recognition.Application.Common.Interfaces.PersonDetection;

namespace human_recognition.Application.Features.Cctv.Commands;

// Data Transfer Object (DTO) representing the summary results of the batch processing operation.
public record YoloxResult(
    int TotalProcessed,
    int TotalPersonFound,
    int TotalNonPerson
);

// Command object that triggers the YOLOX detection process. 
// It takes a target directory path (batch processing) rather than a single file.
public record YoloxCommand(string SourceDirectory) : ICommand<YoloxResult>;

// Validator for the YoloxCommand to ensure the input data is valid before execution begins.
public class YoloxCommandValidator : AbstractValidator<YoloxCommand>
{
    public YoloxCommandValidator()
    {
        RuleFor(v => v.SourceDirectory)
            .NotEmpty().WithMessage("Source Directory Required")
            .Must(Directory.Exists).WithMessage("Source Directory Does Not Exist at The Specified Path");
    }
}

// Handler class responsible for executing the business logic associated with the YoloxCommand.
public class YoloxHandler : ICommandHandler<YoloxCommand, YoloxResult>
{
    private readonly IPersonDetector _personDetector;

    // Hardcoded destination paths for categorizing processed images
    private readonly string _folderPerson = "D:\\Kantor\\Project\\Temp\\human_recognition\\image\\Human Detected";
    private readonly string _folderNonPerson = "D:\\Kantor\\Project\\Temp\\human_recognition\\image\\Human Not Detected";

    public YoloxHandler(IPersonDetector personDetector)
    {
        _personDetector = personDetector;

        // Ensure the destination directories exist before processing begins
        Directory.CreateDirectory(_folderPerson);
        Directory.CreateDirectory(_folderNonPerson);
    }

    public async Task<YoloxResult> Handle(YoloxCommand command, CancellationToken cancellationToken)
    {
        int personCount = 0;
        int nonPersonCount = 0;

        // Retrieve all valid image files (JPG, JPEG, PNG) from the specified source directory
        var files = Directory.GetFiles(command.SourceDirectory, "*.*")
            .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                        f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                        f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Iterate through each image file found in the directory
        foreach (var file in files)
        {
            // Throw an exception if the operation is cancelled midway to safely abort and prevent memory leaks
            cancellationToken.ThrowIfCancellationRequested();

            // Execute the person detection model on the current image
            var detectionResult = _personDetector.CheckForPerson(file, _folderPerson);

            // Extract file metadata to prepare for renaming and moving
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
            string ext = Path.GetExtension(file);

            if (!detectionResult.HasPerson)
            {
                // Construct a new file name appending the confidence score (e.g., originalName_conf_0.12.jpg)
                string newFileName = $"{fileNameWithoutExt}_conf_{detectionResult.ConfidenceScore:F2}{ext}";
                string finalSavedPath = Path.Combine(_folderNonPerson, newFileName);

                // Overwrite the file if it already exists in the destination folder
                if (File.Exists(finalSavedPath))
                    File.Delete(finalSavedPath);

                // Move the image from the source directory to the 'Human Not Detected' folder
                File.Move(file, finalSavedPath);
                nonPersonCount++;
            }
            else
            {
                // Construct a new file name appending the confidence score (e.g., originalName_conf_0.85.jpg)
                string newFileName = $"{fileNameWithoutExt}_conf_{detectionResult.ConfidenceScore:F2}{ext}";
                string finalSavedPath = Path.Combine(_folderPerson, newFileName);

                // Overwrite the file if it already exists in the destination folder
                if (File.Exists(finalSavedPath))
                    File.Delete(finalSavedPath);

                // Move the original image to the 'Human Detected' folder with its new file name
                File.Move(file, finalSavedPath);
                personCount++;
            }
        }

        // Return the final summary record containing processing statistics
        return new YoloxResult(
            TotalProcessed: files.Count,
            TotalPersonFound: personCount,
            TotalNonPerson: nonPersonCount
        );
    }
}