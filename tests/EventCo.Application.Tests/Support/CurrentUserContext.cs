namespace EventCo.Application.Tests.Support;

// Injectée par Reqnroll (context injection) dans les classes [Binding] d'un même scénario,
// pour partager l'utilisateur courant mutable entre le step Given commun (CurrentUserSteps)
// et le ICurrentUserService de test enregistré par chaque classe de steps.
public sealed class CurrentUserContext
{
    public Guid UserId { get; set; } = Guid.NewGuid();
}
