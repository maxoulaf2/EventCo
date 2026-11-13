using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.AddDateOption;

public sealed record AddDateOptionCommand(Guid EventId, DateTime Date) : ICommand<AddDateOptionResult>;
