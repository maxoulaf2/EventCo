using EventCo.Application.Common.Interfaces;
using EventCo.Infrastructure.Common;
using Microsoft.Extensions.DependencyInjection;

namespace EventCo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
