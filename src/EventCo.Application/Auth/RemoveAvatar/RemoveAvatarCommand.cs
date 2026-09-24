using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Auth.RemoveAvatar;

public sealed record RemoveAvatarCommand : ICommand<RemoveAvatarResult>;
