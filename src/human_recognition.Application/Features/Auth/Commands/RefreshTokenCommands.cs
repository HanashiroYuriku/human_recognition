using human_recognition.Application.Common.Interfaces.Authentication;
using human_recognition.Application.Common.Interfaces.Repositories;
using human_recognition.Domain.Exceptions;
using Cortex.Mediator.Commands;
using FluentValidation;

namespace human_recognition.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : ICommand<AuthResult>;

public class RequestTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RequestTokenValidator()
    {
        RuleFor(v => v.RefreshToken)
            .NotEmpty().WithMessage("Refresh Token is Empty");
    }
}


public class RefreshTokenHandler : ICommandHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IDbTransactionManager _txManager;

    public RefreshTokenHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, IDbTransactionManager txManager)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _txManager = txManager;
    }

    public async Task<AuthResult> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        // Find user by refresh token. Throw / return Unauthorized exeption if user not found
        var user = await _userRepository.GetUserByRefreshToken(command.RefreshToken, cancellationToken, trackChanges: true)
            ?? throw new UnauthorizedException("Invalid Refresh Token");

        // Check token not expired yet
        if (DateTime.UtcNow > user.RefreshTokenExpiryTime)
        {
            throw new UnauthorizedException("Expired Refresh Token");
        }

        // Generate token
        var authResult = _jwtTokenGenerator.GenerateToken(user);

        // Set user refresh token
        user.UpdateRefreshToken(
            token: authResult.RefreshToken,
            expTime: DateTime.UtcNow.AddDays(authResult.RefreshTokenExpiryDays)
        );

        // Update user's refresh token
        _userRepository.Update(user);
        // Save changes
        await _txManager.SaveChangesAsync(cancellationToken);

        return authResult;
    }
}