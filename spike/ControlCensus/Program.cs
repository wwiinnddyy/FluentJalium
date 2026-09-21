using System.Reflection;
using System.Xml.Linq;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;

namespace ControlCensus;

/// <summary>
/// Census of what a Fluent control library can actually stand on: Jalium's own shipped theme,
/// which controls are template-driven, which are self-drawn, and which Fluent surfaces exist.
/// Prints one line per fact; nothing here is inferred from documentation.
/// </summary>
internal static class Program
{
    internal static readonly List<string> Lines = [];

    [STAThread]
    private static int Main()
    {
        var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
        renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
        ThemeLoader.Initialize();
        var application = new Application();
        ThemeManager.Initialize(application);

        var genericPath = DumpGenericTheme();
        var census = new Census(genericPath);
        CensusControls(census);
        ProbeThemeStyleResolution();
        ProbeImplicitStyle(application);
        FluentSurfaces();
        OutstandingNames();

        foreach (var line in Lines) Console.WriteLine(line);
        return 0;
    }

    private static string DumpGenericTheme()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "out", "generic.jalxaml");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        path = Path.GetFullPath(path);
        try
        {
            using var stream = ThemeManager.GetGenericThemeStream();
            if (stream is null)
            {
                Note("GENERIC", "GetGenericThemeStream() returned null");
                return string.Empty;
            }
            using var file = File.Create(path);
            stream.CopyTo(file);
            Note("GENERIC", $"dumped to {path} bytes={file.Length}");
            return File.Exists(path) ? path : string.Empty;
        }
        catch (Exception exception)
        {
            Note("GENERIC", $"{exception.GetType().Name}: {Trim(exception.Message)}");
            return string.Empty;
        }
    }

    private static void CensusControls(Census census)
    {
        var assembly = typeof(Button).Assembly;
        Note("CENSUS", $"controls assembly: {assembly.GetName().Name}");
        var types = assembly.GetTypes().Where(static type => type.IsPublic && typeof(Control).IsAssignableFrom(type) && !type.IsAbstract).ToList();
        Note("CENSUS", $"public concrete Control-derived types: {types.Count}");
        Note("CENSUS", $"generic theme defines styles for {census.StyleTargets.Count} target types, {census.KeyedStyles.Count} keyed styles");

        var templateDriven = 0;
        var selfDrawn = 0;
        var noStyle = 0;
        foreach (var type in types.OrderBy(static type => type.Name, StringComparer.Ordinal))
        {
            var hasStyle = census.StyleTargets.Contains(type.Name);
            var overridesRender = type.GetMethod("OnRender", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) is not null;
            var paints = type.GetMethod("OnPaint", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) is not null;
            var brushProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(static property => property.PropertyType.Name.Contains("Brush", StringComparison.Ordinal)
                    || property.Name.Contains("Brush", StringComparison.Ordinal)
                    || property.Name.Contains("Background", StringComparison.Ordinal)
                    || property.Name.Contains("Foreground", StringComparison.Ordinal)
                    || property.Name.Contains("Border", StringComparison.Ordinal))
                .Select(static property => property.Name)
                .ToList();
            var parts = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(static field => field.Name.StartsWith("PART_", StringComparison.Ordinal))
                .Select(static field => field.Name)
                .ToList();
            var template = type.GetProperty("Template", BindingFlags.Public | BindingFlags.Instance) is not null;

            var verdict = !hasStyle ? "no-default-style" : overridesRender || paints ? "self-drawn" : "template-driven";
            switch (verdict)
            {
                case "template-driven": templateDriven++; break;
                case "self-drawn": selfDrawn++; break;
                default: noStyle++; break;
            }

            if (verdict != "template-driven" || brushProperties.Count == 0)
            {
                Note("TYPE", $"{type.Name,-32} style={hasStyle,-5} render={overridesRender} paint={paints} templateProp={template,-5} parts={parts.Count} brushDps={brushProperties.Count} => {verdict}");
            }
        }
        Note("SUMMARY", $"template-driven={templateDriven} self-drawn={selfDrawn} no-default-style={noStyle} total={types.Count}");

        foreach (var name in census.VisualStateManagerUses) Note("VSM", $"generic theme itself uses VisualStateManager: {name}");
        Note("VSM", $"generic theme VisualStateManager occurrences: {census.VisualStateManagerUses.Count}; ControlTemplate count: {census.TemplateCount}; Trigger count: {census.TriggerCount}");
    }

    private static void ProbeThemeStyleResolution()
    {
        Note("STYLE", $"GenericThemeResourceName: {FieldOrProp(typeof(ThemeManager), "GenericThemeResourceName") ?? "null/absent"}");
        Note("STYLE", $"ControlsAssembly: {FieldOrProp(typeof(ThemeManager), "ControlsAssembly") ?? "null/absent"}");

        var controlsAssembly = typeof(Button).Assembly;
        var candidates = controlsAssembly.GetManifestResourceNames()
            .Where(static name => name.Contains("heme", StringComparison.Ordinal) || name.Contains("Generic", StringComparison.Ordinal) || name.EndsWith(".jalxaml", StringComparison.Ordinal))
            .ToList();
        Note("STYLE", $"{controlsAssembly.GetName().Name} embedded theme-ish resources: {candidates.Count} [{string.Join(", ", candidates.Take(20))}]");

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(static candidate => candidate.GetName().Name?.StartsWith("Jalium", StringComparison.Ordinal) == true))
        {
            var hits = assembly.GetManifestResourceNames().Where(static name => name.EndsWith(".jalxaml", StringComparison.Ordinal) || name.Contains("Generic", StringComparison.Ordinal)).ToList();
            if (hits.Count > 0) Note("STYLE", $"  {assembly.GetName().Name}: {hits.Count} [{string.Join(", ", hits.Take(10))}]");
        }

        try
        {
            var loaded = typeof(ThemeManager).GetMethod("LoadGenericTheme", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
            Note("STYLE", $"LoadGenericTheme() -> {DescribeValue(loaded)}");
        }
        catch (Exception exception)
        {
            Note("STYLE", $"LoadGenericTheme threw {exception.InnerException?.GetType().Name ?? exception.GetType().Name}: {Trim(exception.InnerException?.Message ?? exception.Message)}");
        }

        var resolve = typeof(ThemeManager).GetMethod("ResolveThemeStyle", BindingFlags.Public | BindingFlags.Static);
        if (resolve is null) { Note("STYLE", "ResolveThemeStyle is not public"); return; }
        foreach (var type in new[] { typeof(Button), typeof(CheckBox), typeof(Slider), typeof(ComboBox), typeof(TextBox), typeof(ToggleSwitch), typeof(TreeView), typeof(ProgressBar), typeof(Jalium.UI.Controls.NavigationView), typeof(NumberBox), typeof(InfoBar), typeof(Expander) })
        {
            var style = resolve.Invoke(null, [type]) as Style;
            if (style is null) { Note("STYLE", $"{type.Name}: NO theme style"); continue; }
            var template = style.Setters.OfType<Setter>().FirstOrDefault(static setter => setter.Property.Name == "Template")?.Value as ControlTemplate;
            Note("STYLE", $"{type.Name}: theme style found, setters={style.Setters.Count} template={(template is null ? "none" : "yes")} templateTriggers={template?.Triggers.Count ?? -1} styleTriggers={style.Triggers.Count}");
        }
    }

    private static string DescribeValue(object? value) => value switch
    {
        null => "null",
        ResourceDictionary dictionary => $"ResourceDictionary keys={dictionary.Count} themeDictionaries={dictionary.ThemeDictionaries?.GetType().Name ?? "null"}",
        _ => $"{value.GetType().Name}",
    };

    private static object? FieldOrProp(Type type, string name) =>
        type.GetField(name, BindingFlags.Public | BindingFlags.Static)?.GetValue(null)
        ?? type.GetProperty(name, BindingFlags.Public | BindingFlags.Static)?.GetValue(null);

    private static void ProbeImplicitStyle(Application application)
    {
        var implicitButton = new Style(typeof(Button));
        implicitButton.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0x12, 0x34, 0x56))));
        implicitButton.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(Color.FromRgb(0xAB, 0xCD, 0xEF))));
        application.Resources[typeof(Button)] = implicitButton;

        var implicitProgress = new Style(typeof(ProgressBar));
        implicitProgress.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(Color.FromRgb(0x01, 0x02, 0x03))));
        application.Resources[typeof(ProgressBar)] = implicitProgress;

        var autoButton = new Button { Content = "implicit" };
        var hosted = new StackPanel();
        hosted.Children.Add(autoButton);
        var window = new Window { Content = hosted, Width = 320, Height = 200 };
        window.Loaded += (_, _) =>
        {
            autoButton.ApplyTemplate();
            Note("IMPLICIT", $"Button via Application.Resources[typeof(Button)]: bg={Brush(autoButton.Background)} fg={Brush(autoButton.Foreground)}");
            var progress = new ProgressBar();
            hosted.Children.Add(progress);
            progress.ApplyTemplate();
            Note("IMPLICIT", $"ProgressBar implicit: fg={Brush(progress.Foreground)} (WinUI accent equivalent)");

            var explicitStyle = new Style(typeof(Button));
            explicitStyle.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0xFE, 0xDC, 0xBA))));
            var manual = new Button { Content = "explicit", Style = explicitStyle };
            hosted.Children.Add(manual);
            manual.ApplyTemplate();
            Note("IMPLICIT", $"Button with Style assigned directly: bg={Brush(manual.Background)}");

            var modeType = typeof(Application).GetProperty("ThemeMode")?.PropertyType;
            typeof(Application).GetProperty("ThemeMode")?.SetValue(application, modeType?.GetProperty("Dark")?.GetValue(null));
            Note("IMPLICIT", $"after ThemeMode.Dark, implicit Button bg={Brush(autoButton.Background)} (an explicit style setter is not theme-driven unless it uses {{ThemeResource}})");
            window.Close();
        };
        application.Run(window);
    }

    private static string Brush(Brush? brush) => brush switch
    {
        null => "null",
        SolidColorBrush solid => solid.Color.ToString(),
        _ => brush.GetType().Name,
    };

    private static void FluentSurfaces()
    {
        var backdrop = Type.GetType("Jalium.UI.Media.WindowBackdropType, Jalium.UI.Managed")
            ?? Type.GetType("Jalium.UI.WindowBackdropType, Jalium.UI.Managed")
            ?? AppDomain.CurrentDomain.GetAssemblies().SelectMany(static assembly => SafeTypes(assembly))
                .FirstOrDefault(static type => type.Name == "WindowBackdropType");
        Note("FLUENT", $"WindowBackdropType: {(backdrop is null ? "not found" : string.Join(", ", Enum.GetNames(backdrop)))}");

        var symbol = Type.GetType("Jalium.UI.Controls.Symbol, Jalium.UI.Managed") ?? AppDomain.CurrentDomain.GetAssemblies().SelectMany(static assembly => SafeTypes(assembly)).FirstOrDefault(static type => type.Name == "Symbol" && type.IsEnum);
        Note("FLUENT", $"Symbol enum members: {(symbol is null ? "not found" : Enum.GetNames(symbol).Length.ToString())}");

        var fontFamily = typeof(SymbolIcon).Assembly.GetType("Jalium.UI.Controls.SymbolIcon");
        Note("FLUENT", $"SymbolIcon font surface: {string.Join(", ", (fontFamily?.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) ?? []).Select(static property => property.Name))}");
        foreach (var name in new[] { "AcrylicBrush", "MicaBrush", "RevealBrush", "RadialGradientBrush", "LinearGradientBrush", " SolidColorBrush" })
            Note("FLUENT", $"brush type '{name.Trim()}' present: {Type.GetType($"Jalium.UI.Media.{name.Trim()}, Jalium.UI.Managed") is not null}");

        Note("FLUENT", $"ThemeManager.ApplyTypography: {string.Join(" | ", typeof(ThemeManager).GetMethods().Where(static method => method.Name == "ApplyTypography").Select(static method => "(" + string.Join(",", method.GetParameters().Select(static parameter => parameter.ParameterType.Name)) + ")"))}");
        Note("FLUENT", $"fonts now: display={ThemeManager.CurrentDisplayFontFamily} body={ThemeManager.CurrentBodyFontFamily} mono={ThemeManager.CurrentMonospaceFontFamily} bodySize={ThemeManager.CurrentBodyFontSize}");
        Note("FLUENT", $"SystemAccentResolver settable: {typeof(ThemeManager).GetProperty("SystemAccentResolver")?.GetSetMethod() is not null}");
    }

    /// <summary>
    /// Re-measures the names the stage plan still owes, because adaptation/05 section D is a dated snapshot
    /// that already contradicts measured fact (it lists MenuFlyout as absent, and the menu batch drove it with
    /// ShowAt). For each name: is there a type at all, what does it inherit, and if it is a Control, does it
    /// open ContentControl's template lock (S0-m) and does a constructed instance carry a factory Template or
    /// Style. Reference-only reflection; nothing here ships.
    /// </summary>
    private static void OutstandingNames()
    {
        // The original 2026-09-19 list is kept in the order it was run in (docs/astra/adaptation/
        // s0y-outstanding-names.txt). The stage-6 tail below was added on 2026-09-21 for the divider and icon
        // batch, so its readings live in s1q-divider-raw.txt instead.
        var names = new[]
        {
            "TeachingTip", "Card", "CardAction", "CardGroup", "Divider", "Expander",
            "InfoBadge", "RatingControl", "ProgressRing", "TabView", "BreadcrumbBar",
            "RadioButtons", "PipsPager", "MenuFlyout", "MenuFlyoutPresenter", "NavigationView",
            "Flyout", "FlyoutBase", "FlyoutPresenter", "MenuScroller", "MenuScrollViewer",
            "RadioMenuFlyoutItem", "SplitMenuFlyoutItem", "ToggleMenuFlyoutItem", "MenuFlyoutSubItem",
            "InfoBar", "CardElement",
            "Separator", "GridSplitter", "ToolBar", "Status",
            "SymbolIcon", "FontIcon", "BitmapIcon", "PathIcon", "ImageIcon", "AnimatedIcon",
            "IconSource", "FontIconSource", "BitmapIconSource", "PathIconSource", "Symbol",
            "SymbolRegular", "SymbolEnum", "Glyphs",
        };
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(static assembly => SafeTypes(assembly)).ToArray();
        foreach (var name in names)
        {
            Type? type = null;
            foreach (var candidate in types)
            {
                if (candidate.Name == name)
                {
                    type = candidate;
                    break;
                }
            }

            if (type is null)
            {
                Note("OUTSTANDING", $"{name}: no type with this name");
                continue;
            }

            var chain = new List<string>();
            for (var baseType = type.BaseType; baseType is not null && baseType != typeof(ValueType); baseType = baseType.BaseType)
            {
                chain.Add(baseType.Name);
            }

            var control = typeof(Jalium.UI.Controls.Control).IsAssignableFrom(type);
            var detail = control ? DescribeInstance(type) : "not a Control";
            Note("OUTSTANDING", $"{name}: {type.FullName} : {string.Join(" : ", chain)} | {detail}");
        }
    }

    private static string DescribeInstance(Type type)
    {
        try
        {
            var instance = Activator.CreateInstance(type);
            var style = type.GetProperty("Style")?.GetValue(instance);
            var template = type.GetProperty("Template")?.GetValue(instance);
            var lockMethod = type.GetMethod("UseTemplateContentManagement",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            object? locked = null;
            if (lockMethod is not null)
            {
                try
                {
                    locked = lockMethod.Invoke(lockMethod.IsStatic ? null : instance, null);
                }
                catch (Exception exception)
                {
                    locked = $"threw {exception.InnerException?.GetType().Name ?? exception.GetType().Name}";
                }
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length;
            return $"style={style is not null} factoryTemplate={template is not null} " +
                $"templateLock={(lockMethod is null ? "no such method" : $"{lockMethod.ReturnType.Name}:{locked?.ToString() ?? "null-or-void"}")} declaredProperties={properties}";
        }
        catch (Exception exception)
        {
            var constructors = string.Join(", ", type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(static ctor => $"{(ctor.IsPublic ? "public" : "nonpublic")}({string.Join(",", ctor.GetParameters().Select(static parameter => parameter.ParameterType.Name))})"));
            return $"ctor threw {(exception.InnerException ?? exception).GetType().Name}: {constructors}";
        }
    }

    private static IEnumerable<Type> SafeTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch
        {
            return [];
        }
    }

    private static string Trim(string message) => message.ReplaceLineEndings(" ");

    private static void Note(string category, string message) => Lines.Add($"[{category}] {message}");
}

/// <summary>What Jalium's shipped theme actually declares.</summary>
internal sealed class Census
{
    public Census(string? genericThemePath)
    {
        if (string.IsNullOrEmpty(genericThemePath) || !File.Exists(genericThemePath)) return;
        XDocument document;
        try
        {
            document = XDocument.Load(genericThemePath);
        }
        catch (Exception exception)
        {
            Program.Lines.Add($"[GENERIC] parse failed: {exception.GetType().Name}: {exception.Message.ReplaceLineEndings(" ")}");
            return;
        }

        var xaml = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");
        foreach (var element in document.Descendants())
        {
            switch (element.Name.LocalName)
            {
                case "Style":
                    {
                        var target = element.Attribute("TargetType")?.Value ?? string.Empty;
                        var key = element.Attribute(xaml + "Key")?.Value;
                        target = target[(target.LastIndexOf('.') + 1)..];
                        if (key is null) StyleTargets.Add(target);
                        else KeyedStyles.Add($"{key} -> {target}");
                        break;
                    }

                case "ControlTemplate":
                    TemplateCount++;
                    break;
                case "Trigger":
                case "MultiTrigger":
                    TriggerCount++;
                    break;
                case "VisualStateGroup":
                    VisualStateManagerUses.Add(element.Attribute("Name")?.Value ?? element.Attribute(xaml + "Name")?.Value ?? "?");
                    break;
            }

            if (element.Attribute(xaml + "Key")?.Value is { Length: > 0 } attributedKey && element.Name.LocalName.Contains("Brush", StringComparison.Ordinal))
                BrushKeys.Add(attributedKey);
        }
        Program.Lines.Add($"[GENERIC] ThemeDictionaries blocks: {document.Descendants().Count(static element => element.Name.LocalName == "ThemeDictionaries")}");
        Program.Lines.Add($"[GENERIC] distinct resource keys: {document.Descendants().Select(static element => element.Attribute(XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml") + "Key")?.Value).Where(static value => !string.IsNullOrEmpty(value)).Distinct().Count()}");
    }

    public HashSet<string> StyleTargets { get; } = new(StringComparer.Ordinal);

    public List<string> KeyedStyles { get; } = [];

    public List<string> BrushKeys { get; } = [];

    public List<string> VisualStateManagerUses { get; } = [];

    public int TemplateCount;

    public int TriggerCount;
}
