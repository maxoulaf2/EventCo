namespace EventCo.Api.Tests.Support;

// Injectée par Reqnroll (context injection) dans les classes [Binding] d'un même scénario,
// pour partager l'identifiant de l'article posé par le step Given commun (ExistingItemSteps).
public sealed class ItemContext
{
    public Guid? ItemId { get; set; }
}
