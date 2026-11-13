using EventCo.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCo.Infrastructure.Persistence.Configurations;

public class EventDateVoteConfiguration : IEntityTypeConfiguration<EventDateVoteEntity>
{
    public void Configure(EntityTypeBuilder<EventDateVoteEntity> builder)
    {
        builder.ToTable("EventDateVotes");

        // Un vote par personne et par option : la clé composée porte l'unicité.
        builder.HasKey(v => new { v.DateOptionId, v.UserId });

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
