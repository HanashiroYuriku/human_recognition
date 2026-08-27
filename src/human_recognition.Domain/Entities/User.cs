using human_recognition.Domain.Common;
using human_recognition.Domain.Enums;

namespace human_recognition.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; private set; } = null!;
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public Role Role { get; private set; }

    // Auth
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }

    // Empty Constructor for Entity Framework Core
    protected User() { }

    // Constructor for create new User
    public User(string fullName, string username, string email, string passwordHash, Role role)
    {
        FullName = fullName;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    // Update full name and email user
    public void UpdateProfile(string fullName, string email)
    {
        FullName = fullName;
        Email = email;
    }

    // Update password user
    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    // Update refresh token and expiry 
    public void UpdateRefreshToken(string token, DateTime expTime)
    {
        RefreshToken = token;
        RefreshTokenExpiryTime = expTime;
    }

    // Delete refresh token user (logout case)
    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
    }
}