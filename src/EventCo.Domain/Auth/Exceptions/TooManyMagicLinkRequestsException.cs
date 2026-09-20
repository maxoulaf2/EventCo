using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.Exceptions;

public sealed class TooManyMagicLinkRequestsException : DomainException
{
    public string Email { get; }
    public int MaxRequests { get; }
    public int WindowMinutes { get; }

    public TooManyMagicLinkRequestsException(string email, int maxRequests, int windowMinutes)
        : base($"Trop de demandes de lien de connexion pour \"{email}\" (max {maxRequests} par {windowMinutes} minutes).")
    {
        Email = email;
        MaxRequests = maxRequests;
        WindowMinutes = windowMinutes;
    }
}
