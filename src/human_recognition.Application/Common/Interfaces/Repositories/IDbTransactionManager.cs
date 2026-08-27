namespace human_recognition.Application.Common.Interfaces.Repositories;

// Database Transaction Manager Interface
public interface IDbTransactionManager
{
    // Tx Save Changes
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    // Tx Begin Transaction
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    // Tx Commit
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    // Tx Rollback
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}