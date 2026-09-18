using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The pixel leg of the stage-1 base: the only evidence in this repository that a theme mutation
/// reached rendered output rather than only a resource dictionary. Everything runs on the
/// collection fixture's own thread because Jalium objects carry thread affinity, and a templated
/// control is captured through a window because that is the only way one rasterises.
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
        // The claim stage 1 can actually make without any control being styled: an opaque palette
        // brush is the object a visual paints with, so the token's own colour shows up, and the
        // in-place retint moves it. Deliberately a Border rather than a Button - see the note on the
        // test below about native controls not showing our tokens yet.
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
    public void The_theme_driver_moves_a_shown_window_to_the_pixels()
    {
        // What this actually measures, from the raw captures in
        // docs/astra/adaptation/00-pixel-harness-raw-output.txt: a shown window paints 9680 px of
        // #F5F5F7 in Light and #1C1C1E in Dark. Neither value is in the Astra palette - it is Jalium's
        // own surface following Application.ThemeMode. So this is evidence the driver reaches
        // rendered output, and no more: the Button in the client area contributes nothing
        // distinguishable here, because a native control captured on its own rasterises empty.
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

    [Fact(Skip = "Ceiling, not a regression, measured 2026-09-18: an unstyled native Slider paints #207245 across 954 px and a native ProgressBar paints #1D733C-#2B804A, because Jalium ships no generic theme and both draw themselves from the frozen ThemeColors table (docs/astra/adaptation/02-render-ceiling.md). Enable when the Slider and ProgressBar batches re-template them; the claim is that a hosted control shows no brand emerald at all.")]
    public void Hosted_surfaces_show_no_brand_emerald()
    {
        _fixture.Run(() =>
        {
            foreach (FrameworkElement subject in new FrameworkElement[] { new Slider { Value = 50 }, new ProgressBar { Value = 50 } })
            {
                var sample = PixelHarness.Host(subject);
                Assert.True(sample.CountAny(BrandEmerald) == 0, $"{subject.GetType().Name} paints brand emerald {sample.CountAny(BrandEmerald)} px; top={sample.Top(6)}");
            }
        });
    }

    [Fact(Skip = "Known ceiling: the CheckBox check mark is a frozen framework brush that survives both ApplyAccent and a Background/Foreground override (docs/astra/adaptation/02-render-ceiling.md, roadmap item A4). Re-enable when the template-override experiment settles whether a pure template can replace the glyph.")]
    public void Check_mark_follows_the_selected_accent()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyAccent(Sentinel);
            var sample = PixelHarness.Render(new CheckBox { IsChecked = true }, 40, 40);
            Assert.True(sample.Count(Sentinel) > 0, $"check mark ignored the accent; top={sample.Top(6)}");
        });
    }
}
