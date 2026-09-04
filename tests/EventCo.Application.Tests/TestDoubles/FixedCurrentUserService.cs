using EventCo.Application.Common.Interfaces;

namespace EventCo.Application.Tests.TestDoubles;

public sealed class FixedCurrentUserService(Guid? userId) : ICurrentUserService
{
    public Guid? UserId { get; set; } = userId;

    public bool IsAuthenticated => UserId is not null;
}
