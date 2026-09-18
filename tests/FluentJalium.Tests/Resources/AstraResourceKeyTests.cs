using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FluentJalium.Themes;

namespace FluentJalium.Tests.Resources;

/// <summary>
/// Guards the one failure mode Jalium does not report: referencing a resource key that was never
/// declared fails silently and leaves the property at its default value, so both a green build and
/// a rendered window still look correct. This gate reads the shipped dictionaries as XML and
/// resolves every reference against the union of every declared key.
/// It catches typos, deletions and renames. It deliberately does not model lookup order or
/// template-scoped resources; those stay the job of the per-control pixel tests.
/// </summary>
public class AstraResourceKeyTests
{
    private static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

    private static readonly Regex MarkupReference = new(
        @"^\{\s*(StaticResource|ThemeResource|DynamicResource)\s+(?:Key=)?([A-Za-z0-9_.]+)\s*\}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [Fact]
    public void Every_referenced_key_is_declared()
    {
        var dictionaries = AstraDictionaries();
        var declared = new HashSet<string>(StringComparer.Ordinal);
        foreach (var dictionary in dictionaries.Values)
        {
            foreach (var key in DeclaredKeys(dictionary)) declared.Add(key);
        }

        Assert.False(declared.Count == 0);

        var offenders = new List<string>();
        foreach (var (name, dictionary) in dictionaries)
        {
            foreach (var reference in ReferencedKeys(dictionary))
            {
                if (!declared.Contains(reference)) offenders.Add($"{name}: {reference}");
            }
        }

        offenders.Sort(StringComparer.Ordinal);
        Assert.False(offenders.Count > 0, "Undeclared resource references:" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void Palette_declares_the_same_keys_in_both_theme_branches()
    {
        var branches = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var dictionary in AstraDictionaries().Values)
        {
            foreach (var (name, branch) in ThemeBranches(dictionary))
            {
                if (!branches.TryGetValue(name, out var set))
                {
                    set = [];
                    branches[name] = set;
                }

                foreach (var key in DeclaredKeys(branch)) set.Add(key);
            }
        }

        if (branches.Count < 2)
        {
            // Flat-palette form: the two files must still agree key for key.
            var light = DeclaredKeys(Load("Resources/Light.jalxaml") ?? throw new InvalidOperationException("No palette to check.")).ToHashSet(StringComparer.Ordinal);
            var dark = DeclaredKeys(Load("Resources/Dark.jalxaml") ?? throw new InvalidOperationException("No palette to check.")).ToHashSet(StringComparer.Ordinal);
            Assert.False(light.Count == 0);
            Assert.Empty(dark.Except(light).Order(StringComparer.Ordinal));
            Assert.Empty(light.Except(dark).Order(StringComparer.Ordinal));
            return;
        }

        var reference = branches.OrderBy(static entry => entry.Key, StringComparer.Ordinal).First();
        foreach (var (name, keys) in branches)
        {
            Assert.False(keys.Except(reference.Value).Any(), $"branch {name} declares keys absent from {reference.Key}");
            Assert.False(reference.Value.Except(keys).Any(), $"branch {name} is missing keys present in {reference.Key}");
        }
    }

    /// <summary>A <c>{ThemeResource}</c> reference must resolve in every theme branch, or it silently keeps the previous theme's value.</summary>
    [Fact]
    public void Theme_resource_references_resolve_in_every_branch()
    {
        var dictionaries = AstraDictionaries();
        var branches = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        void AddBranch(string name, IEnumerable<string> keys)
        {
            if (!branches.TryGetValue(name, out var set))
            {
                set = [];
                branches[name] = set;
            }

            foreach (var key in keys) set.Add(key);
        }

        foreach (var dictionary in dictionaries.Values)
        {
            foreach (var (name, branch) in ThemeBranches(dictionary)) AddBranch(name, DeclaredKeys(branch));
        }

        if (branches.Count == 0) return;

        var shared = branches.Values.First().ToHashSet(StringComparer.Ordinal);
        foreach (var keys in branches.Values) shared.IntersectWith(keys);
        var offenders = new List<string>();
        foreach (var (name, dictionary) in dictionaries)
        {
            foreach (var (element, key) in ThemedReferences(dictionary))
            {
                if (!shared.Contains(key)) offenders.Add($"{name}: <{element.Name.LocalName}> uses {{ThemeResource {key}}}, missing from at least one theme branch");
            }
        }

        offenders.Sort(StringComparer.Ordinal);
        Assert.False(offenders.Count > 0, "Theme references that cannot resolve in every branch:" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    /// <summary>
    /// A key that the palette declares with a different value per theme is a theme token, and a
    /// theme token has to be consumed as one. <c>{StaticResource}</c> freezes the lookup: today the
    /// frozen value is the same brush instance the palette retints in place, so it still looks right,
    /// but it no longer follows a per-branch token, and no upstream WinUI markup does it this way.
    /// This is the invariant behind the <c>{ThemeResource}</c> conversion in the control dictionaries.
    /// </summary>
    [Fact]
    public void Theme_tokens_are_never_frozen_with_static_resource()
    {
        var palette = new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in new[] { "Resources/Light.jalxaml", "Resources/Dark.jalxaml" })
        {
            using var stream = typeof(FluentThemeManager).Assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Missing Astra resource: {name}");
            foreach (var key in DeclaredKeys(XDocument.Load(stream))) palette.Add(key);
        }

        Assert.NotEmpty(palette);

        var offenders = new List<string>();
        foreach (var (name, dictionary) in AstraDictionaries())
        {
            if (name.EndsWith("Light.jalxaml", StringComparison.Ordinal) || name.EndsWith("Dark.jalxaml", StringComparison.Ordinal)) continue;
            foreach (var (attribute, key) in FrozenReferences(dictionary))
            {
                if (palette.Contains(key)) offenders.Add($"{name}: {attribute.Name.LocalName}=\"{{StaticResource {key}}}\"");
            }
        }

        offenders.Sort(StringComparer.Ordinal);
        Assert.False(offenders.Count > 0, "Theme tokens frozen at parse time:" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    private static Dictionary<string, XDocument> AstraDictionaries()
    {
        var assembly = typeof(FluentThemeManager).Assembly;
        var documents = new Dictionary<string, XDocument>(StringComparer.Ordinal);
        foreach (var resourceName in assembly.GetManifestResourceNames()
                     .Where(static name => name.Replace('\\', '/').StartsWith("Resources/", StringComparison.Ordinal) && name.EndsWith(".jalxaml", StringComparison.Ordinal))
                     .Order(StringComparer.Ordinal))
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            documents[resourceName.Replace('\\', '/')] = XDocument.Load(stream);
        }

        Assert.False(documents.Count == 0, "No Astra dictionaries are embedded; this gate would pass vacuously.");
        return documents;
    }

    private static XDocument? Load(string resourceName)
    {
        using var stream = typeof(FluentThemeManager).Assembly.GetManifestResourceStream(resourceName);
        return stream is null ? null : XDocument.Load(stream);
    }

    private static IEnumerable<string> DeclaredKeys(XContainer root) =>
        root.Descendants().Select(element => element.Attribute(XamlNamespace + "Key")?.Value)
            .Where(static value => !string.IsNullOrEmpty(value)).Select(static value => value!);

    private static IEnumerable<string> ReferencedKeys(XContainer root)
    {
        foreach (var element in root.Descendants())
        {
            foreach (var attribute in element.Attributes())
            {
                if (attribute.Name == XamlNamespace + "Key") continue;
                var match = MarkupReference.Match(attribute.Value);
                if (match.Success) yield return match.Groups[2].Value;
            }

            if (element.Name.LocalName is "StaticResource" or "ThemeResource" or "DynamicResource"
                && element.Attribute("ResourceKey")?.Value is { Length: > 0 } resourceKey)
            {
                yield return resourceKey;
            }
        }
    }

    private static IEnumerable<(XAttribute Attribute, string Key)> FrozenReferences(XContainer root)
    {
        foreach (var element in root.Descendants())
        {
            foreach (var attribute in element.Attributes())
            {
                var match = MarkupReference.Match(attribute.Value);
                if (match.Success && match.Groups[1].Value == "StaticResource") yield return (attribute, match.Groups[2].Value);
            }
        }
    }

    private static IEnumerable<(XElement Element, string Key)> ThemedReferences(XContainer root)
    {
        foreach (var element in root.Descendants())
        {
            foreach (var attribute in element.Attributes())
            {
                var match = MarkupReference.Match(attribute.Value);
                if (match.Success && match.Groups[1].Value == "ThemeResource") yield return (element, match.Groups[2].Value);
            }
        }
    }

    private static IEnumerable<(string Name, XContainer Branch)> ThemeBranches(XContainer root)
    {
        foreach (var container in root.Descendants().Where(static element => element.Name.LocalName == "ThemeDictionaries"))
        {
            foreach (var branch in container.Elements())
            {
                if (branch.Attribute(XamlNamespace + "Key")?.Value is { Length: > 0 } name) yield return (name, branch);
            }
        }
    }
}
