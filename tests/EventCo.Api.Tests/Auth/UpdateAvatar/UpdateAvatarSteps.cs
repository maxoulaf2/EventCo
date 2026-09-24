using System.Net.Http.Headers;
using System.Net.Http.Json;
using EventCo.Api.Contracts.Auth;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Auth.UpdateAvatar;

[Binding]
public sealed class UpdateAvatarSteps(SessionContext sessionContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private CurrentUserResponse? _updatedUser;
    private byte[]? _sentContent;

    [When(@"j'envoie une image ""(.*)"" comme photo de profil via l'API")]
    public async Task QuandJenvoieUneImageCommePhotoDeProfilViaLapi(string format) =>
        await Envoyer(TestImages.ByFormat(format), sessionContext.Cookie);

    [When(@"j'envoie une image ""(.*)"" comme photo de profil via l'API sans cookie de session")]
    public async Task QuandJenvoieUneImageCommePhotoDeProfilViaLapiSansCookieDeSession(string format) =>
        await Envoyer(TestImages.ByFormat(format), null);

    [Then(@"la réponse d'envoi de photo de profil a le statut (\d+)")]
    public void AlorsLaReponseDenvoiDePhotoDeProfilALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    // Fichier relu via l'URL publique retournée, servie par l'API elle-même (fallback LocalFileStorage) :
    // vérifie de bout en bout que ce qui a été envoyé est bien ce qui est servi, avec le bon Content-Type.
    [Then(@"la photo de profil retournée est téléchargeable avec le type ""(.*)""")]
    public async Task AlorsLaPhotoDeProfilRetourneeEstTelechargeableAvecLeType(string contentType)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var response = await client.GetAsync(_updatedUser!.AvatarUrl);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal(contentType, response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(_sentContent, await response.Content.ReadAsByteArrayAsync());
    }

    [Then(@"l'utilisateur courant expose la même photo de profil")]
    public async Task AlorsLutilisateurCourantExposeLaMemePhotoDeProfil()
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Add("Cookie", sessionContext.Cookie);

        using var response = await client.SendAsync(request);
        var currentUser = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();

        Assert.Equal(_updatedUser!.AvatarUrl, currentUser!.AvatarUrl);
    }

    private async Task Envoyer(byte[] content, string? cookieHeader)
    {
        _sentContent = content;
        var client = Hooks.Factory.CreateClient(ClientOptions);

        var fileContent = new ByteArrayContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        using var form = new MultipartFormDataContent { { fileContent, "file", "avatar.png" } };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/api/auth/me/avatar") { Content = form };
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
        if (_response.IsSuccessStatusCode)
        {
            _updatedUser = await _response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        }
    }
}
