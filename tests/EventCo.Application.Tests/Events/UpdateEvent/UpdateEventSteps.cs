using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.UpdateEvent;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.UpdateEvent;

[Binding]
public sealed class UpdateEventSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private UpdateEventResult? _lastResult;
    private Exception? _thrownException;

    public UpdateEventSteps()
    {
        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new FixedCurrentUserService(Guid.NewGuid()));

        _serviceProvider = builder.Build();
    }

    [Given(@"un événement existant ""(.*)"" prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementExistantPrevuLeAuLieu(string title, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);

        _existingEventId = createResult.EventId;
    }

    [When(@"je modifie cet événement avec le titre ""(.*)"" prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task JeModifieCetEvenementAvecLeTitrePrevuLeAuLieu(string title, string eventDate, string location) =>
        await Modifier(_existingEventId!.Value, title, eventDate, location);

    [When(@"je modifie un événement inexistant avec le titre ""(.*)"" prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task JeModifieUnEvenementInexistantAvecLeTitrePrevuLeAuLieu(string title, string eventDate, string location) =>
        await Modifier(Guid.NewGuid(), title, eventDate, location);

    private async Task Modifier(Guid eventId, string title, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(
                new UpdateEventCommand(eventId, title, null, DateTime.Parse(eventDate), location),
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la modification réussit")]
    public void AlorsLaModificationReussit() => Assert.Null(_thrownException);

    [Then(@"la modification échoue avec une erreur de validation")]
    public void AlorsLaModificationEchoueAvecUneErreurDeValidation() =>
        Assert.IsType<ValidationException>(_thrownException);

    [Then(@"la modification échoue avec une erreur d'événement introuvable")]
    public void AlorsLaModificationEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"l'événement modifié a pour titre ""(.*)""")]
    public void AlorsLevenementModifieAPourTitre(string title) => Assert.Equal(title, _lastResult!.Title);

    [Then(@"l'événement modifié a pour date ""(.*)""")]
    public void AlorsLevenementModifieAPourDate(string eventDate) =>
        Assert.Equal(DateTime.Parse(eventDate), _lastResult!.EventDate);

    [Then(@"l'événement modifié a pour lieu ""(.*)""")]
    public void AlorsLevenementModifieAPourLieu(string location) => Assert.Equal(location, _lastResult!.Location);
}
