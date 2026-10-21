using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CricPulse.Infrastructure.Configurations;

public class InningsConfiguration : IEntityTypeConfiguration<Innings>
{
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

        builder.HasOne(x => x.Match)
            .WithMany(x => x.Innings)
            .HasForeignKey(x => x.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

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