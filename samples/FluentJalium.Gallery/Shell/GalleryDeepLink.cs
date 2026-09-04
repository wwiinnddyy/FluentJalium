using FluentJalium.Gallery.Models;

namespace FluentJalium.Gallery.Shell;

/// <summary>
/// Sample deep links (`fluentjalium://sample?uniqueId=...`, mirroring WinUI Gallery's
/// copy-link) plus repository code-search links for the Source menu. Links are copied
/// to the clipboard: Jalium exposes no launcher API, so opening a browser is a
/// documented adaptation, not a silent failure.
/// </summary>
internal static class GalleryDeepLink
{
    public const string Scheme = "fluentjalium";
    public const string SampleHost = "sample";
    public const string RepositoryUrl = "https://github.com/wwiinnddyy/FluentJalium";

    public static string FormatSampleLink(GalleryPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        return FormatSampleLink(page.UniqueId);
    }

    public static string FormatSampleLink(string uniqueId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uniqueId);
        return $"{Scheme}://{SampleHost}?uniqueId={Uri.EscapeDataString(uniqueId)}";
    }

    public static bool TryParseSampleLink(string? link, out string uniqueId)
    {
        uniqueId = string.Empty;

        if (string.IsNullOrWhiteSpace(link))
        {
            return false;
        }

        if (!Uri.TryCreate(link.Trim(), UriKind.Absolute, out var uri)
            || !string.Equals(uri.Scheme, Scheme, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(uri.Host, SampleHost, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var query = uri.Query.TrimStart('?');
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            if (string.Equals(pair[..separator], "uniqueId", StringComparison.OrdinalIgnoreCase))
            {
                uniqueId = Uri.UnescapeDataString(pair[(separator + 1)..]);
                return !string.IsNullOrWhiteSpace(uniqueId);
            }
        }

        return false;
    }

    public static string CodeSearchUrl(string query)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        return $"{RepositoryUrl}/search?q={Uri.EscapeDataString(query)}&type=code";
    }
}
