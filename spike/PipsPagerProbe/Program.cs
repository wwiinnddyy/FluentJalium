using System.Reflection;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

namespace PipsPagerProbe;

/// <summary>
/// The two readings the PipsPager slice cannot write product code without.
///   A api : upstream builds the control out of a FontIcon glyph, an ItemsRepeater, a ScrollViewer with
///           ScrollMode/scroll-chaining switches, UseSystemFocusVisuals, ToolTipService.Placement and
///           AutomationProperties.PositionInSet/SizeOfSet. This runtime has no ItemsRepeater, and the rest is
///           an open question: the census says which of those shapes exist under the name upstream uses, so the
///           template is written against members that are really there rather than against WPF's or WinUI's.
///   B pip : upstream draws a pip as ONE symbol glyph at FontSize 4 (normal) or 6 (selected) inside a 12x24
///           footprint, and the whole difference between the two pips is that font size. The codepoints are in
///           Segoe Fluent Icons' cmap (0.938em square, one contour - a solid dot, not a ring), but cmap is not
///           pixels: what a 4-DIP glyph becomes on this renderer decides whether the glyph route can carry the
///           look at all, or whether a pip has to be a drawn circle. Two drawn references (an 8x8 square and an
///           8x8 rounded square) are measured in the same pass so "circle" here means a fill ratio near 0.785
///           with an inked centre, not somebody's eye.
///   C chrome: what our own implicit Button style does to a Button forced to 12x24 - upstream's pip buttons are
///           chrome-free because they carry an explicit style; ours would otherwise inherit the Fluent button's
///           min height and border, which is the difference between a dot and a box.
/// Modes: api | pip | all.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];

    private static string _mode = "all";

    private const int Cell = 40;

    [STAThread]
    private static int Main(string[] arguments)
    {
        _mode = arguments.Length > 0 ? arguments[0].ToLowerInvariant() : "all";
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

        var path = Path.Combine(AppContext.BaseDirectory, $"pipspager-probe-{_mode}.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, mode={_mode}, full log: {path}");
        return 0;
    }

    private static void Run(Application application)
    {
        var root = new StackPanel { Margin = new Thickness(24) };
        var window = new Window { Content = root, Width = 900, Height = 700, Title = "PipsPager probe" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Pump();
                Note($"theme dictionaries={FluentThemeManager.DictionaryNames.Count}");
                if (_mode is "all" or "api")
                {
                    Api();
                }

                if (_mode is "all" or "pip")
                {
                    Ink(root);
                    Chrome(root);
                }
            }
            catch (Exception exception)
            {
                Note("PROBE threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            }
            finally
            {
                window.Close();
                application.Shutdown();
            }
        };

        application.Run(window);
    }

    // ---------- A. the surface upstream's template leans on ----------

    private static void Api()
    {
        Note("");
        Note("=== A. do the types upstream's markup names exist here ===");
        foreach (var name in new[]
                 {
                     "FontIcon", "BitmapIcon", "SymbolIcon", "PathIcon", "IconSource", "ItemsRepeater", "StackLayout",
                     "FocusManager", "PipsPager", "RadioButtons", "BreadcrumbBar", "ToolTipService", "ToolTip",
                     "AutomationProperties", "ScrollViewer", "Button", "RepeatButton", "ToggleButton", "TextBlock",
                     "ControlTemplate", "VisualStateManager", "VisualState", "Style", "Orientation",
                 })
        {
            var type = TypeByName(name);
            Note($"  {name,-22} {(type is null ? "ABSENT" : Chain(type))}");
        }

        Note("");
        Note("  Orientation enum members (upstream binds StackPanel/StackLayout to it):");
        var orientation = TypeByName("Orientation");
        if (orientation is not null)
        {
            Note($"    {orientation.FullName} = {Join(Enum.GetNames(orientation))}");
        }

        foreach (var (typeName, members) in new[]
                 {
                     ("Button", new[]
                     {
                         "CornerRadius", "BorderThickness", "BorderBrush", "Background", "Foreground", "FontSize",
                         "FontFamily", "Content", "IsTabStop", "FocusVisualMargin", "UseSystemFocusVisuals",
                         "Command", "CommandParameter", "Click", "Template", "Padding", "MinHeight", "MinWidth",
                     }),
                     ("ScrollViewer", new[]
                     {
                         "VerticalScrollBarVisibility", "HorizontalScrollBarVisibility", "VerticalScrollMode",
                         "HorizontalScrollMode", "IsScrollChainingEnabled", "IsHorizontalScrollChainingEnabled",
                         "IsVerticalScrollChainingEnabled", "CanContentScroll", "HorizontalOffset", "Content",
                     }),
                     ("FrameworkElement", new[]
                     {
                         "MaxWidth", "MaxHeight", "MinHeight", "Opacity", "Visibility", "RenderTransform",
                         "RenderTransformOrigin", "FlowDirection", "Name", "Tag", "Parent", "Resources",
                     }),
                     ("UIElement", new[]
                     {
                         "IsEnabled", "IsVisible", "Focus", "IsKeyboardFocused", "KeyDown", "PreviewKeyDown",
                         "GotFocus", "LostFocus", "GettingFocus", "LosingFocus", "DesiredSize", "ActualWidth",
                     }),
                     ("Control", new[]
                     {
                         "OnApplyTemplate", "GetTemplateChild", "IsTabStop", "Template", "Style", "TryFindResource",
                         "Tag", "DefaultStyleKey",
                     }),
                     ("StackPanel", new[] { "Orientation", "Spacing" }),
                     ("TextBlock", new[] { "Text", "FontSize", "FontFamily", "Foreground", "TextAlignment" }),
                     ("Panel", new[] { "Children" }),
                     ("KeyEventArgs", new[] { "Key", "Handled" }),
                 })
        {
            var type = TypeByName(typeName);
            Note("");
            Note($"  {typeName}: {(type is null ? "ABSENT" : type.FullName)}");
            if (type is null)
            {
                continue;
            }

            foreach (var member in members)
            {
                var found = Find(type, member);
                Note($"    {member,-32} {(found is null ? "-" : Describe(found))}");
            }
        }

        Note("");
        Note("  AutomationProperties / ToolTipService attached DP fields:");
        foreach (var typeName in new[] { "AutomationProperties", "ToolTipService" })
        {
            var type = TypeByName(typeName);
            Note($"    {typeName}: {(type is null ? "ABSENT" : Join(type.GetFields(BindingFlags.Public | BindingFlags.Static).Select(field => field.Name)))}");
        }

        Note("");
        Note("  Key enum members the paging arrows would need:");
        var key = TypeByName("Key");
        if (key is not null)
        {
            Note($"    present={Join(new[] { "Left", "Right", "Up", "Down", "Enter", "Space" }.Where(name => Enum.IsDefined(key, name)))}");
        }
    }

    // ---------- B. what the pips become in pixels ----------

    private static void Ink(Panel root)
    {
        Note("");
        Note("=== B. ink of the upstream glyphs and of two drawn references, same capture path ===");
        var white = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        var black = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
        var symbol = new FontFamily("Segoe Fluent Icons");

        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        var labels = new List<string>();

        void AddCell(string label, UIElement content)
        {
            panel.Children.Add(new Border
            {
                Width = Cell,
                Height = Cell,
                Background = black,
                Child = content,
            });
            labels.Add(label);
        }

        AddCell("drawn square  8x8", new Border
        {
            Width = 8,
            Height = 8,
            Background = white,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        });
        AddCell("drawn circle  8x8 r=4", new Border
        {
            Width = 8,
            Height = 8,
            Background = white,
            CornerRadius = new CornerRadius(4),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        });

        foreach (var (label, codepoint, size) in new[]
                 {
                     ("pip EA3B @24", 0xEA3B, 24d),
                     ("pip EA3B @6  selected", 0xEA3B, 6d),
                     ("pip EA3B @4  normal", 0xEA3B, 4d),
                     ("prev EDDB @8", 0xEDDB, 8d),
                     ("next EDDC @8", 0xEDDC, 8d),
                     ("prev EDDB @12", 0xEDDB, 12d),
                     ("absent EFFE @12", 0xEFFE, 12d),
                 })
        {
            AddCell(label, new TextBlock
            {
                Text = char.ConvertFromUtf32(codepoint),
                FontFamily = symbol,
                FontSize = size,
                Foreground = white,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        root.Children.Add(panel);
        Pump(8);
        for (var index = 0; index < panel.Children.Count; index++)
        {
            var cell = (Border)panel.Children[index];
            var label = labels[index];
            var text = cell.Child as TextBlock;
            Note($"  {label,-26} cell={cell.ActualWidth:0.#}x{cell.ActualHeight:0.#} " +
                 (text is null ? "" : $"inkDesired={text.DesiredSize.Width:0.###}x{text.DesiredSize.Height:0.###} " +
                                      $"arranged={text.ActualWidth:0.###}x{text.ActualHeight:0.###}"));
            Analyze(cell);
        }

        root.Children.Remove(panel);
        Pump(2);
    }

    private static void Analyze(FrameworkElement element)
    {
        var width = (int)Math.Round(element.ActualWidth);
        var height = (int)Math.Round(element.ActualHeight);
        if (width <= 0 || height <= 0)
        {
            Note("      capture: no size");
            return;
        }

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(element);
        var buffer = new byte[width * height * 4];
        bitmap.CopyPixels(buffer, width * 4, 0);

        var ink = 0;
        var minX = width;
        var minY = height;
        var maxX = -1;
        var maxY = -1;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 4;
                var brightness = buffer[offset] + buffer[offset + 1] + buffer[offset + 2];
                if (brightness <= 120)
                {
                    continue;
                }

                ink++;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }

        if (ink == 0 || maxX < 0)
        {
            Note("      capture: no ink above threshold");
            return;
        }

        var boxWidth = maxX - minX + 1;
        var boxHeight = maxY - minY + 1;
        var centreX = (minX + maxX) / 2;
        var centreY = (minY + maxY) / 2;
        var centreOffset = (centreY * width + centreX) * 4;
        var centreBrightness = buffer[centreOffset] + buffer[centreOffset + 1] + buffer[centreOffset + 2];
        Note($"      ink={ink} bbox={boxWidth}x{boxHeight}@({minX},{minY}) " +
             $"fillOfBbox={ink / (double)(boxWidth * boxHeight):0.###} centreBright={centreBrightness} " +
             $"ratioToCell={ink / (double)(width * height):0.###}");
    }

    // ---------- C. what our implicit button style does to a pip-sized button ----------

    private static void Chrome(Panel root)
    {
        Note("");
        Note("=== C. our implicit Button style vs a 12x24 pip footprint ===");
        foreach (var (label, button) in new[]
                 {
                     ("default Button", new Button { Content = "x" }),
                     ("Button forced 12x24", new Button { Content = "x", Width = 12, Height = 24 }),
                     ("Button MinHeight=0 forced 12x24", new Button { Content = "x", Width = 12, Height = 24, MinHeight = 0 }),
                 })
        {
            root.Children.Add(button);
            Pump(6);
            var template = Prop(button, "Template");
            Note($"  {label,-34} size={button.ActualWidth:0.##}x{button.ActualHeight:0.##} " +
                 $"style={Show(Prop(button, "Style")?.GetType().Name)} minHeight={Show(Prop(button, "MinHeight"))} " +
                 $"padding={Show(Prop(button, "Padding"))} template={(template is null ? "null" : "SET")} " +
                 $"parts={Join(ChildrenNames(button))}");
            root.Children.Remove(button);
            Pump(2);
        }
    }

    private static IEnumerable<string> ChildrenNames(DependencyObject root)
    {
        var results = new List<string>();
        void Walk(Visual node, int depth)
        {
            if (depth > 8 || results.Count > 12)
            {
                return;
            }

            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
            {
                var child = (Visual)VisualTreeHelper.GetChild(node, index);
                var name = Read(() => child.GetType().GetProperty("Name")?.GetValue(child)) as string;
                if (!string.IsNullOrEmpty(name))
                {
                    results.Add(name);
                }

                Walk(child, depth + 1);
            }
        }

        if (root is Visual visual)
        {
            Walk(visual, 0);
        }

        return results;
    }

    // ---------- shared helpers ----------

    private static MemberInfo? Find(Type type, string name)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        return (MemberInfo?)type.GetProperty(name, flags)
            ?? (MemberInfo?)type.GetEvent(name, flags)
            ?? type.GetMethod(name, flags);
    }

    private static string Describe(MemberInfo member) => member switch
    {
        PropertyInfo property => $"prop {property.PropertyType.Name} ({property.DeclaringType?.Name})" +
                                 (property.GetMethod?.IsPublic == true ? "" : " nonpublic-getter"),
        EventInfo eventInfo => $"event {eventInfo.EventHandlerType?.Name} ({eventInfo.DeclaringType?.Name})",
        MethodInfo method => $"method ({method.DeclaringType?.Name}){(method.IsPublic ? " public" : " protected/internal")}",
        _ => member.MemberType.ToString(),
    };

    private static List<Assembly> LoadedAssemblies()
    {
        var seen = new Dictionary<string, Assembly>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.GetName().Name ?? "?";
            if (name.StartsWith("Jalium", StringComparison.Ordinal) || name.StartsWith("FluentJalium", StringComparison.Ordinal))
            {
                seen.TryAdd(name, assembly);
            }
        }

        return seen.Values.ToList();
    }

    private static Type? TypeByName(string name) => LoadedAssemblies()
        .SelectMany(SafeTypes)
        .FirstOrDefault(type => type.Name == name && !type.IsNested);

    private static IEnumerable<Type> SafeTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type is not null)!;
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static string Chain(Type type)
    {
        var parts = new List<string>();
        for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
        {
            parts.Add(current.Name);
        }

        return string.Join(" < ", parts);
    }

    private static object? Prop(object? target, string name) => target is null
        ? null
        : Read(() => target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(target));

    private static object? Read(Func<object?> reader)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            return " threw " + exception.GetType().Name;
        }
    }

    private static string Show(object? value) => value is null ? "null" : Trim(value.ToString() ?? "?");

    private static string Join(IEnumerable<string> values) => string.Join(", ", values);

    private static void Note(string line) => Lines.Add(line);

    private static string Trim(string text) => text.Replace('\r', ' ').Replace('\n', ' ').Trim();

    private static int Pump(int frames = 6, int budgetMilliseconds = 1500)
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
