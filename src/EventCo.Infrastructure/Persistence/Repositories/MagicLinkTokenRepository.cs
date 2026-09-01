using EventCo.Application.Common.Interfaces;
using EventCo.Domain.Auth;
using Microsoft.EntityFrameworkCore;

namespace EventCo.Infrastructure.Persistence.Repositories;

internal sealed class MagicLinkTokenRepository(EventCoDbContext dbContext) : IMagicLinkTokenRepository
{
    public async Task AddAsync(MagicLinkToken token, CancellationToken cancellationToken)
    {
        await dbContext.MagicLinkTokens.AddAsync(token, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<MagicLinkToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        dbContext.MagicLinkTokens.SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task UpdateAsync(MagicLinkToken token, CancellationToken cancellationToken)
    {
        dbContext.MagicLinkTokens.Update(token);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
