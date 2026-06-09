namespace FluentJalium.Gallery.Models;

internal sealed record GalleryCatalogFilterGroupSnapshot(
    string Group,
    int ControlCount,
    int PageCount);

internal sealed record GalleryCatalogFilterSnapshot(
    GalleryCatalogFilter Filter,
    GalleryControlInfo[] Matches,
    int PageCount,
    int NewCount,
    int UpdatedCount,
    int PreviewCount,
    int DiagnosticCount,
    GalleryCatalogFilterGroupSnapshot[] GroupCounts,
    int WithSourcePathCount,
    int WithSampleCodeKeyCount,
    int WithApiNamespaceCount)
{
    public int ControlCount => Matches.Length;

    public bool HasCompleteNavigationMetadata =>
        WithSourcePathCount == ControlCount
        && WithSampleCodeKeyCount == ControlCount
        && WithApiNamespaceCount == ControlCount;

    public static GalleryCatalogFilterSnapshot Create(GalleryCatalogFilter filter, IEnumerable<GalleryPageInfo> pages)
    {
        ArgumentNullException.ThrowIfNull(pages);

        return Create(filter, GalleryControlInfo.CreateFromPages(pages));
    }

    public static GalleryCatalogFilterSnapshot Create(GalleryCatalogFilter filter, IEnumerable<GalleryControlInfo> controls)
    {
        ArgumentNullException.ThrowIfNull(controls);

        var matches = controls
            .Where(control => MatchesFilter(filter, control))
            .OrderBy(control => control.Group, StringComparer.Ordinal)
            .ThenBy(control => control.Page.Title, StringComparer.Ordinal)
            .ThenBy(control => control.Name, StringComparer.Ordinal)
            .ToArray();

        return new GalleryCatalogFilterSnapshot(
            filter,
            matches,
            matches.Select(control => control.Page.UniqueId).Distinct(StringComparer.Ordinal).Count(),
            matches.Count(control => control.IsNew),
            matches.Count(control => control.IsUpdated),
            matches.Count(control => control.Status == GalleryPageStatus.Preview),
            matches.Count(IsDiagnosticControl),
            matches
                .GroupBy(control => control.Group, StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new GalleryCatalogFilterGroupSnapshot(
                    group.Key,
                    group.Count(),
                    group.Select(control => control.Page.UniqueId).Distinct(StringComparer.Ordinal).Count()))
                .ToArray(),
            matches.Count(control => !string.IsNullOrWhiteSpace(control.SourcePath)),
            matches.Count(control => !string.IsNullOrWhiteSpace(control.SampleCodeKey)),
            matches.Count(control => !string.IsNullOrWhiteSpace(control.ApiNamespace)));
    }

    public bool ContainsControl(string controlName)
    {
        return Matches.Any(control => string.Equals(control.Name, controlName, StringComparison.Ordinal));
    }

    public bool ContainsPage(string pageId)
    {
        return Matches.Any(control => string.Equals(control.Page.UniqueId, pageId, StringComparison.Ordinal));
    }

    public bool ContainsSampleCodeKey(string sampleCodeKey)
    {
        return Matches.Any(control => string.Equals(control.SampleCodeKey, sampleCodeKey, StringComparison.Ordinal));
    }

    private static bool MatchesFilter(GalleryCatalogFilter filter, GalleryControlInfo control)
    {
        return filter switch
        {
            GalleryCatalogFilter.AllControls => true,
            GalleryCatalogFilter.New => control.IsNew,
            GalleryCatalogFilter.Updated => control.IsUpdated,
            GalleryCatalogFilter.Preview => control.Status == GalleryPageStatus.Preview,
            GalleryCatalogFilter.Diagnostic => IsDiagnosticControl(control),
            _ => false
        };
    }

    private static bool IsDiagnosticControl(GalleryControlInfo control)
    {
        return control.Status == GalleryPageStatus.Diagnostic
            || control.Name.Contains("Diagnostics", StringComparison.Ordinal);
    }
}
