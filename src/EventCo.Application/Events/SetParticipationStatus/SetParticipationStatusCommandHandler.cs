using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.SetParticipationStatus;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class SetParticipationStatusCommandHandler(ICurrentUserService currentUserService, IEventRepository eventRepository)
    : ICommandHandler<SetParticipationStatusCommand>
{
    public async Task Handle(SetParticipationStatusCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        var status = Enum.Parse<ParticipationStatus>(command.Status);

        @event.SetParticipationStatus(currentUserService.UserId!.Value, status);

        await eventRepository.ApplyAsync(@event, cancellationToken);
    }
}
