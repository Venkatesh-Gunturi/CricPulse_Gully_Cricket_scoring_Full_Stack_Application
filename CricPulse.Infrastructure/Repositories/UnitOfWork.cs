using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Infrastructure.Data;

namespace CricPulse.Infrastructure.Repositories;
// Provides a centralized way to persist repository changes to the database.
public class UnitOfWork : IUnitOfWork
{
    private readonly CricPulseDbContext _context;
    // Initializes the Unit of Work with the shared database context.
    public UnitOfWork(CricPulseDbContext context)
    {
        _context = context;
    }
    // Persists all tracked changes in the current database context.
    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}