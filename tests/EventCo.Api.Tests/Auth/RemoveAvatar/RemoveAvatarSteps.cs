using System.Net.Http.Json;
using EventCo.Api.Contracts.Auth;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Auth.RemoveAvatar;

[Binding]
public sealed class RemoveAvatarSteps(SessionContext sessionContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private string? _previousAvatarUrl;
    private HttpResponseMessage? _response;
    private CurrentUserResponse? _updatedUser;

    [Given(@"une photo de profil déjà envoyée via l'API")]
    public async Task EtantDonneUnePhotoDeProfilDejaEnvoyeeViaLapi()
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        using var form = new MultipartFormDataContent { { new ByteArrayContent(TestImages.Png), "file", "avatar.png" } };
        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/auth/me/avatar") { Content = form };
        request.Headers.Add("Cookie", sessionContext.Cookie);

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        _previousAvatarUrl = (await response.Content.ReadFromJsonAsync<CurrentUserResponse>())!.AvatarUrl;
    }

    [When(@"je supprime ma photo de profil via l'API")]
    public async Task QuandJeSupprimeMaPhotoDeProfilViaLapi() => await Supprimer(sessionContext.Cookie);

    [When(@"je supprime ma photo de profil via l'API sans cookie de session")]
    public async Task QuandJeSupprimeMaPhotoDeProfilViaLapiSansCookieDeSession() => await Supprimer(null);

    [Then(@"la réponse de suppression de photo de profil a le statut (\d+)")]
    public void AlorsLaReponseDeSuppressionDePhotoDeProfilALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    [Then(@"l'utilisateur retourné n'a plus de photo de profil")]
    public void AlorsLutilisateurRetourneNaPlusDePhotoDeProfil() => Assert.Null(_updatedUser!.AvatarUrl);

    [Then(@"l'ancienne photo de profil n'est plus servie")]
    public async Task AlorsLanciennePhotoDeProfilNestPlusServie()
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var response = await client.GetAsync(_previousAvatarUrl);

        Assert.Equal(404, (int)response.StatusCode);
    }

    private async Task Supprimer(string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        using var request = new HttpRequestMessage(HttpMethod.Delete, "/api/auth/me/avatar");
        if (cookieHeader is not null)
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(request);
        if (_response.IsSuccessStatusCode)
        {
            _updatedUser = await _response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        }
    }
}
