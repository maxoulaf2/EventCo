using EventCo.Domain.Auth;
using EventCo.Domain.ValueObjects;
using EventCo.Infrastructure.Persistence.Entities;

namespace EventCo.Infrastructure.Persistence.Mapping;

internal static class MagicLinkTokenMapper
{
    public static MagicLinkToken ToDomain(MagicLinkTokenEntity entity) =>
        MagicLinkToken.Reconstitute(entity.Id, Email.From(entity.Email), entity.TokenHash, entity.ExpiresAt, entity.ConsumedAt, entity.EventInviteLinkToken);

    public static MagicLinkTokenEntity ToEntity(MagicLinkToken domain) => new()
    {
        Id = domain.Id,
        Email = domain.Email.Value,
        TokenHash = domain.TokenHash,
        ExpiresAt = domain.ExpiresAt,
        ConsumedAt = domain.ConsumedAt,
        EventInviteLinkToken = domain.EventInviteLinkToken,
    };

    public static void ApplyToEntity(MagicLinkToken domain, MagicLinkTokenEntity entity)
    {
        entity.ConsumedAt = domain.ConsumedAt;
    }
}
