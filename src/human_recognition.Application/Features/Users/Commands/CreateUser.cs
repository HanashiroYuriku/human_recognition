using human_recognition.Application.Common.Interfaces.Authentication;
using human_recognition.Application.Common.Interfaces.Repositories;
using human_recognition.Application.Common.Models;
using human_recognition.Domain.Entities;
using human_recognition.Domain.Enums;
using Cortex.Mediator.Commands;
using FluentValidation;

namespace human_recognition.Application.Features.Users.Commands;

// Command
public record CreateUserCommand(
    string FullName,
    string Username,
    string Email,
    string Password,
    string Role
) : ICommand<Guid>;


// Validator field
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUserRepository userRepository)
    {
        RuleFor(v => v.FullName)
            .NotEmpty().WithMessage("Full name is Required")
            .MaximumLength(100).WithMessage("Maximum 100 Characters");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is Required")
            .EmailAddress().WithMessage("Your Email Not Valid")
            .MustAsync(async (email, ct) =>
            {
                var existingEmail = await userRepository.GetByEmailAsync(email, ct);
                return existingEmail == null;
            }).WithMessage("Email Already Used");

        RuleFor(v => v.Username)
            .NotEmpty().WithMessage("Username is Required")
            .MinimumLength(3).WithMessage("Username Minimum 3 Characters.")
            .MaximumLength(50).WithMessage("Username Maximum 50 Characters.")
            .Matches(@"^[a-z._]+$").WithMessage("Username can only contains lower case, underscore, and dot")
            .MustAsync(async (username, ct) =>
            {
                var existingUsername = await userRepository.GetByUsernameAsync(username, ct);
                return existingUsername == null;
            }).WithMessage("Username Already Used");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is Required")
            .MinimumLength(8).WithMessage("Password Minimum 8 Characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(v => v.Role)
            .NotEmpty().WithMessage("Role is Required")
            .IsEnumName(typeof(Role), caseSensitive: false)
            .WithMessage("Role Invalid");
    }
}

// Handler
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDbTransactionManager _txManager;

    public CreateUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IDbTransactionManager txManager)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _txManager = txManager;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Hashing user's password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Make new user's object
        var newUser = new User(
            request.FullName,
            request.Username,
            request.Email,
            passwordHash,
            Enum.Parse<Role>(request.Role, ignoreCase: true)
        );

        // Add user
        _userRepository.Add(newUser);
        // Save user
        await _txManager.SaveChangesAsync(cancellationToken);

        return newUser.Id;
    }
}