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
            EventTask.Reconstitute(t.Id, t.EventId, t.Title, t.Category, t.Quantity, t.AssignedToUserId, t.IsDone, t.CreatedByUserId, t.CreatedAt));

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

    // Retourne les entités enfants retirées des collections (Participants/Tasks), à supprimer explicitement
    // du DbContext par l'appelant : les FK EventParticipants/EventTasks -> Events sont en Restrict (pas de
    // cascade DB, cf. EventRepository.DeleteAsync), donc les retirer de la liste en mémoire ne suffit pas
    // (EF lève une InvalidOperationException sur la relation "severed" sans ce retrait explicite).
    public static IReadOnlyList<object> ApplyToEntity(Event domain, EventEntity entity)
    {
        entity.Title = domain.Title;
        entity.Description = domain.Description;
        entity.EventDate = domain.EventDate;
        entity.Location = domain.Location;
        entity.Status = domain.Status;

        var removedParticipants = SyncChildren(domain.Participants, entity.Participants,
            d => d.Id, e => e.Id,
            (d, e) =>
            {
                e.Role = d.Role;
                e.JoinedAt = d.JoinedAt;
            },
            d => ToEntity(d));

        var removedTasks = SyncChildren(domain.Tasks, entity.Tasks,
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

        return removedParticipants.Cast<object>().Concat(removedTasks.Cast<object>()).ToList();
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
        CreatedByUserId = domain.CreatedByUserId,
        CreatedAt = domain.CreatedAt,
    };

    private static List<TEntity> SyncChildren<TDomain, TEntity>(
        IReadOnlyCollection<TDomain> domainItems,
        List<TEntity> entityItems,
        Func<TDomain, Guid> domainId,
        Func<TEntity, Guid> entityId,
        Action<TDomain, TEntity> updateExisting,
        Func<TDomain, TEntity> createNew)
    {
        var removed = entityItems.Where(e => domainItems.All(d => domainId(d) != entityId(e))).ToList();
        foreach (var entity in removed)
            entityItems.Remove(entity);

        foreach (var domainItem in domainItems)
        {
            var existing = entityItems.FirstOrDefault(e => entityId(e) == domainId(domainItem));
            if (existing is not null)
                updateExisting(domainItem, existing);
            else
                entityItems.Add(createNew(domainItem));
        }

        return removed;
    }
}
