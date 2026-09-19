using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Auth.UpdateProfile;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant : ce Command n'est
// jamais atteint pour une requête non authentifiée.
public sealed class UpdateProfileCommandHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository) : ICommandHandler<UpdateProfileCommand, UpdateProfileResult>
{
    public async Task<UpdateProfileResult> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(currentUserService.UserId!.Value, cancellationToken);

        user!.UpdateProfile(request.DisplayName, user.AvatarUrl);

        await userRepository.ApplyAsync(user, cancellationToken);

        return new UpdateProfileResult(user.Id, user.Email.Value, user.DisplayName);
    }
}
