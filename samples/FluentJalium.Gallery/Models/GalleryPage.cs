using FluentJalium.Icon;
using Jalium.UI;

namespace FluentJalium.Gallery.Models;

internal sealed record GalleryPage(
    GalleryPageInfo Info,
    Func<UIElement> CreateContent,
    Func<UIElement>? CreateSampleMetadata = null)
{
    public string UniqueId => Info.UniqueId;

    public string Title => Info.Title;

    public string Subtitle => Info.Subtitle;

    public string Description => Info.Description;

    public string GroupId => Info.GroupId;

    public string Group => Info.Group;

    public FluentIconRegular Icon => Info.Icon;

    public GalleryPageStatus Status => Info.Status;

    public bool IsFooter => Info.IsFooter;

    public bool IsNew => Info.IsNew;

    public bool IsUpdated => Info.IsUpdated;

    public string? SourcePath => Info.SourcePath;

    public IReadOnlyList<string> BaseClasses => Info.BaseClasses ?? [];

    public string? ApiNamespace => Info.ApiNamespace;

    public string? SampleCodeKey => Info.SampleCodeKey;

    public IReadOnlyList<string> Tags => Info.Tags;

    public IReadOnlyList<string> RelatedControls => Info.RelatedControls;

    public IReadOnlyList<GalleryDocumentationLink> DocumentationLinks => Info.DocumentationLinks;

    public bool MatchesSearch(string searchText) => Info.MatchesSearch(searchText);
}
