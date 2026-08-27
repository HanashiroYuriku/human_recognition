using human_recognition.Application.Common.Interfaces.Authentication;
using human_recognition.Application.Common.Interfaces.Repositories;
using human_recognition.Domain.Exceptions;
using Cortex.Mediator.Commands;
using FluentValidation;

namespace human_recognition.Application.Features.Auth.Commands;

// DTO
public record AuthResult(
    string AccessToken,
    string RefreshToken,
    int RefreshTokenExpiryDays
);

// Command
public record LoginCommand(string Email, string Password) : ICommand<AuthResult>;

// Validator field
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is Required");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is Required");
    }
}

public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IDbTransactionManager _dbTxManager;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IDbTransactionManager dbTxManager
    )
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _dbTxManager = dbTxManager;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken, trackChanges: true);

        // check email valid and password correct
        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Email or Password Invalid");
        }

        // generate JWT (access and refresh token)
        var authResult = _jwtTokenGenerator.GenerateToken(user);

        // set refresh token lifetime
        var refreshTokenExipryTime = DateTime.UtcNow.AddDays(authResult.RefreshTokenExpiryDays);

        // update refresh token and lifetime on user
        user.UpdateRefreshToken(authResult.RefreshToken, refreshTokenExipryTime);

        // update user
        _userRepository.Update(user);
        // save changes updated data
        await _dbTxManager.SaveChangesAsync(cancellationToken);

        // return access and refresh token and refresh token expiry
        return authResult;
    }
}