using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CricPulse.Infrastructure.Configurations;
// Defines database constraints, indexes, and relationships for Match entities.
public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    // Configures match properties, creator relationship, and database indexes.
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TeamAName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TeamALogo)
            .HasMaxLength(100);

        builder.Property(x => x.TeamBName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TeamBLogo)
            .HasMaxLength(100);

        builder.Property(x => x.ScheduledAt)
            .IsRequired();

        builder.Property(x => x.Venue)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.LocationAddress)
            .HasMaxLength(500);

        builder.Property(x => x.State)
            .HasMaxLength(100);

        builder.Property(x => x.PinCode)
            .HasMaxLength(10);

        builder.Property(x => x.Overs)
            .IsRequired();

        builder.Property(x => x.PlayersPerTeam)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.TossWinnerTeam)
            .HasMaxLength(20);

        builder.Property(x => x.TossDecision)
            .HasMaxLength(20);

        builder.Property(x => x.TossAgreed)
            .IsRequired();

        builder.Property(x => x.CancellationReasonType)
            .HasMaxLength(50);

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(500);
        // Prevents deleting a user while matches created by that user still exist.
        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        // Supports geographic filtering when searching for nearby matches.
        builder.HasIndex(x => new
        {
            x.Latitude,
            x.Longitude
        });
        // Improves filtering and sorting by state, status, and scheduled date.
        builder.HasIndex(x => x.State);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.ScheduledAt);

        builder.Property(x => x.WinnerTeam)
            .HasMaxLength(20);

        builder.Property(x => x.ResultText)
            .HasMaxLength(250);
        builder.Property(x => x.PlayerStatisticsFinalized)
            .IsRequired();
    }
}