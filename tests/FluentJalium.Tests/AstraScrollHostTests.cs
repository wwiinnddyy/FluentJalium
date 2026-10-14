using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The scroll host's measured capability envelope. Every claim here is a value read back off a native
/// ScrollViewer or a colour counted out of its pixels, because the framework builds the bar parts in
/// code and the only way to know what a style can reach is to ask the built tree
/// (docs/astra/audits/scrollviewer.md). Nothing in this file adds an entry to Application.Resources:
/// measured in the same session, that churn makes the framework re-resolve {ThemeResource} references
/// away from the palette instances and the next control's pixel claim stops holding.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraScrollHostTests
{
    private static readonly Color Sentinel = Color.FromRgb(0xFF, 0x00, 0xFF);

    /// <summary>The arrow grey the framework paints at rest, whatever the theme says.</summary>
    private static readonly Color ArrowGrey = Color.FromRgb(0xD2, 0xD2, 0xD2);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraScrollHostTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Light));
    }

    [Fact]
    public void The_implicit_style_reaches_the_scroll_host()
    {
        // Upstream's DefaultScrollViewerStyle is not a picture, it is behaviour and a transparent
        // surface. IsTabStop is the one setter that actually differs from the framework default (True),
        // and VerticalScrollBarVisibility is the one upstream setter deliberately not shipped.
        _fixture.Run(() =>
        {
            var viewer = Scroller();
            PixelHarness.Render(viewer, 200, 44);
            Assert.False(viewer.GetValue(Control.IsTabStopProperty) is true);
            Assert.Equal(new Thickness(0), viewer.GetValue(Control.PaddingProperty));
            Assert.Equal(new Thickness(0), viewer.GetValue(Control.BorderThicknessProperty));
            Assert.Equal(Colors.Transparent, Assert.IsType<SolidColorBrush>(viewer.GetValue(Control.BackgroundProperty)).Color);
            Assert.Equal(ScrollBarVisibility.Auto, viewer.VerticalScrollBarVisibility);
        });
    }

    [Fact]
    public void The_resting_scroll_bar_still_paints_through_the_scroll_host_style()
    {
        // The style does give the host a hit-testable Transparent background, so this is the guard that
        // it stayed transparent: the two resting arrow columns are still on screen.
        _fixture.Run(() =>
        {
            var sample = PixelHarness.Render(Scroller(), 200, 44);

            Assert.True(sample.Count(ArrowGrey) > 0,
                $"the arrow chrome disappeared; top={sample.Top(6)}");
        });
    }

    [Fact]
    public void An_opaque_scroll_host_background_covers_its_own_bar()
    {
        // Measured ceiling, and the reason the style must never carry a colour Background: an opaque
        // one is the only reading in the whole capture, so the bar underneath stops contributing pixels.
        _fixture.Run(() =>
        {
            var sample = PixelHarness.Render(new ScrollViewer
            {
                Content = new Border { Height = 800 },
                Background = new SolidColorBrush(Sentinel),
            }, 200, 44);

            Assert.Equal(8_800, sample.Count(Sentinel));
            Assert.Equal(0, sample.Count(ArrowGrey));
        });
    }

    [Fact]
    public void The_framework_owns_the_scroll_bar_thumb_style()
    {
        // Why the palette hook is the only thumb colour route: the bar arrives carrying its own
        // ThumbStyle, whose Background is the ScrollBarThumb resource the palette retints. Nothing here
        // installs a style, because adding and removing an entry in Application.Resources makes the
        // framework re-resolve {ThemeResource} references away from the palette instances - the
        // measurement behind audits/scrollviewer.md, which is also why the façade never touches the
        // dictionary.
        _fixture.Run(() =>
        {
            var viewer = Scroller();
            PixelHarness.Render(viewer, 200, 44);

            Assert.Equal(typeof(Thumb), PixelHarness.Descendant<ScrollBar>(viewer)!.ThumbStyle?.TargetType);
            Assert.Equal(Color.FromArgb(0x72, 0, 0, 0), Assert.IsType<SolidColorBrush>(PixelHarness.Descendant<Thumb>(viewer)!.Background).Color);
        });
    }

    [Fact]
    public void Overlay_scroll_bars_take_the_resting_arrows_away()
    {
        // The one lever that does reproduce the WinUI resting picture - nothing but the content.
        // Not shipped, because no hover evidence exists yet for what replaces it.
        _fixture.Run(() =>
        {
            var viewer = Scroller();
            viewer.IsOverlayScrollBarEnabled = true;
            var sample = PixelHarness.Render(viewer, 200, 44);

            Assert.Equal(0, sample.Count(ArrowGrey));
        });
    }

    [Fact]
    public void Auto_hide_does_not_change_the_resting_picture()
    {
        // Both settings of the framework's own auto-hide switch leave the same arrows on screen, so it
        // is not the route to the WinUI indicator states either.
        _fixture.Run(() =>
        {
            var on = Scroller();
            on.IsScrollBarAutoHideEnabled = true;
            var off = Scroller();
            off.IsScrollBarAutoHideEnabled = false;

            Assert.Equal(
                PixelHarness.Render(on, 200, 44).Count(ArrowGrey),
                PixelHarness.Render(off, 200, 44).Count(ArrowGrey));
        });
    }

    private static ScrollViewer Scroller() => new() { Content = new Border { Height = 800 } };
}
