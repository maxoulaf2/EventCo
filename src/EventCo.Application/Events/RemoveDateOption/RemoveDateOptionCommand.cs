using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.RemoveDateOption;

public sealed record RemoveDateOptionCommand(Guid EventId, Guid DateOptionId) : ICommand;
