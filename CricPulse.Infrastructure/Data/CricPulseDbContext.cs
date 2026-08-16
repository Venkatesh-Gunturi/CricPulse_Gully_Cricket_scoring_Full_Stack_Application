using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Data;

public class CricPulseDbContext : DbContext
{
    public CricPulseDbContext(DbContextOptions<CricPulseDbContext> options) : base(options)
    {

    }
    public DbSet<User> Users   { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u=>u.Email).IsUnique();

        modelBuilder.Entity<User>().HasIndex(u => u.MobileNumber).IsUnique();

    }
}