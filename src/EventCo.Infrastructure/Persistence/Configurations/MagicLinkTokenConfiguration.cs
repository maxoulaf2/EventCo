using EventCo.Domain.Auth;
using EventCo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCo.Infrastructure.Persistence.Configurations;

public class MagicLinkTokenConfiguration : IEntityTypeConfiguration<MagicLinkToken>
{
    public void Configure(EntityTypeBuilder<MagicLinkToken> builder)
    {
        builder.ToTable("MagicLinkTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Email)
            .HasConversion(email => email.Value, value => Email.Create(value))
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
