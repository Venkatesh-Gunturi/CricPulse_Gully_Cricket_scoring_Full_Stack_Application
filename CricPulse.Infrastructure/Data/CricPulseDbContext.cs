using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace CricPulse.Infrastructure.Data;

public class CricPulseDbContext : DbContext
{
    public CricPulseDbContext(DbContextOptions<CricPulseDbContext> options) : base(options)
    {

    }
    public DbSet<User> Users   { get; set; }
    public DbSet<PendingRegistration> PendingRegistrations { get; set; }
    public DbSet<OtpVerification> OtpVerification { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<MatchPlayer> MatchPlayers { get; set; }
    public DbSet<Innings> Innings { get; set; }
    public DbSet<Ball> Balls { get; set; }
    public DbSet<Wicket> Wickets { get; set; }

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

        modelBuilder.Entity<MatchPlayer>()
    .HasOne(mp => mp.Match)
    .WithMany(m => m.MatchPlayers)
    .HasForeignKey(mp => mp.MatchId)
    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MatchPlayer>()
            .HasOne(mp => mp.Player)
            .WithMany(p => p.MatchPlayers)
            .HasForeignKey(mp => mp.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MatchPlayer>()
            .HasIndex(mp => new
            {
                mp.MatchId,
                mp.PlayerId
            })
            .IsUnique();

        modelBuilder.Entity<Innings>()
    .HasOne(i => i.Match)
    .WithMany(m => m.Innings)
    .HasForeignKey(i => i.MatchId)
    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Ball>()
    .HasOne(b => b.Innings)
    .WithMany(i => i.Balls)
    .HasForeignKey(b => b.InningsId)
    .OnDelete(DeleteBehavior.Cascade);



        modelBuilder.Entity<Ball>()
     .HasOne(b => b.StrikerMatchPlayer)
     .WithMany(mp => mp.BallsAsStriker)
     .HasForeignKey(b => b.StrikerMatchPlayerId)
     .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ball>()
            .HasOne(b => b.NonStrikerMatchPlayer)
            .WithMany(mp => mp.BallsAsNonStriker)
            .HasForeignKey(b => b.NonStrikerMatchPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ball>()
            .HasOne(b => b.BowlerMatchPlayer)
            .WithMany(mp => mp.BallsAsBowler)
            .HasForeignKey(b => b.BowlerMatchPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ball>()
    .HasOne(b => b.Wicket)
    .WithOne(w => w.Ball)
    .HasForeignKey<Wicket>(w => w.BallId)
    .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<Wicket>()
    .HasOne(w => w.DismissedMatchPlayer)
    .WithMany(mp => mp.Dismissals)
    .HasForeignKey(w => w.DismissedMatchPlayerId)
    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Wicket>()
            .HasOne(w => w.CaughtByMatchPlayer)
            .WithMany(mp => mp.Catches)
            .HasForeignKey(w => w.CaughtByMatchPlayerId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<Innings>()
    .HasOne(i => i.StrikerMatchPlayer)
    .WithMany()
    .HasForeignKey(i => i.StrikerMatchPlayerId)
    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Innings>()
            .HasOne(i => i.NonStrikerMatchPlayer)
            .WithMany()
            .HasForeignKey(i => i.NonStrikerMatchPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Innings>()
            .HasOne(i => i.CurrentBowlerMatchPlayer)
            .WithMany()
            .HasForeignKey(i => i.CurrentBowlerMatchPlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}