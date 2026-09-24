using System.Net.Http.Headers;
using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.UpdateImage;

[Binding]
public sealed class UpdateImageSteps(SessionContext sessionContext, EventContext eventContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private EventImageResponse? _updatedImage;
    private byte[]? _sentContent;

    [When(@"j'envoie une image ""(.*)"" comme image de présentation de cet événement via l'API")]
    public async Task QuandJenvoieUneImageCommeImageDePresentationDeCetEvenementViaLapi(string format) =>
        await Envoyer(eventContext.EventId!.Value, TestImages.ByFormat(format), sessionContext.Cookie);

    [When(@"j'envoie une image ""(.*)"" comme image de présentation d'un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJenvoieUneImageCommeImageDePresentationDunEvenementInexistantViaLapiSansCookieDeSession(string format) =>
        await Envoyer(Guid.NewGuid(), TestImages.ByFormat(format), null);

    [Then(@"la réponse d'envoi d'image de présentation a le statut (\d+)")]
    public void AlorsLaReponseDenvoiDimageDePresentationALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    // Fichier relu via l'URL publique retournée, servie par l'API elle-même (fallback LocalFileStorage) :
    // vérifie de bout en bout que ce qui a été envoyé est bien ce qui est servi, avec le bon Content-Type.
    [Then(@"l'image de présentation retournée est téléchargeable avec le type ""(.*)""")]
    public async Task AlorsLimageDePresentationRetourneeEstTelechargeableAvecLeType(string contentType)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var response = await client.GetAsync(_updatedImage!.ImageUrl);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal(contentType, response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(_sentContent, await response.Content.ReadAsByteArrayAsync());
    }

    [Then(@"le détail de l'événement expose la même image de présentation")]
    public async Task AlorsLeDetailDeLevenementExposeLaMemeImageDePresentation()
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/events/{eventContext.EventId}");
        request.Headers.Add("Cookie", sessionContext.Cookie);

        using var response = await client.SendAsync(request);
        var eventDetail = await response.Content.ReadFromJsonAsync<EventDetailResponse>();

        Assert.Equal(_updatedImage!.ImageUrl, eventDetail!.ImageUrl);
    }

    private async Task Envoyer(Guid eventId, byte[] content, string? cookieHeader)
    {
        _sentContent = content;
        var client = Hooks.Factory.CreateClient(ClientOptions);

        var fileContent = new ByteArrayContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        using var form = new MultipartFormDataContent { { fileContent, "file", "image.png" } };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/events/{eventId}/image") { Content = form };
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
        if (_response.IsSuccessStatusCode)
        {
            _updatedImage = await _response.Content.ReadFromJsonAsync<EventImageResponse>();
        }
    }
}
