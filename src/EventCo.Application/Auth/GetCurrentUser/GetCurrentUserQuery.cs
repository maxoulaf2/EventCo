using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Auth.GetCurrentUser;

public sealed record GetCurrentUserQuery : ICommand<GetCurrentUserResult>;
