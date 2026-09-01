using System.Net.Http.Json;
using EventCo.Api.Contracts.Auth;
using EventCo.Api.Tests.Support;
using EventCo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Api.Tests.Auth.RequestLink;

[Binding]
public sealed class RequestLinkSteps
{
    private HttpResponseMessage? _response;

    [When(@"j'envoie une requête POST à ""(.*)"" avec l'email ""(.*)""")]
    public async Task QuandJenvoieUneRequetePostAAvecLemail(string path, string email)
    {
        var client = Hooks.Factory.CreateClient();
        _response = await client.PostAsJsonAsync(path, new RequestMagicLinkRequest(email));
    }

    [Then(@"la réponse a le statut (\d+)")]
    public void AlorsLaReponseALeStatut(int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
    }

    [Then(@"un token de connexion est persisté en base pour ""(.*)""")]
    public async Task AlorsUnTokenDeConnexionEstPersisteEnBasePour(string email)
    {
        using var scope = Hooks.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventCoDbContext>();
        var tokens = await dbContext.MagicLinkTokens.ToListAsync();

        Assert.Contains(tokens, token => token.Email.Value == email);
    }
}
