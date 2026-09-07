using EventCo.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCo.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
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
