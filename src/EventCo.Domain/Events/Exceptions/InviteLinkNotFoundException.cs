using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class InviteLinkNotFoundException : DomainException
{
    public string Token { get; }

    public InviteLinkNotFoundException(string token)
        : base("Aucun événement ne correspond à ce lien d'invitation.")
    {
        Token = token;
    }
}
