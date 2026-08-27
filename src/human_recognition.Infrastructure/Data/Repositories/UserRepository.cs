using human_recognition.Application.Common.Interfaces.Repositories;
using human_recognition.Application.Common.Models;
using human_recognition.Domain.Entities;
using human_recognition.Infrastructure.Data.Extensions;
using Microsoft.EntityFrameworkCore;

namespace human_recognition.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Add user to database
    public void Add(User user)
    {
        _context.Users.Add(user);
    }

    // Get user by user's Id
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool trackChanges = false)
    {
        var query = _context.Users.AsQueryable();

        // Set as no tracking if trackChanges value is false (use true if you need change tracking)
        if (!trackChanges) query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    // Get user by user's email
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken, bool trackChanges = false)
    {
        var query = _context.Users.AsQueryable();

        // Set as no tracking if trackChanges value is false (use true if you need change tracking)
        if (!trackChanges) query = query.AsNoTracking();

        return await query
        .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    // Get user by user's username
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken, bool trackChanges = false)
    {
        var query = _context.Users.AsQueryable();

        // Set as no tracking if trackChanges value is false (use true if you need change tracking)
        if (!trackChanges) query = query.AsNoTracking();

        return await query
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    // Get user by user's refresh token
    public async Task<User?> GetUserByRefreshToken(string refreshToken, CancellationToken cancellationToken, bool trackChanges = false)
    {
        var query = _context.Users.AsQueryable();

        // Set as no tracking if trackChanges value is false (use true if you need change tracking)
        if (!trackChanges) query = query.AsNoTracking();

        return await query
            .FirstOrDefaultAsync(u =>
                u.RefreshToken == refreshToken,
                cancellationToken);
    }

    // Get all user with limit and offset using pagination
    public async Task<PagedList<User>> GetAllAsync(PaginationParams paginationParams, CancellationToken cancellationToken, bool trackChanges = false)
    {
        var query = _context.Users.AsQueryable();

        // Set as no tracking if trackChanges value is false (use true if you need change tracking)
        if (!trackChanges) query = query.AsNoTracking();

        query = query.OrderByDescending(u => u.CreatedAt); // Order by username to have consistent pagination results

        return await query.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize, cancellationToken);
    }

    // Update user's edited data
    public void Update(User user)
    {
        // check if any data edited
        var trackEntity = _context.ChangeTracker.Entries<User>()
            .FirstOrDefault(e => e.Entity.Id == user.Id);

        if (trackEntity == null) _context.Users.Update(user);
    }

    // Delete user from database (this isn't soft delete)
    public void Delete(User user)
    {
        _context.Users.Remove(user);
    }
}