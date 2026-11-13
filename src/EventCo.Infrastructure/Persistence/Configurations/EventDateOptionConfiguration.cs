using EventCo.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCo.Infrastructure.Persistence.Configurations;

public class EventDateOptionConfiguration : IEntityTypeConfiguration<EventDateOptionEntity>
{
    public void Configure(EntityTypeBuilder<EventDateOptionEntity> builder)
    {
        builder.ToTable("EventDateOptions");

        builder.HasKey(o => o.Id);

        // Cf. EventParticipantConfiguration : Id généré côté Domain.
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.Date).IsRequired();

        builder.HasMany(o => o.Votes)
            .WithOne()
            .HasForeignKey(v => v.DateOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
