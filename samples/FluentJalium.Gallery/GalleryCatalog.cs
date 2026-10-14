using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FluentJalium.Gallery;

/// <summary>A page the Gallery hosts. <see cref="Host"/> is the element name in MainWindow.jalxaml.</summary>
internal sealed class GalleryPage
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
}

/// <summary>One control type Astra restyles implicitly, and how far the claim about it goes.</summary>
internal sealed class GalleryControl
{
    public string Markup { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Page { get; set; } = string.Empty;
    public string Parity { get; set; } = string.Empty;
    public string[]? Evidence { get; set; }
    public string[]? Gaps { get; set; }

    /// <summary>The type name without its namespace, for display: the catalog keeps prefixed types fully qualified.</summary>
    public string Name => Markup.Contains('.') ? Markup[(Markup.LastIndexOf('.') + 1)..] : Markup;
}

/// <summary>
/// The catalog shared with <c>tests/FluentJalium.Tests/Gallery/AstraGalleryCatalogTests.cs</c>, which
/// fails when it stops matching the shipped styles. Reading it here rather than repeating the claims in
/// page text is what keeps the parity strip from outliving the evidence behind it.
/// </summary>
internal sealed class GalleryCatalog
{
    public string Note { get; set; } = string.Empty;
    public Dictionary<string, string> ParityLegend { get; set; } = [];
    public GalleryPage[] Pages { get; set; } = [];
    public GalleryControl[] Controls { get; set; } = [];

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public static GalleryCatalog Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Catalog.json");
        if (!File.Exists(path)) return new GalleryCatalog();
        return JsonSerializer.Deserialize<GalleryCatalog>(File.ReadAllText(path), SerializerOptions) ?? new GalleryCatalog();
    }

    public GalleryPage? Page(string id) => Pages.FirstOrDefault(page => page.Id == id);

    public GalleryControl[] OnPage(string pageId) =>
        Controls.Where(control => control.Page == pageId).OrderBy(static control => control.Name, StringComparer.Ordinal).ToArray();

    /// <summary>"Button audited · ToggleButton audited", the per-control parity line.</summary>
    public string ParityLine(string pageId) =>
        string.Join("  ·  ", OnPage(pageId).Select(control => $"{control.Name} {control.Parity}"));

    /// <summary>Every open gap the catalog records for the page, so the shortfall is on screen too.</summary>
    public string GapsLine(string pageId) =>
        string.Join("  ·  ", OnPage(pageId).SelectMany(control => control.Gaps ?? []).Distinct(StringComparer.Ordinal));
}
