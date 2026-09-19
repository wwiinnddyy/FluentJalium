using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Interop;
using Jalium.UI.Threading;
using Jalium.UI.Markup;

namespace InfoBarGhostProbe;

/// <summary>
/// Throwaway probe: who paints an info bar twice? The surfaces page shows the title, message, icon and
/// close mark twice with a small offset, so this measures whether the base class still contributes pixels
/// after a template exists, and what virtuals it exposes for turning that contribution off.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "ghost-probe4.txt");

    /// <summary>Opaque sentinel root: nothing in our style can produce this colour, so any pixel of it is the template's.</summary>
    private const uint Lime = 0x00FF00;

    /// <summary>Opt-in plus a silenced base paint layer: the candidate fix for the double-painted surface.</summary>
    private sealed class SilentInfoBar : InfoBar
    {
        public SilentInfoBar() => UseTemplateContentManagement();

        protected override void OnRender(DrawingContext drawingContext)
        {
        }
    }

    /// <summary>The shipped own type with the same suppression, so the shipping template can be measured both ways.</summary>
    private sealed class SilentFluentInfoBar : FluentInfoBar
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
        }
    }

    [STAThread]
    private static int Main()
    {
        Signatures();

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

    private static void Signatures()
    {
        Note("=== R. what InfoBar declares itself ===");
        Note($"  InfoBar base chain: {Chain(typeof(InfoBar))}");
        DumpDeclared(typeof(InfoBar));

        Note("  --- the paint virtual, walked up the chain ---");
        foreach (var type in new[] { typeof(InfoBar), typeof(ContentControl), typeof(Control), typeof(UIElement) })
        {
            var method = type.GetMethod("OnRender", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (method is null)
            {
                Note($"    {type.Name}: no OnRender declared");
                continue;
            }

            Note($"    {type.Name}: {(method.IsVirtual ? "virtual" : "NON-virtual")} {method.ReturnType.Name} OnRender({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.FullName))})");
        }

        Note("=== C. which framework controls declare their own paint layer ===");
        var selfDrawn = new List<string>();
        foreach (var type in typeof(InfoBar).Assembly.GetExportedTypes().OrderBy(static t => t.Name))
        {
            var onRender = type.GetMethod("OnRender", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var template = type.GetProperty("Template", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (onRender is not null)
            {
                selfDrawn.Add(type.Name);
                Note($"  {type.Name}: OnRender {(onRender.IsVirtual ? "virtual" : "sealed")}" +
                     (template is null ? ", no Template property of its own" : ", declares Template") +
                     (typeof(ContentControl).IsAssignableFrom(type) ? ", ContentControl" : string.Empty));
            }
        }

        Note($"  total declaring OnRender: {selfDrawn.Count}");
        Note("  --- the types Astra restyles, and whether each is on that list ---");
        foreach (var name in Restyled)
        {
            var type = typeof(InfoBar).Assembly.GetType("Jalium.UI.Controls." + name);
            if (type is null)
            {
                Note($"    {name}: type not found");
                continue;
            }

            var onRender = type.GetMethod("OnRender", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var owner = "none";
            for (var current = type; current is not null && owner == "none"; current = current.BaseType)
            {
                if (current.GetMethod("OnRender", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) is not null)
                {
                    owner = current.Name;
                }
            }

            var parts = PartNames(type);
            Note($"    {name}: declares OnRender={onRender is not null} nearestOwner={owner} templateChannel={HasTemplateChannel(type)} ldstrInOnApplyTemplate=[{string.Join(", ", parts)}]");
        }
    }

    private static readonly string[] Restyled =
    [
        "Button", "ToggleButton", "RepeatButton", "HyperlinkButton", "SplitButton", "DropDownButton",
        "TextBox", "PasswordBox", "NumberBox", "AutoCompleteBox", "CheckBox", "RadioButton", "Slider",
        "ToggleSwitch", "ComboBox", "Expander", "InfoBar", "Menu", "MenuItem", "MenuBar", "ContextMenu",
        "MenuFlyoutPresenter", "MenuFlyoutItem", "MenuFlyoutSeparator", "MenuFlyoutSubItem", "ToggleMenuFlyoutItem",
        "RadioMenuFlyoutItem", "CommandBar", "AppBarButton", "AppBarToggleButton", "AppBarSeparator",
        "ScrollViewer", "ScrollBar", "Thumb", "ToolTip", "ListBox", "ListBoxItem", "ListView", "ListViewItem",
        "GridView", "TreeView", "TreeViewItem", "TabControl", "TabItem", "NavigationView", "ContentPresenter",
    ];

    private static List<string> PartNames(Type type)
    {
        var literals = new List<string>();
        var method = type.GetMethod("OnApplyTemplate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (method?.GetMethodBody()?.GetILAsByteArray() is not { } il)
        {
            return literals;
        }

        for (var index = 0; index + 5 < il.Length; index++)
        {
            if (il[index] != 0x72)
            {
                continue;
            }

            try
            {
                literals.Add(type.Module.ResolveString(BitConverter.ToInt32(il, index + 1)));
                index += 4;
            }
            catch
            {
                // An operand byte that happens to look like ldstr.
            }
        }

        return literals.Distinct().Where(static text => text.Length < 40 && !text.Contains(' ')).ToList();
    }

    private static string HasTemplateChannel(Type type)
    {
        var property = type.GetProperty("Template", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (property is not null)
        {
            return "declares";
        }

        return type.GetMethod("UseTemplateContentManagement", BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null) is not null
            ? "inherited"
            : "none";
    }

    private static void Strings(Type owner, string methodName)
    {
        var method = owner.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (method is null)
        {
            Note($"  {owner.Name}.{methodName}: not declared");
            return;
        }

        List<string> literals = [];
        List<string> tokens = [];
        try
        {
            var body = method.GetMethodBody();
            var il = body?.GetILAsByteArray();
            if (il is null)
            {
                Note($"  {owner.Name}.{methodName}: no body");
                return;
            }

            var module = owner.Module;
            for (var index = 0; index + 5 < il.Length; index++)
            {
                // 0x72 = ldstr, 0x28 = call, 0x6f = callvirt, 0x7b/0x7e = ldsfld/ldsfld, 0x71 = newobj
                if (il[index] == 0x72)
                {
                    var token = BitConverter.ToInt32(il, index + 1);
                    try
                    {
                        literals.Add(module.ResolveString(token));
                        index += 4;
                    }
                    catch
                    {
                        // Not a string token at this offset: an operand byte that happens to look like an opcode.
                    }
                }
                else if ((il[index] == 0x28 || il[index] == 0x6f || il[index] == 0x73 || il[index] == 0x71 || il[index] == 0x7b || il[index] == 0x7e)
                         && index + 5 < il.Length)
                {
                    var token = BitConverter.ToInt32(il, index + 1);
                    try
                    {
                        if ((token >> 24) == 0x0a || (token >> 24) == 0x06)
                        {
                            var resolved = (token >> 24) == 0x0a ? module.ResolveMember(token) : module.ResolveField(token);
                            if (resolved!.Name.Contains("Border", StringComparison.Ordinal) || resolved.Name.StartsWith("m_", StringComparison.Ordinal)
                                || resolved.DeclaringType == owner)
                            {
                                tokens.Add(resolved.Name);
                            }

                            index += 4;
                        }
                    }
                    catch
                    {
                        // Same: an operand byte that happens to look like a metadata token.
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Note($"  {owner.Name}.{methodName}: IL read threw {exception.GetType().Name}");
            return;
        }

        Note($"  {owner.Name}.{methodName} ldstr: {string.Join(" | ", literals.Distinct())}");
        Note($"      members touched: {string.Join(" | ", tokens.Distinct().OrderBy(static t => t))}");
    }

    private static void DumpDeclared(Type type)
    {
        Note($"  --- {type.Name} declared members ---");
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                     .Where(static m => !m.IsSpecialName)
                     .OrderBy(static m => m.Name))
        {
            var flags = (method.IsPublic ? "public " : method.IsFamily ? "protected " : method.IsAssembly ? "internal " : "private ") +
                        (method.IsVirtual ? "virtual " : string.Empty);
            Note($"    M {flags}{method.ReturnType.Name} {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");
        }

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                     .OrderBy(static f => f.Name))
        {
            Note($"    F {(field.IsPublic ? "public" : field.IsFamily ? "protected" : "private")} {field.FieldType.Name} {field.Name}");
        }
    }

    private static string Chain(Type type)
    {
        var parts = new List<string>();
        for (var current = type; current is not null; current = current.BaseType)
        {
            parts.Add(current.Name);
        }

        return string.Join(" -> ", parts);
    }

    private static void Run(Application application)
    {
        var dictionary = (ResourceDictionary)XamlReader.Parse(Markup)!;
        var sentinel = (ControlTemplate)dictionary["SentinelTemplate"]!;

        var native = new InfoBar { Title = "native self-draw", Message = "message line", Width = 420, Height = 64 };
        var shipped = new FluentInfoBar { Title = "shipped style", Message = "message line", Width = 420, Height = 64 };
        var opaque = new FluentInfoBar { Title = "opaque template", Message = "message line", Width = 420, Height = 64, Template = sentinel };
        var opaqueNative = new InfoBar { Title = "opaque, no opt-in", Message = "message line", Width = 420, Height = 64, Template = sentinel };
        var silentOpaque = new SilentInfoBar { Title = "silenced + opaque", Message = "message line", Width = 420, Height = 64, Template = sentinel };

        // Text-only pairs: one ink layer from our template, one from the base. The ink count is the ghost count.
        var loudText = new FluentInfoBar { Title = "Only one line of text", Width = 420, Height = 48, IsIconVisible = false, IsClosable = false };
        var silentText = new SilentFluentInfoBar { Title = "Only one line of text", Width = 420, Height = 48, IsIconVisible = false, IsClosable = false };

        var panel = new StackPanel { Orientation = Orientation.Vertical };
        panel.Children.Add(native);
        panel.Children.Add(shipped);
        panel.Children.Add(opaque);
        panel.Children.Add(opaqueNative);
        panel.Children.Add(silentOpaque);
        panel.Children.Add(loudText);
        panel.Children.Add(silentText);

        var window = new Window
        {
            Content = panel,
            Width = 700,
            Height = 700,
            Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
        };
        window.Show();

        Pump(20, 1200);

        Note("=== T. tree shape each variant actually built ===");
        foreach (var (label, bar) in new[] { ("native", native), ("shipped", shipped), ("opaque", opaque), ("opaqueNative", opaqueNative), ("silentOpaque", silentOpaque) })
        {
            var control = (Control)bar;
            Note($"  {label}: template={(control.Template?.GetType().Name ?? "null")} children={VisualTreeHelper.GetChildrenCount(control)} actual={control.ActualWidth:0.#}x{control.ActualHeight:0.#}");
            if (label == "shipped")
            {
                Dump(string.Empty, control, 0);
            }
        }

        Note("=== P. pixel attribution ===");
        foreach (var (label, bar) in new[] { ("native", native), ("shipped", shipped), ("opaque", opaque), ("opaqueNative", opaqueNative), ("silentOpaque", silentOpaque), ("loudText", loudText), ("silentText", silentText) })
        {
            var control = (Control)bar;
            var width = (int)Math.Round(control.ActualWidth);
            var height = (int)Math.Round(control.ActualHeight);
            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
            bitmap.Render(control);
            var buffer = new byte[width * 4 * height];
            bitmap.CopyPixels(new Int32Rect(0, 0, width, height), buffer, width * 4, 0);

            var histogram = new Dictionary<uint, int>();
            for (var offset = 0; offset + 3 < buffer.Length; offset += 4)
            {
                var key = (uint)(buffer[offset + 2] << 16 | buffer[offset + 1] << 8 | buffer[offset]);
                histogram[key] = histogram.GetValueOrDefault(key) + 1;
            }

            var total = histogram.Values.Sum();
            var limePixels = histogram.GetValueOrDefault(Lime);
            var ink = histogram.Where(static e => e.Key < 0x505050).Sum(static e => e.Value);
            var top = histogram.OrderByDescending(static e => e.Value).ThenBy(static e => e.Key).Take(6);
            Note($"  {label}: {width}x{height} distinct={histogram.Count} lime={limePixels} ink={ink}");
            Note("      top: " + string.Join("  ", top.Select(e => $"#{e.Key:X6}x{e.Value}")));
        }

        window.Close();
        Pump(4, 400);
    }

    private static void Dump(string prefix, DependencyObject node, int depth)
    {
        if (depth > 7)
        {
            return;
        }

        var text = node is TextBlock textBlock ? $" text=\"{Trim(textBlock.Text ?? string.Empty)}\""
            : node is ContentPresenter presenter ? $" content={presenter.Content?.GetType().Name ?? "null"}"
            : string.Empty;
        var bounds = node is FrameworkElement element
            ? $" {element.ActualWidth:0.#}x{element.ActualHeight:0.#}"
            : string.Empty;
        Note($"    {prefix}{node.GetType().Name}{(string.IsNullOrEmpty(element_Name(node)) ? string.Empty : " \"" + element_Name(node) + "\"")}{bounds}{text}");
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            Dump(prefix + "  ", VisualTreeHelper.GetChild(node, index)!, depth + 1);
        }
    }

    private static string element_Name(DependencyObject node) =>
        node is FrameworkElement element ? element.Name ?? string.Empty : string.Empty;

    private static int Pump(int frames = 8, int budgetMilliseconds = 800)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
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

    private const string Markup = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <ControlTemplate x:Key='SentinelTemplate' TargetType='InfoBar'>
            <Border Background='#FF00FF00' />
          </ControlTemplate>
        </ResourceDictionary>
        """;
}
