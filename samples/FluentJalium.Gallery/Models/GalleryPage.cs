using FluentJalium.Icon;
using Jalium.UI;

namespace FluentJalium.Gallery.Models;

internal sealed record GalleryPage(
    string Title,
    string Description,
    string Group,
    FluentIconRegular Icon,
    Func<UIElement> CreateContent,
    string SearchText,
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
                && !ContainsIgnoreCase(SearchText, token))
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
