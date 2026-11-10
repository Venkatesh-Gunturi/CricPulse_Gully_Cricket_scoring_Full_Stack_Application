
using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Data;

public class CricPulseDbContext : DbContext
{
    public CricPulseDbContext(DbContextOptions<CricPulseDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchPlayer> MatchPlayers => Set<MatchPlayer>();
    public DbSet<Innings> Innings => Set<Innings>();
    public DbSet<Ball> Balls => Set<Ball>();
    public DbSet<Wicket> Wickets => Set<Wicket>();
    public DbSet<PlayerStatistics> PlayerStatistics => Set<PlayerStatistics>();
    public DbSet<OrganizerStatistics> OrganizerStatistics => Set<OrganizerStatistics>();

    // Updates match timestamps before saving tracked changes to the database.
    public override int SaveChanges()
    {
        UpdateMatchTimestamps();
        return base.SaveChanges();
    }
    // Updates match timestamps before saving tracked changes to the database.
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateMatchTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }
    // Automatically maintains CreatedAt and UpdatedAt timestamps
    // for Match entities based on their Entity Framework tracking state.
    private void UpdateMatchTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Match>())
        {
            // Set creation and update timestamps for newly added matches.
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                    entry.Entity.CreatedAt = now;

                entry.Entity.UpdatedAt = now;
            }
            // Refresh the update timestamp whenever an existing match changes.
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }

    // Applies entity configurations defined in this assembly,
    // including relationships, keys, constraints, and property mappings.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CricPulseDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
