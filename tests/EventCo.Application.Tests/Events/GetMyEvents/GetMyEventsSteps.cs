using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.GetMyEvents;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events;
using EventCo.Infrastructure.Persistence;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.GetMyEvents;

[Binding]
public sealed class GetMyEventsSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EventCoDbContext _dbContext;
    private readonly Guid _currentUserId = Guid.NewGuid();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private GetMyEventsResult? _lastResult;
    private Exception? _thrownException;

    public GetMyEventsSteps()
    {
        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new FixedCurrentUserService(_currentUserId));

        _serviceProvider = builder.Build();
        _dbContext = _serviceProvider.GetRequiredService<EventCoDbContext>();
    }

    [Given(@"un événement ""(.*)"" que j'ai créé")]
    public async Task EtantDonneUnEvenementQueJaiCree(string title)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        await dispatcher.Send(new CreateEventCommand(title, null, _now.AddDays(1), null), CancellationToken.None);
    }

    [Given(@"un événement ""(.*)"" créé par un autre utilisateur auquel je ne participe pas")]
    public async Task EtantDonneUnEvenementCreeParUnAutreUtilisateurAuquelJeNeParticipePas(string title)
    {
        var @event = Event.Create(title, null, _now.AddDays(1), null, Guid.NewGuid(), _now);
        _dbContext.Events.Add(@event);
        await _dbContext.SaveChangesAsync();
    }

    [Given(@"un événement ""(.*)"" créé par un autre utilisateur qui m'y a invité sans que j'aie rejoint")]
    public async Task EtantDonneUnEvenementCreeParUnAutreUtilisateurQuiMyAInviteSansQueJaieRejoint(string title)
    {
        var @event = Event.Create(title, null, _now.AddDays(1), null, Guid.NewGuid(), _now);
        @event.InviteParticipant(_currentUserId, _now);
        _dbContext.Events.Add(@event);
        await _dbContext.SaveChangesAsync();
    }

    [When(@"je consulte la liste de mes événements")]
    public async Task QuandJeConsulteLaListeDeMesEvenements()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(new GetMyEventsQuery(), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la consultation de mes événements réussit")]
    public void AlorsLaConsultationDeMesEvenementsReussit() => Assert.Null(_thrownException);

    [Then(@"ma liste d'événements contient ""(.*)""")]
    public void AlorsMaListeDevenementsContient(string title) =>
        Assert.Contains(_lastResult!.Events, e => e.Title == title);

    [Then(@"ma liste d'événements ne contient pas ""(.*)""")]
    public void AlorsMaListeDevenementsNeContientPas(string title) =>
        Assert.DoesNotContain(_lastResult!.Events, e => e.Title == title);

    [Then(@"ma liste d'événements est vide")]
    public void AlorsMaListeDevenementsEstVide() => Assert.Empty(_lastResult!.Events);

    [Then(@"""(.*)"" apparaît avec le rôle ""(.*)"" et le statut ""rejoint""")]
    public void AlorsLevenementApparaitAvecLeRoleEtLeStatutRejoint(string title, string role)
    {
        var summary = Assert.Single(_lastResult!.Events, e => e.Title == title);
        Assert.Equal(role, summary.Role);
        Assert.True(summary.HasJoined);
    }

    [Then(@"""(.*)"" apparaît avec le rôle ""(.*)"" et le statut ""invitation en attente""")]
    public void AlorsLevenementApparaitAvecLeRoleEtLeStatutInvitationEnAttente(string title, string role)
    {
        var summary = Assert.Single(_lastResult!.Events, e => e.Title == title);
        Assert.Equal(role, summary.Role);
        Assert.False(summary.HasJoined);
    }
}
