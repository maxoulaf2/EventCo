using EventCo.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EventCo.Application.Common.Messaging;

internal sealed class CommandDispatcher(
    IServiceProvider serviceProvider,
    IUnitOfWork unitOfWork,
    IDomainEventDispatcher domainEventDispatcher,
    DomainEventCollector domainEventCollector) : ICommandDispatcher
{
    public async Task Send<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : ICommand
    {
        await ValidateAsync(command, cancellationToken);

        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        await handler.Handle(command, cancellationToken);

        await CompleteUnitOfWorkAsync(cancellationToken);
    }

    public async Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken)
    {
        await ValidateAsync(command, cancellationToken);

        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResponse));
        var handler = serviceProvider.GetRequiredService(handlerType);

        var handleMethod = handlerType.GetMethod(nameof(ICommandHandler<ICommand<TResponse>, TResponse>.Handle))!;
        var response = await (Task<TResponse>)handleMethod.Invoke(handler, [command, cancellationToken])!;

        await CompleteUnitOfWorkAsync(cancellationToken);

        return response;
    }

    private async Task CompleteUnitOfWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await domainEventDispatcher.DispatchAsync(domainEventCollector.PendingEvents, cancellationToken);
        }
        finally
        {
            domainEventCollector.Clear();
        }
    }

    private async Task ValidateAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
    {
        var commandType = command!.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(commandType);
        var validators = serviceProvider.GetServices(validatorType).Cast<IValidator>().ToList();

        if (validators.Count == 0)
            return;

        var contextType = typeof(ValidationContext<>).MakeGenericType(commandType);
        var context = (IValidationContext)Activator.CreateInstance(contextType, command)!;

        var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);
    }
}
