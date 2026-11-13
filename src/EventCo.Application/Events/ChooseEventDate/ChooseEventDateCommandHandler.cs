using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.ChooseEventDate;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class ChooseEventDateCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ChooseEventDateCommand>
{
    public async Task Handle(ChooseEventDateCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.ChooseDate(currentUserService.UserId!.Value, command.DateOptionId, dateTimeProvider.UtcNow);

        await eventRepository.ApplyAsync(@event, cancellationToken);
    }
}
