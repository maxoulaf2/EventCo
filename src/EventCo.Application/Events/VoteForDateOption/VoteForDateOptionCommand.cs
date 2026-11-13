using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.VoteForDateOption;

public sealed record VoteForDateOptionCommand(Guid EventId, Guid DateOptionId) : ICommand;
