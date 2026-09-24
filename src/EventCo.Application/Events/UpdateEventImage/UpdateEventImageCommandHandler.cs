using EventCo.Application.Common.Images;
using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.UpdateEventImage;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class UpdateEventImageCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    IFileStorage fileStorage) : ICommandHandler<UpdateEventImageCommand, UpdateEventImageResult>
{
    public async Task<UpdateEventImageResult> Handle(UpdateEventImageCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new EventNotFoundException(request.EventId);

        // Format garanti non nul par UpdateEventImageCommandValidator.
        var format = ImageFormat.Detect(request.Content)!;

        // Même principe que pour les avatars (cf. UpdateAvatarCommandHandler) : nouvelle clé à chaque
        // changement, jamais d'écrasement, pour contourner les caches d'un fichier servi publiquement.
        var key = $"{@event.Id:N}/{Guid.NewGuid():N}.{format.Extension}";
        var previousKey = @event.ImageStorageKey;

        // Autorisation vérifiée par l'agrégat avant l'upload : un utilisateur non autorisé ne doit pas
        // pouvoir déposer de fichier dans le bucket.
        @event.ChangeImage(currentUserService.UserId!.Value, key);

        await fileStorage.UploadAsync(FileStorageBucket.EventImages, key, request.Content, format.ContentType, cancellationToken);
        await eventRepository.ApplyAsync(@event, cancellationToken);

        if (previousKey is not null)
            await fileStorage.DeleteAsync(FileStorageBucket.EventImages, previousKey, cancellationToken);

        return new UpdateEventImageResult(@event.Id, fileStorage.GetEventImageUrl(@event)!);
    }
}
