using EventCo.Domain.Common;

namespace EventCo.Domain.Users.Exceptions;

public sealed class UserNotAdminException : DomainException
{
    public Guid ActingUserId { get; }

    public UserNotAdminException(Guid actingUserId)
        : base($"Seuls les administrateurs peuvent effectuer cette action (ActingUserId: {actingUserId}).")
    {
        ActingUserId = actingUserId;
    }
}
