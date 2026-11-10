using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CricPulse.Infrastructure.Configurations;
// Defines the database mapping and user relationship for player statistics.
public class PlayerStatisticsConfiguration : IEntityTypeConfiguration<PlayerStatistics>
{
    // Configures the player statistics table, key, and user relationship.
    public void Configure(EntityTypeBuilder<PlayerStatistics> builder)
    {
        builder.ToTable("PlayerStatistics");
        builder.HasKey(x => x.UserId);
        builder.Property(x => x.BestBowling).HasMaxLength(50);
        // Automatically deletes player statistics when the associated user is deleted.
        builder.HasOne(x => x.User).WithOne()
            .HasForeignKey<PlayerStatistics>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
