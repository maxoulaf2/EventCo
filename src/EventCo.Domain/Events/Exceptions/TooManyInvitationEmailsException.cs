using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class TooManyInvitationEmailsException : DomainException
{
    public Guid UserId { get; }
    public int MaxEmails { get; }
    public int WindowMinutes { get; }

    public TooManyInvitationEmailsException(Guid userId, int maxEmails, int windowMinutes)
        : base($"Trop d'invitations envoyées à cette personne (UserId: {userId}, max {maxEmails} par {windowMinutes} minutes).")
    {
        UserId = userId;
        MaxEmails = maxEmails;
        WindowMinutes = windowMinutes;
    }
}
