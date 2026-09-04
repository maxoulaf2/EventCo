using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.DeleteEvent;
using EventCo.Application.Events.GetEventById;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.DeleteEvent;

[Binding]
public sealed class DeleteEventSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly FixedCurrentUserService _currentUserService = new(Guid.NewGuid());
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Exception? _thrownException;

    public DeleteEventSteps()
    {
        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => _currentUserService);

        _serviceProvider = builder.Build();
    }

    [Given(@"un événement à supprimer ""(.*)"" prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementASupprimerPrevuLeAuLieu(string title, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);

        _existingEventId = createResult.EventId;
    }

    [Given(@"je change d'utilisateur courant")]
    public void EtantDonneJeChangeDutilisateurCourant() => _currentUserService.UserId = Guid.NewGuid();

    [When(@"je supprime cet événement")]
    public async Task QuandJeSupprimeCetEvenement() => await Supprimer(_existingEventId!.Value);

    [When(@"je supprime un événement inexistant")]
    public async Task QuandJeSupprimeUnEvenementInexistant() => await Supprimer(Guid.NewGuid());

    private async Task Supprimer(Guid eventId)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            await dispatcher.Send(new DeleteEventCommand(eventId), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la suppression réussit")]
    public void AlorsLaSuppressionReussit() => Assert.Null(_thrownException);

    [Then(@"la suppression échoue avec une erreur d'autorisation")]
    public void AlorsLaSuppressionEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventCreatorException>(_thrownException);

    [Then(@"la suppression échoue avec une erreur d'événement introuvable")]
    public void AlorsLaSuppressionEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"l'événement supprimé n'est plus consultable")]
    public async Task AlorsLevenementSupprimeNestPlusConsultable()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        await Assert.ThrowsAsync<EventNotFoundException>(
            () => dispatcher.Send(new GetEventByIdQuery(_existingEventId!.Value), CancellationToken.None));
    }
}
