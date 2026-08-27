using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using human_recognition.Application.Common.Interfaces.Authentication;
using human_recognition.Application.Features.Auth.Commands;
using human_recognition.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace human_recognition.Infrastructure.Authentication;

// JWT Settings Class
public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpiryMinutes { get; set; }
    public int RefreshTokenExpiryDays { get; set; }
}

// JWT Generator Class
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    // Generate access and refresh JW Token
    public AuthResult GenerateToken(User user)
    {
        // Registered JWT Claims
        // Claims include: user id, user email, user name, and user role
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Encode Secret Key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

        // Create credentials
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // handler
        var handler = new JsonWebTokenHandler();

        // Create token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            SigningCredentials = credentials
        };

        // Create access token
        var accessToken = handler.CreateToken(tokenDescriptor);

        // Create Refresh Token
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        return new AuthResult(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            RefreshTokenExpiryDays: _jwtSettings.RefreshTokenExpiryDays
        );
    }
}