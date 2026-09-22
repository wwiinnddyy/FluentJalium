using System.Text.RegularExpressions;

namespace FluentJalium.Tests.Gallery;

/// <summary>
/// The Tokens page paints one swatch per palette brush token from a list generated out of the canonical inventory,
/// which makes that list the part that can go stale: a token renamed in the theme layer would otherwise keep showing
/// up as a swatch nobody notices is missing, and the page's whole claim is that the grid is the set the kernel
/// publishes. So the two readers are independent here, the same way the catalog gate keeps its own JSON reader apart
/// from the Gallery's - this test parses <c>docs/astra/audits/keys.md</c> with its own text walk and the generated
/// array with a regex, and fails when the two lists part ways in either direction or in order.
/// </summary>
public class AstraGalleryTokenTests
{
    private const string KeysPath = "docs/astra/audits/keys.md";
    private const string PageListPath = "samples/FluentJalium.Gallery/TokenCatalog.cs";

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
