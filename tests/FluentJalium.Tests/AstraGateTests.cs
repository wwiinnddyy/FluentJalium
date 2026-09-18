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

    /// <summary>
    /// The other half of the silent-drop family: an attribute the reader cannot resolve is discarded
    /// without a word, so a style setter aimed at a property the control does not have builds, loads and
    /// paints - showing whatever the previous token painted. Every setter and condition this repository
    /// writes against a control itself is checked against the properties that control really has. Setters
    /// aimed at a named template part are excluded: those defer to the part's type at runtime, which the
    /// per-control read-back tests cover.
    /// </summary>
    [Fact]
    public void Style_setters_name_properties_the_controls_actually_have()
    {
        var root = RepositoryRoot();
        var types = typeof(Jalium.UI.Controls.Button).Assembly.GetTypes().Concat(typeof(FluentThemeManager).Assembly.GetTypes())
            .Where(static type => type.IsPublic && type.IsSubclassOf(typeof(Jalium.UI.DependencyObject)))
            .ToList();
        var offenders = new List<string>();
        var checkedSetters = 0;
        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "src", "FluentJalium", "Styles"), "*.jalxaml", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            foreach (var style in XDocument.Load(file).Descendants().Where(static element => element.Name.LocalName == "Style"))
            {
                var target = style.Attributes().First(static attribute => attribute.Name.LocalName == "TargetType").Value;
                // The prefix names the owning assembly's xmlns, not a different type, and Border/TextBlock are
                // styled too - so the lookup runs over every public DependencyObject, not only Controls.
                var name = target[(target.IndexOf(':', StringComparison.Ordinal) + 1)..];
                var candidates = types.Where(type => type.Name == name).ToList();
                if (candidates.Count != 1)
                {
                    offenders.Add($"{relative}: TargetType {target} resolves to {candidates.Count} dependency types");
                    continue;
                }

                var owner = candidates[0];
                foreach (var member in style.Descendants()
                             .Where(static element => element.Name.LocalName is "Setter" or "Condition" or "Trigger"))
                {
                    // A style nested in a template owns its own setters; charging them to the outer control
                    // would invent offenders.
                    if (member.Ancestors().FirstOrDefault(static ancestor => ancestor.Name.LocalName == "Style") != style) continue;
                    var property = member.Attributes().FirstOrDefault(static attribute => attribute.Name.LocalName == "Property")?.Value;
                    if (property is null || member.Attributes().Any(static attribute => attribute.Name.LocalName == "TargetName")) continue;
                    if (property.Contains('.', StringComparison.Ordinal)) continue;
                    checkedSetters++;
                    if (owner.GetProperty(property, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) is null)
                        offenders.Add($"{relative}: {target}.{property}");
                }
            }
        }

        Assert.True(checkedSetters > 100, $"only {checkedSetters} setters were checked; the gate has gone vacuous.");
        offenders.Sort(StringComparer.Ordinal);
        Assert.False(offenders.Count > 0, "Setters naming a property that does not exist:" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    /// <summary>
    /// A ControlTemplate that lives in a dictionary resource, rather than inline in a style's Template setter,
    /// comes back from this reader with every <c>Trigger.Property</c> unresolved: the cells are in the object,
    /// their setters are named, and none of them can ever match, so the control sits in its resting state no
    /// matter what the app does. Measured on 26.10.9 with the Slider (docs/astra/audits/slider.md) - its eight
    /// cells were inert for exactly this reason while the markup looked correct and the build stayed green.
    /// A template resource with no state cells is harmless, so only the ones carrying triggers are offenders.
    /// </summary>
    [Fact]
    public void State_cells_are_not_written_into_a_keyed_template_resource()
    {
        var root = RepositoryRoot();
        var offenders = new List<string>();
        var templates = 0;
        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "src", "FluentJalium"), "*.jalxaml", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            foreach (var template in XDocument.Load(file).Descendants()
                         .Where(static element => element.Name.LocalName == "ControlTemplate"
                                                  && element.Attributes().Any(static attribute => attribute.Name.LocalName == "Key")))
            {
                templates++;
                var cells = template.Descendants().Count(static element => element.Name.LocalName is "Trigger" or "MultiTrigger");
                if (cells > 0)
                {
                    offenders.Add($"{relative}: keyed template {template.Attributes().First(static attribute => attribute.Name.LocalName == "Key").Value} carries {cells} cell(s) this reader cannot resolve");
                }
            }
        }

        Assert.True(templates < 100, $"{templates} keyed template resources - more than this gate expects; re-read it before trusting it.");
        offenders.Sort(StringComparer.Ordinal);
        Assert.False(offenders.Count > 0, "Keyed ControlTemplate resources with state cells:" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FluentJalium.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Cannot locate the repository root.");
    }
}
