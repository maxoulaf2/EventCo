using System.Globalization;
using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.AddDateOption;
using EventCo.Application.Events.ChooseEventDate;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.GetMyEvents;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Events.RemoveDateOption;
using EventCo.Application.Events.UpdateEvent;
using EventCo.Application.Events.VoteForDateOption;
using EventCo.Application.Events.WithdrawDateVote;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.DatePoll;

// Une seule classe de steps pour tous les use cases du sondage de date (création, options, votes,
// clôture) : ils partagent le même événement de départ et les mêmes vérifications.
[Binding]
public sealed class DatePollSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly Guid _organizerUserId;
    private readonly RecordingEmailSender _emailSender = new();
    private readonly RecordingDatePollRealtimeNotifier _realtimeNotifier = new();

    private Guid? _eventId;
    private Guid? _participantUserId;
    private Exception? _thrownException;
    private GetMyEventsResult? _myEvents;

    public DatePollSteps(CurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;
        _organizerUserId = currentUserContext.UserId;

        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)));
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));
        builder.Services.AddSingleton<IEmailSender>(_emailSender);
        builder.Services.AddSingleton<IDatePollRealtimeNotifier>(_realtimeNotifier);

        _serviceProvider = builder.Build();
    }

    private ICommandDispatcher Dispatcher => _serviceProvider.GetRequiredService<ICommandDispatcher>();

    private static DateTime Date(string date) =>
        DateTime.SpecifyKind(DateTime.Parse(date.Trim(), CultureInfo.InvariantCulture), DateTimeKind.Utc);

    private static List<DateTime> Dates(string dates) => dates.Split(',').Select(Date).ToList();

    private async Task Try(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    private async Task<Event> LoadEvent() =>
        (await _serviceProvider.GetRequiredService<IEventRepository>().GetByIdAsync(_eventId!.Value, CancellationToken.None))!;

    private async Task<Guid> OptionId(string date) => (await LoadEvent()).DateOptions.Single(o => o.Date == Date(date)).Id;

    [Given(@"un événement avec un sondage proposant les dates ""(.*)""")]
    [When(@"je crée un événement avec un sondage proposant les dates ""(.*)""")]
    public Task JeCreeUnEvenementAvecUnSondage(string dates) => Creer(null, Dates(dates));

    [When(@"je crée un événement avec la date ""(.*)"" et un sondage proposant les dates ""(.*)""")]
    public Task JeCreeUnEvenementAvecUneDateEtUnSondage(string date, string dates) => Creer(Date(date), Dates(dates));

    [When(@"je crée un événement sans date ni sondage")]
    public Task JeCreeUnEvenementSansDateNiSondage() => Creer(null, null);

    [Given(@"un événement à date fixe ""(.*)""")]
    public async Task UnEvenementADateFixe(string date) =>
        await Dispatcher.Send(new CreateEventCommand("Repas", null, Date(date), "Paris"), CancellationToken.None);

    private Task Creer(DateTime? eventDate, List<DateTime>? dateOptions) => Try(async () =>
    {
        var result = await Dispatcher.Send(new CreateEventCommand("Week-end", null, eventDate, "Lyon", dateOptions), CancellationToken.None);
        _eventId = result.EventId;
    });

    [Given(@"un participant ""(.*)"" invité au sondage")]
    [When(@"j'invite ""(.*)"" au sondage")]
    public async Task UnParticipantInviteAuSondage(string email)
    {
        var result = await Dispatcher.Send(new InviteParticipantCommand(_eventId!.Value, email), CancellationToken.None);
        _participantUserId = result.UserId;
    }

    [Given(@"je suis le participant invité au sondage")]
    public void JeSuisLeParticipantInviteAuSondage() => _currentUserContext.UserId = _participantUserId!.Value;

    [Given(@"je suis l'organisateur du sondage")]
    public void JeSuisLorganisateurDuSondage() => _currentUserContext.UserId = _organizerUserId;

    [Given(@"je vote pour la date ""(.*)""")]
    [When(@"je vote pour la date ""(.*)""")]
    public Task JeVotePourLaDate(string date) => Try(async () =>
        await Dispatcher.Send(new VoteForDateOptionCommand(_eventId!.Value, await OptionId(date)), CancellationToken.None));

    [When(@"je retire mon vote pour la date ""(.*)""")]
    public Task JeRetireMonVotePourLaDate(string date) => Try(async () =>
        await Dispatcher.Send(new WithdrawDateVoteCommand(_eventId!.Value, await OptionId(date)), CancellationToken.None));

    [When(@"j'ajoute l'option de date ""(.*)""")]
    public Task JajouteLoptionDeDate(string date) => Try(async () =>
        await Dispatcher.Send(new AddDateOptionCommand(_eventId!.Value, Date(date)), CancellationToken.None));

    [When(@"je supprime l'option de date ""(.*)""")]
    public Task JeSupprimeLoptionDeDate(string date) => Try(async () =>
        await Dispatcher.Send(new RemoveDateOptionCommand(_eventId!.Value, await OptionId(date)), CancellationToken.None));

    [Given(@"je retiens la date ""(.*)""")]
    [When(@"je retiens la date ""(.*)""")]
    public Task JeRetiensLaDate(string date) => Try(async () =>
        await Dispatcher.Send(new ChooseEventDateCommand(_eventId!.Value, await OptionId(date)), CancellationToken.None));

    [When(@"je renomme l'événement en sondage ""(.*)""")]
    public Task JeRenommeLevenementEnSondage(string title) => Try(async () =>
        await Dispatcher.Send(new UpdateEventCommand(_eventId!.Value, title, null, null, "Lyon"), CancellationToken.None));

    [When(@"je fixe à la main la date ""(.*)"" de l'événement en sondage")]
    public Task JeFixeALaMainLaDate(string date) => Try(async () =>
        await Dispatcher.Send(new UpdateEventCommand(_eventId!.Value, "Week-end", null, Date(date), "Lyon"), CancellationToken.None));

    [When(@"je consulte mes événements")]
    public async Task JeConsulteMesEvenements() =>
        _myEvents = await Dispatcher.Send(new GetMyEventsQuery(), CancellationToken.None);

    [Then(@"l'action sur le sondage réussit")]
    public void LactionSurLeSondageReussit() => Assert.Null(_thrownException);

    [Then(@"l'action sur le sondage échoue avec l'erreur ""(.*)""")]
    public void LactionSurLeSondageEchoueAvecLerreur(string exceptionTypeName) =>
        Assert.Equal(exceptionTypeName, _thrownException?.GetType().Name);

    [Then(@"l'événement en sondage n'a pas encore de date")]
    public async Task LevenementEnSondageNaPasEncoreDeDate() => Assert.Null((await LoadEvent()).EventDate);

    [Then(@"l'événement en sondage a pour date ""(.*)""")]
    public async Task LevenementEnSondageAPourDate(string date) => Assert.Equal(Date(date), (await LoadEvent()).EventDate);

    [Then(@"le sondage propose (\d+) options")]
    public async Task LeSondagePropose(int count) => Assert.Equal(count, (await LoadEvent()).DateOptions.Count);

    [Then(@"la date ""(.*)"" a (\d+) votes?")]
    public async Task LaDateADesVotes(string date, int count) =>
        Assert.Equal(count, (await LoadEvent()).DateOptions.Single(o => o.Date == Date(date)).VoterUserIds.Count);

    [Then(@"le participant invité au sondage a le statut de participation ""(.*)""")]
    public async Task LeParticipantInviteAuSondageALeStatut(string status) =>
        Assert.Equal(status, (await LoadEvent()).Participants.Single(p => p.UserId == _participantUserId!.Value).ParticipationStatus.ToString());

    [Then(@"un changement du sondage est diffusé en temps réel")]
    public void UnChangementDuSondageEstDiffuseEnTempsReel() => Assert.Contains(_eventId!.Value, _realtimeNotifier.ChangedEventIds);

    [Then(@"l'email d'invitation annonce que la date est soumise au vote")]
    public void LemailDinvitationAnnonceLeVote() => Assert.Contains("soumise au vote", Assert.Single(_emailSender.SentEmails).HtmlBody);

    [Then(@"le premier de mes événements n'a pas de date")]
    public void LePremierDeMesEvenementsNaPasDeDate()
    {
        Assert.Equal(2, _myEvents!.Events.Count);
        Assert.Null(_myEvents.Events[0].EventDate);
    }
}
