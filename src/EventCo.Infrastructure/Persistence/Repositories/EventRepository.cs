using EventCo.Application.Common.Interfaces;
using EventCo.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace EventCo.Infrastructure.Persistence.Repositories;

internal sealed class EventRepository(EventCoDbContext dbContext) : IEventRepository
{
    public async Task AddAsync(Event @event, CancellationToken cancellationToken)
    {
        await dbContext.Events.AddAsync(@event, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(Event @event, CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(Event @event, CancellationToken cancellationToken)
    {
        // Les FK EventParticipants/EventTasks -> Events sont en Restrict (pas de cascade DB) :
        // les enfants doivent être supprimés explicitement avant le parent.
        var participants = await dbContext.Set<EventParticipant>()
            .Where(p => p.EventId == @event.Id)
            .ToListAsync(cancellationToken);
        dbContext.RemoveRange(participants);

        var tasks = await dbContext.Set<EventTask>()
            .Where(t => t.EventId == @event.Id)
            .ToListAsync(cancellationToken);
        dbContext.RemoveRange(tasks);

        dbContext.Events.Remove(@event);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Event>> GetByParticipantUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.Events
            .Include(e => e.Participants)
            .Where(e => e.Participants.Any(p => p.UserId == userId))
            .OrderBy(e => e.EventDate)
            .ToListAsync(cancellationToken);
}
