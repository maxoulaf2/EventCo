using EventCo.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCo.Infrastructure.Persistence.Configurations;

public class LoginCodeConfiguration : IEntityTypeConfiguration<LoginCodeEntity>
{
    public void Configure(EntityTypeBuilder<LoginCodeEntity> builder)
    {
        builder.ToTable("LoginCodes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(t => t.Email);

        // Pas d'index unique : le hash d'un code à 6 chiffres peut légitimement se répéter entre deux
        // emails (ou deux demandes successives). Le code est toujours recherché par email (index ci-dessus).
        builder.Property(t => t.CodeHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(t => t.ExpiresAt).IsRequired();

        builder.Property(t => t.ConsumedAt);

        builder.Property(t => t.FailedAttempts)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(t => t.EventInviteLinkToken)
            .HasMaxLength(64);

        builder.Property(t => t.CreatedAt).IsRequired();

        // Utilisé pour compter les demandes récentes par email (anti-spam, cf. TooManyLoginCodeRequestsException).
        builder.HasIndex(t => new { t.Email, t.CreatedAt });
    }
}
