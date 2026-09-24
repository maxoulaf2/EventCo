using EventCo.Domain.Auth;
using EventCo.Domain.ValueObjects;
using EventCo.Infrastructure.Persistence.Entities;

namespace EventCo.Infrastructure.Persistence.Mapping;

internal static class LoginCodeMapper
{
    public static LoginCode ToDomain(LoginCodeEntity entity) =>
        LoginCode.Reconstitute(entity.Id, Email.From(entity.Email), entity.CodeHash, entity.ExpiresAt, entity.ConsumedAt, entity.FailedAttempts, entity.EventInviteLinkToken, entity.CreatedAt);

    public static LoginCodeEntity ToEntity(LoginCode domain) => new()
    {
        Id = domain.Id,
        Email = domain.Email.Value,
        CodeHash = domain.CodeHash,
        ExpiresAt = domain.ExpiresAt,
        ConsumedAt = domain.ConsumedAt,
        FailedAttempts = domain.FailedAttempts,
        EventInviteLinkToken = domain.EventInviteLinkToken,
        CreatedAt = domain.CreatedAt,
    };

    public static void ApplyToEntity(LoginCode domain, LoginCodeEntity entity)
    {
        entity.ConsumedAt = domain.ConsumedAt;
        entity.FailedAttempts = domain.FailedAttempts;
    }
}
