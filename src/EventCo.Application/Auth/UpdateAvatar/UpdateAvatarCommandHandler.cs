using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Auth.UpdateAvatar;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant : ce Command n'est
// jamais atteint pour une requête non authentifiée.
public sealed class UpdateAvatarCommandHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IFileStorage fileStorage) : ICommandHandler<UpdateAvatarCommand, UpdateAvatarResult>
{
    public async Task<UpdateAvatarResult> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(currentUserService.UserId!.Value, cancellationToken);

        // Format garanti non nul par UpdateAvatarCommandValidator.
        var format = AvatarImageFormat.Detect(request.Content)!;

        // Nouvelle clé à chaque changement (jamais d'écrasement) : l'URL change, ce qui contourne les
        // caches navigateur/CDN d'un fichier servi publiquement sous une URL stable.
        var key = $"avatars/{user!.Id:N}/{Guid.NewGuid():N}.{format.Extension}";
        var previousKey = user.AvatarStorageKey;

        await fileStorage.UploadAsync(key, request.Content, format.ContentType, cancellationToken);

        user.ChangeAvatar(key);
        await userRepository.ApplyAsync(user, cancellationToken);

        // Suppression avant la sauvegarde en base (faite par CommandDispatcher après ce handler) : si
        // celle-ci échouait, l'ancien avatar référencé en base serait perdu — cas jugé assez rare pour ne
        // pas justifier un mécanisme de nettoyage différé.
        if (previousKey is not null)
            await fileStorage.DeleteAsync(previousKey, cancellationToken);

        return new UpdateAvatarResult(user.Id, user.Email.Value, user.DisplayName, user.IsAdmin, fileStorage.GetAvatarUrl(user));
    }
}
