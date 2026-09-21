using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Data;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// The hierarchical grid, stage 5's sixth slice. It was kept out of the library by one sentence in
/// <c>audits/datagrid.md</c> §4-5 - "TreeDataGridNode is internal, so how hierarchical data gets in is unmeasured" -
/// and that sentence decided a consumer cannot build the control's data, the control cannot be styled, galleryed or
/// tested. Measured against the shipped 26.10.9 assembly the inference was wrong: the feed is a plain
/// <see cref="IEnumerable" /> plus <c>ChildrenPropertyPath</c> (a string), the node type stays private and is never
/// named by a consumer, and expand/collapse is public API (ExpandAll, CollapseAll, IsExpanded(int), FlattenedCount).
/// So this file can exist, and it tests the feed rather than assuming it (docs/astra/audits/treedatagrid.md §1).
/// <para>
/// What the templates own is the host only. The row is the container the control builds cells into, the same layer
/// the DataGrid batch measured as un-retemplatable, and the control additionally writes its own local Background on
/// every row, so a row style of ours could not have moved the fill either way. The cell, the column header and the
/// row header are the DataGrid family's own styles - this runtime has no tree-only versions of them - and one test
/// below proves the sharing rather than asserting it.
/// </para>
/// <para>
/// The zero-width finding is the other half of this batch and it is a framework layout rule, not a styling choice: a
/// tree column left at Auto measures to width 0, so its text never reaches a pixel, while the plain grid distributes
/// Auto into real widths. Because a bare number in markup silently fails to convert to the column's DataGridLength
/// type (see <c>AstraDataGridTests</c>), <c>MinWidth</c> is the only lever a markup author has on this control - the
/// test that pins that pair is the reason the Gallery page reads the way it does.
/// </para>
/// </summary>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraTreeDataGridTests : IDisposable
{
    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraTreeDataGridTests(AstraThemeRuntimeFixture fixture)
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

    // ---------- the feed, measured rather than inferred ----------

    [Fact]
    public void A_flat_list_and_a_property_path_are_the_whole_feed()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Tree());
            var rows = All<TreeDataGridRow>(grid).ToList();

            Assert.Multiple(
                // Two roots, and only two: children stay in the node graph until a row is expanded, so the row
                // count is the reading that says the path landed rather than the item count.
                () => Assert.Equal(2, rows.Count),
                () => Assert.Equal(2, (int)Prop(grid, "FlattenedCount")!),
                () => Assert.Same(grid.Template, TemplateOf(FluentThemeManager.GetStyle("DefaultTreeDataGridStyle")!)));
        });
    }

    /// <summary>
    /// The teeth of this file, and the reason the control is in the library at all. Expanding is a public call, so
    /// the cycle is driven without OS input the way the ToggleButton batch drives a toggle - and the claim is about
    /// the children's <c>TextBlock</c> having a real width under our host template, not about a counter moving.
    /// </summary>
    [Fact]
    public void Expanding_is_a_public_call_and_the_children_realise_text_that_has_a_width()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Tree());
            Assert.Equal(2, All<TreeDataGridRow>(grid).Count());

            grid.ExpandAll();
            PixelHarness.Settle(60);
            var opened = All<TreeDataGridRow>(grid).ToList();
            Assert.Multiple(
                () => Assert.Equal(6, opened.Count),
                () => Assert.Equal(6, (int)Prop(grid, "FlattenedCount")!),
                () => Assert.True(grid.IsExpanded(0), "row 0 stayed collapsed after ExpandAll"));

            var texts = RealisedText(grid);
            Assert.Multiple(
                () => Assert.Contains("Astra gates", texts),
                () => Assert.Contains("Gallery pages", texts),
                () => Assert.Contains("Pixel harness", texts),
                // A row that painted at zero width would satisfy every count above and show nothing, so the widths
                // are read on the same elements the names came from.
                () => Assert.All(TextBlocks(grid), text => Assert.True(text.ActualWidth > 0, $"\"{text.Text}\" has no width")));

            grid.CollapseAll();
            PixelHarness.Settle(60);
            Assert.Equal(2, All<TreeDataGridRow>(grid).Count());
        });
    }

    [Fact]
    public void A_child_row_is_indented_further_in_than_its_parent()
    {
        _fixture.Run(() =>
        {
            var grid = Tree();
            Assert.Equal(16d, (double)Prop(grid, "IndentSize")!, 3);
            grid.ExpandAll();
            Mount(grid);

            var parent = TextBlocks(grid).First(text => text.Text == "Phase 5 gate list");
            var child = TextBlocks(grid).First(text => text.Text == "Astra gates");
            Assert.True(
                child.TranslatePoint(new Point(), grid).X > parent.TranslatePoint(new Point(), grid).X,
                $"child at {child.TranslatePoint(new Point(), grid).X} is not right of parent at {parent.TranslatePoint(new Point(), grid).X}");
        });
    }

    // ---------- the layout rule that decides whether anything is visible ----------

    /// <summary>
    /// The pair that shaped the Gallery markup. Read in spike/DataGridProbe across seven configurations
    /// (datagrid-probe-cols.txt): a tree column authored with <c>Width='200'</c> silently keeps Auto - the number does
    /// not convert to DataGridLength - and the tree then lays that Auto column at width 0, so the cells host their
    /// text at zero width and the control looks empty. The plain grid distributes Auto instead, which is why the same
    /// markup failed on one grid and not the other. <c>MinWidth</c> is a plain number, lands, and both grids lay it
    /// out. The property, the cell width and the realised text are all asserted: two of the three can be true while
    /// the control still paints nothing.
    /// </summary>
    [Fact]
    public void A_tree_column_at_auto_lays_out_at_zero_width_while_min_width_reaches_the_cells()
    {
        _fixture.Run(() =>
        {
            const string WidthMarkup = """
                <TreeDataGrid xmlns='http://schemas.jalium.ui/2024' Height='160' Width='440'
                              TreeColumnIndex='0' ChildrenPropertyPath='Children'>
                  <TreeDataGrid.Columns>
                    <DataGridTextColumn Header='Name' Binding='{Binding Name}' Width='200' />
                  </TreeDataGrid.Columns>
                </TreeDataGrid>
                """;
            const string MinimumMarkup = """
                <TreeDataGrid xmlns='http://schemas.jalium.ui/2024' Height='160' Width='440'
                              TreeColumnIndex='0' ChildrenPropertyPath='Children'>
                  <TreeDataGrid.Columns>
                    <DataGridTextColumn Header='Name' Binding='{Binding Name}' MinWidth='200' />
                  </TreeDataGrid.Columns>
                </TreeDataGrid>
                """;

            var authored = (TreeDataGrid)XamlReader.Parse(WidthMarkup)!;
            authored.ItemsSource = Nodes();
            Mount(authored);
            var floored = (TreeDataGrid)XamlReader.Parse(MinimumMarkup)!;
            floored.ItemsSource = Nodes();
            Mount(floored);

            var authoredCell = FirstCell(authored);
            var flooredCell = FirstCell(floored);
            Assert.Multiple(
                () => Assert.Equal("Auto", authored.Columns[0].Width.ToString()),
                () => Assert.True(authoredCell.ActualWidth < 1, $"the Auto column laid out at {authoredCell.ActualWidth}"),
                () => Assert.True(flooredCell.ActualWidth >= 200, $"the MinWidth column laid out at {flooredCell.ActualWidth}"),
                () => Assert.True(FirstText(floored).ActualWidth > 0, "the floored grid realised no text width"));
        });
    }

    // ---------- what our style owns, and what the control keeps ----------

    [Fact]
    public void Every_part_the_tree_looks_up_is_present_with_the_type_it_casts_to()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Tree());
            Assert.Multiple(
                () => Assert.IsType<Border>(Part(grid, "PART_OuterBorder")),
                () => Assert.IsType<Grid>(Part(grid, "PART_ColumnHeadersBorder")),
                () => Assert.IsAssignableFrom<ScrollViewer>(Part(grid, "PART_ColumnHeadersScrollViewer")),
                () => Assert.IsType<StackPanel>(Part(grid, "PART_ColumnHeadersHost")),
                () => Assert.IsAssignableFrom<ScrollViewer>(Part(grid, "PART_DataScrollViewer")),
                () => Assert.IsType<StackPanel>(Part(grid, "PART_RowsHost")),
                () => Assert.IsType<Canvas>(Part(grid, "PART_DragOverlay")));
        });
    }

    /// <summary>
    /// The tree has no cell or header of its own, so the DataGrid family's styles have to reach it - otherwise this
    /// control would take the framework's emerald selection gradient and its 30 DIP rows back. The template identity
    /// is the proof on the cell; on the header the measurable consequence of the same style is the 32 DIP floor.
    /// </summary>
    [Fact]
    public void The_tree_shares_the_cell_and_column_header_styles_of_the_plain_grid()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Tree());
            var cell = FirstCell(grid);
            var header = All<DataGridColumnHeader>(grid).First();

            Assert.Multiple(
                () => Assert.Same(
                    TemplateOf(FluentThemeManager.GetStyle("DefaultDataGridCellStyle")!),
                    cell.Template),
                () => Assert.Equal(32d, cell.MinHeight, 3),
                () => Assert.Equal(32d, header.MinHeight, 3),
                () => Assert.Same(Brush("DataGridRowForeground"), cell.Foreground),
                // The row itself is not ours: its fill is a local value the control writes, which is the same
                // precedence wall the DataGrid batch recorded, read here on the tree's own container.
                () => Assert.NotEqual(
                    DependencyProperty.UnsetValue,
                    All<TreeDataGridRow>(grid).First().ReadLocalValue(Control.BackgroundProperty)));
        });
    }

    [Fact]
    public void Alternating_rows_come_through_on_the_tree_too()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Tree());
            var rows = All<TreeDataGridRow>(grid).ToList();
            Assert.Equal(2, rows.Count);

            var first = (Brush)Prop(rows[0], "Background")!;
            var second = (Brush)Prop(rows[1], "Background")!;
            Assert.Multiple(
                () => Assert.NotSame(first, second),
                () => Assert.Same(Brush("DataGridAlternatingRowBackground"), second));
        });
    }

    [Fact]
    public void The_tree_surface_is_our_card_with_our_radius_and_our_line()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Tree());
            var outer = (Border)Part(grid, "PART_OuterBorder");

            Assert.Multiple(
                () => Assert.Same(Brush("DataGridBackground"), outer.Background),
                () => Assert.Same(Brush("DataGridBorderBrush"), outer.BorderBrush),
                () => Assert.Equal((CornerRadius)Resource("ControlCornerRadius"), outer.CornerRadius),
                () => Assert.Equal(1d, grid.BorderThickness.Left, 3));
        });
    }

    // ---------- pixels ----------

    [Fact]
    public void A_selected_tree_row_never_paints_the_frameworks_emerald_gradient()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Tree());
            grid.ExpandAll();
            PixelHarness.Settle(60);
            grid.SelectedIndex = 1;
            PixelHarness.Settle(60);

            var selected = All<TreeDataGridRow>(grid).First(row => row.IsSelected);
            var capture = PixelHarness.Render(selected, (int)selected.ActualWidth, (int)selected.ActualHeight);
            var carriers = new List<string>();
            void Walk(DependencyObject? node)
            {
                if (node is null)
                {
                    return;
                }

                if (node is Control or Border or Panel)
                {
                    var brush = Prop(node, "Background") as LinearGradientBrush;
                    if (brush?.GradientStops is { Count: > 1 } stops)
                    {
                        carriers.Add($"{node.GetType().Name} {string.Join("..", stops.Select(stop => $"{stop.Color.R}, {stop.Color.G}, {stop.Color.B}"))}");
                    }
                }

                for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
                {
                    Walk(VisualTreeHelper.GetChild(node, index));
                }
            }

            Walk(selected);
            Assert.Multiple(
                () => Assert.True(capture.PaintedPixels > 400, $"the row painted nothing: {capture.Top(6)}"),
                () => Assert.Equal(0, capture.Count(FrameworkEmeraldDark)),
                () => Assert.Equal(0, capture.Count(FrameworkEmeraldLight)),
                () => Assert.Equal(0, capture.Count(BrandEmerald)),
                () => Assert.DoesNotContain(carriers, carrier => carrier.Contains("29, 115, 60", StringComparison.Ordinal) || carrier.Contains("43, 128, 74", StringComparison.Ordinal)));
        });
    }

    [Fact]
    public void The_tree_repaints_between_the_two_themes()
    {
        // Same instrument rule as the DataGrid twin: the surface token is translucent and its two themes share one
        // RGB, so each theme is captured on its own opaque page and has to show the card token's composite there
        // (docs/astra/adaptation/06-pixel-attribution.md, last section).
        var lightPlate = Color.FromRgb(0xF3, 0xF3, 0xF3);
        var darkPlate = Color.FromRgb(0x20, 0x20, 0x20);

        _fixture.Run(() =>
        {
            var light = OnPlate(Tree(), lightPlate);
            var lightInk = SurfaceOver(lightPlate);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            var dark = OnPlate(Tree(), darkPlate);
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
        PixelHarness.Over(plate, ((SolidColorBrush)Application.Current!.TryFindResource("DataGridBackground")!).Color);

    /// <summary>
    /// The host's two metrics have to travel through the control, not just sit on it: the row height is a value the
    /// control writes onto every generated row as a local <c>Height</c>, so a setter landing on the grid proves
    /// nothing about the pixels until a row is measured. Both halves are read, and the row's own local value is
    /// asserted to be the number we asked for rather than the runtime's resting 30.
    /// </summary>
    [Fact]
    public void The_row_and_header_heights_we_set_are_the_heights_the_parts_get()
    {
        _fixture.Run(() =>
        {
            var grid = Mount(Tree());
            var row = All<TreeDataGridRow>(grid).First();
            var header = All<DataGridColumnHeader>(grid).First();

            Assert.Multiple(
                () => Assert.Equal(32d, (double)Prop(grid, "RowHeight")!, 3),
                () => Assert.Equal(32d, (double)Prop(grid, "ColumnHeaderHeight")!, 3),
                () => Assert.Equal(32d, row.ActualHeight, 1),
                () => Assert.Equal(32d, header.ActualHeight, 1));
        });
    }

    // ---------- plumbing ----------

    private static readonly Color BrandEmerald = Color.FromRgb(0x20, 0x72, 0x45);
    private static readonly Color FrameworkEmeraldDark = Color.FromRgb(0x1D, 0x73, 0x3C);
    private static readonly Color FrameworkEmeraldLight = Color.FromRgb(0x2B, 0x80, 0x4A);

    private static TreeDataGrid Tree()
    {
        var grid = new TreeDataGrid
        {
            Width = 440,
            Height = 160,
            TreeColumnIndex = 0,
            ChildrenPropertyPath = "Children",
        };
        grid.Columns.Add(new DataGridTextColumn { Header = "Alpha", Width = 200, Binding = new Binding("Name") });
        grid.Columns.Add(new DataGridTextColumn { Header = "Owner", Width = 140, Binding = new Binding("Owner") });
        grid.ItemsSource = Nodes();
        return grid;
    }

    private static List<Node> Nodes() =>
    [
        new("Phase 5 gate list", "Lince", [new("Astra gates", "Lince", []), new("Gallery pages", "Lince", [new("Pixel harness", "Lince", [])])]),
        new("Hardware input evidence", "Lince", [new("Pointer capture", "Lince", [])]),
    ];

    private sealed record Node(string Name, string Owner, IReadOnlyList<Node> Children);

    private static TreeDataGrid Mount(TreeDataGrid grid)
    {
        PixelHarness.Build(grid, 460, 220);
        PixelHarness.Settle(60);
        return grid;
    }

    private static FrameworkElement Part(DependencyObject root, string name) =>
        PixelHarness.Named(root, name) ?? throw new InvalidOperationException($"No part named {name}.");

    private static DataGridCell FirstCell(DependencyObject root) => All<DataGridCell>(root).First();

    private static TextBlock FirstText(DependencyObject root) => TextBlocks(root).First();

    private static IReadOnlyList<TextBlock> TextBlocks(DependencyObject root) =>
        All<TextBlock>(root).Where(text => !string.IsNullOrEmpty(text.Text)).ToList();

    private static IReadOnlyList<string> RealisedText(DependencyObject root) =>
        TextBlocks(root).Where(text => text.ActualWidth > 0).Select(text => text.Text!).ToList();

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
