using EventCo.Application.Common.Interfaces;

namespace EventCo.Application.Tests.Support;

// ICurrentUserService de test qui lit toujours la valeur courante de CurrentUserContext,
// pour que le step Given commun (CurrentUserSteps) puisse changer d'utilisateur en cours de scénario.
public sealed class CurrentUserContextService(CurrentUserContext currentUserContext) : ICurrentUserService
{
    public Guid? UserId => currentUserContext.UserId;

    public bool IsAuthenticated => true;
}
