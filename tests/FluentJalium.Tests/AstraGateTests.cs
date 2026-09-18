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
    private static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

    [Fact]
    public void Dictionary_manifest_resolves_against_the_embedded_resources()
    {
        var names = FluentThemeManager.DictionaryNames;
        Assert.NotEmpty(names);
        Assert.Contains("Controls/Navigation.jalxaml", names);
    }

    [Fact]
    public void Palette_dictionaries_declare_the_same_keys()
    {
        var light = ResourceKeys("Resources/Light.jalxaml");
        var dark = ResourceKeys("Resources/Dark.jalxaml");
        Assert.NotEmpty(light);
        Assert.Empty(dark.Except(light).OrderBy(static key => key, StringComparer.Ordinal));
        Assert.Empty(light.Except(dark).OrderBy(static key => key, StringComparer.Ordinal));
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

    private static HashSet<string> ResourceKeys(string embeddedResourceName)
    {
        var assembly = typeof(FluentThemeManager).Assembly;
        using var stream = assembly.GetManifestResourceStream(embeddedResourceName)
            ?? throw new InvalidOperationException($"Missing Astra resource: {embeddedResourceName}");
        return XDocument.Load(stream).Descendants()
            .Select(element => element.Attribute(XamlNamespace + "Key")?.Value)
            .Where(static value => !string.IsNullOrEmpty(value))
            .Select(static value => value!)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FluentJalium.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Cannot locate the repository root.");
    }
}
