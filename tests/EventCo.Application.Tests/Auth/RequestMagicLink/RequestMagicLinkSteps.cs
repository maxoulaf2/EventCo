using EventCo.Application.Auth.RequestMagicLink;
using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Common.Options;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Auth.Exceptions;
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
        }));

        _serviceProvider = builder.Build();
        _dbContext = _serviceProvider.GetRequiredService<EventCoDbContext>();
    }

    [When(@"je demande un code de connexion pour ""(.*)""")]
    public async Task QuandJeDemandeUnCodeDeConnexionPour(string email)
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

    [Then(@"un code de connexion est enregistré pour ""(.*)"" expirant dans (\d+) minutes")]
    public void AlorsUnCodeEstEnregistrePourExpirantDans(string email, int minutes)
    {
        var token = _dbContext.MagicLinkTokens.Single(t => t.Email == email.ToLowerInvariant());

        Assert.Equal(_now.AddMinutes(minutes), token.ExpiresAt);
        Assert.Null(token.ConsumedAt);
        Assert.Equal(0, token.FailedAttempts);
    }

    [Then(@"un email est envoyé à ""(.*)"" contenant un code à 6 chiffres")]
    public void AlorsUnEmailEstEnvoyeAContenantUnCodeA6Chiffres(string email)
    {
        var sentEmail = Assert.Single(_emailSender.SentEmails);

        Assert.Equal(email.ToLowerInvariant(), sentEmail.ToEmail);
        Assert.Matches(@">\d{6}<", sentEmail.HtmlBody);
    }

    [Then(@"le code envoyé n'est pas stocké en clair")]
    public void AlorsLeCodeEnvoyeNestPasStockeEnClair()
    {
        var code = System.Text.RegularExpressions.Regex.Match(_emailSender.SentEmails.Single().HtmlBody, @">(\d{6})<").Groups[1].Value;
        var token = _dbContext.MagicLinkTokens.Single();

        Assert.NotEqual(code, token.TokenHash);
    }

    [Then(@"aucun email n'est envoyé")]
    public void AlorsAucunEmailNestEnvoye()
    {
        Assert.Empty(_emailSender.SentEmails);
    }

    [Then(@"la demande échoue avec une erreur de trop de requêtes")]
    public void AlorsLaDemandeEchoueAvecUneErreurDeTropDeRequetes()
    {
        Assert.IsType<TooManyMagicLinkRequestsException>(_thrownException);
    }

    [Then(@"exactement (\d+) emails ont été envoyés à ""(.*)""")]
    public void AlorsExactementEmailsOntEteEnvoyesA(int expectedCount, string email)
    {
        Assert.Equal(expectedCount, _emailSender.SentEmails.Count(e => e.ToEmail == email.ToLowerInvariant()));
    }
}
