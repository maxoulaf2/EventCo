using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.Exceptions;

public sealed class LoginCodeHashEmptyException : DomainException
{
    public LoginCodeHashEmptyException() : base("Le hash du code de connexion ne peut pas être vide.")
    {
    }
}
