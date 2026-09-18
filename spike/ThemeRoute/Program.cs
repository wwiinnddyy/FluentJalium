using System.Reflection;
using Jalium.UI;
using Jalium.UI.Controls;

namespace ThemeRoute;

/// <summary>
/// Answers one question for the Astra theme kernel: does Jalium.UI 26.10.9 expose a non-experimental
/// route that flips native control defaults, or is Application.ThemeMode the only driver?
/// Reads attributes and IL only; nothing here renders or opens a window.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];

    private static int Main()
    {
        DumpExperimentalSurface();
        DumpCallGraphs();
        DumpThemeModeMembers();
        foreach (var line in Lines) Console.WriteLine(line);
        return 0;
    }

    private static void DumpExperimentalSurface()
    {
        var asm = typeof(Application).Assembly;
        var controlsAssembly = typeof(Button).Assembly;
        var experimental = new List<string>();

        foreach (var assembly in new[] { asm, controlsAssembly })
        foreach (var type in assembly.GetExportedTypes())
        {
            Record(type, type.GetCustomAttributesData(), experimental);
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(static m => m is PropertyInfo or MethodInfo or ConstructorInfo))
                Record(member, member.GetCustomAttributesData(), experimental);
        }

        Note("EXPERIMENTAL", $"{experimental.Count} public members carry [Experimental]");
        foreach (var line in experimental.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)) Note("EXPERIMENTAL", line);
    }

    private static void Record(MemberInfo member, IList<CustomAttributeData> attrs, List<string> experimental)
    {
        var hit = attrs.FirstOrDefault(static a => a.AttributeType.Name == "ExperimentalAttribute");
        if (hit is null) return;
        var args = string.Join(", ", hit.ConstructorArguments.Select(static a => a.Value));
        var named = string.Join(", ", hit.NamedArguments.Select(static a => $"{a.MemberName}={a.TypedValue.Value}"));
        experimental.Add($"{member.DeclaringType?.FullName}.{member.Name} [{member.MemberType}] -> Experimental({args}{(named.Length == 0 ? "" : "; " + named)})");
    }

    private static void DumpResourceDictionaryLevers()
    {
        var type = typeof(Application).Assembly.GetType("Jalium.UI.ResourceDictionary")!;
        Note("RD", $"public instance/static surface: {string.Join(", ", type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Select(static m => m.Name).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}");
        foreach (var name in new[] { "InvalidateMergedLookupCaches", "NotifyKeysChanged", "NotifyThemeResourcesChanged", "OnGettingValue", "CurrentThemeKey" })
        {
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(static m => m.Name.Contains("Invalidate", StringComparison.Ordinal) || m.Name.Contains("Notify", StringComparison.Ordinal) || m.Name == "CurrentThemeKey" || m.Name.Contains("Theme", StringComparison.Ordinal)))
            {
                if (member.Name != name) continue;
                var method = member as MethodInfo ?? (member as PropertyInfo)?.GetGetMethod(true) ?? (member as PropertyInfo)?.SetMethod;
                Note("RD", $"{member.MemberType} {member.Name} isPublic={IsPublic(member)} calls=[{string.Join(", ", method is null ? [] : Calls(method).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}]");
            }
        }
    }

    private static bool IsPublic(MemberInfo member) => member switch
    {
        MethodInfo method => method.IsPublic,
        PropertyInfo property => property.GetMethod?.IsPublic == true || property.SetMethod?.IsPublic == true,
        _ => false,
    };

    private static void DumpCallGraphs()
    {
        DumpResourceDictionaryLevers();
        var managed = typeof(Application).Assembly;
        var controls = typeof(Button).Assembly;

        var targets = new List<MethodBase>
        {
            Method(typeof(Application), "set_ThemeMode", managed),
            Method(typeof(Application), "NotifyThemeResourcesChanged", managed),
            Method(typeof(Application), "OnThemeModeChanged", managed),
            Method(controls.GetType("Jalium.UI.Controls.Themes.ThemeManager"), "ApplyTheme", controls),
            Method(controls.GetType("Jalium.UI.Controls.Themes.ThemeManager"), "ApplyAccent", controls),
            Method(controls.GetType("Jalium.UI.Controls.Themes.ThemeManager"), "ApplyBrandTheme", controls),
            Method(controls.GetType("Jalium.UI.Controls.Themes.ThemeManager"), "Initialize", controls),
            Method(controls.GetType("Jalium.UI.Controls.Themes.ThemeManager"), "InvalidateThemeStyleCache", controls),
            Method(controls.GetType("Jalium.UI.Controls.Themes.ThemeManager"), "LoadGenericTheme", controls),
            Method(controls.GetType("Jalium.UI.Controls.Themes.ThemeManager"), "Reset", controls),
        }.Where(static m => m is not null).Cast<MethodBase>().ToList();

        foreach (var method in targets)
        {
            var calls = Calls(method).ToList();
            Note("CALLS", $"{method.DeclaringType?.Name}.{method.Name} [{Access(method)}] -> {calls.Count} distinct");
            foreach (var call in calls.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)) Note("  ", call);
        }
    }

    private static void DumpThemeModeMembers()
    {
        var mode = typeof(Application).Assembly.GetType("Jalium.UI.ThemeMode");
        if (mode is null) { Note("THEMEMODE", "type absent"); return; }
        Note("THEMEMODE", $"props: {string.Join(", ", mode.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).Select(static p => p.Name).Order(StringComparer.Ordinal))}");
        Note("THEMEMODE", $"attrs: {string.Join(", ", mode.GetCustomAttributesData().Select(static a => a.AttributeType.Name))}");
        var dp = typeof(Application).GetProperty("ThemeMode")?.DeclaringType?.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(static f => f.Name.Contains("ThemeMode", StringComparison.Ordinal)).ToList() ?? [];
        foreach (var field in dp) Note("THEMEMODE", $"field {field.Name} ({field.Attributes})");
        foreach (var prop in typeof(Application).Assembly.GetType("Jalium.UI.FrameworkElement")?
                     .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                     .Where(static p => p.Name.Contains("Theme", StringComparison.Ordinal)) ?? [])
            Note("THEMEMODE", $"FrameworkElement.{prop.Name} get={Access(prop.GetGetMethod(true))} set={Access(prop.SetMethod)}");
        foreach (var prop in typeof(Application).Assembly.GetType("Jalium.UI.Window")?
                     .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                     .Where(static p => p.Name.Contains("Theme", StringComparison.Ordinal) || p.Name.Contains("Backdrop", StringComparison.Ordinal)) ?? [])
            Note("THEMEMODE", $"Window.{prop.Name} get={Access(prop.GetGetMethod(true))} set={Access(prop.SetMethod)}");
    }

    private static IEnumerable<string> Calls(MethodBase method)
    {
        byte[] il;
        try
        {
            il = method.GetMethodBody()?.GetILAsByteArray() ?? [];
        }
        catch
        {
            return [];
        }

        var module = method.Module;
        var result = new List<string>();
        for (var offset = 0; offset + 4 < il.Length; offset++)
        {
            if (il[offset] is not (0x28 or 0x6F)) continue; // call / callvirt
            var token = BitConverter.ToInt32(il, offset + 1);
            if ((token >> 24) is not (0x06 or 0x0A or 0x2B or 0x30)) continue;

            MethodBase? resolved;
            try
            {
                resolved = module.ResolveMethod(token);
            }
            catch
            {
                continue;
            }

            if (resolved is null) continue;
            result.Add($"{(il[offset] == 0x6F ? "virt " : "")}{resolved.DeclaringType?.Name}.{resolved.Name}"
                       + (resolved.DeclaringType?.Assembly == module.Assembly ? "" : $" [{Short(resolved.DeclaringType?.Assembly)}]")
                       + (Access(resolved) == "public" ? "" : $" <{Access(resolved)}>"));
        }

        return result;
    }

    private static string Short(Assembly? assembly) => assembly?.GetName().Name ?? "?";

    private static string Access(MethodBase? method) =>
        method is null ? "absent"
        : method.IsPublic ? "public"
        : method.IsFamily ? "protected"
        : method.IsAssembly ? "internal"
        : method.IsFamilyOrAssembly ? "protected internal"
        : method.IsPrivate ? "private"
        : "other";

    private static string Access(MethodInfo? method) => method is null ? "-" : Access((MethodBase)method);

    private static MethodBase? Method(Type? type, string name, Assembly owner)
    {
        if (type is null) return null;
        var candidate = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        if (candidate is null) Note("MISS", $"{owner.GetName().Name}:{type.Name}.{name} not found");
        return candidate;
    }

    private static void Note(string category, string message) => Lines.Add($"[{category}] {message}");
}
