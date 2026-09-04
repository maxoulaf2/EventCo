namespace EventCo.Api.Tests.Support;

// Injectée par Reqnroll (context injection) dans les classes [Binding] d'un même scénario,
// pour partager l'identifiant de l'événement obtenu via le step Given commun (ExistingEventSteps).
public sealed class EventContext
{
    public Guid? EventId { get; set; }
}
