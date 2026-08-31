using EventCo.Application.Common.Interfaces;
using EventCo.Infrastructure.Common;
using EventCo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventCo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddDbContext<EventCoDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        return services;
    }
}
