namespace EventCo.Api.Tests.Support;

// Injectée par Reqnroll (context injection) dans les classes [Binding] d'un même scénario,
// pour partager l'identifiant du participant invité via le step Given commun (InvitedParticipantSteps).
public sealed class InvitedParticipantContext
{
    public Guid? UserId { get; set; }
}
