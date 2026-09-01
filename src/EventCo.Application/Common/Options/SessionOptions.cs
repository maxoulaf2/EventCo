namespace EventCo.Application.Common.Options;

public sealed class SessionOptions
{
    public const string SectionName = "Session";

    public string Secret { get; init; } = string.Empty;

    public int ExpiryDays { get; init; } = 30;
}
