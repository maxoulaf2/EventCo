using EventCo.Domain.Events;
using EventCo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCo.Infrastructure.Persistence.Configurations;

public class EventTaskConfiguration : IEntityTypeConfiguration<EventTask>
{
    public void Configure(EntityTypeBuilder<EventTask> builder)
    {
        builder.ToTable("EventTasks");

        builder.HasKey(t => t.Id);

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

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
