using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CricPulse.Infrastructure.Configurations;

public class PlayerStatisticsConfiguration : IEntityTypeConfiguration<PlayerStatistics>
{
    public void Configure(EntityTypeBuilder<PlayerStatistics> builder)
    {
        builder.ToTable("PlayerStatistics");
        builder.HasKey(x => x.UserId);
        builder.Property(x => x.BestBowling).HasMaxLength(50);
        builder.HasOne(x => x.User).WithOne()
            .HasForeignKey<PlayerStatistics>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
