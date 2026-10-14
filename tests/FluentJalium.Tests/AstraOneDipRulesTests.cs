using System.Xml.Linq;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Data;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

/// <summary>
/// The five 1-DIP separator rules upstream draws as <c>Rectangle</c>, checked where they are supposed to be visible.
/// </summary>
/// <remarks>
/// <para>
/// The runtime fact is measured and pinned in <see cref="AstraDividerTests"/>: a 1-DIP <c>Rectangle</c> inks zero
/// pixels on this renderer at any alignment, the same box as a <c>Border</c> inks 600, and <c>Rectangle</c> only
/// recovers at 4 DIP (<c>adaptation/00</c> S1-q clause 2). What this file adds is the witness at the shipped sites -
/// <c>Styles/AppBar.jalxaml</c>, <c>Styles/DataGrid.jalxaml</c> (three rules), <c>Styles/TreeDataGrid.jalxaml</c> -
/// because "the shape cannot print" and "this control's rule is therefore invisible" are different claims, and only
/// the second one is what a user sees.
/// </para>
/// <para>
/// The witness is differential on purpose. An absolute colour assertion was tried first and is wrong twice over:
/// these rows are translucent upstream, so the token's own colour never appears in a capture - measured on
/// 2026-09-21, a fixed header rule lands as <c>#9F9F9F</c> over its strip and the app bar separator as
/// <c>#F1F1F1</c>, both products of the backdrop behind them and neither nameable in advance. Each test therefore
/// collapses the realized rules and counts how much ink each colour loses: put the shape back to a 1-DIP
/// <c>Rectangle</c> and every reading drops to 0 with the two histograms identical, which is how the three witnesses
/// read against the pre-fix markup. The theme axis is part of the claim, not decoration - the two grids pair
/// <c>#29000000</c> and <c>#18FFFFFF</c>, and both print the same 338 pixels on a card matched to the theme.
/// </para>
/// </remarks>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraOneDipRulesTests : IDisposable
{
    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraOneDipRulesTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
        SetTheme(FluentThemeVariant.Light);
    }

    public void Dispose() => SetTheme(FluentThemeVariant.Light);

    [Fact]
    public void No_shipped_template_draws_a_one_dip_rectangle()
    {
        var offenders = new List<string>();
        foreach (var file in Directory.EnumerateFiles(
            Path.Combine(RepositoryRoot(), "src", "FluentJalium"), "*.jalxaml", SearchOption.AllDirectories))
        {
            foreach (var element in XDocument.Load(file).Descendants())
            {
                if (element.Name.LocalName != "Rectangle") continue;
                if ((string?)element.Attribute("Width") != "1" && (string?)element.Attribute("Height") != "1") continue;
                var name = element.Attributes().FirstOrDefault(static attribute => attribute.Name.LocalName == "Name")?.Value;
                offenders.Add($"{Path.GetRelativePath(RepositoryRoot(), file).Replace('\\', '/')} {name ?? "(unnamed)"}");
            }
        }

        // The route is banned rather than the current count being pinned: even if a later renderer learned to print
        // thin rectangles, this layer's separators stay Bordered, because Border is the shape measured to paint.
        Assert.Empty(offenders);
    }

    [Theory]
    [InlineData("DataGrid", FluentThemeVariant.Light)]
    [InlineData("DataGrid", FluentThemeVariant.Dark)]
    [InlineData("TreeDataGrid", FluentThemeVariant.Light)]
    [InlineData("TreeDataGrid", FluentThemeVariant.Dark)]
    public void The_header_rules_paint_a_band_the_strip_cannot_explain_by_itself(string kind, FluentThemeVariant variant)
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(variant);
            FluentThemeManager.ApplyAccent(null);
            var subject = kind == "DataGrid" ? (FrameworkElement)Grid(variant) : Tree(variant);
            var band = Vanished(subject, 440, 200);

            // Measured 2026-09-21: every grid case prints 338 pixels of its line's composite, in both themes (the
            // light stroke pairs #18FFFFFF over the dark card the same way #29000000 pairs over the light one). The
            // floor sits under that and far above the 0 the same witness reads with the rules back to 1-DIP
            // Rectangles; the ceiling keeps a re-layout - a header that stops painting - from passing as this claim.
            Assert.True(band is >= 250 and <= 1_200,
                $"the {kind} header rules in {variant} account for {band} pixels, which is not their line\n{_diagnostic}");
        });
    }

    [Theory]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.Dark)]
    public void The_app_bar_separator_paints_a_column_the_cell_cannot_explain_by_itself(FluentThemeVariant variant)
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ApplyTheme(variant);
            FluentThemeManager.ApplyAccent(null);
            var band = Vanished(Separator(variant), 68, 64);

            // Measured 2026-09-21: 64 pixels - the rule is 32 DIP tall inside the 48 cell once its 2,8,2,8 margin is
            // spent, and at this scale a 1-DIP column straddles two device columns, so the ink is 32 x 2. The ceiling
            // keeps a re-layout from passing as ink.
            Assert.True(band is >= 30 and <= 200,
                $"the app bar separator in {variant} accounted for {band} pixels, which is not its column\n{_diagnostic}");
        });
    }

    /// <summary>
    /// Ink that only exists while the rules are visible: capture, collapse every realized 1-DIP rule in the subtree,
    /// capture again, and sum how much each colour's pixel count dropped. Zero means no rule reached the screen.
    /// </summary>
    /// <remarks>
    /// The first version of this counted colours that disappeared outright, and it was the wrong instrument: the
    /// composite the rule lands on, #9F9F9F on a white header strip, is also produced by glyph antialiasing
    /// elsewhere in the crop, so the key never leaves the histogram and a working 438-pixel band read as 2 pixels.
    /// Per-key delta is both what the claim is about and what survives that coincidence.
    /// </remarks>
    private int Vanished(FrameworkElement subject, int width, int height)
    {
        PixelHarness.Build(subject, width, height);
        PixelHarness.Settle(60);

        // Every band in the subtree, not the first one: the DataGrid header strip and the row-header corner share one
        // composited colour, so collapsing a single part leaves that colour standing and the delta reads zero.
        var rules = Rules(subject);
        Assert.True(rules.Count >= 1, "no 1-DIP rule in the realized tree");

        var withRule = PixelHarness.Render(subject, width, height);
        Assert.True(withRule.Stable, $"the capture never settled: {withRule.Top(6)}");

        foreach (var rule in rules) rule.Visibility = Visibility.Collapsed;
        PixelHarness.Settle(30);
        var withoutRule = PixelHarness.Render(subject, width, height);

        var vanished = withRule.Histogram
            .Sum(entry => Math.Max(0, entry.Value - withoutRule.Histogram.GetValueOrDefault(entry.Key)));

        // Carried into the failure messages below: a red pixel claim without its own instrument reading costs a
        // second cycle to diagnose, and the second reading is usually the one that names the real cause.
        _diagnostic = $"rules: {string.Join(" | ", rules.Select(Describe))}\n" +
            $"with: {withRule.Top(5)}\nwithout: {withoutRule.Top(5)}";

        foreach (var rule in rules) rule.Visibility = Visibility.Visible;
        return vanished;
    }

    private string _diagnostic = string.Empty;

    private static string Describe(FrameworkElement element) =>
        $"{element.GetType().Name} {element.ActualWidth:F1}x{element.ActualHeight:F1} " +
        $"fill={element switch { Border border => border.Background?.ToString(), _ => "n/a" }}";

    /// <summary>
    /// The rule parts, found by the box the markup declares - one DIP in one dimension - rather than by measured
    /// geometry, which a scroll bar's own track can also satisfy. A named lookup is not available: these parts carry
    /// no name upstream.
    /// </summary>
    private static List<FrameworkElement> Rules(DependencyObject root) =>
        Descendants(root).OfType<FrameworkElement>()
            .Where(element => element is Border or Jalium.UI.Shapes.Rectangle)
            .Where(static element => element is { Width: 1, ActualHeight: >= 1 } or { Height: 1, ActualWidth: >= 1 })
            .ToList();

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            if (VisualTreeHelper.GetChild(root, index) is not { } child) continue;
            yield return child;
            foreach (var deeper in Descendants(child)) yield return deeper;
        }
    }

    private static DataGrid Grid(FluentThemeVariant variant)
    {
        var grid = new DataGrid { Width = 420, Height = 160, AutoGenerateColumns = false, Background = Card(variant) };
        grid.Columns.Add(new DataGridTextColumn { Header = "Alpha", Width = 160, Binding = new Binding("Name") });
        grid.Columns.Add(new DataGridTextColumn { Header = "Beta", Width = 160, Binding = new Binding("Value") });
        grid.ItemsSource = new List<Row> { new("Row 1", 1), new("Row 2", 2) };
        return grid;
    }

    private static TreeDataGrid Tree(FluentThemeVariant variant)
    {
        var grid = new TreeDataGrid
        {
            Width = 420,
            Height = 160,
            TreeColumnIndex = 0,
            ChildrenPropertyPath = "Children",
            Background = Card(variant),
        };
        grid.Columns.Add(new DataGridTextColumn { Header = "Alpha", Width = 200, Binding = new Binding("Name") });
        grid.Columns.Add(new DataGridTextColumn { Header = "Owner", Width = 140, Binding = new Binding("Owner") });
        grid.ItemsSource = new List<Node>
        {
            new("Phase 5 gate list", "Lince", [new("Astra gates", "Lince", [])]),
        };
        return grid;
    }

    /// <summary>
    /// The separator on its own, on a card matched to the theme. A whole <c>CommandBar</c> is not the subject here:
    /// measured in the first run of this file, a bare <c>new CommandBar()</c> captured 12 600 black pixels out of
    /// 12 800, so the bar brings its own unthemed-surface question and would blur this claim about the rule. The
    /// card's own colour matters for the same reason - the dark token is a translucent white, and §S1-q clause 4
    /// measured that shape inking nothing over white.
    /// </summary>
    /// <summary>
    /// The surface a rule composites onto. Upstream leaves the grid header band transparent
    /// (<c>DataGridColumnHeaderBackground</c> is <c>SubtleFillColorTransparentBrush</c>), so the witness names the
    /// card rather than borrowing whatever the harness happens to paint: measured 2026-09-21, a dark-theme grid
    /// mounted without one kept a white crop under the band, and a translucent white stroke over white prints
    /// nothing - which is §S1-q clause 4's trap, not this rule's.
    /// </summary>
    private static SolidColorBrush Card(FluentThemeVariant variant) =>
        new(variant == FluentThemeVariant.Dark ? Color.FromRgb(0x20, 0x20, 0x20) : Colors.White);

    private static Border Separator(FluentThemeVariant variant) => new()
    {
        Width = 68,
        Height = 64,
        Background = Card(variant),
        Child = new AppBarSeparator { Width = 48, Height = 48 },
    };

    private void SetTheme(FluentThemeVariant variant) => _fixture.Run(() =>
    {
        FluentThemeManager.ApplyTheme(variant);
        FluentThemeManager.ApplyAccent(null);
    });

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(System.IO.Path.Combine(directory.FullName, "FluentJalium.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Cannot locate the repository root.");
    }

    private sealed record Row(string Name, int Value);

    private sealed record Node(string Name, string Owner, IReadOnlyList<Node> Children);
}
