using EventCo.Domain.Common;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Domain.Events;

public class EventTask : Entity
{
    public Guid EventId { get; private set; }
    public string Title { get; private set; } = null!;
    public TaskCategory Category { get; private set; }
    public string? Quantity { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public bool IsDone { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    internal EventTask(Guid eventId, string title, TaskCategory category, string? quantity, Guid createdByUserId, DateTime createdAt)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new EventTaskTitleEmptyException(eventId);

        EventId = eventId;
        Title = title.Trim();
        Category = category;
        Quantity = quantity;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
    }

    private EventTask(Guid id, Guid eventId, string title, TaskCategory category, string? quantity, Guid? assignedToUserId, bool isDone, Guid createdByUserId, DateTime createdAt)
        : base(id)
    {
        EventId = eventId;
        Title = title;
        Category = category;
        Quantity = quantity;
        AssignedToUserId = assignedToUserId;
        IsDone = isDone;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
    }

    internal static EventTask Reconstitute(Guid id, Guid eventId, string title, TaskCategory category, string? quantity, Guid? assignedToUserId, bool isDone, Guid createdByUserId, DateTime createdAt) =>
        new(id, eventId, title, category, quantity, assignedToUserId, isDone, createdByUserId, createdAt);

    internal void AssignTo(Guid userId) => AssignedToUserId = userId;

    internal void Unassign() => AssignedToUserId = null;

    internal void MarkDone() => IsDone = true;

    internal void MarkNotDone() => IsDone = false;
}
