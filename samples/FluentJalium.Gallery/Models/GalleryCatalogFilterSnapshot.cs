namespace FluentJalium.Gallery.Models;

internal sealed record GalleryCatalogFilterSnapshot(
    GalleryCatalogFilter Filter,
    GalleryControlInfo[] Matches,
    int PageCount,
    int NewCount,
    int UpdatedCount,
    int PreviewCount,
    int DiagnosticCount)
{
    public int ControlCount => Matches.Length;

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
            matches.Count(IsDiagnosticControl));
    }

    public bool ContainsControl(string controlName)
    {
        return Matches.Any(control => string.Equals(control.Name, controlName, StringComparison.Ordinal));
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
