using EventCo.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCo.Infrastructure.Persistence.Configurations;

public class EventParticipantConfiguration : IEntityTypeConfiguration<EventParticipantEntity>
{
    public void Configure(EntityTypeBuilder<EventParticipantEntity> builder)
    {
        builder.ToTable("EventParticipants");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.InvitedAt).IsRequired();

        builder.Property(p => p.JoinedAt);

        builder.HasIndex(p => new { p.EventId, p.UserId }).IsUnique();

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
