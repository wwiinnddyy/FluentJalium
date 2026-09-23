using System.Text.RegularExpressions;
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

    /// <summary>
    /// The other half of the setter gate, added after the TreeView probe measured that this runtime's
    /// <c>ContentPresenter</c> has no <c>Foreground</c> member at all (spike/TreeViewProbe mode K,
    /// <c>docs/astra/adaptation/00</c> S1-e 8). The exclusion above therefore threw away the largest class of dead
    /// cells: fifty state setters across six families aimed at a label presenter, each of which builds, loads and
    /// paints nothing while the family's audit claims the state is implemented. A cell aimed at a named part is now
    /// resolved against the element type that part is declared as in the same template.
    /// </summary>
    [Fact]
    public void State_cells_name_properties_the_template_parts_actually_have()
    {
        var root = RepositoryRoot();
        var types = typeof(Jalium.UI.Controls.Button).Assembly.GetTypes().Concat(typeof(FluentThemeManager).Assembly.GetTypes())
            .Where(static type => type.IsPublic && type.IsSubclassOf(typeof(Jalium.UI.DependencyObject)))
            .ToList();
        var offenders = new List<string>();
        var checkedCells = 0;
        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "src", "FluentJalium"), "*.jalxaml", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            foreach (var template in XDocument.Load(file).Descendants().Where(static element => element.Name.LocalName == "ControlTemplate"))
            {
                var parts = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var element in template.Descendants())
                {
                    var name = element.Attributes().FirstOrDefault(static attribute => attribute.Name.LocalName == "Name")?.Value;
                    if (name is not null) parts[name] = element.Name.LocalName;
                }

                foreach (var setter in template.Descendants().Where(static element => element.Name.LocalName == "Setter"))
                {
                    var target = setter.Attributes().FirstOrDefault(static attribute => attribute.Name.LocalName == "TargetName")?.Value;
                    var property = setter.Attributes().FirstOrDefault(static attribute => attribute.Name.LocalName == "Property")?.Value;
                    if (target is null || property is null || property.Contains('.', StringComparison.Ordinal)) continue;

                    // A target this template never names is the other silent-drop class, and it is an offender too:
                    // the cell cannot reach an element that does not exist.
                    if (!parts.TryGetValue(target, out var partType))
                    {
                        offenders.Add($"{relative}: cell aimed at part {target}, which this template does not declare");
                        continue;
                    }

                    var owner = types.FirstOrDefault(type => type.Name == partType);
                    if (owner is null) continue;
                    checkedCells++;
                    if (owner.GetProperty(property, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) is null)
                        offenders.Add($"{relative}: {partType} '{target}' has no {property}");
                }
            }
        }

        Assert.True(checkedCells > 100, $"only {checkedCells} cells were checked; the gate has gone vacuous.");
        offenders.Sort(StringComparer.Ordinal);
        Assert.False(offenders.Count > 0, $"Cells writing a property their target does not have ({offenders.Count}):" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    /// <summary>
    /// The same rule, one layer down: an attribute an element's type does not declare is the identical silent drop,
    /// and it is the layer the cell gate cannot see. The attribute sweep (docs/astra/adaptation/00 S1-g) found 35 of
    /// them shipped in templates that looked finished - a <c>CornerRadius</c> on a <c>Grid</c>, a
    /// <c>TextWrapping</c> on a <c>ContentPresenter</c>, a <c>Padding</c> on a <c>StackPanel</c> - four of which were
    /// the whole reason a surface came out square: the NumberBox spinner popup, the InfoBar content root and panel,
    /// the TeachingTip card and the menu bar item's fill bleeding past its own rounded border.
    ///
    /// Element names the runtime does not export are skipped rather than failed: <c>StaticResource</c> is a markup
    /// extension with no type to check, and the reader resolves those by other means. Namespaced attributes are the
    /// markup compiler's (<c>x:Key</c>), and a dotted element name is a property path, not an element.
    /// </summary>
    [Fact]
    public void Template_attributes_name_members_the_element_type_actually_has()
    {
        var root = RepositoryRoot();
        var types = typeof(Jalium.UI.Controls.Button).Assembly.GetTypes()
            .Concat(typeof(FluentThemeManager).Assembly.GetTypes())
            .Where(static type => type.IsPublic && type.IsSubclassOf(typeof(Jalium.UI.DependencyObject)))
            .ToList();
        var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
        var staticFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
        var offenders = new List<string>();
        var checkedAttributes = 0;

        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "src", "FluentJalium"), "*.jalxaml", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(root, file);
            foreach (var element in XDocument.Load(file).Descendants())
            {
                var localName = element.Name.LocalName;
                if (localName.Contains('.', StringComparison.Ordinal))
                {
                    continue;
                }

                var owner = types.FirstOrDefault(type => type.Name == localName);
                if (owner is null)
                {
                    continue;
                }

                foreach (var attribute in element.Attributes())
                {
                    if (attribute.IsNamespaceDeclaration || attribute.Name.NamespaceName.Length > 0)
                    {
                        continue;
                    }

                    var name = attribute.Name.LocalName;
                    if (name is "Name" or "Key" or "Uid" or "Class")
                    {
                        continue;
                    }

                    checkedAttributes++;
                    if (name.Contains('.', StringComparison.Ordinal))
                    {
                        var split = name.IndexOf('.');
                        var holder = types.FirstOrDefault(type => type.Name == name[..split]);
                        var member = name[(split + 1)..];
                        if (holder is null)
                        {
                            offenders.Add($"{relative}: <{localName} {name}> names a prefix type {name[..split]} the runtime does not export");
                        }
                        else if (holder.GetProperty(member, flags) is null && holder.GetProperty(member, staticFlags) is null
                                     && holder.GetField(member + "Property", staticFlags) is null
                                     && holder.GetMethod("Get" + member, staticFlags) is null)
                        {
                            offenders.Add($"{relative}: <{localName} {name}> - {holder.Name} exposes no attached {member}");
                        }

                        continue;
                    }

                    if (owner.GetProperty(name, flags) is null && owner.GetEvent(name, flags) is null
                        && owner.GetMethod("Get" + name, staticFlags) is null)
                    {
                        offenders.Add($"{relative}: <{localName} {name}> - {owner.Name} has no {name}");
                    }
                }
            }
        }

        // The floor is lower than spike/AttributeSweep's count on purpose: this universe is DependencyObject
        // subclasses, so markup types that carry no dispatcher-side members (Color, Thickness, Geometry, key
        // frames) are skipped here and walked there. Both read zero offenders.
        Assert.True(checkedAttributes > 1500, $"only {checkedAttributes} attributes were checked; the gate has gone vacuous.");
        offenders.Sort(StringComparer.Ordinal);
        Assert.False(offenders.Count > 0, $"Attributes no element type can hold ({offenders.Count}):" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    /// <summary>
    /// Cross-reads the generated known-gap inventory against the ledger with its own parser, the same way the
    /// suite cross-reads the palette. The generator's <c>-Check</c> leg in the serial gate compares whole-file
    /// text; this one asks a question a test can answer without the generator - whether every document that
    /// states a gap (a "Known Gap" heading or a marker in prose) appears in the index at all - so a gap added
    /// to an audit and never regenerated goes red here even when that gate step is skipped.
    /// </summary>
    [Fact]
    public void The_known_gap_inventory_covers_every_document_that_states_one()
    {
        const string marker = "Known Gap";
        var docs = Path.Combine(RepositoryRoot(), "docs", "astra");
        var index = Path.Combine(docs, "audits", "known-gaps.md");
        Assert.True(File.Exists(index), $"{index} is missing; run tools/Report-AstraKnownGaps.ps1.");

        var stating = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var file in Directory.EnumerateFiles(docs, "*.md", SearchOption.AllDirectories))
        {
            if (string.Equals(file, index, StringComparison.Ordinal)) continue;
            var relative = Path.GetRelativePath(docs, file).Replace('\\', '/');
            if (File.ReadLines(file).Any(static line => line.Contains(marker, StringComparison.Ordinal))) stating.Add(relative);
        }

        var listed = new SortedSet<string>(StringComparer.Ordinal);
        // The generated document is CRLF, and a multiline `$` in this runtime binds before `\n` only, so the
        // group headers would never match without normalising the newlines first.
        var inventory = File.ReadAllText(index).Replace("\r\n", "\n");
        foreach (Match match in Regex.Matches(inventory, @"^## (\S+\.md) - \d+ lines$", RegexOptions.Multiline))
        {
            listed.Add(match.Groups[1].Value);
        }

        Assert.True(stating.Count > 20, $"only {stating.Count} documents state a Known Gap; the ledger has gone quiet.");
        var missing = stating.Except(listed).ToArray();
        var stale = listed.Except(stating).ToArray();
        Assert.False(missing.Length > 0,
            "Documents state a Known Gap but the inventory does not carry them (run tools/Report-AstraKnownGaps.ps1):"
            + Environment.NewLine + string.Join(Environment.NewLine, missing));
        Assert.False(stale.Length > 0,
            "The inventory lists documents that no longer state a Known Gap (run tools/Report-AstraKnownGaps.ps1):"
            + Environment.NewLine + string.Join(Environment.NewLine, stale));
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FluentJalium.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Cannot locate the repository root.");
    }
}
