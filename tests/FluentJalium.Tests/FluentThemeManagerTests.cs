using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentThemeManagerTests
{
    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldInsertFluentDictionariesOnlyOnce()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            var baseCount = app.Resources.MergedDictionaries.Count;

            FluentThemeManager.Apply(app);
            var firstCount = app.Resources.MergedDictionaries.Count;

            FluentThemeManager.Apply(app);
            var secondCount = app.Resources.MergedDictionaries.Count;

            Assert.Equal(baseCount + 3, firstCount);
            Assert.Equal(firstCount, secondCount);
            Assert.True(app.Resources.TryGetValue(typeof(Button), out var style));
            Assert.IsType<Style>(style);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ApplyTheme_ShouldUpdateCurrentThemeKeyAndThemeResources()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);
            var darkWindowBackground = GetBrushColor(app.Resources["WindowBackground"]);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var lightWindowBackground = GetBrushColor(app.Resources["WindowBackground"]);

            Assert.Equal(FluentThemeVariant.Light, FluentThemeManager.CurrentTheme);
            Assert.Equal("Light", ResourceDictionary.CurrentThemeKey);
            Assert.NotEqual(darkWindowBackground, lightWindowBackground);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ApplyAccent_ShouldGenerateDefaultHoverPressedAndDisabledBrushes()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            var accent = Color.FromRgb(0xD8, 0x3B, 0x01);
            FluentThemeManager.ApplyAccent(accent);

            Assert.Equal(accent, FluentThemeManager.CurrentAccentColor);
            Assert.Equal(accent, GetBrushColor(app.Resources["AccentBrush"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["FluentAccentBrush"]));
            Assert.IsType<SolidColorBrush>(app.Resources["AccentBrushHover"]);
            Assert.IsType<SolidColorBrush>(app.Resources["AccentBrushPressed"]);
            Assert.IsType<SolidColorBrush>(app.Resources["AccentBrushDisabled"]);
            Assert.IsType<SolidColorBrush>(app.Resources["FluentAccentBrushHover"]);
            Assert.IsType<SolidColorBrush>(app.Resources["FluentAccentBrushPressed"]);
            Assert.IsType<SolidColorBrush>(app.Resources["FluentAccentBrushDisabled"]);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldLoadFromPackPathAndExposeCoreControlStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var owner = new ResourceDictionary();
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            owner,
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        Assert.NotNull(loaded);
        var dictionary = loaded!;

        AssertContainsStyle<Button>(dictionary);
        AssertContainsStyle<RepeatButton>(dictionary);
        AssertContainsStyle<HyperlinkButton>(dictionary);
        AssertContainsStyle<TextBox>(dictionary);
        AssertContainsStyle<PasswordBox>(dictionary);
        AssertContainsStyle<CheckBox>(dictionary);
        AssertContainsStyle<RadioButton>(dictionary);
        AssertContainsStyle<ToggleButton>(dictionary);
        AssertContainsStyle<ToggleSwitch>(dictionary);
        AssertContainsStyle<Slider>(dictionary);
        AssertContainsStyle<ProgressBar>(dictionary);
        AssertContainsStyle<ComboBox>(dictionary);

        Assert.True(dictionary.Contains("TextPrimary"));
        Assert.True(dictionary.Contains("AccentBrush"));
        Assert.True(dictionary.Contains("ControlContentThemeFontSize"));
    }

    private static void AssertContainsStyle<TControl>(ResourceDictionary dictionary)
        where TControl : Control
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        Assert.IsType<Style>(value);
    }

    private static Color GetBrushColor(object? value)
    {
        return Assert.IsType<SolidColorBrush>(value).Color;
    }

    private static void ResetApplicationState()
    {
        var currentField = typeof(Application).GetField("_current", BindingFlags.NonPublic | BindingFlags.Static);
        currentField?.SetValue(null, null);

        var jaliumReset = typeof(JaliumThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        jaliumReset?.Invoke(null, null);

        var fluentReset = typeof(FluentThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        fluentReset?.Invoke(null, null);
    }
}
