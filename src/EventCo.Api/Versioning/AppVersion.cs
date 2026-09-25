namespace EventCo.Api.Versioning;

/// <summary>
/// Identifiant du build déployé (SHA git injecté au build Docker, cf. Dockerfile), partagé avec le frontend
/// buildé dans la même image : un frontend qui reçoit une autre version que la sienne se recharge.
/// Vaut <see cref="DevValue"/> hors build Docker, ce qui désactive la détection côté frontend.
/// </summary>
public sealed record AppVersion(string Value)
{
    public const string HeaderName = "X-App-Version";
    public const string DevValue = "dev";

    public static AppVersion FromConfiguration(IConfiguration configuration)
    {
        var value = configuration["App:Version"];
        return new AppVersion(string.IsNullOrWhiteSpace(value) ? DevValue : value.Trim());
    }
}
