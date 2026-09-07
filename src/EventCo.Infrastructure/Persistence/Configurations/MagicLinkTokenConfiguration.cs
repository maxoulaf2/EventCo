using EventCo.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCo.Infrastructure.Persistence.Configurations;

public class MagicLinkTokenConfiguration : IEntityTypeConfiguration<MagicLinkTokenEntity>
{
    public void Configure(EntityTypeBuilder<MagicLinkTokenEntity> builder)
    {
        builder.ToTable("MagicLinkTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(t => t.Email);

        builder.Property(t => t.TokenHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.HasIndex(t => t.TokenHash).IsUnique();

        builder.Property(t => t.ExpiresAt).IsRequired();

        builder.Property(t => t.ConsumedAt);
    }
}
