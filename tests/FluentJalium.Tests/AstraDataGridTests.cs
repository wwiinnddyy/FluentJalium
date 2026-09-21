using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Data;
using Jalium.UI.Markup;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The DataGrid batch's behaviour and read-back facts (docs/astra/audits/datagrid.md). This is the first control in
/// the library that arrives with a framework theme of its own: the runtime ships
/// <c>Themes/Controls/DataGrid.jalxaml</c> with eleven setter cells and a full named part tree, so the questions
/// here are not "does a style land at all" but "does ours land on top, does the part contract survive, does the
/// colour reach the text - and does the text survive at all" (docs/astra/adaptation/00 S1-i).
///
/// <para>
/// The last question is the one this batch paid for. A grid under Astra rendered no cell text whatsoever: the cell's
/// <c>ContentPresenter</c> held the <c>TextBlock</c> the control generated and still measured 0x0, because the row
/// style we shipped rebuilt the subtree those content visuals are parented in and 26.10.9 has no presenter hook that
/// releases a visual left behind. The four facts named <c>*cell*</c> below are the regression pins for that, and
/// <c>The_row_keeps_the_frameworks_template</c> is the contract they hold in place: no DataGridRow style ships, and
/// re-adding one makes those read-backs red again.
/// </para>
///
/// <para>
/// Two framework defects are still paid off rather than copied. The runtime paints a selected row with the
/// brand-emerald <c>AccentBrush</c> gradient, which is now retinted through
/// <c>ThemeResources/FrameworkRetints.jalxaml</c> because the row that paints it cannot be re-templated; and its
/// selected foreground never reaches the text, because <c>DataGridCell</c>'s own style writes <c>Foreground</c> and
/// outranks inheritance, so our cell style writes the text-on-accent colour itself.
/// </para>
///
/// Every state read here is taken off the realized element the cell was aimed at, never off the markup, because
/// earlier batches in this repo shipped rows that parsed, built green and never reached a pixel
/// (adaptation/00 S1-f, S1-g, S1-i 7).
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraDataGridTests : IDisposable
{
    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraDataGridTests(AstraThemeRuntimeFixture fixture)
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
        _fixture.Run(() => FluentThemeManager.ApplyTheme(FluentThemeVariant.Light));
    }

    // ---------- the rows ----------

    /// <summary>Every alias row, by the name its reader uses and the palette token it points at. The last pair is not
    /// a ModernWpf name: it is the framework's own, republished by ThemeResources/FrameworkRetints.jalxaml, and it
    /// aliases the same instance rather than redefining it so the application accent keeps moving it.</summary>
    [Theory]
    [InlineData("DataGridBackground", "CardBackgroundFillColorDefaultBrush")]
    [InlineData("DataGridBorderBrush", "CardStrokeColorDefaultBrush")]
    [InlineData("DataGridGridLinesBrush", "DividerStrokeColorDefaultBrush")]
    [InlineData("DataGridRowBackground", "SubtleFillColorTransparentBrush")]
    [InlineData("DataGridAlternatingRowBackground", "SubtleFillColorSecondaryBrush")]
    [InlineData("DataGridRowForeground", "TextFillColorPrimaryBrush")]
    [InlineData("DataGridRowForegroundDisabled", "TextFillColorDisabledBrush")]
    [InlineData("DataGridRowSelectedForeground", "TextOnAccentFillColorPrimaryBrush")]
    [InlineData("DataGridColumnHeaderForeground", "TextFillColorPrimaryBrush")]
    [InlineData("DataGridColumnHeaderBackground", "SubtleFillColorTransparentBrush")]
    [InlineData("DataGridColumnHeaderForegroundDisabled", "TextFillColorDisabledBrush")]
    [InlineData("DataGridHeaderBackgroundPointerOver", "SubtleFillColorSecondaryBrush")]
    [InlineData("DataGridHeaderBackgroundPressed", "SubtleFillColorTertiaryBrush")]
    [InlineData("DataGridHeaderBackgroundDisabled", "SubtleFillColorDisabledBrush")]
    [InlineData("DataGridHeaderBorderBrush", "ControlStrokeColorSecondaryBrush")]
    [InlineData("DataGridHeaderSeparatorBrush", "ControlStrokeColorSecondaryBrush")]
    [InlineData("DataGridRowHeaderBackground", "SubtleFillColorTransparentBrush")]
    [InlineData("AccentBrush", "AccentFillColorDefaultBrush")]
    public void An_alias_row_resolves_to_its_target(string alias, string target)
    {
        _fixture.Run(() => Assert.Same(Brush(target), Brush(alias)));
    }

    [Theory]
    [InlineData("DataGridBorderThickness", 1, 1, 1, 1)]
    [InlineData("DataGridCellBorderThickness", 0, 0, 1, 0)]
    [InlineData("DataGridColumnHeaderBorderThickness", 0, 0, 0, 1)]
    [InlineData("DataGridColumnHeaderPadding", 4, 0, 4, 0)]
    public void A_metric_row_reads_as_its_literal(string key, double left, double top, double right, double bottom)
    {
        _fixture.Run(() => Assert.Equal(new Thickness(left, top, right, bottom), (Thickness)Resource(key)!));
    }

    /// <summary>
    /// The rows withheld from the transcription, each by the reason it has no consumer here. Upstream's selected
    /// -row opacities are <c>x:Double</c> rows this markup reader cannot parse (the same limit that kept sixteen
    /// TabView metrics out of the palette), and the four <c>DataGridRow*</c> fills below lost their consumer for the
    /// measured reason in <see cref="The_row_keeps_the_frameworks_template"/>: no row template of ours ships, so
    /// nothing of ours can read a row state. The checkbox, group-header, validation and grid-line-visibility rows
    /// belong to features this batch does not ship.
    /// </summary>
    [Theory]
    [InlineData("DataGridRowSelectedBackground")]
    [InlineData("DataGridRowHoveredBackground")]
    [InlineData("DataGridRowBorderThickness")]
    [InlineData("DataGridDetailsPresenterBackground")]
    [InlineData("DataGridRowSelectedBackgroundOpacity")]
    [InlineData("DataGridRowSelectedHoveredBackgroundOpacity")]
    [InlineData("DataGridRowSelectedUnfocusedBackgroundOpacity")]
    [InlineData("DataGridRowSelectedHoveredUnfocusedBackgroundOpacity")]
    [InlineData("DataGridCheckBoxBackgroundChecked")]
    [InlineData("DataGridRowGroupHeaderBackgroundBrush")]
    [InlineData("DataGridRowGroupHeaderForegroundBrush")]
    [InlineData("DataGridRowInvalidBrush")]
    [InlineData("DataGridCellInvalidBrush")]
    [InlineData("DataGridCellFocusVisualPrimaryBrush")]
    [InlineData("DataGridCurrencyVisualPrimaryBrush")]
    [InlineData("ScrollBarsSeparatorBackground")]
    [InlineData("FillerGridLinesBrush")]
    [InlineData("DataGridRowHeaderForeground")]
    public void A_withheld_upstream_row_is_not_published(string key)
    {
        _fixture.Run(() => Assert.Null(Application.Current!.TryFindResource(key)));
    }

    // ---------- the templates land, and the part contract survives ----------

    [Fact]
    public void The_grid_takes_our_style_rather_than_the_frameworks()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Grid());
            var ours = FluentThemeManager.GetStyle("DefaultDataGridStyle");

            // An implicit style never lands in FrameworkElement.Style on this runtime - and neither does the
            // framework's own, which is the reading S1-i 8 had to correct - so the template object is the proof.
            Assert.Null(grid.Style);
            Assert.Same(TemplateOf(ours!), grid.Template);
        });
    }

    /// <summary>
    /// The part names the control casts its lookups to, checked against the type each cast asks for. A name that is
    /// present but the wrong type reads as absent to the control and loses the feature silently: an
    /// <c>ItemsPresenter</c>-only template measured 0 cells, no header row and no row-header gutter
    /// (audits/datagrid.md §2, from DataGrid.cs:1071-1077).
    /// </summary>
    [Fact]
    public void Every_part_the_control_looks_up_is_present_with_the_type_it_casts_to()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Grid());
            Assert.Multiple(
                () => Assert.IsType<StackPanel>(Part(grid, "PART_ColumnHeadersHost")),
                () => Assert.IsType<StackPanel>(Part(grid, "PART_RowsHost")),
                () => Assert.IsType<ScrollViewer>(Part(grid, "PART_ColumnHeadersScrollViewer")),
                () => Assert.IsType<ScrollViewer>(Part(grid, "PART_DataScrollViewer")),
                () => Assert.IsType<Canvas>(Part(grid, "PART_DragOverlay")),
                () => Assert.IsAssignableFrom<FrameworkElement>(Part(grid, "PART_ColumnHeadersBorder")),
                () => Assert.IsAssignableFrom<FrameworkElement>(Part(grid, "PART_RowHeaderCorner")),
                () => Assert.NotNull(Part(grid, "PART_OuterBorder")));
        });
    }

    [Fact]
    public void The_grid_builds_headers_rows_and_cells_under_our_templates()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Grid(rows: 3));
            var rows = All<DataGridRow>(grid);
            var headers = All<DataGridColumnHeader>(grid);
            Assert.Multiple(
                () => Assert.Equal(3, rows.Count),
                () => Assert.Equal(2, headers.Count),
                () => Assert.All(rows, row => Assert.Equal(2, All<DataGridCell>(row).Count)),
                () => Assert.All(rows, row => Assert.IsType<StackPanel>(Part(row, "PART_CellsPanel"))),
                () => Assert.All(rows, row => Assert.NotNull(Part(row, "PART_RowBorder"))),
                () => Assert.All(headers, header =>
                {
                    Assert.NotNull(Part(header, "PART_HeaderBorder"));
                    Assert.IsType<TextBlock>(Part(header, "PART_SortIndicator"));
                    Assert.IsAssignableFrom<FrameworkElement>(Part(header, "PART_ResizeGrip"));
                }),
                () => Assert.Equal("Alpha", PixelHarness.Descendant<TextBlock>(headers[0])!.Text));
        });
    }

    /// <summary>
    /// The two metrics the framework's own theme sets and this style overrides, read off the control and off the
    /// realized geometry - 32/32 upstream rather than the runtime's 30/34.
    /// </summary>
    [Fact]
    public void The_row_and_header_heights_are_upstreams_and_not_the_frameworks()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Grid());
            var row = All<DataGridRow>(grid).First();
            var header = All<DataGridColumnHeader>(grid).First();
            Assert.Multiple(
                () => Assert.Equal(32d, grid.RowHeight),
                () => Assert.Equal(32d, grid.ColumnHeaderHeight),
                () => Assert.Equal(32d, row.ActualHeight),
                () => Assert.Equal(32d, header.ActualHeight));
        });
    }

    // ---------- surface and state ----------

    [Fact]
    public void The_surface_geometry_and_its_line_are_our_rows()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Grid());
            var outer = (Border)Part(grid, "PART_OuterBorder");
            var corner = (FrameworkElement)Part(grid, "PART_RowHeaderCorner");
            Assert.Multiple(
                () => Assert.Same(Brush("DataGridBackground"), outer.Background),
                () => Assert.Same(Brush("DataGridBorderBrush"), outer.BorderBrush),
                () => Assert.Equal((Thickness)Resource("DataGridBorderThickness"), outer.BorderThickness),
                () => Assert.Equal((CornerRadius)Resource("ControlCornerRadius")!, outer.CornerRadius),
                () => Assert.Equal(20d, corner.Width),
                () => Assert.NotEqual(new CornerRadius(12), outer.CornerRadius));
        });
    }

    /// <summary>
    /// The contract the whole batch turns on, pinned rather than assumed: no DataGridRow style is published, so the
    /// row keeps the template the control instantiated. The reading that earned it is in Styles/DataGrid.jalxaml -
    /// installing a row style of ours leaves a bound cell holding its TextBlock as Content while the presenter that
    /// should host it measures 0x0 and the text never reaches a pixel. This fact is the cheap half of that pair;
    /// A_bound_cell_renders_its_text is the half with teeth.
    /// </summary>
    [Fact]
    public void The_row_keeps_the_frameworks_template()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Grid());
            var row = All<DataGridRow>(grid).First();
            Assert.Multiple(
                () => Assert.Null(Application.Current!.TryFindResource("DefaultDataGridRowStyle")),
                // The parts the control needs are there; the part only our template had is not.
                () => Assert.IsType<StackPanel>(Part(row, "PART_CellsPanel")),
                () => Assert.NotNull(Part(row, "PART_RowBorder")),
                () => Assert.Null(PixelHarness.Named(row, "RowSelectionBackground")),
                () => Assert.Null(PixelHarness.Named(row, "PART_DetailsPresenter")));
        });
    }

    /// <summary>
    /// The regression pin for the stranded content visual: a bound cell's TextBlock has to be in the tree, sized, and
    /// showing the item's value. Before this batch fixed it the same read returned an element off Content that no
    /// presenter hosted, a presenter at desired 0,0 and no text at all - which is why the assertion names the
    /// receiver it reads and the size it got.
    /// </summary>
    [Fact]
    public void A_bound_cell_renders_its_text()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Grid(rows: 2));
            var cell = FirstCell(All<DataGridRow>(grid)[1]);
            var text = PixelHarness.Descendant<TextBlock>(cell);
            Assert.True(text is not null,
                $"a bound cell holds content but hosts no text visual: content={cell.Content?.GetType().Name ?? "null"} " +
                $"isEditing={(bool)Prop(cell, "IsEditing")!} cells={All<DataGridCell>(All<DataGridRow>(grid)[1]).Count}");
            Assert.Multiple(
                () => Assert.Same(cell.Content, text),
                () => Assert.Equal("Row 2", text!.Text),
                () => Assert.True(text.ActualWidth > 20, $"the cell's text measured {text.ActualWidth}x{text.ActualHeight}"),
                () => Assert.True(text.ActualHeight > 10, $"the cell's text measured {text.ActualWidth}x{text.ActualHeight}"),
                () => Assert.Same(Brush("DataGridRowForeground"), cell.Foreground),
                () => Assert.Equal(14d, cell.FontSize));
        });
    }

    /// <summary>
    /// Selection, read where it is painted and where it has to land. The fill is the framework row's own work, driven
    /// by the retinted AccentBrush name (FrameworkRetints.jalxaml) rather than by a part of ours, so the fact is that
    /// the row fills with the application accent instance and not with the emerald gradient; the text colour is our
    /// cell's work, because the framework's row-level TextOnAccent is outranked by the cell's own Foreground.
    /// </summary>
    [Fact]
    public void The_selected_row_fills_through_the_frameworks_own_name_and_the_cell_takes_the_text_on_accent_colour()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Grid(rows: 2));
            var rows = All<DataGridRow>(grid);
            grid.SelectedItem = rows[1].Item;
            PixelHarness.Settle(20);

            var restBorder = (Border)Part(rows[0], "PART_RowBorder");
            var selectedBorder = (Border)Part(rows[1], "PART_RowBorder");
            var cell = FirstCell(rows[1]);
            var restCell = FirstCell(rows[0]);
            var text = PixelHarness.Descendant<TextBlock>(cell);
            Assert.Multiple(
                () => Assert.Equal(rows[1].Item, grid.SelectedItem),
                () => Assert.True(rows[1].IsSelected),
                () => Assert.False(rows[0].IsSelected),
                () => Assert.Same(Brush("AccentBrush"), selectedBorder.Background),
                () => Assert.Same(Brush("AccentFillColorDefaultBrush"), selectedBorder.Background),
                () => Assert.IsNotType<LinearGradientBrush>(selectedBorder.Background),
                () => Assert.NotSame(Brush("AccentBrush"), restBorder.Background),
                () => Assert.Same(Brush("DataGridRowSelectedForeground"), cell.Foreground),
                () => Assert.NotSame(Brush("DataGridRowSelectedForeground"), restCell.Foreground),
                () => Assert.Equal("Row 2", text?.Text),
                () => Assert.Same(Brush("DataGridRowSelectedForeground"), text!.Foreground));
        });
    }

    /// <summary>
    /// The accent override has to reach the selected row, and it only does because the retint aliases the same brush
    /// instance instead of redefining one. This is the A2 reading the alias layer has been waiting for.
    /// </summary>
    [Fact]
    public void Overriding_the_accent_moves_the_selected_row_fill_through_both_names()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", AccentSentinel);
            try
            {
                Assert.Same(Brush("AccentFillColorDefaultBrush"), Brush("AccentBrush"));
                var grid = Mount(Grid(rows: 2));
                var rows = All<DataGridRow>(grid);
                grid.SelectedItem = rows[1].Item;
                PixelHarness.Settle(20);
                var capture = PixelHarness.Render(grid, 420, 160);
                Assert.Multiple(
                    () => Assert.Same(Brush("AccentBrush"), ((Border)Part(rows[1], "PART_RowBorder")).Background),
                    () => Assert.True(capture.Count(AccentSentinel) > 200,
                        $"the accent override never reached the selected row: top={capture.Top(8)}"));
            }
            finally
            {
                FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
            }
        });
    }

    private static DataGridCell FirstCell(DataGridRow row) => All<DataGridCell>(row).First();

    [Fact]
    public void Alternation_and_the_gridlines_reach_the_parts_that_draw_them()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Grid(rows: 2));
            var rows = All<DataGridRow>(grid);
            var first = (Border)Part(rows[0], "PART_RowBorder");
            var second = (Border)Part(rows[1], "PART_RowBorder");
            var cell = All<DataGridCell>(grid).First();
            var cellBorder = (Border)Part(cell, "PART_CellBorder");
            var header = All<DataGridColumnHeader>(grid).First();
            Assert.Multiple(
                () => Assert.Same(Brush("DataGridRowBackground"), first.Background),
                () => Assert.Same(Brush("DataGridAlternatingRowBackground"), second.Background),
                // The vertical line is ours: the cell border is the one this batch templates. The row's own line is
                // the framework's ControlBorder, which is why no row-thickness row of ours is published.
                () => Assert.Same(Brush("DataGridGridLinesBrush"), cellBorder.BorderBrush),
                () => Assert.Equal((Thickness)Resource("DataGridCellBorderThickness"), cellBorder.BorderThickness),
                () => Assert.Same(Brush("DataGridRowBackground"), cell.Background),
                () => Assert.Same(Brush("DataGridHeaderSeparatorBrush"), header.SeparatorBrush),
                () => Assert.Equal(Visibility.Visible, header.SeparatorVisibility));
        });
    }

    /// <summary>
    /// Two losses, both measured rather than assumed. A disabled host does not carry its IsEnabled down to the
    /// cells and headers on this runtime, so the disabled cells of those styles can only be proven by disabling the
    /// element in its own right; and the host's own Foreground is a LOCAL write the framework makes when it is
    /// disabled (#FFAEAEB2, the same bill TextBox, ComboBox and NumberBox paid - adaptation/00's precedence rule),
    /// which outranks our cell. Both are pinned here instead of dropped, and audits/datagrid.md §4 names them.
    /// </summary>
    [Fact]
    public void A_disabled_element_takes_the_disabled_rows_while_a_disabled_host_only_moves_the_frameworks_local()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Grid(rows: 2));
            var header = All<DataGridColumnHeader>(grid).First();
            var cell = All<DataGridCell>(grid).First();

            cell.IsEnabled = false;
            header.IsEnabled = false;
            PixelHarness.Settle(20);
            Assert.Multiple(
                () => Assert.Same(Brush("DataGridRowForegroundDisabled"), cell.Foreground),
                () => Assert.Same(Brush("DataGridColumnHeaderForegroundDisabled"), header.Foreground),
                () => Assert.Same(
                    Brush("DataGridHeaderBackgroundDisabled"),
                    ((Border)Part(header, "PART_HeaderBorder")).Background));
            cell.IsEnabled = true;
            header.IsEnabled = true;
            PixelHarness.Settle(20);

            grid.IsEnabled = false;
            PixelHarness.Settle(20);
            Assert.Multiple(
                // Host IsEnabled does reach the children's own IsEnabled...
                () => Assert.False(cell.IsEnabled),
                // ...but our disabled cells do not then land on the host path: the framework writes its own
                // #FFAEAEB2 locally, and the cell keeps the resting brush our style set.
                () => Assert.NotSame(Brush("DataGridRowForegroundDisabled"), cell.Foreground),
                () => Assert.NotSame(Brush("DataGridRowForegroundDisabled"), grid.Foreground),
                () => Assert.Equal(Color.FromRgb(0xAE, 0xAE, 0xB2), Assert.IsType<SolidColorBrush>(grid.Foreground).Color));
            grid.IsEnabled = true;
            PixelHarness.Settle(20);
        });
    }

    /// <summary>
    /// The surface token has to reach pixels, not just the brush property - the reason this file's colour facts
    /// started as read-backs is that two earlier batches shipped rows that read correctly and painted nothing
    /// (adaptation/00 S1-f). A sentinel on the row's own alias is the cheapest proof the alias is live.
    /// </summary>
    [Fact]
    public void The_grid_surface_paints_its_background_row()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.OverrideBrush("CardBackgroundFillColorDefaultBrush", SurfaceSentinel);
            var capture = PixelHarness.Render(Mount(Grid(rows: 3)), 420, 160);
            FluentThemeManager.OverrideBrush("CardBackgroundFillColorDefaultBrush", null);
            Assert.True(capture.Stable, $"capture never settled: {capture.Top(6)}");
            Assert.True(capture.Count(SurfaceSentinel) > 2_000,
                $"the grid surface row did not reach pixels; top={capture.Top(6)}");
        });
    }

    /// <summary>
    /// The framework's own selection is the brand-emerald gradient, whose two stops measure #1D733C and #2B804A
    /// (s1i-datagrid-tokens-raw.txt §J and docs/astra/adaptation/00 S0-w). Ours paints an accent overlay at 0.4
    /// opacity, so neither raw stop has any business in a capture of a selected grid. This is the fact with teeth:
    /// remove the Template cell from Styles/DataGrid.jalxaml and the row repaints green.
    /// </summary>
    [Fact]
    public void A_selected_grid_never_paints_the_frameworks_emerald_gradient()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Grid(rows: 3));
            grid.SelectedItem = All<DataGridRow>(grid)[1].Item;
            var capture = PixelHarness.Render(grid, 420, 160);
            Assert.True(capture.Stable, $"capture never settled: {capture.Top(8)}");
            var carriers = new List<string>();
            foreach (var node in All<DependencyObject>(grid))
            {
                foreach (var property in new[] { "Foreground", "Background", "BorderBrush", "Fill", "Stroke" })
                {
                    if (Prop(node, property) is not Brush brush)
                    {
                        continue;
                    }

                    var colour = brush is SolidColorBrush solid ? $"#{solid.Color.R:X2}{solid.Color.G:X2}{solid.Color.B:X2}" : "-";
                    if (brush is LinearGradientBrush || colour is "#1D733C" or "#2B804A" or "#207245")
                    {
                        carriers.Add($"{node.GetType().Name}'{(node is FrameworkElement element ? element.Name : "")}!{property}={brush.GetType().Name} {colour}");
                    }
                }
            }
            Assert.True(capture.Count(FrameworkEmeraldDark) == 0,
                $"#1D733C appears {capture.Count(FrameworkEmeraldDark)} times, carriers=[{string.Join(" | ", carriers.Distinct())}], top={capture.Top(10)}");
            Assert.True(capture.Count(FrameworkEmeraldLight) == 0,
                $"#2B804A appears {capture.Count(FrameworkEmeraldLight)} times, top={capture.Top(10)}");
            Assert.Equal(0, capture.Count(BrandEmerald));
        });
    }

    [Fact]
    public void The_grid_repaints_between_the_two_themes()
    {
        // Both branches' surface token is translucent and the two share one RGB (Light #B3FFFFFF, Dark #0DFFFFFF),
        // so a bare crop of the difference proves nothing - see adaptation/06's last section. Each theme is captured
        // on its own opaque page and has to show the card token's composite there.
        var lightPlate = Color.FromRgb(0xF3, 0xF3, 0xF3);
        var darkPlate = Color.FromRgb(0x20, 0x20, 0x20);

        _fixture.Run(() =>
        {
            var light = OnPlate(Grid(), lightPlate);
            var lightInk = SurfaceOver(lightPlate);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var dark = OnPlate(Grid(rows: 3), darkPlate);
            var darkInk = SurfaceOver(darkPlate);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            Assert.True(light.Stable && dark.Stable, $"a capture never settled: {light.Top(6)} / {dark.Top(6)}");
            Assert.NotEqual(lightInk, darkInk);
            Assert.True(light.Count(lightInk) > 2_000,
                $"the light card token did not paint the surface over its own page; ink={lightInk} count={light.Count(lightInk)} top={light.Top(6)}");
            Assert.True(dark.Count(darkInk) > 2_000,
                $"the dark card token did not paint the surface over its own page; ink={darkInk} count={dark.Count(darkInk)} top={dark.Top(6)}");
            Assert.Equal(0, dark.Count(lightInk));
        });
    }

    private static PixelHarness.Sample OnPlate(FrameworkElement subject, Color plate)
    {
        var host = PixelHarness.Backdrop(subject, plate);
        PixelHarness.Build(host, 440, 200);
        PixelHarness.Settle(60);
        return PixelHarness.Render(host, 440, 200);
    }

    private static Color SurfaceOver(Color plate) =>
        PixelHarness.Over(plate, Assert.IsType<SolidColorBrush>(Brush("DataGridBackground")).Color);

    /// <summary>
    /// The instrument fact underneath every dark-theme pixel claim in this suite, pinned where it was first met.
    /// This palette's dark card surface is a translucent white, and a capture reads only the RGB bytes: over real
    /// content the composite is exact (#202020 page and magenta both come back as the source-over result), but where
    /// nothing lies behind, the pixel keeps white RGB with a small alpha and the capture reports <c>#FFFFFF</c>.
    /// Measured 2026-09-21 after a bare reading of that kind had been filed as "the dark grid paints white" - the
    /// claim is withdrawn, the cause is the capture (<c>adaptation/06</c>).
    /// </summary>
    [Fact]
    public void A_dark_surface_token_only_reads_true_over_a_matching_backdrop()
    {
        var page = Color.FromRgb(0x20, 0x20, 0x20);

        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var token = Assert.IsType<SolidColorBrush>(Brush("CardBackgroundFillColorDefaultBrush")).Color;

            var bare = Mount(Grid());
            var noBackdrop = PixelHarness.PixelAt(Part(bare, "PART_OuterBorder"), 200, 60);

            var host = new Border
            {
                Width = 440,
                Height = 200,
                Background = new SolidColorBrush(page),
                Child = Grid(),
            };
            PixelHarness.Build(host, 440, 200);
            PixelHarness.Settle(60);
            var withBackdrop = PixelHarness.PixelAt(host, 220, 120);

            // A grey page would let a renderer that drops the alpha byte pass by accident (white over grey is still
            // grey-ish). Magenta cannot: only a real source-over composite turns #FF00FF into #FF0DFF.
            var tinted = new Border
            {
                Width = 440,
                Height = 200,
                Background = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0xFF)),
                Child = Grid(),
            };
            PixelHarness.Build(tinted, 440, 200);
            PixelHarness.Settle(60);
            var overMagenta = PixelHarness.PixelAt(tinted, 220, 120);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            var grey = (uint)((page.R << 16) | (page.G << 8) | page.B);
            Assert.True(withBackdrop == Over(page, token),
                $"the grid surface reads {PixelHarness.Hex(withBackdrop)} over a {PixelHarness.Hex(grey)} page, " +
                $"not the card token's composite {PixelHarness.Hex(Over(page, token))} (token #{token.A:X2}{token.R:X2}{token.G:X2}{token.B:X2})");
            Assert.True(overMagenta == Over(Color.FromRgb(0xFF, 0x00, 0xFF), token),
                $"the grid surface reads {PixelHarness.Hex(overMagenta)} over magenta, " +
                $"not {PixelHarness.Hex(Over(Color.FromRgb(0xFF, 0x00, 0xFF), token))} - the alpha byte is not reaching the capture");
            Assert.True(withBackdrop != overMagenta,
                "both backdrops read the same, so the surface is not compositing over them at all");
            Assert.Equal(0xFF_FFFFul, noBackdrop);
        });
    }

    /// <summary>The sRGB-over-opaque composite a translucent brush produces, in the byte-quantised form this renderer writes.</summary>
    private static uint Over(Color backdrop, Color ink)
    {
        static byte Blend(byte back, byte front, double alpha) => (byte)Math.Round(back + (front - back) * alpha);
        var alpha = ink.A / 255d;
        return (uint)(Blend(backdrop.R, ink.R, alpha) << 16 | Blend(backdrop.G, ink.G, alpha) << 8 | Blend(backdrop.B, ink.B, alpha));
    }

    /// <summary>
    /// The row-header gutter must not speak. This runtime hands the row's own data item to the row header's Content
    /// (full path in the probe log: PART_RowsHost &gt; DataGridRow &gt; PART_CellsPanel &gt; DataGridRowHeader &gt;
    /// PART_RowHeaderBorder &gt; ContentPresenter), so a presenter in that template paints the model's ToString
    /// inside 20 DIP - which is what the Gallery's grid did from the moment the DataGrid batch shipped, on a page no
    /// gate had ever captured. The framework still owns the Content, so the claim is about what reaches the tree.
    /// </summary>
    [Fact]
    public void The_row_header_carries_the_item_but_draws_no_text_of_its_own()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Grid());
            var headers = All<DataGridRowHeader>(grid).ToList();
            Assert.NotEmpty(headers);
            Assert.All(headers, header =>
            {
                Assert.IsType<Row>(Prop(header, "Content"));
                Assert.Empty(All<ContentPresenter>(header));
                Assert.Empty(All<TextBlock>(header));
            });

            // Read across the whole grid as well: the gutter is the framework's own child, so a regression can only
            // show up as model text somewhere in the subtree.
            Assert.DoesNotContain(All<TextBlock>(grid), text => text.Text is { } value && value.Contains("Value =", StringComparison.Ordinal));
        });
    }

    /// <summary>
    /// Two silent failures this family has to author around, and neither one shows up in a build. A bare number on a
    /// column's Width does not convert to that property's DataGridLength type: the parse succeeds, the value reads
    /// back as Auto, and the columns then take the control's own Auto share - the Gallery shipped 200/140/120 markup
    /// that never applied. MinWidth is a plain number and does land, which is what the page uses now. The
    /// code-constructed grid every other test in this file mounts goes through the property directly and was
    /// unaffected, which is exactly why no earlier reading caught it (spike/DataGridProbe datagrid-probe-cols.txt).
    /// </summary>
    [Fact]
    public void A_column_width_written_in_markup_is_silently_auto_while_min_width_lands()
    {
        _fixture.Run(() =>
        {
            const string WidthMarkup = """
                <DataGrid xmlns='http://schemas.jalium.ui/2024' AutoGenerateColumns='False'>
                  <DataGrid.Columns>
                    <DataGridTextColumn Header='Alpha' Binding='{Binding Name}' Width='200' />
                  </DataGrid.Columns>
                </DataGrid>
                """;
            const string MinimumMarkup = """
                <DataGrid xmlns='http://schemas.jalium.ui/2024' AutoGenerateColumns='False'>
                  <DataGrid.Columns>
                    <DataGridTextColumn Header='Alpha' Binding='{Binding Name}' MinWidth='200' />
                  </DataGrid.Columns>
                </DataGrid>
                """;

            var written = (DataGrid)XamlReader.Parse(WidthMarkup)!;
            var floored = (DataGrid)XamlReader.Parse(MinimumMarkup)!;
            Assert.Multiple(
                () => Assert.Equal("Auto", written.Columns[0].Width.ToString()),
                () => Assert.Equal(20d, written.Columns[0].MinWidth),
                () => Assert.Equal(200d, floored.Columns[0].MinWidth));
        });
    }

    // ---------- plumbing ----------
    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);
    private static readonly Color FrameworkEmeraldDark = Color.FromRgb(0x1D, 0x73, 0x3C);
    private static readonly Color FrameworkEmeraldLight = Color.FromRgb(0x2B, 0x80, 0x4A);
    private static readonly Color SurfaceSentinel = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color AccentSentinel = Color.FromRgb(0x00, 0x80, 0xFF);

    private static DataGrid Grid(int rows = 2)
    {
        var grid = new DataGrid { Width = 420, Height = 160, AutoGenerateColumns = false };
        grid.Columns.Add(new DataGridTextColumn { Header = "Alpha", Width = 160, Binding = new Binding("Name") });
        grid.Columns.Add(new DataGridTextColumn { Header = "Beta", Width = 160, Binding = new Binding("Value") });
        grid.ItemsSource = Enumerable.Range(1, rows).Select(index => new Row($"Row {index}", index)).ToList();
        return grid;
    }

    private sealed record Row(string Name, int Value);

    private static DataGrid Mount(DataGrid grid)
    {
        PixelHarness.Build(grid, 440, 200);
        PixelHarness.Settle(60);
        return grid;
    }

    private static FrameworkElement Part(DependencyObject root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name}.");

    private static IReadOnlyList<T> All<T>(DependencyObject root) where T : DependencyObject
    {
        var found = new List<T>();
        void Walk(DependencyObject? node)
        {
            if (node is null)
            {
                return;
            }

            if (node is T match)
            {
                found.Add(match);
            }

            var count = VisualTreeHelper.GetChildrenCount(node);
            for (var index = 0; index < count; index++)
            {
                Walk(VisualTreeHelper.GetChild(node, index));
            }
        }

        Walk(root);
        return found;
    }

    private static ControlTemplate TemplateOf(Style style)
    {
        foreach (var setter in style.Setters.OfType<Setter>())
        {
            if (Equals(setter.Property, Control.TemplateProperty) && setter.Value is ControlTemplate template)
            {
                return template;
            }
        }

        throw new InvalidOperationException("The style carries no template.");
    }

    private static object Resource(string key) => Application.Current!.TryFindResource(key)!;

    private static Brush Brush(string key) => (Brush)Resource(key)!;

    private static object? Prop(DependencyObject target, string name) =>
        target.GetType().GetProperty(name)?.GetValue(target);

}
