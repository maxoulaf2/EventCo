using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.InviteParticipant;

public sealed record InviteParticipantCommand(Guid EventId, string Email) : ICommand<InviteParticipantResult>;
