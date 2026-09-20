using FluentJalium.Controls;
using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Automation.Provider;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Shapes;

namespace FluentJalium.Tests;

/// <summary>
/// What the pager generates, what it reports as selected, and how much of the row it lets you see. Three things
/// here are worth the assertions they cost: the pip footprint has to reach a container the control builds in code
/// (a pip that never got sized would still be a button, and every other test would pass); the viewport clamp is the
/// only thing <see cref="FluentPipsPager.MaxVisiblePips"/> does, so an unread value is invisible from the tree; and
/// the selected/normal difference is a dot that grows by 1.8 DIP, which only a measurement can tell apart from a
/// style that never applied.
/// </summary>
/// <remarks>
/// <para>
/// Upstream's own numbers are the yardstick, and two of them contradict how the control is usually described: the
/// pip list is one element per page with no flanking and no ellipsis pip, and the previous/next pair moves a single
/// page rather than a screen of pips (<c>PipsPager.cpp:310-346</c> and <c>:522-565</c>). Both are pinned below so a
/// later reader cannot "fix" them toward the description.
/// </para>
/// <para>
/// Nothing in this file is driven by a pointer or a keyboard. The hover arms of the pip and navigation styles, and
/// the arrow-key focus walk, are markup and handlers the harness cannot activate (task #13); they are recorded as
/// gaps in docs/astra/audits/pips-pager.md rather than claimed here.
/// </para>
/// </remarks>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraPipsPagerTests
{
    /// <summary>The framework's own brand green, which no Astra token produces (adaptation/00 S1-e).</summary>
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraPipsPagerTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    [Fact]
    public void The_pager_builds_one_button_per_page_under_upstreams_part_names()
    {
        _fixture.Run(() =>
        {
            var pager = Pager(5);
            Assert.NotNull(PixelHarness.Named(pager, "RootPanel"));
            Assert.NotNull(PixelHarness.Named(pager, "PreviousPageButton"));
            Assert.NotNull(PixelHarness.Named(pager, "PipsPagerScrollViewer"));
            Assert.NotNull(PixelHarness.Named(pager, "NextPageButton"));

            for (var index = 0; index < 5; index++)
            {
                Assert.NotNull(pager.PipFromIndex(index));
            }

            Assert.Null(pager.PipFromIndex(5));
            Assert.Null(pager.PipFromIndex(-1));

            // Upstream names its repeater part the same way and reaches elements through it; the count is read off
            // the built tree so a list the control keeps to itself cannot pass.
            var host = PixelHarness.Named(pager, "PipsPagerItemsHost") ?? throw new InvalidOperationException("no pip host");
            Assert.Equal(5, Assert.IsType<StackPanel>(host).Children.Count);
        });
    }

    [Fact]
    public void The_footprint_reaches_the_container_the_pager_generates()
    {
        _fixture.Run(() =>
        {
            var pager = Pager(4);
            var pip = pager.PipFromIndex(0)!;

            // 12x24 by default, and not because a Button asked for it: the implicit Fluent button style brings a
            // 32 DIP floor onto every button in the process (spike/PipsPagerProbe mode C measured 12x32 until the
            // pip style takes MinHeight off), so a style that failed to reach the generated pip shows up here as a
            // 24 DIP-tall row of pills.
            Assert.Equal(12, pip.ActualWidth, 1);
            Assert.Equal(24, pip.ActualHeight, 1);
            Assert.NotNull(PixelHarness.Named(pip, "RootGrid"));
            Assert.IsType<Ellipse>(PixelHarness.Named(pip, "NormalDot"));
            Assert.Equal(0d, pip.MinHeight);
        });
    }

    [Fact]
    public void Invoking_a_pip_selects_that_page_and_restyles_the_pair()
    {
        _fixture.Run(() =>
        {
            var pager = Pager(4);
            var raises = new List<string>();
            pager.SelectedIndexChanged += (_, _) => raises.Add(pager.SelectedPageIndex.ToString());

            Invoke(pager.PipFromIndex(2)!);
            Assert.Equal(2, pager.SelectedPageIndex);
            Assert.Equal(new[] { "2" }, raises);

            // Upstream carries selection purely as which style sits on the container (PipsPager.cpp:488-496), so
            // the two styles swapping is the state, not a side effect of it.
            Assert.Same(pager.SelectedPipStyle, pager.PipFromIndex(2)!.Style);
            Assert.Same(pager.NormalPipStyle, pager.PipFromIndex(0)!.Style);
            Assert.Same(pager.NormalPipStyle, pager.PipFromIndex(3)!.Style);

            Invoke(pager.PipFromIndex(0)!);
            Assert.Equal(new[] { "2", "0" }, raises);
        });
    }

    [Fact]
    public void An_out_of_range_page_is_clamped_and_reported_once()
    {
        _fixture.Run(() =>
        {
            var pager = Pager(3);
            var raises = new List<string>();
            pager.SelectedIndexChanged += (_, _) => raises.Add(pager.SelectedPageIndex.ToString());

            pager.SelectedPageIndex = 9;
            Assert.Equal(2, pager.SelectedPageIndex);
            Assert.Equal(new[] { "2" }, raises);

            // The rejected write lands back on the value already set, so the property system stays quiet and the
            // event still has to reach the app: upstream raises once on this path too.
            pager.SelectedPageIndex = 9;
            Assert.Equal(new[] { "2", "2" }, raises);

            pager.SelectedPageIndex = -4;
            Assert.Equal(0, pager.SelectedPageIndex);
            Assert.Equal(new[] { "2", "2", "0" }, raises);
        });
    }

    [Fact]
    public void Unbounded_pages_grow_with_the_selection_and_never_shrink()
    {
        _fixture.Run(() =>
        {
            var pager = new FluentPipsPager { MaxVisiblePips = 5, Width = 200, Height = 40 };
            PixelHarness.Build(pager, 200, 40);

            // NumberOfPages defaults to -1, and an unbounded pager shows one pip per visible slot until the
            // selection needs more (PipsPager.cpp:318-332).
            Assert.Equal(-1, pager.NumberOfPages);
            Assert.Equal(5, Count(pager.PipFromIndex));

            pager.SelectedPageIndex = 7;
            Assert.True(Count(pager.PipFromIndex) == 8,
                $"index {pager.SelectedPageIndex} of {pager.NumberOfPages} pages grew to {Count(pager.PipFromIndex)} pips");

            pager.SelectedPageIndex = 1;
            Assert.Equal(8, Count(pager.PipFromIndex));
        });
    }

    [Fact]
    public void Zero_pages_leaves_the_row_empty_and_disables_both_buttons()
    {
        _fixture.Run(() =>
        {
            var pager = Pager(0, visibility: FluentPipsPagerButtonVisibility.Visible);
            Assert.Null(pager.PipFromIndex(0));
            Assert.False(Named<Button>(pager, "PreviousPageButton")!.IsEnabled);
            Assert.False(Named<Button>(pager, "NextPageButton")!.IsEnabled);
        });
    }

    [Fact]
    public void The_navigation_pair_moves_one_page_and_disables_itself_at_the_edges()
    {
        _fixture.Run(() =>
        {
            var pager = Pager(3, visibility: FluentPipsPagerButtonVisibility.Visible);
            var previous = Named<Button>(pager, "PreviousPageButton")!;
            var next = Named<Button>(pager, "NextPageButton")!;
            var raises = new List<string>();
            pager.SelectedIndexChanged += (_, _) => raises.Add(pager.SelectedPageIndex.ToString());

            Assert.False(previous.IsEnabled);
            Assert.True(next.IsEnabled);

            Invoke(next);
            Invoke(next);
            Assert.Equal(2, pager.SelectedPageIndex);
            Assert.True(previous.IsEnabled);
            Assert.False(next.IsEnabled);

            // A disabled button is not pressed: the framework's own invoke pattern refuses it, so the index stays
            // put and no third raise arrives.
            Assert.Throws<InvalidOperationException>(() => Invoke(next));
            Assert.Equal(new[] { "1", "2" }, raises);

            Invoke(previous);
            Assert.Equal(1, pager.SelectedPageIndex);
        });
    }

    [Fact]
    public void MaxVisiblePips_clamps_the_row_to_that_many_pip_footprints()
    {
        _fixture.Run(() =>
        {
            var pager = Pager(10, visiblePips: 5);
            var scroll = Named<ScrollViewer>(pager, "PipsPagerScrollViewer")!;

            // Upstream's viewport is (k - 1) default pips plus the selected one (PipsPager.cpp:274-290), and every
            // pip footprint is 12 DIP across, so five visible pips is 60 - not the 120 the ten pips would need.
            Assert.Equal(60d, scroll.MaxWidth, 1);

            pager.MaxVisiblePips = 10;
            PixelHarness.Settle(6);
            Assert.Equal(120d, scroll.MaxWidth, 1);

            pager.MaxVisiblePips = 2;
            PixelHarness.Settle(6);
            Assert.Equal(24d, scroll.MaxWidth, 1);

            // The clamp is measured off the realized pips, so it follows the axis rather than a second copy of the
            // numbers: a vertical row is limited by height and let loose across.
            pager.Orientation = Jalium.UI.Controls.Orientation.Vertical;
            PixelHarness.Settle(6);
            Assert.Equal(double.PositiveInfinity, scroll.MaxWidth);
            Assert.Equal(24d, scroll.MaxHeight, 1);
        });
    }

    [Fact]
    public void Orientation_swaps_the_footprint_and_aims_the_chevrons()
    {
        _fixture.Run(() =>
        {
            var pager = Pager(4, visibility: FluentPipsPagerButtonVisibility.Visible);
            var previous = Named<Button>(pager, "PreviousPageButton")!;
            var next = Named<Button>(pager, "NextPageButton")!;

            // RootPanelOrientationStates (PipsPager.xaml:59-80) is what rotates upstream's glyphs; here the same
            // trigger writes the buttons' transforms, so the reading is the transform and not a state name.
            Assert.Equal(-90d, Assert.IsType<RotateTransform>(previous.RenderTransform).Angle, 1);
            Assert.Equal(90d, Assert.IsType<RotateTransform>(next.RenderTransform).Angle, 1);

            pager.Orientation = Jalium.UI.Controls.Orientation.Vertical;
            PixelHarness.Settle(6);
            Assert.IsNotType<RotateTransform>(previous.RenderTransform);
            Assert.Equal(24, pager.PipFromIndex(0)!.ActualWidth, 1);
            Assert.Equal(12, pager.PipFromIndex(0)!.ActualHeight, 1);
        });
    }

    [Fact]
    public void Each_visibility_mode_holds_or_frees_its_own_slot()
    {
        _fixture.Run(() =>
        {
            var pager = Pager(3);
            var previous = Named<Button>(pager, "PreviousPageButton")!;

            Assert.Equal(Visibility.Collapsed, previous.Visibility);

            // Hidden keeps the room and drops the paint; Collapsed gives the room back. Upstream splits these two
            // states for a reason (PipsPager.xaml:17-29) and the reason is layout.
            pager.PreviousButtonVisibility = FluentPipsPagerButtonVisibility.VisibleOnPointerOver;
            PixelHarness.Settle(6);
            Assert.Equal(Visibility.Visible, previous.Visibility);
            Assert.Equal(0d, previous.Opacity, 1);

            pager.PreviousButtonVisibility = FluentPipsPagerButtonVisibility.Visible;
            PixelHarness.Settle(6);
            Assert.Equal(1d, previous.Opacity, 1);
        });
    }

    [Fact]
    public void The_dot_grows_and_takes_its_selected_row()
    {
        _fixture.Run(() =>
        {
            var pager = Pager(3);
            var (selectedWide, selectedNarrow) = Dots(pager, 0);
            var (normalWide, normalNarrow) = Dots(pager, 1);

            // One of the two drawn dots shows, and it is the wider one on the chosen pip.
            Assert.Equal(Visibility.Visible, selectedWide.Visibility);
            Assert.Equal(Visibility.Collapsed, selectedNarrow.Visibility);
            Assert.Equal(5.6d, selectedWide.Width, 1);
            Assert.Equal(Visibility.Visible, normalNarrow.Visibility);
            Assert.Equal(Visibility.Collapsed, normalWide.Visibility);
            Assert.Equal(3.8d, normalNarrow.Width, 1);

            // Upstream's selected and normal foregrounds are the same token (ControlStrongFillColorDefaultBrush at
            // :15 and :18), so the diameter is the whole difference and the colour identity is the claim that
            // holds. Reading the brush rather than a pixel is deliberate: the dot is a 4 DIP shape in a 12x24 box.
            Assert.Same(Resource("PipsPagerSelectionIndicatorForegroundSelected"), selectedWide.Fill);
            Assert.Same(Resource("PipsPagerSelectionIndicatorForeground"), normalNarrow.Fill);
        });
    }

    [Fact]
    public void The_pager_paints_its_dots_and_the_bigger_dot_reaches_further_across_its_box()
    {
        _fixture.Run(() =>
        {
            // Two things had to be measured before this claim could be made: a capture of a single element drops
            // the alpha its brush is blended with (adaptation/00 S1-b), which is what read 0 at the centre of a
            // painted dot, and the host window's own backdrop is black - so a pip, which in Light is black at 37%
            // alpha, composited onto black-on-black and an added pip changed no pixel at all. On a lit surface the
            // row does paint, and one page versus two turns the two diameters into a comparison of like with like.
            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
                Width = 200,
                Height = 48,
            };
            var pager = new FluentPipsPager { NumberOfPages = 0, MaxVisiblePips = 5 };
            card.Child = pager;

            PixelHarness.Host(card, 200, 48);
            PixelHarness.Settle(8);
            var empty = PixelHarness.Host(card, 200, 48);
            pager.NumberOfPages = 1;
            PixelHarness.Settle(8);
            var one = PixelHarness.Host(card, 200, 48);
            pager.NumberOfPages = 2;
            PixelHarness.Settle(8);
            var two = PixelHarness.Host(card, 200, 48);

            // Counting the token's own colour is not a probe here: in Light the pip brush is black at 37% alpha, so
            // its packed RGB is the same key as every dark pixel of the backdrop, and the count came back as the
            // whole capture (105966 of 9600 pixels). The ink is therefore measured as what the row gains when a pip
            // is added - selected dot first, normal dot second - which also puts the two diameters side by side.
            var scroll = Assert.IsType<ScrollViewer>(PixelHarness.Named(pager, "PipsPagerScrollViewer"));
            var selectedInk = Ink(empty, one);
            var normalInk = Ink(one, two);
            Assert.NotNull(pager.PipFromIndex(1));
            Assert.True(selectedInk > 0, $"the pager painted no pip at all: {one.Top(4)} against {empty.Top(4)}");
            Assert.True(normalInk > 0,
                $"a second pip added no ink: {two.Top(6)} over {one.Top(6)}, viewport {scroll.MaxWidth}");
            Assert.True(selectedInk > normalInk,
                $"the selected dot painted no more ink than the normal one: {selectedInk} against {normalInk}");
        });
    }

    [Fact]
    public void Dark_and_light_paint_the_row_with_different_tokens()
    {
        Color? lightColour = null;
        _fixture.Run(() =>
        {
            lightColour = ((SolidColorBrush)Resource("PipsPagerSelectionIndicatorForeground")!).Color;
            Assert.True(PixelHarness.Host(Pager(3, build: false), 200, 48).Count(lightColour.Value) > 0);
        });

        _fixture.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark));

        try
        {
            _fixture.Run(() =>
            {
                FluentThemeManager.ApplyAccent(null);
                var darkColour = ((SolidColorBrush)Resource("PipsPagerSelectionIndicatorForeground")!).Color;
                Assert.NotEqual(lightColour!.Value, darkColour);
                var sample = PixelHarness.Host(Pager(3, build: false), 200, 48);
                Assert.True(sample.Count(darkColour) > 0, $"the dark row painted no pip colour: {sample.Top(4)}");
            });
        }
        finally
        {
            _fixture.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Light));
        }
    }

    [Theory]
    [InlineData("PipsPagerSelectionIndicatorForeground")]
    [InlineData("PipsPagerSelectionIndicatorForegroundSelected")]
    [InlineData("PipsPagerSelectionIndicatorForegroundPointerOver")]
    [InlineData("PipsPagerSelectionIndicatorForegroundPressed")]
    [InlineData("PipsPagerSelectionIndicatorForegroundDisabled")]
    [InlineData("PipsPagerNavigationButtonForeground")]
    [InlineData("PipsPagerNavigationButtonForegroundDisabled")]
    [InlineData("PipsPagerButtonBorderThickness")]
    [InlineData("PipsPagerNavigationButtonBorderThickness")]
    public void Published_rows_resolve_to_a_resource(string key)
    {
        _fixture.Run(() => Assert.NotNull(Resource(key)));
    }

    [Theory]
    [InlineData("PipsPagerButtonWidth")]
    [InlineData("PipsPagerButtonHeight")]
    [InlineData("PipsPagerHorizontalOrientationButtonWidth")]
    [InlineData("PipsPagerNavigationButtonScalePressed")]
    [InlineData("PipsPagerSelectedGlyph")]
    [InlineData("PipsPagerNormalGlyphFontSize")]
    [InlineData("PipsPagerPreviousPageButtonGlyph")]
    public void Rows_this_runtime_cannot_carry_stay_out_of_the_public_set(string key)
    {
        // PipsPagerButtonWidth/Height are upstream's own dead rows - no template or code file in the reference tree
        // reads them - and the rest are x:Double or x:String rows, which this resource reader drops on the floor
        // along with the dictionary that declares them. Six Style rows are deliberately absent from this list: the
        // runtime parses those fine, so Styles/PipsPager.jalxaml publishes them under upstream's own keys.
        _fixture.Run(() => Assert.Null(Resource(key)));
    }

    private static FluentPipsPager Pager(
        int pages,
        int visiblePips = 5,
        FluentPipsPagerButtonVisibility visibility = FluentPipsPagerButtonVisibility.Collapsed,
        bool build = true)
    {
        var pager = new FluentPipsPager
        {
            NumberOfPages = pages,
            MaxVisiblePips = visiblePips,
            PreviousButtonVisibility = visibility,
            NextButtonVisibility = visibility,
            Width = 200,
            Height = 48,
        };

        // Build() and Host() are two different parents in the harness, so a control that is about to be hosted is
        // left unattached here (adaptation/00 S1-b).
        if (build)
        {
            PixelHarness.Build(pager, 200, 48);
        }

        return pager;
    }

    private static int Count(Func<int, Button?> reader)
    {
        var count = 0;
        while (reader(count) is not null)
        {
            count++;
        }

        return count;
    }

    private static T Named<T>(FluentPipsPager pager, string name) where T : FrameworkElement =>
        (T)PixelHarness.Named(pager, name)!;

    private static (Ellipse Wide, Ellipse Narrow) Dots(FluentPipsPager pager, int index) => (
        Assert.IsType<Ellipse>(PixelHarness.Named(pager.PipFromIndex(index)!, "SelectedDot")),
        Assert.IsType<Ellipse>(PixelHarness.Named(pager.PipFromIndex(index)!, "NormalDot")));

    private static int Ink(PixelHarness.Sample fewer, PixelHarness.Sample more) => more.Histogram
        .Where(entry => entry.Value > fewer.Histogram.GetValueOrDefault(entry.Key))
        .Sum(entry => entry.Value - fewer.Histogram.GetValueOrDefault(entry.Key));

    private static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button)).Invoke();

    private static object? Resource(string key) => Application.Current?.TryFindResource(key);
}
