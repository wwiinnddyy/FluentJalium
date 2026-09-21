using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// What the host's own progress bar keeps for itself and what this layer's template puts around it. The bar is the
/// first control in the library whose *geometry* is not ours: spike/ProgressProbe mode B measured that the host
/// re-sizes the template part named <c>PART_Indicator</c> to a proportion of Value, and mode C measured that no
/// declarative binding does the same job (a <c>{TemplateBinding Value}</c> width yields the value in DIP, 25 for
/// 25%). Everything the tests below can therefore claim about size is a claim about the band that geometry lands
/// in - which is exactly the thing the mode E reading forced: the host writes over an indicator's own Height.
/// </summary>
/// <remarks>
/// Nothing here is driven by a pointer: the bar takes no input, so there is no interaction arm to lose, and the
/// indeterminate state is asserted as a still picture on purpose (the animation routes this runtime does and does
/// not run are in docs/astra/audits/progress-bar.md, not claimed as pixels).
/// </remarks>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraProgressBarTests
{
    /// <summary>The framework's own brand green, which no Astra token produces (adaptation/00 S1-e).</summary>
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraProgressBarTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    [Fact]
    public void Our_band_replaces_the_hosts_own_template()
    {
        _fixture.Run(() =>
        {
            var bar = Bar(50);

            // Implicit styles never land in FrameworkElement.Style on this runtime (S0-l), so the reading is the
            // tree the template produced rather than the Style property.
            Assert.NotNull(Part(bar, "Band"));
            Assert.NotNull(Part(bar, "PART_Track"));
            Assert.NotNull(Part(bar, "PART_Indicator"));
            Assert.Equal(3d, Part(bar, "Band")!.ActualHeight, 1);
            Assert.Equal(1d, Part(bar, "PART_Track")!.ActualHeight, 1);
            Assert.Equal(new CornerRadius(1.5), bar.CornerRadius);
        });
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(25.0, 75.0)]
    [InlineData(50.0, 150.0)]
    [InlineData(75.0, 225.0)]
    [InlineData(100.0, 300.0)]
    public void The_indicator_width_is_a_proportion_of_the_row(double value, double expected)
    {
        _fixture.Run(() =>
        {
            var bar = Bar(value);
            Assert.Equal(expected, Part(bar, "PART_Indicator")!.ActualWidth, 1);
        });
    }

    [Fact]
    public void The_host_owns_the_indicator_size_so_the_band_has_to_carry_it()
    {
        _fixture.Run(() =>
        {
            var bar = Bar(50);
            var indicator = Part(bar, "PART_Indicator")!;

            // The indicator's own Height is not set by us (the band's is), because the host writes NaN/Stretch onto
            // the part - mode E watched a markup Height=4 read back as NaN and the bar fill the control's whole
            // 40 DIP box. The 3 DIP picture below is therefore the band's doing, and this is the pair that says so.
            Assert.True(double.IsNaN(indicator.Height), $"the indicator carries its own height: {indicator.Height}");
            Assert.Equal(3d, indicator.ActualHeight, 1);
            Assert.Equal(3d, Part(bar, "Band")!.Height, 1);
        });
    }

    [Fact]
    public void The_style_carries_upstreams_nine_setters()
    {
        _fixture.Run(() =>
        {
            var bar = Bar(50);

            Assert.Same(Resource("ProgressBarForeground"), bar.Foreground);
            Assert.Same(Resource("ProgressBarBackground"), bar.Background);
            Assert.Same(Resource("ProgressBarBorderBrush"), bar.BorderBrush);
            Assert.Equal(new Thickness(0), bar.BorderThickness);
            Assert.Equal(3d, bar.MinHeight, 1);
            Assert.Equal(100d, bar.Maximum, 1);
            Assert.Equal(0d, bar.Minimum, 1);
            Assert.False(bar.IsTabStop);
            Assert.Equal(VerticalAlignment.Center, bar.VerticalAlignment);
        });
    }

    [Fact]
    public void A_vertical_bar_moves_the_band_to_the_other_axis()
    {
        _fixture.Run(() =>
        {
            var bar = Bar(50, width: 60, height: 40, orientation: Jalium.UI.Controls.Orientation.Vertical);

            // Upstream's ProgressBar.idl declares no Orientation at all, so this cell is ours because the host has
            // the property, not because WinUI does (audit, section 4). The reading is the swap, not a look claim.
            Assert.Equal(3d, Part(bar, "Band")!.ActualWidth, 1);
            Assert.Equal(1d, Part(bar, "PART_Track")!.ActualWidth, 1);
            Assert.Equal(20d, Part(bar, "PART_Indicator")!.ActualHeight, 1);
        });
    }

    [Fact]
    public void The_band_size_follows_MinHeight_on_both_axes()
    {
        _fixture.Run(() =>
        {
            var bar = Bar(50);
            bar.MinHeight = 7;
            PixelHarness.Settle(6);
            Assert.Equal(7d, Part(bar, "Band")!.ActualHeight, 1);
        });

        _fixture.Run(() =>
        {
            var bar = Bar(50, width: 60, height: 40, orientation: Jalium.UI.Controls.Orientation.Vertical);
            bar.MinHeight = 7;
            PixelHarness.Settle(6);

            // The vertical cell writes Width="{TemplateBinding MinHeight}" inside a trigger setter; a setter whose
            // binding silently failed would leave the band at the control's 60 DIP width rather than 7.
            Assert.Equal(7d, Part(bar, "Band")!.ActualWidth, 1);
        });
    }

    [Fact]
    public void An_indeterminate_bar_is_a_still_picture_in_this_library()
    {
        _fixture.Run(() =>
        {
            var bar = Bar(50);
            bar.IsIndeterminate = true;
            PixelHarness.Settle(8);
            var indicator = Part(bar, "PART_Indicator")!;
            var first = indicator.ActualWidth;
            PixelHarness.Settle(40);
            var second = indicator.ActualWidth;

            Assert.True(first > 0 && first < bar.Width, $"the indeterminate bar filled nothing or everything: {first}");
            Assert.Equal(first, second, 1);
        });
    }

    [Fact]
    public void The_row_upstream_names_a_colour_for_is_a_brush_here()
    {
        _fixture.Run(() =>
        {
            // ProgressBar_themeresources.xaml:9 points the rail at ControlStrongStrokeColorDefault, a Color row in
            // this palette. A Color that reaches a Brush property paints nothing at all - probe mode alias read
            // Background back as null for exactly that shape and as the brush for its twin - so the row keeps
            // upstream's name and takes the brush target. This assertion is the substitution's witness.
            Assert.Same(Resource("ControlStrongStrokeColorDefaultBrush"), Resource("ProgressBarBackground"));
            Assert.IsNotType<Color>(Resource("ProgressBarBackground")!);
        });
    }

    [Fact]
    public void The_band_paints_across_the_row_and_never_the_whole_box()
    {
        _fixture.Run(() =>
        {
            var accent = ((SolidColorBrush)Resource("AccentFillColorDefaultBrush")!).Color;
            var sample = PixelHarness.Host(Card(Bar(50, build: false)), 300, 40);
            var filled = sample.Count(accent);

            // The arithmetic that makes this a claim rather than a glance: 50% of a 300 DIP row, three DIP tall,
            // is around 450 inked pixels. The framework's own template, which fills the control's full height,
            // measured 5 704 of them for the same subject (probe mode B).
            Assert.True(filled > 200, $"the bar painted no accent at all: {sample.Top(4)}");
            Assert.True(filled < 1500,
                $"the accent filled {filled} pixels of a 300x40 box, which is a block rather than a 3 DIP band: {sample.Top(4)}");
            Assert.Equal(0, sample.CountAny(BrandEmerald));
        });
    }

    [Fact]
    public void Light_and_dark_hand_over_different_rail_brushes()
    {
        Color? lightRail = null;
        Color? lightAccent = null;
        string? lightTop = null;

        _fixture.Run(() =>
        {
            lightRail = ((SolidColorBrush)Resource("ProgressBarBackground")!).Color;
            lightAccent = ((SolidColorBrush)Resource("ProgressBarForeground")!).Color;
            lightTop = PixelHarness.Host(Card(Bar(50, build: false)), 300, 40).Top(4);
        });

        _fixture.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark));

        try
        {
            _fixture.Run(() =>
            {
                FluentThemeManager.ApplyAccent(null);
                var darkRail = ((SolidColorBrush)Resource("ProgressBarBackground")!).Color;
                var darkAccent = ((SolidColorBrush)Resource("ProgressBarForeground")!).Color;

                // Both rows are aliases, so the theme test is on their targets: the rail is black at 44% alpha in
                // Light and white at 55% in Dark, and the accent moves with the palette.
                Assert.NotEqual(lightRail, darkRail);
                Assert.NotEqual(lightAccent, darkAccent);

                var dark = PixelHarness.Host(Card(Bar(50, build: false)), 300, 40);
                Assert.True(dark.Count(darkAccent) > 200, $"the dark bar painted no accent: {dark.Top(4)}");
                Assert.NotEqual(lightTop, dark.Top(4));
            });
        }
        finally
        {
            _fixture.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Light));
        }
    }

    [Theory]
    [InlineData("ProgressBarForeground")]
    [InlineData("ProgressBarBackground")]
    [InlineData("ProgressBarBorderBrush")]
    [InlineData("ProgressBarBorderThemeThickness")]
    [InlineData("ProgressBarCornerRadius")]
    [InlineData("ProgressBarTrackCornerRadius")]
    public void Published_rows_resolve_to_a_resource(string key)
    {
        _fixture.Run(() => Assert.NotNull(Resource(key)));
    }

    [Theory]
    [InlineData("ProgressBarMinHeight")]
    [InlineData("ProgressBarTrackHeight")]
    [InlineData("ProgressBarErrorForegroundColor")]
    [InlineData("ProgressBarPausedForegroundColor")]
    [InlineData("ProgressRingStrokeThickness")]
    [InlineData("ProgressBarThemeMinHeight")]
    [InlineData("ProgressBarIndicatorPauseOpacity")]
    [InlineData("ProgressBarBackgroundThemeBrush")]
    [InlineData("ProgressBarForegroundThemeBrush")]
    [InlineData("ProgressBarIndeterminateForegroundThemeBrush")]
    public void Rows_this_runtime_cannot_carry_stay_out_of_the_public_set(string key)
    {
        // The first four are upstream's own and are held for two different reasons: the two x:Double metric rows
        // this reader cannot parse at all, and the two foreground rows whose ShowError/ShowPaused drivers the host
        // type does not have. ProgressRingStrokeThickness is upstream-dead (no consumer in the reference tree). The
        // rest are the deprecated CommonStyles names ModernWpf still publishes; this layer publishes no deprecated
        // ProgressBar row, which is what the audit's key census records.
        _fixture.Run(() => Assert.Null(Resource(key)));
    }

    [Theory]
    [InlineData("Band")]
    [InlineData("PART_Track")]
    [InlineData("PART_Indicator")]
    [InlineData("ProgressBarRoot")]
    [InlineData("LayoutRoot")]
    public void The_template_names_the_parts_upstream_names_them(string name)
    {
        _fixture.Run(() => Assert.NotNull(Part(Bar(50), name)));
    }

    private static ProgressBar Bar(
        double value,
        double width = 300,
        double height = 40,
        Jalium.UI.Controls.Orientation orientation = Jalium.UI.Controls.Orientation.Horizontal,
        bool build = true)
    {
        var bar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Value = value,
            Orientation = orientation,
            Width = width,
            Height = height,
        };

        // Build() and Host() are two different parents (adaptation/00 S1-b), so a bar that is about to be hosted is
        // left unattached.
        if (build)
        {
            PixelHarness.Build(bar, (int)width, (int)height);
        }

        return bar;
    }

    private static Border Card(FrameworkElement child) => new()
    {
        // The host window's own backdrop is black, and a translucent rail on black reads as no ink at all; a lit
        // card is what lets the same picture be counted (S1-m 6).
        Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
        Width = 300,
        Height = 40,
        Child = child,
    };

    private static FrameworkElement? Part(FrameworkElement root, string name) => PixelHarness.Named(root, name);

    private static object? Resource(string key) => Application.Current?.TryFindResource(key);
}
