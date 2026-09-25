using EventCo.Domain.Events;
using EventCo.Infrastructure.Persistence.Entities;

namespace EventCo.Infrastructure.Persistence.Mapping;

internal static class EventMapper
{
    public static Event ToDomain(EventEntity entity)
    {
        var participants = entity.Participants.Select(p =>
            EventParticipant.Reconstitute(p.Id, p.EventId, p.UserId, p.Role, p.InvitedAt, p.ParticipationStatus));

        var items = entity.Items.Select(t =>
            EventItem.Reconstitute(t.Id, t.EventId, t.Title, t.Quantity, t.Kind, t.AssignedToUserId, t.CreatedByUserId, t.CreatedAt));

        return Event.Reconstitute(
            entity.Id, entity.Title, entity.Description, entity.EventDate, entity.Location, entity.ImageStorageKey, entity.InviteLinkToken,
            entity.CreatedByUserId, entity.Status, entity.CreatedAt, participants, items);
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
            ImageStorageKey = domain.ImageStorageKey,
            InviteLinkToken = domain.InviteLinkToken,
            CreatedByUserId = domain.CreatedByUserId,
            Status = domain.Status,
            CreatedAt = domain.CreatedAt,
        };

        entity.Participants.AddRange(domain.Participants.Select(ToEntity));
        entity.Items.AddRange(domain.Items.Select(ToEntity));

        return entity;
    }

    // Accessibles depuis EventRepository (même assembly) : chaque domain event porté par l'agrégat Event
    // pilote une opération de persistance précise (insertion d'un seul enfant, mise à jour d'un seul champ...)
    // plutôt qu'un diff générique de tout l'agrégat.
    internal static EventParticipantEntity ToEntity(EventParticipant domain) => new()
    {
        Id = domain.Id,
        EventId = domain.EventId,
        UserId = domain.UserId,
        Role = domain.Role,
        InvitedAt = domain.InvitedAt,
        ParticipationStatus = domain.ParticipationStatus,
    };

    internal static EventItemEntity ToEntity(EventItem domain) => new()
    {
        Id = domain.Id,
        EventId = domain.EventId,
        Title = domain.Title,
        Quantity = domain.Quantity,
        Kind = domain.Kind,
        AssignedToUserId = domain.AssignedToUserId,
        CreatedByUserId = domain.CreatedByUserId,
        CreatedAt = domain.CreatedAt,
    };
}
