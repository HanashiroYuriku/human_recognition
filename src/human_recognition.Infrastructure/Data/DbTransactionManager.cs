using human_recognition.Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace human_recognition.Infrastructure.Data;

public class DbTransactionManager : IDbTransactionManager, IDisposable
{
    private readonly ApplicationDbContext _ctx;
    private IDbContextTransaction? _tx;

    public DbTransactionManager(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    // Begin transaction
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // if transaction already created
        if (_tx != null) return;

        _tx = await _ctx.Database.BeginTransactionAsync(cancellationToken);
    }

    // Commit transaction
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        // if no transaction running
        if (_tx == null) throw new InvalidOperationException("No Transaction Running");

        try
        {
            await _tx.CommitAsync(cancellationToken);
        }
        finally
        {
            await _tx.DisposeAsync();
            _tx = null;
        }
    }

    // Rollback transaction
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        // if no transaction running
        if (_tx == null) return;

        try
        {
            await _tx.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _tx.DisposeAsync();
            _tx = null;
        }
    }

    // Save changes / Save to database
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _ctx.SaveChangesAsync(cancellationToken);
    }

    // Clear transaction and context
    public void Dispose()
    {
        // clear transaction (?)
        _tx?.Dispose();
    }
}