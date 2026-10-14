using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Automation.Provider;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// TreeView and its item container, the third slice of the list family. This is the one control in the batch whose
/// upstream shape does not survive contact with the runtime: WinUI's TreeView is a flattened list
/// (<c>controls/dev/TreeView/TreeView.xaml:26</c> makes its whole template one <c>TreeViewList</c>, a ListViewBase)
/// while this runtime declares <c>TreeView : ItemsControl</c> and <c>TreeViewItem : HeaderedItemsControl</c> - the
/// nested WPF shape. Nothing is exported on the WinUI side of that gap: TreeViewList, TreeViewNode,
/// TreeViewItemPresenter and TreeViewItemTemplateSettings are all absent, and so are SelectionMode, MultiSelect,
/// the glyph properties and every drag member (spike/TreeViewProbe mode A). What ships here is therefore the
/// nested control wearing WinUI's row, and the four measurements that decide it are:
/// <list type="number">
/// <item><description>a template part named Expander hooks nothing - this runtime's stock tree contains no
/// ToggleButton at all, so the only route a re-template has to the property is the two-way
/// <c>TemplatedParent</c> binding, which was measured to carry both directions
/// (adaptation/00 S1-e 3).</description></item>
/// <item><description>no dependency property carries the nesting depth: a full public and attached-DP diff between
/// a level-1 and a level-2 row showed nothing but IsExpanded, Header and layout bookkeeping, so the indent has to
/// come from the nesting itself (S1-e 5).</description></item>
/// <item><description>unlike ListBoxItem and ListViewItem, a TreeViewItem container's own <see cref="FrameworkElement.Style"/>
/// is NOT null on this runtime, because the runtime ships its own implicit TreeViewItem style. Nothing below
/// asserts "no style was applied"; it asserts the effect, and the merge order is what makes ours win (S1-e 6).
/// </description></item>
/// <item><description>a selection here is a subtle fill plus a 3x16 accent pill whose geometry is readable in the
/// reference file - the only row in the list family where it was (S1-d 9).</description></item>
/// </list>
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraTreeViewTests : IDisposable
{
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);
    private static readonly Color FrameworkSelectionPurple = Color.FromRgb(0x68, 0x00, 0x81);
    private static readonly Color SurfaceSentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color AccentSentinel = Color.FromRgb(0x00, 0xE0, 0x00);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraTreeViewTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            FluentThemeManager.ApplyAccent(null);
        });
    }

    public void Dispose()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("SolidBackgroundFillColorBaseBrush", null);
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
        });
    }

    // ---------- the rows ----------

    /// <summary>The 28 alias rows, by upstream name and upstream target.</summary>
    [Theory]
    [InlineData("TreeViewItemBackground", "SubtleFillColorTransparentBrush")]
    [InlineData("TreeViewItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("TreeViewItemBackgroundPressed", "SubtleFillColorTertiaryBrush")]
    [InlineData("TreeViewItemBackgroundDisabled", "SubtleFillColorDisabledBrush")]
    [InlineData("TreeViewItemBackgroundSelected", "SubtleFillColorSecondaryBrush")]
    [InlineData("TreeViewItemBackgroundSelectedPointerOver", "SubtleFillColorTertiaryBrush")]
    [InlineData("TreeViewItemBackgroundSelectedPressed", "SubtleFillColorSecondaryBrush")]
    [InlineData("TreeViewItemBackgroundSelectedDisabled", "SubtleFillColorDisabledBrush")]
    [InlineData("TreeViewItemForeground", "TextFillColorPrimaryBrush")]
    [InlineData("TreeViewItemForegroundPointerOver", "TextFillColorPrimaryBrush")]
    [InlineData("TreeViewItemForegroundPressed", "TextFillColorSecondaryBrush")]
    [InlineData("TreeViewItemForegroundDisabled", "TextFillColorDisabledBrush")]
    [InlineData("TreeViewItemForegroundSelected", "TextFillColorPrimaryBrush")]
    [InlineData("TreeViewItemForegroundSelectedPointerOver", "TextFillColorPrimaryBrush")]
    [InlineData("TreeViewItemForegroundSelectedPressed", "TextFillColorSecondaryBrush")]
    [InlineData("TreeViewItemForegroundSelectedDisabled", "TextFillColorDisabledBrush")]
    [InlineData("TreeViewItemBorderBrush", "SubtleFillColorTransparentBrush")]
    [InlineData("TreeViewItemBorderBrushPointerOver", "SubtleFillColorTransparentBrush")]
    [InlineData("TreeViewItemBorderBrushPressed", "SubtleFillColorTransparentBrush")]
    [InlineData("TreeViewItemBorderBrushDisabled", "SubtleFillColorTransparentBrush")]
    [InlineData("TreeViewItemBorderBrushSelected", "SubtleFillColorTransparentBrush")]
    [InlineData("TreeViewItemBorderBrushSelectedPointerOver", "SubtleFillColorTransparentBrush")]
    [InlineData("TreeViewItemBorderBrushSelectedPressed", "SubtleFillColorTransparentBrush")]
    [InlineData("TreeViewItemBorderBrushSelectedDisabled", "SubtleFillColorTransparentBrush")]
    [InlineData("TreeViewItemSelectionIndicatorForeground", "AccentFillColorDefaultBrush")]
    [InlineData("TreeViewItemSelectionIndicatorForegroundPointerOver", "AccentFillColorDefaultBrush")]
    [InlineData("TreeViewItemSelectionIndicatorForegroundPressed", "AccentFillColorDefaultBrush")]
    [InlineData("TreeViewItemSelectionIndicatorForegroundDisabled", "TextFillColorDisabledBrush")]
    public void An_alias_row_resolves_to_its_target(string alias, string target)
    {
        _fixture.Run(() => Assert.Same(Res(target), Res(alias)));
    }

    [Fact]
    public void A_selected_row_is_a_subtle_fill_and_the_accent_is_only_the_pill()
    {
        // The trap this slice had to avoid: ListBox selects with an accent block, ListView and TreeView do not.
        // Asserting the wrong family's shape here would put a full-width accent behind every row.
        _fixture.Run(() =>
        {
            Assert.Equal(ToSolid(Res("TreeViewItemBackgroundSelected")).Color, ToSolid(Res("SubtleFillColorSecondaryBrush")).Color);
            Assert.NotEqual(ToSolid(Res("TreeViewItemBackgroundSelected")).Color, ToSolid(Res("AccentFillColorDefaultBrush")).Color);
            Assert.Equal(ToSolid(Res("TreeViewItemSelectionIndicatorForeground")).Color,
                ToSolid(Res("AccentFillColorDefaultBrush")).Color);
        });
    }

    /// <summary>The three geometry rows, at upstream's values, in the only types this reader can parse.</summary>
    [Theory]
    [InlineData("TreeViewItemBorderThemeThickness", "0,0,0,0")]
    [InlineData("TreeViewItemPresenterMargin", "4,2,4,2")]
    [InlineData("TreeViewItemPresenterPadding", "0,3,0,5")]
    public void A_geometry_row_carries_upstreams_value(string key, string value)
    {
        _fixture.Run(() => Assert.Equal(value, Resource(key)?.ToString()));
    }

    /// <summary>
    /// The eight upstream rows this file withholds: five belong to a multi-select surface the runtime does not
    /// expose (no SelectionMode, no MultiSelect, no check glyph), three are x:Double rows this reader cannot parse
    /// - and one of those three still shapes the row, as a literal in the style.
    /// </summary>
    [Theory]
    [InlineData("TreeViewItemMultiSelectBorderBrushSelected")]
    [InlineData("TreeViewItemCheckBoxBackgroundSelected")]
    [InlineData("TreeViewItemCheckBoxBorderSelected")]
    [InlineData("TreeViewItemCheckGlyphSelected")]
    [InlineData("TreeViewItemMultiSelectSelectedItemBorderMargin")]
    [InlineData("TreeViewItemMinHeight")]
    [InlineData("TreeViewItemMultiSelectCheckBoxMinHeight")]
    [InlineData("TreeViewItemContentHeight")]
    public void A_withheld_upstream_row_is_not_published(string key)
    {
        _fixture.Run(() => Assert.Null(Resource(key)));
    }

    // ---------- the host ----------

    [Fact]
    public void The_tree_takes_our_style_rather_than_the_framework_chrome()
    {
        _fixture.Run(() =>
        {
            var tree = Mount(Sample());
            var ours = (ControlTemplate)TemplateOf(FluentThemeManager.GetStyle("DefaultTreeViewStyle"));
            Assert.Same(ours, tree.Template);
            Assert.NotNull(PartNamed(tree, "LayoutRoot"));
            Assert.NotNull(PartNamed(tree, "ScrollViewer"));
            Assert.NotNull(PartNamed(tree, "ItemsPresenter"));

            // The runtime's own host parts. PART_ScrollViewer and RootBorder are the stock template's names, and
            // a row that still carried PART_HeaderBorder or PART_IndentSpacer would mean the stock template is
            // still in the tree.
            foreach (var stock in new[] { "RootBorder", "PART_ScrollViewer", "PART_HeaderBorder", "PART_IndentSpacer", "PART_ExpanderBorder", "PART_ExpanderArrow" })
            {
                Assert.DoesNotContain(stock, PartNames(tree));
            }
        });
    }

    [Fact]
    public void The_host_reads_no_colour_rows_because_upstream_publishes_none()
    {
        // DefaultTreeViewStyle (TreeView.xaml:9-30) sets no Foreground, no Background row and no Border row: its
        // body is a TreeViewList that only TemplateBindings the Background through. So a surface token must not
        // paint this control - and the same override must still move a ListBox, which proves the route is dead on
        // the tree rather than dead in the harness.
        _fixture.Run(() =>
        {
            foreach (var invented in new[] { "TreeViewBackground", "TreeViewForeground", "TreeViewBorder", "TreeViewBorderThemeThickness" })
            {
                Assert.Null(Resource(invented));
            }

            FluentThemeManager.OverrideBrush("SolidBackgroundFillColorBaseBrush", SurfaceSentinel);
            var tree = PixelHarness.Render(Mount(Sample()), 300, 220);
            var boxes = PixelHarness.Render(Boxes(), 300, 220);
            FluentThemeManager.OverrideBrush("SolidBackgroundFillColorBaseBrush", null);

            Assert.Equal(0, tree.Count(SurfaceSentinel));
            Assert.True(boxes.Count(SurfaceSentinel) > 0,
                $"the sentinel moved no pixels at all, so the comparison proves nothing: {boxes.Top(6)}");
        });
    }

    [Fact]
    public void The_tree_style_leaves_item_container_style_to_the_application()
    {
        _fixture.Run(() =>
        {
            var ours = FluentThemeManager.GetStyle("DefaultTreeViewStyle");
            Assert.DoesNotContain(ours.Setters.OfType<Setter>(), setter => setter.Property?.Name == "ItemContainerStyle");
            Assert.Null(new TreeView().ItemContainerStyle);
        });
    }

    // ---------- the rows ----------

    [Fact]
    public void A_plain_string_becomes_a_container_wearing_our_template()
    {
        _fixture.Run(() =>
        {
            var tree = new TreeView { Width = 300, Height = 220 };
            tree.Items.Add("Alpha");
            tree.Items.Add("Beta");
            Mount(tree);
            var rows = Rows(tree);
            Assert.Equal(2, rows.Count);
            Assert.Same(TemplateOf(FluentThemeManager.GetStyle("DefaultTreeViewItemStyle")), rows[0].Template);
            Assert.Equal("Alpha", HeaderText(rows[0]));
        });
    }

    [Fact]
    public void The_row_is_upstreams_measure_and_not_the_runtime_default()
    {
        // Measured on the runtime's own row (probe mode B): padding 6,3,6,3, MinHeight 0, CornerRadius 8. The
        // style replaces the first two and the template's root replaces the third.
        _fixture.Run(() =>
        {
            var tree = Mount(Sample());
            var row = Container(tree);
            Assert.Equal(new Thickness(0), row.Padding);
            Assert.Equal(28d, row.MinHeight);
            var border = (Border)PartNamed(row, "ContentBorder")!;
            Assert.Equal(new Thickness(4, 2, 4, 2), border.Margin);
            Assert.Equal(new Thickness(0, 3, 0, 5), border.Padding);
            Assert.Equal(4d, border.CornerRadius.TopLeft);

            // The header line itself: 20 of content plus 3 and 5 of padding, which is the 28 upstream draws.
            Assert.Equal(28d, border.ActualHeight, 1);
            Assert.True(row.ActualHeight <= 33, $"a collapsed row measured {row.ActualHeight}, which is more than the header line");
        });
    }

    [Fact]
    public void The_chevron_is_bound_to_the_property_and_not_to_a_part_name()
    {
        // The property route is the only route this runtime has, so both directions of it are asserted: writing
        // IsExpanded moves the toggle, and driving the toggle through its automation pattern - the code path a
        // pointer click lands in - writes IsExpanded back and realises the children.
        _fixture.Run(() =>
        {
            var tree = Mount(Sample());
            var parent = Container(tree);
            var chevron = (ToggleButton)PartNamed(parent, "ChevronHitTarget")!;
            Assert.False(parent.IsExpanded);
            Assert.False(chevron.IsChecked ?? false);

            parent.IsExpanded = true;
            PixelHarness.Settle(30);
            Assert.True(chevron.IsChecked ?? false);

            parent.IsExpanded = false;
            PixelHarness.Settle(30);
            Assert.False(chevron.IsChecked ?? false);

            // Collapsing keeps containers realised - the runtime does the same (S1-e 4) - so the count is not
            // the observable here; the visible effect of the click route is the child host coming back.
            var pattern = (IToggleProvider)new ToggleButtonAutomationPeer(chevron).GetPattern(PatternInterface.Toggle)!;
            pattern.Toggle();
            PixelHarness.Settle(40);
            Assert.True(parent.IsExpanded);
            Assert.True(chevron.IsChecked ?? false);
            Assert.Equal(Visibility.Visible, PartNamed(parent, "ChildHost")!.Visibility);

            pattern.Toggle();
            PixelHarness.Settle(40);
            Assert.False(parent.IsExpanded);
            Assert.Equal(Visibility.Collapsed, PartNamed(parent, "ChildHost")!.Visibility);
        });
    }

    [Fact]
    public void A_collapsed_parent_realises_no_children_and_an_expanded_one_does()
    {
        _fixture.Run(() =>
        {
            var tree = Mount(Sample());
            Assert.Equal(2, Rows(tree).Count);
            var host = PartNamed(Container(tree), "ChildHost")!;
            Assert.Equal(Visibility.Collapsed, host.Visibility);

            Container(tree).IsExpanded = true;
            PixelHarness.Settle(40);
            var rows = Rows(tree);
            Assert.Equal(4, rows.Count);
            Assert.Equal(Visibility.Visible, PartNamed(rows[0], "ChildHost")!.Visibility);
        });
    }

    [Fact]
    public void A_leaf_hides_its_chevron_and_keeps_its_column()
    {
        // Hidden, not Collapsed: a leaf that dropped the column would pull its label 20 DIP left of its
        // siblings', which is the kind of one-sided gap this batch has already been called out for twice.
        _fixture.Run(() =>
        {
            var tree = Mount(Sample());
            var parent = Container(tree);
            var leaf = Container(tree, 1);
            Assert.False(leaf.HasItems);
            Assert.True(parent.HasItems);

            var leafChevron = PartNamed(leaf, "ChevronHitTarget")!;
            var parentChevron = PartNamed(parent, "ChevronHitTarget")!;
            Assert.Equal(Visibility.Hidden, leafChevron.Visibility);
            Assert.Equal(Visibility.Visible, parentChevron.Visibility);
            Assert.Equal(parentChevron.ActualWidth, leafChevron.ActualWidth, 1);
            Assert.Equal(20d, leafChevron.ActualWidth, 1);
        });
    }

    [Fact]
    public void The_indent_comes_from_the_nesting_and_is_sixteen_per_level()
    {
        // There is no depth to read: the diff between a level-1 and a level-2 row is IsExpanded, Header and
        // layout bookkeeping (S1-e 5), so the nested host's 16 DIP inset is the whole mechanism, and the runtime's
        // own stock spacer measured the same 16. The cost is recorded in audits/treeview.md §5.3: the child row is
        // 16 DIP narrower than its parent, where WinUI's row spans the list.
        _fixture.Run(() =>
        {
            var tree = Mount(Sample());
            var parent = Container(tree);
            parent.IsExpanded = true;
            PixelHarness.Settle(40);

            var parentBorder = PartNamed(parent, "ContentBorder")!;
            var child = Rows(tree).First(row => !ReferenceEquals(row, parent) && row.Header is string);
            var childBorder = PartNamed(child, "ContentBorder")!;
            var parentAt = parentBorder.TranslatePoint(new Point(), tree);
            var childAt = childBorder.TranslatePoint(new Point(), tree);

            Assert.Equal(16d, Math.Round(childAt.X - parentAt.X, 2), 1);
            Assert.Equal(16d, Math.Round(parentBorder.ActualWidth - childBorder.ActualWidth, 2), 1);
        });
    }

    [Fact]
    public void A_row_spans_the_surface_with_equal_gaps()
    {
        // The 12 DIP gutter, third host in the family: the stock tree measured left=1 right=13 and ours measures
        // equal gaps with the overlay switch on (probe mode H).
        _fixture.Run(() =>
        {
            var tree = Mount(Sample());
            var (left, right, width) = Gaps(tree);
            Assert.Equal(left, right, 1);
            Assert.Equal(300d, width, 1);
        });
    }

    [Fact]
    public void An_overflowing_tree_does_not_give_one_side_more_room_than_the_other()
    {
        _fixture.Run(() =>
        {
            var tree = new TreeView { Width = 300, Height = 160 };
            for (var index = 0; index < 40; index++)
            {
                tree.Items.Add($"Row {index + 1}");
            }

            Mount(tree);
            var (left, right, _) = Gaps(tree);
            Assert.Equal(left, right, 1);
            var bar = BarOf(tree);
            Assert.True(bar.ActualHeight > 0);
            Assert.True(bar.ActualWidth >= 24,
                $"the overlay bar's hit area measured {bar.ActualWidth}; 24 or more is what keeps the row clickable");
            Assert.Equal(2d, ((FrameworkElement)PartNamed(bar, "ThumbBorder")!).ActualWidth, 1);
        });
    }

    // ---------- the states ----------

    [Fact]
    public void Selecting_raises_the_pill_and_leaves_the_fill_subtle()
    {
        _fixture.Run(() =>
        {
            var tree = Mount(Sample());
            var row = Container(tree);
            var pill = PartNamed(row, "SelectionIndicator")!;
            var border = (Border)PartNamed(row, "ContentBorder")!;
            Assert.Equal(0d, Opacity(pill), 1);
            Assert.Equal(ToSolid(Res("TreeViewItemBackground")).Color, ToSolid(border.Background!).Color);

            row.IsSelected = true;
            PixelHarness.Settle(30);
            Assert.Equal(1d, Opacity(pill), 1);
            Assert.Equal(ToSolid(Res("TreeViewItemBackgroundSelected")).Color, ToSolid(border.Background!).Color);
            Assert.NotEqual(ToSolid(Res("AccentFillColorDefaultBrush")).Color, ToSolid(border.Background!).Color);
        });
    }

    [Fact]
    public void A_state_recolours_the_text_through_the_container()
    {
        // The route this slice had to discover: ContentPresenter carries no Foreground member on this runtime, so
        // a template cell aimed at it parses, loads and writes nothing forever (adaptation/00 S1-e 7). The eight
        // foreground rows are consumed by cells on the container instead, and the generated text takes the colour
        // by inheritance - so the assertion belongs on the text element, not on the presenter.
        _fixture.Run(() =>
        {
            var tree = Mount(Sample());
            var row = Container(tree);
            var text = TextOf(row) ?? throw new InvalidOperationException("the row rendered no text to read");
            var resting = ToSolid(text.Foreground!).Color;
            Assert.Equal(ToSolid(Res("TreeViewItemForeground")).Color, resting);

            row.IsSelected = true;
            PixelHarness.Settle(30);
            Assert.Equal(ToSolid(Res("TreeViewItemForegroundSelected")).Color, ToSolid(text.Foreground!).Color);

            row.IsEnabled = false;
            PixelHarness.Settle(30);
            Assert.Equal(ToSolid(Res("TreeViewItemForegroundSelectedDisabled")).Color, ToSolid(text.Foreground!).Color);
            Assert.NotEqual(resting, ToSolid(text.Foreground!).Color);
        });
    }

    [Fact]
    public void Selection_stays_singular_and_the_control_follows_the_row()
    {
        // The list family needed a measured boundary for this: on a ListView, writing IsSelected on two
        // containers left two selected (S1-d 6). A tree arbitrates even direct writes.
        _fixture.Run(() =>
        {
            var tree = Mount(Sample());
            var rows = Rows(tree);
            rows[0].IsSelected = true;
            PixelHarness.Settle(20);
            rows[^1].IsSelected = true;
            PixelHarness.Settle(20);

            Assert.Single(Rows(tree), row => row.IsSelected);
            Assert.False(rows[0].IsSelected);
            Assert.True(rows[^1].IsSelected);
            Assert.Same(rows[^1], tree.SelectedItem);
        });
    }

    [Fact]
    public void A_disabled_row_drops_its_pill_and_a_selected_disabled_row_keeps_it()
    {
        // Upstream keeps SelectedDisabled as its own state (TreeViewItem.xaml:84-94): the fill and the pill stay,
        // in the disabled colours. Plain Triggers are additive on this reader, so the later cell is what wins and
        // this test is the guard on that ordering.
        _fixture.Run(() =>
        {
            var tree = Mount(Sample());
            var row = Container(tree);
            var pill = PartNamed(row, "SelectionIndicator")!;
            var border = (Border)PartNamed(row, "ContentBorder")!;

            row.IsSelected = true;
            row.IsEnabled = false;
            PixelHarness.Settle(30);

            Assert.Equal(1d, Opacity(pill), 1);
            Assert.Equal(ToSolid(Res("TreeViewItemBackgroundSelectedDisabled")).Color, ToSolid(border.Background!).Color);
            Assert.Equal(ToSolid(Res("TreeViewItemForegroundSelectedDisabled")).Color, ToSolid(TextOf(row)!.Foreground!).Color);

            var leaf = Container(tree, 1);
            leaf.IsEnabled = false;
            PixelHarness.Settle(30);
            Assert.Equal(0d, Opacity(PartNamed(leaf, "SelectionIndicator")!), 1);
            Assert.Equal(ToSolid(Res("TreeViewItemForegroundDisabled")).Color, ToSolid(TextOf(leaf)!.Foreground!).Color);
        });
    }

    [Fact]
    public void The_row_stays_on_one_line_however_long_the_header_is()
    {
        _fixture.Run(() =>
        {
            var tree = new TreeView { Width = 300, Height = 220 };
            tree.Items.Add("A header long enough to want a second line if the template let it wrap at all");
            tree.Items.Add("Short");
            Mount(tree);
            var rows = Rows(tree);
            Assert.Equal(rows[0].ActualHeight, rows[1].ActualHeight, 1);
            var text = FindText(rows[0]);
            Assert.NotNull(text);
            Assert.Equal(TextWrapping.NoWrap, text!.TextWrapping);
        });
    }

    [Fact]
    public void A_wide_tree_still_realises_only_a_windowful_of_rows()
    {
        _fixture.Run(() =>
        {
            var tree = new TreeView { Width = 300, Height = 200 };
            for (var index = 0; index < 500; index++)
            {
                tree.Items.Add($"Row {index + 1}");
            }

            Mount(tree);
            var realised = Rows(tree).Count;
            Assert.True(realised > 0, "no container was realised at all");
            Assert.True(realised < 500,
                $"all 500 containers were realised, so the panel stopped virtualising: {realised}");
        });
    }

    // ---------- pixels ----------

    [Fact]
    public void A_selected_pill_puts_the_accent_on_the_row()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", AccentSentinel);
            var tree = Mount(Sample());
            Container(tree).IsSelected = true;
            PixelHarness.Settle(40);
            try
            {
                var sample = PixelHarness.Render(tree, 300, 220);
                // Counted by footprint rather than by exact key: the host is transparent (upstream's own shape)
                // so the pill lands on the harness's unlit black, and the rounded 3x16 rectangle blends into
                // #00D800 and #00A300 at its edges. Matching the exact sentinel would fail on a correct draw.
                var pill = PillPixels(sample);
                Assert.True(pill >= 20, $"the pill did not reach pixels; top={sample.Top(6)}");
                // 3x16 is 48 samples of a 300x28 row; anything near a full-row block is the wrong family.
                Assert.True(pill <= 64, $"the accent covers {pill} pixels, which is more than a 3x16 pill");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            }
        });
    }

    [Fact]
    public void The_selection_is_not_the_frameworks_purple()
    {
        // #99680081 is what the stock row paints for a selection - the OS accent at 60 %, measured in probe mode B
        // - and a tree row upstream is a subtle fill plus a pill, so it must not survive the re-template.
        _fixture.Run(() =>
        {
            var tree = Mount(Sample());
            Container(tree).IsSelected = true;
            PixelHarness.Settle(30);
            var sample = PixelHarness.Render(tree, 300, 220);
            Assert.Equal(0, sample.Count(FrameworkSelectionPurple));
            Assert.Equal(0, sample.Count(BrandEmerald));
        });
    }

    [Fact]
    public void The_tree_follows_the_theme()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var (lightInk, _) = PixelHarness.AssertSurfaceLands(
                NarrowSample(), element => PixelHarness.NamedSurface(element, "ContentBorder"),
                PixelHarness.LightPage, 300, 220, 2_000, "the tree's content surface");

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var (darkInk, _) = PixelHarness.AssertSurfaceLands(
                NarrowSample(), element => PixelHarness.NamedSurface(element, "ContentBorder"),
                PixelHarness.DarkPage, 300, 220, 2_000, "the tree's content surface");

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.NotEqual(lightInk, darkInk);
        });
    }

    // ---------- helpers ----------

    /// <summary>Parent with two children plus a sibling leaf, which is the shape the probe measured.</summary>
    private static TreeView Sample()
    {
        var tree = new TreeView { Width = 300, Height = 220 };
        var parent = new TreeViewItem { Header = "Parent" };
        parent.Items.Add(new TreeViewItem { Header = "Child 1" });
        parent.Items.Add(new TreeViewItem { Header = "Child 2" });
        tree.Items.Add(parent);
        tree.Items.Add(new TreeViewItem { Header = "Sibling" });
        return tree;
    }

    /// <summary>The same content with a selected and a disabled row, for the theme-difference capture.</summary>
    private static TreeView NarrowSample()
    {
        var tree = new TreeView { Width = 300, Height = 220 };
        var parent = new TreeViewItem { Header = "Parent", IsExpanded = true };
        parent.Items.Add(new TreeViewItem { Header = "Child 1" });
        parent.Items.Add(new TreeViewItem { Header = "Child 2", IsSelected = true });
        tree.Items.Add(parent);
        tree.Items.Add(new TreeViewItem { Header = "Disabled", IsEnabled = false });
        return tree;
    }

    private static TreeView Mount(TreeView tree)
    {
        PixelHarness.Build(tree, 300, 220);
        PixelHarness.Settle(60);
        return tree;
    }

    /// <summary>A shipped ListBox, for the facts that compare two hosts under one token override.</summary>
    private static ListBox Boxes()
    {
        var list = new ListBox { Width = 300, Height = 220 };
        for (var index = 0; index < 5; index++)
        {
            list.Items.Add($"Item {index + 1}");
        }

        PixelHarness.Build(list, 300, 220);
        PixelHarness.Settle(60);
        return list;
    }

    private static List<TreeViewItem> Rows(TreeView tree)
    {
        var rows = new List<TreeViewItem>();
        Collect(tree, rows);
        Assert.NotEmpty(rows);
        return rows;
    }

    private static void Collect(DependencyObject node, List<TreeViewItem> rows)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            var child = VisualTreeHelper.GetChild(node, index);
            if (child is TreeViewItem item)
            {
                rows.Add(item);
            }

            Collect(child!, rows);
        }
    }

    private static TreeViewItem Container(TreeView tree, int index = 0)
    {
        var rows = Rows(tree);
        Assert.True(index < rows.Count, $"only {rows.Count} containers are realized, and index {index} is not among them");
        return rows[index];
    }

    private static string? HeaderText(TreeViewItem row) =>
        (PartNamed(row, "ContentPresenter") as ContentPresenter)?.Content as string ?? FindText(row)?.Text;

    /// <summary>The text a row actually shows, which is where a state colour has to arrive.</summary>
    private static TextBlock? TextOf(TreeViewItem row) => FindText(row);

    private static TextBlock? FindText(DependencyObject root)
    {
        TextBlock? found = null;
        Walk(root, node =>
        {
            if (found is null && node is TextBlock block && !string.IsNullOrEmpty(block.Text))
            {
                found = block;
            }
        });

        return found;
    }

    private static (double Left, double Right, double Width) Gaps(TreeView tree)
    {
        var row = Container(tree);
        var origin = row.TranslatePoint(new Point(), tree);
        return (Math.Round(origin.X, 2), Math.Round(tree.ActualWidth - origin.X - row.ActualWidth, 2), row.ActualWidth);
    }

    private static List<string> PartNames(DependencyObject root)
    {
        var names = new List<string>();
        Walk(root, node =>
        {
            var name = (node as FrameworkElement)?.Name;
            if (!string.IsNullOrEmpty(name))
            {
                names.Add(name!);
            }
        });

        return names;
    }

    private static FrameworkElement? PartNamed(DependencyObject root, string name)
    {
        FrameworkElement? found = null;
        Walk(root, node =>
        {
            if (found is null && node is FrameworkElement element && element.Name == name)
            {
                found = element;
            }
        });

        return found;
    }

    /// <summary>
    /// The vertical bar, found by type name rather than by CLR type: which primitives namespace a Jalium build
    /// exports ScrollBar under is not something an appearance test should depend on.
    /// </summary>
    private static FrameworkElement BarOf(DependencyObject root)
    {
        FrameworkElement? found = null;
        Walk(root, node =>
        {
            if (found is null && node is FrameworkElement element &&
                element.GetType().Name == "ScrollBar" && element.ActualHeight > 0)
            {
                found = element;
            }
        });

        return found ?? throw new InvalidOperationException("no realized vertical scroll bar in the tree");
    }

    private static void Walk(DependencyObject node, Action<DependencyObject> visit)
    {
        visit(node);
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            Walk(VisualTreeHelper.GetChild(node, index)!, visit);
        }
    }

    private static double Opacity(FrameworkElement element) => (double)element.GetValue(UIElement.OpacityProperty)!;

    private static SolidColorBrush ToSolid(Brush brush) => (SolidColorBrush)brush;

    /// <summary>How much of the capture is the accent: any pixel green-dominant enough to be the sentinel pill.</summary>
    private static int PillPixels(PixelHarness.Sample sample) => sample.Histogram
        .Where(static entry => (entry.Key & 0x00FF0000) < 0x00800000 &&
                               (entry.Key & 0x0000FF00) >= 0x00008000 &&
                               (entry.Key & 0x000000FF) < 0x00000080)
        .Sum(static entry => entry.Value);

    private static ControlTemplate TemplateOf(Style style)
    {
        foreach (var setter in style.Setters.OfType<Setter>())
        {
            if (setter.Property?.Name == "Template")
            {
                return (ControlTemplate)setter.Value!;
            }
        }

        throw new InvalidOperationException("the shipped tree style carries no Template setter");
    }

    private static Brush Res(string key) => (Brush)Application.Current!.TryFindResource(key)!;

    private static object? Resource(string key) => Application.Current!.TryFindResource(key);
}
