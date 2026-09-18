using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Events.SetParticipationStatus;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Domain.Users;
using EventCo.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.SetParticipationStatus;

[Binding]
public sealed class SetParticipationStatusSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Guid? _participantUserId;
    private Exception? _thrownException;

    public SetParticipationStatusSteps(CurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;

        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));

        _serviceProvider = builder.Build();
    }

    [Given(@"un événement ""(.*)"" avec un participant invité ""(.*)"", qui participera le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementAvecUnParticipantInviteQuiParticiperaLeAuLieu(string title, string email, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);
        _existingEventId = createResult.EventId;

        var inviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId.Value, email),
            CancellationToken.None);
        _participantUserId = inviteResult.UserId;
    }

    [Given(@"je suis le participant invité")]
    public void EtantDonneJeSuisLeParticipantInvite() => _currentUserContext.UserId = _participantUserId!.Value;

    [When(@"j'indique le statut de participation ""(.*)""")]
    public async Task QuandJindiqueLeStatutDeParticipation(string status) =>
        await IndiquerLeStatut(_existingEventId!.Value, status);

    [When(@"j'indique le statut de participation ""(.*)"" sur un événement inexistant")]
    public async Task QuandJindiqueLeStatutDeParticipationSurUnEvenementInexistant(string status) =>
        await IndiquerLeStatut(Guid.NewGuid(), status);

    private async Task IndiquerLeStatut(Guid eventId, string status)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            await dispatcher.Send(new SetParticipationStatusCommand(eventId, status), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"le changement de statut réussit")]
    public void AlorsLeChangementDeStatutReussit() => Assert.Null(_thrownException);

    [Then(@"le changement de statut échoue avec une erreur de validation")]
    public void AlorsLeChangementDeStatutEchoueAvecUneErreurDeValidation() =>
        Assert.IsType<ValidationException>(_thrownException);

    [Then(@"le changement de statut échoue avec une erreur de participant introuvable")]
    public void AlorsLeChangementDeStatutEchoueAvecUneErreurDeParticipantIntrouvable() =>
        Assert.IsType<ParticipantNotFoundException>(_thrownException);

    [Then(@"le changement de statut échoue avec une erreur réservée au créateur")]
    public void AlorsLeChangementDeStatutEchoueAvecUneErreurReserveeAuCreateur() =>
        Assert.IsType<EventCreatorCannotChangeParticipationStatusException>(_thrownException);

    [Then(@"le changement de statut échoue avec une erreur d'événement introuvable")]
    public void AlorsLeChangementDeStatutEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"mon statut de participation est ""(.*)""")]
    public async Task AlorsMonStatutDeParticipationEst(string status)
    {
        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId!.Value, CancellationToken.None);
        var participant = @event!.Participants.Single(p => p.UserId == _participantUserId!.Value);

        Assert.Equal(status, participant.ParticipationStatus.ToString());
    }
}
