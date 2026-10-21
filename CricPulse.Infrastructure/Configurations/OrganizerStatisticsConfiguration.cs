using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CricPulse.Infrastructure.Configurations;

public class OrganizerStatisticsConfiguration : IEntityTypeConfiguration<OrganizerStatistics>
{
    public void Configure(EntityTypeBuilder<OrganizerStatistics> builder)
    {
        // Keep the existing SQL table name to preserve historical data.
        builder.ToTable("UmpireStatistics");
        builder.HasKey(x => x.UserId);
        builder.HasOne(x => x.User).WithOne()
            .HasForeignKey<OrganizerStatistics>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
