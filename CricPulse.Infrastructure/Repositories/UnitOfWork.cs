using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Infrastructure.Data;

namespace CricPulse.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CricPulseDbContext _context;

    public UnitOfWork(CricPulseDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}