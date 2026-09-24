using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Auth.UpdateAvatar;

public sealed record UpdateAvatarCommand(byte[] Content) : ICommand<UpdateAvatarResult>;
