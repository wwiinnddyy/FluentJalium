using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The window shell: the eight ThemeResource hooks the framework's own TitleBar and TitleBarButton
/// styles consume, the two supported entry points for replacing that style, and the numbers the
/// chrome is built with. The chrome is not ours - Jalium composes it around Window.Content - so this
/// is the one place where a theme claim is made about a control no Astra file declares
/// (docs/astra/audits/window-shell.md).
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraWindowShellTests
{
    /// <summary>The framework's own light title-bar surface, which must not survive our override.</summary>
    private static readonly Color FrameworkLightSurface = Color.FromRgb(0xF5, 0xF5, 0xF7);

    private static readonly Color FrameworkDarkSurface = Color.FromRgb(0x1C, 0x1C, 0x1E);

    /// <summary>The brand green the framework paints with wherever nothing overrides it.</summary>
    private static readonly Color[] BrandEmerald =
    [
        Color.FromRgb(0x20, 0x72, 0x45),
        Color.FromRgb(0x1E, 0x79, 0x3F),
    ];

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraWindowShellTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    [Fact]
    public void The_title_bar_hooks_reach_the_chrome_the_window_builds()
    {
        // Identity, not colour equality: the chrome must paint with the same palette object the rest
        // of the tree paints with, which is what makes the theme flip and the high-contrast decision
        // reach it without a second translation layer.
        _fixture.Run(() =>
        {
            var bar = Chrome();

            Assert.Same(FluentThemeManager.GetBrush("SolidBackgroundFillColorBaseBrush"), bar.Background);
            Assert.Same(FluentThemeManager.GetBrush("TextFillColorPrimaryBrush"), bar.Foreground);

            var button = PixelHarness.Descendant<TitleBarButton>(bar)!;
            Assert.Same(FluentThemeManager.GetBrush("SubtleFillColorTransparentBrush"), button.Background);
            Assert.Same(FluentThemeManager.GetBrush("TextFillColorPrimaryBrush"), button.Foreground);
        });
    }

    [Fact]
    public void The_title_bar_surface_reaches_the_pixels_and_follows_the_theme()
    {
        // Read-back alone would also pass if the chrome were drawn by the OS rather than by this
        // runtime, so the same claim is made about rasterised output in both modes.
        _fixture.Run(() =>
        {
            var bar = Chrome();

            var light = PixelHarness.Chrome(bar);
            Assert.True(light.Stable, $"capture never settled: {light.Top(6)}");
            Assert.Equal(0, light.Count(FrameworkLightSurface));
            Assert.True(light.Count(Color.FromRgb(0xF3, 0xF3, 0xF3)) > 1_000,
                        $"our surface token did not paint the title bar; subject={light.Subject} top={light.Top(6)}");
            Assert.Equal(0, light.CountAny(BrandEmerald));

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            try
            {
                var dark = PixelHarness.Chrome(bar);
                Assert.Equal(0, dark.Count(FrameworkDarkSurface));
                Assert.True(dark.Count(Color.FromRgb(0x20, 0x20, 0x20)) > 1_000,
                            $"dark surface token did not paint the title bar; top={dark.Top(6)}");
            }
            finally
            {
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            }
        });
    }

    [Fact]
    public void The_shell_numbers_the_chrome_is_built_with_are_pinned()
    {
        // Window.TitleBarHeight and the button width come from the framework, not from us; upstream
        // states the same compact height in its own TitleBar resource dictionary, and the button
        // width is a host difference recorded in the audit rather than a parity claim.
        _fixture.Run(() =>
        {
            var window = PixelHarness.HostWindow();

            Assert.Equal(32d, window.TitleBarHeight);
            Assert.Equal(14d, window.TitleBarFontSize);
            Assert.True(window.IsShowTitleBar);
            Assert.Equal(WindowTitleBarStyle.Custom, window.TitleBarStyle);

            var bar = Chrome();
            Assert.Equal(32d, bar.Height);
            Assert.Equal(46d, PixelHarness.Descendant<TitleBarButton>(bar)!.Width);
        });
    }

    [Fact]
    public void A_backdrop_is_off_by_default_and_names_five_materials()
    {
        // The Materials page has to say what this runtime can actually ask for. Default None is why
        // the title-bar surface takes a solid token instead of a material, and no claim is made here
        // about what Mica or Acrylic would composite into - that needs a real desktop behind a window
        // (docs/astra/audits/window-shell.md, Known Gaps).
        _fixture.Run(() =>
        {
            Assert.Equal(WindowBackdropType.None, PixelHarness.HostWindow().SystemBackdrop);
            Assert.Equal(
                ["None", "Auto", "Mica", "Acrylic", "MicaAlt"],
                Enum.GetNames<WindowBackdropType>());
        });
    }

    [Fact]
    public void High_contrast_moves_the_chrome_with_the_palette()
    {
        // The chrome holds the same brush objects as the rest of the tree, so the high-contrast
        // decision made for the palette keys below the aliases needs no title-bar specific row.
        _fixture.Run(() =>
        {
            var bar = Chrome();
            FluentThemeManager.ApplyTheme(FluentThemeVariant.HighContrast);
            try
            {
                Assert.Same(FluentThemeManager.GetBrush("SolidBackgroundFillColorBaseBrush"), bar.Background);
                Assert.Equal(SystemColors.WindowColor, ((SolidColorBrush)bar.Background!).Color);
                Assert.Equal(SystemColors.WindowTextColor, ((SolidColorBrush)bar.Foreground!).Color);
            }
            finally
            {
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            }
        });
    }

    [Fact]
    public void The_supported_style_hook_reaches_the_chrome_title_bar()
    {
        // Window.CustomTitleBarStyle is public API, so a control batch that needs a different title
        // bar does not have to reach into the framework's private fields. Restored afterwards: the
        // host window is shared with every pixel test in this collection.
        _fixture.Run(() =>
        {
            var window = PixelHarness.HostWindow();
            var style = new Style(typeof(TitleBar));
            style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00))));
            window.CustomTitleBarStyle = style;
            try
            {
                var bar = Chrome();

                Assert.Same(style, bar.Style);
                Assert.Equal(Color.FromRgb(0xFF, 0xA5, 0x00), ((SolidColorBrush)bar.Background!).Color);
            }
            finally
            {
                window.CustomTitleBarStyle = null!;
            }
        });
    }

    /// <summary>The title bar the shown host window built around its content.</summary>
    private static TitleBar Chrome() =>
        PixelHarness.Descendant<TitleBar>(PixelHarness.HostWindow())
        ?? throw new InvalidOperationException("The shown window built no TitleBar.");
}
