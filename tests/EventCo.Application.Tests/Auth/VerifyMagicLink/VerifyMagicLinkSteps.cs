using EventCo.Application.Auth.RequestMagicLink;
using EventCo.Application.Auth.VerifyMagicLink;
using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Common.Options;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.RegenerateEventInviteLink;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Auth.Exceptions;
using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;
using EventCo.Infrastructure.Auth;
using EventCo.Infrastructure.Persistence;
using EventCo.Infrastructure.Persistence.Entities;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reqnroll;

namespace EventCo.Application.Tests.Auth.VerifyMagicLink;

[Binding]
public sealed class VerifyMagicLinkSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RecordingEmailSender _emailSender;
    private readonly FixedDateTimeProvider _dateTimeProvider;
    private readonly EventCoDbContext _dbContext;
    private readonly FixedCurrentUserService _organizerCurrentUserService = new(Guid.NewGuid());
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private string? _lastRawToken;
    private VerifyMagicLinkResult? _lastResult;
    private Exception? _thrownException;
    private Guid? _eventId;
    private string? _eventInviteLinkToken;

    public VerifyMagicLinkSteps()
    {
        var builder = new ApplicationTestHostBuilder();
        _emailSender = new RecordingEmailSender();
        _dateTimeProvider = new FixedDateTimeProvider(_now);

        builder.Services.AddScoped<IMagicLinkTokenRepository, MagicLinkTokenRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<ICurrentUserService>(_ => _organizerCurrentUserService);
        builder.Services.AddSingleton<IDateTimeProvider>(_dateTimeProvider);
        builder.Services.AddSingleton<IEmailSender>(_emailSender);
        builder.Services.AddSingleton<ISessionTokenService, SessionTokenService>();
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
        _dbContext = _serviceProvider.GetRequiredService<EventCoDbContext>();
    }

    [When(@"un(?: nouveau)? lien de connexion est demandé pour ""(.*)""")]
    public async Task UnLienDeConnexionEstDemandePour(string email)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        await dispatcher.Send(new RequestMagicLinkCommand(email), CancellationToken.None);
        _lastRawToken = ExtractRawToken(_emailSender.SentEmails.Last().HtmlBody);
    }

    [Given(@"un événement ""(.*)"" avec un lien d'invitation actif")]
    public async Task EtantDonneUnEvenementAvecUnLienDinvitationActif(string title)
    {
        var userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
        var organizer = User.Create(Email.From("organisateur-invite-link@example.com"), "Organisateur", _now);
        await userRepository.ApplyAsync(organizer, CancellationToken.None);
        await _serviceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken.None);
        _organizerCurrentUserService.UserId = organizer.Id;

        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var createResult = await dispatcher.Send(new CreateEventCommand(title, null, _now.AddDays(1), null), CancellationToken.None);
        _eventId = createResult.EventId;

        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_eventId.Value, CancellationToken.None);
        _eventInviteLinkToken = @event!.InviteLinkToken;
    }

    [Given(@"un lien de connexion avec intention de rejoindre cet événement est demandé pour ""(.*)""")]
    [When(@"un lien de connexion avec intention de rejoindre cet événement est demandé pour ""(.*)""")]
    public async Task UnLienDeConnexionAvecIntentionDeRejoindreCetEvenementEstDemandePour(string email)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        await dispatcher.Send(new RequestMagicLinkCommand(email, _eventInviteLinkToken), CancellationToken.None);
        _lastRawToken = ExtractRawToken(_emailSender.SentEmails.Last().HtmlBody);
    }

    [Given(@"le lien d'invitation de cet événement est régénéré")]
    public async Task EtantDonneLeLienDinvitationDeCetEvenementEstRegenere()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        await dispatcher.Send(new RegenerateEventInviteLinkCommand(_eventId!.Value), CancellationToken.None);
    }

    [When(@"le temps avance de (\d+) minutes")]
    public void LeTempsAvanceDeMinutes(int minutes)
    {
        _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddMinutes(minutes);
    }

    [When(@"je valide le lien de connexion reçu")]
    [When(@"je valide à nouveau le même lien de connexion")]
    public async Task JeValideLeLienDeConnexionRecu()
    {
        await ValiderToken(_lastRawToken!);
    }

    [When(@"je valide le token ""(.*)""")]
    public async Task JeValideLeToken(string token)
    {
        await ValiderToken(token);
    }

    [Then(@"la validation réussit")]
    public void AlorsLaValidationReussit() => Assert.Null(_thrownException);

    [Then(@"la validation échoue avec une erreur de token invalide")]
    public void AlorsLaValidationEchoueAvecUneErreurDeTokenInvalide() =>
        Assert.IsType<MagicLinkTokenNotFoundException>(_thrownException);

    [Then(@"la validation échoue avec une erreur de token déjà utilisé")]
    public void AlorsLaValidationEchoueAvecUneErreurDeTokenDejaUtilise() =>
        Assert.IsType<MagicLinkTokenAlreadyConsumedException>(_thrownException);

    [Then(@"la validation échoue avec une erreur d'expiration")]
    public void AlorsLaValidationEchoueAvecUneErreurDexpiration() =>
        Assert.IsType<MagicLinkTokenExpiredException>(_thrownException);

    [Then(@"un compte est créé pour ""(.*)""")]
    public void AlorsUnCompteEstCreePour(string email)
    {
        var user = _dbContext.Users.Single(u => u.Email == email.ToLowerInvariant());
        Assert.Equal(_lastResult!.UserId, user.Id);
    }

    [Then(@"un seul compte existe pour ""(.*)""")]
    public void AlorsUnSeulCompteExistePour(string email)
    {
        var users = _dbContext.Users.Where(u => u.Email == email.ToLowerInvariant()).ToList();
        Assert.Single(users);
    }

    [Then(@"une session est ouverte pour ""(.*)""")]
    public void AlorsUneSessionEstOuvertePour(string email)
    {
        Assert.Equal(email.ToLowerInvariant(), _lastResult!.Email);
        Assert.False(string.IsNullOrWhiteSpace(_lastResult.SessionToken));
        Assert.True(_lastResult.SessionExpiresAt > _now);
    }

    [Then(@"le lien de connexion pour ""(.*)"" est marqué comme utilisé")]
    public void AlorsLeLienDeConnexionPourEstMarqueCommeUtilise(string email)
    {
        var token = _dbContext.MagicLinkTokens.Single(t => t.Email == email.ToLowerInvariant());
        Assert.NotNull(token.ConsumedAt);
    }

    [Then(@"je rejoins l'événement ""(.*)""")]
    public void AlorsJeRejoinsLevenement(string title)
    {
        Assert.Equal(_eventId, _lastResult!.EventId);

        var eventEntity = _dbContext.Events.Single(e => e.Id == _eventId);
        Assert.Equal(title, eventEntity.Title);

        var participant = _dbContext.Set<EventParticipantEntity>()
            .SingleOrDefault(p => p.EventId == _eventId && p.UserId == _lastResult.UserId);
        Assert.NotNull(participant);
    }

    [Then(@"je ne rejoins aucun événement")]
    public void AlorsJeNeRejoinsAucunEvenement() => Assert.Null(_lastResult!.EventId);

    private async Task ValiderToken(string token)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            _lastResult = await dispatcher.Send(new VerifyMagicLinkCommand(token), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    private static string ExtractRawToken(string emailHtmlBody)
    {
        var match = System.Text.RegularExpressions.Regex.Match(emailHtmlBody, @"token=([^""&]+)");
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }
}
