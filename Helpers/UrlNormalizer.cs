namespace Browser.Helpers;

public static class UrlNormalizer
{
    public static string Resolve(string input, string searchUrlTemplate)
    {
        var text = (input ?? "").Trim();
        if (string.IsNullOrEmpty(text))
            return "";

        if (Uri.TryCreate(text, UriKind.Absolute, out var absolute) &&
            (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
        {
            return absolute.ToString();
        }

        if (text.Contains('.') && !text.Contains(' ') &&
            Uri.TryCreate("https://" + text, UriKind.Absolute, out var httpsUri))
        {
            return httpsUri.ToString();
        }

        var encoded = Uri.EscapeDataString(text);
        return string.Format(searchUrlTemplate, encoded);
    }

    public static bool TryResolve(string input, string searchUrlTemplate, out Uri uri)
    {
        uri = null!;
        var resolved = Resolve(input, searchUrlTemplate);
        return !string.IsNullOrEmpty(resolved) && Uri.TryCreate(resolved, UriKind.Absolute, out uri!);
    }
}
