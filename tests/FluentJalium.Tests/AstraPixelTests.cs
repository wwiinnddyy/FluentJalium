using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The pixel leg of the theme base: the only evidence in this repository that a theme mutation
/// reached rendered output rather than only a resource dictionary. Everything runs on the
/// collection fixture's own thread because Jalium objects carry thread affinity, and a control is
/// captured through a shown window with real frames pumped, because nothing rasterises otherwise
/// (docs/astra/adaptation/06-pixel-attribution.md).
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraPixelTests
{
    private static readonly Color Sentinel = Color.FromRgb(0xFF, 0x00, 0xFF);

    /// <summary>The brand green the framework paints with wherever nothing overrides it.</summary>
    private static readonly Color[] BrandEmerald =
    [
        Color.FromRgb(0x20, 0x72, 0x45),
        Color.FromRgb(0x1E, 0x79, 0x3F),
    ];

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraPixelTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    [Fact]
    public void A_brush_dependency_property_reaches_the_rendered_pixels()
    {
        // Without this, an empty capture below could be read as "the theme won". A Border needs no
        // style, so this measures the capture path itself and none of our work.
        _fixture.Run(() =>
        {
            var sample = PixelHarness.Render(new Border { Background = new SolidColorBrush(Sentinel) }, 40, 40);

            Assert.Equal(1_600, sample.Count(Sentinel));
        });
    }

    [Fact]
    public void An_astra_token_reaches_the_pixels_and_follows_the_theme()
    {
        // The palette brush is the object a visual paints with, so the token's own colour shows up,
        // and the in-place retint moves it.
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var token = (SolidColorBrush)FluentThemeManager.GetBrush("SolidBackgroundFillColorBaseBrush");
            Assert.Equal(byte.MaxValue, token.Color.A);
            var light = PixelHarness.Render(new Border { Background = token }, 40, 40);
            Assert.Equal(1_600, light.Count(token.Color));

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var dark = PixelHarness.Render(new Border { Background = token }, 40, 40);
            Assert.Equal(1_600, dark.Count(token.Color));
            Assert.Equal(Color.FromRgb(0x20, 0x20, 0x20), token.Color);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
        });
    }

    [Fact]
    public void The_implicit_astra_style_reaches_a_native_button()
    {
        // The claim stage 2 was blocked on, and the one every later control batch inherits: a plain
        // native Button with nothing set on it picks up Astra's application-level implicit style,
        // that style's template consumes the palette brush, and the brush colour is what the
        // rasteriser wrote. A local Background outranks the style setter, which is the second half of
        // the same evidence.
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", Sentinel);
            try
            {
                var styled = PixelHarness.Render(new Button { Content = "astra" }, 200, 44);
                Assert.True(styled.Stable, $"capture never settled: {styled.Top(6)}");
                Assert.True(styled.Count(Sentinel) > 6_000, $"implicit style did not paint the button; subject={styled.Subject} top={styled.Top(6)}");
                Assert.Equal(0, styled.CountAny(BrandEmerald));

                var local = PixelHarness.Render(new Button { Content = "astra", Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00)) }, 200, 44);
                Assert.True(local.Count(Color.FromRgb(0xFF, 0xA5, 0x00)) > 6_000, $"local Background did not reach the template; top={local.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", null);
            }
        });
    }

    [Fact]
    public void The_accent_token_reaches_an_explicit_button_style()
    {
        // Named styles resolve their {ThemeResource} setters the same way the implicit one does, so a
        // Gallery page that asks for AccentButtonStyle by name is covered by the same kernel.
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", Sentinel);
            try
            {
                var sample = PixelHarness.Render(new Button { Content = "accent", Style = FluentThemeManager.GetStyle("AccentButtonStyle") }, 200, 44);
                Assert.True(sample.Stable, $"capture never settled: {sample.Top(6)}");
                Assert.True(sample.Count(Sentinel) > 6_000, $"accent token did not paint the button; top={sample.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            }
        });
    }

    [Fact]
    public void The_theme_driver_moves_a_shown_window_to_the_pixels()
    {
        // A shown window repaints across the theme flip. Counts are monitor-scale, so this asserts
        // the picture changed rather than a pixel total.
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var light = PixelHarness.Host(new Button { Content = "astra" });

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var dark = PixelHarness.Host(new Button { Content = "astra" });

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.True(light.PaintedPixels > 0, $"light capture is empty: {light.Top(6)}");
            Assert.True(dark.PaintedPixels > 0, $"dark capture is empty: {dark.Top(6)}");
            Assert.NotEqual(light.Top(2), dark.Top(2));
        });
    }

    [Fact]
    public void Hosted_surfaces_show_no_brand_emerald()
    {
        // Retrained claim: the 954 px of #207245 recorded for an unstyled Slider in
        // docs/astra/adaptation/02-render-ceiling.md came from the old un-framed capture and does not
        // reproduce. These two controls still have no Astra style, so what is on screen is the
        // framework's own default, and the default follows the driver rather than the frozen brand.
        // The IL reading that the drawing code consults ThemeColors is untouched by this.
        _fixture.Run(() =>
        {
            foreach (FrameworkElement subject in new FrameworkElement[] { new Slider { Value = 50 }, new ProgressBar { Value = 50 } })
            {
                var sample = PixelHarness.Render(subject, 220, 40);
                // Guard against the trivial pass: an unpainted capture has no emerald either.
                Assert.True(sample.PaintedPixels > 0, $"{subject.GetType().Name} captured empty; top={sample.Top(6)}");
                Assert.Equal(0, sample.CountAny(BrandEmerald));
            }
        });
    }

    [Fact]
    public void Check_mark_follows_the_selected_accent()
    {
        // Roadmap item A4 was recorded as a frozen-brush ceiling from a capture that never rendered a
        // frame. With the harness fixed the same subject does follow the accent, so the ceiling is
        // withdrawn for the check mark; it stays open for anything Astra has not templated yet.
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyAccent(Sentinel);
            var sample = PixelHarness.Render(new CheckBox { IsChecked = true }, 44, 44);
            Assert.True(sample.Count(Sentinel) > 0, $"check mark ignored the accent; top={sample.Top(6)}");
        });
    }

    /// <summary>
    /// A scroll bar only exists inside a scroll host, and the content has to be taller than the
    /// viewport for the vertical one to appear. A standalone ScrollBar is not an option: it stops the
    /// frame loop from returning (docs/astra/adaptation/07-scroll-host-substitution.md).
    /// </summary>
    private static ScrollViewer Scroller() => new()
    {
        VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
        Content = new Border { Height = 800 },
    };

    [Fact]
    public void The_scroll_bar_matches_the_upstream_size_and_thumb_width()
    {
        // WinUI fixes the bar at ScrollBarSize=12 and the vertical thumb at
        // ScrollBarVerticalThumbMinWidth=8. The runtime already builds those, so the metric is a
        // claim to hold rather than a value to set, and a future framework default that drifts
        // fails here.
        _fixture.Run(() =>
        {
            Assert.Equal(12, PixelHarness.RenderPart<ScrollBar>(Scroller(), 200, 44).Width);
            Assert.Equal(8, PixelHarness.RenderPart<Thumb>(Scroller(), 200, 44).Width);
        });
    }

    [Fact]
    public void The_scrollbar_thumb_hook_paints_the_thumb()
    {
        // 26.10.9 builds the scroll bar's parts in code: ControlTemplate and every implicit style on
        // ScrollBar, RepeatButton, Thumb and ScrollViewer itself were measured inert. The Thumb's own
        // border paints with the resource named ScrollBarThumb, which is the only colour route the
        // runtime leaves open and the reason that key is a palette brush.
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("ScrollBarThumb", Sentinel);
            try
            {
                var sample = PixelHarness.RenderPart<Thumb>(Scroller(), 200, 44);
                Assert.True(sample.Count(Sentinel) > 48,
                    $"ScrollBarThumb did not paint the thumb; subject={sample.Subject} top={sample.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("ScrollBarThumb", null);
            }
        });
    }

    [Fact]
    public void The_scrollbar_track_hook_paints_the_track()
    {
        // ScrollBarTrack covers the bar behind the thumb; the thumb hook is left at its palette value
        // so the two claims stay separate.
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("ScrollBarTrack", Sentinel);
            try
            {
                var sample = PixelHarness.RenderPart<ScrollBar>(Scroller(), 200, 44);
                Assert.True(sample.Count(Sentinel) > 48,
                    $"ScrollBarTrack did not paint the bar; subject={sample.Subject} top={sample.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("ScrollBarTrack", null);
            }
        });
    }

    [Fact]
    public void The_scroll_bar_thumb_follows_the_theme_and_shows_no_brand_emerald()
    {
        // The palette holds ScrollBarThumb as #72000000 for Light and #8BFFFFFF for Dark, and the
        // capture keeps the colour channels, so the dark branch has to show white pixels where the
        // thumb is and the light branch must not leak them. The whole bar is captured rather than the
        // thumb because a light-theme thumb is black, which an unpainted surface also reads as.
        _fixture.Run(() =>
        {
            var light = PixelHarness.RenderPart<ScrollBar>(Scroller(), 200, 44);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var dark = PixelHarness.RenderPart<ScrollBar>(Scroller(), 200, 44);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.Equal(0, light.CountAny(BrandEmerald));
            Assert.True(dark.Count(Colors.White) > 48,
                $"dark thumb brush did not reach the bar; top={dark.Top(6)}");
            Assert.Equal(0, light.Count(Colors.White));
        });
    }

    /// <summary>
    /// ToolTip is the control whose template root is the shared flyout surface and that can still be
    /// captured in-process. ComboBox and MenuFlyout hang the same chrome under a Popup, which lives in
    /// its own visual root the harness cannot reach, so this is where the flyout claim is tested.
    /// </summary>
    private static ToolTip FlyoutSurface() => new() { Content = "flyout" };

    [Fact]
    public void The_flyout_surface_token_paints_the_shared_popup_chrome()
    {
        // Overriding the palette brush under upstream's own acrylic name is the claim that
        // FlyoutPresenterBackground resolves to that object rather than to a copy frozen at parse time.
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("AcrylicInAppFillColorDefaultBrush", Sentinel);
            try
            {
                var sample = PixelHarness.Render(FlyoutSurface(), 200, 40);
                Assert.True(sample.Stable, $"capture never settled: {sample.Top(6)}");
                Assert.True(sample.Count(Sentinel) > 6_000,
                    $"flyout surface did not paint the popup chrome; subject={sample.Subject} top={sample.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("AcrylicInAppFillColorDefaultBrush", null);
            }
        });
    }

    [Fact]
    public void The_flyout_surface_follows_the_theme_and_shows_no_brand_emerald()
    {
        // The same chrome read through the palette instead of a sentinel: WinUI's acrylic fallback
        // values, which is what the popup shows on a runtime that cannot composite the material.
        _fixture.Run(() =>
        {
            var light = PixelHarness.Render(FlyoutSurface(), 200, 40);
            Assert.True(light.Count(Color.FromRgb(0xF9, 0xF9, 0xF9)) > 6_000,
                $"light flyout surface missing; subject={light.Subject} top={light.Top(6)}");

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var dark = PixelHarness.Render(FlyoutSurface(), 200, 40);
            Assert.True(dark.Count(Color.FromRgb(0x2C, 0x2C, 0x2C)) > 6_000,
                $"dark flyout surface missing; subject={dark.Subject} top={dark.Top(6)}");
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.Equal(0, light.CountAny(BrandEmerald));
            Assert.Equal(0, dark.CountAny(BrandEmerald));
        });
    }
}
