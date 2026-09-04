namespace EventCo.Api.Tests.Support;

// Injectée par Reqnroll (context injection) dans les classes [Binding] d'un même scénario,
// pour partager le cookie de session obtenu via le step Given commun (AuthenticatedSessionSteps).
public sealed class SessionContext
{
    public string? Cookie { get; set; }
}
