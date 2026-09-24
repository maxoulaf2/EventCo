using System.Text.RegularExpressions;
using EventCo.Application.Auth.RequestLoginCode;
using EventCo.Application.Auth.VerifyLoginCode;
using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Common.Options;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.RegenerateEventInviteLink;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;
using EventCo.Infrastructure.Auth;
using EventCo.Infrastructure.Persistence;
using EventCo.Infrastructure.Persistence.Entities;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reqnroll;

namespace EventCo.Application.Tests.Auth.VerifyLoginCode;

[Binding]
public sealed class VerifyLoginCodeSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RecordingEmailSender _emailSender;
    private readonly FixedDateTimeProvider _dateTimeProvider;
    private readonly EventCoDbContext _dbContext;
    private readonly FixedCurrentUserService _organizerCurrentUserService = new(Guid.NewGuid());
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly List<string> _receivedCodes = [];

    private string? _lastEmail;
    private VerifyLoginCodeResult? _lastResult;
    private Guid? _eventId;
    private string? _eventInviteLinkToken;

    public VerifyLoginCodeSteps()
    {
        var builder = new ApplicationTestHostBuilder();
        _emailSender = new RecordingEmailSender();
        _dateTimeProvider = new FixedDateTimeProvider(_now);

        builder.Services.AddScoped<ILoginCodeRepository, LoginCodeRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<ICurrentUserService>(_ => _organizerCurrentUserService);
        builder.Services.AddSingleton<IDateTimeProvider>(_dateTimeProvider);
        builder.Services.AddSingleton<IEmailSender>(_emailSender);
        builder.Services.AddSingleton<ISessionTokenService, SessionTokenService>();
        builder.Services.AddSingleton(Options.Create(new LoginCodeOptions { ExpiryMinutes = 15 }));
        builder.Services.AddSingleton(Options.Create(new SessionOptions
        {
            Secret = "test-secret-not-for-production",
            ExpiryDays = 30,
        }));

        _serviceProvider = builder.Build();
        _dbContext = _serviceProvider.GetRequiredService<EventCoDbContext>();
    }

    private VerifyLoginCodeResult.Succeeded SucceededResult => Assert.IsType<VerifyLoginCodeResult.Succeeded>(_lastResult);

    [When(@"un(?: nouveau)? code de connexion est demandé pour ""(.*)""")]
    public async Task UnCodeDeConnexionEstDemandePour(string email)
    {
        await DemanderCode(email, eventInviteLinkToken: null);
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

    [Given(@"un code de connexion avec intention de rejoindre cet événement est demandé pour ""(.*)""")]
    [When(@"un code de connexion avec intention de rejoindre cet événement est demandé pour ""(.*)""")]
    public async Task UnCodeDeConnexionAvecIntentionDeRejoindreCetEvenementEstDemandePour(string email)
    {
        await DemanderCode(email, _eventInviteLinkToken);
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

    [When(@"je saisis le code de connexion reçu")]
    [When(@"je saisis à nouveau le même code de connexion")]
    public async Task JeSaisisLeCodeDeConnexionRecu()
    {
        await SaisirCode(_lastEmail!, _receivedCodes.Last());
    }

    [When(@"je saisis le premier code de connexion reçu")]
    public async Task JeSaisisLePremierCodeDeConnexionRecu()
    {
        await SaisirCode(_lastEmail!, _receivedCodes.First());
    }

    [When(@"je saisis le code de connexion reçu pour l'email ""(.*)""")]
    public async Task JeSaisisLeCodeDeConnexionRecuPourLemail(string email)
    {
        await SaisirCode(email, _receivedCodes.Last());
    }

    [When(@"je saisis le code ""(.*)"" pour ""(.*)""")]
    public async Task JeSaisisLeCodePour(string code, string email)
    {
        await SaisirCode(email, code);
    }

    [When(@"je saisis un code erroné")]
    public async Task JeSaisisUnCodeErrone()
    {
        await SaisirCode(_lastEmail!, CodeErrone());
    }

    [When(@"je saisis (\d+) fois un code erroné")]
    public async Task JeSaisisFoisUnCodeErrone(int count)
    {
        for (var i = 0; i < count; i++)
            await SaisirCode(_lastEmail!, CodeErrone());
    }

    [Then(@"la validation réussit")]
    public void AlorsLaValidationReussit() => Assert.IsType<VerifyLoginCodeResult.Succeeded>(_lastResult);

    [Then(@"la validation échoue avec un code invalide")]
    public void AlorsLaValidationEchoueAvecUnCodeInvalide() => Assert.IsType<VerifyLoginCodeResult.Invalid>(_lastResult);

    [Then(@"(\d+) essai erroné est comptabilisé sur le code de connexion pour ""(.*)""")]
    public void AlorsEssaiErroneEstComptabilisePour(int expectedFailedAttempts, string email)
    {
        var token = _dbContext.LoginCodes.Single(t => t.Email == email.ToLowerInvariant());
        Assert.Equal(expectedFailedAttempts, token.FailedAttempts);
        Assert.Null(token.ConsumedAt);
    }

    [Then(@"un compte est créé pour ""(.*)""")]
    public void AlorsUnCompteEstCreePour(string email)
    {
        var user = _dbContext.Users.Single(u => u.Email == email.ToLowerInvariant());
        Assert.Equal(SucceededResult.UserId, user.Id);
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
        var result = SucceededResult;
        Assert.Equal(email.ToLowerInvariant(), result.Email);
        Assert.False(string.IsNullOrWhiteSpace(result.SessionToken));
        Assert.True(result.SessionExpiresAt > _now);
    }

    [Then(@"le code de connexion pour ""(.*)"" est marqué comme utilisé")]
    public void AlorsLeCodeDeConnexionPourEstMarqueCommeUtilise(string email)
    {
        var token = _dbContext.LoginCodes.Single(t => t.Email == email.ToLowerInvariant());
        Assert.NotNull(token.ConsumedAt);
    }

    [Then(@"je rejoins l'événement ""(.*)""")]
    public void AlorsJeRejoinsLevenement(string title)
    {
        var result = SucceededResult;
        Assert.Equal(_eventId, result.EventId);

        var eventEntity = _dbContext.Events.Single(e => e.Id == _eventId);
        Assert.Equal(title, eventEntity.Title);

        var participant = _dbContext.Set<EventParticipantEntity>()
            .SingleOrDefault(p => p.EventId == _eventId && p.UserId == result.UserId);
        Assert.NotNull(participant);
    }

    [Then(@"je ne rejoins aucun événement")]
    public void AlorsJeNeRejoinsAucunEvenement() => Assert.Null(SucceededResult.EventId);

    private async Task DemanderCode(string email, string? eventInviteLinkToken)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        await dispatcher.Send(new RequestLoginCodeCommand(email, eventInviteLinkToken), CancellationToken.None);
        _lastEmail = email;
        _receivedCodes.Add(Regex.Match(_emailSender.SentEmails.Last().HtmlBody, @">(\d{6})<").Groups[1].Value);
    }

    private async Task SaisirCode(string email, string code)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _lastResult = await dispatcher.Send(new VerifyLoginCodeCommand(email, code), CancellationToken.None);
    }

    // Premier code à 6 chiffres différent de tous ceux reçus : garantit un code erroné quel que soit le tirage.
    private string CodeErrone() =>
        Enumerable.Range(0, 1_000_000).Select(n => n.ToString("D6")).First(code => !_receivedCodes.Contains(code));
}
