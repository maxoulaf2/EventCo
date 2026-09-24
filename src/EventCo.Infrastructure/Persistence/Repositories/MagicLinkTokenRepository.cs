using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Auth;
using EventCo.Domain.Auth.DomainEvents;
using EventCo.Domain.ValueObjects;
using EventCo.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace EventCo.Infrastructure.Persistence.Repositories;

internal sealed class MagicLinkTokenRepository(EventCoDbContext dbContext, DomainEventCollector domainEventCollector) : IMagicLinkTokenRepository
{
    public async Task<IReadOnlyList<MagicLinkToken>> GetUsableByEmailAsync(Email email, DateTime now, CancellationToken cancellationToken)
    {
        var entities = await dbContext.MagicLinkTokens
            .Where(t => t.Email == email.Value
                && t.ConsumedAt == null
                && t.ExpiresAt > now
                && t.FailedAttempts < MagicLinkToken.MaxFailedAttempts)
            .ToListAsync(cancellationToken);

        return entities.Select(MagicLinkTokenMapper.ToDomain).ToList();
    }

    public Task<int> CountCreatedSinceAsync(Email email, DateTime since, CancellationToken cancellationToken) =>
        dbContext.MagicLinkTokens.CountAsync(t => t.Email == email.Value && t.CreatedAt >= since, cancellationToken);

    public async Task ApplyAsync(MagicLinkToken token, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in token.DomainEvents)
        {
            switch (domainEvent)
            {
                case MagicLinkTokenCreatedDomainEvent:
                    await InsertToken(token, cancellationToken);
                    break;
                case MagicLinkTokenConsumedDomainEvent:
                case MagicLinkTokenFailedAttemptRegisteredDomainEvent:
                    await UpdateTokenState(token, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Domain event non géré par {nameof(MagicLinkTokenRepository)} : {domainEvent.GetType().Name}.");
            }
        }

        domainEventCollector.Collect(token);
    }

    private async Task InsertToken(MagicLinkToken token, CancellationToken cancellationToken)
    {
        var entity = MagicLinkTokenMapper.ToEntity(token);
        await dbContext.MagicLinkTokens.AddAsync(entity, cancellationToken);
    }

    private async Task UpdateTokenState(MagicLinkToken token, CancellationToken cancellationToken)
    {
        var entity = await dbContext.MagicLinkTokens.FindAsync([token.Id], cancellationToken)
            ?? throw new InvalidOperationException($"MagicLinkTokenEntity {token.Id} introuvable.");
        MagicLinkTokenMapper.ApplyToEntity(token, entity);
    }
}
