using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events;
using EventCo.Domain.Events.DomainEvents;
using EventCo.Infrastructure.Persistence.Entities;
using EventCo.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace EventCo.Infrastructure.Persistence.Repositories;

internal sealed class EventRepository(EventCoDbContext dbContext, DomainEventCollector domainEventCollector) : IEventRepository
{
    public async Task ApplyAsync(Event @event, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in @event.DomainEvents)
        {
            switch (domainEvent)
            {
                case EventCreatedDomainEvent:
                    await InsertEvent(@event, cancellationToken);
                    break;
                case EventDetailsUpdatedDomainEvent:
                    await UpdateEventDetails(@event, cancellationToken);
                    break;
                case EventStatusChangedDomainEvent:
                    await UpdateEventStatus(@event, cancellationToken);
                    break;
                case EventImageChangedDomainEvent:
                    await UpdateEventImage(@event, cancellationToken);
                    break;
                case EventInviteLinkRegeneratedDomainEvent:
                    await UpdateEventInviteLink(@event, cancellationToken);
                    break;
                case ParticipantInvitedDomainEvent e:
                    await InsertParticipant(@event, e.ParticipantId, cancellationToken);
                    break;
                case ParticipantRoleChangedDomainEvent e:
                    await UpdateParticipantRole(@event, e.ParticipantId, cancellationToken);
                    break;
                case ParticipantParticipationStatusChangedDomainEvent e:
                    await UpdateParticipantParticipationStatus(@event, e.ParticipantId, cancellationToken);
                    break;
                case ParticipantRemovedDomainEvent e:
                    await DeleteParticipant(e.ParticipantId, cancellationToken);
                    break;
                case ItemCreatedDomainEvent e:
                    await InsertItem(e.Item, cancellationToken);
                    break;
                case ItemAssignedDomainEvent e:
                    await UpdateItemAssignment(e.Item, cancellationToken);
                    break;
                case ItemUnassignedDomainEvent e:
                    await UpdateItemAssignment(e.Item, cancellationToken);
                    break;
                case ItemDeletedDomainEvent e:
                    await DeleteItem(e.ItemId, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Domain event non géré par {nameof(EventRepository)} : {domainEvent.GetType().Name}.");
            }
        }

        domainEventCollector.Collect(@event);
    }

    private async Task InsertEvent(Event @event, CancellationToken cancellationToken)
    {
        var entity = EventMapper.ToEntity(@event);
        await dbContext.Events.AddAsync(entity, cancellationToken);
    }

    private async Task UpdateEventDetails(Event @event, CancellationToken cancellationToken)
    {
        var entity = await FindEventEntityAsync(@event.Id, cancellationToken);
        entity.Title = @event.Title;
        entity.Description = @event.Description;
        entity.EventDate = @event.EventDate;
        entity.Location = @event.Location;
    }

    private async Task UpdateEventStatus(Event @event, CancellationToken cancellationToken)
    {
        var entity = await FindEventEntityAsync(@event.Id, cancellationToken);
        entity.Status = @event.Status;
    }

    private async Task UpdateEventImage(Event @event, CancellationToken cancellationToken)
    {
        var entity = await FindEventEntityAsync(@event.Id, cancellationToken);
        entity.ImageStorageKey = @event.ImageStorageKey;
    }

    private async Task UpdateEventInviteLink(Event @event, CancellationToken cancellationToken)
    {
        var entity = await FindEventEntityAsync(@event.Id, cancellationToken);
        entity.InviteLinkToken = @event.InviteLinkToken;
    }

    private async Task InsertParticipant(Event @event, Guid participantId, CancellationToken cancellationToken)
    {
        var participant = @event.Participants.Single(p => p.Id == participantId);
        await dbContext.Set<EventParticipantEntity>().AddAsync(EventMapper.ToEntity(participant), cancellationToken);
    }

    private async Task UpdateParticipantRole(Event @event, Guid participantId, CancellationToken cancellationToken)
    {
        var participant = @event.Participants.Single(p => p.Id == participantId);
        var entity = await FindParticipantEntityAsync(participantId, cancellationToken);
        entity.Role = participant.Role;
    }

    private async Task UpdateParticipantParticipationStatus(Event @event, Guid participantId, CancellationToken cancellationToken)
    {
        var participant = @event.Participants.Single(p => p.Id == participantId);
        var entity = await FindParticipantEntityAsync(participantId, cancellationToken);
        entity.ParticipationStatus = participant.ParticipationStatus;
    }

    private async Task DeleteParticipant(Guid participantId, CancellationToken cancellationToken)
    {
        var entity = await FindParticipantEntityAsync(participantId, cancellationToken);
        dbContext.Remove(entity);
    }

    private async Task InsertItem(EventItem item, CancellationToken cancellationToken) =>
        await dbContext.Set<EventItemEntity>().AddAsync(EventMapper.ToEntity(item), cancellationToken);

    private async Task UpdateItemAssignment(EventItem item, CancellationToken cancellationToken)
    {
        var entity = await FindItemEntityAsync(item.Id, cancellationToken);
        entity.AssignedToUserId = item.AssignedToUserId;
    }

    private async Task DeleteItem(Guid itemId, CancellationToken cancellationToken)
    {
        var entity = await FindItemEntityAsync(itemId, cancellationToken);
        dbContext.Remove(entity);
    }

    // FindAsync résout depuis le ChangeTracker local sans requête SQL si l'entité est déjà suivie
    // (typiquement déjà chargée par un GetByIdAsync antérieur dans la même requête), sinon la charge.
    private async Task<EventEntity> FindEventEntityAsync(Guid eventId, CancellationToken cancellationToken) =>
        await dbContext.Events.FindAsync([eventId], cancellationToken)
        ?? throw new InvalidOperationException($"EventEntity {eventId} introuvable.");

    private async Task<EventParticipantEntity> FindParticipantEntityAsync(Guid participantId, CancellationToken cancellationToken) =>
        await dbContext.Set<EventParticipantEntity>().FindAsync([participantId], cancellationToken)
        ?? throw new InvalidOperationException($"EventParticipantEntity {participantId} introuvable.");

    private async Task<EventItemEntity> FindItemEntityAsync(Guid itemId, CancellationToken cancellationToken) =>
        await dbContext.Set<EventItemEntity>().FindAsync([itemId], cancellationToken)
        ?? throw new InvalidOperationException($"EventItemEntity {itemId} introuvable.");

    public async Task DeleteAsync(Event @event, CancellationToken cancellationToken)
    {
        // Les FK EventParticipants/EventItems -> Events sont en Restrict (pas de cascade DB) :
        // les enfants doivent être supprimés explicitement avant le parent.
        var participants = await dbContext.Set<EventParticipantEntity>()
            .Where(p => p.EventId == @event.Id)
            .ToListAsync(cancellationToken);
        dbContext.RemoveRange(participants);

        var items = await dbContext.Set<EventItemEntity>()
            .Where(t => t.EventId == @event.Id)
            .ToListAsync(cancellationToken);
        dbContext.RemoveRange(items);

        var entity = await dbContext.Events.FirstAsync(e => e.Id == @event.Id, cancellationToken);
        dbContext.Events.Remove(entity);
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Events
            .Include(e => e.Participants)
            .Include(e => e.Items)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return entity is null ? null : EventMapper.ToDomain(entity);
    }

    public async Task<Event?> GetByInviteLinkTokenAsync(string token, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Events
            .Include(e => e.Participants)
            .Include(e => e.Items)
            .FirstOrDefaultAsync(e => e.InviteLinkToken == token, cancellationToken);

        return entity is null ? null : EventMapper.ToDomain(entity);
    }

    public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.Events
            .Include(e => e.Participants)
            .OrderBy(e => e.EventDate)
            .ToListAsync(cancellationToken);

        return entities.Select(EventMapper.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Event>> GetByParticipantUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var entities = await dbContext.Events
            .Include(e => e.Participants)
            .Where(e => e.Participants.Any(p => p.UserId == userId))
            .OrderBy(e => e.EventDate)
            .ToListAsync(cancellationToken);

        return entities.Select(EventMapper.ToDomain).ToList();
    }

    public Task<int> CountInvitationsForUserSinceAsync(Guid userId, DateTime since, CancellationToken cancellationToken) =>
        dbContext.Set<EventParticipantEntity>().CountAsync(p => p.UserId == userId && p.InvitedAt >= since, cancellationToken);
}
