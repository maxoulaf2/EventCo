using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.WithdrawDateVote;

public sealed record WithdrawDateVoteCommand(Guid EventId, Guid DateOptionId) : ICommand;
