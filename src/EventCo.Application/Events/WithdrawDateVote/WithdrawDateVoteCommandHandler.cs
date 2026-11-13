using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.WithdrawDateVote;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class WithdrawDateVoteCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository)
    : ICommandHandler<WithdrawDateVoteCommand>
{
    public async Task Handle(WithdrawDateVoteCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.WithdrawDateVote(currentUserService.UserId!.Value, command.DateOptionId);

        await eventRepository.ApplyAsync(@event, cancellationToken);
    }
}
