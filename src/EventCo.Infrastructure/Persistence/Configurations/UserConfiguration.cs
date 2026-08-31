using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCo.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(2048);

        builder.Property(u => u.CreatedAt)
            .IsRequired();
    }
}
