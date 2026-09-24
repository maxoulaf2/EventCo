using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Auth.RemoveAvatar;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant : ce Command n'est
// jamais atteint pour une requête non authentifiée.
public sealed class RemoveAvatarCommandHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IFileStorage fileStorage) : ICommandHandler<RemoveAvatarCommand, RemoveAvatarResult>
{
    public async Task<RemoveAvatarResult> Handle(RemoveAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(currentUserService.UserId!.Value, cancellationToken);
        var previousKey = user!.AvatarStorageKey;

        user.RemoveAvatar();
        await userRepository.ApplyAsync(user, cancellationToken);

        if (previousKey is not null)
            await fileStorage.DeleteAsync(previousKey, cancellationToken);

        return new RemoveAvatarResult(user.Id, user.Email.Value, user.DisplayName, user.IsAdmin, AvatarUrl: null);
    }
}
