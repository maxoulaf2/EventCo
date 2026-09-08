using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.PromoteToOrganizer;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class PromoteToOrganizerCommandHandler(ICurrentUserService currentUserService, IEventRepository eventRepository)
    : ICommandHandler<PromoteToOrganizerCommand>
{
    public async Task Handle(PromoteToOrganizerCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.PromoteToOrganizer(currentUserService.UserId!.Value, command.TargetUserId);

        await eventRepository.UpdateAsync(@event, cancellationToken);
    }
}
