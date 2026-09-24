using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Options;
using EventCo.Infrastructure.Auth;
using EventCo.Infrastructure.Common;
using EventCo.Infrastructure.Emailing;
using EventCo.Infrastructure.Persistence;
using EventCo.Infrastructure.Persistence.Repositories;
using EventCo.Infrastructure.Realtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;

namespace EventCo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddDbContext<EventCoDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<ILoginCodeRepository, LoginCodeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<ISessionTokenService, SessionTokenService>();

        services.AddSignalR();
        services.AddSingleton<ITaskRealtimeNotifier, TaskRealtimeNotifier>();

        services.AddScoped<LoggingEmailSender>();
        services.AddScoped<SmtpEmailSender>();
        services.AddScoped<IEmailSender>(sp =>
        {
            var emailOptions = sp.GetRequiredService<IOptions<EmailOptions>>().Value;
            return string.IsNullOrWhiteSpace(emailOptions.Smtp.Host)
                ? sp.GetRequiredService<LoggingEmailSender>()
                : sp.GetRequiredService<SmtpEmailSender>();
        });

        services.Configure<LoginCodeOptions>(configuration.GetSection(LoginCodeOptions.SectionName));
        services.Configure<SessionOptions>(configuration.GetSection(SessionOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<InvitationOptions>(configuration.GetSection(InvitationOptions.SectionName));
        services.Configure<FrontendOptions>(configuration.GetSection(FrontendOptions.SectionName));

        return services;
    }
}
