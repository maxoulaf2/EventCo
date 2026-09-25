using System.Net.Http.Json;
using EventCo.Api.Contracts.Versioning;
using EventCo.Api.Tests.Support;
using Reqnroll;

namespace EventCo.Api.Tests.Versioning.AppVersion;

[Binding]
public sealed class AppVersionSteps
{
    private HttpResponseMessage? _response;

    [When(@"je consulte la version de l'API")]
    public async Task QuandJeConsulteLaVersionDeLapi()
    {
        _response = await Hooks.Factory.CreateClient().GetAsync("/api/version");
    }

    [When(@"je consulte l'utilisateur courant sans cookie de session")]
    public async Task QuandJeConsulteLutilisateurCourantSansCookieDeSession()
    {
        _response = await Hooks.Factory.CreateClient().GetAsync("/api/auth/me");
    }

    [Then(@"la réponse de version a le statut (\d+)")]
    public void AlorsLaReponseDeVersionALeStatut(int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
    }

    [Then(@"la version retournée est celle du build déployé")]
    public async Task AlorsLaVersionRetourneeEstCelleDuBuildDeploye()
    {
        var body = await _response!.Content.ReadFromJsonAsync<AppVersionResponse>();
        Assert.Equal(ApiWebApplicationFactory.AppVersion, body!.Version);
    }

    [Then(@"la réponse de version n'est pas mise en cache")]
    public void AlorsLaReponseDeVersionNestPasMiseEnCache()
    {
        Assert.True(_response!.Headers.CacheControl?.NoStore);
    }

    [Then(@"la réponse porte l'en-tête de version du build déployé")]
    public void AlorsLaReponsePorteLenTeteDeVersionDuBuildDeploye()
    {
        Assert.True(_response!.Headers.TryGetValues(EventCo.Api.Versioning.AppVersion.HeaderName, out var values));
        Assert.Equal(ApiWebApplicationFactory.AppVersion, Assert.Single(values!));
    }
}
