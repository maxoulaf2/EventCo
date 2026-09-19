using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Auth.RequestMagicLink;

public sealed record RequestMagicLinkCommand(string Email, string? EventInviteLinkToken = null) : ICommand;
