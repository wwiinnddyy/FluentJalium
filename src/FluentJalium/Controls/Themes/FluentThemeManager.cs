using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Controls.Themes;

/// <summary>
/// FluentJalium theme variants.
/// </summary>
public enum FluentThemeVariant
{
    Dark,
    Light,
    HighContrast
}

/// <summary>
/// Options used to apply the FluentJalium theme layer.
/// </summary>
public sealed class FluentThemeOptions
{
    public FluentThemeVariant Theme { get; init; } = FluentThemeVariant.Dark;

    public Color AccentColor { get; init; } = FluentThemeManager.DefaultAccentColor;

    public string? DisplayFontFamily { get; init; }

    public string? BodyFontFamily { get; init; }

    public string? MonoFontFamily { get; init; }
}

/// <summary>
/// Applies and updates the FluentJalium resource layer for a Jalium application.
/// </summary>
public static class FluentThemeManager
{
    public const string GenericThemeResourceName = "FluentJalium.Themes.Generic.jalxaml";

    private const string ThemeRefreshVersionKey = "__FluentJalium.ThemeVersion";
    private static ResourceDictionary? s_themeDictionary;
    private static ResourceDictionary? s_accentDictionary;
    private static ResourceDictionary? s_typographyDictionary;
    private static Application? s_application;
    private static int s_themeVersion;

    public static readonly Color DefaultAccentColor = Color.FromRgb(0x00, 0x78, 0xD4);

    public static FluentThemeVariant CurrentTheme { get; private set; } = FluentThemeVariant.Dark;

    public static Color CurrentAccentColor { get; private set; } = DefaultAccentColor;

    public static string CurrentDisplayFontFamily { get; private set; } = "Segoe UI Variable Display";

    public static string CurrentBodyFontFamily { get; private set; } = "Segoe UI Variable Text";

    public static string CurrentMonoFontFamily { get; private set; } = "Cascadia Code";

    public static Assembly ThemeAssembly => typeof(FluentThemeManager).Assembly;

    /// <summary>
    /// Applies the FluentJalium theme layer with default options.
    /// </summary>
    [RequiresUnreferencedCode("Loads a generated or XAML resource dictionary through Jalium.UI theme infrastructure.")]
    public static void Apply(Application app)
    {
        Apply(app, new FluentThemeOptions());
    }

    /// <summary>
    /// Applies the FluentJalium theme layer using the supplied options.
    /// </summary>
    [RequiresUnreferencedCode("Loads a generated or XAML resource dictionary through Jalium.UI theme infrastructure.")]
    public static void Apply(Application app, FluentThemeOptions options)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(options);

        s_application = app;
        CurrentTheme = options.Theme;
        CurrentAccentColor = options.AccentColor;
        CurrentDisplayFontFamily = NormalizeFontFamily(options.DisplayFontFamily, CurrentDisplayFontFamily);
        CurrentBodyFontFamily = NormalizeFontFamily(options.BodyFontFamily, CurrentBodyFontFamily);
        CurrentMonoFontFamily = NormalizeFontFamily(options.MonoFontFamily, CurrentMonoFontFamily);

        ResourceDictionary.CurrentThemeKey = CurrentTheme.ToString();

        var resources = app.Resources.MergedDictionaries;
        var theme = LoadGenericTheme();
        ReplaceOrAppend(resources, ref s_themeDictionary, theme);

        var accent = BuildAccentDictionary(CurrentAccentColor);
        ReplaceOrAppend(resources, ref s_accentDictionary, accent);

        var typography = BuildTypographyDictionary();
        ReplaceOrAppend(resources, ref s_typographyDictionary, typography);

        ForceRefresh(app);
    }

    /// <summary>
    /// Switches the active FluentJalium theme variant.
    /// </summary>
    [RequiresUnreferencedCode("Reloads a generated or XAML resource dictionary through Jalium.UI theme infrastructure.")]
    public static void ApplyTheme(FluentThemeVariant theme)
    {
        CurrentTheme = theme;
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        if (s_application == null)
            return;

        ReplaceOrAppend(s_application.Resources.MergedDictionaries, ref s_themeDictionary, LoadGenericTheme());
        ReplaceOrAppend(s_application.Resources.MergedDictionaries, ref s_accentDictionary, BuildAccentDictionary(CurrentAccentColor));
        ForceRefresh(s_application);
    }

    /// <summary>
    /// Switches the active FluentJalium accent color.
    /// </summary>
    public static void ApplyAccent(Color accent)
    {
        CurrentAccentColor = accent;

        if (s_application == null)
            return;

        ReplaceOrAppend(s_application.Resources.MergedDictionaries, ref s_accentDictionary, BuildAccentDictionary(accent));
        ForceRefresh(s_application);
    }

    /// <summary>
    /// Switches the active FluentJalium typography resources.
    /// </summary>
    public static void ApplyTypography(string display, string body, string mono)
    {
        CurrentDisplayFontFamily = NormalizeFontFamily(display, CurrentDisplayFontFamily);
        CurrentBodyFontFamily = NormalizeFontFamily(body, CurrentBodyFontFamily);
        CurrentMonoFontFamily = NormalizeFontFamily(mono, CurrentMonoFontFamily);

        if (s_application == null)
            return;

        ReplaceOrAppend(s_application.Resources.MergedDictionaries, ref s_typographyDictionary, BuildTypographyDictionary());
        ForceRefresh(s_application);
    }

    /// <summary>
    /// Resets static state for tests.
    /// </summary>
    internal static void Reset()
    {
        s_themeDictionary = null;
        s_accentDictionary = null;
        s_typographyDictionary = null;
        s_application = null;
        s_themeVersion = 0;
        CurrentTheme = FluentThemeVariant.Dark;
        CurrentAccentColor = DefaultAccentColor;
        CurrentDisplayFontFamily = "Segoe UI Variable Display";
        CurrentBodyFontFamily = "Segoe UI Variable Text";
        CurrentMonoFontFamily = "Cascadia Code";
    }

    [RequiresUnreferencedCode("Uses Jalium.UI ResourceDictionary.SourceLoader fallback when generated dictionaries are unavailable.")]
    private static ResourceDictionary LoadGenericTheme()
    {
        var source = new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative);
        var owner = new ResourceDictionary();
        var loaded = ResourceDictionary.SourceLoader?.Invoke(owner, source, ThemeAssembly);
        if (loaded != null)
        {
            AddFluentControlStyleAliases(loaded);
            return loaded;
        }

        var fallback = new ResourceDictionary
        {
            Source = source
        };
        AddFluentControlStyleAliases(fallback);
        return fallback;
    }

    private static void AddFluentControlStyleAliases(ResourceDictionary dictionary)
    {
        AliasStyle<FWButton, Button>(dictionary);
        AliasStyle<FWRepeatButton, RepeatButton>(dictionary);
        AliasStyle<FWHyperlinkButton, HyperlinkButton>(dictionary);
        AliasStyle<FWCheckBox, CheckBox>(dictionary);
        AliasStyle<FWRadioButton, RadioButton>(dictionary);
        AliasStyle<FWToggleButton, ToggleButton>(dictionary);
        AliasStyle<FWToggleSwitch, ToggleSwitch>(dictionary);
        AliasStyle<FWSlider, Slider>(dictionary);
        AliasStyle<FWRangeSlider, RangeSlider>(dictionary);
        AliasStyle<FWProgressBar, ProgressBar>(dictionary);
        AliasStyle<FWComboBox, ComboBox>(dictionary);
        AliasStyle<FWComboBoxItem, ComboBoxItem>(dictionary);
        AliasStyle<FWListBox, ListBox>(dictionary);
        AliasStyle<FWListBoxItem, ListBoxItem>(dictionary);
        AliasStyle<FWListView, ListView>(dictionary);
        AliasStyle<FWListViewItem, ListViewItem>(dictionary);
        AliasStyle<FWTreeView, TreeView>(dictionary);
        AliasStyle<FWTreeViewItem, TreeViewItem>(dictionary);
        AliasStyle<FWDataGrid, DataGrid>(dictionary);
        AliasStyle<FWTreeDataGrid, TreeDataGrid>(dictionary);
        AliasStyle<FWNavigationView, NavigationView>(dictionary);
        AliasStyle<FWNavigationViewItem, NavigationViewItem>(dictionary);
        AliasStyle<FWNavigationViewItemHeader, NavigationViewItemHeader>(dictionary);
        AliasStyle<FWNavigationViewItemSeparator, NavigationViewItemSeparator>(dictionary);
        AliasStyle<FWTabControl, TabControl>(dictionary);
        AliasStyle<FWTabItem, TabItem>(dictionary);
        AliasStyle<FWFrame, Frame>(dictionary);
        AliasStyle<FWSplitButton, SplitButton>(dictionary);
        AliasStyle<FWAppBarButton, AppBarButton>(dictionary);
        AliasStyle<FWAppBarToggleButton, AppBarToggleButton>(dictionary);
        AliasStyle<FWAppBarSeparator, AppBarSeparator>(dictionary);
    }

    private static void AliasStyle<TFluentControl, TJaliumControl>(ResourceDictionary dictionary)
        where TFluentControl : Control, IFluentJaliumControl
        where TJaliumControl : Control
    {
        if (dictionary.TryGetValue(typeof(TJaliumControl), out var baseStyle) && baseStyle is Style style)
        {
            dictionary[typeof(TFluentControl)] = new Style(typeof(TFluentControl), style);
        }
    }

    private static ResourceDictionary BuildAccentDictionary(Color accent)
    {
        var light1 = Blend(accent, Color.White, 0.20);
        var light2 = Blend(accent, Color.White, 0.36);
        var light3 = Blend(accent, Color.White, 0.52);
        var dark1 = Blend(accent, Color.Black, 0.14);
        var dark2 = Blend(accent, Color.Black, 0.28);
        var dark3 = Blend(accent, Color.Black, 0.42);
        var hover = CurrentTheme == FluentThemeVariant.Dark ? light1 : dark1;
        var pressed = CurrentTheme == FluentThemeVariant.Dark ? dark1 : dark2;
        var disabled = CurrentTheme == FluentThemeVariant.Dark
            ? Color.FromArgb(0x5D, 0xFF, 0xFF, 0xFF)
            : Color.FromArgb(0x37, 0x00, 0x00, 0x00);

        return new ResourceDictionary
        {
            ["SystemAccentColor"] = accent,
            ["SystemAccentColorLight1"] = light1,
            ["SystemAccentColorLight2"] = light2,
            ["SystemAccentColorLight3"] = light3,
            ["SystemAccentColorDark1"] = dark1,
            ["SystemAccentColorDark2"] = dark2,
            ["SystemAccentColorDark3"] = dark3,
            ["FluentAccentFillColorDefault"] = accent,
            ["FluentAccentFillColorSecondary"] = hover,
            ["FluentAccentFillColorTertiary"] = pressed,
            ["FluentAccentFillColorDisabled"] = disabled,
            ["AccentFillColorDefault"] = accent,
            ["AccentFillColorSecondary"] = hover,
            ["AccentFillColorTertiary"] = pressed,
            ["AccentFillColorDisabled"] = disabled,
            ["FluentAccentBrush"] = new SolidColorBrush(accent),
            ["FluentAccentBrushHover"] = new SolidColorBrush(hover),
            ["FluentAccentBrushPressed"] = new SolidColorBrush(pressed),
            ["FluentAccentBrushDisabled"] = new SolidColorBrush(disabled),
            ["AccentBrush"] = new SolidColorBrush(accent),
            ["AccentBrushHover"] = new SolidColorBrush(hover),
            ["AccentBrushPressed"] = new SolidColorBrush(pressed),
            ["AccentBrushDisabled"] = new SolidColorBrush(disabled),
            ["AccentFillColorDefaultBrush"] = new SolidColorBrush(accent),
            ["AccentFillColorSecondaryBrush"] = new SolidColorBrush(hover),
            ["AccentFillColorTertiaryBrush"] = new SolidColorBrush(pressed),
            ["AccentFillColorDisabledBrush"] = new SolidColorBrush(disabled),
            ["SelectionBackground"] = new SolidColorBrush(Color.FromArgb(0x66, accent.R, accent.G, accent.B)),
            ["SelectionBackgroundWeak"] = new SolidColorBrush(Color.FromArgb(0x33, accent.R, accent.G, accent.B)),
            ["NavigationViewItemBackgroundSelected"] = new SolidColorBrush(Color.FromArgb(0x33, accent.R, accent.G, accent.B)),
            ["NavigationViewItemBackgroundSelectedHover"] = new SolidColorBrush(Color.FromArgb(0x66, accent.R, accent.G, accent.B)),
            ["TabItemIndicator"] = new SolidColorBrush(accent),
            ["ProgressRingForeground"] = new SolidColorBrush(accent),
        };
    }

    private static ResourceDictionary BuildTypographyDictionary()
    {
        return new ResourceDictionary
        {
            ["FluentDisplayFontFamily"] = CurrentDisplayFontFamily,
            ["FluentBodyFontFamily"] = CurrentBodyFontFamily,
            ["FluentMonoFontFamily"] = CurrentMonoFontFamily,
            ["DisplayFontFamily"] = CurrentDisplayFontFamily,
            ["BodyFontFamily"] = CurrentBodyFontFamily,
            ["MonoFontFamily"] = CurrentMonoFontFamily,
            ["ControlContentThemeFontSize"] = 14.0,
            ["FluentCaptionFontSize"] = 12.0,
            ["FluentBodyFontSize"] = 14.0,
            ["FluentSubtitleFontSize"] = 20.0,
            ["FluentTitleFontSize"] = 28.0,
        };
    }

    private static void ReplaceOrAppend(IList<ResourceDictionary> dictionaries, ref ResourceDictionary? current, ResourceDictionary replacement)
    {
        if (current != null)
        {
            var index = dictionaries.IndexOf(current);
            if (index >= 0)
            {
                dictionaries[index] = replacement;
                current = replacement;
                return;
            }
        }

        if (!dictionaries.Contains(replacement))
        {
            dictionaries.Add(replacement);
        }

        current = replacement;
    }

    private static void ForceRefresh(Application app)
    {
        ResourceLookup.InvalidateResourceCache();
        app.Resources[ThemeRefreshVersionKey] = ++s_themeVersion;
    }

    private static string NormalizeFontFamily(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static Color Blend(Color color, Color target, double factor)
    {
        factor = Math.Clamp(factor, 0.0, 1.0);

        static byte Lerp(byte from, byte to, double t)
        {
            return (byte)Math.Clamp((int)Math.Round(from + ((to - from) * t)), 0, 255);
        }

        return Color.FromArgb(
            Lerp(color.A, target.A, factor),
            Lerp(color.R, target.R, factor),
            Lerp(color.G, target.G, factor),
            Lerp(color.B, target.B, factor));
    }
}

