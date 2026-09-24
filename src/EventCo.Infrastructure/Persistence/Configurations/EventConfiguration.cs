using EventCo.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCo.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<EventEntity>
{
    public void Configure(EntityTypeBuilder<EventEntity> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Location)
            .HasMaxLength(300);

        builder.Property(e => e.ImageStorageKey)
            .HasMaxLength(256);

        builder.Property(e => e.InviteLinkToken)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(e => e.InviteLinkToken).IsUnique();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.CreatedByUserId).IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Participants)
            .WithOne()
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Items)
            .WithOne()
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
