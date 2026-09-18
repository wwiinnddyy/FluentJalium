using System.Xml.Linq;
using FluentJalium.Themes;

namespace FluentJalium.Tests;

/// <summary>
/// Structural gates. Each one replaces a check that used to live in a script someone had to
/// remember to run, which is how two non-existent dictionaries ended up in the load list and
/// crashed every host on startup.
/// </summary>
public class AstraGateTests
{
    [Fact]
    public void Dictionary_manifest_resolves_against_the_embedded_resources()
    {
        var names = FluentThemeManager.DictionaryNames;
        Assert.NotEmpty(names);
        Assert.Contains("Styles/Navigation.jalxaml", names);
    }

    [Fact]
    public void Markup_avoids_extensions_jalium_drops_silently()
    {
        var root = RepositoryRoot();
        var offenders = new List<string>();
        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.jalxaml", SearchOption.AllDirectories))
        {
            // {x:Bind} parses without error yet never reaches type resolution, so a green build
            // is not evidence that the markup bound anything. Attribute values only: a comment
            // mentioning it is not a use of it.
            var bound = XDocument.Load(file).Descendants().Attributes().Any(static attribute =>
                attribute.Value.StartsWith("{x:Bind", StringComparison.Ordinal));
            if (bound) offenders.Add(Path.GetRelativePath(root, file));
        }
        Assert.Empty(offenders);
    }

    /// <summary>
    /// AGENTS.md forbids three things Astra previously did: walk the visual tree to restyle live
    /// controls, invalidate every window after a theme mutation, and reflect into private framework
    /// members. Keeping them out is a text gate because nothing else would notice their return, and
    /// the experimental-API suppression must stay confined to one file.
    /// </summary>
    [Fact]
    public void Theme_kernel_stays_free_of_repair_loops_and_reflection()
    {
        var root = RepositoryRoot();
        var offenders = new List<string>();
        var suppressions = 0;
        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "src", "FluentJalium"), "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            var relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (text.Contains("#pragma warning disable WPF0001", StringComparison.Ordinal))
            {
                suppressions++;
                if (relative != "src/FluentJalium/Themes/ApplicationThemeDriver.cs") offenders.Add($"{relative}: suppresses WPF0001 outside the driver");
            }

            foreach (var banned in new[] { "VisualTreeHelper", ".InvalidateVisual(", "BindingFlags.NonPublic", "GetField(", "GetMethod(" })
                if (text.Contains(banned, StringComparison.Ordinal)) offenders.Add($"{relative}: {banned}");
        }

        Assert.Equal(1, suppressions);
        offenders.Sort(StringComparer.Ordinal);
        Assert.False(offenders.Count > 0, "Forbidden implementation pattern:" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FluentJalium.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Cannot locate the repository root.");
    }
}
