using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace RatingInkProbe;

/// <summary>
/// Asks the question #52 could not ask in-process: where does one star cell's ink actually land.
///
/// The user's report is that the row sits too far right and that the right arm of a star is cut off. The control
/// builds each cell as a host the width of the scaled star with the run drawn at double size, scaled by 0.5 and
/// pulled left by a negative margin, so the numbers that decide the picture are the host width, the run's own
/// arranged width, and the box the run occupies after its render transform. All three are printed per cell, and the
/// window is then held open for a monitor grab - the only path that has ever shown glyph ink on this runtime
/// (spike/GlyphInkProbe, #96).
/// </summary>
internal static class Program
{
    [STAThread]
    private static int Main()
    {
        var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
        renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
        ThemeLoader.Initialize();
        var application = new Application();
        // Without the palette merged the control mounts with zero visual children and zero height - which is what the
        // first run of this probe read, and it is a fact about the probe, not about the defect.
        FluentThemeManager.Apply(application);
        FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

        var stack = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(20) };
        var ratings = new List<FluentRatingControl>();
        foreach (var value in new[] { 3.5, 5.0, 2.0, 4.7 })
        {
            var rating = new FluentRatingControl { Value = value, Margin = new Thickness(0, 14, 0, 0) };
            ratings.Add(rating);
            stack.Children.Add(rating);
        }

        var window = new Window
        {
            Title = "RatingInkProbe",
            Width = 700,
            Height = 420,
            Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
            Content = stack,
        };
        window.Show();
        Pump(30, 3000);

        for (var row = 0; row < ratings.Count; row++)
        {
            var rating = ratings[row];
            Console.WriteLine($"row {row}: Value={rating.Value} MaxRating={Read<object>(rating, "MaxRating")} " +
                $"ActualItemSize={Read<object>(rating, "ActualItemSize")} ItemInset={Read<object>(rating, "ItemInset")} " +
                $"ItemSpacing={Read<object>(rating, "ItemSpacing")} ActualRatingWidth={Read<object>(rating, "ActualRatingWidth")} " +
                $"| control FontFamily='{rating.FontFamily?.Source ?? "<null>"}' _itemAdvance={Read<object>(rating, "_itemAdvance")}");
            DumpPanels(rating);
            if (row == 0) Console.WriteLine(Tree(rating, 0));
        }

        var ready = Path.Combine(AppContext.BaseDirectory, "rating-ready.flag");
        if (File.Exists(ready)) File.Delete(ready);
        File.WriteAllText(ready, "laid out");

        var flag = Path.Combine(AppContext.BaseDirectory, "rating-stop.flag");
        if (File.Exists(flag)) File.Delete(flag);
        var deadline = Environment.TickCount64 + 120_000;
        while (Environment.TickCount64 < deadline && !File.Exists(flag))
        {
            Pump(4, 250);
            Thread.Sleep(30);
        }

        window.Close();
        Pump(4, 500);
        return 0;
    }

    /// <summary>Prints the mounted visual tree, because "no panels found" is itself a reading about this runtime.</summary>
    private static string Tree(Visual node, int depth)
    {
        var lines = new List<string>
        {
            $"{new string('.', depth)}{node.GetType().Name} " +
            $"{(node is FrameworkElement fe ? $"{fe.ActualWidth:0.#}x{fe.ActualHeight:0.#}" : "")} " +
            $"children={VisualTreeHelper.GetChildrenCount(node)}"
        };
        if (depth >= 6) return string.Join(" / ", lines);
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            if (VisualTreeHelper.GetChild(node, index) is Visual child) lines.Add(Tree(child, depth + 1));
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>Walks the two item panels the template names and prints one line per cell.</summary>
    private static void DumpPanels(FluentRatingControl rating)
    {
        var panels = new List<StackPanel>();
        Walk(rating, node =>
        {
            if (node is StackPanel panel && panel.Orientation == Orientation.Horizontal) panels.Add(panel);
        });

        for (var index = 0; index < panels.Count; index++)
        {
            var panel = panels[index];
            var kind = index == 0 ? "background" : "foreground";
            Console.WriteLine($"  {kind} panel: children={panel.Children.Count} " +
                $"spacing={Read<double>(panel, "Spacing"):0.###} width={panel.ActualWidth:0.###}");
            for (var cell = 0; cell < panel.Children.Count; cell++)
            {
                if (panel.Children[cell] is not FrameworkElement host) continue;
                var content = host is Panel container ? RunOf(container) : null;
                if (content is null)
                {
                    Console.WriteLine($"    cell {cell}: host {host.ActualWidth:0.###}x{host.ActualHeight:0.###} <no child>");
                    continue;
                }

                var box = BoxIn(content, rating);
                Console.WriteLine($"    cell {cell}: host w={host.ActualWidth:0.###} clip={Read<bool>(host, "ClipToBounds")} " +
                    $"| run {content.GetType().Name} desired={content.DesiredSize.Width:0.###} arranged={content.ActualWidth:0.###} " +
                    $"family='{(content as TextBlock)?.FontFamily?.Source ?? "<none>"}' " +
                    $"margin={content.Margin} scale={(content.RenderTransform as ScaleTransform)?.ScaleX:0.###} " +
                    $"| ink box x={box.X:0.##} w={box.Width:0.##} in control space");
            }
        }
    }

    /// <summary>The run a cell draws with, reaching through whatever panel the crop host lays out.</summary>
    private static FrameworkElement? RunOf(Panel host)
    {
        for (var index = 0; index < host.Children.Count; index++)
        {
            if (host.Children[index] is not FrameworkElement child) continue;
            if (child is TextBlock or Image) return child;
            if (child is Panel nested)
            {
                var deeper = RunOf(nested);
                if (deeper is not null) return deeper;
            }
        }

        return null;
    }

    private static Rect BoxIn(FrameworkElement element, Visual ancestor)
    {
        var transform = element.TransformToVisual(ancestor);
        if (transform is null) return Rect.Empty;
        return transform.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
    }

    private static void Walk(Visual node, Action<Visual> visit)
    {
        visit(node);
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            if (VisualTreeHelper.GetChild(node, index) is Visual child) Walk(child, visit);
        }
    }

    /// <summary>Reads an internal member. This is a probe, so reflection is allowed here; in src/FluentJalium the
    /// same call is what the structure gate rejects.</summary>
    private static T Read<T>(object target, string name)
    {
        var type = target.GetType();
        while (type is not null)
        {
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property?.GetValue(target) is T value) return value;
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field?.GetValue(target) is T fieldValue) return fieldValue;
            type = type.BaseType;
        }

        return default!;
    }

    private static void Pump(int frames, int budgetMilliseconds)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var seen = 0;
        var deadline = Environment.TickCount64 + budgetMilliseconds;
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames || Environment.TickCount64 > deadline) frame.Continue = false;
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        using var watchdog = new System.Threading.Timer(_ => dispatcher.InvokeAsync(() => frame.Continue = false));
        watchdog.Change(TimeSpan.FromMilliseconds(budgetMilliseconds * 2L), System.Threading.Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
    }
}
