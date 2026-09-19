using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Markup;
using Jalium.UI.Interop;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace RightGapProbe;

/// <summary>
/// The "right gap" question, made measurable. The reading from the running Gallery is that several controls
/// carry a wider empty band on their right than the inset their content starts at on the left. This lays the
/// suspect controls out once, all at the same width, on a shown window, prints each one's own geometry, and
/// holds for an outside PrintWindow capture - so a gap becomes a number twice over, once from the tree and
/// once from the frame.
///   --open closed   : every control at rest.
///   --open suggest  : the AutoCompleteBox with its suggestion list dropped, the pass that showed the hole.
///   --open combo    : the combo box dropped.
///   --quick         : hold two seconds instead of forty-five, for a run that only reads the tree.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];

    private static string Mode = "closed";

    [STAThread]
    private static int Main(string[] arguments)
    {
        for (var index = 0; index < arguments.Length; index++)
        {
            if (arguments[index] == "--open" && index + 1 < arguments.Length)
            {
                Mode = arguments[index + 1];
            }
        }

        try
        {
            var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
            renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            var application = new Application();
            FluentThemeManager.Apply(application);
            Run(Mode, arguments);
        }
        catch (Exception exception)
        {
            Note("BOOT threw " + exception.GetType().Name + ": " + (exception.InnerException?.Message ?? exception.Message));
        }

        var path = Path.Combine(AppContext.BaseDirectory, $"right-gap-{Mode}.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, full log: {path}");
        return 0;
    }

    private static void Run(string mode, string[] arguments)
    {
        var panel = new StackPanel { Margin = new Thickness(24), Spacing = 10 };
        var window = new Window
        {
            Content = panel,
            Width = 760,
            Height = 900,
            Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
        };

        var combo = new ComboBox { Width = 260 };
        foreach (var name in new[] { "Compact", "Comfortable", "Cozy" })
        {
            combo.Items.Add(name);
        }

        combo.SelectedIndex = 0;

        var suggest = new AutoCompleteBox { Width = 260, Text = "Ap" };
        suggest.ItemsSource = new List<string> { "Apple", "Apricot", "Avocado" };

        var number = new NumberBox { Width = 260, Value = 42, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline };
        var split = new SplitButton { Width = 260, Content = "Split primary" };
        var dropDown = new FluentDropDownButton { Width = 260, Content = "Drop-down" };
        var expander = new Expander { Width = 260, Header = "Weekly digest", Content = "the body" };
        var navItem = new FluentNavigationItem { Content = "Teaching tip and dialogs", Width = 260 };

        Add(panel, "TextBox", new TextBox { Width = 260, Text = "Astra" });
        Add(panel, "PasswordBox", new PasswordBox { Width = 260, Password = "secret" });
        Add(panel, "ComboBox", combo);
        Add(panel, "NumberBox", number);
        Add(panel, "AutoCompleteBox", suggest);
        Add(panel, "SplitButton", split);
        Add(panel, "FluentDropDownButton", dropDown);
        Add(panel, "Expander", expander);
        Add(panel, "Nav item (long label)", navItem);

        window.Show();
        Pump(20, 1500);

        if (mode == "suggest")
        {
            // The suggestion list opens on its own once the text is edited after the box is on screen, and the
            // combo has to stay out of the way: focus moves close it again.
            window.Activate();
            suggest.Focus();
            suggest.Text = "Apr";
            Pump(40, 2500);
            Note("-- suggestion list open --");
            FindNamed(window, 0);
        }
        else if (mode == "combo")
        {
            window.Activate();
            combo.Focus();
            combo.IsDropDownOpen = true;
            Pump(40, 2500);
            Note($"-- combo box open={combo.IsDropDownOpen} --");
            FindNamed(window, 0);
        }

        foreach (var (label, control) in new (string, Control)[]
                 {
                     ("ComboBox", combo), ("NumberBox", number), ("AutoCompleteBox", suggest),
                     ("SplitButton", split), ("DropDownButton", dropDown), ("Expander", expander),
                     ("Nav item", navItem),
                 })
        {
            Report(label, control);
        }

        Note("holding for an outside capture");
        Hold(arguments.Contains("--quick") ? 2 : 45);
        window.Close();
        Pump(4, 400);
    }

    private static void Add(Panel panel, string label, Control control)
    {
        panel.Children.Add(new TextBlock { Text = label, FontSize = 11, Foreground = new SolidColorBrush(Color.FromRgb(0x9A, 0x9A, 0x9A)) });
        panel.Children.Add(control);
    }

    private static readonly string[] Interesting =
    [
        "PART_Popup", "PART_PopupBorder", "SuggestionsContainer", "PART_DropDownBorder",
        "PART_DropDownScrollViewer", "PART_DropDownItemsHost", "PopupContentRoot", "UpDownPopup",
    ];

    /// <summary>
    /// Popup children are re-parented into the host window's overlay layer, so the only way to read what the
    /// framework did to their width is to walk down from the window rather than from the control.
    /// </summary>
    private static void FindNamed(DependencyObject node, int depth)
    {
        if (depth > 14)
        {
            return;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            var child = VisualTreeHelper.GetChild(node, index);
            if (child is FrameworkElement { Name: { Length: > 0 } } element && Interesting.Contains(element.Name))
            {
                var width = DependencyProperty.FromName(child.GetType(), "Width");
                var minWidth = DependencyProperty.FromName(child.GetType(), "MinWidth");
                var min = minWidth is null ? "?" : element.GetValue(minWidth);
                var localMin = minWidth is null ? "?" : element.ReadLocalValue(minWidth);
                var wide = width is null ? "?" : element.GetValue(width);
                var localWide = width is null ? "?" : element.ReadLocalValue(width);
                Note($"popup part {child.GetType().Name} \"{element.Name}\" {element.ActualWidth:0.#}x{element.ActualHeight:0.#}"
                     + $" min={min} localMin={localMin} width={wide} localWidth={localWide}");
                Console.Out.WriteLine(element.Name);
                Console.Out.Flush();
            }

            FindNamed(child, depth + 1);
        }
    }

    private static void Report(string label, Control control)
    {
        Note($"== {label} {control.GetType().Name} actual={control.ActualWidth:0.#}x{control.ActualHeight:0.#} "
             + $"padding={Property(control, "Padding")} margin={control.Margin}");
        Walk(control, 1);
    }

    private static void Walk(DependencyObject node, int depth)
    {
        if (depth > 5)
        {
            return;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            var child = VisualTreeHelper.GetChild(node, index);
            if (child is FrameworkElement element)
            {
                var name = element.Name;
                var type = child.GetType().Name;
                if (name.Length > 0 || type.Contains("Presenter", StringComparison.Ordinal) || type.Contains("Path", StringComparison.Ordinal)
                    || type.Contains("Toggle", StringComparison.Ordinal) || type.Contains("Thumb", StringComparison.Ordinal))
                {
                    var indent = new string(' ', depth * 2);
                    Note($"  {indent}{type} \"{name}\" {element.ActualWidth:0.#}x{element.ActualHeight:0.#}"
                         + $" margin={element.Margin} padding={Property(element, "Padding")} align={element.HorizontalAlignment}");
                }
            }

            Walk(child, depth + 1);
        }
    }

    private static string Property(object target, string name)
    {
        var value = target.GetType().GetProperty(name)?.GetValue(target);
        return value?.ToString() ?? "-";
    }

    private static void Hold(int seconds)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var deadline = DateTime.UtcNow.AddSeconds(seconds);
        void OnRendering(object? sender, EventArgs arguments)
        {
            if (DateTime.UtcNow > deadline) frame.Continue = false;
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        using var timer = new System.Threading.Timer(
            _ => dispatcher.InvokeAsync(() => frame.Continue = false),
            null,
            TimeSpan.FromSeconds(seconds + 1),
            Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
    }

    private static int Pump(int frames, int budgetMilliseconds)
    {
        var count = 0;
        var frame = new DispatcherFrame();
        var start = DateTime.UtcNow;
        void OnRendering(object? sender, EventArgs arguments)
        {
            count++;
            if (count >= frames || (DateTime.UtcNow - start).TotalMilliseconds > budgetMilliseconds) frame.Continue = false;
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
        return count;
    }

    private static void Note(string line) => Lines.Add(line);
}
