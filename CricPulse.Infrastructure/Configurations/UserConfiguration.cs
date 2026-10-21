using CricPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CricPulse.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);
        builder.Property(x => x.MobileNumber).IsRequired().HasMaxLength(10);
        builder.HasIndex(x => x.MobileNumber).IsUnique();
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ProfileImageUrl).HasMaxLength(500);
        builder.Property(x => x.Gender).HasMaxLength(30);
        builder.Property(x => x.State).HasMaxLength(100);
        builder.Property(x => x.PinCode).HasMaxLength(10);
        builder.Property(x => x.BattingStyle).HasMaxLength(50);
        builder.Property(x => x.BowlingStyle).HasMaxLength(50);
        builder.Property(x => x.PlayerRole).HasMaxLength(50);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
