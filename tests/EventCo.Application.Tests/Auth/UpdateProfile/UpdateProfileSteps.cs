using EventCo.Application.Auth.RequestMagicLink;
using EventCo.Application.Auth.UpdateProfile;
using EventCo.Application.Auth.VerifyMagicLink;
using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Common.Options;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Infrastructure.Auth;
using EventCo.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reqnroll;

namespace EventCo.Application.Tests.Auth.UpdateProfile;

[Binding]
public sealed class UpdateProfileSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingEmailSender _emailSender = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private UpdateProfileResult? _lastResult;
    private Exception? _thrownException;

    public UpdateProfileSteps(CurrentUserContext currentUserContext)
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

    [Given(@"un compte existant pour ""(.*)""")]
    public async Task EtantDonneUnCompteExistantPour(string email)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        await dispatcher.Send(new RequestMagicLinkCommand(email), CancellationToken.None);
        var rawToken = ExtractRawToken(_emailSender.SentEmails.Last().HtmlBody);
        var verifyResult = await dispatcher.Send(new VerifyMagicLinkCommand(rawToken), CancellationToken.None);

        _currentUserContext.UserId = verifyResult.UserId;
    }

    [When(@"je modifie mon nom d'affichage en ""(.*)""")]
    public async Task JeModifieMonNomDaffichageEn(string displayName)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(new UpdateProfileCommand(displayName), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la modification du nom d'affichage réussit")]
    public void AlorsLaModificationDuNomDaffichageReussit() => Assert.Null(_thrownException);

    [Then(@"la modification du nom d'affichage échoue avec une erreur de validation")]
    public void AlorsLaModificationDuNomDaffichageEchoueAvecUneErreurDeValidation() =>
        Assert.IsType<ValidationException>(_thrownException);

    [Then(@"mon nom d'affichage est ""(.*)""")]
    public void AlorsMonNomDaffichageEst(string displayName) => Assert.Equal(displayName, _lastResult!.DisplayName);

    private static string ExtractRawToken(string emailHtmlBody)
    {
        var match = System.Text.RegularExpressions.Regex.Match(emailHtmlBody, @"token=([^""&]+)");
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }
}
