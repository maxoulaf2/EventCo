namespace EventCo.Application.Common.Interfaces;

public interface ISessionTokenService
{
    SessionToken CreateSessionToken(Guid userId, string email, DateTime now);
}
