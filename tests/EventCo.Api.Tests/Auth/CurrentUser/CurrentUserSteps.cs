using System.Net.Http.Json;
using EventCo.Api.Contracts.Auth;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Auth.CurrentUser;

[Binding]
public sealed class CurrentUserSteps(SessionContext sessionContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private CurrentUserResponse? _currentUser;

    [When(@"j'appelle ""(.*)"" avec le cookie de session obtenu")]
    public async Task QuandJappelleAvecLeCookieDeSessionObtenu(string path) =>
        await AppelerAvecCookie(path, sessionContext.Cookie);

    [When(@"j'appelle ""(.*)"" sans cookie de session")]
    public async Task QuandJappelleSansCookieDeSession(string path) =>
        await AppelerAvecCookie(path, null);

    [When(@"j'appelle ""(.*)"" avec le cookie de session invalide ""(.*)""")]
    public async Task QuandJappelleAvecLeCookieDeSessionInvalide(string path, string cookieValue) =>
        await AppelerAvecCookie(path, $"eventco_session={cookieValue}");

    private async Task AppelerAvecCookie(string path, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (cookieHeader is not null)
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(request);
        if (_response.IsSuccessStatusCode)
        {
            _currentUser = await _response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        }
    }

    [Then(@"la réponse de l'utilisateur courant a le statut (\d+)")]
    public void AlorsLaReponseDeLutilisateurCourantALeStatut(int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
    }

    [Then(@"l'utilisateur courant retourné a pour email ""(.*)""")]
    public void AlorsLutilisateurCourantRetourneAPourEmail(string email)
    {
        Assert.Equal(email, _currentUser!.Email);
    }
}
