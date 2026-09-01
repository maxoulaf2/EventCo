namespace EventCo.Application.Common.Messaging;

public interface ICommand;

// Volontairement indépendante de ICommand (pas d'héritage) : les deux implémentées sur un même type
// rendraient les deux surcharges de ICommandDispatcher.Send ambiguës à la résolution.
public interface ICommand<TResponse>;
