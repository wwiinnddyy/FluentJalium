// Census: pass 2 of the native tab levers - attribution, not just presence.
//
// Pass 1 (spike/TabViewTint/tint-1.txt) settled which levers paint at all: TabStripBackground, TabItem.Background,
// TabItem.SelectedBackground and TabItem.IndicatorBrush + IndicatorHeight reach pixels, an implicit
// Style TargetType=TabItem from application resources applies (padding, min height and fill all landed), while an
// assigned ControlTemplate is recorded and never built. Three readings were inconclusive and are what this pass is
// for: the label Foreground was tinted #112233, which is the same luminance as the #1C1C1E surface it sits on, so
// "zero pixels" measured nothing; the item fills were all the same colour, so a single item's worth of ink could not
// be attributed to selected-versus-index; and TabStripBorderBrush produced a blended green that could not be told
// apart from a green the framework already paints at rest. Each lever now gets its own colour and a bounding box,
// and the selection is flipped to see which reading follows it.
using System.Reflection;
using System.Text;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

namespace TabViewTint;

internal static class Program
{
    private static readonly List<string> Lines = [];
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "tab-tint2.txt");

    private const string TabsMarkup = """
        <TabControl xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                    Width='{0}' Height='120'>
          <TabItem Header='one'><TextBlock Text='body one' /></TabItem>
          <TabItem Header='two'><TextBlock Text='body two' /></TabItem>
          <TabItem Header='three' IsEnabled='False'><TextBlock Text='body three' /></TabItem>
        </TabControl>
        """;

    private const string CrowdedMarkup = """
        <TabControl xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                    Width='200' Height='120'>
          <TabItem Header='alpha' /><TabItem Header='beta' /><TabItem Header='gamma' />
          <TabItem Header='delta' /><TabItem Header='epsilon' /><TabItem Header='zeta' />
          <TabItem Header='eta' /><TabItem Header='theta' /><TabItem Header='iota' />
          <TabItem Header='kappa' />
        </TabControl>
        """;

    private static readonly uint[] PerItem = [0xFFFF0000, 0xFF00FF00, 0xFF0000FF];
    private static readonly uint[] PerItemSelected = [0xFFFF0000, 0xFF00FF00, 0xFF0000FF];

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
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
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
        var root = new StackPanel { Margin = new Thickness(12) };

        var perItem = (TabControl)XamlReader.Parse(string.Format(TabsMarkup, 420))!;
        var items = Items(perItem);
        for (var index = 0; index < items.Count; index++)
        {
            Set(items[index], "Background", Paint(PerItem[index]));
            Set(items[index], "SelectedBackground", Paint(PerItemSelected[index]));
        }

        var label = (TabControl)XamlReader.Parse(string.Format(TabsMarkup, 420))!;
        foreach (var item in Items(label))
        {
            Set(item, "Foreground", Paint(0xFF00FF00));
            Set(item, "HoverBackground", Paint(0xFFFF00FF));
        }

        var strip = (TabControl)XamlReader.Parse(string.Format(TabsMarkup, 420))!;
        Set(strip, "TabStripBackground", Paint(0xFFFF00FF));
        Set(strip, "TabStripBorderBrush", Paint(0xFF00FF00));
        Set(strip, "TabStripHeight", 60d);

        var indicator = (TabControl)XamlReader.Parse(string.Format(TabsMarkup, 420))!;
        foreach (var item in Items(indicator))
        {
            Set(item, "IndicatorBrush", Paint(0xFFFF00FF));
            Set(item, "IndicatorHeight", 10d);
        }

        var placement = (TabControl)XamlReader.Parse(string.Format(TabsMarkup, 420))!;
        SetByEnum(placement, "TabStripPlacement", "Bottom");

        var crowded = (TabControl)XamlReader.Parse(CrowdedMarkup)!;

        foreach (var (name, element) in new (string, FrameworkElement)[]
                 {
                     ("A per-item fills", perItem),
                     ("B label foreground", label),
                     ("C strip background + border + height", strip),
                     ("D indicator brush + height 10", indicator),
                     ("E strip placement bottom", placement),
                     ("F ten tabs in 200 DIP", crowded),
                 })
        {
            root.Children.Add(new TextBlock { Text = name, FontSize = 10 });
            root.Children.Add(element);
        }

        var window = new Window { Content = root, Width = 520, Height = 1400, Title = "TabView tint 2" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Pump(14);

                Note("=== A. per-item fills: which box paints, and does it follow selection? ===");
                Shot("A rest (index 0 selected)", perItem);
                Note($"  item fills read back: {string.Join("  ", Items(perItem).Select((item, index) => $"[{index}]={Hex(Get(item, "Background") as Brush)} sel={Get(item, "IsSelected")}"))}");
                Set(perItem, "SelectedIndex", 1);
                Pump(10);
                Shot("A after SelectedIndex=1", perItem);
                Note($"  item fills read back: {string.Join("  ", Items(perItem).Select((item, index) => $"[{index}]={Hex(Get(item, "Background") as Brush)} sel={Get(item, "IsSelected")}"))}");
                Note("  header band of the flipped selection:");
                Dump("    ", perItem, 3);

                Note(string.Empty);
                Note("=== B. label foreground: bright lime, counted inside the header band ===");
                Shot("B label foreground lime", label);
                Note("  labels in the tree: " + string.Join("  ", Descendants(label, "TextBlock").Take(6).Select(node =>
                    $"\"{Trim(Text(node))}\" fg={Hex(Get(node, "Foreground") as Brush)}")));

                Note(string.Empty);
                Note("=== C. the strip: background, border brush, height 60 ===");
                Shot("C strip", strip);
                Note("  strip reads: " + Try(() =>
                {
                    Note($"    TabStripBackground={Hex(Get(strip, "TabStripBackground") as Brush)} TabStripBorderBrush={Hex(Get(strip, "TabStripBorderBrush") as Brush)} TabStripHeight={Get(strip, "TabStripHeight")}");
                    return "read";
                }));
                Note("  header row geometry: " + HeaderGeometry(strip));

                Note(string.Empty);
                Note("=== D. the selection indicator ===");
                Shot("D indicator height 10", indicator);
                Set(indicator, "SelectedIndex", 2);
                Pump(10);
                Shot("D indicator, selection on the disabled third tab", indicator);

                Note(string.Empty);
                Note("=== E. TabStripPlacement=Bottom ===");
                Note("  read back: " + Get(placement, "TabStripPlacement"));
                Shot("E placement bottom", placement);
                Note("  header row geometry: " + HeaderGeometry(placement));
                Note("  baseline header row geometry: " + HeaderGeometry(label));

                Note(string.Empty);
                Note("=== F. ten tabs in a 200 DIP host: what does the strip do? ===");
                Note("  items: " + string.Join(",", Items(crowded).Select(item => $"{Get(item, "ActualWidth"):0.#}x{Get(item, "ActualHeight"):0.#}")));
                Shot("F crowded", crowded);
                Dump("    ", crowded, 3);

                Note(string.Empty);
                Note("=== G. keyboard: why did pass 1's constructors throw? ===");
                Note("  focus: " + Try(() =>
                {
                    Note($"    Focus() returned {perItem.Focus()}; IsKeyboardFocused={Get(perItem, "IsKeyboardFocused")}");
                    return "called";
                }));
                Keys(perItem);
            }
            catch (Exception exception)
            {
                Note("LOADED threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            }
            finally
            {
                application.Shutdown();
            }
        };
        application.Run(window);
    }

    private static void Keys(UIElement target)
    {
        var type = typeof(KeyEventArgs);
        var device = typeof(Keyboard).GetProperty("PrimaryDevice", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        Note($"  Keyboard.PrimaryDevice={(device?.GetType().Name ?? "null")}");
        var window = Window.GetWindow(target);
        var sourceType = typeof(UIElement).Assembly.GetType("Jalium.UI.PresentationSource")
                         ?? typeof(Control).Assembly.GetType("Jalium.UI.PresentationSource");
        object? source = null;
        Note($"  PresentationSource={(sourceType?.FullName ?? "no such type")}, window is one: {(window is not null && sourceType?.IsInstanceOfType(window) == true)}");
        foreach (var factory in sourceType?.GetMethods(BindingFlags.Public | BindingFlags.Static)
                     .Where(method => method.Name.StartsWith("From", StringComparison.Ordinal)) ?? [])
        {
            Note($"    static {factory.Name}({string.Join(",", factory.GetParameters().Select(parameter => parameter.ParameterType.Name))})");
            if (factory.GetParameters() is [{ ParameterType.Name: "Visual" or "DependencyObject" }]
                && factory.Invoke(null, [target]) is { } created)
            {
                source = created;
                Note($"      {factory.Name}(target) -> {created.GetType().Name}");
                break;
            }
        }

        Note($"  source used: {(source?.GetType().Name ?? "null")}");

        foreach (var name in new[] { "Right", "Left" })
        {
            var key = Enum.Parse(typeof(Key), name);
            var raised = false;
            foreach (var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var arguments = ctor.GetParameters().Select(parameter => Fill(parameter.ParameterType, key, device, source)).ToArray();
                object? args;
                try
                {
                    args = ctor.Invoke(arguments);
                }
                catch (TargetInvocationException exception)
                {
                    Note($"  {name} ctor({string.Join(",", parameters(ctor))}) threw {exception.GetBaseException().GetType().Name}: {Trim(exception.GetBaseException().Message)}");
                    continue;
                }

                var routed = typeof(UIElement).GetField("KeyDownEvent", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                type.GetProperty("RoutedEvent")?.SetValue(args, routed);
                type.GetProperty("Source")?.SetValue(args, target);
                target.RaiseEvent((RoutedEventArgs)args);
                Pump(8);
                Note($"  {name} via ctor({string.Join(",", parameters(ctor))}) raised: SelectedIndex={Get(target, "SelectedIndex")}");
                raised = true;
                break;
            }

            if (!raised)
            {
                Note($"  {name}: no constructor accepted the arguments");
            }
        }

        Note("  the control's own selection handlers, with real arguments:");
        var adjacent = target.GetType().GetMethod("SelectAdjacentTab", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var delta in new[] { 1, -1 })
        {
            if (adjacent is null)
            {
                Note("    SelectAdjacentTab: missing");
                break;
            }

            var before = Get(target, "SelectedIndex");
            Note("    " + Try(() =>
            {
                adjacent.Invoke(target, [delta]);
                return "ok";
            }) + $" SelectAdjacentTab({delta}): {before} -> {Get(target, "SelectedIndex")}");
            Pump(8);
        }

        var selectTab = target.GetType().GetMethod("SelectTab", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var last = Items(target).LastOrDefault();
        if (selectTab is not null && last is not null)
        {
            var before = Get(target, "SelectedIndex");
            Note("    " + Try(() =>
            {
                selectTab.Invoke(target, [last]);
                return "ok";
            }) + $" SelectTab(item {Items(target).IndexOf(last)}): {before} -> {Get(target, "SelectedIndex")}");
            Pump(8);
        }

        Note($"    after the drives: {GeometryLine(target)}");
        Shot("G after the selection drives", (FrameworkElement)target);
    }

    private static string GeometryLine(DependencyObject host) =>
        string.Join("  ", Items(host).Select((item, index) => $"[{index}] sel={Get(item, "IsSelected")} bg={Hex(Get(item, "Background") as Brush)} selectedBg={Hex(Get(item, "SelectedBackground") as Brush)}"));

    private static object? Fill(Type parameter, object key, object? device, object? source)
    {
        if (parameter.IsEnum)
        {
            return key;
        }

        if (parameter == typeof(int))
        {
            return 0;
        }

        if (parameter == typeof(bool))
        {
            return false;
        }

        if (parameter.IsAssignableTo(typeof(KeyboardDevice)) && device is not null)
        {
            return device;
        }

        if (parameter.Name.Contains("PresentationSource", StringComparison.Ordinal))
        {
            return source;
        }

        return parameter.IsValueType ? Activator.CreateInstance(parameter) : null;
    }

    // ---------- measurement ----------

    private static void Shot(string label, FrameworkElement surface)
    {
        var width = (int)Math.Round(surface.ActualWidth);
        var height = (int)Math.Round(surface.ActualHeight);
        if (width < 4 || height < 4)
        {
            Note($"  {label}: no surface ({width}x{height})");
            return;
        }

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(surface);
        var buffer = new byte[width * 4 * height];
        bitmap.CopyPixels(new Int32Rect(0, 0, width, height), buffer, width * 4, 0);
        Note($"  {label}: {width}x{height}");
        foreach (var (name, argb) in new (string, uint)[]
                 {
                     ("red", 0xFFFF0000), ("lime", 0xFF00FF00), ("blue", 0xFF0000FF), ("magenta", 0xFFFF00FF),
                     ("ink", 0xFF112233), ("surface", 0xFF1C1C1E),
                 })
        {
            Trace(buffer, width, height, name, argb);
        }

        WriteBitmap(buffer, width, height, Path.Combine(AppContext.BaseDirectory, $"t2-{Sanitize(label)}.bmp"));
    }

    /// <summary>
    /// Count and bounding box of everything within a colour distance of a tint. The tolerance matters: a GPU
    /// antialiases both the fill edges and the text, so a lever that paints shows a cloud around the exact value
    /// and a lever that paints nothing shows an exact zero. Both readings are printed so a later pass can tell a
    /// blend from an absence.
    /// </summary>
    private static void Trace(byte[] buffer, int width, int height, string name, uint argb)
    {
        var targetR = (int)((argb >> 16) & 0xFF);
        var targetG = (int)((argb >> 8) & 0xFF);
        var targetB = (int)(argb & 0xFF);
        var exact = 0;
        var near = 0;
        var minX = -1;
        var maxX = -1;
        var minY = -1;
        var maxY = -1;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 4;
                var b = buffer[offset];
                var g = buffer[offset + 1];
                var r = buffer[offset + 2];
                if (b == (argb & 0xFF) && g == ((argb >> 8) & 0xFF) && r == ((argb >> 16) & 0xFF))
                {
                    exact++;
                }

                var distance = Math.Abs(r - targetR) + Math.Abs(g - targetG) + Math.Abs(b - targetB);
                if (distance > 96)
                {
                    continue;
                }

                near++;
                if (minX < 0 || x < minX)
                {
                    minX = x;
                }

                if (x > maxX)
                {
                    maxX = x;
                }

                if (minY < 0 || y < minY)
                {
                    minY = y;
                }

                if (y > maxY)
                {
                    maxY = y;
                }
            }
        }

        var box = near == 0 ? "-" : $"{minX}..{maxX}x{minY}..{maxY}";
        Note($"    {name,-8} exact={exact,-6} within96={near,-6} box={box}");
    }

    private static string HeaderGeometry(DependencyObject host)
    {
        var items = Items(host);
        if (items.Count == 0 || host is not Visual visual)
        {
            return "no items";
        }

        var first = items[0];
        var offset = first.TransformToVisual((Visual)host).Transform(new Point(0, 0));
        var panel = VisualTreeHelper.GetChild(visual, 0);
        var panelOffset = panel is Visual panelVisual ? ((FrameworkElement)panel).ActualHeight : 0;
        return $"item0 at ({offset.X:0.#},{offset.Y:0.#}) size {first.ActualWidth:0.#}x{first.ActualHeight:0.#}, first child height {panelOffset:0.#}";
    }

    private static List<DependencyObject> Descendants(DependencyObject root, string simpleName)
    {
        var found = new List<DependencyObject>();
        void Walk(DependencyObject node)
        {
            if (node.GetType().Name == simpleName)
            {
                found.Add(node);
            }

            if (node is not Visual visual)
            {
                return;
            }

            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
            {
                Walk(VisualTreeHelper.GetChild(visual, index));
            }
        }

        Walk(root);
        return found;
    }

    private static List<FrameworkElement> Items(DependencyObject root) =>
        Descendants(root, "TabItem").OfType<FrameworkElement>().ToList();

    // ---------- helpers ----------

    private static void Note(string line) => Lines.Add(line);

    private static string Trim(string text) => text.Replace('\r', ' ').Replace('\n', ' ').Trim();

    private static string Sanitize(string text) => new(text.Select(character => char.IsLetterOrDigit(character) ? character : '-').ToArray());

    private static object? Get(DependencyObject target, string name) => target.GetType().GetProperty(name)?.GetValue(target);

    private static string Text(DependencyObject node) => (Get(node, "Text") as string) ?? string.Empty;

    private static string Hex(Brush? brush) => brush is SolidColorBrush solid
        ? $"#{solid.Color.A:X2}{solid.Color.R:X2}{solid.Color.G:X2}{solid.Color.B:X2}"
        : brush is null ? "-" : brush.GetType().Name;

    private static Brush? Paint(uint argb) => new SolidColorBrush(Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb));

    private static void SetByEnum(object target, string name, string value)
    {
        var property = target.GetType().GetProperty(name);
        if (property is null || !property.CanWrite || !property.PropertyType.IsEnum)
        {
            Note($"  set {name}: {(property is null ? "missing" : property.CanWrite ? "not an enum" : "read-only")}");
            return;
        }

        try
        {
            property.SetValue(target, Enum.Parse(property.PropertyType, value));
            Note($"  set {name}={value}: ok (type {property.PropertyType.Name}, values {string.Join(",", Enum.GetNames(property.PropertyType))})");
        }
        catch (Exception exception)
        {
            Note($"  set {name}={value}: " + exception.GetBaseException().GetType().Name);
        }
    }

    private static void Set(object target, string name, object? value)
    {
        var property = target.GetType().GetProperty(name);
        if (property is null || !property.CanWrite)
        {
            Note($"  set {name}: {(property is null ? target.GetType().Name + " declares no such property" : "read-only")}");
            return;
        }

        try
        {
            property.SetValue(target, value);
        }
        catch (Exception exception)
        {
            Note($"  set {name}: " + exception.GetBaseException().GetType().Name + " " + Trim(exception.GetBaseException().Message));
        }
    }

    private static IEnumerable<string> parameters(MethodBase method) => method.GetParameters().Select(parameter => parameter.ParameterType.Name);

    private static string Try(Func<string> action)
    {
        try
        {
            return action();
        }
        catch (Exception exception)
        {
            return exception.GetType().Name + ": " + Trim(exception.GetBaseException().Message);
        }
    }

    private static void Walk(string prefix, DependencyObject node, int depth, int maxDepth)
    {
        if (depth > maxDepth)
        {
            return;
        }

        if (node is not Visual visual)
        {
            Note(prefix + new string(' ', depth * 2) + "(non-visual) " + node.GetType().Name);
            return;
        }

        var builder = new StringBuilder($"{prefix}{new string(' ', depth * 2)}{node.GetType().Name}");
        var name = (node as FrameworkElement)?.Name;
        if (!string.IsNullOrEmpty(name))
        {
            builder.Append($" '{name}'");
        }

        if (node is Control control)
        {
            builder.Append($" bg={Hex(control.Background)} fg={Hex(control.Foreground)} sel={Get(node, "IsSelected")}");
        }
        else if (node is TextBlock textBlock)
        {
            builder.Append($" text=\"{Trim(textBlock.Text ?? string.Empty)}\" fg={Hex(textBlock.Foreground)}");
        }

        if (node is FrameworkElement element)
        {
            builder.Append($" {element.ActualWidth:0.##}x{element.ActualHeight:0.##}");
        }

        if (node is UIElement uiElement && !uiElement.IsVisible)
        {
            builder.Append(" [hidden]");
        }

        Note(builder.ToString());
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
        {
            if (VisualTreeHelper.GetChild(visual, index) is { } child)
            {
                Walk(prefix, child, depth + 1, maxDepth);
            }
        }
    }

    private static void Dump(string prefix, DependencyObject root, int maxDepth = 8) => Walk(prefix, root, 0, maxDepth);

    private static void WriteBitmap(byte[] buffer, int width, int height, string path)
    {
        var stride = width * 3;
        using var writer = new BinaryWriter(File.Create(path));
        writer.Write((ushort)0x4D42);
        writer.Write(54 + stride * height);
        writer.Write(0);
        writer.Write(54);
        writer.Write(40);
        writer.Write(width);
        writer.Write(height);
        writer.Write((ushort)1);
        writer.Write((ushort)24);
        writer.Write(0);
        writer.Write(stride * height);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        for (var y = height - 1; y >= 0; y--)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 4;
                writer.Write(buffer[offset]);
                writer.Write(buffer[offset + 1]);
                writer.Write(buffer[offset + 2]);
            }
        }
    }

    private static int Pump(int frames = 10, int budgetMilliseconds = 1500)
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
        return seen;
    }
}
