namespace EventCo.Application.Auth.VerifyLoginCode;

// Un code erroné est un résultat (Invalid), pas une exception : l'essai raté doit être persisté
// (compteur anti force brute, cf. LoginCode.RegisterFailedAttempt), or une exception
// court-circuite le SaveChangesAsync unique de fin de requête (cf. CommandDispatcher).
public abstract record VerifyLoginCodeResult
{
    public sealed record Succeeded(
        Guid UserId,
        string Email,
        string DisplayName,
        string SessionToken,
        DateTime SessionExpiresAt,
        Guid? EventId) : VerifyLoginCodeResult;

    public sealed record Invalid : VerifyLoginCodeResult;
}
