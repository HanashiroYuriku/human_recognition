namespace human_recognition.Application.Common.Interfaces.Authentication;

public interface IPasswordHasher
{
    // Hashing password interface
    string HashPassword(string password);
    // Compare password interface
    bool VerifyPassword(string password, string passwordHash);
}