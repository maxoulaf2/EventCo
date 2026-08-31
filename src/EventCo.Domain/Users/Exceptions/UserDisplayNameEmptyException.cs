using EventCo.Domain.Common;

namespace EventCo.Domain.Users.Exceptions;

public sealed class UserDisplayNameEmptyException : DomainException
{
    public Guid? UserId { get; }

    public UserDisplayNameEmptyException(Guid? userId = null)
        : base("Le nom affiché ne peut pas être vide." + (userId is not null ? $" (UserId: {userId})" : string.Empty))
    {
        UserId = userId;
    }
}
