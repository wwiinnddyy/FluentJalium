using FluentJalium.Icon;

namespace FluentJalium.Gallery.Models;

internal sealed record GalleryDocumentationLink(string Title, string Uri);

internal sealed record GalleryPageInfo(
    string UniqueId,
    string Title,
    string Subtitle,
    string Description,
    string GroupId,
    string Group,
    FluentIconRegular Icon,
    string[] Tags,
    string[] RelatedControls,
    GalleryDocumentationLink[] DocumentationLinks,
    GalleryPageStatus Status = GalleryPageStatus.Stable,
    bool IsFooter = false,
    bool IsNew = false,
    bool IsUpdated = false,
    string? SourcePath = null,
    string[]? BaseClasses = null,
    string? ApiNamespace = null,
    string? SampleCodeKey = null)
{
    public bool MatchesSearch(string searchText)
    {
        var query = searchText.Trim();
        if (query.Length == 0)
        {
            return true;
        }

        foreach (var token in query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!ContainsIgnoreCase(UniqueId, token)
                && !ContainsIgnoreCase(Title, token)
                && !ContainsIgnoreCase(Subtitle, token)
                && !ContainsIgnoreCase(Description, token)
                && !ContainsIgnoreCase(Group, token)
                && !Tags.Any(tag => ContainsIgnoreCase(tag, token))
                && !RelatedControls.Any(control => ContainsIgnoreCase(control, token))
                && !(BaseClasses?.Any(baseClass => ContainsIgnoreCase(baseClass, token)) == true)
                && !ContainsOptional(SourcePath, token)
                && !ContainsOptional(ApiNamespace, token)
                && !ContainsOptional(SampleCodeKey, token)
                && !ContainsIgnoreCase(Status.ToString(), token)
                && !(IsNew && ContainsIgnoreCase("new", token))
                && !(IsUpdated && ContainsIgnoreCase("updated", token))
                && !DocumentationLinks.Any(link =>
                    ContainsIgnoreCase(link.Title, token) || ContainsIgnoreCase(link.Uri, token)))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ContainsIgnoreCase(string value, string query)
    {
        return value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool ContainsOptional(string? value, string query)
    {
        return !string.IsNullOrWhiteSpace(value) && ContainsIgnoreCase(value, query);
    }
}
