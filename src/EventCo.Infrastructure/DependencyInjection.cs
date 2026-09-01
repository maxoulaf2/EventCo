using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Options;
using EventCo.Infrastructure.Auth;
using EventCo.Infrastructure.Common;
using EventCo.Infrastructure.Emailing;
using EventCo.Infrastructure.Persistence;
using EventCo.Infrastructure.Persistence.Repositories;
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

        services.AddScoped<IMagicLinkTokenRepository, MagicLinkTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        services.AddSingleton<ISessionTokenService, SessionTokenService>();

        services.Configure<MagicLinkOptions>(configuration.GetSection(MagicLinkOptions.SectionName));
        services.Configure<SessionOptions>(configuration.GetSection(SessionOptions.SectionName));

        return services;
    }
}
