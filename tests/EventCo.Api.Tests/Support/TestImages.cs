namespace EventCo.Api.Tests.Support;

// Contenus minimaux reconnus par ImageFormat (signature en tête de fichier) : le format n'est
// détecté que sur ces premiers octets, le reste du fichier n'est jamais décodé côté serveur.
public static class TestImages
{
    public static byte[] Png { get; } = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];

    public static byte[] Text { get; } = "<script>alert('pas une image')</script>"u8.ToArray();

    public static byte[] ByFormat(string format) => format switch
    {
        "PNG" => Png,
        "texte" => Text,
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
    };
}
