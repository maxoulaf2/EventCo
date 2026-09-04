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

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
}
