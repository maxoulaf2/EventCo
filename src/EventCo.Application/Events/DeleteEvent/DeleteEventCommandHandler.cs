using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.DeleteEvent;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class DeleteEventCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    IFileStorage fileStorage) : ICommandHandler<DeleteEventCommand>
{
    public async Task Handle(DeleteEventCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.EnsureCanBeDeletedBy(currentUserService.UserId!.Value);

        await eventRepository.DeleteAsync(@event, cancellationToken);

        // Best effort, comme le remplacement d'une image (cf. IFileStorage.DeleteAsync).
        if (@event.ImageStorageKey is not null)
            await fileStorage.DeleteAsync(FileStorageBucket.EventImages, @event.ImageStorageKey, cancellationToken);
    }
}
