using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// ListView and its item container, the second slice of the list family. The measurements that shape this file
/// are spike/ListViewProbe, recorded in adaptation/s1d-listview-host-raw.txt, and three of them decide the design:
/// <list type="number">
/// <item><description>the runtime inherits rather than parallels - <c>ListView : ListBox : Selector : ItemsControl</c>
/// and <c>ListViewItem : ListBoxItem</c> with zero declared members of its own - so the container's state surface is
/// the same four DPs the ListBox item had, and no own type is warranted.</description></item>
/// <item><description>WinUI's selected ListView row is NOT an accent block: all eight background rows alias subtle
/// fills and the accent lives in a 4 DIP indicator pill. Copying the ListBox rows here would have been the visible
/// mistake, so the fill-vs-accent split is asserted both ways.</description></item>
/// <item><description>the container's part names are not a contract - renaming or dropping PART_BackgroundBorder and
/// PART_CellsPanel changes nothing, and the #99680081 a stock row paints was the stock template's own trigger, not
/// the framework writing into a part it looks up by name. Only a ContentPresenter is required, and the selection
/// signal (container.IsSelected) still arrives with our template on.</description></item>
/// </list>
/// Like the ListBox batch, a container's own <see cref="FrameworkElement.Style"/> stays null under the implicit
/// type key, so nothing here asserts that a style "was applied"; it asserts the effect.
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraListViewTests : IDisposable
{
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);
    private static readonly Color FrameworkSelectionPurple = Color.FromRgb(0x68, 0x00, 0x81);
    private static readonly Color SurfaceSentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color AccentSentinel = Color.FromRgb(0x00, 0xE0, 0x00);

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraListViewTests(AstraThemeRuntimeFixture fixture)
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

    /// <summary>The sixteen alias rows, by upstream name and upstream target.</summary>
    [Theory]
    [InlineData("ListViewItemBorderBackground", "SubtleFillColorTertiaryBrush")]
    [InlineData("ListViewItemBackground", "SubtleFillColorTransparentBrush")]
    [InlineData("ListViewItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("ListViewItemBackgroundPressed", "SubtleFillColorTertiaryBrush")]
    [InlineData("ListViewItemBackgroundSelected", "SubtleFillColorSecondaryBrush")]
    [InlineData("ListViewItemBackgroundSelectedPointerOver", "SubtleFillColorTertiaryBrush")]
    [InlineData("ListViewItemBackgroundSelectedPressed", "SubtleFillColorSecondaryBrush")]
    [InlineData("ListViewItemBackgroundSelectedDisabled", "SubtleFillColorSecondaryBrush")]
    [InlineData("ListViewItemForeground", "TextFillColorPrimaryBrush")]
    [InlineData("ListViewItemForegroundPointerOver", "TextFillColorPrimaryBrush")]
    [InlineData("ListViewItemForegroundPressed", "TextFillColorPrimaryBrush")]
    [InlineData("ListViewItemForegroundSelected", "TextFillColorPrimaryBrush")]
    [InlineData("ListViewItemForegroundSelectedPointerOver", "TextFillColorPrimaryBrush")]
    [InlineData("ListViewItemForegroundSelectedPressed", "TextFillColorPrimaryBrush")]
    [InlineData("ListViewItemSelectionIndicatorBrush", "AccentFillColorDefaultBrush")]
    [InlineData("ListViewItemSelectionIndicatorPointerOverBrush", "AccentFillColorDefaultBrush")]
    [InlineData("ListViewItemSelectionIndicatorPressedBrush", "AccentFillColorDefaultBrush")]
    [InlineData("ListViewItemSelectionIndicatorDisabledBrush", "AccentFillColorDisabledBrush")]
    public void An_alias_row_resolves_to_its_target(string alias, string target)
    {
        _fixture.Run(() => Assert.Same(Res(target), Res(alias)));
    }

    /// <summary>
    /// The copy claim that matters for this control: a selected ListView row is a subtle fill plus an accent pill,
    /// where a selected ListBox row is an accent block. If the ListView rows ever alias the accent the two controls
    /// collapse into each other and the appearance is wrong on the row surface.
    /// </summary>
    [Fact]
    public void A_selected_row_is_a_subtle_fill_and_not_an_accent_block()
    {
        _fixture.Run(() =>
        {
            Assert.NotSame(Res("AccentFillColorDefaultBrush"), Res("ListViewItemBackgroundSelected"));
            Assert.Same(Res("SubtleFillColorSecondaryBrush"), Res("ListViewItemBackgroundSelected"));
            Assert.Same(Res("AccentFillColorDefaultBrush"), Res("ListViewItemSelectionIndicatorBrush"));
            // Upstream keeps the text colour identical across all six states (its own aliasing); a row that
            // brightened or dimmed the text on selection would not be this template.
            Assert.Same(Res("ListViewItemForeground"), Res("ListViewItemForegroundSelected"));
            Assert.Same(Res("ListViewItemForeground"), Res("ListViewItemForegroundSelectedPressed"));
        });
    }

    /// <summary>The two CornerRadius rows, compared against the value upstream declares.</summary>
    [Theory]
    [InlineData("ListViewItemCornerRadius", 4d)]
    [InlineData("ListViewItemSelectionIndicatorCornerRadius", 1.5d)]
    public void A_geometry_row_carries_upstreams_value(string key, double corner)
    {
        _fixture.Run(() => Assert.Equal(new CornerRadius(corner).ToString(), Resource(key)!.ToString()));
    }

    /// <summary>
    /// Rows upstream declares but this host cannot consume, named rather than counted: the eleven x:Double and
    /// x:Boolean rows and the CheckMode enum are types this reader cannot parse (the value rides as a literal
    /// instead, and ThemeResources/ListView.jalxaml says which), and the focus, check, drag, reorder, placeholder
    /// and legacy families have no part to paint - spike/ListViewProbe mode A read no CheckMode, no
    /// IsMultiSelectCheckBoxVisible and no ShowsCheckHint on the container, and mode B read no MultiSelectSquare
    /// in the shipped tree.
    /// </summary>
    [Theory]
    [InlineData("ListViewItemMinHeight")]
    [InlineData("ListViewItemMinWidth")]
    [InlineData("ListViewItemSelectedBorderThemeThickness")]
    [InlineData("ListViewItemDisabledThemeOpacity")]
    [InlineData("ListViewItemContentOffsetX")]
    [InlineData("ListViewItemCompactSelectedBorderThemeThickness")]
    [InlineData("ListViewItemSelectionIndicatorVisualEnabled")]
    [InlineData("ListViewItemSelectionCheckMarkVisualEnabled")]
    [InlineData("ListViewItemCheckMode")]
    [InlineData("ListViewItemFocusVisualPrimaryBrush")]
    [InlineData("ListViewItemFocusVisualSecondaryBrush")]
    [InlineData("ListViewItemFocusBorderBrush")]
    [InlineData("ListViewItemFocusSecondaryBorderBrush")]
    [InlineData("ListViewItemCheckBrush")]
    [InlineData("ListViewItemCheckBoxBrush")]
    [InlineData("ListViewItemCheckBoxSelectedBrush")]
    [InlineData("ListViewItemCheckBoxDisabledBorderBrush")]
    [InlineData("ListViewItemDragBackground")]
    [InlineData("ListViewItemDragForeground")]
    [InlineData("ListViewItemPlaceholderBackground")]
    [InlineData("ListViewItemMultiArrangeOverlayTextBackground")]
    [InlineData("ListViewItemSelectedBackgroundThemeBrush")]
    [InlineData("ListViewItemPointerOverBackgroundThemeBrush")]
    [InlineData("ListViewItemOverlayBackgroundThemeBrush")]
    public void A_withheld_upstream_row_is_not_published(string key)
    {
        _fixture.Run(() => Assert.Null(Application.Current!.TryFindResource(key)));
    }

    // ---------- the host ----------

    [Fact]
    public void The_list_takes_our_style_rather_than_the_framework_chrome()
    {
        _fixture.Run(() =>
        {
            var list = Mount(List());
            var ours = FluentThemeManager.GetStyle("DefaultListViewStyle");

            // An implicit style never lands in FrameworkElement.Style on this runtime, so the template object is
            // the readable proof that our style is the effective one.
            Assert.Null(list.Style);
            Assert.Same(TemplateOf(ours), list.Template);

            Assert.NotNull(PixelHarness.Named(list, "LayoutRoot"));
            Assert.NotNull(PixelHarness.Descendant<ScrollViewer>(list));
            Assert.NotNull(PixelHarness.Descendant<ItemsPresenter>(list));
            // The stock host's names are gone: PART_OuterBorder was the framework's surface and
            // PART_ColumnHeadersBorder / PART_ColumnHeadersHost carried WPF-style columns.
            Assert.DoesNotContain("PART_OuterBorder", PartNames(list));
            Assert.DoesNotContain("PART_ColumnHeadersBorder", PartNames(list));
        });
    }

    /// <summary>
    /// The reference commit ships no ListView-level theme rows - grep finds no ListViewBackground, no
    /// ListViewStyle key and no controls/dev/ListView directory, because the host chrome lives in the closed
    /// dxaml generic.xaml. The host therefore reads the ListBox rows, which is also what the runtime's own type
    /// shape says. Pinned so the reuse stays a decision and not an accident.
    /// </summary>
    [Fact]
    public void The_host_reads_the_list_rows_because_upstream_publishes_no_listview_rows()
    {
        // The claim is "the ListView surface IS the ListBox surface", so the evidence is one override moving both
        // hosts, not a name we did not publish. The two Assert.Null lines below are the other half: no
        // ListView-prefixed host name was invented, because upstream has none to copy.
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("SolidBackgroundFillColorBaseBrush", SurfaceSentinel);
            try
            {
                var list = PixelHarness.Render(Mount(List()), 300, 220);
                var boxes = PixelHarness.Render(Boxes(), 300, 220);
                Assert.True(list.Count(SurfaceSentinel) > 1_000, $"the shared row did not reach the list; top={list.Top(6)}");
                Assert.True(boxes.Count(SurfaceSentinel) > 1_000, $"the shared row did not reach the box; top={boxes.Top(6)}");
            }
            finally
            {
                FluentThemeManager.OverrideBrush("SolidBackgroundFillColorBaseBrush", null);
            }

            Assert.Null(Application.Current!.TryFindResource("ListViewBackground"));
            Assert.Null(Application.Current!.TryFindResource("ListViewBorderThemeThickness"));
        });
    }

    [Fact]
    public void The_list_style_leaves_item_container_style_to_the_application()
    {
        _fixture.Run(() =>
        {
            var ours = FluentThemeManager.GetStyle("DefaultListViewStyle");
            Assert.DoesNotContain(ours.Setters.OfType<Setter>(), setter => setter.Property?.Name == "ItemContainerStyle");
            Assert.Null(new ListView().ItemContainerStyle);
        });
    }

    // ---------- the container ----------

    [Fact]
    public void The_container_carries_our_item_template_and_its_content()
    {
        _fixture.Run(() =>
        {
            var list = Mount(List());
            var row = Container(list);
            var ours = FluentThemeManager.GetStyle("DefaultListViewItemStyle");

            // Style stays null under the implicit type key (mode D), so the parts are the evidence.
            Assert.Null(row.Style);
            Assert.Same(TemplateOf(ours), row.Template);
            var names = PartNames(row);
            Assert.Contains("ContentBorder", names);
            Assert.Contains("BorderBackground", names);
            Assert.Contains("SelectionIndicator", names);
            Assert.Contains("ContentPresenter", names);
            // The two names the stock container uses are not contracts (mode H) and are not ours.
            Assert.DoesNotContain("PART_BackgroundBorder", names);
            Assert.DoesNotContain("PART_CellsPanel", names);
            Assert.Equal("Item 1", PixelHarness.Descendant<TextBlock>(row)!.Text);
        });
    }

    [Fact]
    public void The_row_geometry_is_upstreams_measure_not_the_runtime_default()
    {
        _fixture.Run(() =>
        {
            var row = Container(Mount(List()));
            // Measured off the runtime before this batch: padding 10,5,10,5 and MinHeight 30 (mode B).
            Assert.Equal(new Thickness(16, 0, 12, 0), row.Padding);
            Assert.Equal(40d, row.MinHeight);
            Assert.True(row.ActualHeight >= 40, $"the row measured {row.ActualHeight}");
        });
    }

    [Fact]
    public void The_container_renders_without_the_stock_part_names()
    {
        // Mode H's decisive half, re-run against the shipped style: the content arrives and the selection signal
        // arrives even though neither PART_BackgroundBorder nor PART_CellsPanel exists in our template. If the
        // framework ever starts looking those names up, the text or the highlight disappears and this fails.
        _fixture.Run(() =>
        {
            var list = Mount(List());
            list.SelectedIndex = 0;
            PixelHarness.Settle(30);
            var row = Container(list);

            Assert.True(row.IsSelected);
            Assert.NotNull(PixelHarness.Descendant<ContentPresenter>(row));
        });
    }

    // ---------- the gutter and the overlay bar ----------

    [Fact]
    public void A_row_spans_the_surface_with_equal_gaps()
    {
        _fixture.Run(() =>
        {
            var (left, right, width) = Gaps(Mount(List()));
            Assert.Equal(0, left);
            Assert.Equal(0, right);
            Assert.Equal(300, width);
        });
    }

    [Fact]
    public void An_overflowing_list_does_not_give_one_side_more_room_than_the_other()
    {
        // The user's report, on the list surface: the stock host reserved 12 DIP for the bar even while it was
        // collapsed and measured 1 left / 13 right (mode F).
        _fixture.Run(() =>
        {
            var (left, right, width) = Gaps(Mount(List(50)));
            Assert.Equal(0, left);
            Assert.Equal(0, right);
            Assert.Equal(300, width);
        });
    }

    /// <summary>
    /// The other half of that fix, and the reason it is safe: with the overlay switch on the bar element itself
    /// reports 40 DIP wide, which reads like a bar covering a third of the row. Mode I resolved it by walking the
    /// subtree - the part that carries a fill is a 2 DIP ThumbBorder, so the 40 is the hit area, exactly the WinUI
    /// shape. Pinned because "the bar is 40 wide" would otherwise be a plausible reason to undo the switch.
    /// </summary>
    [Fact]
    public void The_overlay_bar_paints_a_thin_thumb_and_keeps_a_wide_hit_area()
    {
        _fixture.Run(() =>
        {
            var list = Mount(List(50));
            var bar = BarOf(list);
            Assert.True(bar.ActualWidth >= 24, $"expected the wide hit area, got {bar.ActualWidth}");

            var thumb = PartNamed(bar, "ThumbBorder");
            Assert.Equal(2d, thumb!.ActualWidth);
        });
    }

    // ---------- selection states ----------

    [Fact]
    public void Selecting_brings_the_fill_up_and_raises_the_indicator()
    {
        _fixture.Run(() =>
        {
            var list = Mount(List());
            var row = Container(list);
            var fill = PartNamed(row, "BorderBackground")!;
            var pill = PartNamed(row, "SelectionIndicator")!;
            Assert.Equal(0d, Opacity(pill));

            list.SelectedIndex = 0;
            PixelHarness.Settle(30);

            Assert.Equal(1d, Opacity(fill));
            Assert.Equal(1d, Opacity(pill));
            Assert.Same(Res("ListViewItemBackgroundSelected"), ((Border)fill).Background);
            Assert.Same(Res("ListViewItemSelectionIndicatorBrush"), ((Border)pill).Background);
        });
    }

    [Fact]
    public void A_disabled_row_dims_rather_than_recolouring_the_text()
    {
        // Upstream's Disabled state animates the row's own Opacity to ListViewItemDisabledThemeOpacity (0.3) and
        // publishes no disabled foreground row at all, so the copy has to dim the row.
        _fixture.Run(() =>
        {
            var list = Mount(List());
            var row = Container(list);
            Assert.Equal(1d, Opacity(PartNamed(row, "ContentBorder")!));

            row.IsEnabled = false;
            PixelHarness.Settle(30);
            Assert.Equal(0.3d, Opacity(PartNamed(row, "ContentBorder")!), 2);

            // Diagnostic for how far that cell can be reached from the host: the framework's IsEnabled does carry
            // into a generated container, but the template cell does not re-run on the inherited change, so a
            // disabled list still shows full-opacity rows. Recorded as a gap in audits/listview.md §5.4 rather
            // than papered over - the same cell fires the moment the row itself is disabled, as above.
            row.IsEnabled = true;
            list.IsEnabled = false;
            PixelHarness.Settle(30);
            Assert.True(row.IsEnabled is false, "the host's IsEnabled never reaches a generated container");
            Assert.Equal(1d, Opacity(PartNamed(row, "ContentBorder")!));
            list.IsEnabled = true;
        });
    }

    /// <summary>
    /// Upstream gives "selected and disabled" its own row (ListViewItemPresenter's SelectedDisabledBackground), and
    /// the cell has to sit after the plain disabled one because on this runtime the later writer takes the value.
    /// This is also the fact that keeps ThemeResources/ListView.jalxaml honest: a published row with no cell would
    /// fail the reverse consumption gate.
    /// </summary>
    [Fact]
    public void A_selected_row_that_is_also_disabled_keeps_its_fill_and_lowers_the_pill()
    {
        _fixture.Run(() =>
        {
            var list = Mount(List());
            list.SelectedIndex = 0;
            PixelHarness.Settle(30);
            var row = Container(list);
            row.IsEnabled = false;
            PixelHarness.Settle(30);

            var fill = PartNamed(row, "BorderBackground")!;
            var pill = PartNamed(row, "SelectionIndicator")!;
            Assert.Equal(1d, Opacity(fill));
            Assert.Same(Res("ListViewItemBackgroundSelectedDisabled"), ((Border)fill).Background);
            Assert.Same(Res("ListViewItemSelectionIndicatorDisabledBrush"), ((Border)pill).Background);
            row.IsEnabled = true;
        });
    }

    [Fact]
    public void The_selection_mode_still_governs_how_many_rows_are_selected()
    {
        // Driven through the control's own selection path. The container's IsSelected is a two-way signal the
        // cells read, not the API that arbitrates a mode: writing it on a second row of a Single list leaves both
        // rows painted (the second half of this fact), which is why every state claim here goes through the list.
        _fixture.Run(() =>
        {
            var single = Mount(List());
            single.SelectedIndex = 0;
            PixelHarness.Settle(20);
            single.SelectedIndex = 1;
            PixelHarness.Settle(20);
            Assert.Equal(1, Rows(single).Count(row => row.IsSelected));

            single.SelectedIndex = 0;
            Container(single, 1).IsSelected = true;
            PixelHarness.Settle(20);
            Assert.Equal(2, Rows(single).Count(row => row.IsSelected));
            single.SelectedIndex = -1;
            Rows(single).ForEach(row => row.IsSelected = false);

            var manyList = List();
            manyList.SelectionMode = SelectionMode.Multiple;
            var many = Mount(manyList);
            Container(many).IsSelected = true;
            Container(many, 1).IsSelected = true;
            PixelHarness.Settle(20);
            Assert.Equal(2, Rows(many).Count(row => row.IsSelected));
        });
    }

    // ---------- behaviour that must not regress ----------

    [Fact]
    public void A_thousand_items_still_realize_a_windowful_of_containers()
    {
        _fixture.Run(() =>
        {
            var rows = Rows(Mount(List(1_000)));
            Assert.True(rows.Count is > 0 and < 30, $"realized {rows.Count} containers for 1000 items");
        });
    }

    [Fact]
    public void The_row_stays_on_one_line_however_long_the_text_is()
    {
        _fixture.Run(() =>
        {
            var list = Mount(List());
            list.Items.Clear();
            list.Items.Add(new string('w', 200));
            PixelHarness.Settle(40);
            var row = Container(list);
            Assert.Equal(40d, Math.Round(row.ActualHeight), 0);
        });
    }

    /// <summary>
    /// The grid surface the objective asks for, measured as what the runtime can actually do: there is no WinUI
    /// GridView here (Jalium's GridView is the WPF column View, a ViewBase that is not a Control, and no
    /// GridViewItem type is exported), so the card layout is an ItemsPanel swap on the same host. This asserts the
    /// route works; it does not claim the result is WinUI's GridView, which audits/listview.md §5 keeps as a gap.
    /// </summary>
    [Fact]
    public void A_wrap_panel_items_panel_lays_the_rows_side_by_side()
    {
        _fixture.Run(() =>
        {
            var panel = (ItemsPanelTemplate)XamlReader.Parse(
                "<ItemsPanelTemplate xmlns='http://schemas.jalium.ui/2024'><WrapPanel/></ItemsPanelTemplate>")!;
            var list = List();
            list.ItemsPanel = panel;
            var rows = Rows(Mount(list));
            Assert.True(rows.Count >= 3, $"only {rows.Count} rows realized");
            Assert.True(rows[0].ActualWidth < list.ActualWidth, $"{rows[0].ActualWidth} is not narrower than {list.ActualWidth}");
            Assert.Equal(rows[0].TranslatePoint(new Point(), list).Y, rows[1].TranslatePoint(new Point(), list).Y, 1);
            Assert.True(rows[^1].TranslatePoint(new Point(), list).Y > rows[0].TranslatePoint(new Point(), list).Y);
        });
    }

    // ---------- pixels ----------

    [Fact]
    public void The_surface_paints_its_background_token()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("SolidBackgroundFillColorBaseBrush", SurfaceSentinel);
            try
            {
                var sample = PixelHarness.Render(Mount(List()), 300, 220);
                Assert.True(sample.Count(SurfaceSentinel) > 3_000,
                    $"the surface token did not reach pixels; top={sample.Top(6)}");
                Assert.Equal(0, sample.Count(BrandEmerald));
            }
            finally
            {
                FluentThemeManager.OverrideBrush("SolidBackgroundFillColorBaseBrush", null);
            }
        });
    }

    [Fact]
    public void A_selected_indicator_puts_the_accent_on_the_row()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", AccentSentinel);
            try
            {
                var list = Mount(List());
                list.SelectedIndex = 0;
                PixelHarness.Settle(30);
                var sample = PixelHarness.Render(list, 300, 220);
                Assert.True(sample.Count(AccentSentinel) > 20,
                    $"the indicator did not reach pixels; top={sample.Top(6)}");
                // 4 DIP wide by 16 tall is at most 64 samples of a row that is 300x40; anything approaching a
                // full-row accent block would be the ListBox appearance copied onto the wrong control.
                Assert.True(sample.Count(AccentSentinel) < 400,
                    $"the accent covers {sample.Count(AccentSentinel)} pixels, which is more than a pill");
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
        // #99680081 is what the stock container paints for a selected row - the OS accent at 60 % - and it must
        // not survive the re-template on a control whose selected row is a subtle fill plus a pill.
        _fixture.Run(() =>
        {
            var list = Mount(List());
            list.SelectedIndex = 0;
            PixelHarness.Settle(30);
            var sample = PixelHarness.Render(list, 300, 220);
            Assert.Equal(0, sample.Count(FrameworkSelectionPurple));
        });
    }

    [Fact]
    public void The_list_follows_the_theme()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            var light = PixelHarness.Render(Mount(List()), 300, 220);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var dark = PixelHarness.Render(Mount(List()), 300, 220);

            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.True(light.PaintedPixels > 0, $"light capture is empty: {light.Top(6)}");
            Assert.True(dark.PaintedPixels > 0, $"dark capture is empty: {dark.Top(6)}");
            Assert.NotEqual(light.Top(2), dark.Top(2));
        });
    }

    // ---------- helpers ----------

    private static ListView List(int items = 5)
    {
        // 300x220, the size PixelHarness.Build hosts at and the size spike/ListViewProbe measured the gutter on;
        // a narrower control here would make the width assertion below a tautology about the shell.
        var list = new ListView { Width = 300, Height = 220 };
        for (var index = 0; index < items; index++)
        {
            list.Items.Add($"Item {index + 1}");
        }

        return list;
    }

    private static ListView Mount(ListView list)
    {
        PixelHarness.Build(list, 300, 220);
        PixelHarness.Settle(60);
        return list;
    }

    /// <summary>A shipped ListBox with the same five rows, for the facts that compare the two hosts.</summary>
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

    private static List<ListViewItem> Rows(ListView list)
    {
        var rows = new List<ListViewItem>();
        Collect(list, rows);
        Assert.NotEmpty(rows);
        return rows;
    }

    private static void Collect(DependencyObject node, List<ListViewItem> rows)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            var child = VisualTreeHelper.GetChild(node, index);
            if (child is ListViewItem item)
            {
                rows.Add(item);
            }

            Collect(child!, rows);
        }
    }

    private static ListViewItem Container(ListView list, int index = 0)
    {
        var rows = Rows(list);
        Assert.True(index < rows.Count, $"only {rows.Count} containers are realized, and index {index} is not among them");
        return rows[index];
    }

    private static (double Left, double Right, double Width) Gaps(ListView list)
    {
        var row = Container(list);
        var origin = row.TranslatePoint(new Point(), list);
        return (Math.Round(origin.X, 2), Math.Round(list.ActualWidth - origin.X - row.ActualWidth, 2), row.ActualWidth);
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

        return found ?? throw new InvalidOperationException("no realized vertical scroll bar in the list");
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

    private static ControlTemplate TemplateOf(Style style)
    {
        foreach (var setter in style.Setters.OfType<Setter>())
        {
            if (setter.Property?.Name == "Template")
            {
                return (ControlTemplate)setter.Value!;
            }
        }

        throw new InvalidOperationException("the shipped list style carries no Template setter");
    }

    private static Brush Res(string key) => (Brush)Application.Current!.TryFindResource(key)!;

    private static object? Resource(string key) => Application.Current!.TryFindResource(key);
}
