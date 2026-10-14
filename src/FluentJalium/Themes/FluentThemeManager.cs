using System.Collections;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Media;
using Jalium.UI.Media.Animation;
using Microsoft.Win32;

namespace FluentJalium.Themes;

/// <summary>
/// Installs the Astra dictionaries once and keeps one palette live across variants. Brushes keep
/// their identity and are retinted in place, so existing references, user-set values and retained
/// drawing all follow without replacing controls; <see cref="ApplicationThemeDriver"/> hands the
/// same decision to Jalium so native defaults move together. High contrast applies WinUI's own
/// per-key mapping instead of guessing from key names. All mutation is owned by the application's
/// UI thread.
/// </summary>
public static class FluentThemeManager
{
    private static Application? _application;
    private static ResourceDictionary? _palette;
    private static ResourceDictionary? _light;
    private static ResourceDictionary? _dark;
    private static ResourceDictionary? _motion;
    private static readonly Dictionary<string, Duration> MotionDesigns = new(StringComparer.Ordinal);
    private static Dictionary<string, string>? _highContrastMap;
    private static readonly List<ResourceDictionary> Installed = [];
    private static readonly Dictionary<string, Color> BrushOverrides = new(StringComparer.Ordinal);
    private static int _threadId;
    private static bool _reduceMotion;
    private static Color? _accent;

    /// <summary>
    /// The body size the framework is asked to project its type scale from. Upstream sets control content and the body
    /// text style to 14 and the caption text style to 12 (microsoft-ui-xaml @19e3bdc3,
    /// dxaml/xcp/dxaml/themes/generic.xaml:36 / :13280 / :13283); 26.10.9 projects 12/10/8 from a body of 12 while its
    /// own <c>TextBlock</c> default is already 14, so the projection is the half that disagrees.
    /// </summary>
    private const double UpstreamBodyFontSize = 14d;

    /// <summary>The style dictionaries loaded in dependency order, after the palette. Read from Themes/Manifest.txt.</summary>
    public static IReadOnlyList<string> DictionaryNames => Manifest ??= ReadManifest();

    private static IReadOnlyList<string>? Manifest;

    public static FluentThemeVariant CurrentTheme { get; private set; } = FluentThemeVariant.System;
    public static bool IsDark { get; private set; }
    public static bool IsHighContrast { get; private set; }
    public static bool IsInitialized => _application is not null;

    /// <summary>The mode Jalium itself was last asked for: Light, Dark, System or None.</summary>
    public static string NativeThemeMode => _application is null ? ApplicationThemeDriver.None : ApplicationThemeDriver.Read(_application);

    public static Color CurrentAccentColor => _accent ?? (IsDark ? Color.FromRgb(0x60, 0xCD, 0xFF) : Color.FromRgb(0, 0x78, 0xD4));
    public static bool AnimationsEnabled => !_reduceMotion && SystemParameters.ClientAreaAnimation;
    public static event Action? Changed;

    /// <summary>
    /// Suppresses the animations Astra drives from code - the page entrance and the navigation selection
    /// indicator - and rewrites the three published animation durations to zero, which is what a template
    /// transition reads at the moment it would start. The rows live in ThemeResources/Motion.jalxaml and are
    /// rewritten in place, so a realised template picks the change up on its next transition without a visual-tree
    /// walk; a transition already running keeps its clock. Jalium additionally gates every automatic transition
    /// on the OS accessibility settings, so a user who has turned animations off is honoured either way.
    /// </summary>
    public static bool ReduceMotion
    {
        get => _reduceMotion;
        set
        {
            VerifyAccess();
            if (_reduceMotion == value) return;
            _reduceMotion = value;
            ApplyMotionKeys();
            NotifyChanged();
        }
    }

    /// <summary>Call once after creating Application, before constructing application controls.</summary>
    public static void Apply(Application application, FluentThemeVariant theme = FluentThemeVariant.System)
    {
        ArgumentNullException.ThrowIfNull(application);
        if (!Enum.IsDefined(theme)) throw new ArgumentOutOfRangeException(nameof(theme));
        if (_application is not null)
        {
            VerifyAccess();
            if (!ReferenceEquals(application, _application)) throw new InvalidOperationException("Astra is already attached to another Application.");
            ApplyTheme(theme);
            return;
        }
        _threadId = Environment.CurrentManagedThreadId;
        _application = application;
        CurrentTheme = theme;
        try
        {
            _light = Load(Palette + "Light.jalxaml");
            _dark = Load(Palette + "Dark.jalxaml");
            _palette = Load(Palette + "Light.jalxaml");
            RefreshPalette();
            Add(_palette);
            foreach (var path in DictionaryNames)
            {
                var dictionary = Load(path);
                if (path.EndsWith(MotionDictionary, StringComparison.Ordinal)) _motion = dictionary;
                Add(dictionary);
            }
            if (_motion is null) throw new InvalidOperationException($"The Astra manifest does not carry {MotionDictionary}, so ReduceMotion would have nothing to rewrite.");
            CaptureMotionDesigns();
            ApplyMotionKeys();

            // The last step of the install, and it has to be a step: a {ThemeResource BodyFontSize} consumer that is
            // already built does not re-resolve when the framework swaps its typography dictionary (a block mounted
            // before the call kept reading 12 afterwards - spike/TypoProbe), so the swap belongs here, before any
            // application control exists. The families are handed straight back, because this call exists to move one
            // number and not to choose a typeface; SystemFonts keeps its own sizes either way.
            ThemeManager.ApplyTypography(
                ThemeManager.CurrentDisplayFontFamily,
                ThemeManager.CurrentBodyFontFamily,
                ThemeManager.CurrentMonospaceFontFamily,
                UpstreamBodyFontSize);
        }
        catch
        {
            foreach (var dictionary in Installed) application.Resources.MergedDictionaries.Remove(dictionary);
            Installed.Clear();
            _application = null;
            _palette = _light = _dark = null;
            _motion = null;
            MotionDesigns.Clear();
            throw;
        }
        NotifyChanged();

        void Add(ResourceDictionary dictionary)
        {
            application.Resources.MergedDictionaries.Add(dictionary);
            Installed.Add(dictionary);
        }
    }

    public static void ApplyTheme(FluentThemeVariant theme)
    {
        VerifyAccess();
        if (!Enum.IsDefined(theme)) throw new ArgumentOutOfRangeException(nameof(theme));
        CurrentTheme = theme;
        RefreshSystemTheme();
    }

    /// <summary>Null restores the source-backed application accent. The Windows accent ramp is not emulated.</summary>
    public static void ApplyAccent(Color? color)
    {
        VerifyAccess();
        if (color is { A: < 255 }) throw new ArgumentException("An accent must be opaque.", nameof(color));
        _accent = color;
        RefreshSystemTheme();
    }

    /// <summary>Overrides one semantic brush while preserving every existing reference to it.</summary>
    public static void OverrideBrush(string key, Color? color)
    {
        VerifyAccess();
        _ = GetBrush(key);
        if (color is { } value) BrushOverrides[key] = value;
        else BrushOverrides.Remove(key);
        RefreshSystemTheme();
    }

    /// <summary>Host windows should call this from SystemSettingsChanged.</summary>
    public static void RefreshSystemTheme()
    {
        VerifyAccess();
        RefreshPalette();
        NotifyChanged();
    }

    public static Brush GetBrush(string key)
    {
        VerifyAccess();
        return _palette?[key] as Brush ?? throw new KeyNotFoundException($"Unknown Astra brush: {key}");
    }

    public static Style GetStyle(string key)
    {
        VerifyAccess();
        return _application!.Resources[key] as Style ?? throw new KeyNotFoundException($"Unknown Astra style: {key}");
    }

    private static void VerifyAccess()
    {
        if (_application is null) throw new InvalidOperationException("Call FluentThemeManager.Apply(application) before using Astra resources.");
        if (_threadId != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Theme changes must run on the application's UI thread.");
    }

    private static ResourceDictionary Load(string name)
    {
        using var stream = OpenResource(name) ?? throw new InvalidOperationException($"Missing Astra resource: {name}");
        try
        {
            return Jalium.UI.Markup.XamlReader.Load(stream) as ResourceDictionary
                ?? throw new InvalidOperationException("Resource is not a dictionary.");
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Cannot parse Astra resource: {name}", exception);
        }
    }

    /// <summary>
    /// Embedded names keep the backslash that %(RecursiveDir) produces on Windows, so lookups
    /// are normalised to forward slashes once here rather than at each call site.
    /// </summary>
    private static Stream? OpenResource(string name)
    {
        var normalized = name.Replace('\\', '/');
        return EmbeddedNames.TryGetValue(normalized, out var actual)
            ? typeof(FluentThemeManager).Assembly.GetManifestResourceStream(actual)
            : null;
    }

    private const string Palette = "ThemeResources/";
    private const string MotionDictionary = "Motion.jalxaml";
    private static readonly string[] ResourceRoots = ["ThemeResources/", "Styles/", "Themes/"];

    private static readonly Dictionary<string, string> EmbeddedNames = BuildEmbeddedNames();

    private static Dictionary<string, string> BuildEmbeddedNames()
    {
        var names = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var name in typeof(FluentThemeManager).Assembly.GetManifestResourceNames())
        {
            if (ResourceRoots.Any(root => name.StartsWith(root, StringComparison.Ordinal))) names[name.Replace('\\', '/')] = name;
        }
        return names;
    }

    private static void RefreshPalette()
    {
        IsDark = CurrentTheme == FluentThemeVariant.Dark || (CurrentTheme == FluentThemeVariant.System && IsSystemDark());
        IsHighContrast = CurrentTheme == FluentThemeVariant.HighContrast || SystemParameters.HighContrast;
        ResourceDictionary.CurrentThemeKey = IsHighContrast ? "HighContrast" : IsDark ? "Dark" : "Light";
        var source = IsDark ? _dark : _light;
        foreach (DictionaryEntry entry in source!)
        {
            if (entry.Value is SolidColorBrush from && _palette![entry.Key] is SolidColorBrush to)
            {
                to.Color = from.Color;
                to.Opacity = from.Opacity;
            }
            else _palette![entry.Key] = entry.Value;
        }
        if (IsHighContrast) ApplyHighContrastPalette();
        else if (_accent is { } accent)
        {
            foreach (var key in new[] { "AccentFillColorDefaultBrush", "AccentFillColorSecondaryBrush", "AccentFillColorTertiaryBrush", "AccentTextFillColorPrimaryBrush", "AccentTextFillColorSecondaryBrush", "AccentTextFillColorTertiaryBrush", "AccentFillColorSelectedTextBackgroundBrush" }) SetColor(key, accent);
            // Keep text readable for an application-specified accent, independent of theme.
            var text = RelativeLuminance(accent) > 0.179 ? Colors.Black : Colors.White;
            SetColor("TextOnAccentFillColorPrimaryBrush", text);
            SetColor("TextOnAccentFillColorSecondaryBrush", Color.FromArgb(0xB3, text.R, text.G, text.B));
        }
        foreach (var (key, value) in BrushOverrides) SetColor(key, value);
        ApplyNativeThemeMode();
    }

    /// <summary>
    /// Reads every published duration back out of the loaded row, so the designed numbers keep one authority: a
    /// code-side copy of 83ms and 167ms would be a second set that could drift from the dictionary. A row the reader
    /// turned into something that is not a <see cref="Duration"/> is not captured, which is what makes an
    /// unreadable row a startup failure rather than a silent default.
    /// </summary>
    private static void CaptureMotionDesigns()
    {
        MotionDesigns.Clear();
        foreach (DictionaryEntry entry in _motion!)
        {
            if (entry.Key is string key && entry.Value is Duration duration && duration.HasTimeSpan) MotionDesigns[key] = duration;
        }

        if (MotionDesigns.Count == 0) throw new InvalidOperationException("ThemeResources/Motion.jalxaml holds no row that reads back as a Duration.");
    }

    /// <summary>
    /// Writes the designed duration, or zero, into the live motion rows. Zero is what the framework reads as "do not
    /// animate": UIElement consults TransitionDuration at the moment a transition would start and starts nothing when
    /// the value has no TimeSpan or is at or below zero, so rewriting the row reaches a realised template on its next
    /// transition without a visual-tree walk (spike/MotionProbe [3b] and [4]).
    /// </summary>
    private static void ApplyMotionKeys()
    {
        if (_motion is null) return;
        foreach (var (key, designed) in MotionDesigns) _motion[key] = _reduceMotion ? new Duration(TimeSpan.Zero) : designed;
    }

    /// <summary>
    /// Hands the decision about native control defaults back to Jalium. High contrast has no
    /// Jalium equivalent, so it resolves to the concrete mode our palette is already showing.
    /// </summary>
    private static void ApplyNativeThemeMode()
    {
        var mode = CurrentTheme switch
        {
            FluentThemeVariant.Light => ApplicationThemeDriver.Light,
            FluentThemeVariant.Dark => ApplicationThemeDriver.Dark,
            _ => IsHighContrast ? (IsDark ? ApplicationThemeDriver.Dark : ApplicationThemeDriver.Light) : ApplicationThemeDriver.System,
        };
        ApplicationThemeDriver.Apply(_application!, mode);
    }

    private static IReadOnlyList<string> ReadManifest()
    {
        const string manifest = "Themes/Manifest.txt";
        var names = new List<string>();
        using (var stream = OpenResource(manifest)
            ?? throw new InvalidOperationException($"Missing Astra resource: {manifest}"))
        using (var reader = new StreamReader(stream))
        {
            var lineNumber = 0;
            while (reader.ReadLine() is { } text)
            {
                lineNumber++;
                var entry = text.Trim().Replace('\\', '/');
                if (entry.Length == 0 || entry.StartsWith('#')) continue;
                if (OpenResource(entry) is null)
                    throw new InvalidOperationException($"Astra manifest line {lineNumber} lists {entry}, which is not an embedded resource.");
                names.Add(entry);
            }
        }
        if (names.Count == 0) throw new InvalidOperationException("Astra manifest lists no dictionaries.");

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in names)
        {
            if (!seen.Add(name)) throw new InvalidOperationException($"Astra manifest lists {name} more than once.");
        }
        var accounted = new HashSet<string>(seen, StringComparer.Ordinal) { Palette + "Light.jalxaml", Palette + "Dark.jalxaml" };
        // An unlisted dictionary embeds and parses fine yet never loads; fail the same way a missing one does.
        foreach (var resource in EmbeddedNames.Keys)
        {
            if (!resource.EndsWith(".jalxaml", StringComparison.Ordinal)) continue;
            if (!accounted.Contains(resource))
                throw new InvalidOperationException($"{resource} is not listed in {manifest}.");
        }
        return names.AsReadOnly();
    }

    private static void SetColor(string key, Color color)
    {
        if (_palette![key] is SolidColorBrush brush) brush.Color = color;
        else throw new KeyNotFoundException($"Palette is missing {key}");
    }

    /// <summary>The eight Win32 high-contrast colors WinUI references, plus the transparent role.</summary>
    private static Color SystemColor(string name) => name switch
    {
        "Transparent" => Colors.Transparent,
        "SystemColorWindowColor" => SystemColors.WindowColor,
        "SystemColorWindowTextColor" => SystemColors.WindowTextColor,
        "SystemColorButtonFaceColor" => SystemColors.ControlColor,
        "SystemColorButtonTextColor" => SystemColors.ControlTextColor,
        "SystemColorGrayTextColor" => SystemColors.GrayTextColor,
        "SystemColorHighlightColor" => SystemColors.HighlightColor,
        "SystemColorHighlightTextColor" => SystemColors.HighlightTextColor,
        "SystemColorHotlightColor" => SystemColors.HotTrackColor,
        _ => throw new InvalidOperationException($"Unknown Win32 system color name: {name}"),
    };

    /// <summary>
    /// Platform substitution: applies WinUI's own per-key high-contrast mapping, generated from the
    /// upstream HighContrast branch by tools/Sync-AstraPalette.ps1. It does not claim every WinUI
    /// per-control high-contrast visual-state override has been ported.
    /// </summary>
    private static void ApplyHighContrastPalette()
    {
        foreach (var (key, name) in HighContrastMap)
        {
            // The eight platform-slot rows are aliases onto the framework's own colour resources, not values to
            // resolve: SystemColors.<Slot> reads a resource named <Slot>Brush before it consults the platform, so
            // writing a resolved colour through here is a second write over the channel every reader goes through
            // (spike/SystemColorProbe measures that priority, and measures the collapse it caused). RefreshPalette
            // has already copied these rows' resolved values in, so skipping them here costs nothing - and whether a
            // row would then still follow a later platform change is unmeasured, because this host has no
            // high-contrast scheme to switch.
            if (key.StartsWith("SystemColor", StringComparison.Ordinal)) continue;
            if (_palette![key] is not SolidColorBrush brush) throw new InvalidOperationException($"High-contrast map targets a key that is not a brush: {key}");
            brush.Color = SystemColor(name);
            brush.Opacity = 1;
        }
    }

    internal static Dictionary<string, string> HighContrastMap => _highContrastMap ??= ReadHighContrastMap();

    /// <summary>
    /// Every brush key the live palette declares, for the resource gates. Membership is decided by
    /// the value, not by a name suffix: the ScrollBar hooks are brushes whose names the framework
    /// fixed, and a suffix rule would drop them out of the high-contrast coverage gate.
    /// </summary>
    internal static IReadOnlyList<string> PaletteBrushKeys
    {
        get
        {
            VerifyAccess();
            return [.. _palette!.Keys.OfType<string>().Where(static key => _palette[key] is SolidColorBrush).Order(StringComparer.Ordinal)];
        }
    }

    private static Dictionary<string, string> ReadHighContrastMap()
    {
        const string name = Palette + "HighContrast.map";
        using var stream = OpenResource(name) ?? throw new InvalidOperationException($"Missing Astra resource: {name}");
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        using var reader = new StreamReader(stream);
        var lineNumber = 0;
        while (reader.ReadLine() is { } text)
        {
            lineNumber++;
            var line = text.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            var separator = line.IndexOf('=');
            if (separator <= 0) throw new InvalidOperationException($"{name} line {lineNumber} is not key=value.");
            var key = line[..separator];
            var target = line[(separator + 1)..];
            SystemColor(target); // Fail on an unmappable name here rather than on every theme switch.
            if (_palette is null || !_palette.Contains(key)) throw new InvalidOperationException($"{name} line {lineNumber} maps {key}, which the palette does not declare.");
            map.Add(key, target);
        }
        if (map.Count == 0) throw new InvalidOperationException($"{name} lists no keys.");
        return map;
    }

    private static bool IsSystemDark()
    {
        // 这条路径只在「跟随系统主题」时走。注册表是 Windows 的事：别的平台上没有"系统深浅"可问，
        // 返回浅色（与没有该项时的行为一致）。异常兜底留给权限类问题，平台问题在入口就挡掉，
        // 免得 LibraryBuild 的 net10.0 目标上一堆 CA1416、Linux 上一发 PlatformNotSupportedException。
        if (!OperatingSystem.IsWindows()) return false;

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
        }
        catch (Exception exception) when (exception is System.Security.SecurityException or UnauthorizedAccessException or IOException)
        {
            return false;
        }
    }

    private static double RelativeLuminance(Color color)
    {
        static double Channel(byte value) { var c = value / 255.0; return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4); }
        return 0.2126 * Channel(color.R) + 0.7152 * Channel(color.G) + 0.0722 * Channel(color.B);
    }

    private static void NotifyChanged() => Changed?.Invoke();

    /// <summary>Optional short content entrance; does not replace the host's content.</summary>
    public static void Enter(UIElement element)
    {
        element.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline?)null);
        element.Opacity = 1;
        if (!AnimationsEnabled) return;
        element.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation
        {
            From = 0, To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(167)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }, FillBehavior = FillBehavior.Stop,
        });
    }
}
