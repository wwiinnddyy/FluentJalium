using FluentJalium.Icon;
using Jalium.UI;

namespace FluentJalium.Gallery.Models;

internal sealed record GalleryPage(
    GalleryPageInfo Info,
    Func<UIElement> CreateContent,
    Func<UIElement>? CreateSampleMetadata = null)
{
    public string Title => Info.Title;

    public string Description => Info.Description;

    public string Group => Info.Group;

    public FluentIconRegular Icon => Info.Icon;

    public GalleryPageStatus Status => Info.Status;

    public bool IsFooter => Info.IsFooter;

    public bool MatchesSearch(string searchText) => Info.MatchesSearch(searchText);
}
