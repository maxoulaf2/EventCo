using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.CreateItem;
using EventCo.Application.Events.GetEventItems;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.GetEventItems;

[Binding]
public sealed class GetEventItemsSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private GetEventItemsResult? _lastResult;
    private Exception? _thrownException;

    public GetEventItemsSteps(CurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;

        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));
        builder.Services.AddSingleton<IItemRealtimeNotifier>(new RecordingItemRealtimeNotifier());

        _serviceProvider = builder.Build();
    }

    [Given(@"un événement ""(.*)"" prévu le ""(.*)"" au lieu ""(.*)"" dont je veux consulter les articles")]
    public async Task EtantDonneUnEvenementDontJeVeuxConsulterLesArticles(string title, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);

        _existingEventId = createResult.EventId;
    }

    [Given(@"un article ""(.*)"" est ajouté à cet événement")]
    public async Task EtantDonneUnArticleEstAjouteACetEvenement(string title)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        await dispatcher.Send(new CreateItemCommand(_existingEventId!.Value, title, null, "ToBring"), CancellationToken.None);
    }

    [When(@"je consulte les articles de cet événement")]
    public async Task QuandJeConsulteLesArticlesDeCetEvenement() => await ConsulterLesArticles(_existingEventId!.Value);

    [Given(@"je change d'utilisateur courant pour un administrateur")]
    [Scope(Feature = "Consultation des articles d'un événement")]
    public async Task EtantDonneJeChangeDutilisateurCourantPourUnAdministrateur() =>
        _currentUserContext.UserId = await AdminUserSeeder.SeedAsync(_serviceProvider);

    [When(@"je consulte les articles d'un événement inexistant")]
    public async Task QuandJeConsulteLesArticlesDunEvenementInexistant() => await ConsulterLesArticles(Guid.NewGuid());

    private async Task ConsulterLesArticles(Guid eventId)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(new GetEventItemsQuery(eventId), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la consultation des articles réussit")]
    public void AlorsLaConsultationDesArticlesReussit() => Assert.Null(_thrownException);

    [Then(@"la consultation des articles échoue avec une erreur d'événement introuvable")]
    public void AlorsLaConsultationDesArticlesEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"la consultation des articles échoue avec une erreur d'autorisation")]
    public void AlorsLaConsultationDesArticlesEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventParticipantException>(_thrownException);

    [Then(@"l'événement consulté a (\d+) articles?")]
    public void AlorsLevenementConsulteAArticles(int count) => Assert.Equal(count, _lastResult!.Items.Count);

    [Then(@"l'événement consulté a un article ""(.*)""")]
    public void AlorsLevenementConsulteAUnArticle(string title) =>
        Assert.Single(_lastResult!.Items, t => t.Title == title);
}
