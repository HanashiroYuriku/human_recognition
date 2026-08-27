using human_recognition.Application.Common.Interfaces.Repositories;
using human_recognition.Application.Common.Models;
using Cortex.Mediator.Queries;

namespace human_recognition.Application.Features.Users.Queries;

public record UserResponse(
    Guid Id,
    string FullName,
    string Username,
    string Email,
    string Role
);

public record GetAllUsersQuery : PaginationParams, IQuery<PagedList<UserResponse>>;

public class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, PagedList<UserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedList<UserResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var usersPagedList = await _userRepository.GetAllAsync(request, cancellationToken, trackChanges: false);

        var userResponses = usersPagedList.Items.Select(user => new UserResponse(
            user.Id,
            user.FullName,
            user.Username,
            user.Email,
            user.Role.ToString()
        )).ToList();

        return new PagedList<UserResponse>(
            items: userResponses,
            pageNumber: usersPagedList.PageNumber,
            pageSize: usersPagedList.PageSize,
            totalCount: usersPagedList.TotalCount);
    }
}