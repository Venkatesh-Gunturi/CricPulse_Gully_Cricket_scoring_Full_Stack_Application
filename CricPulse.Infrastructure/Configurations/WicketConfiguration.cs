using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CricPulse.Infrastructure.Configurations;
// Defines database constraints and relationships for Wicket entities.
public class WicketConfiguration : IEntityTypeConfiguration<Wicket>
{
    // Configures wicket properties and relationships with balls and dismissed players.
    public void Configure(EntityTypeBuilder<Wicket> builder)
    {
        builder.ToTable("Wickets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.WicketType)
            .IsRequired()
            .HasMaxLength(30);
        // Automatically deletes the associated wicket when its ball is deleted.
        builder.HasOne(x => x.Ball)
            .WithOne(x => x.Wicket)
            .HasForeignKey<Wicket>(x => x.BallId)
            .OnDelete(DeleteBehavior.Cascade);
        // Prevents deleting a match player while they are referenced in a wicket record.
        builder.HasOne(x => x.DismissedMatchPlayer)
            .WithMany()
            .HasForeignKey(x => x.DismissedMatchPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CaughtByMatchPlayer)
            .WithMany()
            .HasForeignKey(x => x.CaughtByMatchPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RunOutByMatchPlayer)
            .WithMany()
            .HasForeignKey(x => x.RunOutByMatchPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StumpedByMatchPlayer)
            .WithMany()
            .HasForeignKey(x => x.StumpedByMatchPlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}