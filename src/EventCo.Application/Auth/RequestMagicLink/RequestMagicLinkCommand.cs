using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Auth.RequestMagicLink;

public sealed record RequestMagicLinkCommand(string Email) : ICommand;
