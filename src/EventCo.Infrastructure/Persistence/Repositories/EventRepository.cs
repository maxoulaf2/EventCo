using EventCo.Application.Common.Interfaces;
using EventCo.Domain.Events;
using EventCo.Infrastructure.Persistence.Entities;
using EventCo.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace EventCo.Infrastructure.Persistence.Repositories;

internal sealed class EventRepository(EventCoDbContext dbContext) : IEventRepository
{
    public async Task AddAsync(Event @event, CancellationToken cancellationToken)
    {
        var entity = EventMapper.ToEntity(@event);
        await dbContext.Events.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Event @event, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Events
            .Include(e => e.Participants)
            .Include(e => e.Tasks)
            .FirstAsync(e => e.Id == @event.Id, cancellationToken);

        var removedChildren = EventMapper.ApplyToEntity(@event, entity);
        dbContext.RemoveRange(removedChildren);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

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
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Events
            .Include(e => e.Participants)
            .Include(e => e.Tasks)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

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
