using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Auth.RequestLoginCode;

public sealed record RequestLoginCodeCommand(string Email, string? EventInviteLinkToken = null) : ICommand;
