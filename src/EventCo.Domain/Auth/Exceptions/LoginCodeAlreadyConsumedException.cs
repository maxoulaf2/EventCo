using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.Exceptions;

public sealed class LoginCodeAlreadyConsumedException : DomainException
{
    public Guid LoginCodeId { get; }
    public DateTime? ConsumedAt { get; }

    public LoginCodeAlreadyConsumedException(Guid loginCodeId, DateTime? consumedAt)
        : base($"Ce code de connexion a déjà été utilisé (LoginCodeId: {loginCodeId}, ConsumedAt: {consumedAt:O}).")
    {
        LoginCodeId = loginCodeId;
        ConsumedAt = consumedAt;
    }
}
