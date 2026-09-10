using EventCo.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace EventCo.Application.Common.Messaging;

internal sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle))!;

            foreach (var handler in serviceProvider.GetServices(handlerType))
                await (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
        }
    }
}
