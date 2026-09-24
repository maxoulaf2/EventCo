using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Auth.VerifyLoginCode;

public sealed record VerifyLoginCodeCommand(string Email, string Code) : ICommand<VerifyLoginCodeResult>;
