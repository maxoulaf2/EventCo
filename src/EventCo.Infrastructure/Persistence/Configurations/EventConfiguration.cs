using EventCo.Domain.Events;
using EventCo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCo.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
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

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.CreatedByUserId).IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Participants)
            .WithOne()
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(e => e.Participants)
            .HasField("_participants")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(e => e.Tasks)
            .WithOne()
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(e => e.Tasks)
            .HasField("_tasks")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
