using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CricPulse.Infrastructure.Configurations;
// Defines database constraints and relationships for Innings entities.
public class InningsConfiguration : IEntityTypeConfiguration<Innings>
{
    // Configures innings properties, match relationship, and database indexes.
    public void Configure(EntityTypeBuilder<Innings> builder)
    {
        builder.ToTable("Innings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BattingTeam)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.BowlingTeam)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30);
        // Automatically deletes associated innings when their parent match is deleted.
        builder.HasOne(x => x.Match)
            .WithMany(x => x.Innings)
            .HasForeignKey(x => x.MatchId)
            .OnDelete(DeleteBehavior.Cascade);
        // Prevents duplicate innings numbers within the same match.
        builder.HasIndex(x => new
        {
            x.MatchId,
            x.InningsNumber
        })
        .IsUnique();

        builder.Property(x => x.CompletionPending)
        .IsRequired();

        builder.Property(x => x.IsFreeHit)
            .IsRequired();

        builder.Property(x => x.UndoUsed)
        .IsRequired();
    }
}