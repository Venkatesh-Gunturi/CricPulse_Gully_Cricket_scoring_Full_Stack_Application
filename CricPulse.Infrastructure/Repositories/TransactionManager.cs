using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace CricPulse.Infrastructure.Repositories;

public class TransactionManager : ITransactionManager
{
    private readonly CricPulseDbContext _context;
    private IDbContextTransaction? _transaction;

    public TransactionManager(CricPulseDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

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