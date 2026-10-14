using System.Reflection;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;

namespace RenderCeiling;

/// <summary>
/// Decides the open question from the census: of the controls that draw themselves, which ones
/// are actually beyond a style's reach. Answers it three ways that cross-check each other - IL
/// decoding of every method (calibrated against a known-dirty and a known-clean method),
/// dependency-property default metadata, and measured pixels from an offscreen render target.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];

    [STAThread]
    private static int Main(string[] arguments)
    {
        var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
        renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
        ThemeLoader.Initialize();
        new Application();

        if (arguments.Contains("material"))
        {
            ProbeMaterial();
            foreach (var line in Lines) Console.WriteLine(line);
            return 0;
        }

        ProbeRenderTarget();
        ProbeDependencyPropertyDefaults();
        Scan();
        ProbePixels();

        foreach (var line in Lines) Console.WriteLine(line);
        return 0;
    }

    private static void Scan()
    {
        var controlsAssembly = typeof(Button).Assembly;
        var module = controlsAssembly.GetModules()[0];

        var dirty = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);
        var renderOverrides = new SortedSet<string>(StringComparer.Ordinal);
        int scannedMethods = 0, scannedTypes = 0;

        foreach (var type in controlsAssembly.GetTypes())
        {
            if (!type.IsPublic || type.IsAbstract) continue;
            if (!typeof(Control).IsAssignableFrom(type) && !typeof(Window).IsAssignableFrom(type)) continue;
            scannedTypes++;

            if (type.GetMethod("OnRender", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) is not null
                || type.GetMethod("OnPaint", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) is not null)
            {
                renderOverrides.Add(type.Name);
            }

            foreach (var method in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).OfType<MethodBase>())
            {
                scannedMethods++;
                foreach (var hit in ThemeColorCalls(method, module))
                {
                    if (!dirty.TryGetValue(type.Name, out var set))
                    {
                        set = [];
                        dirty[type.Name] = set;
                    }

                    set.Add(hit);
                }
            }
        }

        foreach (var probe in new[] { nameof(KnownDirty), nameof(KnownClean) })
        {
            var method = typeof(Program).GetMethod(probe, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)!;
            var hits = ThemeColorCalls(method, method.Module).ToList();
            Note("CALIBRATION", $"{probe}: decoder reports {hits.Count} -> {string.Join("; ", hits)}");
        }

        Note("SCAN", $"types={scannedTypes} methods={scannedMethods} renderOverriding={renderOverrides.Count} themeColorsReferencing={dirty.Count}");
        foreach (var (type, hits) in dirty.OrderBy(static entry => entry.Key, StringComparer.Ordinal))
            Note("DIRTY", $"{type,-32} {string.Join("; ", hits.Order(StringComparer.Ordinal))}");

        var liveTypes = dirty
            .Select(entry => (Name: entry.Key, Hits: entry.Value.Where(static hit => !hit.Contains(".cctor ->", StringComparison.Ordinal)).ToList()))
            .Where(static entry => entry.Hits.Count > 0)
            .ToList();
        Note("SPLIT", $".cctor-only = {dirty.Count - liveTypes.Count} types; live reads = {liveTypes.Count} types");
        foreach (var (name, hits) in liveTypes)
            Note("CEILING", $"{name,-16} {string.Join("; ", hits.Order(StringComparer.Ordinal))}");

        var cleanRenderers = renderOverrides.Where(name => !dirty.ContainsKey(name)).Order(StringComparer.Ordinal).ToList();
        Note("CLEAN-BUT-DRAWN", $"{cleanRenderers.Count}: {string.Join(" ", cleanRenderers)}");
    }

    private static void ProbeDependencyPropertyDefaults()
    {
        var table = new Dictionary<Color, string>();
        foreach (var property in typeof(ThemeColors).GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            if (property.GetValue(null) is Color color) table[color] = property.Name;
        }

        var confirmed = 0;
        foreach (var type in typeof(Button).Assembly.GetTypes().Where(static candidate => candidate.IsPublic && !candidate.IsAbstract && (typeof(Control).IsAssignableFrom(candidate) || typeof(Window).IsAssignableFrom(candidate))))
        {
            var hits = new List<string>();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(static candidate => candidate.FieldType == typeof(DependencyProperty)))
            {
                if (field.GetValue(null) is not DependencyProperty property) continue;
                var value = property.DefaultMetadata.DefaultValue;
                if (value is not SolidColorBrush and not Color) continue;
                var color = value is SolidColorBrush brush ? brush.Color : (Color)value;
                if (table.TryGetValue(color, out var source)) hits.Add($"{field.Name} default {color} == ThemeColors.{source}");
            }

            if (hits.Count == 0) continue;
            confirmed++;
            Note("DP-DEFAULT", $"{type.Name,-20} {string.Join("; ", hits)}");
        }

        Note("DP-DEFAULT", $"types whose DP defaults literally equal a ThemeColors value: {confirmed}");
    }

    private static void ProbeRenderTarget()
    {
        var target = Type.GetType("Jalium.UI.Media.Imaging.RenderTargetBitmap, Jalium.UI.Managed");
        if (target is null) { Note("RTB", "RenderTargetBitmap absent"); return; }
        foreach (var method in target.GetConstructors())
            Note("RTB", $"ctor({string.Join(", ", method.GetParameters().Select(static parameter => parameter.ParameterType.Name + " " + parameter.Name))})");
        foreach (var method in target.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(static candidate => candidate.Name is "Render" or "CopyPixels"))
            Note("RTB", $"{method.Name}({string.Join(", ", method.GetParameters().Select(static parameter => parameter.ParameterType.Name + " " + parameter.Name))}) -> {method.ReturnType.Name}");
        Note("RTB", $"PixelFormat isEnum={typeof(PixelFormat).IsEnum}; statics=[{string.Join(",", typeof(PixelFormat).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(static property => property.Name).Take(14))}]");
    }

    private const uint MagentaRgb = 0xFF_00_FF;
    private const uint EmeraldRgb = 0x20_72_45;

    /// <summary>
    /// Three passes over the same subjects: untouched, after ThemeManager.ApplyAccent, and after
    /// forcing Background/Foreground. A control whose pixels move with a DP is styleable; one that
    /// keeps painting the same colour through all three is the real ceiling.
    /// </summary>
    private static void ProbePixels()
    {
        var sentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
        var subjects = new List<Control>
        {
            new Slider { Width = 220, Value = 50 },
            new ProgressBar { Width = 220, Value = 50 },
            new CheckBox { Content = "chk", IsChecked = true },
            new RadioButton { Content = "rad", IsChecked = true },
        };
        if (Type.GetType("Jalium.UI.Controls.Primitives.ScrollBar, Jalium.UI.Managed") is { } scrollBarType && Activator.CreateInstance(scrollBarType) is Control scrollBar)
        {
            scrollBar.Width = 220;
            subjects.Add(scrollBar);
        }

        var host = new StackPanel();
        foreach (var subject in subjects) host.Children.Add(subject);
        var window = new Window { Content = host, Width = 480, Height = 520, Title = "pixel probe" };
        var format = ResolvePixelFormat();

        window.Loaded += (_, _) =>
        {
            foreach (var pass in new[] { "1-default", "2-ApplyAccent(magenta)", "3-Background+Foreground=magenta" })
            {
                if (pass.StartsWith("2", StringComparison.Ordinal)) ThemeManager.ApplyAccent(sentinel);
                foreach (var subject in subjects)
                {
                    try
                    {
                        if (pass.StartsWith("3", StringComparison.Ordinal))
                        {
                            subject.SetValue(Control.BackgroundProperty, new SolidColorBrush(sentinel));
                            subject.SetValue(Control.ForegroundProperty, new SolidColorBrush(sentinel));
                        }

                        subject.Measure(new Size(220, 60));
                        subject.Arrange(new Rect(0, 0, 220, 60));
                        subject.UpdateLayout();

                        var histogram = Sample(subject, format);
                        var top = histogram.OrderByDescending(static entry => entry.Value)
                            .Where(static entry => entry.Key != 0)
                            .Take(4)
                            .Select(static entry => $"#{entry.Key:X6}x{entry.Value}");
                        Note("PIXEL", $"[{pass,-31}] {subject.GetType().Name,-12} magenta={histogram.ContainsKey(MagentaRgb)} emerald={histogram.ContainsKey(EmeraldRgb)} top=[{string.Join(" ", top)}]");
                    }
                    catch (Exception exception)
                    {
                        Note("PIXEL", $"[{pass}] {subject.GetType().Name}: {exception.GetType().Name}: {Trim(exception.Message)}");
                    }
                }
            }

            window.Close();
        };
        (Application.Current ?? new Application()).Run(window);
    }

    private static Dictionary<uint, int> Sample(Control subject, PixelFormat format)
    {
        var bitmap = new Jalium.UI.Media.Imaging.RenderTargetBitmap(220, 60, 96, 96, format);
        bitmap.Render(subject);
        var pixels = new byte[220 * 60 * 4];
        bitmap.CopyPixels(new Int32Rect(0, 0, 220, 60), pixels, 220 * 4, 0);

        var histogram = new Dictionary<uint, int>();
        for (var index = 0; index + 3 < pixels.Length; index += 4)
        {
            var key = (uint)(pixels[index + 2] << 16 | pixels[index + 1] << 8 | pixels[index]);
            histogram[key] = histogram.TryGetValue(key, out var count) ? count + 1 : 1;
        }

        return histogram;
    }

    private static PixelFormat ResolvePixelFormat()
    {
        foreach (var candidate in typeof(PixelFormat).Assembly.GetTypes().Where(static type => type.Name.StartsWith("PixelFormat", StringComparison.Ordinal)))
        {
            foreach (var property in candidate.GetProperties(BindingFlags.Public | BindingFlags.Static).Where(static property => property.PropertyType == typeof(PixelFormat)))
            {
                if (property.Name.Contains("32", StringComparison.Ordinal) && property.GetValue(null) is PixelFormat format) return format;
            }
        }

        foreach (var field in typeof(PixelFormat).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType == typeof(PixelFormat) && field.GetValue(null) is PixelFormat format) return format;
        }

        return default;
    }

    private static string Trim(string message) => message.ReplaceLineEndings(" ");

    /// <summary>
    /// Scans a method body for call instructions resolving to ThemeColors members. Raw bytes can
    /// coincidentally look like an opcode, so every candidate token must resolve before it counts;
    /// the calibration probes measure whether that guard is sufficient.
    /// </summary>
    private static IEnumerable<string> ThemeColorCalls(MethodBase method, Module module)
    {
        byte[] il;
        try
        {
            il = method.GetMethodBody()?.GetILAsByteArray() ?? [];
        }
        catch
        {
            yield break;
        }

        for (var offset = 0; offset + 4 < il.Length; offset++)
        {
            if (il[offset] != 0x28) continue;
            var token = BitConverter.ToInt32(il, offset + 1);
            if ((token >> 24) is not (0x06 or 0x0A)) continue;

            MethodBase? resolved;
            try
            {
                resolved = module.ResolveMethod(token);
            }
            catch
            {
                continue;
            }

            if (resolved?.DeclaringType == typeof(ThemeColors)) yield return $"{method.Name} -> {resolved.Name}";
        }
    }

    private static void KnownDirty() => Note("CALIBRATION", $"sentinels {ThemeColors.Accent} {ThemeColors.SliderThumb}");

    private static void KnownClean() => Note("CALIBRATION", "this method reads no theme colors");

    private static readonly string[] MaterialKeywords =
    [
        "Backdrop", "Acrylic", "Mica", "Blur", "Noise", "Frosted", "Refraction", "Chromatic",
        "Liquid", "Glass", "Ripple", "Reveal", "Shadow", "Effect", "Composition",
    ];

    private static void ProbeMaterial()
    {
        var types = typeof(Button).Assembly.GetTypes().Where(static type => type.IsPublic && !type.IsAbstract)
            .Where(type => MaterialKeywords.Any(keyword => type.Name.Contains(keyword, StringComparison.Ordinal)))
            .OrderBy(static type => type.FullName, StringComparer.Ordinal);
        foreach (var type in types)
        {
            var surface = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(static property => property.Name).Order(StringComparer.Ordinal);
            Note("MATERIAL", $"{type.FullName} : {(type.BaseType?.Name)} props=[{string.Join(",", surface.Take(10))}]");
        }

        Note("WINDOW", $"public Window members: {string.Join(", ", typeof(Window).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(static member => member.Name).Distinct().Order(StringComparer.Ordinal))}");

        Note("MOTION", $"animation types: {string.Join(", ", typeof(Button).Assembly.GetTypes().Where(static type => type.IsPublic && (type.Name.Contains("Animation", StringComparison.Ordinal) || type.Name.Contains("Easing", StringComparison.Ordinal) || type.Name.Contains("Storyboard", StringComparison.Ordinal) || type.Name.Contains("KeyFrame", StringComparison.Ordinal) || type.Name.Contains("Transition", StringComparison.Ordinal))).Select(static type => type.Name).Order(StringComparer.Ordinal))}");
        Note("MOTION", $"UIElement transition surface: {string.Join(", ", typeof(UIElement).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(static property => property.Name.Contains("Transition", StringComparison.Ordinal)).Select(static property => property.Name).Order(StringComparer.Ordinal))}");
    }

    private static void Note(string category, string message) => Lines.Add($"[{category}] {message}");
}
