using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.GetEventById;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.GetEventById;

[Binding]
public sealed class GetEventByIdSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private GetEventByIdResult? _lastResult;
    private Exception? _thrownException;

    public GetEventByIdSteps()
    {
        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new FixedCurrentUserService(Guid.NewGuid()));

        _serviceProvider = builder.Build();
    }

    [Given(@"un événement ""(.*)"" prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementPrevuLeAuLieu(string title, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);

        _existingEventId = createResult.EventId;
    }

    [When(@"je consulte cet événement")]
    public async Task QuandJeConsulteCetEvenement()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(new GetEventByIdQuery(_existingEventId!.Value), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [When(@"je consulte un événement inexistant")]
    public async Task QuandJeConsulteUnEvenementInexistant()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(new GetEventByIdQuery(Guid.NewGuid()), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la consultation réussit")]
    public void AlorsLaConsultationReussit() => Assert.Null(_thrownException);

    [Then(@"la consultation échoue avec une erreur d'événement introuvable")]
    public void AlorsLaConsultationEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"l'événement consulté a pour titre ""(.*)""")]
    public void AlorsLevenementConsulteAPourTitre(string title) => Assert.Equal(title, _lastResult!.Title);
}
