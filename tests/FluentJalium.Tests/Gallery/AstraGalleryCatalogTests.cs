using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using FluentJalium.Themes;

namespace FluentJalium.Tests.Gallery;

/// <summary>
/// The gallery catalog is the only place that says which controls Astra restyles, so it is the place a
/// missing sample page shows up. This gate enumerates the shipped dictionaries and fails when the
/// catalog and the implicit-style universe part ways: an app inherits those styles silently, and a
/// control nobody demonstrates is how "it looked fine in the Gallery" stops meaning anything.
/// </summary>
/// <remarks>
/// The parser here is deliberately separate from <c>samples/FluentJalium.Gallery/GalleryCatalog.cs</c>.
/// Sharing a model would let both drift from the file in the same direction; two readers that have to
/// agree is the point.
/// </remarks>
public class AstraGalleryCatalogTests
{
    private static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

    private const string CatalogPath = "samples/FluentJalium.Gallery/Catalog.json";
    private const string GalleryMarkup = "samples/FluentJalium.Gallery/MainWindow.jalxaml";
    private const string GalleryCode = "samples/FluentJalium.Gallery/MainWindow.jalxaml.cs";
    private const string GalleryProject = "samples/FluentJalium.Gallery/FluentJalium.Gallery.csproj";

    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    private sealed record Catalog(Dictionary<string, string> ParityLegend, Page[] Pages, Restyled[] Controls);

    private sealed record Page(string Id, string Title, string Host);

    private sealed record Restyled(string Markup, string Source, string Page, string Parity, string[]? Evidence, string[]? Gaps);

    /// <summary>
    /// Controls the Gallery demonstrably uses with no Astra style of any kind. Each line is a queue
    /// entry rather than a pass, and the reason names the batch that owes it.
    /// </summary>
    private static readonly Dictionary<string, string> WaivedUnstyled = new(StringComparer.Ordinal)
    {
        ["Jalium.UI.Window"] = "Never restyled by a style: the shell is reached through the title-bar hooks and the backdrop choice, audited in audits/window-shell.md.",
        ["Jalium.UI.Controls.ListBox"] = "Owed by the list batch: host and container are restyled together; the container path itself is measured in adaptation/09.",
        ["Jalium.UI.Controls.ListBoxItem"] = "Owed by the list batch: its upstream ListItem key list is not transcribed yet.",
    };

    [Fact]
    public void The_catalog_covers_every_type_astra_restyles_implicitly()
    {
        var restyled = ImplicitlyStyledTypes();
        var catalogued = Load().Controls.Select(static control => control.Markup).ToArray();

        Assert.False(restyled.Length == 0, "No implicit style was found; this gate would pass vacuously.");

        var missing = restyled.Except(catalogued, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        var orphaned = catalogued.Except(restyled, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        Assert.False(missing.Length > 0 || orphaned.Length > 0,
            $"Catalog and shipped styles disagree: {restyled.Length} restyled, {catalogued.Length} catalogued." +
            $"{Environment.NewLine}Restyled but not catalogued: {string.Join(", ", missing)}" +
            $"{Environment.NewLine}Catalogued but not restyled: {string.Join(", ", orphaned)}");
        Assert.Equal(restyled.Length, catalogued.Length);
    }

    [Fact]
    public void Catalog_names_resolve_to_one_loaded_control_type_each()
    {
        // A catalog name is display text in the Gallery, so a typo has to be impossible. Names written
        // through a clr-namespace prefix are kept fully qualified, which is also what lets the same
        // string be looked up in the loaded assemblies.
        var candidates = CandidateAssemblies().SelectMany(ExportedTypes).ToArray();
        foreach (var control in Load().Controls)
        {
            var type = Resolve(control.Markup, candidates);
            Assert.True(typeof(Jalium.UI.FrameworkElement).IsAssignableFrom(type),
                $"{control.Markup} resolves to {type.FullName}, which is not an element.");
        }
    }

    [Fact]
    public void Every_catalog_page_is_a_page_the_gallery_hosts_and_navigates_to()
    {
        var catalog = Load();
        var markup = Read(GalleryMarkup);
        var code = Read(GalleryCode);
        var ids = catalog.Pages.Select(static page => page.Id).ToArray();

        foreach (var page in catalog.Pages)
        {
            Assert.Contains($"x:Name=\"{page.Host}\"", markup, StringComparison.Ordinal);
            Assert.Contains($"\"{page.Id}\"", code, StringComparison.Ordinal);
        }

        foreach (var control in catalog.Controls)
            Assert.Contains(control.Page, ids, StringComparer.Ordinal);
    }

    [Fact]
    public void Every_parity_claim_cites_evidence_that_exists()
    {
        var catalog = Load();
        var offenders = new List<string>();
        foreach (var control in catalog.Controls)
        {
            if (!catalog.ParityLegend.ContainsKey(control.Parity))
            {
                offenders.Add($"{control.Markup}: parity '{control.Parity}' is not in the legend");
                continue;
            }

            if (control.Evidence is not { Length: > 0 })
            {
                offenders.Add($"{control.Markup}: no evidence cited");
                continue;
            }

            foreach (var evidence in control.Evidence)
                if (!File.Exists(Path.Combine(RepositoryRoot(), evidence.Replace('/', Path.DirectorySeparatorChar))))
                    offenders.Add($"{control.Markup}: {evidence} does not exist");

            if (control.Parity != "audited") continue;
            if (!control.Evidence.Any(static path => path.StartsWith("docs/astra/audits/", StringComparison.Ordinal)))
                offenders.Add($"{control.Markup}: claims an upstream audit without citing one");
            if (!control.Evidence.Any(static path => path.StartsWith("tests/", StringComparison.Ordinal)))
                offenders.Add($"{control.Markup}: claims an upstream audit without citing a test");
        }

        Assert.False(offenders.Count > 0, "Unsustainable catalog claims:" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    /// <summary>
    /// The other half of the same question: the catalog universe cannot notice a control nobody ever
    /// restyled, and a control the Gallery shows with the framework look is the defect the whole E
    /// track exists to catch.
    /// </summary>
    [Fact]
    public void The_gallery_shows_no_control_astra_neither_restyles_nor_waives()
    {
        var candidates = CandidateAssemblies().SelectMany(ExportedTypes).ToArray();
        var document = XDocument.Parse(Read(GalleryMarkup));

        var shown = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var element in document.Descendants())
        {
            if (element.Name.NamespaceName is "" or "http://www.w3.org/2000/xmlns/"
                || element.Name.NamespaceName.StartsWith("http://schemas.microsoft.com/winfx", StringComparison.Ordinal)) continue;

            var markup = WriteAsMarkup(element);
            var type = TryResolve(markup, candidates);
            if (type is not null && typeof(Jalium.UI.Controls.Control).IsAssignableFrom(type)) shown.Add(type.FullName!);
        }

        Assert.False(shown.Count == 0, "No control was found in the gallery markup; this gate would pass vacuously.");
        var restyled = Load().Controls
            .Select(control => TryResolve(control.Markup, candidates)?.FullName ?? control.Markup)
            .ToHashSet(StringComparer.Ordinal);
        var unstyled = shown.Except(restyled, StringComparer.Ordinal).Except(WaivedUnstyled.Keys, StringComparer.Ordinal).ToArray();
        Assert.False(unstyled.Length > 0,
            "Gallery controls wearing the framework look:" + Environment.NewLine + string.Join(Environment.NewLine, unstyled));
    }

    /// <summary>
    /// The Gallery reads the catalog out of its own output folder, so the file that ships has to be the
    /// file that is gated.
    /// </summary>
    [Fact]
    public void The_gallery_project_carries_the_catalog_it_reads()
    {
        var project = Read(GalleryProject);
        Assert.Contains("Catalog.json", project, StringComparison.Ordinal);
        Assert.Contains("CopyToOutputDirectory", project, StringComparison.Ordinal);

        var source = Read(CatalogPath);
        foreach (var path in Directory.EnumerateFiles(Path.Combine(RepositoryRoot(), "samples", "FluentJalium.Gallery", "bin"),
                     "Catalog.json", SearchOption.AllDirectories))
        {
            Assert.Equal(source, File.ReadAllText(path));
        }
    }

    /// <summary>
    /// A style with a <c>TargetType</c> and no <c>x:Key</c> at the top of a product dictionary is the
    /// definition of "every app using Astra gets this look". Styles nested inside a template belong to
    /// that part and stay out of the universe.
    /// </summary>
    private static string[] ImplicitlyStyledTypes()
    {
        var assembly = typeof(FluentThemeManager).Assembly;
        var names = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var resourceName in assembly.GetManifestResourceNames()
                     .Where(static name => name.EndsWith(".jalxaml", StringComparison.Ordinal)))
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            var dictionary = XDocument.Load(stream);
            foreach (var style in dictionary.Root!.Elements().Where(static element => element.Name.LocalName == "Style"))
            {
                if (style.Attribute(XamlNamespace + "Key") is not null) continue;
                if (style.Attribute("TargetType")?.Value is not { Length: > 0 } target) continue;
                names.Add(Normalize(target, dictionary));
            }
        }

        return names.ToArray();
    }

    /// <summary><c>local:FluentNavigationView</c> becomes <c>FluentJalium.Controls.FluentNavigationView</c>.</summary>
    private static string Normalize(string target, XDocument dictionary)
    {
        var separator = target.IndexOf(':');
        if (separator < 0) return target;
        var declaration = dictionary.Root!.Attribute(XNamespace.Xmlns + target[..separator])?.Value;
        return declaration is null ? target : Qualify(declaration, target[(separator + 1)..]);
    }

    /// <summary>How the markup would have written this element: a bare name, or prefix-resolved.</summary>
    private static string WriteAsMarkup(XElement element) =>
        element.Name.NamespaceName == "http://schemas.jalium.ui/2024"
            ? element.Name.LocalName
            : Qualify(element.Name.NamespaceName, element.Name.LocalName);

    private static string Qualify(string declaration, string localName)
    {
        var clrNamespace = declaration.Split(';')
            .FirstOrDefault(static part => part.StartsWith("clr-namespace:", StringComparison.Ordinal))?["clr-namespace:".Length..];
        return clrNamespace is null ? localName : $"{clrNamespace}.{localName}";
    }

    private static Catalog Load()
    {
        var catalog = JsonSerializer.Deserialize<Catalog>(Read(CatalogPath), SerializerOptions)
            ?? throw new InvalidOperationException($"{CatalogPath} did not deserialize.");
        Assert.False(catalog.Controls.Length == 0, $"{CatalogPath} lists no controls.");
        Assert.False(catalog.ParityLegend.Count == 0, $"{CatalogPath} has no parity legend.");
        return catalog;
    }

    private static IEnumerable<Assembly> CandidateAssemblies()
    {
        // Anchors rather than a name filter: the control types are spread over the metapackage's
        // assemblies, and this must work whether or not they happened to be touched yet.
        var anchors = new[]
        {
            typeof(Jalium.UI.FrameworkElement).Assembly,
            typeof(Jalium.UI.Controls.Button).Assembly,
            typeof(Jalium.UI.Controls.Primitives.ToggleButton).Assembly,
            typeof(FluentThemeManager).Assembly,
        };
        return anchors.Concat(AppDomain.CurrentDomain.GetAssemblies()
                .Where(static assembly => assembly.GetName().Name?.StartsWith("Jalium.UI", StringComparison.Ordinal) == true))
            .Distinct();
    }

    private static IEnumerable<Type> ExportedTypes(Assembly assembly)
    {
        try
        {
            return assembly.ExportedTypes;
        }
        catch (Exception exception) when (exception is ReflectionTypeLoadException or NotSupportedException)
        {
            return [];
        }
    }

    private static Type Resolve(string name, IReadOnlyCollection<Type> candidates) =>
        TryResolve(name, candidates) ?? throw new InvalidOperationException($"{name} resolves to no loaded type, or to more than one.");

    private static Type? TryResolve(string name, IReadOnlyCollection<Type> candidates) =>
        (name.Contains('.')
            ? candidates.Where(type => type.FullName == name)
            : candidates.Where(type => type.Name == name)).ToArray() is { Length: 1 } matches ? matches[0] : null;

    private static string Read(string relativePath) =>
        File.ReadAllText(Path.Combine(RepositoryRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FluentJalium.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Cannot locate the repository root.");
    }
}
