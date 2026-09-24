using EventCo.Domain.Common;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Domain.Events;

public class EventItem : Entity
{
    public Guid EventId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Quantity { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    internal EventItem(Guid eventId, string title, string? quantity, Guid createdByUserId, DateTime createdAt)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new EventItemTitleEmptyException(eventId);

        EventId = eventId;
        Title = title.Trim();
        Quantity = quantity;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
    }

    private EventItem(Guid id, Guid eventId, string title, string? quantity, Guid? assignedToUserId, Guid createdByUserId, DateTime createdAt)
        : base(id)
    {
        EventId = eventId;
        Title = title;
        Quantity = quantity;
        AssignedToUserId = assignedToUserId;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
    }

    internal static EventItem Reconstitute(Guid id, Guid eventId, string title, string? quantity, Guid? assignedToUserId, Guid createdByUserId, DateTime createdAt) =>
        new(id, eventId, title, quantity, assignedToUserId, createdByUserId, createdAt);

    internal void AssignTo(Guid userId) => AssignedToUserId = userId;

    internal void Unassign() => AssignedToUserId = null;
}
