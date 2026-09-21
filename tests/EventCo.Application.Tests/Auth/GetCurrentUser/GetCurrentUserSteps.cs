using EventCo.Application.Auth.GetCurrentUser;
using EventCo.Application.Auth.RequestMagicLink;
using EventCo.Application.Auth.VerifyMagicLink;
using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Common.Options;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Infrastructure.Auth;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reqnroll;

namespace EventCo.Application.Tests.Auth.GetCurrentUser;

[Binding]
public sealed class GetCurrentUserSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingEmailSender _emailSender = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private GetCurrentUserResult? _lastResult;
    private Exception? _thrownException;

    public GetCurrentUserSteps(CurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;

        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IMagicLinkTokenRepository, MagicLinkTokenRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddSingleton<IEmailSender>(_emailSender);
        builder.Services.AddSingleton<ISessionTokenService, SessionTokenService>();
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));
        builder.Services.AddSingleton(Options.Create(new MagicLinkOptions
        {
            ExpiryMinutes = 15,
            VerificationUrlBase = "http://localhost:5173/auth/verify",
        }));
        builder.Services.AddSingleton(Options.Create(new SessionOptions
        {
            Secret = "test-secret-not-for-production",
            ExpiryDays = 30,
        }));

        _serviceProvider = builder.Build();
    }

    [Given(@"un compte connecté pour ""(.*)""")]
    public async Task EtantDonneUnCompteConnectePour(string email)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        await dispatcher.Send(new RequestMagicLinkCommand(email), CancellationToken.None);
        var rawToken = ExtractRawToken(_emailSender.SentEmails.Last().HtmlBody);
        var verifyResult = await dispatcher.Send(new VerifyMagicLinkCommand(rawToken), CancellationToken.None);

        _currentUserContext.UserId = verifyResult.UserId;
    }

    [When(@"je consulte mes informations de profil")]
    public async Task JeConsulteMesInformationsDeProfil()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(new GetCurrentUserQuery(), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la consultation de mon profil réussit")]
    public void AlorsLaConsultationDeMonProfilReussit() => Assert.Null(_thrownException);

    [Then(@"mes informations de profil correspondent à ""(.*)""")]
    public void AlorsMesInformationsDeProfilCorrespondentA(string email)
    {
        Assert.Equal(_currentUserContext.UserId, _lastResult!.UserId);
        Assert.Equal(email, _lastResult.Email);
    }

    private static string ExtractRawToken(string emailHtmlBody)
    {
        var match = System.Text.RegularExpressions.Regex.Match(emailHtmlBody, @"token=([^""&]+)");
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }
}
