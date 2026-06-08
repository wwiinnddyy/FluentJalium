using FluentJalium.Icon;
using System.Collections.ObjectModel;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Charts;
using Jalium.UI.Media;
using FWBarChart = FluentJalium.Controls.FWBarChart;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWCandlestickChart = FluentJalium.Controls.FWCandlestickChart;
using FWChartLegend = FluentJalium.Controls.FWChartLegend;
using FWChartTooltip = FluentJalium.Controls.FWChartTooltip;
using FWGaugeChart = FluentJalium.Controls.FWGaugeChart;
using FWGanttChart = FluentJalium.Controls.FWGanttChart;
using FWHeatmap = FluentJalium.Controls.FWHeatmap;
using FWLineChart = FluentJalium.Controls.FWLineChart;
using FWNetworkGraph = FluentJalium.Controls.FWNetworkGraph;
using FWPieChart = FluentJalium.Controls.FWPieChart;
using FWScatterPlot = FluentJalium.Controls.FWScatterPlot;
using FWSankeyDiagram = FluentJalium.Controls.FWSankeyDiagram;
using FWSparkline = FluentJalium.Controls.FWSparkline;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWTreeMap = FluentJalium.Controls.FWTreeMap;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal readonly record struct GalleryChartVisualQaSnapshot(
    string Chart,
    string Title,
    double Width,
    double Height,
    int SeriesCount,
    int DataPointCount,
    bool HasPalette,
    LegendPosition LegendPosition,
    bool IsTooltipEnabled,
    bool IsAnimationEnabled,
    bool HasAxes,
    bool IsGridVisible,
    bool IsZoomEnabled,
    bool IsPanEnabled);

internal sealed class GalleryChartsPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Charts");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateChartCard(
            FluentIconRegular.ChartMultiple24,
            "FWLineChart",
            "Series and DataPoints with category axes, area fill, smoothing, legend, and tooltip state.",
            CreateLineChartSample()));
        examples.Children.Add(CreateChartCard(
            FluentIconRegular.DataHistogram24,
            "FWBarChart",
            "Grouped, stacked, and value-label bar states using BarSeries.DataPoints.",
            CreateBarChartSample()));
        examples.Children.Add(CreateChartCard(
            FluentIconRegular.ChartMultiple24,
            "FWPieChart",
            "PieSeries.DataPoints with donut ratio, labels, slice brushes, and exploded slices.",
            CreatePieChartSample()));
        examples.Children.Add(CreateChartCard(
            FluentIconRegular.DataHistogram24,
            "FWScatterPlot",
            "ScatterSeries.DataPoints with point shapes, trend line state, and numeric axes.",
            CreateScatterPlotSample()));
        examples.Children.Add(CreateChartCard(
            FluentIconRegular.Table24,
            "FWHeatmap",
            "Matrix Data with XLabels, YLabels, color scales, cell borders, and cell value labels.",
            CreateHeatmapSample()));
        examples.Children.Add(CreateChartCard(
            FluentIconRegular.DataUsage24,
            "FWSparkline",
            "Inline Values rendered as line, area, bar, and win/loss signals for dense dashboards.",
            CreateSparklineSample()));
        examples.Children.Add(CreateChartCard(
            FluentIconRegular.ChartMultiple24,
            "FWGaugeChart",
            "Value, range bands, tick marks, and value text for operational scorecards.",
            CreateGaugeChartSample()));
        examples.Children.Add(CreateChartCard(
            FluentIconRegular.BranchFork24,
            "FWTreeMap",
            "TreeMapItem values and child collections with layout algorithm switching.",
            CreateTreeMapSample()));
        examples.Children.Add(CreateChartCard(
            FluentIconRegular.DataHistogram24,
            "FWCandlestickChart",
            "OHLC ItemsSource with volume bars, DateTimeAxis labels, and moving average overlays.",
            CreateCandlestickChartSample()));
        examples.Children.Add(CreateChartCard(
            FluentIconRegular.BranchFork24,
            "FWNetworkGraph",
            "NetworkNode and NetworkLink collections with circular, hierarchical, and force layouts.",
            CreateNetworkGraphSample()));
        examples.Children.Add(CreateChartCard(
            FluentIconRegular.TableResizeColumn24,
            "FWGanttChart",
            "GanttTask collections with progress, milestones, dependencies, and today-line state.",
            CreateGanttChartSample()));
        examples.Children.Add(CreateChartCard(
            FluentIconRegular.DataUsage24,
            "FWSankeyDiagram",
            "SankeyNode and SankeyLink flows with label, value, opacity, and orientation states.",
            CreateSankeyDiagramSample()));
        examples.Children.Add(CreateChartCard(
            FluentIconRegular.InfoSparkle24,
            "FWChartLegend and FWChartTooltip",
            "Standalone legend items and tooltip value surfaces for custom chart compositions.",
            CreateLegendAndTooltipSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateLineChartSample()
    {
        var chart = PrepareAxisChart(new FWLineChart
        {
            LineSmoothing = true,
            ShowArea = true,
            AreaOpacity = 0.18,
            ShowDataPoints = true,
            DataPointRadius = 3.4,
            XAxis = CreateCategoryAxis("Day", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"),
            YAxis = CreateNumericAxis("Signals", min: 0, max: 100),
            IsZoomEnabled = true
        }, "Pipeline signal", 482, 220);
        chart.Series.Add(CreateLineSeries(
            "Current",
            PaletteBrush(0),
            Point("Mon", 62),
            Point("Tue", 70),
            Point("Wed", 66),
            Point("Thu", 82),
            Point("Fri", 78),
            Point("Sat", 91)));
        chart.Series.Add(CreateLineSeries(
            "Baseline",
            PaletteBrush(5),
            Point("Mon", 55),
            Point("Tue", 58),
            Point("Wed", 61),
            Point("Thu", 63),
            Point("Fri", 68),
            Point("Sat", 72)));
        var output = CreateOutput(FormatChartVisualQa("Line chart QA", chart));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateChartSurface(chart),
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.DataUsage24, "Area", () =>
                    {
                        chart.ShowArea = !chart.ShowArea;
                        output.Text = FormatChartVisualQa("Area toggled", chart);
                    }),
                    CreateIconActionButton(FluentIconRegular.TextSortAscending24, "Smooth", () =>
                    {
                        chart.LineSmoothing = !chart.LineSmoothing;
                        output.Text = FormatChartVisualQa("Smoothing toggled", chart);
                    }),
                    CreateIconActionButton(FluentIconRegular.InfoSparkle24, "Points", () =>
                    {
                        chart.ShowDataPoints = !chart.ShowDataPoints;
                        output.Text = FormatChartVisualQa("Data points toggled", chart);
                    }),
                    CreateIconActionButton(FluentIconRegular.ArrowCollapseAll24, "Reset", () =>
                    {
                        chart.ResetZoom();
                        output.Text = FormatChartVisualQa("Viewport reset", chart);
                    })),
                CreateStatus(output)
            }
        };
    }

    internal static GalleryChartVisualQaSnapshot CreateChartVisualQaSnapshot(ChartBase chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        var axisChart = chart as AxisChartBase;
        return new GalleryChartVisualQaSnapshot(
            chart.GetType().Name,
            chart.Title ?? string.Empty,
            chart.Width,
            chart.Height,
            CountChartSeries(chart),
            CountChartDataPoints(chart),
            chart.ChartPalette is { Count: > 0 },
            chart.LegendPosition,
            chart.IsTooltipEnabled,
            chart.IsAnimationEnabled,
            axisChart != null,
            axisChart?.IsGridLinesVisible ?? false,
            axisChart?.IsZoomEnabled ?? false,
            axisChart?.IsPanEnabled ?? false);
    }

    internal static string FormatChartVisualQa(string action, ChartBase chart)
    {
        return FormatChartVisualQa(action, CreateChartVisualQaSnapshot(chart));
    }

    internal static string FormatChartVisualQa(string action, GalleryChartVisualQaSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(action);

        return $"{action}. Chart: {snapshot.Chart}. Title: {snapshot.Title}. Size: {snapshot.Width:0}x{snapshot.Height:0}. Series: {snapshot.SeriesCount}. Points: {snapshot.DataPointCount}. Palette: {FormatOnOff(snapshot.HasPalette)}. Legend: {snapshot.LegendPosition}. Tooltip: {FormatOnOff(snapshot.IsTooltipEnabled)}. Animation: {FormatOnOff(snapshot.IsAnimationEnabled)}. Axes: {FormatOnOff(snapshot.HasAxes)}. Grid: {FormatOnOff(snapshot.IsGridVisible)}. Zoom: {FormatOnOff(snapshot.IsZoomEnabled)}. Pan: {FormatOnOff(snapshot.IsPanEnabled)}.";
    }

    private static int CountChartSeries(ChartBase chart)
    {
        return chart switch
        {
            LineChart lineChart => lineChart.Series.Count,
            BarChart barChart => barChart.Series.Count,
            ScatterPlot scatterPlot => scatterPlot.Series.Count,
            PieChart pieChart => pieChart.Series.DataPoints.Count > 0 ? 1 : 0,
            Heatmap heatmap => heatmap.Data != null ? 1 : 0,
            GaugeChart gaugeChart => gaugeChart.Ranges.Count > 0 ? 1 : 0,
            TreeMap treeMap => treeMap.Items.Count > 0 ? 1 : 0,
            CandlestickChart candlestickChart => CountEnumerable(candlestickChart.ItemsSource) > 0 ? 1 : 0,
            NetworkGraph networkGraph => networkGraph.Nodes.Count > 0 ? 1 : 0,
            GanttChart ganttChart => ganttChart.Tasks.Count > 0 ? 1 : 0,
            SankeyDiagram sankeyDiagram => sankeyDiagram.Nodes.Count > 0 ? 1 : 0,
            _ => 0
        };
    }

    private static int CountChartDataPoints(ChartBase chart)
    {
        return chart switch
        {
            LineChart lineChart => lineChart.Series.Sum(series => CountSeriesDataPoints(series.DataPoints, series.ItemsSource)),
            BarChart barChart => barChart.Series.Sum(series => CountSeriesDataPoints(series.DataPoints, series.ItemsSource)),
            ScatterPlot scatterPlot => scatterPlot.Series.Sum(series => CountSeriesDataPoints(series.DataPoints, series.ItemsSource)),
            PieChart pieChart => pieChart.Series.DataPoints.Count,
            Heatmap heatmap => heatmap.Data?.Length ?? 0,
            GaugeChart gaugeChart => gaugeChart.Ranges.Count,
            TreeMap treeMap => treeMap.Items.Sum(CountTreeMapItems),
            CandlestickChart candlestickChart => CountEnumerable(candlestickChart.ItemsSource),
            NetworkGraph networkGraph => networkGraph.Nodes.Count + networkGraph.Links.Count,
            GanttChart ganttChart => ganttChart.Tasks.Count,
            SankeyDiagram sankeyDiagram => sankeyDiagram.Nodes.Count + sankeyDiagram.Links.Count,
            _ => 0
        };
    }

    private static int CountSeriesDataPoints<TDataPoint>(ICollection<TDataPoint> dataPoints, System.Collections.IEnumerable? itemsSource)
    {
        return dataPoints.Count > 0 ? dataPoints.Count : CountEnumerable(itemsSource);
    }

    private static int CountTreeMapItems(TreeMapItem item)
    {
        return 1 + item.Children.Sum(CountTreeMapItems);
    }

    private static int CountEnumerable(System.Collections.IEnumerable? items)
    {
        if (items == null)
        {
            return 0;
        }

        var count = 0;
        foreach (var _ in items)
        {
            count++;
        }

        return count;
    }

    private static UIElement CreateBarChartSample()
    {
        var output = CreateOutput("Mode: Grouped. Labels: off. Orientation: vertical.");
        var chart = PrepareAxisChart(new FWBarChart
        {
            BarMode = BarMode.Grouped,
            BarSpacing = 2,
            GroupSpacing = 9,
            ShowValueLabels = false,
            XAxis = CreateCategoryAxis("Area", "Core", "Charts", "Data", "Shell"),
            YAxis = CreateNumericAxis("Coverage", min: 0, max: 100)
        }, "Control coverage", 482, 220);
        chart.Series.Add(CreateBarSeries(
            "Ready",
            PaletteBrush(2),
            Point("Core", 74),
            Point("Charts", 62),
            Point("Data", 80),
            Point("Shell", 67)));
        chart.Series.Add(CreateBarSeries(
            "Review",
            PaletteBrush(3),
            Point("Core", 12),
            Point("Charts", 24),
            Point("Data", 11),
            Point("Shell", 18)));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateChartSurface(chart),
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.TableResizeColumn24, "Mode", () =>
                    {
                        chart.BarMode = chart.BarMode == BarMode.Grouped ? BarMode.Stacked : BarMode.Grouped;
                        output.Text = $"Mode: {chart.BarMode}. Labels: {FormatOnOff(chart.ShowValueLabels)}. Orientation: {chart.Orientation.ToString().ToLowerInvariant()}.";
                    }),
                    CreateIconActionButton(FluentIconRegular.NumberRow24, "Labels", () =>
                    {
                        chart.ShowValueLabels = !chart.ShowValueLabels;
                        output.Text = $"Mode: {chart.BarMode}. Labels: {FormatOnOff(chart.ShowValueLabels)}. Orientation: {chart.Orientation.ToString().ToLowerInvariant()}.";
                    }),
                    CreateIconActionButton(FluentIconRegular.TextSortAscending24, "Axis", () =>
                    {
                        chart.Orientation = chart.Orientation == Orientation.Vertical ? Orientation.Horizontal : Orientation.Vertical;
                        output.Text = $"Mode: {chart.BarMode}. Labels: {FormatOnOff(chart.ShowValueLabels)}. Orientation: {chart.Orientation.ToString().ToLowerInvariant()}.";
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreatePieChartSample()
    {
        var output = CreateOutput("Donut: off. Labels: connector. Exploded: Charts.");
        var chart = PrepareChart(new FWPieChart
        {
            InnerRadiusRatio = 0,
            StartAngle = -90,
            ExplodeOffset = 14,
            ShowLabels = true,
            LabelPosition = PieLabelPosition.Connector,
            LabelFormat = "{0}: {1:P0}",
            LegendPosition = LegendPosition.Right,
            PlotAreaMargin = new Thickness(20, 18, 20, 30)
        }, "Usage mix", 482, 220);
        chart.Series.DataPoints.Add(new PieDataPoint { Label = "Charts", Value = 34, Brush = PaletteBrush(0), IsExploded = true });
        chart.Series.DataPoints.Add(new PieDataPoint { Label = "Tables", Value = 26, Brush = PaletteBrush(2) });
        chart.Series.DataPoints.Add(new PieDataPoint { Label = "Inspect", Value = 22, Brush = PaletteBrush(3) });
        chart.Series.DataPoints.Add(new PieDataPoint { Label = "Inputs", Value = 18, Brush = PaletteBrush(4) });

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateChartSurface(chart),
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.ChartMultiple24, "Donut", () =>
                    {
                        chart.InnerRadiusRatio = chart.InnerRadiusRatio == 0 ? 0.48 : 0;
                        output.Text = $"Donut: {FormatOnOff(chart.InnerRadiusRatio > 0)}. Labels: {chart.LabelPosition}. Exploded: {FormatExplodedSlice(chart.Series)}.";
                    }),
                    CreateIconActionButton(FluentIconRegular.TextSortAscending24, "Labels", () =>
                    {
                        chart.LabelPosition = chart.LabelPosition == PieLabelPosition.Connector ? PieLabelPosition.Inside : PieLabelPosition.Connector;
                        output.Text = $"Donut: {FormatOnOff(chart.InnerRadiusRatio > 0)}. Labels: {chart.LabelPosition}. Exploded: {FormatExplodedSlice(chart.Series)}.";
                    }),
                    CreateIconActionButton(FluentIconRegular.ArrowExpand24, "Explode", () =>
                    {
                        var first = chart.Series.DataPoints[0];
                        first.IsExploded = !first.IsExploded;
                        output.Text = $"Donut: {FormatOnOff(chart.InnerRadiusRatio > 0)}. Labels: {chart.LabelPosition}. Exploded: {FormatExplodedSlice(chart.Series)}.";
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateScatterPlotSample()
    {
        var output = CreateOutput("Trend: on. Point shape: circle. Zoom: enabled.");
        var chart = PrepareAxisChart(new FWScatterPlot
        {
            ShowTrendLine = true,
            TrendLineType = TrendLineType.Linear,
            MinPointSize = 4,
            MaxPointSize = 14,
            XAxis = CreateNumericAxis("Latency", min: 0, max: 120),
            YAxis = CreateNumericAxis("Satisfaction", min: 0, max: 100),
            IsZoomEnabled = true,
            IsPanEnabled = true
        }, "Latency vs quality", 482, 220);
        chart.Series.Add(CreateScatterSeries(
            "Sessions",
            PaletteBrush(0),
            Point(18, 91),
            Point(24, 88),
            Point(37, 83),
            Point(45, 80),
            Point(58, 72),
            Point(74, 66),
            Point(86, 61),
            Point(105, 54)));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateChartSurface(chart),
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.DataHistogram24, "Trend", () =>
                    {
                        chart.ShowTrendLine = !chart.ShowTrendLine;
                        output.Text = $"Trend: {FormatOnOff(chart.ShowTrendLine)}. Point shape: {chart.Series[0].PointShape.ToString().ToLowerInvariant()}. Zoom: enabled.";
                    }),
                    CreateIconActionButton(FluentIconRegular.ChartMultiple24, "Shape", () =>
                    {
                        chart.Series[0].PointShape = chart.Series[0].PointShape == PointShape.Circle ? PointShape.Diamond : PointShape.Circle;
                        output.Text = $"Trend: {FormatOnOff(chart.ShowTrendLine)}. Point shape: {chart.Series[0].PointShape.ToString().ToLowerInvariant()}. Zoom: enabled.";
                    }),
                    CreateIconActionButton(FluentIconRegular.ArrowCollapseAll24, "Reset", () =>
                    {
                        chart.ResetZoom();
                        output.Text = "Viewport reset. Wheel zoom and middle-button pan remain enabled.";
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateHeatmapSample()
    {
        var output = CreateOutput("Scale: Viridis. Cell values: on. Matrix: 5 x 6.");
        var chart = PrepareAxisChart(new FWHeatmap
        {
            Data = new double[,]
            {
                { 0.62, 0.71, 0.69, 0.76, 0.82, 0.86 },
                { 0.41, 0.48, 0.57, 0.63, 0.68, 0.72 },
                { 0.32, 0.36, 0.42, 0.51, 0.59, 0.66 },
                { 0.78, 0.75, 0.73, 0.70, 0.69, 0.74 },
                { 0.55, 0.61, 0.67, 0.71, 0.77, 0.81 }
            },
            XLabels = new List<string> { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" },
            YLabels = new List<string> { "Build", "Theme", "Docs", "Tests", "Gallery" },
            ColorScale = HeatmapColorScale.Viridis,
            ShowCellValues = true,
            CellValueFormat = "P0",
            CellBorderBrush = ThemeBrush("ControlBorder", Color.FromArgb(80, 255, 255, 255)),
            CellBorderThickness = 0.75,
            DataMinimum = 0,
            DataMaximum = 1,
            PlotAreaMargin = new Thickness(40, 22, 58, 32)
        }, "Activity density", 482, 220);

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateChartSurface(chart),
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.DataUsage24, "Scale", () =>
                    {
                        chart.ColorScale = chart.ColorScale == HeatmapColorScale.Viridis ? HeatmapColorScale.BlueToRed : HeatmapColorScale.Viridis;
                        output.Text = $"Scale: {chart.ColorScale}. Cell values: {FormatOnOff(chart.ShowCellValues)}. Matrix: 5 x 6.";
                    }),
                    CreateIconActionButton(FluentIconRegular.NumberRow24, "Values", () =>
                    {
                        chart.ShowCellValues = !chart.ShowCellValues;
                        output.Text = $"Scale: {chart.ColorScale}. Cell values: {FormatOnOff(chart.ShowCellValues)}. Matrix: 5 x 6.";
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateSparklineSample()
    {
        var output = CreateOutput("Sparkline variants use Values with line, area, bar, and win/loss render modes.");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateSparklineRow("Revenue", SparklineType.Line, [72, 76, 75, 81, 88, 86, 92], PaletteBrush(0)),
                CreateSparklineRow("Capacity", SparklineType.Area, [42, 45, 49, 53, 58, 64, 71], PaletteBrush(5)),
                CreateSparklineRow("Errors", SparklineType.Bar, [3, -2, 1, -4, 2, 0, -1], PaletteBrush(3)),
                CreateSparklineRow("SLA", SparklineType.WinLoss, [1, 1, -1, 1, 1, -1, 1], PaletteBrush(2)),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateGaugeChartSample()
    {
        var output = CreateOutput("Value: 78. Ranges: warn, stable, target. Ticks: on.");
        var chart = PrepareChart(new FWGaugeChart
        {
            Value = 78,
            Minimum = 0,
            Maximum = 100,
            ValueFormat = "F0",
            ValueFontSize = 22,
            TrackThickness = 18,
            NeedleBrush = PaletteBrush(3),
            TrackBrush = ThemeBrush("LayerFillColorDefaultBrush", Color.FromArgb(50, 255, 255, 255)),
            ShowTickMarks = true,
            MajorTickInterval = 25,
            MinorTickInterval = 5,
            IsLegendVisible = true,
            LegendPosition = LegendPosition.Bottom,
            PlotAreaMargin = new Thickness(18, 16, 18, 42)
        }, "Readiness score", 482, 220);
        chart.Ranges.Add(new GaugeRange { Minimum = 0, Maximum = 55, Brush = PaletteBrush(3, 190), InnerRadiusRatio = 0.76 });
        chart.Ranges.Add(new GaugeRange { Minimum = 55, Maximum = 80, Brush = PaletteBrush(6, 190), InnerRadiusRatio = 0.76 });
        chart.Ranges.Add(new GaugeRange { Minimum = 80, Maximum = 100, Brush = PaletteBrush(2, 190), InnerRadiusRatio = 0.76 });

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateChartSurface(chart),
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.ArrowDown24, "Lower", () =>
                    {
                        chart.Value = Math.Max(chart.Minimum, chart.Value - 8);
                        output.Text = $"Value: {chart.Value:F0}. Ranges: warn, stable, target. Ticks: {FormatOnOff(chart.ShowTickMarks)}.";
                    }),
                    CreateIconActionButton(FluentIconRegular.ArrowExpand24, "Raise", () =>
                    {
                        chart.Value = Math.Min(chart.Maximum, chart.Value + 8);
                        output.Text = $"Value: {chart.Value:F0}. Ranges: warn, stable, target. Ticks: {FormatOnOff(chart.ShowTickMarks)}.";
                    }),
                    CreateIconActionButton(FluentIconRegular.NumberRow24, "Ticks", () =>
                    {
                        chart.ShowTickMarks = !chart.ShowTickMarks;
                        output.Text = $"Value: {chart.Value:F0}. Ranges: warn, stable, target. Ticks: {FormatOnOff(chart.ShowTickMarks)}.";
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateTreeMapSample()
    {
        var output = CreateOutput("Algorithm: Squarified. Labels: on. Items: 4 groups.");
        var chart = PrepareChart(new FWTreeMap
        {
            Algorithm = TreeMapAlgorithm.Squarified,
            ShowLabels = true,
            LabelMinFontSize = 8,
            CellPadding = 3,
            CellBorderBrush = ThemeBrush("ControlBorder", Color.FromArgb(90, 255, 255, 255)),
            CellBorderThickness = 1,
            PlotAreaMargin = new Thickness(12, 16, 12, 12)
        }, "Portfolio allocation", 482, 220);
        chart.Items.Add(CreateTreeMapItem("Core", 42, PaletteBrush(0), CreateTreeMapItem("Input", 18, PaletteBrush(0, 220)), CreateTreeMapItem("Layout", 24, PaletteBrush(5, 220))));
        chart.Items.Add(CreateTreeMapItem("Data", 28, PaletteBrush(2), CreateTreeMapItem("Tables", 16, PaletteBrush(2, 220)), CreateTreeMapItem("Charts", 12, PaletteBrush(6, 220))));
        chart.Items.Add(CreateTreeMapItem("Media", 18, PaletteBrush(3)));
        chart.Items.Add(CreateTreeMapItem("Shell", 12, PaletteBrush(4)));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateChartSurface(chart),
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.BranchFork24, "Layout", () =>
                    {
                        chart.Algorithm = chart.Algorithm == TreeMapAlgorithm.Squarified ? TreeMapAlgorithm.SliceAndDice : TreeMapAlgorithm.Squarified;
                        output.Text = $"Algorithm: {chart.Algorithm}. Labels: {FormatOnOff(chart.ShowLabels)}. Items: 4 groups.";
                    }),
                    CreateIconActionButton(FluentIconRegular.TextSortAscending24, "Labels", () =>
                    {
                        chart.ShowLabels = !chart.ShowLabels;
                        output.Text = $"Algorithm: {chart.Algorithm}. Labels: {FormatOnOff(chart.ShowLabels)}. Items: 4 groups.";
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateCandlestickChartSample()
    {
        var output = CreateOutput("Volume: on. Moving average: SMA(3). Candles: 7.");
        var chart = PrepareAxisChart(new FWCandlestickChart
        {
            ItemsSource = CreateOhlcData(),
            ShowVolume = true,
            VolumeHeight = 0.24,
            CandleWidth = 0.72,
            BullishBrush = PaletteBrush(2, 210),
            BearishBrush = PaletteBrush(3, 210),
            XAxis = new DateTimeAxis
            {
                Title = "Date",
                DateFormat = "MM/dd",
                IntervalType = DateTimeIntervalType.Day,
                TickCount = 4,
                LabelForeground = ThemeBrush("TextSecondary", Color.FromRgb(180, 180, 180)),
                LabelFontSize = 10
            },
            YAxis = CreateNumericAxis("Price", min: 90, max: 128, labelFormat: "F0"),
            PlotAreaMargin = new Thickness(58, 24, 20, 42)
        }, "Price action", 482, 220);
        chart.MovingAverages.Add(new MovingAverageConfig { Period = 3, Type = MovingAverageType.SMA, Brush = PaletteBrush(6), Thickness = 1.4 });

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateChartSurface(chart),
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.DataHistogram24, "Volume", () =>
                    {
                        chart.ShowVolume = !chart.ShowVolume;
                        output.Text = $"Volume: {FormatOnOff(chart.ShowVolume)}. Moving average: {FormatMovingAverage(chart)}. Candles: 7.";
                    }),
                    CreateIconActionButton(FluentIconRegular.DataUsage24, "Average", () =>
                    {
                        if (chart.MovingAverages.Count == 0)
                        {
                            chart.MovingAverages.Add(new MovingAverageConfig { Period = 3, Type = MovingAverageType.SMA, Brush = PaletteBrush(6), Thickness = 1.4 });
                        }
                        else
                        {
                            chart.MovingAverages.Clear();
                        }
                        output.Text = $"Volume: {FormatOnOff(chart.ShowVolume)}. Moving average: {FormatMovingAverage(chart)}. Candles: 7.";
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateNetworkGraphSample()
    {
        var output = CreateOutput("Layout: Circular. Labels: on. Dragging: off.");
        var chart = PrepareChart(new FWNetworkGraph
        {
            LayoutAlgorithm = NetworkLayoutAlgorithm.Circular,
            NodeRadius = 13,
            LinkThickness = 1.4,
            NodeBrush = PaletteBrush(0),
            LinkBrush = ThemeBrush("ControlBorder", Color.FromArgb(130, 255, 255, 255)),
            ShowLabels = true,
            IsNodeDraggable = false,
            PlotAreaMargin = new Thickness(18, 18, 18, 18)
        }, "Dependency graph", 482, 220);
        chart.Nodes.Add(new NetworkNode { Id = "theme", Label = "Theme", Brush = PaletteBrush(0), Radius = 16 });
        chart.Nodes.Add(new NetworkNode { Id = "charts", Label = "Charts", Brush = PaletteBrush(5), Radius = 16 });
        chart.Nodes.Add(new NetworkNode { Id = "data", Label = "Data", Brush = PaletteBrush(2), Radius = 14 });
        chart.Nodes.Add(new NetworkNode { Id = "tokens", Label = "Tokens", Brush = PaletteBrush(6), Radius = 13 });
        chart.Nodes.Add(new NetworkNode { Id = "docs", Label = "Docs", Brush = PaletteBrush(3), Radius = 12 });
        chart.Links.Add(new NetworkLink { SourceId = "theme", TargetId = "charts", Weight = 2 });
        chart.Links.Add(new NetworkLink { SourceId = "theme", TargetId = "tokens", Weight = 3 });
        chart.Links.Add(new NetworkLink { SourceId = "charts", TargetId = "data", Weight = 2 });
        chart.Links.Add(new NetworkLink { SourceId = "charts", TargetId = "docs", Weight = 1 });
        chart.Links.Add(new NetworkLink { SourceId = "data", TargetId = "docs", Weight = 1 });

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateChartSurface(chart),
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.BranchFork24, "Layout", () =>
                    {
                        chart.LayoutAlgorithm = chart.LayoutAlgorithm == NetworkLayoutAlgorithm.Circular
                            ? NetworkLayoutAlgorithm.Hierarchical
                            : NetworkLayoutAlgorithm.Circular;
                        output.Text = $"Layout: {chart.LayoutAlgorithm}. Labels: {FormatOnOff(chart.ShowLabels)}. Dragging: {FormatOnOff(chart.IsNodeDraggable)}.";
                    }),
                    CreateIconActionButton(FluentIconRegular.TextSortAscending24, "Labels", () =>
                    {
                        chart.ShowLabels = !chart.ShowLabels;
                        output.Text = $"Layout: {chart.LayoutAlgorithm}. Labels: {FormatOnOff(chart.ShowLabels)}. Dragging: {FormatOnOff(chart.IsNodeDraggable)}.";
                    }),
                    CreateIconActionButton(FluentIconRegular.ArrowExpand24, "Drag", () =>
                    {
                        chart.IsNodeDraggable = !chart.IsNodeDraggable;
                        output.Text = $"Layout: {chart.LayoutAlgorithm}. Labels: {FormatOnOff(chart.ShowLabels)}. Dragging: {FormatOnOff(chart.IsNodeDraggable)}.";
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateGanttChartSample()
    {
        var output = CreateOutput("Dependencies: on. Progress: on. Today line: off.");
        var chart = PrepareAxisChart(new FWGanttChart
        {
            Tasks = CreateGanttTasks(),
            RowHeight = 24,
            TaskBrush = PaletteBrush(0, 210),
            MilestoneBrush = PaletteBrush(6),
            DependencyLineBrush = ThemeBrush("ControlBorder", Color.FromArgb(160, 255, 255, 255)),
            ShowDependencies = true,
            ShowProgress = true,
            ShowToday = false,
            BarCornerRadius = 4,
            XAxis = new DateTimeAxis
            {
                Title = "Sprint",
                DateFormat = "MM/dd",
                IntervalType = DateTimeIntervalType.Day,
                TickCount = 5,
                LabelForeground = ThemeBrush("TextSecondary", Color.FromRgb(180, 180, 180)),
                LabelFontSize = 10
            },
            PlotAreaMargin = new Thickness(52, 22, 18, 38)
        }, "Release plan", 482, 220);

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateChartSurface(chart),
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.BranchFork24, "Links", () =>
                    {
                        chart.ShowDependencies = !chart.ShowDependencies;
                        output.Text = $"Dependencies: {FormatOnOff(chart.ShowDependencies)}. Progress: {FormatOnOff(chart.ShowProgress)}. Today line: {FormatOnOff(chart.ShowToday)}.";
                    }),
                    CreateIconActionButton(FluentIconRegular.DataUsage24, "Progress", () =>
                    {
                        chart.ShowProgress = !chart.ShowProgress;
                        output.Text = $"Dependencies: {FormatOnOff(chart.ShowDependencies)}. Progress: {FormatOnOff(chart.ShowProgress)}. Today line: {FormatOnOff(chart.ShowToday)}.";
                    }),
                    CreateIconActionButton(FluentIconRegular.NumberRow24, "Today", () =>
                    {
                        chart.ShowToday = !chart.ShowToday;
                        output.Text = $"Dependencies: {FormatOnOff(chart.ShowDependencies)}. Progress: {FormatOnOff(chart.ShowProgress)}. Today line: {FormatOnOff(chart.ShowToday)}.";
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateSankeyDiagramSample()
    {
        var output = CreateOutput("Values: on. Orientation: horizontal. Opacity: 0.46.");
        var chart = PrepareChart(new FWSankeyDiagram
        {
            NodeWidth = 18,
            NodeSpacing = 12,
            LinkOpacity = 0.46,
            NodeBrush = PaletteBrush(0),
            ShowLabels = true,
            ShowValues = true,
            LabelPosition = SankeyLabelPosition.Right,
            Orientation = Orientation.Horizontal,
            PlotAreaMargin = new Thickness(16, 18, 16, 18)
        }, "Work intake flow", 482, 220);
        chart.Nodes.Add(new SankeyNode { Id = "ideas", Label = "Ideas", Value = 40, Brush = PaletteBrush(0) });
        chart.Nodes.Add(new SankeyNode { Id = "triage", Label = "Triage", Value = 32, Brush = PaletteBrush(5) });
        chart.Nodes.Add(new SankeyNode { Id = "build", Label = "Build", Value = 21, Brush = PaletteBrush(2) });
        chart.Nodes.Add(new SankeyNode { Id = "docs", Label = "Docs", Value = 11, Brush = PaletteBrush(6) });
        chart.Nodes.Add(new SankeyNode { Id = "ship", Label = "Ship", Value = 27, Brush = PaletteBrush(3) });
        chart.Links.Add(new SankeyLink { SourceId = "ideas", TargetId = "triage", Value = 32, Brush = PaletteBrush(0, 150) });
        chart.Links.Add(new SankeyLink { SourceId = "triage", TargetId = "build", Value = 21, Brush = PaletteBrush(5, 150) });
        chart.Links.Add(new SankeyLink { SourceId = "triage", TargetId = "docs", Value = 11, Brush = PaletteBrush(6, 150) });
        chart.Links.Add(new SankeyLink { SourceId = "build", TargetId = "ship", Value = 18, Brush = PaletteBrush(2, 150) });
        chart.Links.Add(new SankeyLink { SourceId = "docs", TargetId = "ship", Value = 9, Brush = PaletteBrush(3, 150) });

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateChartSurface(chart),
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.NumberRow24, "Values", () =>
                    {
                        chart.ShowValues = !chart.ShowValues;
                        output.Text = $"Values: {FormatOnOff(chart.ShowValues)}. Orientation: {chart.Orientation.ToString().ToLowerInvariant()}. Opacity: {chart.LinkOpacity:F2}.";
                    }),
                    CreateIconActionButton(FluentIconRegular.TableResizeColumn24, "Flow", () =>
                    {
                        chart.Orientation = chart.Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
                        output.Text = $"Values: {FormatOnOff(chart.ShowValues)}. Orientation: {chart.Orientation.ToString().ToLowerInvariant()}. Opacity: {chart.LinkOpacity:F2}.";
                    }),
                    CreateIconActionButton(FluentIconRegular.DataUsage24, "Opacity", () =>
                    {
                        chart.LinkOpacity = chart.LinkOpacity < 0.7 ? 0.78 : 0.46;
                        output.Text = $"Values: {FormatOnOff(chart.ShowValues)}. Orientation: {chart.Orientation.ToString().ToLowerInvariant()}. Opacity: {chart.LinkOpacity:F2}.";
                    })),
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateLegendAndTooltipSample()
    {
        var output = CreateOutput("Legend items: 4. Tooltip binds SeriesTitle, XValue, YValue, and SeriesBrush.");
        var legend = new FWChartLegend
        {
            Width = 470,
            Height = 36,
            Padding = new Thickness(8, 9, 8, 6),
            Foreground = ThemeBrush("TextPrimary", Color.FromRgb(230, 230, 230)),
            Items = new[]
            {
                new ChartLegendItem { Label = "Current", Brush = PaletteBrush(0) },
                new ChartLegendItem { Label = "Baseline", Brush = PaletteBrush(5) },
                new ChartLegendItem { Label = "Warning", Brush = PaletteBrush(6) },
                new ChartLegendItem { Label = "Risk", Brush = PaletteBrush(3) }
            }
        };
        var tooltip = new FWChartTooltip
        {
            Width = 210,
            Height = 72,
            SeriesTitle = "Current",
            XValue = "Fri",
            YValue = "78",
            SeriesBrush = PaletteBrush(0),
            Background = ThemeBrush("ChartTooltipBackground", Color.FromArgb(232, 36, 36, 36)),
            Foreground = ThemeBrush("ChartTooltipForeground", Color.FromRgb(245, 245, 245)),
            BorderBrush = ThemeBrush("ChartTooltipBorderBrush", Color.FromArgb(190, 255, 255, 255))
        };

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateLegendTooltipSurface(new FWStackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 12,
                    Children =
                    {
                        legend,
                        tooltip
                    }
                }),
                CreateButtonRow(
                    CreateIconActionButton(FluentIconRegular.TableResizeColumn24, "Legend", () =>
                    {
                        legend.Orientation = legend.Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
                        legend.Height = legend.Orientation == Orientation.Horizontal ? 36 : 116;
                        output.Text = $"Legend orientation: {legend.Orientation}. Tooltip series: {tooltip.SeriesTitle}.";
                    }),
                    CreateIconActionButton(FluentIconRegular.InfoSparkle24, "Tooltip", () =>
                    {
                        tooltip.SeriesTitle = tooltip.SeriesTitle == "Current" ? "Risk" : "Current";
                        tooltip.YValue = tooltip.SeriesTitle == "Current" ? "78" : "24";
                        tooltip.SeriesBrush = tooltip.SeriesTitle == "Current" ? PaletteBrush(0) : PaletteBrush(3);
                        output.Text = $"Legend orientation: {legend.Orientation}. Tooltip series: {tooltip.SeriesTitle}.";
                    })),
                CreateStatus(output)
            }
        };
    }

    private static FWBorder CreateChartCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return new FWBorder
        {
            Width = 540,
            Background = ThemeBrush("ControlBackground", Color.FromArgb(32, 255, 255, 255)),
            BorderBrush = ThemeBrush("ControlBorder", Color.FromArgb(90, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 10,
                Children =
                {
                    new FWStackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        Children =
                        {
                            CreateIcon(icon, 20, ThemeBrush("TextPrimary", Color.FromRgb(240, 240, 240))),
                            new FWTextBlock
                            {
                                Text = title,
                                FontSize = 15,
                                Foreground = ThemeBrush("TextPrimary", Color.FromRgb(240, 240, 240)),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    },
                    new FWTextBlock
                    {
                        Text = description,
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary", Color.FromRgb(180, 180, 180)),
                        TextWrapping = TextWrapping.Wrap
                    },
                    content
                }
            }
        };
    }

    private static FWBorder CreateChartSurface(UIElement content)
    {
        return new FWBorder
        {
            Width = 500,
            Height = 238,
            Background = ThemeBrush("LayerFillColorDefaultBrush", Color.FromArgb(24, 255, 255, 255)),
            BorderBrush = ThemeBrush("ControlBorder", Color.FromArgb(80, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8),
            Child = content
        };
    }

    private static FWBorder CreateLegendTooltipSurface(UIElement content)
    {
        return new FWBorder
        {
            Width = 500,
            Height = 150,
            Background = ThemeBrush("LayerFillColorDefaultBrush", Color.FromArgb(24, 255, 255, 255)),
            BorderBrush = ThemeBrush("ControlBorder", Color.FromArgb(80, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = content
        };
    }

    private static FWStackPanel CreateSparklineRow(string label, SparklineType type, IList<double> values, Brush brush)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                new FWTextBlock
                {
                    Text = label,
                    Width = 86,
                    FontSize = 12,
                    Foreground = ThemeBrush("TextSecondary", Color.FromRgb(180, 180, 180)),
                    VerticalAlignment = VerticalAlignment.Center
                },
                new FWBorder
                {
                    Width = 392,
                    Height = 38,
                    Background = ThemeBrush("LayerFillColorDefaultBrush", Color.FromArgb(24, 255, 255, 255)),
                    BorderBrush = ThemeBrush("ControlBorder", Color.FromArgb(80, 255, 255, 255)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(8),
                    Child = new FWSparkline
                    {
                        Width = 370,
                        Height = 20,
                        Values = values,
                        SparklineType = type,
                        LineBrush = brush,
                        FillBrush = PaletteBrush(0, 45),
                        NegativeBarBrush = PaletteBrush(3),
                        ShowHighLowPoints = type is SparklineType.Line or SparklineType.Area,
                        HighPointBrush = PaletteBrush(2),
                        LowPointBrush = PaletteBrush(3),
                        FirstPointBrush = PaletteBrush(5),
                        LastPointBrush = PaletteBrush(6),
                        WinColor = PaletteBrush(2),
                        LossColor = PaletteBrush(3),
                        BarSpacing = 2
                    }
                }
            }
        };
    }

    private static T PrepareChart<T>(T chart, string title, double width, double height)
        where T : ChartBase
    {
        chart.Width = width;
        chart.Height = height;
        chart.Title = title;
        chart.TitleFontSize = 13;
        chart.TitleForeground = ThemeBrush("TextPrimary", Color.FromRgb(240, 240, 240));
        chart.Foreground = ThemeBrush("TextSecondary", Color.FromRgb(180, 180, 180));
        chart.Background = ThemeBrush("LayerFillColorDefaultBrush", Color.FromArgb(18, 255, 255, 255));
        chart.ChartPalette = CreateChartPalette();
        chart.IsTooltipEnabled = true;
        chart.IsAnimationEnabled = false;
        return chart;
    }

    private static T PrepareAxisChart<T>(T chart, string title, double width, double height)
        where T : AxisChartBase
    {
        PrepareChart(chart, title, width, height);
        chart.AxisBrush = ThemeBrush("ControlBorder", Color.FromArgb(150, 255, 255, 255));
        chart.GridLineBrush = ThemeBrush("LayerFillColorDefaultBrush", Color.FromArgb(36, 255, 255, 255));
        chart.IsGridLinesVisible = true;
        return chart;
    }

    private static LineSeries CreateLineSeries(string title, Brush brush, params ChartDataPoint[] points)
    {
        var series = new LineSeries
        {
            Title = title,
            Brush = brush,
            StrokeBrush = brush,
            StrokeThickness = 2.2
        };

        foreach (var point in points)
        {
            series.DataPoints.Add(point);
        }

        return series;
    }

    private static BarSeries CreateBarSeries(string title, Brush brush, params ChartDataPoint[] points)
    {
        var series = new BarSeries
        {
            Title = title,
            Brush = brush,
            StrokeBrush = brush,
            StrokeThickness = 1
        };

        foreach (var point in points)
        {
            series.DataPoints.Add(point);
        }

        return series;
    }

    private static ScatterSeries CreateScatterSeries(string title, Brush brush, params ChartDataPoint[] points)
    {
        var series = new ScatterSeries
        {
            Title = title,
            Brush = brush,
            StrokeBrush = brush,
            StrokeThickness = 1,
            PointShape = PointShape.Circle
        };

        foreach (var point in points)
        {
            series.DataPoints.Add(point);
        }

        return series;
    }

    private static ChartDataPoint Point(object xValue, double yValue, string? label = null)
    {
        return new ChartDataPoint
        {
            XValue = xValue,
            YValue = yValue,
            Label = label
        };
    }

    private static TreeMapItem CreateTreeMapItem(string label, double value, Brush brush, params TreeMapItem[] children)
    {
        var item = new TreeMapItem
        {
            Label = label,
            Value = value,
            Brush = brush
        };

        foreach (var child in children)
        {
            item.Children.Add(child);
        }

        return item;
    }

    private static OhlcDataPoint[] CreateOhlcData()
    {
        return
        [
            new OhlcDataPoint { Date = new DateTime(2026, 2, 2), Open = 101, High = 111, Low = 98, Close = 108, Volume = 1180 },
            new OhlcDataPoint { Date = new DateTime(2026, 2, 3), Open = 108, High = 116, Low = 105, Close = 112, Volume = 1320 },
            new OhlcDataPoint { Date = new DateTime(2026, 2, 4), Open = 112, High = 114, Low = 103, Close = 106, Volume = 1420 },
            new OhlcDataPoint { Date = new DateTime(2026, 2, 5), Open = 106, High = 119, Low = 104, Close = 117, Volume = 1640 },
            new OhlcDataPoint { Date = new DateTime(2026, 2, 6), Open = 117, High = 124, Low = 113, Close = 121, Volume = 1510 },
            new OhlcDataPoint { Date = new DateTime(2026, 2, 7), Open = 121, High = 126, Low = 116, Close = 118, Volume = 1390 },
            new OhlcDataPoint { Date = new DateTime(2026, 2, 8), Open = 118, High = 128, Low = 117, Close = 125, Volume = 1710 }
        ];
    }

    private static ObservableCollection<GanttTask> CreateGanttTasks()
    {
        var design = new GanttTask
        {
            Id = "design",
            Name = "Design",
            StartDate = new DateTime(2026, 2, 2),
            EndDate = new DateTime(2026, 2, 5),
            Progress = 1.0,
            Group = "Charts",
            Brush = PaletteBrush(0, 210)
        };
        var build = new GanttTask
        {
            Id = "build",
            Name = "Build",
            StartDate = new DateTime(2026, 2, 5),
            EndDate = new DateTime(2026, 2, 10),
            Progress = 0.72,
            Group = "Charts",
            Brush = PaletteBrush(5, 210)
        };
        build.DependsOn.Add("design");
        var qa = new GanttTask
        {
            Id = "qa",
            Name = "QA",
            StartDate = new DateTime(2026, 2, 10),
            EndDate = new DateTime(2026, 2, 13),
            Progress = 0.36,
            Group = "Validation",
            Brush = PaletteBrush(2, 210)
        };
        qa.DependsOn.Add("build");
        var ship = new GanttTask
        {
            Id = "ship",
            Name = "Ship",
            StartDate = new DateTime(2026, 2, 14),
            EndDate = new DateTime(2026, 2, 14),
            Progress = 0,
            Group = "Validation",
            IsMilestone = true,
            Brush = PaletteBrush(6)
        };
        ship.DependsOn.Add("qa");

        return new ObservableCollection<GanttTask> { design, build, qa, ship };
    }

    private static CategoryAxis CreateCategoryAxis(string title, params string[] categories)
    {
        return new CategoryAxis
        {
            Title = title,
            Categories = new List<string>(categories),
            LabelForeground = ThemeBrush("TextSecondary", Color.FromRgb(180, 180, 180)),
            LabelFontSize = 10,
            TickCount = categories.Length
        };
    }

    private static NumericAxis CreateNumericAxis(string title, double? min = null, double? max = null, string? labelFormat = null)
    {
        return new NumericAxis
        {
            Title = title,
            Minimum = min,
            Maximum = max,
            LabelFormat = labelFormat,
            LabelForeground = ThemeBrush("TextSecondary", Color.FromRgb(180, 180, 180)),
            LabelFontSize = 10,
            TickCount = 5
        };
    }

    private static FWWrapPanel CreateButtonRow(params FWButton[] buttons)
    {
        var row = new FWWrapPanel
        {
            HorizontalSpacing = 8,
            VerticalSpacing = 8
        };

        foreach (var button in buttons)
        {
            row.Children.Add(button);
        }

        return row;
    }

    private static FWButton CreateIconActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    CreateIcon(icon, 16, ThemeBrush("TextPrimary", Color.FromRgb(240, 240, 240))),
                    new FWTextBlock
                    {
                        Text = text,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static FWTextBlock CreateOutput(string text)
    {
        return new FWTextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary", Color.FromRgb(180, 180, 180)),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateStatus(FWTextBlock status)
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush", Color.FromArgb(24, 255, 255, 255)),
            BorderBrush = ThemeBrush("ControlBorder", Color.FromArgb(80, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(FluentIconRegular.InfoSparkle24, 18, ThemeBrush("TextSecondary", Color.FromRgb(180, 180, 180))),
                    status
                }
            }
        };
    }

    private static FWStackPanel CreateSection(string title)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children =
            {
                new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    Children =
                    {
                        CreateIcon(FluentIconRegular.ChartMultiple24, 24, ThemeBrush("TextPrimary", Color.FromRgb(240, 240, 240))),
                        new FWTextBlock
                        {
                            Text = title,
                            FontSize = 22,
                            Foreground = ThemeBrush("TextPrimary", Color.FromRgb(240, 240, 240)),
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                }
            }
        };
    }

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size, Brush? foreground = null)
    {
        return FluentIconFactory.Regular(icon, size, foreground ?? ThemeBrush("TextPrimary", Color.FromRgb(240, 240, 240)));
    }

    private static Brush[] CreateChartPalette()
    {
        return
        [
            PaletteBrush(0),
            PaletteBrush(5),
            PaletteBrush(2),
            PaletteBrush(3),
            PaletteBrush(4),
            PaletteBrush(6)
        ];
    }

    private static Brush PaletteBrush(int index, byte alpha = 255)
    {
        var color = PaletteColor(index);
        return new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
    }

    private static Color PaletteColor(int index)
    {
        var colors = new[]
        {
            Color.FromRgb(0x41, 0x7E, 0xE0),
            Color.FromRgb(0xE0, 0x59, 0x3E),
            Color.FromRgb(0x4C, 0xAF, 0x50),
            Color.FromRgb(0xFF, 0x9E, 0x22),
            Color.FromRgb(0x9C, 0x5F, 0xC4),
            Color.FromRgb(0x00, 0xBC, 0xD4),
            Color.FromRgb(0xE9, 0x1E, 0x63)
        };

        return colors[index % colors.Length];
    }

    private static Brush ThemeBrush(string key, Color fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(fallback);
    }

    private static string FormatOnOff(bool value) => value ? "on" : "off";

    private static string FormatExplodedSlice(PieSeries series)
    {
        foreach (var point in series.DataPoints)
        {
            if (point.IsExploded)
            {
                return point.Label ?? "slice";
            }
        }

        return "none";
    }

    private static string FormatMovingAverage(FWCandlestickChart chart)
    {
        return chart.MovingAverages.Count == 0
            ? "off"
            : $"{chart.MovingAverages[0].Type}({chart.MovingAverages[0].Period})";
    }
}
