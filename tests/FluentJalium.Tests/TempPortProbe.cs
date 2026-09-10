using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Xunit.Abstractions;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class TempPortProbe(ITestOutputHelper output)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Probe loads runtime theme dictionaries.")]
    public void Probe_ScrollViewerViewportMetrics()
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = new Border { Width = 100, Height = 240 }
        };
        scrollViewer.Measure(new Size(100, 40));
        scrollViewer.Arrange(new Rect(0, 0, 100, 40));
        scrollViewer.UpdateLayout();

        output.WriteLine($"arranged={scrollViewer.RenderSize}");
        output.WriteLine($"ViewportWidth={scrollViewer.ViewportWidth} ViewportHeight={scrollViewer.ViewportHeight}");
        output.WriteLine($"ExtentWidth={scrollViewer.ExtentWidth} ExtentHeight={scrollViewer.ExtentHeight}");
        output.WriteLine($"ScrollableWidth={scrollViewer.ScrollableWidth} ScrollableHeight={scrollViewer.ScrollableHeight}");

        scrollViewer.ScrollToVerticalOffset(50);
        output.WriteLine($"after ScrollToVerticalOffset(50): VerticalOffset={scrollViewer.VerticalOffset}");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Probe loads runtime theme dictionaries.")]
    public void Probe_SnackbarStyleTriggerValues()
    {
        var app = new Application();
        FluentThemeManager.Apply(app);

        var style = app.Resources.OfType<System.Collections.DictionaryEntry>()
            .First(entry => entry.Key as Type == typeof(FWSnackbar))
            .Value as Style;

        output.WriteLine($"style={style is not null}");
        foreach (var trigger in style!.Triggers.OfType<Trigger>())
        {
            output.WriteLine(
                $"trigger Property={trigger.Property?.OwnerType?.Name}.{trigger.Property?.Name} " +
                $"ValueType={trigger.Value?.GetType().Name ?? "null"} Value={trigger.Value}");
        }
    }
}
