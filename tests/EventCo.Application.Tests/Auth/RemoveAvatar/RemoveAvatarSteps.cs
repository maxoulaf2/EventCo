using EventCo.Application.Auth.GetCurrentUser;
using EventCo.Application.Auth.RemoveAvatar;
using EventCo.Application.Auth.RequestLoginCode;
using EventCo.Application.Auth.UpdateAvatar;
using EventCo.Application.Auth.VerifyLoginCode;
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

namespace EventCo.Application.Tests.Auth.RemoveAvatar;

[Binding]
public sealed class RemoveAvatarSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingEmailSender _emailSender = new();
    private readonly InMemoryFileStorage _fileStorage = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private RemoveAvatarResult? _lastResult;
    private Exception? _thrownException;

    public RemoveAvatarSteps(CurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;

        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<ILoginCodeRepository, LoginCodeRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddSingleton<IEmailSender>(_emailSender);
        builder.Services.AddSingleton<IFileStorage>(_fileStorage);
        builder.Services.AddSingleton<ISessionTokenService, SessionTokenService>();
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));
        builder.Services.AddSingleton(Options.Create(new LoginCodeOptions
        {
            ExpiryMinutes = 15,
        }));
        builder.Services.AddSingleton(Options.Create(new SessionOptions
        {
            Secret = "test-secret-not-for-production",
            ExpiryDays = 30,
        }));

        _serviceProvider = builder.Build();
    }

    [Given(@"un compte n'ayant jamais eu de photo de profil pour ""(.*)""")]
    public async Task EtantDonneUnCompteNayantJamaisEuDePhotoDeProfilPour(string email)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        await dispatcher.Send(new RequestLoginCodeCommand(email), CancellationToken.None);
        var code = ExtractCode(_emailSender.SentEmails.Last().HtmlBody);
        var verifyResult = (VerifyLoginCodeResult.Succeeded)await dispatcher.Send(new VerifyLoginCodeCommand(email, code), CancellationToken.None);

        _currentUserContext.UserId = verifyResult.UserId;
    }

    [Given(@"un compte ayant une photo de profil pour ""(.*)""")]
    public async Task EtantDonneUnCompteAyantUnePhotoDeProfilPour(string email)
    {
        await EtantDonneUnCompteNayantJamaisEuDePhotoDeProfilPour(email);

        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        await dispatcher.Send(new UpdateAvatarCommand(TestImages.Png), CancellationToken.None);
    }

    [When(@"je supprime ma photo de profil")]
    public async Task QuandJeSupprimeMaPhotoDeProfil()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(new RemoveAvatarCommand(), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la suppression de la photo de profil réussit")]
    public void AlorsLaSuppressionDeLaPhotoDeProfilReussit() => Assert.Null(_thrownException);

    // Relu via GetCurrentUser plutôt que depuis le seul résultat de la commande, pour vérifier que la
    // suppression a bien été persistée.
    [Then(@"je n'ai plus de photo de profil")]
    public async Task AlorsJeNaiPlusDePhotoDeProfil()
    {
        Assert.Null(_lastResult!.AvatarUrl);

        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var currentUser = await dispatcher.Send(new GetCurrentUserQuery(), CancellationToken.None);
        Assert.Null(currentUser.AvatarUrl);
    }

    [Then(@"le fichier de ma photo de profil a été supprimé du stockage")]
    public void AlorsLeFichierDeMaPhotoDeProfilAEteSupprimeDuStockage() => Assert.Empty(_fileStorage.Files);

    private static string ExtractCode(string emailHtmlBody) =>
        System.Text.RegularExpressions.Regex.Match(emailHtmlBody, @">(\d{6})<").Groups[1].Value;
}
