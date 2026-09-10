using EventCo.Domain.Common;

namespace EventCo.Application.Common.Messaging;

public sealed class DomainEventCollector
{
    private readonly List<IDomainEvent> _pendingEvents = [];

    public IReadOnlyCollection<IDomainEvent> PendingEvents => _pendingEvents.AsReadOnly();

    public void Collect(Entity entity)
    {
        _pendingEvents.AddRange(entity.DomainEvents);
        entity.ClearDomainEvents();
    }

    public void Clear() => _pendingEvents.Clear();
}
