using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Pages;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Charts;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentChartControlsTests
{
    private static readonly string[] ChartResourceKeys =
    [
        "ChartBackground",
        "ChartForeground",
        "ChartBorderBrush",
        "ChartTitleForeground",
        "ChartAxisBrush",
        "ChartGridLineBrush",
        "ChartPalette1",
        "ChartPositiveBrush",
        "ChartNegativeBrush",
        "ChartTooltipBackground",
        "ChartTooltipForeground"
    ];

    public static TheoryData<Type, Type> ChartControlPairs =>
        new()
        {
            { typeof(FWLineChart), typeof(LineChart) },
            { typeof(FWBarChart), typeof(BarChart) },
            { typeof(FWPieChart), typeof(PieChart) },
            { typeof(FWScatterPlot), typeof(ScatterPlot) },
            { typeof(FWHeatmap), typeof(Heatmap) },
            { typeof(FWSparkline), typeof(Sparkline) },
            { typeof(FWGaugeChart), typeof(GaugeChart) },
            { typeof(FWTreeMap), typeof(TreeMap) },
            { typeof(FWCandlestickChart), typeof(CandlestickChart) },
            { typeof(FWNetworkGraph), typeof(NetworkGraph) },
            { typeof(FWGanttChart), typeof(GanttChart) },
            { typeof(FWSankeyDiagram), typeof(SankeyDiagram) },
            { typeof(FWChartLegend), typeof(ChartLegend) },
            { typeof(FWChartTooltip), typeof(ChartTooltip) }
        };

    [Theory]
    [MemberData(nameof(ChartControlPairs))]
    public void FWChartControls_ShouldExposeFluentSurface(Type fluentType, Type jaliumType)
    {
        var control = Activator.CreateInstance(fluentType);

        Assert.NotNull(control);
        Assert.IsAssignableFrom(jaliumType, control);
        Assert.IsAssignableFrom<IFluentJaliumControl>(control);
    }

    [Fact]
    public void FWChartControls_ShouldPreserveJaliumChartDefaults()
    {
        var line = new FWLineChart();
        var bar = new FWBarChart();
        var pie = new FWPieChart();
        var sparkline = new FWSparkline();
        var gauge = new FWGaugeChart();
        var treeMap = new FWTreeMap();
        var network = new FWNetworkGraph();
        var gantt = new FWGanttChart();
        var sankey = new FWSankeyDiagram();
        var tooltip = new FWChartTooltip();

        Assert.Empty(line.Series);
        Assert.True(line.ShowDataPoints);
        Assert.Equal(4.0, line.DataPointRadius);
        Assert.Empty(bar.Series);
        Assert.NotNull(pie.Series);
        Assert.Null(sparkline.Values);
        Assert.Equal(0, gauge.Minimum);
        Assert.Equal(100, gauge.Maximum);
        Assert.Empty(treeMap.Items);
        Assert.Empty(network.Nodes);
        Assert.Empty(network.Links);
        Assert.Empty(gantt.Tasks);
        Assert.Empty(sankey.Nodes);
        Assert.Empty(sankey.Links);
        Assert.Null(tooltip.SeriesTitle);
        Assert.Null(tooltip.XValue);
        Assert.Null(tooltip.YValue);
    }

    [Fact]
    public void GalleryChartsPage_ShouldFormatLineChartVisualQaSnapshot()
    {
        var chart = new FWLineChart
        {
            Title = "Pipeline signal",
            Width = 482,
            Height = 220,
            IsTooltipEnabled = true,
            IsAnimationEnabled = false,
            IsZoomEnabled = true,
            IsPanEnabled = true,
            IsGridLinesVisible = true,
            LegendPosition = LegendPosition.Right,
            ChartPalette =
            [
                new SolidColorBrush(Color.FromRgb(0x41, 0x7E, 0xE0)),
                new SolidColorBrush(Color.FromRgb(0x00, 0xBC, 0xD4))
            ]
        };
        chart.Series.Add(new LineSeries
        {
            Title = "Current",
            DataPoints =
            {
                new ChartDataPoint { XValue = "Mon", YValue = 62 },
                new ChartDataPoint { XValue = "Tue", YValue = 70 },
                new ChartDataPoint { XValue = "Wed", YValue = 66 }
            }
        });
        chart.Series.Add(new LineSeries
        {
            Title = "Baseline",
            DataPoints =
            {
                new ChartDataPoint { XValue = "Mon", YValue = 55 },
                new ChartDataPoint { XValue = "Tue", YValue = 58 }
            }
        });

        var snapshot = GalleryChartsPage.CreateChartVisualQaSnapshot(chart);
        var text = GalleryChartsPage.FormatChartVisualQa("Chart visual QA", snapshot);

        Assert.Equal("FWLineChart", snapshot.Chart);
        Assert.Equal("Pipeline signal", snapshot.Title);
        Assert.Equal(482, snapshot.Width);
        Assert.Equal(220, snapshot.Height);
        Assert.Equal(2, snapshot.SeriesCount);
        Assert.Equal(5, snapshot.DataPointCount);
        Assert.True(snapshot.HasPalette);
        Assert.Equal(LegendPosition.Right, snapshot.LegendPosition);
        Assert.True(snapshot.IsTooltipEnabled);
        Assert.False(snapshot.IsAnimationEnabled);
        Assert.True(snapshot.HasAxes);
        Assert.True(snapshot.IsGridVisible);
        Assert.True(snapshot.IsZoomEnabled);
        Assert.True(snapshot.IsPanEnabled);

        Assert.Contains("Chart visual QA", text, StringComparison.Ordinal);
        Assert.Contains("Chart: FWLineChart", text, StringComparison.Ordinal);
        Assert.Contains("Title: Pipeline signal", text, StringComparison.Ordinal);
        Assert.Contains("Size: 482x220", text, StringComparison.Ordinal);
        Assert.Contains("Series: 2", text, StringComparison.Ordinal);
        Assert.Contains("Points: 5", text, StringComparison.Ordinal);
        Assert.Contains("Palette: on", text, StringComparison.Ordinal);
        Assert.Contains("Legend: Right", text, StringComparison.Ordinal);
        Assert.Contains("Tooltip: on", text, StringComparison.Ordinal);
        Assert.Contains("Animation: off", text, StringComparison.Ordinal);
        Assert.Contains("Axes: on", text, StringComparison.Ordinal);
        Assert.Contains("Grid: on", text, StringComparison.Ordinal);
        Assert.Contains("Zoom: on", text, StringComparison.Ordinal);
        Assert.Contains("Pan: on", text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeChartTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in ChartResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldDefineChartStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var (fluentType, jaliumType) in ChartControlPairs.Select(pair => ((Type)pair[0], (Type)pair[1])))
        {
            AssertBasedOnStyle(dictionary, fluentType, jaliumType);
        }

        var lineStyle = ResolveStyleDefinition(AssertStyle<LineChart>(dictionary));
        AssertSetter(lineStyle, Control.BackgroundProperty);
        AssertSetter(lineStyle, Control.BorderBrushProperty);
        AssertSetter(lineStyle, ChartBase.TitleForegroundProperty);
        AssertSetter(lineStyle, AxisChartBase.AxisBrushProperty);
        AssertSetter(lineStyle, AxisChartBase.GridLineBrushProperty);

        var barStyle = ResolveStyleDefinition(AssertStyle<BarChart>(dictionary));
        AssertSetter(barStyle, BarChart.BarCornerRadiusProperty);

        var sparklineStyle = ResolveStyleDefinition(AssertStyle<Sparkline>(dictionary));
        AssertSetter(sparklineStyle, Sparkline.LineBrushProperty);
        AssertSetter(sparklineStyle, Sparkline.FillBrushProperty);

        var gaugeStyle = ResolveStyleDefinition(AssertStyle<GaugeChart>(dictionary));
        AssertSetter(gaugeStyle, GaugeChart.TrackBrushProperty);
        AssertSetter(gaugeStyle, GaugeChart.NeedleBrushProperty);

        var tooltipStyle = ResolveStyleDefinition(AssertStyle<ChartTooltip>(dictionary));
        AssertSetter(tooltipStyle, Control.BackgroundProperty);
        AssertSetter(tooltipStyle, Control.BorderBrushProperty);
        AssertSetter(tooltipStyle, Control.FontSizeProperty);

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwChartStylesBasedOnJaliumStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            foreach (var (fluentType, jaliumType) in ChartControlPairs.Select(pair => ((Type)pair[0], (Type)pair[1])))
            {
                AssertBasedOnStyle(app.Resources, fluentType, jaliumType);
            }
        }
        finally
        {
            ResetApplicationState();
        }
    }

    private static ResourceDictionary LoadGenericThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
    }

    private static Style AssertStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        return Assert.IsType<Style>(value);
    }

    private static void AssertBasedOnStyle(ResourceDictionary dictionary, Type fluentType, Type jaliumType)
    {
        Assert.True(dictionary.TryGetValue(jaliumType, out var baseValue), $"{jaliumType.Name} base style was not found.");
        var baseStyle = Assert.IsType<Style>(baseValue);

        Assert.True(dictionary.TryGetValue(fluentType, out var fluentValue), $"{fluentType.Name} FW style was not found.");
        var fluentStyle = Assert.IsType<Style>(fluentValue);

        Assert.Equal(fluentType, fluentStyle.TargetType);
        Assert.True(
            ReferenceEquals(baseStyle, fluentStyle.BasedOn)
            || (baseStyle.BasedOn != null && ReferenceEquals(baseStyle.BasedOn, fluentStyle.BasedOn)),
            $"{fluentType.Name} FW style should be based on {jaliumType.Name} or share its keyed base style.");
    }

    private static Style ResolveStyleDefinition(Style style)
    {
        return style.Setters.Count > 0 ? style : style.BasedOn ?? style;
    }

    private static void AssertSetter(Style style, DependencyProperty property)
    {
        Assert.Contains(style.Setters, setter => setter.Property == property);
    }

    private static void ResetApplicationState()
    {
        var currentField = typeof(Application).GetField("_current", BindingFlags.NonPublic | BindingFlags.Static);
        currentField?.SetValue(null, null);

        var jaliumReset = typeof(JaliumThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        jaliumReset?.Invoke(null, null);

        var fluentReset = typeof(FluentThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        fluentReset?.Invoke(null, null);
    }
}
