namespace human_recognition.Domain.Entities;

public class PersonDetectionResult
{
    public bool HasPerson { get; set; }
    public string OriginalImagePath { get; set; } = string.Empty;
    public string ProcessedImagePath { get; set; } = string.Empty;
    public float ConfidenceScore { get; set; }
}