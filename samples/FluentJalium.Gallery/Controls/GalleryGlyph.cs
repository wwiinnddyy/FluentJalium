using FluentJalium.Icon;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Gallery.Controls;

/// <summary>
/// Maps the gallery's <see cref="FluentIconRegular"/> metadata onto glyphs from the Segoe icon
/// font that ships with Windows.
/// </summary>
/// <remarks>
/// FluentJalium's own Fluent System Icons set has to be installed into the user font folder
/// before DirectWrite will see it, so a first run shows nothing but notdef boxes. Shell chrome
/// needs to render on every run, which is why navigation and page headers draw from Segoe
/// instead. Every codepoint below was checked against the installed <c>segmdl2.ttf</c> cmap.
/// </remarks>
internal static class GalleryGlyph
{
    public static FluentIcon Create(FluentIconRegular icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
    {
        var element = FluentIconFactory.Segoe(Map(icon), size, foreground);

        if (foreground is null)
        {
            element.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
        }

        return element;
    }

    private static SegoeFluentIcon Map(FluentIconRegular icon) => icon switch
    {
        FluentIconRegular.Home24 => SegoeFluentIcon.Home,
        FluentIconRegular.Filter24 => SegoeFluentIcon.SelectAll,
        FluentIconRegular.DesignIdeas24 => SegoeFluentIcon.Color,
        FluentIconRegular.ControlButton24 => SegoeFluentIcon.Button,
        FluentIconRegular.Textbox24 => SegoeFluentIcon.TextInput,
        FluentIconRegular.LayoutColumnTwo24 => SegoeFluentIcon.Layout,
        FluentIconRegular.LayoutRowTwo24 => SegoeFluentIcon.Layout,
        FluentIconRegular.Table24 => SegoeFluentIcon.DataTable,
        FluentIconRegular.TransparencySquare24 => SegoeFluentIcon.ColorMedia,
        FluentIconRegular.SlideTransition24 => SegoeFluentIcon.Play,
        FluentIconRegular.Navigation24 => SegoeFluentIcon.Navigation,
        FluentIconRegular.DataUsage24 => SegoeFluentIcon.Diagnostics,
        FluentIconRegular.Search24 => SegoeFluentIcon.Search,
        FluentIconRegular.WindowBrush24 => SegoeFluentIcon.Layout,
        FluentIconRegular.Settings24 => SegoeFluentIcon.Settings,
        FluentIconRegular.DocumentBulletList24 => SegoeFluentIcon.List,
        FluentIconRegular.New24 => SegoeFluentIcon.Add,
        FluentIconRegular.ArrowClockwise24 => SegoeFluentIcon.Refresh,
        FluentIconRegular.Sparkle24 => SegoeFluentIcon.Font,
        FluentIconRegular.Diagram24 => SegoeFluentIcon.Layout,
        FluentIconRegular.Color24 => SegoeFluentIcon.Color,
        FluentIconRegular.TextFont24 => SegoeFluentIcon.Font,
        FluentIconRegular.Ruler24 => SegoeFluentIcon.Grid,
        FluentIconRegular.Gauge24 => SegoeFluentIcon.Range,
        FluentIconRegular.CheckboxChecked24 => SegoeFluentIcon.Toggle,
        FluentIconRegular.ToggleMultiple24 => SegoeFluentIcon.Switch,
        FluentIconRegular.FormMultiple24 => SegoeFluentIcon.Document,
        FluentIconRegular.CalendarLtr24 => SegoeFluentIcon.Calendar,
        FluentIconRegular.Image24 => SegoeFluentIcon.Image,
        FluentIconRegular.CursorClick24 => SegoeFluentIcon.Click,
        FluentIconRegular.AppFolder24 => SegoeFluentIcon.Folder,
        FluentIconRegular.DatabaseSearch24 => SegoeFluentIcon.SelectAll,
        FluentIconRegular.Code24 => SegoeFluentIcon.Document,
        FluentIconRegular.ChartMultiple24 => SegoeFluentIcon.DataTable,
        FluentIconRegular.List24 => SegoeFluentIcon.List,
        _ => Fallback(icon.ToString())
    };

    private static SegoeFluentIcon Fallback(string name) => name switch
    {
        var s when s.StartsWith("Calendar", StringComparison.Ordinal) => SegoeFluentIcon.Calendar,
        var s when s.StartsWith("Clock", StringComparison.Ordinal) || s.StartsWith("Time", StringComparison.Ordinal)
            || s.StartsWith("History", StringComparison.Ordinal) || s.StartsWith("Hourglass", StringComparison.Ordinal)
            || s.StartsWith("Timer", StringComparison.Ordinal) => SegoeFluentIcon.Clock,
        var s when s.StartsWith("Table", StringComparison.Ordinal) || s.StartsWith("Chart", StringComparison.Ordinal)
            || s.StartsWith("Data", StringComparison.Ordinal) => SegoeFluentIcon.DataTable,
        var s when s.StartsWith("Text", StringComparison.Ordinal) || s.StartsWith("Braces", StringComparison.Ordinal)
            || s.StartsWith("Code", StringComparison.Ordinal) || s.StartsWith("Markdown", StringComparison.Ordinal) => SegoeFluentIcon.TextInput,
        var s when s.StartsWith("Alert", StringComparison.Ordinal) || s.StartsWith("Error", StringComparison.Ordinal)
            || s.StartsWith("Warning", StringComparison.Ordinal) || s.StartsWith("Status", StringComparison.Ordinal)
            || s.StartsWith("Shield", StringComparison.Ordinal) || s.StartsWith("Info", StringComparison.Ordinal)
            || s.StartsWith("Question", StringComparison.Ordinal) || s.StartsWith("Prohibited", StringComparison.Ordinal) => SegoeFluentIcon.Status,
        var s when s.StartsWith("Play", StringComparison.Ordinal) || s.StartsWith("Video", StringComparison.Ordinal)
            || s.StartsWith("Pause", StringComparison.Ordinal) || s.StartsWith("Stop", StringComparison.Ordinal)
            || s.StartsWith("FastForward", StringComparison.Ordinal) || s.StartsWith("Motion", StringComparison.Ordinal) => SegoeFluentIcon.Play,
        var s when s.StartsWith("Settings", StringComparison.Ordinal) || s.StartsWith("Slide", StringComparison.Ordinal) => SegoeFluentIcon.Settings,
        var s when s.StartsWith("Color", StringComparison.Ordinal) || s.StartsWith("Paint", StringComparison.Ordinal)
            || s.StartsWith("Ink", StringComparison.Ordinal) || s.StartsWith("Pen", StringComparison.Ordinal) => SegoeFluentIcon.Color,
        var s when s.StartsWith("Shape", StringComparison.Ordinal) || s.StartsWith("Draw", StringComparison.Ordinal)
            || s.StartsWith("Border", StringComparison.Ordinal) || s.StartsWith("Square", StringComparison.Ordinal)
            || s.StartsWith("Circle", StringComparison.Ordinal) || s.StartsWith("Box", StringComparison.Ordinal)
            || s.StartsWith("Layer", StringComparison.Ordinal) || s.StartsWith("Layout", StringComparison.Ordinal) => SegoeFluentIcon.Layout,
        var s when s.StartsWith("Folder", StringComparison.Ordinal) || s.StartsWith("Library", StringComparison.Ordinal)
            || s.StartsWith("Archive", StringComparison.Ordinal) => SegoeFluentIcon.Folder,
        var s when s.StartsWith("Document", StringComparison.Ordinal) || s.StartsWith("Clipboard", StringComparison.Ordinal)
            || s.StartsWith("Page", StringComparison.Ordinal) => SegoeFluentIcon.Document,
        var s when s.StartsWith("Checkbox", StringComparison.Ordinal) || s.StartsWith("Radio", StringComparison.Ordinal)
            || s.StartsWith("Toggle", StringComparison.Ordinal) || s.StartsWith("Switch", StringComparison.Ordinal) => SegoeFluentIcon.Toggle,
        var s when s.StartsWith("Cursor", StringComparison.Ordinal) || s.StartsWith("Click", StringComparison.Ordinal) => SegoeFluentIcon.Click,
        var s when s.StartsWith("Arrow", StringComparison.Ordinal) || s.StartsWith("Chevron", StringComparison.Ordinal)
            || s.StartsWith("Panel", StringComparison.Ordinal) => SegoeFluentIcon.ChevronUp,
        var s when s.StartsWith("Image", StringComparison.Ordinal) || s.StartsWith("Resize", StringComparison.Ordinal) => SegoeFluentIcon.Image,
        var s when s.StartsWith("People", StringComparison.Ordinal) || s.StartsWith("Person", StringComparison.Ordinal)
            || s.StartsWith("Account", StringComparison.Ordinal) => SegoeFluentIcon.People,
        var s when s.StartsWith("List", StringComparison.Ordinal) || s.StartsWith("Group", StringComparison.Ordinal)
            || s.StartsWith("Apps", StringComparison.Ordinal) => SegoeFluentIcon.List,
        var s when s.StartsWith("Key", StringComparison.Ordinal) || s.StartsWith("Password", StringComparison.Ordinal)
            || s.StartsWith("Lock", StringComparison.Ordinal) || s.StartsWith("Pin", StringComparison.Ordinal) => SegoeFluentIcon.Account,
        var s when s.StartsWith("Save", StringComparison.Ordinal) || s.StartsWith("Download", StringComparison.Ordinal)
            || s.StartsWith("ArrowDownload", StringComparison.Ordinal) => SegoeFluentIcon.Save,
        var s when s.StartsWith("Delete", StringComparison.Ordinal) || s.StartsWith("Dismiss", StringComparison.Ordinal)
            || s.StartsWith("Cancel", StringComparison.Ordinal) || s.StartsWith("Eraser", StringComparison.Ordinal) => SegoeFluentIcon.Delete,
        var s when s.StartsWith("Add", StringComparison.Ordinal) || s.StartsWith("New", StringComparison.Ordinal) => SegoeFluentIcon.Add,
        var s when s.StartsWith("Edit", StringComparison.Ordinal) || s.StartsWith("Rename", StringComparison.Ordinal) => SegoeFluentIcon.Edit,
        var s when s.StartsWith("Link", StringComparison.Ordinal) || s.StartsWith("Share", StringComparison.Ordinal)
            || s.StartsWith("Send", StringComparison.Ordinal) || s.StartsWith("Upload", StringComparison.Ordinal) => SegoeFluentIcon.Share,
        var s when s.StartsWith("Copy", StringComparison.Ordinal) => SegoeFluentIcon.Copy,
        var s when s.StartsWith("Refresh", StringComparison.Ordinal) || s.StartsWith("Cloud", StringComparison.Ordinal)
            || s.StartsWith("Sync", StringComparison.Ordinal) => SegoeFluentIcon.Refresh,
        var s when s.StartsWith("Search", StringComparison.Ordinal) => SegoeFluentIcon.Search,
        var s when s.StartsWith("Badge", StringComparison.Ordinal) || s.StartsWith("Star", StringComparison.Ordinal)
            || s.StartsWith("Flag", StringComparison.Ordinal) || s.StartsWith("Tag", StringComparison.Ordinal) => SegoeFluentIcon.Badge,
        var s when s.StartsWith("Grid", StringComparison.Ordinal) || s.StartsWith("Align", StringComparison.Ordinal)
            || s.StartsWith("FullScreen", StringComparison.Ordinal) => SegoeFluentIcon.Grid,
        var s when s.StartsWith("Globe", StringComparison.Ordinal) || s.StartsWith("Server", StringComparison.Ordinal)
            || s.StartsWith("Web", StringComparison.Ordinal) || s.StartsWith("Connected", StringComparison.Ordinal) => SegoeFluentIcon.Scroll,
        var s when s.StartsWith("Mail", StringComparison.Ordinal) => SegoeFluentIcon.Mail,
        var s when s.StartsWith("Phone", StringComparison.Ordinal) => SegoeFluentIcon.Account,
        var s when s.StartsWith("Microphone", StringComparison.Ordinal) || s.StartsWith("Audio", StringComparison.Ordinal) => SegoeFluentIcon.Microphone,
        var s when s.StartsWith("Scroll", StringComparison.Ordinal) || s.StartsWith("Swipe", StringComparison.Ordinal) => SegoeFluentIcon.Scroll,
        var s when s.StartsWith("Range", StringComparison.Ordinal) || s.StartsWith("Slider", StringComparison.Ordinal) => SegoeFluentIcon.Range,
        var s when s.StartsWith("Beaker", StringComparison.Ordinal) || s.StartsWith("Bug", StringComparison.Ordinal)
            || s.StartsWith("Diagnostics", StringComparison.Ordinal) || s.StartsWith("Desktop", StringComparison.Ordinal) => SegoeFluentIcon.Diagnostics,
        _ => SegoeFluentIcon.Controls
    };
}
