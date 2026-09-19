using System.Net.Http.Json;
using EventCo.Api.Contracts.Auth;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Auth.UpdateProfile;

[Binding]
public sealed class UpdateProfileSteps(SessionContext sessionContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private CurrentUserResponse? _updatedUser;

    [When(@"je modifie mon nom d'affichage via l'API en ""(.*)""")]
    public async Task QuandJeModifieMonNomDaffichageViaLapiEn(string displayName) =>
        await AppelerAvecCookie(displayName, sessionContext.Cookie);

    [When(@"je modifie mon nom d'affichage via l'API en ""(.*)"" sans cookie de session")]
    public async Task QuandJeModifieMonNomDaffichageViaLapiEnSansCookieDeSession(string displayName) =>
        await AppelerAvecCookie(displayName, null);

    private async Task AppelerAvecCookie(string displayName, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        var request = new UpdateProfileRequest(displayName);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/api/auth/me")
        {
            Content = JsonContent.Create(request),
        };
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

    [Then(@"la réponse de modification de profil a le statut (\d+)")]
    public void AlorsLaReponseDeModificationDeProfilALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    [Then(@"l'utilisateur retourné a pour nom d'affichage ""(.*)""")]
    public void AlorsLutilisateurRetourneAPourNomDaffichage(string displayName) =>
        Assert.Equal(displayName, _updatedUser!.DisplayName);
}
