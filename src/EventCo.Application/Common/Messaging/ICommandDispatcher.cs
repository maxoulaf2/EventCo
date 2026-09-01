namespace EventCo.Application.Common.Messaging;

public interface ICommandDispatcher
{
    Task Send<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : ICommand;

    Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken);
}
