using FluentJalium.Controls;
using FluentJalium.Tests.Pixel;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Automation.Provider;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// Which crumbs a row keeps, what it puts behind the ellipsis, and what the dropped ones do to the picture. Two
/// assertions here carry weight beyond the control: the capture of a collapsed crumb is the proof that this layer's
/// collapse mechanism hides in pixels rather than only in <c>ActualWidth</c> (adaptation/s1n [G4], [G5]), and the
/// packed-with-no-spacing reading is what says the row is upstream's layout rather than a panel that happens to
/// look similar at one width.
/// </summary>
/// <remarks>
/// Nothing is driven by a pointer or a keyboard: crumb and ellipsis activation go through the invoke pattern, the
/// same substitution the pager tests document, so the hover and pressed arms of the crumb style and the entry-click
/// arm of the ellipsis list are recorded as gaps in docs/astra/audits/breadcrumb-bar.md rather than claimed here.
/// </remarks>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraBreadcrumbBarTests
{
    /// <summary>The framework's own brand green, which no Astra token produces (adaptation/00 S1-e).</summary>
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraBreadcrumbBarTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    [Fact]
    public void The_bar_generates_one_crumb_per_item_and_marks_only_the_last()
    {
        _fixture.Run(() =>
        {
            var bar = Bar(4, 600);

            for (var index = 0; index < 4; index++)
            {
                Assert.NotNull(bar.ContainerFromIndex(index));
            }

            Assert.Null(bar.ContainerFromIndex(4));
            Assert.Null(bar.ContainerFromIndex(-1));

            // Upstream marks exactly one crumb as the current item, and the mark is what collapses its button and
            // its chevron (BreadcrumbBar.xaml:87-95) - which is why activating the last item raises no event.
            Assert.False(bar.ContainerFromIndex(0)!.IsLastItem);
            Assert.False(bar.ContainerFromIndex(2)!.IsLastItem);
            Assert.True(bar.ContainerFromIndex(3)!.IsLastItem);

            var last = bar.ContainerFromIndex(3)!;
            var second = bar.ContainerFromIndex(1)!;
            Assert.Equal(Visibility.Collapsed, Part(last, "PART_ItemButton").Visibility);
            Assert.Equal(Visibility.Collapsed, Part(last, "PART_ChevronTextBlock").Visibility);
            Assert.Equal(Visibility.Visible, Part(last, "PART_LastItemContentPresenter").Visibility);
            Assert.Equal(Visibility.Visible, Part(second, "PART_ItemButton").Visibility);
        });
    }

    [Fact]
    public void A_row_with_room_left_over_keeps_every_crumb_and_packs_them_with_no_spacing()
    {
        _fixture.Run(() =>
        {
            var bar = Bar(3, 600);
            Assert.False(bar.IsEllipsisRendered);
            Assert.Equal(Visibility.Collapsed, Ellipsis(bar).Visibility);

            var first = bar.ContainerFromIndex(0)!;
            var second = bar.ContainerFromIndex(1)!;
            var third = bar.ContainerFromIndex(2)!;
            var origin = first.TranslatePoint(new Point(0, 0), bar);
            var next = second.TranslatePoint(new Point(0, 0), bar);
            var last = third.TranslatePoint(new Point(0, 0), bar);

            // ArrangeItem adds the crumb's own desired width and nothing else (BreadcrumbLayout.cpp:74-81): there is
            // no spacing token anywhere in upstream's file, so a gap between two readings is a real defect.
            Assert.Equal(0d, origin.X, 1);
            Assert.Equal(origin.X + first.ActualWidth, next.X, 1);
            Assert.Equal(next.X + second.ActualWidth, last.X, 1);
        });
    }

    [Fact]
    public void A_narrow_row_drops_a_prefix_and_never_the_ellipsis_or_the_last_crumb()
    {
        _fixture.Run(() =>
        {
            var bar = Bar(4, 260);

            // Revealing the ellipsis is one layout round of its own: the first fit runs while the part is still
            // collapsed and therefore unread, and the pass that shows it re-fits the row against what is left.
            PixelHarness.Settle(8);
            var last = bar.ContainerFromIndex(3)!;

            // The floor of the fit is the ellipsis plus the last crumb (BreadcrumbLayout.cpp:101-118).
            Assert.True(bar.IsEllipsisRendered);
            Assert.Equal(Visibility.Visible, Ellipsis(bar).Visibility);
            Assert.Equal(Visibility.Visible, last.Visibility);

            // The hidden set is a prefix: no crumb may be missing with a visible one to its right, which is the
            // difference between "the first that does not fit and everything before it" and a packing that skips.
            var visible = Enumerable.Range(0, 4)
                .Where(index => bar.ContainerFromIndex(index)!.Visibility == Visibility.Visible)
                .ToArray();
            Assert.Equal(Enumerable.Range(visible[0], 4 - visible[0]), visible);
            Assert.True(visible.Length < 4, "a 260 DIP row kept four 100 DIP crumbs");

            // The ellipsis owns the head of the row, so the first kept crumb starts at or past its right edge.
            var ellipsis = Ellipsis(bar);
            var ellipsisAt = ellipsis.TranslatePoint(new Point(0, 0), bar);
            var keptAt = bar.ContainerFromIndex(visible[0])!.TranslatePoint(new Point(0, 0), bar);
            Assert.True(keptAt.X >= ellipsisAt.X + ellipsis.ActualWidth - 1,
                $"the first kept crumb sits under the ellipsis: {keptAt.X} against {ellipsisAt.X + ellipsis.ActualWidth}");
        });
    }

    [Fact]
    public void The_dropped_crumb_paints_nothing_in_the_capture()
    {
        _fixture.Run(() =>
        {
            // The fit is read back off pixels rather than off ActualWidth because the two mechanisms this layer
            // could have used agree on the geometry and disagree on the picture: upstream's zero rect leaves a sized
            // child at its own 100x24 and painted, while Collapsed takes it to 0x0.
            var bar = new FluentBreadcrumbBar
            {
                Width = 260,
                Height = 40,
                ItemsSource = new object[]
                {
                    Sentinel(Color.FromRgb(0xFF, 0, 0)),
                    Sentinel(Color.FromRgb(0, 0xFF, 0)),
                    Sentinel(Color.FromRgb(0, 0, 0xFF)),
                },
            };
            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
                Width = 260,
                Height = 40,
                Child = bar,
            };

            PixelHarness.Host(card, 260, 40);
            PixelHarness.Settle(10);
            var sample = PixelHarness.Host(card, 260, 40);

            PixelHarness.Settle(8);
            var colours = new[]
            {
                Color.FromRgb(0xFF, 0, 0), Color.FromRgb(0, 0xFF, 0), Color.FromRgb(0, 0, 0xFF),
            };
            Assert.True(bar.IsEllipsisRendered, $"a 260 DIP row dropped nothing: {sample.Top(6)}");

            // Every crumb is checked in whichever state the fit put it in, because the expectation is the fit's own
            // answer rather than a guess at it: a dropped crumb owes zero pixels and a kept one owes its whole
            // 100x24 sentinel. Both arms are needed - the zero on its own would also be satisfied by a row that
            // paints nothing at all, which is the failure this test exists to tell apart from the mechanism.
            var dropped = 0;
            for (var index = 0; index < colours.Length; index++)
            {
                var ink = sample.Count(colours[index]);
                if (bar.ContainerFromIndex(index)!.Visibility == Visibility.Visible)
                {
                    Assert.True(ink >= 100 * 20,
                        $"crumb {index} is visible and painted {ink} pixels of its sentinel: {sample.Top(6)}");
                }
                else
                {
                    dropped++;
                    Assert.True(ink == 0,
                        $"crumb {index} is dropped and still painted {ink} pixels of its sentinel: {sample.Top(6)}");
                }
            }

            Assert.True(dropped > 0, $"no crumb was dropped, so the zero above proves nothing: {sample.Top(6)}");
        });
    }

    [Fact]
    public void Invoking_a_crumb_raises_ItemClicked_with_its_position_and_item()
    {
        _fixture.Run(() =>
        {
            var bar = Bar(3, 600);
            var hits = new List<string>();
            bar.ItemClicked += (_, args) => hits.Add($"{args.Index}:{args.Item}");

            Invoke((Button)Part(bar.ContainerFromIndex(1)!, "PART_ItemButton"));
            Invoke((Button)Part(bar.ContainerFromIndex(0)!, "PART_ItemButton"));

            // Upstream reports the position in the item collection rather than the one in its own peer list,
            // because that list carries the synthetic ellipsis at index 0 (BreadcrumbBarItem.cpp:181-192).
            Assert.Equal(new[] { "1:Crumb 1", "0:Crumb 0" }, hits);
        });
    }

    [Fact]
    public void The_ellipsis_lists_the_collapsed_crumbs_deepest_first()
    {
        _fixture.Run(() =>
        {
            var bar = Bar(4, 260);
            PixelHarness.Settle(8);
            Invoke(Ellipsis(bar));

            var texts = bar.EllipsisFlyout!.Items.Cast<MenuFlyoutItem>().Select(item => item.Text).ToArray();

            // CloneEllipsisItemSource walks the hidden array backwards (BreadcrumbBarItem.cpp:287-296), so the row
            // nearest the visible crumbs is the first one in the list.
            Assert.True(texts.Length > 0, "the ellipsis opened an empty list");
            Assert.Equal(texts.OrderByDescending(text => text, StringComparer.Ordinal).ToArray(), texts);
            Assert.DoesNotContain("Crumb 3", texts);
        });
    }

    [Fact]
    public void Only_the_visible_crumbs_are_counted_in_the_set()
    {
        _fixture.Run(() =>
        {
            var wide = Bar(4, 600);
            Assert.Equal(4, Position(wide.ContainerFromIndex(3)!));
            Assert.Equal(4, SizeOfSet(wide.ContainerFromIndex(0)!));

            var narrow = Bar(4, 260);
            PixelHarness.Settle(8);
            var kept = Enumerable.Range(0, 4)
                .Where(index => narrow.ContainerFromIndex(index)!.Visibility == Visibility.Visible)
                .ToArray();

            // Upstream re-indexes after every arrange pass over the crumbs it rendered, and the ellipsis is in
            // neither the positions nor the count (BreadcrumbLayout.cpp:188-192, BreadcrumbBar.cpp:326-353).
            for (var ordinal = 0; ordinal < kept.Length; ordinal++)
            {
                var crumb = narrow.ContainerFromIndex(kept[ordinal])!;
                Assert.Equal(ordinal + 1, Position(crumb));
                Assert.Equal(kept.Length, SizeOfSet(crumb));
            }
        });
    }

    [Fact]
    public void The_chevron_and_the_current_crumb_read_their_own_rows()
    {
        _fixture.Run(() =>
        {
            var bar = Bar(3, 600);
            var chevron = (TextBlock)Part(bar.ContainerFromIndex(0)!, "PART_ChevronTextBlock");
            Assert.Same(Resource("BreadcrumbBarNormalForegroundBrush"), chevron.Foreground);
            Assert.Equal(new Thickness(2, 0, 2, 0), chevron.Padding);

            // The last crumb loses its button, so its foreground is the row the current-crumb states would have
            // written. It is read off the item rather than off its presenter, because a ContentPresenter declares no
            // Foreground on this runtime and a cell written there is the dead-write class adaptation/00 S1-f gates.
            var last = bar.ContainerFromIndex(2)!;
            Assert.Same(Resource("BreadcrumbBarCurrentNormalForegroundBrush"), last.Foreground);

            // Two of these rows are the same palette object in Light, which is upstream's own fact rather than a
            // flattening of ours (BreadcrumbBar_themeresources.xaml:9, :14, :26).
            Assert.Same(Resource("BreadcrumbBarForegroundBrush"), Resource("BreadcrumbBarNormalForegroundBrush"));
        });
    }

    [Fact]
    public void Dark_and_light_hand_the_row_different_foregrounds_and_never_the_brand_green()
    {
        Color? light = null;
        _fixture.Run(() =>
        {
            light = ((SolidColorBrush)Resource("BreadcrumbBarNormalForegroundBrush")!).Color;
            var bar = Bar(2, 320, build: false);
            var sample = PixelHarness.Host(Card(bar), 320, 40);
            var chevron = (TextBlock)Part(bar.ContainerFromIndex(0)!, "PART_ChevronTextBlock");
            Assert.Equal(light, ((SolidColorBrush?)chevron.Foreground)?.Color);
            var brand = sample.Count(BrandEmerald);
            Assert.True(brand == 0, $"the framework's brand green reached the row: {sample.Top(6)}");
        });

        _fixture.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark));

        try
        {
            _fixture.Run(() =>
            {
                FluentThemeManager.ApplyAccent(null);
                var dark = ((SolidColorBrush)Resource("BreadcrumbBarNormalForegroundBrush")!).Color;
                Assert.NotEqual(light!.Value, dark);
            });
        }
        finally
        {
            _fixture.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Light));
        }
    }

    [Theory]
    [InlineData("BreadcrumbBarChevronPadding")]
    [InlineData("BreadcrumbBarNormalForegroundBrush")]
    [InlineData("BreadcrumbBarHoverForegroundBrush")]
    [InlineData("BreadcrumbBarPressedForegroundBrush")]
    [InlineData("BreadcrumbBarDisabledForegroundBrush")]
    [InlineData("BreadcrumbBarFocusForegroundBrush")]
    [InlineData("BreadcrumbBarCurrentNormalForegroundBrush")]
    [InlineData("BreadcrumbBarForegroundBrush")]
    [InlineData("BreadcrumbBarBackgroundBrush")]
    [InlineData("BreadcrumbBarBorderBrush")]
    public void Published_rows_resolve_to_a_resource(string key)
    {
        _fixture.Run(() => Assert.NotNull(Resource(key)));
    }

    [Theory]
    [InlineData("BreadcrumbBarChevronLeftToRight")]
    [InlineData("BreadcrumbBarChevronRightToLeft")]
    [InlineData("BreadcrumbBarItemFontWeight")]
    [InlineData("BreadcrumbBarItemThemeFontSize")]
    [InlineData("BreadcrumbBarChevronFontSize")]
    [InlineData("BreadcrumbBarEllipsisFlyoutPresenterBackground")]
    [InlineData("BreadcrumbBarEllipsisFlyoutPresenterBorderBrush")]
    [InlineData("BreadcrumbBarEllipsisFlyoutPresenterBorderThemeThickness")]
    [InlineData("BreadcrumbBarCurrentHoverForegroundBrush")]
    [InlineData("BreadcrumbBarCurrentPressedForegroundBrush")]
    [InlineData("BreadcrumbBarCurrentDisabledForegroundBrush")]
    [InlineData("BreadcrumbBarCurrentFocusForegroundBrush")]
    [InlineData("BreadcrumbBarEllipsisDropDownItemBackground")]
    [InlineData("BreadcrumbBarEllipsisDropDownItemBackgroundPointerOver")]
    [InlineData("BreadcrumbBarEllipsisDropDownItemBackgroundPressed")]
    [InlineData("BreadcrumbBarEllipsisDropDownItemBackgroundDisabled")]
    [InlineData("BreadcrumbBarEllipsisDropDownItemForegroundPointerOver")]
    [InlineData("BreadcrumbBarEllipsisDropDownItemForegroundPressed")]
    [InlineData("BreadcrumbBarEllipsisDropDownItemForegroundDisabled")]
    public void Rows_this_runtime_cannot_carry_stay_out_of_the_public_set(string key)
    {
        // Nineteen of upstream's 29 rows are held out, in two kinds. Six are unpublishable by this pipeline: four
        // x:String / x:Double / x:FontWeight rows (this reader destroys the whole dictionary on one), one alias of
        // an x:Double row the layer does not publish, and one alias of a palette brush the Astra palette has no row
        // for. The other thirteen are rows nothing in this port could read without overwriting a live cell: the four
        // Current* twins belong to states of the last crumb's own button, which is collapsed, and the seven
        // drop-down rows plus the two surface rows belong to an ellipsis list this layer renders with the menu
        // family. The second kind is the one worth a name in the public set, and the gate in
        // AstraResourceKeyTests.Transcribed_control_rows_are_read_by_a_template is what forbids publishing it.
        _fixture.Run(() => Assert.Null(Resource(key)));
    }

    private static Border Sentinel(Color color) => new()
    {
        Width = 100,
        Height = 24,
        Background = new SolidColorBrush(color),
    };

    /// <summary>
    /// A row of <paramref name="count"/> crumbs at <paramref name="width"/>. Each crumb carries a fixed-size
    /// sentinel rather than a bare word, because the fit is a comparison of widths and a test that depends on how
    /// wide a string renders cannot say up front how many crumbs it expected to keep.
    /// </summary>
    private static FluentBreadcrumbBar Bar(int count, double width, bool build = true)
    {
        var bar = new FluentBreadcrumbBar { Width = width, Height = 40 };
        for (var index = 0; index < count; index++)
        {
            bar.Items.Add(new CrumbTag(index));
        }

        if (build)
        {
            PixelHarness.Build(bar, (int)width, 40);
        }

        return bar;
    }

    private static Border Card(FluentBreadcrumbBar bar) => new()
    {
        Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
        Width = 320,
        Height = 40,
        Child = bar,
    };

    private static FrameworkElement Part(FrameworkElement root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No {name} under {root.GetType().Name}.");

    private static Button Ellipsis(FluentBreadcrumbBar bar) =>
        (Button)(PixelHarness.Named(bar, "PART_EllipsisButton") ?? throw new InvalidOperationException("No ellipsis part."));

    private static int Position(DependencyObject target) =>
        target.GetValue(AutomationProperties.PositionInSetProperty) is int value ? value : 0;

    private static int SizeOfSet(DependencyObject target) =>
        target.GetValue(AutomationProperties.SizeOfSetProperty) is int value ? value : 0;

    private static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button)).Invoke();

    private static object? Resource(string key) => Application.Current?.TryFindResource(key);

    /// <summary>An item whose reported text is stable and whose rendered size the test chooses.</summary>
    private sealed class CrumbTag : ContentControl
    {
        internal CrumbTag(int index)
        {
            Index = index;
            Width = 100;
            Height = 24;
            Content = new Border { Width = 100, Height = 24, Background = Brushes.Transparent };
        }

        internal int Index { get; }

        public override string ToString() => $"Crumb {Index}";
    }
}
