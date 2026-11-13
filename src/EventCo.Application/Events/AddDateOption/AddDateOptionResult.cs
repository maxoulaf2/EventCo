namespace EventCo.Application.Events.AddDateOption;

public sealed record AddDateOptionResult(Guid DateOptionId, Guid EventId, DateTime Date);
