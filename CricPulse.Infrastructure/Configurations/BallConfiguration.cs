using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CricPulse.Infrastructure.Configurations;

public class BallConfiguration : IEntityTypeConfiguration<Ball>
{
    public void Configure(EntityTypeBuilder<Ball> builder)
    {
        builder.ToTable("Balls");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExtraType)
            .HasMaxLength(30);

        builder.Property(x => x.Notation)
            .HasMaxLength(100);

        builder.Property(x => x.PreviousInningsStatus)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Innings)
            .WithMany(x => x.Balls)
            .HasForeignKey(x => x.InningsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StrikerMatchPlayer)
            .WithMany()
            .HasForeignKey(x => x.StrikerMatchPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.NonStrikerMatchPlayer)
            .WithMany()
            .HasForeignKey(x => x.NonStrikerMatchPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BowlerMatchPlayer)
            .WithMany()
            .HasForeignKey(x => x.BowlerMatchPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.InningsId,
            x.OverNumber,
            x.BallNumber
        });

        builder.Property(x => x.PreviousCompletionPending)
    .IsRequired();

        builder.Property(x => x.PreviousIsFreeHit)
            .IsRequired();
    }
}