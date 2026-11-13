using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.AddDateOption;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class AddDateOptionCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<AddDateOptionCommand, AddDateOptionResult>
{
    public async Task<AddDateOptionResult> Handle(AddDateOptionCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        var option = @event.AddDateOption(currentUserService.UserId!.Value, command.Date, dateTimeProvider.UtcNow);

        await eventRepository.ApplyAsync(@event, cancellationToken);

        return new AddDateOptionResult(option.Id, @event.Id, option.Date);
    }
}
