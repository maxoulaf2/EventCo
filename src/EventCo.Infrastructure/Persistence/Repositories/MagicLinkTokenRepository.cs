using EventCo.Application.Common.Interfaces;
using EventCo.Domain.Auth;

namespace EventCo.Infrastructure.Persistence.Repositories;

internal sealed class MagicLinkTokenRepository(EventCoDbContext dbContext) : IMagicLinkTokenRepository
{
    public async Task AddAsync(MagicLinkToken token, CancellationToken cancellationToken)
    {
        await dbContext.MagicLinkTokens.AddAsync(token, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
