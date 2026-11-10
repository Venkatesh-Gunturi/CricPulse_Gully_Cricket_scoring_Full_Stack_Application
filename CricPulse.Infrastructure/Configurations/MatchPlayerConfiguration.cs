using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CricPulse.Infrastructure.Configurations;
// Defines database constraints, indexes, and relationships for MatchPlayer entities.
public class MatchPlayerConfiguration : IEntityTypeConfiguration<MatchPlayer>
{
    // Configures player roster properties and their match and user relationships.
    public void Configure(EntityTypeBuilder<MatchPlayer> builder)
    {
        builder.ToTable("MatchPlayers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Team).IsRequired().HasMaxLength(20);
        builder.Property(x => x.IsCaptain).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        // Automatically removes roster entries when their associated match is deleted.
        builder.HasOne(x => x.Match).WithMany(x => x.MatchPlayers)
            .HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Cascade);
        // Prevents deleting a user while they are part of a match roster.
        builder.HasOne(x => x.User).WithMany()
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        // The same user can only occupy one roster slot in a match, regardless
        // of which team they are assigned to.
        builder.HasIndex(x => new { x.MatchId, x.UserId }).IsUnique();
        builder.HasIndex(x => new { x.MatchId, x.Team });
    }
}
