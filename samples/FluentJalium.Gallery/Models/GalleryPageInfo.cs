using FluentJalium.Icon;

namespace FluentJalium.Gallery.Models;

internal sealed record GalleryPageInfo(
    string Title,
    string Description,
    string Group,
    FluentIconRegular Icon,
    string[] Keywords,
    GalleryPageStatus Status = GalleryPageStatus.Stable,
    bool IsFooter = false)
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
            if (!ContainsIgnoreCase(Title, token)
                && !ContainsIgnoreCase(Description, token)
                && !ContainsIgnoreCase(Group, token)
                && !Keywords.Any(keyword => ContainsIgnoreCase(keyword, token)))
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
}
