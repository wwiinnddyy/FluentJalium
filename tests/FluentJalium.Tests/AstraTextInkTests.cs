using FluentJalium.Controls;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// A pinned instrument limit, not a passing claim about the product: on Jalium.UI 26.10.9 a text run reaches no
/// pixel in any capture path this suite has, while geometry and surfaces do. Every leg is differential - the same
/// subject is captured twice, once with its text and once without - so the reading cannot be produced by anything
/// else in the crop: no surface, border, chevron or chrome contributes to the difference because it is in both
/// captures.
/// </summary>
/// <remarks>
/// Measured 2026-09-21 (<c>docs/astra/adaptation/06-pixel-attribution.md</c>, text-ink section): a
/// <c>Button { Content = "Hello World" }</c> and a <c>Button { Content = "" }</c> at 200x44 give byte-identical
/// histograms on the white page, the dark page and bare, in both themes (light: surface 8296 + border 456 + 12 + 8,
/// identical with and without the string). A menu item's 38 pixels are its submenu chevron - the same 38 with the
/// text removed - and they are the positive control: opaque geometry ink arrives and flips with the theme, so the
/// capture is not globally blind. A detached <c>TextBlock</c> additionally never settles (<c>Stable == false</c>,
/// all black), which is why the plated legs below are the route that carries the claim.
/// <para>
/// <b>What to do when this file goes red:</b> text started printing. Delete the pins and turn the real per-control
/// text claims on (the fore-<i>ground</i> tokens those claims were standing in for stay asserted on the tree, so
/// nothing else in the suite changes).
/// </para>
/// </remarks>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraTextInkTests : IDisposable
{
    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraTextInkTests(AstraThemeRuntimeFixture fixture) => _fixture = fixture;

    public void Dispose() => _fixture.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Light));

    [Theory]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.Dark)]
    public void A_buttons_text_owes_it_no_pixels(FluentThemeVariant variant)
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(variant);
            var plate = variant == FluentThemeVariant.Light ? PixelHarness.LightPage : PixelHarness.DarkPage;

            var withText = OnPlate(() => new Button { Content = "Hello World", Width = 200, Height = 44 }, plate, 200, 44);
            var withoutText = OnPlate(() => new Button { Content = "", Width = 200, Height = 44 }, plate, 200, 44);

            // The control has to paint, or "no difference" would only mean a dead capture.
            Assert.True(withText.Count(PixelHarness.Over(plate, SurfaceColor)) > 2_000,
                $"the button's own surface did not reach the page, so the text comparison proves nothing: {withText.Top(4)}");
            Assert.Equal(withText.Top(6), withoutText.Top(6));
            Assert.Equal(withText.DistinctColors, withoutText.DistinctColors);
        });
    }

    [Theory]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.Dark)]
    public void A_menu_items_ink_is_its_chevron_and_not_its_text(FluentThemeVariant variant)
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(variant);
            var plate = variant == FluentThemeVariant.Light ? Color.FromRgb(0xFF, 0x00, 0xFF) : PixelHarness.DarkPage;

            var withText = OnPlate(() => new MenuFlyoutSubItem { Text = "sub" }, plate, 240, 38);
            var withoutText = OnPlate(() => new MenuFlyoutSubItem { Text = "" }, plate, 240, 38);

            var offPlate = CountOther(withText, plate);

            // Positive control: opaque geometry ink reaches the page and survives a magenta backdrop unchanged,
            // so a zero here would mean the capture path is dead rather than that text does not print.
            Assert.True(offPlate is > 0 and < 200,
                $"the submenu arrow owes {offPlate} pixels; expected a few dozen, and the same in both themes: {withText.Top(5)}");
            Assert.Equal(withText.Top(6), withoutText.Top(6));
        });
    }

    [Theory]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.Dark)]
    public void A_detached_text_run_prints_nothing_on_a_page(FluentThemeVariant variant)
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(variant);
            var plate = variant == FluentThemeVariant.Light ? Color.FromRgb(0xFF, 0x00, 0xFF) : Color.FromRgb(0x00, 0x80, 0x00);

            var sample = OnPlate(
                () => new TextBlock { Text = "Hello World 0123456789", FontSize = 24, Width = 320, Height = 40 },
                plate, 320, 40);

            Assert.True(sample.Stable, $"the capture never settled: {sample.Top(4)}");
            Assert.Equal(0, CountOther(sample, plate));
        });
    }

    private static Color SurfaceColor =>
        Assert.IsType<SolidColorBrush>(Application.Current!.TryFindResource("ButtonBackground")!).Color;

    private static PixelHarness.Sample OnPlate(Func<FrameworkElement> subject, Color plate, int width, int height)
    {
        var host = PixelHarness.Backdrop(subject(), plate);
        PixelHarness.Build(host, width, height);
        PixelHarness.Settle(60);
        return PixelHarness.Render(host, width, height);
    }

    private static int CountOther(PixelHarness.Sample sample, Color exclude)
    {
        var key = (uint)(exclude.R << 16 | exclude.G << 8 | exclude.B);
        return sample.Histogram.Where(entry => entry.Key != key).Sum(entry => entry.Value);
    }
}
