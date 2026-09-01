using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EventCo.Application.Common.Messaging;

internal sealed class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public async Task Send<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : ICommand
    {
        var validators = serviceProvider.GetServices<IValidator<TCommand>>().ToList();

        if (validators.Count > 0)
        {
            var context = new ValidationContext<TCommand>(command);
            var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken))))
                .SelectMany(result => result.Errors)
                .ToList();

            if (failures.Count > 0)
                throw new ValidationException(failures);
        }

        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        await handler.Handle(command, cancellationToken);
    }
}
