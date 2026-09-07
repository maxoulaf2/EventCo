using EventCo.Domain.Events;
using EventCo.Infrastructure.Persistence.Entities;

namespace EventCo.Infrastructure.Persistence.Mapping;

internal static class EventMapper
{
    public static Event ToDomain(EventEntity entity)
    {
        var participants = entity.Participants.Select(p =>
            EventParticipant.Reconstitute(p.Id, p.EventId, p.UserId, p.Role, p.InvitedAt, p.JoinedAt));

        var tasks = entity.Tasks.Select(t =>
            EventTask.Reconstitute(t.Id, t.EventId, t.Title, t.Category, t.Quantity, t.AssignedToUserId, t.IsDone, t.CreatedAt));

        return Event.Reconstitute(
            entity.Id, entity.Title, entity.Description, entity.EventDate, entity.Location,
            entity.CreatedByUserId, entity.Status, entity.CreatedAt, participants, tasks);
    }

    public static EventEntity ToEntity(Event domain)
    {
        var entity = new EventEntity
        {
            Id = domain.Id,
            Title = domain.Title,
            Description = domain.Description,
            EventDate = domain.EventDate,
            Location = domain.Location,
            CreatedByUserId = domain.CreatedByUserId,
            Status = domain.Status,
            CreatedAt = domain.CreatedAt,
        };

        entity.Participants.AddRange(domain.Participants.Select(ToEntity));
        entity.Tasks.AddRange(domain.Tasks.Select(ToEntity));

        return entity;
    }

    public static void ApplyToEntity(Event domain, EventEntity entity)
    {
        entity.Title = domain.Title;
        entity.Description = domain.Description;
        entity.EventDate = domain.EventDate;
        entity.Location = domain.Location;
        entity.Status = domain.Status;

        SyncChildren(domain.Participants, entity.Participants,
            d => d.Id, e => e.Id,
            (d, e) =>
            {
                e.Role = d.Role;
                e.JoinedAt = d.JoinedAt;
            },
            d => ToEntity(d));

        SyncChildren(domain.Tasks, entity.Tasks,
            d => d.Id, e => e.Id,
            (d, e) =>
            {
                e.Title = d.Title;
                e.Category = d.Category;
                e.Quantity = d.Quantity;
                e.AssignedToUserId = d.AssignedToUserId;
                e.IsDone = d.IsDone;
            },
            d => ToEntity(d));
    }

    private static EventParticipantEntity ToEntity(EventParticipant domain) => new()
    {
        Id = domain.Id,
        EventId = domain.EventId,
        UserId = domain.UserId,
        Role = domain.Role,
        InvitedAt = domain.InvitedAt,
        JoinedAt = domain.JoinedAt,
    };

    private static EventTaskEntity ToEntity(EventTask domain) => new()
    {
        Id = domain.Id,
        EventId = domain.EventId,
        Title = domain.Title,
        Category = domain.Category,
        Quantity = domain.Quantity,
        AssignedToUserId = domain.AssignedToUserId,
        IsDone = domain.IsDone,
        CreatedAt = domain.CreatedAt,
    };

    private static void SyncChildren<TDomain, TEntity>(
        IReadOnlyCollection<TDomain> domainItems,
        List<TEntity> entityItems,
        Func<TDomain, Guid> domainId,
        Func<TEntity, Guid> entityId,
        Action<TDomain, TEntity> updateExisting,
        Func<TDomain, TEntity> createNew)
    {
        entityItems.RemoveAll(e => domainItems.All(d => domainId(d) != entityId(e)));

        foreach (var domainItem in domainItems)
        {
            var existing = entityItems.FirstOrDefault(e => entityId(e) == domainId(domainItem));
            if (existing is not null)
                updateExisting(domainItem, existing);
            else
                entityItems.Add(createNew(domainItem));
        }
    }
}
