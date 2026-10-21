using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Data;

public class CricPulseDbContext : DbContext
{
    public CricPulseDbContext(DbContextOptions<CricPulseDbContext> options)
        : base(options)
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CricPulseDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}