using FluentJalium.Controls;
using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// What the ring owns and what its arc has to prove. Upstream plays this control out of a Lottie asset through an
/// <c>AnimatedVisualPlayer</c>; 26.10.9 has neither, so the whole look is geometry this layer computes (the four
/// dead ends that ruled the alternatives out are recorded in <c>FluentProgressRing</c>'s remarks and in
/// <c>docs/astra/audits/progress-ring.md</c>). The assertions therefore aim at two things the probe could not fake:
/// that a value really moves the drawn arc, and that the drawn arc really reaches the picture.
/// </summary>
/// <remarks>
/// Nothing is driven by a pointer or a keyboard - upstream sets <c>IsHitTestVisible=False</c> and
/// <c>IsTabStop=False</c>, and both are asserted below, so there is no input arm for these tests to lose. The spin
/// is asserted through the angle it writes, not through a screenshot of a moving arc, because a capture sees one
/// instant of it.
/// </remarks>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraProgressRingTests
{
    /// <summary>The framework's own brand green, which no Astra token produces (adaptation/00 S1-e).</summary>
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraProgressRingTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    [Fact]
    public void The_ring_carries_upstreams_twelve_setters()
    {
        _fixture.Run(() =>
        {
            var ring = Ring(50, indeterminate: false);

            // ProgressRing.xaml:4-14, in order. Every one of them is a property the style can reach, unlike the
            // StrokeThickness upstream leaves to the asset - which is why the arc's stroke is asserted separately.
            Assert.Equal(32d, ring.Width, 1);
            Assert.Equal(32d, ring.Height, 1);
            Assert.Equal(16d, ring.MinWidth, 1);
            Assert.Equal(16d, ring.MinHeight, 1);
            Assert.Equal(100d, ring.Maximum, 1);
            Assert.Equal(0d, ring.Minimum, 1);
            Assert.False(ring.IsTabStop);
            Assert.False(ring.IsHitTestVisible);
            Assert.Equal(HorizontalAlignment.Center, ring.HorizontalAlignment);
            Assert.Equal(VerticalAlignment.Center, ring.VerticalAlignment);
            Assert.NotNull(ring.Foreground);
            Assert.NotNull(ring.Background);
        });
    }

    [Fact]
    public void The_foreground_and_background_rows_are_upstreams_aliases()
    {
        _fixture.Run(() =>
        {
            // Upstream's two rows are themselves aliases of aliases (ProgressRing_themeresources.xaml:5-6), so the
            // honest claim is the target pair rather than a colour: the ring paints the accent on a clear plate.
            Assert.Same(Resource("AccentFillColorDefaultBrush"), Resource("ProgressRingForegroundThemeBrush"));
            Assert.Same(Resource("ControlFillColorTransparentBrush"), Resource("ProgressRingBackgroundThemeBrush"));
            Assert.IsNotType<Color>(Resource("ProgressRingForegroundThemeBrush")!);
        });
    }

    [Fact]
    public void The_template_lays_out_a_root_and_an_arc_whose_geometry_is_real()
    {
        _fixture.Run(() =>
        {
            var ring = Ring(50, indeterminate: false);

            // LayoutRoot is upstream's only state target (ProgressRing.xaml:18). The arc replaces its
            // AnimatedVisualPlayer, and the reading that separates this from a silently empty template is the
            // figure count: markup-authored geometry reports zero figures here (RingProbe mode E5), so a Data that
            // carries one figure and one segment is proof the control wrote it.
            Assert.NotNull(Part(ring, "LayoutRoot"));
            var arc = Assert.IsType<Jalium.UI.Shapes.Path>(Part(ring, "ProgressRingArc"));
            var geometry = Assert.IsType<PathGeometry>(arc.Data);
            var figure = Assert.Single(geometry.Figures);
            Assert.IsType<ArcSegment>(Assert.Single(figure.Segments));
        });
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(25.0, 90.0)]
    [InlineData(50.0, 180.0)]
    [InlineData(75.0, 270.0)]
    [InlineData(100.0, 359.9)]
    public void The_arc_sweep_is_a_proportion_of_the_range(double value, double expected)
    {
        _fixture.Run(() =>
        {
            var ring = Ring(value, indeterminate: false);
            Assert.Equal(expected, ring.CurrentSweepAngle, 1);
        });
    }

    [Theory]
    [InlineData(25.0, false)]
    [InlineData(75.0, true)]
    public void A_sweep_past_half_the_circle_declares_itself_the_large_arc(double value, bool expected)
    {
        _fixture.Run(() =>
        {
            // An arc's flag is what tells the rasteriser which of the two possible arcs to draw; a sweep of 270
            // drawn with the flag clear would come back as the 90 it is not.
            var ring = Ring(value, indeterminate: false);
            Assert.Equal(expected, Segment(ring).IsLargeArc);
        });
    }

    [Fact]
    public void The_range_remaps_the_same_value()
    {
        _fixture.Run(() =>
        {
            var ring = Ring(50, indeterminate: false);
            ring.Maximum = 200;
            PixelHarness.Settle(4);
            Assert.Equal(90d, ring.CurrentSweepAngle, 1);

            ring.Minimum = 40;
            PixelHarness.Settle(4);

            // (50 - 40) / (200 - 40) is a sixteenth of the circle, not an eighth: the range is what the value is
            // divided by, and both ends moved.
            Assert.Equal(22.5d, ring.CurrentSweepAngle, 1);
        });
    }

    [Fact]
    public void A_degenerate_range_draws_nothing_rather_than_dividing_by_zero()
    {
        _fixture.Run(() =>
        {
            var ring = Ring(50, indeterminate: false);
            ring.Maximum = ring.Minimum;
            PixelHarness.Settle(4);
            Assert.Equal(0d, ring.CurrentSweepAngle, 1);

            // The guard is against a cap dot: a zero-length arc still inks two accent pixels where it starts, so
            // 0 % hands back a geometry with nothing in it.
            Assert.Empty(Assert.IsType<PathGeometry>(Arc(ring)!.Data).Figures);
        });
    }

    [Fact]
    public void An_indeterminate_ring_is_half_a_circle_and_ignores_the_value()
    {
        _fixture.Run(() =>
        {
            // The asset's TrimEnd stops at 0.5 and its determinate playback is what Value drives, so a value write
            // while indeterminate must not touch the arc.
            var ring = Ring(10);
            Assert.Equal(180d, ring.CurrentSweepAngle, 1);
            ring.Value = 90;
            PixelHarness.Settle(4);
            Assert.Equal(180d, ring.CurrentSweepAngle, 1);
        });
    }

    [Fact]
    public void The_arc_measures_the_radius_and_stroke_the_asset_carries()
    {
        _fixture.Run(() =>
        {
            var ring = Ring(50, indeterminate: false);

            // Determinate: radius 8 and stroke 1.5 on a 32 unit canvas at shape scale 1.77, i.e. 0.4425 and 0.0830
            // of the side. At the authored 32 DIP box that is 14.16 and 2.66.
            Assert.Equal(32 * 1.5 * 1.77 / 32, Arc(ring)!.StrokeThickness, 2);

            // Bounds are read off a closed ring on purpose: the geometry box of a 180° arc is a radius wide and a
            // diameter tall, which is a true statement about arcs and a useless one about this factor.
            var closed = Ring(100, indeterminate: false);
            Assert.Equal(2 * (32 * 8 * 1.77 / 32), Arc(closed)!.Data!.Bounds.Width, 1);
        });

        _fixture.Run(() =>
        {
            var ring = Ring(50);

            // Indeterminate is a different asset: radius 7 and stroke 1.5 on an 80 unit canvas at scale 5, i.e.
            // 0.4375 and 0.09375. ModernWpf draws both states from the determinate numbers; this one does not.
            Assert.Equal(32 * 1.5 * 5 / 80, Arc(ring)!.StrokeThickness, 2);
        });
    }

    [Fact]
    public void A_bigger_box_scales_the_arc()
    {
        _fixture.Run(() =>
        {
            var ring = Ring(100, size: 64, indeterminate: false);
            Assert.Equal(64 * 1.5 * 1.77 / 32, Arc(ring)!.StrokeThickness, 2);
            Assert.Equal(64 * 8 * 1.77 / 32 * 2, Arc(ring)!.Data!.Bounds.Width, 1);
        });
    }

    [Fact]
    public void The_spin_moves_the_arc_and_stops_with_the_ring()
    {
        _fixture.Run(() =>
        {
            // A frame loop's evidence is a difference between two reads, never a threshold on one: how much time
            // elapses between two settles is not fixed, so the only stable claim is "it moved while running and it
            // did not while stopped". The two shapes carry different preconditions, and the rates measured here say
            // why: while the ring spins the pump delivers 144 frames inside one settle budget, so a movement claim
            // can simply demand elapsed render time and get it; once the loop stops the scene is static, the
            // framework stops raising Rendering, and asking a stopped ring for 24 frames delivers 19 before the
            // budget expires. A floor on the stationary legs would therefore be a floor on a state that is correct
            // by design - those two legs report the count instead of requiring one.
            var ring = Ring(50);
            var before = Start(ring);
            SettleMoved("the spin leg");
            Assert.NotEqual(before, Start(ring));

            ring.IsIndeterminate = false;
            PixelHarness.Settle(8);

            // The determinate arc is anchored at 12 o'clock, and switching to it zeroes the spin.
            Assert.Equal(16d, Start(ring).X, 1);
            var anchored = Start(ring);
            var heldStill = PixelHarness.Settle(24);
            Assert.True(anchored == Start(ring),
                $"the determinate arc moved while nothing should animate it (frames delivered while it held: {heldStill})");

            ring.IsIndeterminate = true;
            PixelHarness.Settle(8);
            var restarted = Start(ring);
            SettleMoved("the restart leg");
            Assert.NotEqual(restarted, Start(ring));

            // Going inactive stops the loop rather than rewinding it, which is what upstream's Stop() on the
            // player does.
            ring.IsActive = false;
            PixelHarness.Settle(6);
            var held = Start(ring);
            var heldInactive = PixelHarness.Settle(24);
            Assert.True(held == Start(ring),
                $"the inactive ring's arc moved after the loop stopped (frames delivered while it held: {heldInactive})");
        });
    }

    [Fact]
    public void An_inactive_ring_puts_its_root_out_of_the_picture()
    {
        _fixture.Run(() =>
        {
            var ring = Ring(50, indeterminate: false);
            Assert.Equal(1d, Part(ring, "LayoutRoot")!.Opacity, 2);

            ring.IsActive = false;
            PixelHarness.Settle(6);

            // The Inactive state upstream writes (ProgressRing.xaml:21-26) is one opacity setter and an
            // AccessibilityView this runtime has no member for.
            Assert.Equal(0d, Part(ring, "LayoutRoot")!.Opacity, 2);
        });
    }

    [Fact]
    public void A_half_ring_prints_about_half_of_what_a_whole_one_does()
    {
        _fixture.Run(() =>
        {
            var accent = ((SolidColorBrush)Resource("AccentFillColorDefaultBrush")!).Color;
            var half = PixelHarness.Host(Card(Ring(50, indeterminate: false, build: false)), 60, 60);
            var whole = PixelHarness.Host(Card(Ring(100, indeterminate: false, build: false)), 60, 60);
            var halfCount = half.Count(accent);
            var wholeCount = whole.Count(accent);

            // The guard against the trivial pass: a capture that inked nothing would divide by zero into a pass.
            Assert.True(halfCount > 40, $"the determinate ring inked nothing: {half.Top(4)}");
            Assert.True(wholeCount > halfCount * 1.4,
                $"50 % inked {halfCount} and 100 % inked {wholeCount}: the value never reached the arc");
            Assert.Equal(0, half.CountAny(BrandEmerald));
        });
    }

    [Fact]
    public void An_empty_value_prints_no_accent_and_an_inactive_one_prints_nothing()
    {
        _fixture.Run(() =>
        {
            var accent = ((SolidColorBrush)Resource("AccentFillColorDefaultBrush")!).Color;
            var empty = PixelHarness.Host(Card(Ring(0, indeterminate: false, build: false)), 60, 60);
            Assert.Equal(0, empty.Count(accent));
            Assert.True(empty.PaintedPixels > 0, $"the card itself did not print: {empty.Top(4)}");
        });

        _fixture.Run(() =>
        {
            var accent = ((SolidColorBrush)Resource("AccentFillColorDefaultBrush")!).Color;
            var ring = Ring(50, indeterminate: false, build: false);
            ring.IsActive = false;
            var off = PixelHarness.Host(Card(ring), 60, 60);
            Assert.Equal(0, off.Count(accent));
            Assert.True(off.PaintedPixels > 0, $"the card itself did not print: {off.Top(4)}");
        });
    }

    [Fact]
    public void The_indeterminate_ring_prints_an_arc_rather_than_a_disc()
    {
        _fixture.Run(() =>
        {
            var accent = ((SolidColorBrush)Resource("AccentFillColorDefaultBrush")!).Color;
            var sample = PixelHarness.Host(Card(Ring(0, build: false)), 60, 60);
            var inked = sample.Count(accent);

            // Half a circle of 0.4375-radius stroke at a 32 DIP box, inside a 60x60 card: more than a quarter of
            // the ring, less than the whole of it.
            Assert.True(inked > 40 && inked < 320,
                $"the indeterminate ring inked {inked} accent pixels: {sample.Top(4)}");
            Assert.Equal(0, sample.CountAny(BrandEmerald));
        });
    }

    [Fact]
    public void Light_and_dark_hand_over_different_ring_paint()
    {
        Color? lightAccent = null;
        string? lightTop = null;

        _fixture.Run(() =>
        {
            lightAccent = ((SolidColorBrush)Resource("ProgressRingForegroundThemeBrush")!).Color;
            lightTop = PixelHarness.Host(Card(Ring(50, build: false)), 60, 60).Top(4);
        });

        _fixture.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark));

        try
        {
            _fixture.Run(() =>
            {
                FluentThemeManager.ApplyAccent(null);
                var darkAccent = ((SolidColorBrush)Resource("ProgressRingForegroundThemeBrush")!).Color;
                Assert.NotEqual(lightAccent, darkAccent);

                var dark = PixelHarness.Host(Card(Ring(50, build: false)), 60, 60);
                Assert.True(dark.Count(darkAccent) > 40, $"the dark ring painted no accent: {dark.Top(4)}");
                Assert.NotEqual(lightTop, dark.Top(4));
            });
        }
        finally
        {
            _fixture.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Light));
        }
    }

    [Theory]
    [InlineData("ProgressRingForegroundThemeBrush")]
    [InlineData("ProgressRingBackgroundThemeBrush")]
    public void Published_rows_resolve_to_a_brush(string key)
    {
        _fixture.Run(() => Assert.IsAssignableFrom<Brush>(Resource(key)));
    }

    [Theory]
    // ProgressRingStrokeThickness is held out on two counts (ThemeResources/ProgressRing.jalxaml): this reader
    // rejects an x:Double row outright, and upstream's own template never reads it. The rest are names the rest of
    // stage 6 will still have to decide about. ProgressRingForeground and ProgressRingBackground are deliberately
    // not in this list: the runtime's own dictionaries already answer those two names (RingProbe read
    // ProgressRingForeground back as a solid brush), which is a further reason the published rows keep upstream's
    // longer Theme-brush names rather than the short ones.
    [InlineData("ProgressRingStrokeThickness")]
    [InlineData("ProgressRingRingSize")]
    [InlineData("ProgressRingEllipsisVisibility")]
    [InlineData("DividerStrokeThickness")]
    [InlineData("DividerStroke")]
    [InlineData("InfoBadgeValueKindMin")]
    [InlineData("RatingControlSelectedValue")]
    public void Rows_this_layer_has_no_way_to_publish_are_absent(string key)
    {
        _fixture.Run(() => Assert.Null(Resource(key)));
    }

    /// <summary>
    /// Settles until the rendered time a movement claim needs has actually gone by, and refuses the claim when it
    /// never does. This is #47's ring member: <see cref="PixelHarness.Settle"/> stops on its wall-clock budget as
    /// well as on its frame count, so a loaded sequential run can hand back no frames at all, and "the arc did not
    /// move" then measures the harness rather than the control. Two frames is the floor because what the claim
    /// needs is elapsed render time, not a particular count - a spinning ring delivers far more than that
    /// (measured: 24 and up), while a static scene delivers what the watchdog allows.
    /// </summary>
    private static void SettleMoved(string leg, int atLeast = 2)
    {
        var delivered = PixelHarness.SettleFrames(atLeast);
        Assert.True(delivered >= atLeast,
            $"{leg} claims the arc moved across rendered time, and the pump delivered {delivered} frame(s) of the " +
            $"{atLeast} this claim asks for - short of that the two reads sit inside no elapsed render time, so the " +
            "run measures the harness rather than the ring (#47's instrument shape)");
    }

    private static FluentProgressRing Ring(
        double value,
        double size = 32,
        bool indeterminate = true,
        bool build = true)
    {
        var ring = new FluentProgressRing
        {
            Minimum = 0,
            Maximum = 100,
            Value = value,
            IsIndeterminate = indeterminate,
            Width = size,
            Height = size,
        };

        if (build)
        {
            PixelHarness.Build(ring, (int)size, (int)size);
        }

        return ring;
    }

    private static Border Card(FrameworkElement child) => new()
    {
        // The host window's own backdrop is black and the ring's plate is deliberately transparent, so a lit card
        // is what lets the same picture be counted (S1-m 6).
        Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
        Width = 60,
        Height = 60,
        Child = child,
    };

    private static Jalium.UI.Shapes.Path? Arc(FrameworkElement root) => Part(root, "ProgressRingArc") as Jalium.UI.Shapes.Path;

    private static ArcSegment Segment(FrameworkElement root)
    {
        var geometry = Assert.IsType<PathGeometry>(Arc(root)!.Data);
        var figure = Assert.Single(geometry.Figures);
        return Assert.IsType<ArcSegment>(Assert.Single(figure.Segments));
    }

    private static Point Start(FrameworkElement root)
    {
        var geometry = Assert.IsType<PathGeometry>(Arc(root)!.Data);
        var figure = Assert.Single(geometry.Figures);
        return figure.StartPoint;
    }

    private static FrameworkElement? Part(FrameworkElement root, string name) => PixelHarness.Named(root, name);

    private static object? Resource(string key) => Application.Current?.TryFindResource(key);
}
