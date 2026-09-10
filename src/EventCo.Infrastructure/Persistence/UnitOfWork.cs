using EventCo.Application.Common.Interfaces;

namespace EventCo.Infrastructure.Persistence;

internal sealed class UnitOfWork(EventCoDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
