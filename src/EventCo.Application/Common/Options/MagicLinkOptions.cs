namespace EventCo.Application.Common.Options;

public sealed class MagicLinkOptions
{
    public const string SectionName = "MagicLink";

    public int ExpiryMinutes { get; init; } = 15;

    // Anti-spam : au-delà de MaxRequestsPerWindow demandes pour le même email sur RateLimitWindowMinutes,
    // les demandes suivantes sont rejetées (cf. TooManyMagicLinkRequestsException).
    public int MaxRequestsPerWindow { get; init; } = 5;

    public int RateLimitWindowMinutes { get; init; } = 60;
}
