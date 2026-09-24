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
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reqnroll;

namespace EventCo.Application.Tests.Auth.UpdateAvatar;

[Binding]
public sealed class UpdateAvatarSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingEmailSender _emailSender = new();
    private readonly InMemoryFileStorage _fileStorage = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private UpdateAvatarResult? _previousResult;
    private UpdateAvatarResult? _lastResult;
    private Exception? _thrownException;

    public UpdateAvatarSteps(CurrentUserContext currentUserContext)
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

    [Given(@"un compte sans photo de profil pour ""(.*)""")]
    public async Task EtantDonneUnCompteSansPhotoDeProfilPour(string email)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        await dispatcher.Send(new RequestLoginCodeCommand(email), CancellationToken.None);
        var code = ExtractCode(_emailSender.SentEmails.Last().HtmlBody);
        var verifyResult = (VerifyLoginCodeResult.Succeeded)await dispatcher.Send(new VerifyLoginCodeCommand(email, code), CancellationToken.None);

        _currentUserContext.UserId = verifyResult.UserId;
    }

    [Given(@"j'ai déjà envoyé une image ""(.*)"" comme photo de profil")]
    public async Task EtantDonneJaiDejaEnvoyeUneImageCommePhotoDeProfil(string format)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        _previousResult = await dispatcher.Send(new UpdateAvatarCommand(TestImages.ByFormat(format)), CancellationToken.None);
    }

    [When(@"j'envoie une image ""(.*)"" comme photo de profil")]
    public async Task QuandJenvoieUneImageCommePhotoDeProfil(string format) =>
        await EnvoyerPhoto(TestImages.ByFormat(format));

    [When(@"j'envoie une image PNG de 3 Mo comme photo de profil")]
    public async Task QuandJenvoieUneImagePngDe3MoCommePhotoDeProfil()
    {
        var content = new byte[3 * 1024 * 1024];
        TestImages.Png.CopyTo(content, 0);

        await EnvoyerPhoto(content);
    }

    [Then(@"l'envoi de la photo de profil réussit")]
    public void AlorsLenvoiDeLaPhotoDeProfilReussit() => Assert.Null(_thrownException);

    [Then(@"l'envoi de la photo de profil échoue avec une erreur de validation")]
    public void AlorsLenvoiDeLaPhotoDeProfilEchoueAvecUneErreurDeValidation() =>
        Assert.IsType<ValidationException>(_thrownException);

    [Then(@"ma photo de profil est accessible à une URL publique")]
    public void AlorsMaPhotoDeProfilEstAccessibleAUneUrlPublique()
    {
        var key = Assert.Single(_fileStorage.Files.Keys);
        Assert.Equal(_fileStorage.GetPublicUrl(key), _lastResult!.AvatarUrl);
    }

    [Then(@"ma photo de profil a changé d'URL")]
    public void AlorsMaPhotoDeProfilAChangeDurl()
    {
        Assert.NotNull(_lastResult!.AvatarUrl);
        Assert.NotEqual(_previousResult!.AvatarUrl, _lastResult.AvatarUrl);
    }

    [Then(@"le stockage contient (\d+) fichiers? de type ""(.*)""")]
    public void AlorsLeStockageContientFichiersDeType(int count, string contentType)
    {
        Assert.Equal(count, _fileStorage.Files.Count);
        Assert.All(_fileStorage.Files.Values, file => Assert.Equal(contentType, file.ContentType));
    }

    [Then(@"le stockage ne contient aucun fichier")]
    public void AlorsLeStockageNeContientAucunFichier() => Assert.Empty(_fileStorage.Files);

    private async Task EnvoyerPhoto(byte[] content)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(new UpdateAvatarCommand(content), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    private static string ExtractCode(string emailHtmlBody) =>
        System.Text.RegularExpressions.Regex.Match(emailHtmlBody, @">(\d{6})<").Groups[1].Value;
}
