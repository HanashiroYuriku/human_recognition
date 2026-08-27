using Cortex.Mediator.Commands;
using FluentValidation;
using human_recognition.Application.Common.Interfaces.PersonDetection;

namespace human_recognition.Application.Features.Cctv.Commands;

// 1. DTO Hasil Batch
public record YoloxResult(
    int TotalProcessed,
    int TotalPersonFound,
    int TotalNonPerson
);

// 2. Command (Menerima input folder, bukan file)
public record YoloxCommand(string SourceDirectory) : ICommand<YoloxResult>;

// 3. Validator
public class YoloxCommandValidator : AbstractValidator<YoloxCommand>
{
    public YoloxCommandValidator()
    {
        RuleFor(v => v.SourceDirectory)
            .NotEmpty().WithMessage("Source Directory Required")
            .Must(Directory.Exists).WithMessage("Source Directory Does Not Exist at The Specified Path");
    }
}

// 4. Command Handler
public class YoloxHandler : ICommandHandler<YoloxCommand, YoloxResult>
{
    private readonly IPersonDetector _personDetector;
    private readonly string _folderPerson = "D:\\Kantor\\Project\\Temp\\human_recognition\\image\\Human Detected";
    private readonly string _folderNonPerson = "D:\\Kantor\\Project\\Temp\\human_recognition\\image\\Human Not Detected";

    public YoloxHandler(IPersonDetector personDetector)
    {
        _personDetector = personDetector;

        Directory.CreateDirectory(_folderPerson);
        Directory.CreateDirectory(_folderNonPerson);
    }

    public async Task<YoloxResult> Handle(YoloxCommand command, CancellationToken cancellationToken)
    {
        int personCount = 0;
        int nonPersonCount = 0;

        // Ambil semua file gambar dari folder sumber
        var files = Directory.GetFiles(command.SourceDirectory, "*.*")
            .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                        f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                        f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Lakukan iterasi untuk setiap foto
        foreach (var file in files)
        {
            // Mencegah memory leak jika request dibatalkan di tengah jalan
            cancellationToken.ThrowIfCancellationRequested();

            var detectionResult = _personDetector.CheckForPerson(file, _folderPerson);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
            string ext = Path.GetExtension(file);

            if (!detectionResult.HasPerson)
            {
                string newFileName = $"{fileNameWithoutExt}_conf_{detectionResult.ConfidenceScore:F2}{ext}";
                string finalSavedPath = Path.Combine(_folderNonPerson, newFileName);

                if (File.Exists(finalSavedPath))
                    File.Delete(finalSavedPath);

                File.Move(file, finalSavedPath);
                nonPersonCount++;
            }
            else
            {
                // Format nama baru: namaAsli_conf_0.85.jpg
                string newFileName = $"{fileNameWithoutExt}_conf_{detectionResult.ConfidenceScore:F2}{ext}";
                string finalSavedPath = Path.Combine(_folderPerson, newFileName);

                if (File.Exists(finalSavedPath))
                    File.Delete(finalSavedPath);

                // Pindahkan file asli ke folder target dengan nama baru
                File.Move(file, finalSavedPath);
                personCount++;
            }
        }

        return new YoloxResult(
            TotalProcessed: files.Count,
            TotalPersonFound: personCount,
            TotalNonPerson: nonPersonCount
        );
    }
}