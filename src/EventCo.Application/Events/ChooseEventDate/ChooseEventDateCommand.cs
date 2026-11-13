using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.ChooseEventDate;

public sealed record ChooseEventDateCommand(Guid EventId, Guid DateOptionId) : ICommand;
