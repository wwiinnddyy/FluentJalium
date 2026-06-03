using System.ComponentModel;

namespace FluentJalium.Gallery.Models;

internal static class GallerySampleData
{
    public static GalleryRow[] CreateRows()
    {
        return
        [
            new GalleryRow("Buttons", "Complete", 9),
            new GalleryRow("Selection", "Review", 4),
            new GalleryRow("Collections", "Active", 8)
        ];
    }

    public static GalleryTreeRow[] CreateTree()
    {
        return
        [
            new GalleryTreeRow(
                "FluentJalium",
                "Active",
                [
                    new GalleryTreeRow("Theme resources", "Loaded", []),
                    new GalleryTreeRow("FW controls", "Expanding", [])
                ]),
            new GalleryTreeRow("Gallery", "Visible", [])
        ];
    }
}

internal sealed record GalleryRow(string Name, string State, int Count);

internal sealed record GalleryTreeRow(string Name, string State, GalleryTreeRow[] Children);

internal sealed class GalleryPropertySample
{
    [Category("Appearance")]
    [Description("Displayed title for the selected Gallery card.")]
    public string Title { get; set; } = "FluentJalium";

    [Category("Layout")]
    [Description("Preferred preview width in pixels.")]
    public double PreviewWidth { get; set; } = 420;

    [Category("Behavior")]
    [Description("Whether inline editing is enabled.")]
    public bool IsEditingEnabled { get; set; } = true;

    [Category("State")]
    [Description("Current accent sample count.")]
    public int AccentSamples { get; set; } = 6;
}
