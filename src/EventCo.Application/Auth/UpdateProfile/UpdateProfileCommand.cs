using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Auth.UpdateProfile;

public sealed record UpdateProfileCommand(string DisplayName) : ICommand<UpdateProfileResult>;
