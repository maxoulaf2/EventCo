using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.Exceptions;

public sealed class LoginCodeLockedException : DomainException
{
    public Guid LoginCodeId { get; }

    public LoginCodeLockedException(Guid loginCodeId)
        : base($"Ce code de connexion est bloqué après trop d'essais erronés (LoginCodeId: {loginCodeId}).")
    {
        LoginCodeId = loginCodeId;
    }
}
