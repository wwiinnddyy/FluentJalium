using Jalium.UI.Controls;

namespace FluentJalium.Icon;

internal static class FluentIconSymbolMapper
{
    public static string GetGlyph(FluentIconRegular icon) => GetGlyph(icon.ToString(), (int)icon);

    public static string GetGlyph(FluentIconFilled icon) => GetGlyph(icon.ToString(), (int)icon);

    private static string GetGlyph(string name, int codePoint)
    {
        if (codePoint <= 0)
        {
            return string.Empty;
        }

        return char.ConvertFromUtf32((int)Map(name));
    }

    private static Symbol Map(string name)
    {
        if (name.Contains("Search", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Find", StringComparison.OrdinalIgnoreCase))
            return Symbol.Search;
        if (name.Contains("Setting", StringComparison.OrdinalIgnoreCase))
            return Symbol.Settings;
        if (name.Contains("Home", StringComparison.OrdinalIgnoreCase))
            return Symbol.Home;
        if (name.Contains("ChevronDown", StringComparison.OrdinalIgnoreCase))
            return Symbol.ChevronDown;
        if (name.Contains("ChevronUp", StringComparison.OrdinalIgnoreCase))
            return Symbol.ChevronUp;
        if (name.Contains("ChevronLeft", StringComparison.OrdinalIgnoreCase))
            return Symbol.ChevronLeft;
        if (name.Contains("ChevronRight", StringComparison.OrdinalIgnoreCase))
            return Symbol.ChevronRight;
        if (name.Contains("More", StringComparison.OrdinalIgnoreCase))
            return Symbol.More;
        if (name.Contains("ControlButton", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Button", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Cursor", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Click", StringComparison.OrdinalIgnoreCase))
            return Symbol.Click;
        if (name.Contains("Transparency", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Layer", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Material", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Acrylic", StringComparison.OrdinalIgnoreCase))
            return Symbol.MapLayers;
        if (name.Contains("Slide", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Transition", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Motion", StringComparison.OrdinalIgnoreCase))
            return Symbol.Slideshow;
        if (name.Contains("Diagram", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Branch", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Fork", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Flow", StringComparison.OrdinalIgnoreCase))
            return Symbol.Go;
        if (name.Contains("Database", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Data", StringComparison.OrdinalIgnoreCase))
            return Symbol.ViewAll;
        if (name.Contains("Gauge", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Progress", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Range", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Slider", StringComparison.OrdinalIgnoreCase))
            return Symbol.Refresh;
        if (name.Contains("Checkbox", StringComparison.OrdinalIgnoreCase))
            return Symbol.CheckboxComposite;
        if (name.Contains("Radio", StringComparison.OrdinalIgnoreCase))
            return Symbol.Accept;
        if (name.Contains("Design", StringComparison.OrdinalIgnoreCase))
            return Symbol.Color;
        if (name.Contains("Flash", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Lightning", StringComparison.OrdinalIgnoreCase))
            return Symbol.Refresh;
        if (name.Contains("Pause", StringComparison.OrdinalIgnoreCase))
            return Symbol.Pause;
        if (name.Contains("Archive", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Box", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Package", StringComparison.OrdinalIgnoreCase))
            return Symbol.Folder;
        if (name.Contains("Shield", StringComparison.OrdinalIgnoreCase))
            return Symbol.Shield;
        if (name.Contains("Lock", StringComparison.OrdinalIgnoreCase))
            return Symbol.Lock;
        if (name.Contains("App", StringComparison.OrdinalIgnoreCase))
            return Symbol.AllApps;
        if (name.Contains("Save", StringComparison.OrdinalIgnoreCase))
            return Symbol.Save;
        if (name.Contains("Share", StringComparison.OrdinalIgnoreCase))
            return Symbol.Share;
        if (name.Contains("Add", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Create", StringComparison.OrdinalIgnoreCase))
            return Symbol.Add;
        if (name.Contains("Delete", StringComparison.OrdinalIgnoreCase))
            return Symbol.Delete;
        if (name.Contains("Dismiss", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Cancel", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Close", StringComparison.OrdinalIgnoreCase))
            return Symbol.Cancel;
        if (name.Contains("Checkmark", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Accept", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Success", StringComparison.OrdinalIgnoreCase))
            return Symbol.Accept;
        if (name.Contains("Calendar", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Date", StringComparison.OrdinalIgnoreCase))
            return Symbol.Calendar;
        if (name.Contains("Clock", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Time", StringComparison.OrdinalIgnoreCase))
            return Symbol.Clock;
        if (name.Contains("FolderOpen", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("OpenFolder", StringComparison.OrdinalIgnoreCase))
            return Symbol.FolderOpen;
        if (name.Contains("Folder", StringComparison.OrdinalIgnoreCase))
            return Symbol.Folder;
        if (name.Contains("Pdf", StringComparison.OrdinalIgnoreCase))
            return Symbol.Document;
        if (name.Contains("Document", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("File", StringComparison.OrdinalIgnoreCase))
            return Symbol.Document;
        if (name.Contains("Play", StringComparison.OrdinalIgnoreCase))
            return Symbol.Play;
        if (name.Contains("Download", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("ArrowDown", StringComparison.OrdinalIgnoreCase))
            return Symbol.Download;
        if (name.Contains("Upload", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("ArrowUp", StringComparison.OrdinalIgnoreCase))
            return Symbol.Upload;
        if (name.Contains("Send", StringComparison.OrdinalIgnoreCase))
            return Symbol.Send;
        if (name.Contains("Refresh", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Clockwise", StringComparison.OrdinalIgnoreCase))
            return Symbol.Refresh;
        if (name.Contains("Rename", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Edit", StringComparison.OrdinalIgnoreCase))
            return Symbol.Edit;
        if (name.Contains("Pin", StringComparison.OrdinalIgnoreCase))
            return Symbol.Pin;
        if (name.Contains("Badge", StringComparison.OrdinalIgnoreCase))
            return Symbol.Warning;
        if (name.Contains("Image", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Picture", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Visual", StringComparison.OrdinalIgnoreCase))
            return Symbol.Picture;
        if (name.Contains("Color", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Brush", StringComparison.OrdinalIgnoreCase))
            return Symbol.Color;
        if (name.Contains("People", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Person", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Account", StringComparison.OrdinalIgnoreCase))
            return Symbol.People;
        if (name.Contains("Mail", StringComparison.OrdinalIgnoreCase))
            return Symbol.Mail;
        if (name.Contains("List", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Table", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Grid", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Collection", StringComparison.OrdinalIgnoreCase))
            return Symbol.BulletedList;
        if (name.Contains("Navigation", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Menu", StringComparison.OrdinalIgnoreCase))
            return Symbol.GlobalNavButton;
        if (name.Contains("Window", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Layout", StringComparison.OrdinalIgnoreCase))
            return Symbol.NewWindow;
        if (name.Contains("Info", StringComparison.OrdinalIgnoreCase))
            return Symbol.Help;
        if (name.Contains("Warning", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Caution", StringComparison.OrdinalIgnoreCase))
            return Symbol.Warning;
        if (name.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Critical", StringComparison.OrdinalIgnoreCase))
            return Symbol.Error;
        if (name.Contains("Star", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Favorite", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Rating", StringComparison.OrdinalIgnoreCase))
            return Symbol.Favorite;
        if (name.Contains("Code", StringComparison.OrdinalIgnoreCase))
            return Symbol.Code;
        if (name.Contains("History", StringComparison.OrdinalIgnoreCase))
            return Symbol.History;
        if (name.Contains("Text", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Font", StringComparison.OrdinalIgnoreCase))
            return Symbol.Font;
        if (name.Contains("Toggle", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Switch", StringComparison.OrdinalIgnoreCase))
            return Symbol.Switch;
        if (name.Contains("Open", StringComparison.OrdinalIgnoreCase))
            return Symbol.OpenFile;
        if (name.Contains("More", StringComparison.OrdinalIgnoreCase))
            return Symbol.More;

        return Symbol.Document;
    }
}
