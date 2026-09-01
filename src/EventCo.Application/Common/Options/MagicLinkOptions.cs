namespace EventCo.Application.Common.Options;

public sealed class MagicLinkOptions
{
    public const string SectionName = "MagicLink";

    public int ExpiryMinutes { get; init; } = 15;

    public string VerificationUrlBase { get; init; } = string.Empty;
}
