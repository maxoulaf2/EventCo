using EventCo.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCo.Infrastructure.Persistence.Configurations;

public class EventTaskConfiguration : IEntityTypeConfiguration<EventTaskEntity>
{
    public void Configure(EntityTypeBuilder<EventTaskEntity> builder)
    {
        builder.ToTable("EventTasks");

        builder.HasKey(t => t.Id);

        // Cf. EventParticipantConfiguration : même piège EF Core anticipé ici pour le lot 3 (AddTask sur un Event chargé).
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Category)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Quantity)
            .HasMaxLength(100);

        builder.Property(t => t.IsDone).IsRequired();

        builder.Property(t => t.CreatedAt).IsRequired();

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
