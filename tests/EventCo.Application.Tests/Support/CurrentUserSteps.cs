using Reqnroll;

namespace EventCo.Application.Tests.Support;

[Binding]
public sealed class CurrentUserSteps(CurrentUserContext currentUserContext)
{
    [Given(@"je change d'utilisateur courant")]
    public void EtantDonneJeChangeDutilisateurCourant() => currentUserContext.UserId = Guid.NewGuid();
}
