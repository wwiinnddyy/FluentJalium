using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentJalium.Gallery;
using FluentJalium.Launcher;
using FluentJalium.Localization;

namespace FluentJalium.Tests.Localization;

/// <summary>
/// Every user-visible literal either host declares has to have a 简体中文 entry, because a language switch that
/// silently leaves one page in English is worse than one that is not offered.
/// </summary>
/// <remarks>
/// <para>
/// The gate reads the <c>.jalxaml</c> rather than the built tree on purpose. It needs no window, it runs in CI, and
/// it catches the case the sweeper cannot see at runtime: a string added to the markup and never registered.
/// <see cref="TextSweeper.MissingTranslations"/> reports the same thing for a live tree; this reports it for the
/// source of truth.
/// </para>
/// <para>
/// The reader here is deliberately a second implementation, not the one the product uses. Sharing it would let
/// both drift from the file in the same direction, which is the same argument
/// <c>AstraGalleryCatalogTests</c> makes about its catalog reader.
/// </para>
/// </remarks>
[Collection(nameof(LocalizationCoverageTests))]
public class LocalizationCoverageTests
{
    private const string GalleryMarkup = "samples/FluentJalium.Gallery/MainWindow.jalxaml";
    private const string LauncherMarkup = "samples/FluentJalium.Launcher/LauncherWindow.jalxaml";
    private const string CatalogPath = "samples/FluentJalium.Gallery/Catalog.json";

    private static readonly string[] TextAttributes =
        ["Text", "Content", "Header", "Title", "Label", "Message"];

    [Fact]
    public void The_gate_has_something_to_check()
    {
        // A coverage test that passes because the reader found nothing is worse than none. This pins the shape of
        // the file the other assertions are about.
        Assert.True(MarkupLiterals(GalleryMarkup).Count > 200,
            "the Gallery reader stopped finding the page's own text, so the gate below would pass vacuously");
        Assert.True(MarkupLiterals(LauncherMarkup).Count > 5,
            "the launcher reader stopped finding the window's own text");
    }

    [Fact]
    public void Every_gallery_string_the_markup_declares_is_translated()
    {
        GalleryText.Register();

        var missing = Missing(GalleryMarkup);
        Assert.True(missing.Count == 0,
            $"{missing.Count} Gallery string(s) have no 简体中文 entry:\n  " + string.Join("\n  ", missing));
    }

    [Fact]
    public void Every_launcher_string_the_markup_declares_is_translated()
    {
        LauncherText.Register();

        var missing = Missing(LauncherMarkup);
        Assert.True(missing.Count == 0,
            $"{missing.Count} launcher string(s) have no 简体中文 entry:\n  " + string.Join("\n  ", missing));
    }

    [Fact]
    public void Every_catalog_page_title_is_translated_because_the_parity_strip_shows_it()
    {
        GalleryText.Register();
        using var catalog = JsonDocument.Parse(Read(CatalogPath));

        var missing = new List<string>();
        foreach (var page in catalog.RootElement.GetProperty("pages").EnumerateArray())
        {
            var title = page.GetProperty("title").GetString();
            if (title is { Length: > 0 } && !UiText.HasChinese(title))
            {
                missing.Add($"{page.GetProperty("id").GetString()}: {title}");
            }
        }

        Assert.True(missing.Count == 0,
            $"{missing.Count} Catalog.json page title(s) have no 简体中文 entry:\n  " + string.Join("\n  ", missing));
    }

    [Fact]
    public void A_registered_key_never_differs_only_by_whitespace()
    {
        GalleryText.Register();
        LauncherText.Register();

        // A trailing space survives a copy-paste and reads as a missing translation at runtime, because the
        // sweeper's lookup is ordinal.
        var sloppy = UiText.RegisteredKeys
            .Where(key => UiText.HasChinese(key) && key != key.Trim())
            .ToArray();

        Assert.True(sloppy.Length == 0, "keys with stray whitespace: " + string.Join(", ", sloppy));
    }

    private static List<string> Missing(string relativePath)
    {
        UiText.SetLanguage(UiLanguage.English);
        return [.. MarkupLiterals(relativePath).Where(literal => !UiText.HasChinese(literal))];
    }

    private static IReadOnlyCollection<string> MarkupLiterals(string relativePath)
    {
        var markup = Read(relativePath);
        var literals = new SortedSet<string>(StringComparer.Ordinal);

        foreach (Match element in Regex.Matches(markup, "<[\\w:.]+[^>]*?/>"))
        {
            foreach (var attribute in TextAttributes)
            {
                var match = Regex.Match(element.Value, $@"\s{attribute}=""([^""]*)""");
                if (match.Success && IsProse(match.Groups[1].Value))
                {
                    literals.Add(match.Groups[1].Value);
                }
            }
        }

        return literals;
    }

    /// <summary>
    /// A literal with no letters is an icon code point rather than prose. So is a numeric character reference -
    /// <c>&amp;#xE713;</c> on a menu item's glyph reads as letters because of its <c>xE</c> tail, so the check
    /// has to look at the whole shape rather than at whether a letter appears anywhere in it.
    /// </summary>
    private static bool IsProse(string value) =>
        value.Length > 0
        && !Regex.IsMatch(value, @"^\s*(&#x[0-9A-Fa-f]+;|\\u[0-9A-Fa-f]{4})\s*$")
        && value.Any(char.IsLetter);

    private static string Read(string relativePath) =>
        File.ReadAllText(Path.Combine(RepositoryRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FluentJalium.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Cannot locate the repository root.");
    }
}

/// <summary>
/// One collection, so the two classes that register catalogs into the same process-wide
/// <see cref="UiText"/> cannot interleave and see each other's half-registered state.
/// </summary>
[CollectionDefinition(nameof(LocalizationCoverageTests))]
public class LocalizationCoverageCollection;
