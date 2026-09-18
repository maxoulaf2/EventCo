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

        // Id généré côté Domain (Guid.NewGuid()), jamais par la base : sans ce ValueGeneratedNever, EF Core ne sait
        // pas distinguer "nouvelle entité" d'"entité existante modifiée" quand elle est ajoutée à la collection
        // d'un agrégat déjà suivi par le ChangeTracker (ex: EventRepository.UpdateAsync après Event.InviteParticipant)
        // et la marque à tort Modified, ce qui fait échouer SaveChanges (DbUpdateConcurrencyException).
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.InvitedAt).IsRequired();

        builder.Property(p => p.JoinedAt);

        builder.Property(p => p.ParticipationStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(p => new { p.EventId, p.UserId }).IsUnique();

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
