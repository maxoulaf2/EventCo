using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Auth;
using EventCo.Domain.Auth.DomainEvents;
using EventCo.Domain.ValueObjects;
using EventCo.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace EventCo.Infrastructure.Persistence.Repositories;

internal sealed class LoginCodeRepository(EventCoDbContext dbContext, DomainEventCollector domainEventCollector) : ILoginCodeRepository
{
    public async Task<IReadOnlyList<LoginCode>> GetUsableByEmailAsync(Email email, DateTime now, CancellationToken cancellationToken)
    {
        var entities = await dbContext.LoginCodes
            .Where(t => t.Email == email.Value
                && t.ConsumedAt == null
                && t.ExpiresAt > now
                && t.FailedAttempts < LoginCode.MaxFailedAttempts)
            .ToListAsync(cancellationToken);

        return entities.Select(LoginCodeMapper.ToDomain).ToList();
    }

    public Task<int> CountCreatedSinceAsync(Email email, DateTime since, CancellationToken cancellationToken) =>
        dbContext.LoginCodes.CountAsync(t => t.Email == email.Value && t.CreatedAt >= since, cancellationToken);

    public async Task ApplyAsync(LoginCode loginCode, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in loginCode.DomainEvents)
        {
            switch (domainEvent)
            {
                case LoginCodeCreatedDomainEvent:
                    await InsertToken(loginCode, cancellationToken);
                    break;
                case LoginCodeConsumedDomainEvent:
                case LoginCodeFailedAttemptRegisteredDomainEvent:
                    await UpdateTokenState(loginCode, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Domain event non géré par {nameof(LoginCodeRepository)} : {domainEvent.GetType().Name}.");
            }
        }

        domainEventCollector.Collect(loginCode);
    }

    private async Task InsertToken(LoginCode loginCode, CancellationToken cancellationToken)
    {
        var entity = LoginCodeMapper.ToEntity(loginCode);
        await dbContext.LoginCodes.AddAsync(entity, cancellationToken);
    }

    private async Task UpdateTokenState(LoginCode loginCode, CancellationToken cancellationToken)
    {
        var entity = await dbContext.LoginCodes.FindAsync([loginCode.Id], cancellationToken)
            ?? throw new InvalidOperationException($"LoginCodeEntity {loginCode.Id} introuvable.");
        LoginCodeMapper.ApplyToEntity(loginCode, entity);
    }
}
