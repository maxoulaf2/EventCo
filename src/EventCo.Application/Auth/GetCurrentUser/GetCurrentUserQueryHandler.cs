using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Auth.GetCurrentUser;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant : cette query n'est
// jamais atteinte pour une requête non authentifiée.
public sealed class GetCurrentUserQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository) : ICommandHandler<GetCurrentUserQuery, GetCurrentUserResult>
{
    public async Task<GetCurrentUserResult> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(currentUserService.UserId!.Value, cancellationToken);

        return new GetCurrentUserResult(user!.Id, user.Email.Value, user.DisplayName);
    }
}
