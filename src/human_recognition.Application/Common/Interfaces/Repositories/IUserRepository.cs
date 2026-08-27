using human_recognition.Application.Common.Models;
using human_recognition.Domain.Entities;

namespace human_recognition.Application.Common.Interfaces.Repositories;

// User Repository Interface / Contract
public interface IUserRepository
{
    // Get User by ID
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool trackChanges = false);

    // Get User by Email
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken, bool trackChanges = false);

    // Get User by Username
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken, bool trackChanges = false);

    // Get User by Refresh 
    Task<User?> GetUserByRefreshToken(string refreshToken, CancellationToken cancellationToken, bool trackChanges = false);

    // Get All Users
    Task<PagedList<User>> GetAllAsync(PaginationParams paginationParams, CancellationToken cancellationToken, bool trackChanges = false); // will update using pagination

    // Add new User
    void Add(User user);

    // Update a spesific User
    void Update(User user);

    // Delete a User
    void Delete(User user);
}