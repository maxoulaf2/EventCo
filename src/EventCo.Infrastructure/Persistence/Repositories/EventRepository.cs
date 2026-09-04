using EventCo.Application.Common.Interfaces;
using EventCo.Domain.Events;

namespace EventCo.Infrastructure.Persistence.Repositories;

internal sealed class EventRepository(EventCoDbContext dbContext) : IEventRepository
{
    public async Task AddAsync(Event @event, CancellationToken cancellationToken)
    {
        await dbContext.Events.AddAsync(@event, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
