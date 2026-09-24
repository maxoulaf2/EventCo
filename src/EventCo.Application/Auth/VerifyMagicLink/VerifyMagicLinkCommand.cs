using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Auth.VerifyMagicLink;

public sealed record VerifyMagicLinkCommand(string Email, string Code) : ICommand<VerifyMagicLinkResult>;
