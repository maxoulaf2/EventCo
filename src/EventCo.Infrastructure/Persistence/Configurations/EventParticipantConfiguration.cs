using EventCo.Domain.Events;
using EventCo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCo.Infrastructure.Persistence.Configurations;

public class EventParticipantConfiguration : IEntityTypeConfiguration<EventParticipant>
{
    public void Configure(EntityTypeBuilder<EventParticipant> builder)
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

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
