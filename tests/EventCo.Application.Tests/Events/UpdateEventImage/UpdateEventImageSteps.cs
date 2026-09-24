using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.UpdateEventImage;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence;
using EventCo.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.UpdateEventImage;

[Binding]
public sealed class UpdateEventImageSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EventCoDbContext _dbContext;
    private readonly InMemoryFileStorage _fileStorage = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private UpdateEventImageResult? _previousResult;
    private UpdateEventImageResult? _lastResult;
    private Exception? _thrownException;

    public UpdateEventImageSteps(CurrentUserContext currentUserContext)
    {
        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddSingleton<IFileStorage>(_fileStorage);
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));

        _serviceProvider = builder.Build();
        _dbContext = _serviceProvider.GetRequiredService<EventCoDbContext>();
    }

    [Given(@"un événement sans image ""(.*)""")]
    public async Task EtantDonneUnEvenementSansImage(string title)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, new DateTime(2026, 12, 24, 0, 0, 0, DateTimeKind.Utc), "Chez Alice"),
            CancellationToken.None);

        _existingEventId = createResult.EventId;
    }

    [Given(@"j'ai déjà envoyé une image ""(.*)"" comme image de présentation de cet événement")]
    public async Task EtantDonneJaiDejaEnvoyeUneImageCommeImageDePresentationDeCetEvenement(string format)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        _previousResult = await dispatcher.Send(
            new UpdateEventImageCommand(_existingEventId!.Value, TestImages.ByFormat(format)),
            CancellationToken.None);
    }

    [When(@"j'envoie une image ""(.*)"" comme image de présentation de cet événement")]
    public async Task QuandJenvoieUneImageCommeImageDePresentationDeCetEvenement(string format) =>
        await Envoyer(_existingEventId!.Value, TestImages.ByFormat(format));

    [When(@"j'envoie une image ""(.*)"" comme image de présentation d'un événement inexistant")]
    public async Task QuandJenvoieUneImageCommeImageDePresentationDunEvenementInexistant(string format) =>
        await Envoyer(Guid.NewGuid(), TestImages.ByFormat(format));

    [When(@"j'envoie une image PNG de 6 Mo comme image de présentation de cet événement")]
    public async Task QuandJenvoieUneImagePngDe6MoCommeImageDePresentationDeCetEvenement()
    {
        var content = new byte[6 * 1024 * 1024];
        TestImages.Png.CopyTo(content, 0);

        await Envoyer(_existingEventId!.Value, content);
    }

    [Then(@"l'envoi de l'image de présentation réussit")]
    public void AlorsLenvoiDeLimageDePresentationReussit() => Assert.Null(_thrownException);

    [Then(@"l'envoi de l'image de présentation échoue avec une erreur de validation")]
    public void AlorsLenvoiDeLimageDePresentationEchoueAvecUneErreurDeValidation() =>
        Assert.IsType<ValidationException>(_thrownException);

    [Then(@"l'envoi de l'image de présentation échoue avec une erreur d'autorisation")]
    public void AlorsLenvoiDeLimageDePresentationEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventOrganizerException>(_thrownException);

    [Then(@"l'envoi de l'image de présentation échoue avec une erreur d'événement introuvable")]
    public void AlorsLenvoiDeLimageDePresentationEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"l'image de présentation est accessible à une URL publique du bucket des images d'événement")]
    public void AlorsLimageDePresentationEstAccessibleAUneUrlPubliqueDuBucketDesImagesDevenement()
    {
        var location = Assert.Single(_fileStorage.Files.Keys);
        Assert.Equal(FileStorageBucket.EventImages, location.Bucket);
        Assert.Equal(_fileStorage.GetPublicUrl(location.Bucket, location.Key), _lastResult!.ImageUrl);
    }

    [Then(@"l'image de présentation a changé d'URL")]
    public void AlorsLimageDePresentationAChangeDurl() =>
        Assert.NotEqual(_previousResult!.ImageUrl, _lastResult!.ImageUrl);

    [Then(@"le bucket des images d'événement contient (\d+) fichiers? de type ""(.*)""")]
    public void AlorsLeBucketDesImagesDevenementContientFichiersDeType(int count, string contentType)
    {
        var files = _fileStorage.Files.Where(file => file.Key.Bucket == FileStorageBucket.EventImages).ToList();
        Assert.Equal(count, files.Count);
        Assert.All(files, file => Assert.Equal(contentType, file.Value.ContentType));
    }

    [Then(@"le bucket des images d'événement ne contient aucun fichier")]
    public void AlorsLeBucketDesImagesDevenementNeContientAucunFichier() =>
        Assert.DoesNotContain(_fileStorage.Files.Keys, location => location.Bucket == FileStorageBucket.EventImages);

    [Then(@"l'événement référence l'image envoyée")]
    public async Task AlorsLevenementReferenceLimageEnvoyee()
    {
        var @event = await _dbContext.Events.AsNoTracking().SingleAsync(e => e.Id == _existingEventId);

        var location = Assert.Single(_fileStorage.Files.Keys);
        Assert.Equal(location.Key, @event.ImageStorageKey);
    }

    private async Task Envoyer(Guid eventId, byte[] content)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(new UpdateEventImageCommand(eventId, content), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }
}
