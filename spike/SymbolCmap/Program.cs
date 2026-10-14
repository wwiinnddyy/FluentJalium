using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Jalium.UI.Controls;

namespace SymbolCmap;

/// <summary>
/// What the stage-6 icon family has to be measured on before a line of it is written: which types the pinned
/// 26.10.9 runtime really exports, which enumeration <c>SymbolIcon.Symbol</c> declares, and how that set relates
/// to the enum upstream WinUI declares. Names and numbers only - every line is a read-back off the assembly the
/// test project resolves, never off a source file that happens to be checked out next to it.
/// </summary>
/// <remarks>
/// Two earlier readings of this surface claimed absence, and both were the instrument: <c>Assembly.LoadFrom</c>
/// from Windows PowerShell 5.1, which runs on .NET Framework. A net10.0 assembly loads there and then throws on
/// the first <c>GetTypes()</c>, and an empty catch turns that throw into "nothing named Symbol found".
/// <c>Jalium.UI.Controls.dll</c> is additionally a type-forwarding shell (measured in spike/RatingProbe), so the
/// enum's real name has to be resolved through a live type reference rather than guessed from the source tree.
/// </remarks>
internal static class Program
{
    private static readonly List<string> Lines = [];

    private static int Main()
    {
        var repoRoot = FindRepoRoot();
        var referenceRoot = Path.GetFullPath(Path.Combine(repoRoot, ".."));
        var outDir = Path.Combine(repoRoot, "spike/SymbolCmap/out");
        Directory.CreateDirectory(outDir);

        Note("instrument: spike/SymbolCmap, in-proc on .NET " + Environment.Version);
        Note("why not PowerShell: 5.1 is .NET Framework, LoadFrom of a net10.0 assembly throws on GetTypes, " +
             "and the empty catch in the earlier script read that as an absent type");

        // --- [A] the instrument itself --------------------------------------------------------------
        Section("[A] assembly identity and per-file load report");
        var iconType = typeof(SymbolIcon);
        Note($"SymbolIcon runtime type: {iconType.FullName} | version {iconType.Assembly.GetName().Version}");
        Note($"SymbolIcon declaring assembly: {iconType.Assembly.GetName().Name}");
        Note("SymbolIcon assembly file version: " + (iconType.Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "<none>"));

        var assemblies = new List<Assembly>();
        foreach (var file in Directory.GetFiles(AppContext.BaseDirectory, "Jalium*.dll").OrderBy(f => f, StringComparer.Ordinal))
        {
            var name = Path.GetFileName(file);
            try
            {
                var assembly = Assembly.LoadFrom(file);
                Note($"load {name}: ok -> {assembly.GetName().Name} {assembly.GetName().Version}, " +
                     $"{Exported(assembly).Count} exported types");
                assemblies.Add(assembly);
            }
            catch (Exception e)
            {
                Note($"load {name}: THREW {e.GetType().Name}: {FirstLine(e.Message)}");
            }
        }

        // --- [B] the icon family the runtime exports -------------------------------------------------
        Section("[B] exported types whose name contains Symbol or Icon");
        var interesting = new List<Type>();
        foreach (var assembly in assemblies)
        {
            foreach (var type in Exported(assembly))
            {
                if (type.Name.Contains("Symbol", StringComparison.Ordinal) ||
                    type.Name.Contains("Icon", StringComparison.Ordinal))
                {
                    interesting.Add(type);
                }
            }
        }

        foreach (var type in interesting.Distinct().OrderBy(t => t.FullName, StringComparer.Ordinal))
        {
            var chain = new List<string>();
            for (var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
            {
                chain.Add(baseType.Name);
            }

            var kind = type.IsEnum ? "enum" : type.IsInterface ? "interface" : type.IsAbstract && type.IsSealed ? "static" : "class";
            Note($"{type.FullName} [{type.Assembly.GetName().Name}] {kind} <- {string.Join(" <- ", chain)}");
        }

        foreach (var name in new[] { "IconElement", "SymbolIcon", "FontIcon", "PathIcon", "BitmapIcon", "ImageIcon", "IconSource", "Symbol" })
        {
            var hit = interesting.FirstOrDefault(t => t.Name == name);
            Note($"probe {name}: " + (hit?.FullName ?? "not exported under this name by any loaded Jalium assembly"));
        }

        // --- [C] the enum SymbolIcon.Symbol declares, read off the property ---------------------------
        Section("[C] the icon family's own declared surface");
        foreach (var name in new[] { "IconElement", "SymbolIcon", "FontIcon", "PathIcon" })
        {
            var type = interesting.FirstOrDefault(t => t.Name == name);
            if (type is null)
            {
                Note($"{name}: absent");
                continue;
            }

            var declared = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .OrderBy(p => p.Name, StringComparer.Ordinal).ToList();
            Note($"{name} declares {declared.Count} public instance properties: " +
                 string.Join(", ", declared.Select(p => $"{p.Name}:{p.PropertyType.Name}")));
            foreach (var property in declared.Where(p => p.PropertyType.IsEnum || p.Name == "FontFamily"))
            {
                Note($"  {property.Name} : {property.PropertyType.FullName}");
            }

            var constructors = type.GetConstructors().Length;
            Note($"{name} public constructors: {constructors}");
        }

        var symbolProperty = iconType.GetProperty("Symbol");
        if (symbolProperty is null)
        {
            Note("SymbolIcon declares no Symbol property - the icon family has no enum-typed glyph selector here");
            return Finish(repoRoot, outDir, 1);
        }

        var symbolEnum = symbolProperty.PropertyType;
        Note($"property: {iconType.Name}.Symbol : {symbolEnum.FullName} " +
             $"(declared by {symbolEnum.Assembly.GetName().Name}) | IsEnum={symbolEnum.IsEnum} | " +
             $"underlying={Enum.GetUnderlyingType(symbolEnum).Name}");

        var shipped = ReadEnum(symbolEnum);
        Note("shipped members: " + shipped.Count);

        var byNumber = new Dictionary<int, List<string>>();
        foreach (var (name, value) in shipped)
        {
            if (!byNumber.TryGetValue(value, out var carriers))
            {
                byNumber[value] = carriers = [];
            }

            carriers.Add(name);
        }

        Note("shipped codepoints carried by two or more names: " + byNumber.Count(kvp => kvp.Value.Count > 1));

        // --- [D] upstream, read twice from two independent files ---------------------------------------
        Section("[D] upstream Symbol, read from the IDL and from the generated model");
        var idl = Path.Combine(referenceRoot, "microsoft-ui-xaml/dxaml/xcp/dxaml/idl/winrt/controls/microsoft.ui.xaml.controls.controls2.idl");
        var model = Path.Combine(referenceRoot, "microsoft-ui-xaml/dxaml/xcp/tools/XCPTypesAutoGen/XamlOM/Model/Microsoft.UI.Xaml.Controls.cs");
        var fromIdl = ReadIdl(idl);
        var fromModel = ReadModel(model);
        Note($"idl: {Count(fromIdl)} members, {CountDistinct(fromIdl)} distinct codepoints");
        Note($"model: {Count(fromModel)} members, {CountDistinct(fromModel)} distinct codepoints");

        // The IDL spells one explicit value per run and lets the rest increment; the model writes every value out.
        // Two parsers agreeing name-by-name and number-by-number is what makes the upstream side trustworthy;
        // the first version of this instrument required "name = value" on every IDL line and read one member.
        var disagreement = fromIdl.Keys.Union(fromModel.Keys, StringComparer.Ordinal)
            .Where(k => !fromIdl.TryGetValue(k, out var a) || !fromModel.TryGetValue(k, out var b) || a != b)
            .ToList();
        Note("idl vs model disagreement: " + disagreement.Count +
             (disagreement.Count == 0 ? " (the two upstream readings agree)" : " -> " + string.Join(", ", disagreement.Take(20))));
        if (disagreement.Count > 0)
        {
            Note("ABORT: the upstream side is not settled, so no diff is claimed from it");
            return Finish(repoRoot, outDir, 2);
        }

        var upstream = fromIdl;

        // --- [E] the three lists ----------------------------------------------------------------------
        Section("[E] upstream vs shipped");
        var onlyUpstream = upstream.Keys.Where(k => !shipped.ContainsKey(k)).OrderBy(k => k, StringComparer.Ordinal).ToList();
        var onlyShipped = shipped.Keys.Where(k => !upstream.ContainsKey(k)).OrderBy(k => k, StringComparer.Ordinal).ToList();
        var mismatch = upstream.Keys.Where(k => shipped.ContainsKey(k) && shipped[k] != upstream[k])
            .OrderBy(k => k, StringComparer.Ordinal).ToList();
        var agree = upstream.Keys.Count(k => shipped.ContainsKey(k) && shipped[k] == upstream[k]);

        Note($"names on both sides with the same codepoint: {agree}");
        Note($"upstream-only: {onlyUpstream.Count}");
        var orphan = 0;
        foreach (var name in onlyUpstream)
        {
            var codepoint = upstream[name];
            string note;
            if (byNumber.TryGetValue(codepoint, out var carriers))
            {
                note = "codepoint carried by " + string.Join("/", carriers);
            }
            else
            {
                note = "codepoint not in the shipped enum at all";
                orphan++;
            }

            Note($"  {name} = 0x{codepoint:X4}  ({note})");
        }

        Note("upstream codepoints with no shipped carrier at all: " + orphan);
        Note($"value mismatch under a shared name: {mismatch.Count}");
        var mapped = 0;
        foreach (var name in mismatch)
        {
            var upstreamCodepoint = upstream[name];
            var carrier = byNumber.TryGetValue(upstreamCodepoint, out var carriers)
                ? string.Join("/", carriers)
                : "absent";
            if (byNumber.ContainsKey(upstreamCodepoint))
            {
                mapped++;
            }

            Note($"  {name}: upstream 0x{upstreamCodepoint:X4} vs shipped 0x{shipped[name]:X4} " +
                 $"(upstream's number is carried here by {carrier})");
        }

        Note("shared names whose upstream number exists in the shipped enum under some other name: " +
             mapped + " of " + mismatch.Count);

        Note($"shipped-only: {onlyShipped.Count}");
        Note("  first 25: " + string.Join(", ", onlyShipped.Take(25)));

        // --- [F] the glyph that is actually painted -------------------------------------------------------
        // The enum's number is not what reaches the font: upstream converts it before writing the TextBlock's Text
        // (icon.cpp:461 ConvertSymbolValueToGlyph, a switch with one case per enum member, blob 7ae82256). So the
        // appearance question is upstream's *converted* number against ours, and the IDL number answers neither.
        // Two upstream files carry the same mapping - the compiled switch and the design note's table - and they
        // are parsed separately here so a slip in one parser cannot become a finding.
        Section("[F] the converted codepoint upstream paints, against the number our enum holds");
        var iconCpp = Path.Combine(referenceRoot, "microsoft-ui-xaml/dxaml/xcp/core/core/elements/icon.cpp");
        var noteMd = Path.Combine(referenceRoot, "microsoft-ui-xaml/docs/design-notes/symbol-enum-spec.md");
        var converted = ReadIconSwitch(iconCpp);
        var noted = ReadMarkdownTable(noteMd);
        Note($"icon.cpp: {converted.Count} cases | design note: {noted.Count} table rows");

        var noteSlip = converted.Keys
            .Where(k => !noted.TryGetValue(k, out var n) || n != converted[k].Glyph)
            .Concat(noted.Keys.Where(k => !converted.ContainsKey(k)))
            .OrderBy(k => k, StringComparer.Ordinal).ToList();
        Note("icon.cpp vs design-note disagreement: " + noteSlip.Count +
             (noteSlip.Count == 0 ? " (the two upstream renderings of the map agree)" : " -> " + string.Join(", ", noteSlip.Take(20))));

        var idlSlip = upstream.Keys
            .Where(k => !converted.ContainsKey(k) || converted[k].Legacy != upstream[k])
            .OrderBy(k => k, StringComparer.Ordinal).ToList();
        Note("enum members with no case, or a case whose source number differs: " + idlSlip.Count +
             (idlSlip.Count == 0 ? " (every enum member is converted exactly once)" : " -> " + string.Join(", ", idlSlip.Take(20))));

        var painted = converted
            .Where(kvp => shipped.TryGetValue(kvp.Key, out var ours) && ours != kvp.Value.Glyph)
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal).ToList();
        var paintedSame = converted.Count - painted.Count - converted.Keys.Count(k => !shipped.ContainsKey(k));
        Note($"names where our number paints the same glyph upstream paints: {paintedSame}");
        Note($"names where our number paints a different glyph: {painted.Count}");
        foreach (var (name, map) in painted)
        {
            Note($"  {name}: upstream paints 0x{map.Glyph:X4} (enum 0x{map.Legacy:X4}), ours holds 0x{shipped[name]:X4}");
        }

        var unnameable = converted.Keys.Where(k => !shipped.ContainsKey(k)).OrderBy(k => k, StringComparer.Ordinal).ToList();
        Note("upstream names with no member of the same name here: " + unnameable.Count);
        foreach (var name in unnameable)
        {
            var map = converted[name];
            var carrier = byNumber.TryGetValue(map.Glyph, out var carriers) ? string.Join("/", carriers) : "absent";
            Note($"  {name}: upstream paints 0x{map.Glyph:X4}, carried here by {carrier}");
        }

        // --- [G] machine-readable sidecars --------------------------------------------------------------
        Section("[G] sidecars");
        WriteTsv(Path.Combine(outDir, "shipped-symbol.tsv"), shipped);
        WriteTsv(Path.Combine(outDir, "upstream-symbol.tsv"), upstream);
        WriteTsv(Path.Combine(outDir, "upstream-painted.tsv"), converted.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Glyph));
        Note("shipped-symbol.tsv + upstream-symbol.tsv written to spike/SymbolCmap/out");

        return Finish(repoRoot, outDir, 0);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory)!;
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "FluentJalium.slnx")))
        {
            dir = dir.Parent;
        }

        return dir!.FullName;
    }

    private static int Finish(string repoRoot, string outDir, int code)
    {
        var target = Path.Combine(repoRoot, "docs/astra/adaptation/s2-symbol-surface-raw.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.WriteAllLines(target, Lines, new UTF8Encoding(false));
        foreach (var line in Lines)
        {
            Console.Out.WriteLine(line);
        }

        Console.Out.WriteLine("wrote " + target + " (" + Lines.Count + " lines)");
        return code;
    }

    private static void WriteTsv(string path, Dictionary<string, int> map) =>
        File.WriteAllLines(path, map.OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select(kvp => $"{kvp.Key}\t0x{kvp.Value:X4}\t{kvp.Value}"), new UTF8Encoding(false));

    private static Dictionary<string, int> ReadEnum(Type enumType)
    {
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        var names = Enum.GetNames(enumType);
        var values = Enum.GetValuesAsUnderlyingType(enumType);
        for (var i = 0; i < names.Length; i++)
        {
            map[names[i]] = Convert.ToInt32(values.GetValue(i));
        }

        return map;
    }

    /// <summary>
    /// The IDL form: an explicit value at the head of each run, every later member implicit and incrementing.
    /// </summary>
    private static Dictionary<string, int> ReadIdl(string path)
    {
        var text = File.ReadAllText(path);
        var block = System.Text.RegularExpressions.Regex.Match(text, @"(?s)enum Symbol\s*\{(.*?)\};");
        if (!block.Success)
        {
            throw new InvalidOperationException("no enum Symbol block in " + path);
        }

        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        var last = -1;
        foreach (var rawLine in block.Groups[1].Value.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal) || line.StartsWith("[", StringComparison.Ordinal))
            {
                continue;
            }

            var match = System.Text.RegularExpressions.Regex.Match(line, @"^([A-Za-z][A-Za-z0-9_]*)\s*(?:=\s*(0x[0-9a-fA-F]+|\d+))?\s*,?\s*$");
            if (!match.Success)
            {
                continue;
            }

            var value = match.Groups[2].Success ? ParseNumber(match.Groups[2].Value) : last + 1;
            map[match.Groups[1].Value] = value;
            last = value;
        }

        return map;
    }

    /// <summary>
    /// Upstream's compiled conversion, one case per enum member: the number the enum holds, the number the font
    /// is asked for, and the member's name as the comment.
    /// </summary>
    private sealed record GlyphMap(int Legacy, int Glyph);

    private static Dictionary<string, GlyphMap> ReadIconSwitch(string path)
    {
        var text = File.ReadAllText(path);
        var map = new Dictionary<string, GlyphMap>(StringComparer.Ordinal);
        foreach (var match in Regex.Matches(text,
                     @"case 0x([0-9A-Fa-f]{4}): return static_cast<WCHAR>\(0x([0-9A-Fa-f]{4})\);\s*//\s*([A-Za-z][A-Za-z0-9_]*)")
                 .Cast<Match>())
        {
            map[match.Groups[3].Value] = new GlyphMap(
                Convert.ToInt32(match.Groups[1].Value, 16),
                Convert.ToInt32(match.Groups[2].Value, 16));
        }

        if (map.Count == 0)
        {
            throw new InvalidOperationException("no conversion cases in " + path);
        }

        return map;
    }

    /// <summary>
    /// The same mapping as upstream's design note: a markdown table of name, legacy number, new number. Parsed on
    /// its own so the note can contradict the .cpp - the one cross-check that catches a slip in either parser.
    /// </summary>
    private static Dictionary<string, int> ReadMarkdownTable(string path)
    {
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var line in File.ReadLines(path))
        {
            var match = Regex.Match(line.Trim(), @"^\|\s*([A-Za-z][A-Za-z0-9_]*)\s*\|\s*([0-9A-Fa-f]{4})\s*\|\s*([0-9A-Fa-f]{4})\s*\|");
            if (match.Success)
            {
                map[match.Groups[1].Value] = Convert.ToInt32(match.Groups[3].Value, 16);
            }
        }

        if (map.Count == 0)
        {
            throw new InvalidOperationException("no mapping rows in " + path);
        }

        return map;
    }

    private static Dictionary<string, int> ReadModel(string path)
    {
        var text = File.ReadAllText(path);
        var start = text.IndexOf("public enum Symbol", StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException("no public enum Symbol in " + path);
        }

        var end = text.IndexOf("\n    }", start, StringComparison.Ordinal);
        var block = text.Substring(start, end - start);
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var match in System.Text.RegularExpressions.Regex.Matches(block, @"([A-Za-z][A-Za-z0-9_]*)\s*=\s*(0x[0-9a-fA-F]+|\d+)").Cast<System.Text.RegularExpressions.Match>())
        {
            map[match.Groups[1].Value] = ParseNumber(match.Groups[2].Value);
        }

        return map;
    }

    private static int ParseNumber(string raw) =>
        raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt32(raw[2..], 16) : int.Parse(raw);

    private static readonly Dictionary<Assembly, IReadOnlyList<Type>> ExportedCache = new();

    private static IReadOnlyList<Type> Exported(Assembly assembly)
    {
        if (ExportedCache.TryGetValue(assembly, out var cached))
        {
            return cached;
        }

        var list = new List<Type>();
        try
        {
            list.AddRange(assembly.GetExportedTypes());
        }
        catch (ReflectionTypeLoadException e)
        {
            list.AddRange(e.Types.Where(t => t is not null)!);
            Note($"  GetExportedTypes threw {e.GetType().Name} for {assembly.GetName().Name}; " +
                 $"{list.Count} types were still reachable");
        }
        catch (Exception e)
        {
            Note($"  GetExportedTypes threw {e.GetType().Name} for {assembly.GetName().Name}: {FirstLine(e.Message)}");
        }

        var result = list.AsReadOnly();
        ExportedCache[assembly] = result;
        return result;
    }

    private static string FirstLine(string text) => text.Split('\n')[0].Trim();

    private static int Count(IReadOnlyDictionary<string, int> map) => map.Count;

    private static int CountDistinct(IReadOnlyDictionary<string, int> map) => map.Values.Distinct().Count();

    private static void Section(string title)
    {
        Note(string.Empty);
        Note("=== " + title + " ===");
    }

    private static void Note(string line) => Lines.Add(line);
}
