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
                case TaskCreatedDomainEvent e:
                    await InsertTask(e.Task, cancellationToken);
                    break;
                case TaskAssignedDomainEvent e:
                    await UpdateTaskAssignment(e.Task, cancellationToken);
                    break;
                case TaskUnassignedDomainEvent e:
                    await UpdateTaskAssignment(e.Task, cancellationToken);
                    break;
                case TaskDeletedDomainEvent e:
                    await DeleteTask(e.TaskId, cancellationToken);
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

    private async Task InsertTask(EventTask task, CancellationToken cancellationToken) =>
        await dbContext.Set<EventTaskEntity>().AddAsync(EventMapper.ToEntity(task), cancellationToken);

    private async Task UpdateTaskAssignment(EventTask task, CancellationToken cancellationToken)
    {
        var entity = await FindTaskEntityAsync(task.Id, cancellationToken);
        entity.AssignedToUserId = task.AssignedToUserId;
    }

    private async Task DeleteTask(Guid taskId, CancellationToken cancellationToken)
    {
        var entity = await FindTaskEntityAsync(taskId, cancellationToken);
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

    private async Task<EventTaskEntity> FindTaskEntityAsync(Guid taskId, CancellationToken cancellationToken) =>
        await dbContext.Set<EventTaskEntity>().FindAsync([taskId], cancellationToken)
        ?? throw new InvalidOperationException($"EventTaskEntity {taskId} introuvable.");

    public async Task DeleteAsync(Event @event, CancellationToken cancellationToken)
    {
        // Les FK EventParticipants/EventTasks -> Events sont en Restrict (pas de cascade DB) :
        // les enfants doivent être supprimés explicitement avant le parent.
        var participants = await dbContext.Set<EventParticipantEntity>()
            .Where(p => p.EventId == @event.Id)
            .ToListAsync(cancellationToken);
        dbContext.RemoveRange(participants);

        var tasks = await dbContext.Set<EventTaskEntity>()
            .Where(t => t.EventId == @event.Id)
            .ToListAsync(cancellationToken);
        dbContext.RemoveRange(tasks);

        var entity = await dbContext.Events.FirstAsync(e => e.Id == @event.Id, cancellationToken);
        dbContext.Events.Remove(entity);
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Events
            .Include(e => e.Participants)
            .Include(e => e.Tasks)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return entity is null ? null : EventMapper.ToDomain(entity);
    }

    public async Task<Event?> GetByInviteLinkTokenAsync(string token, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Events
            .Include(e => e.Participants)
            .Include(e => e.Tasks)
            .FirstOrDefaultAsync(e => e.InviteLinkToken == token, cancellationToken);

        return entity is null ? null : EventMapper.ToDomain(entity);
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
}
