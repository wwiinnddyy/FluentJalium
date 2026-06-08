using FluentJalium.Icon;

namespace FluentJalium.Gallery.Models;

internal sealed record GalleryControlInfo(
    string Name,
    GalleryPageInfo Page,
    FluentIconRegular Icon,
    string Group,
    GalleryPageStatus Status,
    bool IsNew,
    bool IsUpdated,
    string? SourcePath,
    string[] BaseClasses,
    string? ApiNamespace,
    string? SampleCodeKey,
    string[] Tags)
{
    public static GalleryControlInfo[] CreateFromPages(IEnumerable<GalleryPageInfo> pages)
    {
        return pages
            .Where(IsControlPage)
            .SelectMany(CreateFromPage)
            .GroupBy(control => control.Name, StringComparer.Ordinal)
            .Select(group => group
                .OrderByDescending(control => ScoreSourceMatch(control))
                .ThenBy(control => control.Page.Title, StringComparer.Ordinal)
                .First())
            .OrderBy(control => control.Name, StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<GalleryControlInfo> CreateFromPage(GalleryPageInfo page)
    {
        var controls = page.RelatedControls.Length > 0
            ? page.RelatedControls
            : page.Tags.Where(IsControlName).Distinct(StringComparer.Ordinal).ToArray();

        foreach (var control in controls.Where(IsControlName))
        {
            yield return new GalleryControlInfo(
                control,
                page,
                page.Icon,
                page.Group,
                page.Status,
                page.IsNew,
                page.IsUpdated,
                CreateControlSourcePath(page.SourcePath, control),
                page.BaseClasses ?? [],
                page.ApiNamespace,
                page.SampleCodeKey,
                page.Tags);
        }
    }

    private static bool IsControlPage(GalleryPageInfo page)
    {
        return !page.IsFooter
            && page.Group != GalleryNavigationGroup.Home
            && page.Group != GalleryNavigationGroup.Catalog
            && (page.RelatedControls.Any(IsControlName) || page.Tags.Any(IsControlName));
    }

    private static bool IsControlName(string value)
    {
        return value.StartsWith("FW", StringComparison.Ordinal)
            && value.Length > 2
            && value.All(character => char.IsLetterOrDigit(character) || character == '_');
    }

    private static string? CreateControlSourcePath(string? pageSourcePath, string control)
    {
        if (string.IsNullOrWhiteSpace(pageSourcePath))
        {
            return null;
        }

        var trimmed = pageSourcePath.Trim();
        var separatorIndex = trimmed.LastIndexOfAny(['/','\\']);
        if (separatorIndex >= 0 &&
            string.Equals(trimmed[(separatorIndex + 1)..], control, StringComparison.Ordinal))
        {
            return trimmed;
        }

        return $"{trimmed.TrimEnd('/', '\\')}/{control}";
    }

    private static int ScoreSourceMatch(GalleryControlInfo control)
    {
        var source = control.Page.SourcePath;
        var score = !string.IsNullOrWhiteSpace(source) &&
            source.EndsWith("/" + control.Name, StringComparison.Ordinal)
                ? 1
                : 0;

        if (control.Tags.Any(tag => string.Equals(tag, control.Name, StringComparison.Ordinal)))
        {
            score += 2;
        }

        return score;
    }
}
