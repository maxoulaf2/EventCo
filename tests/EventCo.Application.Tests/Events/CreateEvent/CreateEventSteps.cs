using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events;
using EventCo.Infrastructure.Persistence;
using EventCo.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.CreateEvent;

[Binding]
public sealed class CreateEventSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EventCoDbContext _dbContext;
    private readonly Guid _currentUserId = Guid.NewGuid();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private CreateEventResult? _lastResult;
    private Exception? _thrownException;

    public CreateEventSteps()
    {
        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new FixedCurrentUserService(_currentUserId));

        _serviceProvider = builder.Build();
        _dbContext = _serviceProvider.GetRequiredService<EventCoDbContext>();
    }

    [When(@"je crée l'événement ""(.*)"" prévu le ""(.*)"" au lieu ""([^""]*)""")]
    public async Task JeCreeLevenementPrevuLeAuLieu(string title, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(
                new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [When(@"je crée l'événement ""(.*)"" prévu le ""(.*)"" au lieu ""([^""]*)"" avec l'image ""(.*)""")]
    public async Task JeCreeLevenementPrevuLeAuLieuAvecLimage(string title, string eventDate, string location, string imageUrl)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(
                new CreateEventCommand(title, null, DateTime.Parse(eventDate), location, imageUrl),
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [When(@"je crée l'événement ""(.*)"" prévu le ""(.*)"" au lieu ""([^""]*)"" avec une URL d'image trop longue")]
    public async Task JeCreeLevenementPrevuLeAuLieuAvecUneUrlDimageTropLongue(string title, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var tooLongImageUrl = "https://example.com/" + new string('a', 2048);

        try
        {
            _lastResult = await dispatcher.Send(
                new CreateEventCommand(title, null, DateTime.Parse(eventDate), location, tooLongImageUrl),
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la création réussit")]
    public void AlorsLaCreationReussit() => Assert.Null(_thrownException);

    [Then(@"la création échoue avec une erreur de validation")]
    public void AlorsLaCreationEchoueAvecUneErreurDeValidation() =>
        Assert.IsType<ValidationException>(_thrownException);

    [Then(@"l'événement créé a pour titre ""(.*)""")]
    public void AlorsLevenementCreeAPourTitre(string title) => Assert.Equal(title, _lastResult!.Title);

    [Then(@"l'événement créé a pour image ""(.*)""")]
    public void AlorsLevenementCreeAPourImage(string imageUrl) => Assert.Equal(imageUrl, _lastResult!.ImageUrl);

    [Then(@"l'événement créé n'a pas d'image")]
    public void AlorsLevenementCreeNaPasDimage() => Assert.Null(_lastResult!.ImageUrl);

    [Then(@"je suis inscrit comme organisateur de l'événement créé")]
    public async Task AlorsJeSuisInscritCommeOrganisateurDeLevenementCree()
    {
        var @event = await _dbContext.Events
            .Include(e => e.Participants)
            .SingleAsync(e => e.Id == _lastResult!.EventId);

        var creatorParticipant = Assert.Single(@event.Participants);
        Assert.Equal(_currentUserId, creatorParticipant.UserId);
        Assert.Equal(ParticipantRole.Organizer, creatorParticipant.Role);
        Assert.Equal(ParticipationStatus.Attending, creatorParticipant.ParticipationStatus);
    }
}
