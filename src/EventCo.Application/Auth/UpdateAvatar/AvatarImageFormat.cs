namespace EventCo.Application.Auth.UpdateAvatar;

// Format détecté à partir des premiers octets du fichier (signature), jamais du Content-Type ou de
// l'extension envoyés par le client : le fichier est ensuite servi publiquement avec ce Content-Type,
// un contenu arbitraire déclaré "image/png" ne doit pas pouvoir passer.
public sealed record AvatarImageFormat(string Extension, string ContentType)
{
    public static readonly AvatarImageFormat Jpeg = new("jpg", "image/jpeg");
    public static readonly AvatarImageFormat Png = new("png", "image/png");
    public static readonly AvatarImageFormat Webp = new("webp", "image/webp");

    private static ReadOnlySpan<byte> JpegSignature => [0xFF, 0xD8, 0xFF];
    private static ReadOnlySpan<byte> PngSignature => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static ReadOnlySpan<byte> RiffSignature => "RIFF"u8;
    private static ReadOnlySpan<byte> WebpSignature => "WEBP"u8;

    public static AvatarImageFormat? Detect(ReadOnlySpan<byte> content)
    {
        if (content.StartsWith(JpegSignature))
            return Jpeg;

        if (content.StartsWith(PngSignature))
            return Png;

        if (content.Length >= 12 && content.StartsWith(RiffSignature) && content[8..12].SequenceEqual(WebpSignature))
            return Webp;

        return null;
    }
}
