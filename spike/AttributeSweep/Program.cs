// Census: which markup attributes have no member to receive them?
//
// The dead-cell sweep (docs/astra/adaptation/00 S1-f) proved that a `<Setter TargetName="X" Property="Foreground">`
// aimed at a part whose type has no such member loads, builds and paints nothing. Template *attributes* are the same
// shape - the reader accepts `BorderBrush="..."` on a `<Grid>` because a Grid is a perfectly good element to write
// it on - so this walks the shipped markup, reflects every attribute against the declared element type, and prints
// what has no receiver. Two passes: attributes without a dot have to be a member of the element's own type,
// attributes with a dot have to be an attached member of the prefix type. Nothing opens a window.
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using FluentJalium.Themes;
using Jalium.UI.Controls;

internal static class Program
{
    private const BindingFlags Instance = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
    private const BindingFlags Static = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    private static readonly Dictionary<string, List<Type>> Types = new();
    private static readonly HashSet<string> MarkupNames = new(StringComparer.Ordinal)
    {
        "Name", "Key", "Uid", "Class", "ClassModifier", "Shared", "NullExtension", "TypeExtension",
    };

    private static int Main()
    {
        var root = RepositoryRoot();
        foreach (var assembly in new[]
                 {
                     typeof(Button).Assembly, typeof(FluentThemeManager).Assembly,
                     typeof(Jalium.UI.DependencyObject).Assembly, typeof(Jalium.UI.Media.Brush).Assembly,
                 })
        {
            Type[] exported;
            try
            {
                exported = assembly.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException error)
            {
                exported = error.Types.Where(static type => type is not null).ToArray()!;
            }

            foreach (var type in exported)
            {
                if (!Types.TryGetValue(type.Name, out var list))
                {
                    Types[type.Name] = list = new List<Type>();
                }

                list.Add(type);
            }
        }

        var directOffenders = new List<string>();
        var attachedOffenders = new List<string>();
        var unknownElements = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var checkedAttributes = 0;
        var checkedElements = 0;

        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "src", "FluentJalium"), "*.jalxaml",
                     SearchOption.AllDirectories).OrderBy(static path => path))
        {
            var relative = Path.GetRelativePath(root, file);
            var lineOf = File.ReadAllLines(file);
            XDocument document;
            try
            {
                document = XDocument.Load(file, LoadOptions.SetLineInfo);
            }
            catch (Exception error)
            {
                Console.WriteLine($"UNPARSED {relative}: {error.GetType().Name}");
                continue;
            }

            foreach (var element in document.Descendants())
            {
                var local = element.Name.LocalName;
                // A dotted element name is a property path (ControlTemplate.Triggers, Style.Setters), not a type.
                if (local.Contains('.'))
                {
                    continue;
                }

                var owners = Resolve(local);
                if (owners is null)
                {
                    unknownElements[local] = unknownElements.GetValueOrDefault(local) + 1;
                    continue;
                }

                checkedElements++;
                foreach (var attribute in element.Attributes())
                {
                    if (attribute.IsNamespaceDeclaration || attribute.Name.NamespaceName.Length > 0)
                    {
                        continue; // x:Key and friends belong to the markup compiler, not to the element.
                    }

                    var name = attribute.Name.LocalName;
                    var line = ((IXmlLineInfo)attribute).LineNumber;
                    var where = $"{relative}:{line} <{local} {name}>";

                    if (name.Contains('.'))
                    {
                        var prefix = name[..name.IndexOf('.')];
                        var member = name[(name.IndexOf('.') + 1)..];
                        var holders = Resolve(prefix);
                        if (holders is null)
                        {
                            attachedOffenders.Add($"{where} - prefix {prefix} is not an exported type");
                            continue;
                        }

                        checkedAttributes++;
                        if (!holders.Any(type => IsAttachedOn(type, member)))
                        {
                            attachedOffenders.Add($"{where} - {prefix} exposes no attached {member}");
                        }

                        continue;
                    }

                    if (MarkupNames.Contains(name))
                    {
                        continue;
                    }

                    checkedAttributes++;
                    if (!owners.Any(type => HasMember(type, name)))
                    {
                        var kinds = string.Join("|", owners.Select(type => type.Name).Distinct());
                        directOffenders.Add($"{where} - {kinds} has no {name} " +
                            $"(value: {Trim(attribute.Value, lineOf, line)})");
                    }
                }
            }
        }

        Console.WriteLine($"elements checked: {checkedElements}, attributes checked: {checkedAttributes}");
        Console.WriteLine($"unknown element names (not exported by the runtime): {unknownElements.Sum(static pair => pair.Value)}");
        foreach (var (name, count) in unknownElements)
        {
            Console.WriteLine($"  ? {name} x{count}");
        }

        Console.WriteLine();
        Console.WriteLine($"direct attributes with no member on the element type: {directOffenders.Count}");
        foreach (var group in directOffenders
                     .GroupBy(static line => line.Split(" - ", 3)[1])
                     .OrderByDescending(static group => group.Count()))
        {
            Console.WriteLine($"  {group.Key}  x{group.Count()}");
            foreach (var line in group)
            {
                Console.WriteLine($"      {line.Split(" - ", 2)[0]}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"attached attributes with no member on the prefix type: {attachedOffenders.Count}");
        foreach (var line in attachedOffenders.Take(60))
        {
            Console.WriteLine($"  {line}");
        }

        return 0;
    }

    private static List<Type>? Resolve(string name) => Types.TryGetValue(name, out var list) ? list : null;

    private static bool HasMember(Type type, string name) =>
        type.GetProperty(name, Instance) is not null
        || type.GetField(name, Instance) is not null
        || type.GetEvent(name, Instance) is not null
        || type.GetMethod("Get" + name, Static) is not null;

    private static bool IsAttachedOn(Type type, string member) =>
        type.GetProperty(member, Static) is not null
        || type.GetField(member + "Property", Static) is not null
        || type.GetMethod("Get" + member, Static) is not null
        || type.GetProperty(member, Instance) is not null;

    private static string Trim(string value, string[] lines, int line)
    {
        var text = value.Length > 60 ? value[..60] + "..." : value;
        return line - 1 < lines.Length ? $"{text}" : text;
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FluentJalium.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("no FluentJalium.slnx above the probe");
    }
}
