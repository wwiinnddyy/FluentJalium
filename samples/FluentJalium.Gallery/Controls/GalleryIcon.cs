using FluentJalium.Icon;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Gallery.Controls;

/// <summary>
/// Creates icons for Gallery chrome using the Windows compatibility font stack.
///
/// FluentSystemIcons are embedded in the FluentJalium assembly and installed lazily. DirectWrite
/// keeps a shared font collection, so a window created during the same process can still render
/// a missing-glyph box until the collection is refreshed. Gallery chrome must be visible on first
/// frame, therefore it uses the stable Segoe compatibility glyphs (rendered through the
/// "Segoe Fluent Icons, Segoe MDL2 Assets" fallback stack, so Win11 shows the modern Fluent
/// look while older Windows falls back to MDL2) while still returning a FluentJalium
/// <see cref="FluentIcon"/> control.
/// </summary>
internal static class GalleryIcon
{
    public static FluentIcon Create(FluentIconRegular icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
        => FluentIconFactory.Segoe(Map(icon), size, foreground);

    private static SegoeFluentIcon Map(FluentIconRegular icon) => icon switch
    {
        FluentIconRegular.Home24 => SegoeFluentIcon.Home,
        FluentIconRegular.Cursor24 => SegoeFluentIcon.Click,
        FluentIconRegular.Edit24 => SegoeFluentIcon.Edit,
        FluentIconRegular.Grid24 => SegoeFluentIcon.Grid,
        FluentIconRegular.Navigation24 => SegoeFluentIcon.Navigation,
        FluentIconRegular.Sparkle24 => SegoeFluentIcon.Font,
        FluentIconRegular.Alert24 => SegoeFluentIcon.Status,
        FluentIconRegular.CheckmarkCircle24 => SegoeFluentIcon.Accept,
        FluentIconRegular.DataBarVertical24 => SegoeFluentIcon.DataTable,
        FluentIconRegular.ContentView24 => SegoeFluentIcon.Layout,
        FluentIconRegular.BookInformation24 => SegoeFluentIcon.Document,
        FluentIconRegular.Apps24 => SegoeFluentIcon.List,
        FluentIconRegular.Color24 => SegoeFluentIcon.Color,
        _ => Fallback(icon)
    };

    private static SegoeFluentIcon Fallback(FluentIconRegular icon)
    {
        var name = icon.ToString();
        if (name.StartsWith("Home", StringComparison.Ordinal)) return SegoeFluentIcon.Home;
        if (name.StartsWith("Search", StringComparison.Ordinal)) return SegoeFluentIcon.Search;
        if (name.StartsWith("Settings", StringComparison.Ordinal)) return SegoeFluentIcon.Settings;
        if (name.StartsWith("Alert", StringComparison.Ordinal) || name.StartsWith("Status", StringComparison.Ordinal)) return SegoeFluentIcon.Status;
        if (name.StartsWith("Color", StringComparison.Ordinal)) return SegoeFluentIcon.Color;
        if (name.StartsWith("Grid", StringComparison.Ordinal) || name.StartsWith("Layout", StringComparison.Ordinal)) return SegoeFluentIcon.Layout;
        if (name.StartsWith("Data", StringComparison.Ordinal) || name.StartsWith("Chart", StringComparison.Ordinal)) return SegoeFluentIcon.DataTable;
        if (name.StartsWith("Text", StringComparison.Ordinal) || name.StartsWith("Code", StringComparison.Ordinal)) return SegoeFluentIcon.TextInput;
        if (name.StartsWith("Add", StringComparison.Ordinal) || name.StartsWith("New", StringComparison.Ordinal)) return SegoeFluentIcon.Add;
        return SegoeFluentIcon.Controls;
    }
}
