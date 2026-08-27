using human_recognition.Domain.Entities;

namespace human_recognition.Application.Common.Interfaces.PersonDetection;

public interface IPersonDetector
{
    PersonDetectionResult CheckForPerson(string imagePath, string imageDirectory);
}