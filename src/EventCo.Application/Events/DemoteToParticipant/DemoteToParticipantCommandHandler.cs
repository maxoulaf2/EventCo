using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.DemoteToParticipant;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class DemoteToParticipantCommandHandler(ICurrentUserService currentUserService, IEventRepository eventRepository)
    : ICommandHandler<DemoteToParticipantCommand>
{
    public async Task Handle(DemoteToParticipantCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.DemoteToParticipant(currentUserService.UserId!.Value, command.TargetUserId);

        await eventRepository.UpdateAsync(@event, cancellationToken);
    }
}
