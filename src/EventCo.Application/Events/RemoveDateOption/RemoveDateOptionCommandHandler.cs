using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.RemoveDateOption;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class RemoveDateOptionCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository)
    : ICommandHandler<RemoveDateOptionCommand>
{
    public async Task Handle(RemoveDateOptionCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.RemoveDateOption(currentUserService.UserId!.Value, command.DateOptionId);

        await eventRepository.ApplyAsync(@event, cancellationToken);
    }
}
