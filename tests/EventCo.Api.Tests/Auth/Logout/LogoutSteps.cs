using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Auth.Logout;

[Binding]
public sealed class LogoutSteps(SessionContext sessionContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"je me déconnecte via l'API avec le cookie de session obtenu")]
    public Task QuandJeMeDeconnecteViaLapiAvecLeCookieDeSessionObtenu() => SeDeconnecter(sessionContext.Cookie);

    [When(@"je me déconnecte via l'API sans cookie de session")]
    public Task QuandJeMeDeconnecteViaLapiSansCookieDeSession() => SeDeconnecter(null);

    private async Task SeDeconnecter(string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        if (cookieHeader is not null)
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(request);
    }

    [Then(@"la réponse de déconnexion a le statut (\d+)")]
    public void AlorsLaReponseDeDeconnexionALeStatut(int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
    }

    [Then(@"le cookie de session est supprimé dans la réponse")]
    public void AlorsLeCookieDeSessionEstSupprimeDansLaReponse()
    {
        Assert.True(_response!.Headers.TryGetValues("Set-Cookie", out var cookies));
        // La suppression d'un cookie (Response.Cookies.Delete) le réémet avec une valeur vide et une
        // date d'expiration passée : la valeur vide suffit à vérifier l'intention sans dépendre du
        // format exact de la date (qui varie selon la culture/le format HTTP utilisé).
        Assert.Contains(cookies!, c => c.StartsWith("eventco_session=;", StringComparison.Ordinal));
    }
}
