using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CricPulse.Infrastructure.Configurations;
// Defines database constraints, indexes, and relationships for password reset tokens.
public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    // Configures token properties, uniqueness, expiration, and user relationship.
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(500);
        // Ensures each password reset token hash is unique.
        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.UsedAt);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
        // Automatically deletes a user's reset tokens when that user is deleted.
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
    }
}