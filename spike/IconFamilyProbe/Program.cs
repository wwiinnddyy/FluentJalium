using System.Reflection;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Interop;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

namespace IconFamilyProbe;

/// <summary>
/// The four questions that decide the icon-family slice before any of it is written, asked of the pinned 26.10.9
/// runtime with real mounted elements.
/// </summary>
/// <remarks>
/// 1. Does a native icon build a visual at all? <c>IconElement</c> derives from <c>FrameworkElement</c>, not
///    <c>Control</c>, so there is no template to swap and no part to name: either the type paints itself or the
///    family cannot be used the way upstream's is.
/// 2. What box does it ask for, and is that box the upstream one (upstream's <c>SymbolIcon</c> is a TextBlock
///    whose FontSize is set in <c>ApplyTemplate</c>, and its layout size is documented at 16x16/20x20 - the
///    number here is measured, not assumed).
/// 3. Can markup reach the properties? An attribute this runtime does not know parses clean and does nothing, so
///    the only reading that counts is the value back off the loaded element.
/// 4. Does any of it reach a capture? Glyph ink has never reached either capture path on this runtime
///    (adaptation/00 S1-r clause 3), so a blank picture here would be ambiguous on its own - which is why the
///    mounted-child read-back runs first and the ink is reported beside it, not instead of it.
/// </remarks>
internal static class Program
{
    private static readonly List<string> Lines = [];

    private static readonly Color Accent = Color.FromRgb(0x00, 0x78, 0xD4);

    private const string Root =
        "xmlns=\"http://schemas.jalium.ui/2024\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" " +
        "xmlns:c=\"clr-namespace:Jalium.UI.Controls;assembly=Jalium.UI.Controls\"";

    private static Grid _root = new();

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
            var window = new Window { Width = 320, Height = 240, Content = _root };
            window.Show();
            Pump(8);

            Mounted();
            Markup();
            Ink();
            Control();
            Hosts();
        }
        catch (Exception exception)
        {
            Note("BOOT", "threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }

        var path = Path.Combine(AppContext.BaseDirectory, "icon-probe.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        foreach (var line in Lines)
        {
            Console.WriteLine(line);
        }

        return 0;
    }

    // --- [1] what a mounted native icon really is ----------------------------------------------------
    private static void Mounted()
    {
        Heading("[1] mounted");
        var symbol = new SymbolIcon { Symbol = Symbol.Add };
        Mount(symbol);
        Report("SymbolIcon{Add}", symbol);

        var font = new FontIcon { Glyph = "\uE710", FontSize = 20 };
        Mount(font);
        Report("FontIcon{E710,20}", font);

        var path = new PathIcon { Data = Geometry.Parse("M 0,0 L 16,16") };
        Mount(path);
        Report("PathIcon{line}", path);

        // Does the symbol selection move the child's text at all, or is the visual built once and frozen?
        var moved = new SymbolIcon { Symbol = Symbol.Remove };
        Mount(moved);
        Report("SymbolIcon{Remove}", moved);
        var firstText = ChildText(moved);
        moved.Symbol = Symbol.Add;
        _root.UpdateLayout();
        Pump(4);
        Note("SymbolIcon{Add after a live change}", "child text now = " + Describe(ChildText(moved)) +
             "; before the change it was " + Describe(firstText));
        Unmount(moved);
    }

    private static void Report(string label, FrameworkElement element)
    {
        var children = ChildrenOf(element);
        Note(label, $"desired={element.DesiredSize.Width:0.##}x{element.DesiredSize.Height:0.##} " +
             $"actual={element.ActualWidth:0.##}x{element.ActualHeight:0.##} visual children={children.Count}");
        foreach (var child in children)
        {
            Note(label, "  child " + Describe(child));
        }

        Note(label, "  Foreground=" + (element is IconElement icon ? Describe(icon.Foreground) : "<not an IconElement>") +
             "; declared FontSize? " + (element.GetType().GetProperty("FontSize")?.Name ?? "no"));
    }

    private static string ChildText(FrameworkElement element)
    {
        foreach (var child in ChildrenOf(element))
        {
            var text = child.GetType().GetProperty("Text");
            var glyph = child.GetType().GetProperty("Glyph");
            var value = text?.GetValue(child) as string ?? glyph?.GetValue(child) as string;
            if (value is not null)
            {
                return value.Length == 1 ? $"{value} U+{(int)value[0]:X4}" : value;
            }
        }

        return "<none>";
    }

    private static string Describe(object? value) => value switch
    {
        null => "<null>",
        FrameworkElement element => element.GetType().Name +
            (element is TextBlock text
                ? $" Text={Codepoints(text.Text)} FontFamily={element.GetType().GetProperty("FontFamily")?.GetValue(element)} " +
                  $"FontSize={element.GetType().GetProperty("FontSize")?.GetValue(element)}"
                : string.Empty) +
            $" desired={element.DesiredSize.Width:0.##}x{element.DesiredSize.Height:0.##}",
        string text => Codepoints(text),
        Brush brush => brush.GetType().Name + (brush is SolidColorBrush solid ? $" #{solid.Color}" : string.Empty),
        _ => value.ToString() ?? "<unprintable>",
    };

    private static string Codepoints(string? text) => string.IsNullOrEmpty(text)
        ? "\"\""
        : "\"" + text + "\" U+" + string.Join(" ", text.Select(ch => ((int)ch).ToString("X4")));

    // --- [2] can markup reach it ----------------------------------------------------------------------
    private static void Markup()
    {
        Heading("[2] markup");
        Load("SymbolIcon by name", "<c:SymbolIcon Symbol=\"Add\" />");
        Load("SymbolIcon by hex", "<c:SymbolIcon Symbol=\"59152\" />");
        Load("FontIcon", "<c:FontIcon Glyph=\"&#xE710;\" FontSize=\"20\" />");
        Load("PathIcon", "<c:PathIcon Data=\"M 0,0 L 16,16 Z\" />");
        Load("unknown symbol name", "<c:SymbolIcon Symbol=\"NoSuchMember\" />");
    }

    private static void Load(string label, string markup)
    {
        var file = Path.Combine(AppContext.BaseDirectory, "probe-element.jalxaml");
        File.WriteAllText(file, $"<Grid {Root}>{markup}</Grid>");
        try
        {
            using var stream = File.OpenRead(file);
            if (XamlReader.Load(stream) is not Grid grid || grid.Children.Count == 0)
            {
                Note(label, "load produced no child");
                return;
            }

            var child = grid.Children[0];
            grid.Children.RemoveAt(0);
            if (child is not FrameworkElement element)
            {
                Note(label, "child was " + child.GetType().Name);
                return;
            }

            Mount(element);
            var symbol = element.GetType().GetProperty("Symbol")?.GetValue(element);
            var glyph = element.GetType().GetProperty("Glyph")?.GetValue(element) as string;
            Note(label, $"type={element.GetType().Name} symbol={symbol ?? "<n/a>"}" +
                 (symbol is null ? string.Empty : $" (0x{Convert.ToInt32(symbol):X4})") +
                 $" glyph={(glyph is null ? "<n/a>" : Codepoints(glyph))} " +
                 $"children={ChildrenOf(element).Count} desired={element.DesiredSize.Width:0.##}x{element.DesiredSize.Height:0.##} " +
                 $"children-after-layout=[{string.Join(" | ", ChildrenOf(element).Select(Describe))}]");
            Unmount(element);
        }
        catch (Exception exception)
        {
            Note(label, "THREW " + exception.GetType().Name + ": " + Trim(exception.Message));
        }
    }

    // --- [3] ink, through an ancestor wide enough to see a transform ----------------------------------
    private static void Ink()
    {
        Heading("[3] ink");
        foreach (var (label, element) in new (string, FrameworkElement)[]
                 {
                     ("SymbolIcon{Add}", new SymbolIcon { Symbol = Symbol.Add }),
                     ("FontIcon{E710}", new FontIcon { Glyph = "\uE710", FontSize = 20 }),
                     ("PathIcon{line}", new PathIcon { Data = Geometry.Parse("M 0,0 L 16,16") }),
                 })
        {
            // A panel behind the icon: an entirely empty capture cannot tell "nothing was drawn" from "the icon
            // is 0x0", and the backing colour is what makes the box's own extent readable.
            var host = new Grid { Width = 64, Height = 64, Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)) };
            host.Children.Add(element);
            Mount(host);
            var buffer = Grab(host, 64, 64);
            var nonWhite = 0;
            var accent = 0;
            for (var offset = 0; offset + 3 < buffer.Length; offset += 4)
            {
                var b = buffer[offset];
                var g = buffer[offset + 1];
                var r = buffer[offset + 2];
                if (r != 0xFF || g != 0xFF || b != 0xFF)
                {
                    nonWhite++;
                }

                if (Math.Abs(r - Accent.R) < 40 && Math.Abs(g - Accent.G) < 40 && Math.Abs(b - Accent.B) < 40)
                {
                    accent++;
                }
            }

            Note(label, $"over a 64x64 white field: non-white px={nonWhite}, accent-ish px={accent}, " +
                 $"icon desired={element.DesiredSize.Width:0.##}x{element.DesiredSize.Height:0.##}");
            Unmount(host);
        }

        // A solid-colour PathIcon is the control that tells the two apart: if a filled geometry prints and a glyph
        // does not, the blank glyph is the judge's gap and not the control's.
        var filled = new PathIcon { Data = Geometry.Parse("M 0,0 L 20,0 L 20,20 L 0,20 Z"), Foreground = new SolidColorBrush(Accent) };
        var panel = new Grid { Width = 64, Height = 64, Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)) };
        panel.Children.Add(filled);
        Mount(panel);
        var fillBuffer = Grab(panel, 64, 64);
        var accentPixels = 0;
        for (var offset = 0; offset + 3 < fillBuffer.Length; offset += 4)
        {
            if (Math.Abs(fillBuffer[offset + 2] - Accent.R) < 40 && Math.Abs(fillBuffer[offset + 1] - Accent.G) < 40 &&
                Math.Abs(fillBuffer[offset] - Accent.B) < 40)
            {
                accentPixels++;
            }
        }

        Note("PathIcon{20x20 solid block}", $"accent-ish px={accentPixels} (expected near 400 if a fill prints)");
        Unmount(panel);
    }

    // --- [4] the discriminator: is a blank icon the control's or the judge's? --------------------------
    // Glyph ink has never reached a capture on this runtime (adaptation/00 S1-r clause 3), so "SymbolIcon printed
    // nothing" is only evidence if something that must print does. Three controls run in the same field: a plain
    // TextBlock, an icon-font TextBlock, and a solid PathIcon. On top of them the icon is re-run with an explicit
    // Foreground, because a null brush is one explanation for a blank that a style can fix without a new type.
    private static void Control()
    {
        Heading("[4] control");
        Field("TextBlock plain text", new TextBlock { Text = "IIII", FontSize = 20, Foreground = Brush(Accent) });
        Field("TextBlock glyph U+E710", new TextBlock { Text = "", FontSize = 20, FontFamily = new FontFamily("Segoe Fluent Icons"), Foreground = Brush(Accent) });
        Field("SymbolIcon default brush", new SymbolIcon { Symbol = Symbol.Add });
        Field("SymbolIcon accent brush", new SymbolIcon { Symbol = Symbol.Add, Foreground = Brush(Accent) });
        Field("FontIcon accent brush", new FontIcon { Glyph = "", FontSize = 20, Foreground = Brush(Accent) });
        Field("Border 20x20 accent", new Border { Width = 20, Height = 20, Background = Brush(Accent) });
        Field("PathIcon closed block", new PathIcon { Data = Geometry.Parse("M 0,0 L 20,0 L 20,20 L 0,20 Z"), Foreground = Brush(Accent) });
        Field("PathIcon closed block, 20x20 explicit", new PathIcon { Data = Geometry.Parse("M 0,0 L 20,0 L 20,20 L 0,20 Z"), Foreground = Brush(Accent), Width = 20, Height = 20, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top });

        // Nothing above can tell "the type draws no glyph" from "the judge cannot see a glyph", so the question
        // moves to measurement: a type that hosts a text run has to report a box that follows the run's size, and
        // one that reports a constant does not hold a run at all. TextBlock is the control that must scale.
        Heading("[4b] does the box follow the content?");
        foreach (var size in new[] { 8d, 20d, 40d })
        {
            Field($"TextBlock glyph at {size:0}", new TextBlock { Text = "", FontSize = size, FontFamily = new FontFamily("Segoe Fluent Icons") });
            Field($"FontIcon at {size:0}", new FontIcon { Glyph = "", FontSize = size });
        }

        foreach (var symbol in new[] { Symbol.Add, Symbol.GlobalNavButton, Symbol.Calories, Symbol.Delete })
        {
            Field($"SymbolIcon {symbol}", new SymbolIcon { Symbol = symbol });
        }

        // A subclass is the route an adaptation takes when the native type cannot be retemplated: IconElement is
        // not a Control, so there is no Template to hand it, and the only way in is to derive. That it compiles at
        // all is the fact this line records, plus what the override is asked for.
        var probe = new ProbeIcon { Symbol = Symbol.Add };
        Field("ProbeIcon : IconElement override", probe);
        Note("subclass", $"MeasureOverride called {probe.MeasureCalls} time(s), base desired={probe.BaseDesired.Width:0.##}x{probe.BaseDesired.Height:0.##}");

        // And the same question for the glyph the native type would have to draw: is there a member at all that
        // reports what the icon shows?
        Note("SymbolIcon members", string.Join(", ", typeof(SymbolIcon)
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(p => p.Name)));
        Note("FontIcon members", string.Join(", ", typeof(FontIcon)
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(p => p.Name)));
        Note("IconElement methods", string.Join(", ", typeof(IconElement)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.Name.StartsWith("Measure", StringComparison.Ordinal) || m.Name.Contains("Render", StringComparison.Ordinal) ||
                        m.Name.Contains("Content", StringComparison.Ordinal))
            .Select(m => m.Name + "/" + m.GetParameters().Length).Distinct()));
    }

    private sealed class ProbeIcon : IconElement
    {
        public int MeasureCalls;

        public Size BaseDesired;

        public Symbol Symbol { get; set; }

        protected override Size MeasureOverride(Size constraint)
        {
            MeasureCalls++;
            BaseDesired = base.MeasureOverride(constraint);
            return new Size(24, 24);
        }
    }


    private static void Field(string label, FrameworkElement element)
    {
        var host = new Grid { Width = 64, Height = 64, Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)) };
        host.Children.Add(element);
        Mount(host);
        var buffer = Grab(host, 64, 64);
        var nonWhite = 0;
        var accent = 0;
        var minX = 64;
        var minY = 64;
        var maxX = -1;
        var maxY = -1;
        for (var y = 0; y < 64; y++)
        {
            for (var x = 0; x < 64; x++)
            {
                var offset = (y * 64 + x) * 4;
                var b = buffer[offset];
                var g = buffer[offset + 1];
                var r = buffer[offset + 2];
                if (r != 0xFF || g != 0xFF || b != 0xFF)
                {
                    nonWhite++;
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }

                if (Math.Abs(r - Accent.R) < 24 && Math.Abs(g - Accent.G) < 24 && Math.Abs(b - Accent.B) < 24)
                {
                    accent++;
                }
            }
        }

        Note(label, $"non-white={nonWhite}, accent={accent}, ink box={(maxX < 0 ? "<empty>" : $"{maxX - minX + 1}x{maxY - minY + 1} at {minX},{minY}")}, " +
            $"desired={element.DesiredSize.Width:0.##}x{element.DesiredSize.Height:0.##}, actual={element.ActualWidth:0.##}x{element.ActualHeight:0.##}");
        Unmount(host);
    }

    private static SolidColorBrush Brush(Color color) => new(color);

    // --- [5] as someone else's content ----------------------------------------------------------------
    private static void Hosts()
    {
        Heading("[5] hosted");
        foreach (var (label, icon) in new (string, IconElement)[]
                 {
                     ("SymbolIcon", new SymbolIcon { Symbol = Symbol.Add }),
                     ("FontIcon", new FontIcon { Glyph = "\uE710" }),
                     ("PathIcon", new PathIcon { Data = Geometry.Parse("M 0,0 L 16,16") }),
                 })
        {
            var button = new Button { Content = icon, Width = 120, Height = 40 };
            Mount(button);
            var walk = Walk(button, 0, new List<string>());
            var found = walk.Count(line => line.Contains(icon.GetType().Name, StringComparison.Ordinal));
            Note(label + " as Button.Content",
                $"icon appears in the button's visual tree: {found > 0}; icon desired={icon.DesiredSize.Width:0.##}x{icon.DesiredSize.Height:0.##}");
            if (label == "SymbolIcon")
            {
                foreach (var line in walk.Where(line => line.Contains("Icon", StringComparison.Ordinal) ||
                                                        line.Contains("ContentPresenter", StringComparison.Ordinal)).Take(8))
                {
                    Note(label + " as Button.Content", "  " + line);
                }
            }

            Unmount(button);
        }
    }

    private static List<string> Walk(DependencyObject visual, int depth, List<string> into)
    {
        into.Add(new string(' ', depth * 2) + Describe(visual));
        foreach (var child in Children(visual))
        {
            Walk(child, depth + 1, into);
        }

        return into;
    }

    // --- shared plumbing ------------------------------------------------------------------------------
    private static List<FrameworkElement> ChildrenOf(FrameworkElement element) =>
        Children(element).OfType<FrameworkElement>().ToList();

    private static List<DependencyObject> Children(DependencyObject visual)
    {
        var list = new List<DependencyObject>();
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
        {
            list.Add(VisualTreeHelper.GetChild(visual, index));
        }

        return list;
    }

    private static void Mount(FrameworkElement element)
    {
        _root.Children.Clear();
        _root.Children.Add(element);
        _root.UpdateLayout();
        Pump(6);
    }

    private static void Unmount(FrameworkElement element)
    {
        _root.Children.Remove(element);
        Pump(2);
    }

    private static byte[] Grab(Visual target, int width, int height)
    {
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(target);
        var stride = width * 4;
        var buffer = new byte[stride * height];
        bitmap.CopyPixels(new Int32Rect(0, 0, width, height), buffer, stride, 0);
        return buffer;
    }

    private static void Pump(int frames, int budgetMilliseconds = 800)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var seen = 0;
        var deadline = Environment.TickCount64 + budgetMilliseconds;
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames || Environment.TickCount64 > deadline)
            {
                frame.Continue = false;
            }
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        using var watchdog = new System.Threading.Timer(_ => dispatcher.InvokeAsync(() => frame.Continue = false));
        watchdog.Change(TimeSpan.FromMilliseconds(budgetMilliseconds * 2L), System.Threading.Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
    }

    private static void Heading(string title) => Note(title, string.Empty);

    private static void Note(string label, string text) => Lines.Add($"[{label}] {text}".TrimEnd());

    private static string Trim(string text) => text.Length <= 220 ? text : text[..220] + "...";
}
