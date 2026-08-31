using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Media;
using Xunit;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

/// <summary>
/// Guards the token layer the gallery shell is built on: the theme dictionaries must resolve
/// variant-specific values so a theme switch can repaint through resource notifications alone.
/// </summary>
/// <remarks>
/// The companion claim — that a live <c>{ThemeResource}</c> reference repaints without rebuilding
/// the visual tree — cannot be asserted here, because <see cref="Application"/> only notifies the
/// roots of windows that are actually shown. It is covered at runtime by the gallery's
/// <c>XamlPipelineProbe</c> page, which reports the live brush before and after a switch.
/// </remarks>
[Collection("Application")]
public sealed class FluentThemeResourceRefreshTests
{
    [Fact]
    [RequiresUnreferencedCode("Loads the generated theme dictionaries through FluentThemeManager.")]
    public void ApplyTheme_ResolvesThemeVariantSpecificBrushValue()
    {
        ResetApplicationState();
        var app = new Application();
        JaliumThemeManager.Initialize(app);
        FluentThemeManager.Apply(app);

        try
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var dark = ResolvePrimaryText(app);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var light = ResolvePrimaryText(app);

            Assert.Equal("#FFFFFFFF", ToHex(dark));
            Assert.Equal("#E4000000", ToHex(light));
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Loads the generated theme dictionaries through FluentThemeManager.")]
    public void ApplyTheme_ResolvesNewGalleryTokensPerVariant()
    {
        ResetApplicationState();
        var app = new Application();
        JaliumThemeManager.Initialize(app);
        FluentThemeManager.Apply(app);

        try
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var darkDisplay = Resolve(app, "ControlExampleDisplayBrush");

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var lightDisplay = Resolve(app, "ControlExampleDisplayBrush");

            Assert.Equal("#FF202020", ToHex(darkDisplay));
            Assert.Equal("#FFF3F3F3", ToHex(lightDisplay));
        }
        finally
        {
            ResetApplicationState();
        }
    }

    private static Color ResolvePrimaryText(Application app) => Resolve(app, "TextFillColorPrimaryBrush");

    private static Color Resolve(Application app, string key) =>
        Assert.IsAssignableFrom<SolidColorBrush>(app.Resources[key]).Color;

    private static string ToHex(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private static void ResetApplicationState()
    {
        typeof(Application).GetField("_current", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, null);
        typeof(JaliumThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
        typeof(FluentThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
    }
}
