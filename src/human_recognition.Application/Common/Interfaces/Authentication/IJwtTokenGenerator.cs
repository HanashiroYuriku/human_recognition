using human_recognition.Application.Features.Auth.Commands;
using human_recognition.Domain.Entities;

namespace human_recognition.Application.Common.Interfaces.Authentication;

public interface IJwtTokenGenerator
{
    // Generate token interface
    AuthResult GenerateToken(User user);
}