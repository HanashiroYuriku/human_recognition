using human_recognition.Application.Common.Interfaces.Repositories;
using human_recognition.Domain.Exceptions;
using Cortex.Mediator.Queries;

namespace human_recognition.Application.Features.Users.Queries;

// DTO - Represent Response
public record GetUserByIdResponse(
    Guid Id,
    string FullName,
    string Username,
    string Email,
    string Role
);

// Query
// IRequest indicates this is a message for Cortex Mediator
// ApiResponse<GetUserByIdResponse> is the expected response
public record GetUserByIdQuery(Guid Id) : IQuery<GetUserByIdResponse>;

// Handler
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, GetUserByIdResponse>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetUserByIdResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // Find user by id. trackChanges false for read only data
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken, trackChanges: false)
            ?? throw new NotFoundException($"User with ID {request.Id} not found.");

        var userResponse = new GetUserByIdResponse(
            user.Id,
            user.FullName,
            user.Username,
            user.Email,
            user.Role.ToString()
        );

        return userResponse;
    }
}