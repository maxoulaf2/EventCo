using EventCo.Domain.Events;
using EventCo.Infrastructure.Persistence.Entities;

namespace EventCo.Infrastructure.Persistence.Mapping;

internal static class EventMapper
{
    public static Event ToDomain(EventEntity entity)
    {
        var participants = entity.Participants.Select(p =>
            EventParticipant.Reconstitute(p.Id, p.EventId, p.UserId, p.Role, p.InvitedAt, p.ParticipationStatus));

        var tasks = entity.Tasks.Select(t =>
            EventTask.Reconstitute(t.Id, t.EventId, t.Title, t.Category, t.Quantity, t.AssignedToUserId, t.CreatedByUserId, t.CreatedAt));

        return Event.Reconstitute(
            entity.Id, entity.Title, entity.Description, entity.EventDate, entity.Location, entity.ImageUrl, entity.InviteLinkToken,
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
            ImageUrl = domain.ImageUrl,
            InviteLinkToken = domain.InviteLinkToken,
            CreatedByUserId = domain.CreatedByUserId,
            Status = domain.Status,
            CreatedAt = domain.CreatedAt,
        };

        entity.Participants.AddRange(domain.Participants.Select(ToEntity));
        entity.Tasks.AddRange(domain.Tasks.Select(ToEntity));

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

    internal static EventTaskEntity ToEntity(EventTask domain) => new()
    {
        Id = domain.Id,
        EventId = domain.EventId,
        Title = domain.Title,
        Category = domain.Category,
        Quantity = domain.Quantity,
        AssignedToUserId = domain.AssignedToUserId,
        CreatedByUserId = domain.CreatedByUserId,
        CreatedAt = domain.CreatedAt,
    };
}
