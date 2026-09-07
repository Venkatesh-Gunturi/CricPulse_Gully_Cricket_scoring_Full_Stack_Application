using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace CricPulse.Infrastructure.Data;

public class CricPulseDbContext : DbContext
{
    public CricPulseDbContext(DbContextOptions<CricPulseDbContext> options) : base(options)
    {

    }
    public DbSet<User> Users   { get; set; }
    public DbSet<OtpVerification> OtpVerification { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Match> Matches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u=>u.Email).IsUnique();

        modelBuilder.Entity<User>().HasIndex(u => u.MobileNumber).IsUnique();

        modelBuilder.Entity<OtpVerification>()
                    .HasOne(o => o.User)
                    .WithMany(u => u.OtpVerifications)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Player>()
            .HasOne(p => p.User)
            .WithOne(u => u.Player)
            .HasForeignKey<Player>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Match>()
            .HasOne(m => m.Umpire)
            .WithMany(u => u.MatchesAsUmpire)
            .HasForeignKey(m => m.UmpireId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}