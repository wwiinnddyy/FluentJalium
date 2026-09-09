using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Charts;
using Jalium.UI.Media;

namespace FluentJalium.Gallery.Pages;

public sealed partial class ChartSample : UserControl
{
    public ChartSample()
    {
        InitializeComponent();

        var series = new LineSeries
        {
            Title = "Sessions",
            StrokeBrush = AccentBrush(),
            Brush = AccentBrush(),
            StrokeThickness = 3
        };
        foreach (var point in new[] { 18d, 32d, 28d, 46d, 42d, 64d, 72d })
        {
            series.DataPoints.Add(new ChartDataPoint { YValue = point });
        }

        Chart.Series.Add(series);
    }

    private static Brush AccentBrush() =>
        Application.Current?.Resources.TryGetValue("AccentFillColorDefaultBrush", out var value) == true && value is Brush brush
            ? brush
            : new SolidColorBrush(Color.FromRgb(0, 120, 212));
}
