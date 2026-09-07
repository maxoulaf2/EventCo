using EventCo.Application.Common.Interfaces;
using EventCo.Domain.Auth;
using EventCo.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace EventCo.Infrastructure.Persistence.Repositories;

internal sealed class MagicLinkTokenRepository(EventCoDbContext dbContext) : IMagicLinkTokenRepository
{
    public async Task AddAsync(MagicLinkToken token, CancellationToken cancellationToken)
    {
        var entity = MagicLinkTokenMapper.ToEntity(token);
        await dbContext.MagicLinkTokens.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MagicLinkToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        var entity = await dbContext.MagicLinkTokens.SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
        return entity is null ? null : MagicLinkTokenMapper.ToDomain(entity);
    }

    public async Task UpdateAsync(MagicLinkToken token, CancellationToken cancellationToken)
    {
        var entity = await dbContext.MagicLinkTokens.SingleAsync(t => t.Id == token.Id, cancellationToken);
        MagicLinkTokenMapper.ApplyToEntity(token, entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
