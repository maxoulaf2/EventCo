using EventCo.Domain.Common;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Domain.Events;

public class EventItem : Entity
{
    public Guid EventId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Quantity { get; private set; }
    public EventItemKind Kind { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public EventItemCategory? CorrectedCategory { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Seule la correction est stockée : la catégorie déduite est recalculée à chaque lecture
    // (cf. docs/adr/0001-categorie-deduite-a-la-lecture.md).
    public EventItemCategory Category => CorrectedCategory ?? EventItemCategoryClassifier.Classify(Title);

    internal EventItem(Guid eventId, string title, string? quantity, EventItemKind kind, Guid createdByUserId, DateTime createdAt)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new EventItemTitleEmptyException(eventId);

        EventId = eventId;
        Title = title.Trim();
        Quantity = quantity;
        Kind = kind;
        AssignedToUserId = kind == EventItemKind.Contribution ? createdByUserId : null;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
    }

    private EventItem(Guid id, Guid eventId, string title, string? quantity, EventItemKind kind, Guid? assignedToUserId, EventItemCategory? correctedCategory, Guid createdByUserId, DateTime createdAt)
        : base(id)
    {
        EventId = eventId;
        Title = title;
        Quantity = quantity;
        Kind = kind;
        AssignedToUserId = assignedToUserId;
        CorrectedCategory = correctedCategory;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
    }

    internal static EventItem Reconstitute(Guid id, Guid eventId, string title, string? quantity, EventItemKind kind, Guid? assignedToUserId, EventItemCategory? correctedCategory, Guid createdByUserId, DateTime createdAt) =>
        new(id, eventId, title, quantity, kind, assignedToUserId, correctedCategory, createdByUserId, createdAt);

    internal void AssignTo(Guid userId) => AssignedToUserId = userId;

    internal void CorrectCategory(EventItemCategory category) => CorrectedCategory = category;
}
