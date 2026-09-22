using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Media;
using Xunit;

namespace FluentJalium.Tests.Gallery;

/// <summary>
/// The Tokens page paints one swatch per palette brush token from a list generated out of the canonical inventory,
/// which makes that list the part that can go stale: a token renamed in the theme layer would otherwise keep showing
/// up as a swatch nobody notices is missing, and the page's whole claim is that the grid is the set the kernel
/// publishes. So the two readers are independent here, the same way the catalog gate keeps its own JSON reader apart
/// from the Gallery's - this test parses <c>docs/astra/audits/keys.md</c> with its own text walk and the generated
/// array with a regex, and fails when the two lists part ways in either direction or in order.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public class AstraGalleryTokenTests : IDisposable
{
    private const string KeysPath = "docs/astra/audits/keys.md";
    private const string PageListPath = "samples/FluentJalium.Gallery/TokenCatalog.cs";

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraGalleryTokenTests(AstraThemeRuntimeFixture fixture) => _fixture = fixture;

    public void Dispose() => _fixture.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Light));

    [Fact]
    public void The_grid_the_tokens_page_paints_is_the_palette_the_kernel_publishes()
    {
        var inventory = InventoryTokens(ReadRepositoryFile(KeysPath));
        var page = PageTokens(ReadRepositoryFile(PageListPath));

        Assert.NotEmpty(inventory);
        Assert.Multiple(
            () => Assert.Equal(inventory.Count, page.Count),
            () => Assert.Equal(inventory, page));
    }

    /// <summary>
    /// The half the page cannot assert for itself: the Tokens readout names an unresolved token on screen, and a
    /// readout nobody reads is not evidence. Every name in the inventory has to resolve to a brush in both variants,
    /// which is also the precondition for the swatch grid following a theme flip - a row that resolves to a non-brush
    /// (a Color, say) paints nothing and the grid just looks thinner.
    /// </summary>
    [Theory]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.Dark)]
    public void Every_token_the_grid_paints_from_resolves_to_a_brush(FluentThemeVariant variant)
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(variant);
            try
            {
                var notABrush = InventoryTokens(ReadRepositoryFile(KeysPath))
                    .Where(key => Application.Current!.TryFindResource(key) is not Brush)
                    .ToList();
                Assert.True(notABrush.Count == 0,
                    $"{notABrush.Count} of the published brush tokens do not resolve to a Brush in {variant}: "
                    + string.Join(", ", notABrush));
            }
            finally
            {
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            }
        });
    }

    // ---------- readers ----------

    /// <summary>
    /// The SolidColorBrush rows of the Light block: the row's type cell is plain text, so the first backticked span on
    /// the line is the key.
    /// </summary>
    private static List<string> InventoryTokens(string keys)
    {
        var tokens = new List<string>();
        var inLight = false;
        foreach (var line in keys.Split('\n'))
        {
            var text = line.TrimEnd('\r');
            if (text.StartsWith("### ThemeResources/Light.jalxaml", StringComparison.Ordinal))
            {
                inLight = true;
                continue;
            }

            if (text.StartsWith("### ", StringComparison.Ordinal))
            {
                if (inLight) break;
                continue;
            }

            if (!inLight || !text.StartsWith("| SolidColorBrush |", StringComparison.Ordinal)) continue;
            var match = Regex.Match(text, "`([^`]+)`");
            if (match.Success) tokens.Add(match.Groups[1].Value);
        }

        return tokens;
    }

    private static List<string> PageTokens(string source)
    {
        var tokens = new List<string>();
        foreach (Match match in Regex.Matches(source, "^[\\t ]*\"([^\"]+)\"", RegexOptions.Multiline))
        {
            tokens.Add(match.Groups[1].Value);
        }

        return tokens;
    }

    private static string ReadRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate)) return File.ReadAllText(candidate);
            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find {relativePath} walking up from {AppContext.BaseDirectory}.");
    }
}
