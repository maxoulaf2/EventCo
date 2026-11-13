using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.VoteForDateOption;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class VoteForDateOptionCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository)
    : ICommandHandler<VoteForDateOptionCommand>
{
    public async Task Handle(VoteForDateOptionCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.VoteForDateOption(currentUserService.UserId!.Value, command.DateOptionId);

        await eventRepository.ApplyAsync(@event, cancellationToken);
    }
}
