using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace CricPulse.Infrastructure.Repositories;
// Manages database transactions for operations that require atomic execution.
public class TransactionManager : ITransactionManager
{
    private readonly CricPulseDbContext _context;
    private IDbContextTransaction? _transaction;
    // Initializes the transaction manager with the shared database context.
    public TransactionManager(CricPulseDbContext context)
    {
        _context = context;
    }

    // Starts a new database transaction.
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    // Commits the active transaction and releases its database resources.
    public async Task CommitTransactionAsync()
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("No active transaction exists.");
        }

        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }
    // Rolls back the active transaction and releases its database resources.
    public async Task RollbackTransactionAsync()
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}