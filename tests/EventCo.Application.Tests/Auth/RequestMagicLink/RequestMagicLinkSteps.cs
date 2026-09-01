using EventCo.Application.Auth.RequestMagicLink;
using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Common.Options;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Infrastructure.Persistence;
using EventCo.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reqnroll;

namespace EventCo.Application.Tests.Auth.RequestMagicLink;

[Binding]
public sealed class RequestMagicLinkSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RecordingEmailSender _emailSender;
    private readonly EventCoDbContext _dbContext;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private Exception? _thrownException;

    public RequestMagicLinkSteps()
    {
        var builder = new ApplicationTestHostBuilder();
        _emailSender = new RecordingEmailSender();

        builder.Services.AddScoped<IMagicLinkTokenRepository, MagicLinkTokenRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddSingleton<IEmailSender>(_emailSender);
        builder.Services.AddSingleton(Options.Create(new MagicLinkOptions
        {
            ExpiryMinutes = 15,
            VerificationUrlBase = "http://localhost:5173/auth/verify",
        }));

        _serviceProvider = builder.Build();
        _dbContext = _serviceProvider.GetRequiredService<EventCoDbContext>();
    }

    [When(@"je demande un lien de connexion pour ""(.*)""")]
    public async Task QuandJeDemandeUnLienDeConnexionPour(string email)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            await dispatcher.Send(new RequestMagicLinkCommand(email), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la demande est acceptée")]
    public void AlorsLaDemandeEstAcceptee()
    {
        Assert.Null(_thrownException);
    }

    [Then(@"la demande échoue avec une erreur de validation")]
    public void AlorsLaDemandeEchoueAvecUneErreurDeValidation()
    {
        Assert.IsType<ValidationException>(_thrownException);
    }

    [Then(@"un token de connexion est enregistré pour ""(.*)"" expirant dans (\d+) minutes")]
    public void AlorsUnTokenEstEnregistrePourExpirantDans(string email, int minutes)
    {
        var token = _dbContext.MagicLinkTokens.Single(t => t.Email.Value == email.ToLowerInvariant());

        Assert.Equal(_now.AddMinutes(minutes), token.ExpiresAt);
        Assert.False(token.IsConsumed);
    }

    [Then(@"un email est envoyé à ""(.*)"" contenant un lien de vérification")]
    public void AlorsUnEmailEstEnvoyeAContenantUnLienDeVerification(string email)
    {
        var sentEmail = Assert.Single(_emailSender.SentEmails);

        Assert.Equal(email.ToLowerInvariant(), sentEmail.ToEmail);
        Assert.Contains("http://localhost:5173/auth/verify?token=", sentEmail.HtmlBody);
    }

    [Then(@"aucun email n'est envoyé")]
    public void AlorsAucunEmailNestEnvoye()
    {
        Assert.Empty(_emailSender.SentEmails);
    }
}
