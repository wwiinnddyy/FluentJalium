using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace TextWrapProbe;

/// <summary>
/// Throwaway probe: why does a wrapping TextBlock that needs two lines get one line of arrange, so the
/// next sibling is painted on top of it? Reproduces the Gallery footer's container shape and the info
/// bar's templated panel and prints the measure result against the arranged size.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "wrap-probe1.txt");

    private const string LongOne =
        "Inputs - AutoCompleteBox audited, FluentToggleSwitch own-type, NumberBox audited, PasswordBox audited, Slider audited, TextBox audited - 6 of 36 restyled types";

    private const string LongTwo =
        "Not claimed: upstream's AutoSuggestBox has no type here: the surface is the native AutoCompleteBox, so the 5 transcribed rows only earn their place where that type has a property or a part to land on.";

    [STAThread]
    private static int Main()
    {
        try
        {
            var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
            renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            var application = new Application();
            FluentThemeManager.Apply(application);
            Run(application);
        }
        catch (Exception exception)
        {
            Note("BOOT threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }

        File.WriteAllText(LogPath, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, full log: {LogPath}");
        return 0;
    }

    private static void Run(Application application)
    {
        // Case 1: the shell's exact shape - a two-row grid whose second row spans a star column and an
        // auto column, holding a vertical stack of two wrapping captions.
        var grid = new Grid { Width = 600 };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var parity = new StackPanel { Orientation = Orientation.Vertical };
        var parityStatus = Text("parityStatus", LongOne);
        var parityGaps = Text("parityGaps", LongTwo);
        parityGaps.Margin = new Thickness(0, 2, 0, 0);
        parity.Children.Add(parityStatus);
        parity.Children.Add(parityGaps);
        Grid.SetRow(parity, 1);
        Grid.SetColumnSpan(parity, 2);
        parity.Margin = new Thickness(0, 10, 0, 0);

        grid.Children.Add(new Button { Content = "Clear" });
        Grid.SetColumn((UIElement)grid.Children[0], 1);
        grid.Children.Add(parity);

        // Case 2: identical stack, no grid, no span.
        var plain = new StackPanel { Orientation = Orientation.Vertical, Width = 600 };
        var plainStatus = Text("plainStatus", LongOne);
        var plainGaps = Text("plainGaps", LongTwo);
        plain.Children.Add(plainStatus);
        plain.Children.Add(plainGaps);

        // Case 3: the info bar's own templated panel, with a message long enough to wrap.
        var bar = new FluentInfoBar
        {
            Title = "Weekly digest ready",
            Message = LongTwo,
            Width = 600,
            IsIconVisible = false,
            IsClosable = false,
        };

        // Case 4: the same bar with the message set before the template is ever asked to measure twice.
        var barEarly = new FluentInfoBar { Width = 600, IsIconVisible = false, IsClosable = false };
        barEarly.Message = LongTwo;

        var root = new StackPanel { Orientation = Orientation.Vertical };
        root.Children.Add(grid);
        root.Children.Add(plain);
        root.Children.Add(bar);
        root.Children.Add(barEarly);

        var window = new Window
        {
            Content = root,
            Width = 900,
            Height = 900,
            Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
        };
        window.Show();
        Pump(24, 1500);

        Note("=== A. measure against arrange ===");
        Report("case1 parityStatus", parityStatus, grid);
        Report("case1 parityGaps", parityGaps, grid);
        Report("case2 plainStatus", plainStatus, plain);
        Report("case2 plainGaps", plainGaps, plain);

        Note("=== B. the info bar's templated panel ===");
        Note($"  bar: actual={bar.ActualWidth:0.#}x{bar.ActualHeight:0.#} desired={bar.DesiredSize:0.##}");
        ReportTemplate(bar);
        Note($"  barEarly: actual={barEarly.ActualWidth:0.#}x{barEarly.ActualHeight:0.#}");
        ReportTemplate(barEarly);

        Note("=== C. does a second measure pass fix it? ===");
        grid.Measure(new Size(600, 2000));
        grid.Arrange(new Rect(new Point(grid.TranslatePoint(new Point(0, 0), root).X, grid.TranslatePoint(new Point(0, 0), root).Y), grid.DesiredSize));
        Pump(6, 400);
        Report("after-remeasure parityStatus", parityStatus, grid);
        Report("after-remeasure parityGaps", parityGaps, grid);

        window.Close();
        Pump(4, 400);

        Note("=== D. does the navigation host measure its content at another width than it arranges? ===");
        var navStack = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(24) };
        var navStatus = Text("navStatus", LongOne);
        var navGaps = Text("navGaps", LongTwo);
        navStack.Children.Add(navStatus);
        navStack.Children.Add(navGaps);
        var nav = new FluentNavigationView { Content = navStack };
        var navWindow = new Window
        {
            Content = nav,
            Width = 1100,
            Height = 700,
            Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
        };
        navWindow.Show();
        Pump(24, 1500);
        Report("nav navStatus", navStatus, navStack);
        Report("nav navGaps", navGaps, navStack);
        Note($"  nav: actual={nav.ActualWidth:0.#}x{nav.ActualHeight:0.#} stack={navStack.ActualWidth:0.#}x{navStack.ActualHeight:0.#}");
        navWindow.Close();
        Pump(4, 400);

        Note("=== E. the same footer, but with the text assigned after the first layout ===");
        var lateGrid = new Grid { Width = 600 };
        lateGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        lateGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        lateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        lateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var lateStack = new StackPanel { Orientation = Orientation.Vertical };
        var lateStatus = Text("lateStatus", string.Empty);
        var lateGaps = Text("lateGaps", string.Empty);
        lateStack.Children.Add(lateStatus);
        lateStack.Children.Add(lateGaps);
        Grid.SetRow(lateStack, 1);
        Grid.SetColumnSpan(lateStack, 2);
        lateGrid.Children.Add(new Button { Content = "Clear" });
        Grid.SetColumn((UIElement)lateGrid.Children[0], 1);
        lateGrid.Children.Add(lateStack);

        var lateWindow = new Window { Content = lateGrid, Width = 900, Height = 500 };
        lateWindow.Show();
        Pump(16, 900);
        Note("  before the text lands:");
        Report("   lateStatus", lateStatus, lateStack);
        Report("   lateGaps", lateGaps, lateStack);

        lateStatus.Text = LongOne;
        lateGaps.Text = LongTwo;
        Pump(16, 900);
        Note("  after the text lands:");
        Report("   lateStatus", lateStatus, lateStack);
        Report("   lateGaps", lateGaps, lateStack);

        lateStack.InvalidateMeasure();
        lateGrid.InvalidateMeasure();
        Pump(16, 900);
        Note("  after an explicit invalidate on the stack and the grid:");
        Report("   lateStatus", lateStatus, lateStack);
        Report("   lateGaps", lateGaps, lateStack);

        lateWindow.Close();
        Pump(4, 400);
    }

    private static TextBlock Text(string name, string text) => new()
    {
        Name = name,
        Text = text,
        TextWrapping = TextWrapping.Wrap,
        FontSize = 12,
    };

    private static void Report(string label, TextBlock block, UIElement container)
    {
        var offset = TryOffset(block, container);
        var containerSize = container is FrameworkElement frame ? $"{frame.ActualWidth:0.#}x{frame.ActualHeight:0.#}" : "n/a";
        Note($"  {label}: desired={block.DesiredSize.Width:0.#}x{block.DesiredSize.Height:0.#} " +
             $"actual={block.ActualWidth:0.#}x{block.ActualHeight:0.#} offsetInContainer={offset:0.#} " +
             $"containerActual={containerSize}");
    }

    private static void ReportTemplate(Control control)
    {
        foreach (var name in new[] { "Title", "Message", "Panel", "RootBorder" })
        {
            var found = FindByName(control, name, 0);
            if (found is FrameworkElement element)
            {
                Note($"    {name}: desired={element.DesiredSize.Width:0.#}x{element.DesiredSize.Height:0.#} actual={element.ActualWidth:0.#}x{element.ActualHeight:0.#}");
            }
            else
            {
                Note($"    {name}: not found in tree");
            }
        }
    }

    private static DependencyObject? FindByName(DependencyObject node, string name, int depth)
    {
        if (depth > 8)
        {
            return null;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            var child = VisualTreeHelper.GetChild(node, index)!;
            if (child is FrameworkElement element && element.Name == name)
            {
                return element;
            }

            if (FindByName(child, name, depth + 1) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    private static double TryOffset(UIElement element, UIElement relativeTo)
    {
        try
        {
            return element.TranslatePoint(new Point(0, 0), relativeTo).Y;
        }
        catch (Exception)
        {
            return double.NaN;
        }
    }

    private static int Pump(int frames = 8, int budgetMilliseconds = 800)
    {
        var frame = new DispatcherFrame();
        var seen = 0;
        var deadline = DateTime.UtcNow.AddMilliseconds(budgetMilliseconds);
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames || DateTime.UtcNow > deadline) frame.Continue = false;
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
        return seen;
    }

    private static void Note(string line) => Lines.Add(line);

    private static string Trim(string text) => text.Replace('\r', ' ').Replace('\n', ' ').Trim();
}
