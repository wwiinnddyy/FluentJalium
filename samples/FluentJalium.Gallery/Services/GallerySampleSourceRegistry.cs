using System.Reflection;
using FluentJalium.Gallery.Models;

namespace FluentJalium.Gallery.Services;

/// <summary>
/// Reads a gallery page's own source file out of the assembly's embedded resources, so the source
/// panel shows what the page actually does.
/// </summary>
/// <remarks>
/// The catalog used to carry 1900 lines of hand-written sample text that had drifted away from
/// the pages it described.
///
/// Resolution deliberately avoids <see cref="GalleryPageInfo.Title"/> and <c>Group</c>: both are
/// localized display names, so anything keyed on them breaks in a non-English culture. The
/// locale-independent <see cref="GalleryPageInfo.SourcePath"/> segment is used instead, and every
/// candidate is verified against the manifest before it is read - a page whose file cannot be
/// found is reported as unresolved rather than shown with invented code.
/// </remarks>
internal static class GallerySampleSourceRegistry
{
    private const string ResourcePrefix = "FluentJalium.Gallery.Pages.";

    private static readonly Lazy<string[]> s_resourceNames =
        new(() => typeof(GallerySampleSourceRegistry).Assembly.GetManifestResourceNames());

    public static bool TryGetSource(GalleryPageInfo page, out string source)
    {
        ArgumentNullException.ThrowIfNull(page);

        var parts = new List<string>();

        foreach (var fileName in CandidateFileNames(page))
        {
            var resourceName = Array.Find(
                s_resourceNames.Value,
                name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal) &&
                        name.EndsWith($".{fileName}", StringComparison.Ordinal));

            if (resourceName is null)
            {
                continue;
            }

            using var stream = typeof(GallerySampleSourceRegistry).Assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                continue;
            }

            using var reader = new StreamReader(stream);
            parts.Add(reader.ReadToEnd().Trim());
        }

        source = parts.Count == 0
            ? string.Empty
            : string.Join(Environment.NewLine + Environment.NewLine, parts);

        return parts.Count > 0;
    }

    /// <summary>
    /// The page source files a catalog entry could live in, most specific first.
    /// </summary>
    public static string[] CandidateFileNames(GalleryPageInfo page)
    {
        var stems = new List<string>();

        var folder = page.SourcePath?.Trim().Trim('/');
        if (!string.IsNullOrWhiteSpace(folder))
        {
            var separator = folder.IndexOf('/');
            stems.Add(separator > 0 ? folder[..separator] : folder);
        }

        stems.Add(PascalCase(page.UniqueId));

        return stems
            .Where(stem => !string.IsNullOrWhiteSpace(stem))
            .Distinct(StringComparer.Ordinal)
            .SelectMany(stem => new[] { $"Gallery{stem}Page.cs", $"{stem}Page.cs", $"Gallery{stem}Page.jalxaml" })
            .ToArray();
    }

    private static string PascalCase(string? uniqueId)
    {
        if (string.IsNullOrWhiteSpace(uniqueId))
        {
            return string.Empty;
        }

        var builder = new System.Text.StringBuilder(uniqueId.Length);
        var capitalizeNext = true;

        foreach (var character in uniqueId)
        {
            if (character is '-' or '_' or ' ')
            {
                capitalizeNext = true;
                continue;
            }

            builder.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);
            capitalizeNext = false;
        }

        return builder.ToString();
    }
}
