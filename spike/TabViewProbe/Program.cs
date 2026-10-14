// Census: what does 26.10.9 actually give a tab strip?
//
// The stage-5 line says TabView "starts its own type". Three earlier batches proved that assumption can be
// wrong in both directions - InfoBar turned out to be one unopened protected switch away from being templateable,
// TreeView turned out to be exported, and GridView turned out not to exist as the name the objective assumed.
// So nothing here is typed against a guessed class: the probe reflects by simple name, parses what it mounts,
// swaps a template into it, moves selection and reaches for a keyboard event - all before any markup ships.
//
// Q: which Tab* types does the runtime export, and what do they declare?
// R: which Tab* resource rows are already in the framework's own dictionaries, before and after our theme?
// S: does the parser build a TabControl with TabItem children, and what part names does the realized tree carry?
// T: does an assigned ControlTemplate take over the paint, or does the control draw its own chrome regardless?
// U: does selection move by property and by a synthesized key, and does the selected body actually swap?
using System.Collections;
using System.Reflection;
using System.Text;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Input;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace TabViewProbe;

internal static class Program
{
    private static readonly List<string> Lines = [];
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "tab-probe1.txt");

    private static Application _application = null!;

    private const string TabsMarkup = """
        <TabControl xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                    Width='420' Height='160'>
          <TabItem Header='one'><TextBlock Text='body one' /></TabItem>
          <TabItem Header='two'><TextBlock Text='body two' /></TabItem>
          <TabItem Header='three' IsEnabled='False'><TextBlock Text='body three' /></TabItem>
        </TabControl>
        """;

    private const string ProbeTemplates = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <ControlTemplate x:Key='HostTemplate'>
            <Border x:Name='ProbeHostRoot' Background='#FFFF00FF' CornerRadius='8' Padding='4'>
              <StackPanel>
                <ContentPresenter x:Name='ProbeHeaderSite' />
                <ContentPresenter x:Name='ProbeBodySite' />
              </StackPanel>
            </Border>
          </ControlTemplate>
          <ControlTemplate x:Key='ItemTemplate'>
            <Border x:Name='ProbeItemRoot' Background='#FF00FF00' CornerRadius='4' MinHeight='32' Padding='8,3'>
              <TextBlock x:Name='ProbeItemText' Text='{TemplateBinding Header}' />
            </Border>
          </ControlTemplate>
        </ResourceDictionary>
        """;

    [STAThread]
    private static int Main()
    {
        try
        {
            var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
            renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            var application = new Application();
            _application = application;

            Note("=== R. framework rows, before Astra is applied ===");
            Note(FrameworkRows(application));
            FluentThemeManager.Apply(application);
            Note("=== R. framework rows, after Astra is applied ===");
            Note(FrameworkRows(application));
            Note($"  Astra dictionaries={FluentThemeManager.DictionaryNames.Count}");

            Note(string.Empty);
            Note("=== Q. what the runtime exports for tabs ===");
            TypeInventory();

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
        object? parsed;
        try
        {
            parsed = XamlReader.Parse(TabsMarkup);
        }
        catch (Exception exception)
        {
            Note(string.Empty);
            Note("=== S. parse a TabControl with TabItem children: FAILED ===");
            Note("  " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            return;
        }

        var host = parsed as FrameworkElement ?? throw new InvalidOperationException("parser returned a non-element");
        Note(string.Empty);
        Note("=== S. parse succeeded ===");
        Note($"  type={host.GetType().FullName}");

        var root = new StackPanel { Margin = new Thickness(24) };
        root.Children.Add(host);
        var window = new Window { Content = root, Width = 900, Height = 700, Title = "TabView probe 1" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Pump();

                Note(string.Empty);
                Note("=== S. realized tree, resting ===");
                Report("host", host);
                Dump("    ", host, 8);

                var items = ChildrenOfType(host, "TabItem");
                Note($"  TabItem containers realized: {items.Count}");
                foreach (var item in items)
                {
                    Report("   item", item);
                    Note("     parts: " + PartNames(item));
                }

                Note(string.Empty);
                Note("=== T. does an assigned template take over? ===");
                var dictionary = (ResourceDictionary)XamlReader.Parse(ProbeTemplates)!;
                var hostTemplate = dictionary["HostTemplate"];
                var itemTemplate = dictionary["ItemTemplate"];
                Note($"  parsed templates: host={hostTemplate?.GetType().Name ?? "missing"} item={itemTemplate?.GetType().Name ?? "missing"}");
                Note("  assign host: " + Try(() => SetProperty(host, "Template", hostTemplate)));
                foreach (var item in items)
                {
                    Note("  assign item: " + Try(() => SetProperty(item, "Template", itemTemplate)));
                }

                Pump(10);
                Dump("    ", host, 8);
                Note($"  probe host part present: {Part(host, "ProbeHostRoot") is not null}, item part present: {(items.Count > 0 && Part(items[0], "ProbeItemRoot") is not null)}");

                Note(string.Empty);
                Note("=== U. selection: property, then a synthesized key ===");
                Note($"  rest: SelectedIndex={Read(host, "SelectedIndex")} SelectedItem={Describe(Read(host, "SelectedItem"))}");
                Note("  set SelectedIndex=1: " + Try(() => SetProperty(host, "SelectedIndex", 1)));
                Pump(6);
                Note($"  after: SelectedIndex={Read(host, "SelectedIndex")} item0.IsSelected={Read(items[0], "IsSelected")} item1.IsSelected={Read(items[1], "IsSelected")}");
                Dump("    ", host, 6);
                Note("  visible body texts: " + string.Join(" | ", BodyTexts(host)));

                Note("  focus the host: " + Try(() => host.Focus()));
                foreach (var key in new[] { "Right", "Left", "Tab" })
                {
                    Note($"  synthesized KeyDown {key}: " + Try(() => SendKeyDown(host, key)));
                    Pump(6);
                    Note($"    after: SelectedIndex={Read(host, "SelectedIndex")}");
                }

                Note(string.Empty);
                Note("=== V. what the framework wrote as local values ===");
                foreach (var (label, element) in new[] { ("host", host) }.Concat(items.Select((item, index) => ($"item{index}", item))))
                {
                    var control = element as Control;
                    if (control is null)
                    {
                        continue;
                    }

                    Note($"  {label}: Background local={Local(control, Control.BackgroundProperty)} Foreground local={Local(control, Control.ForegroundProperty)}" +
                         $" BorderBrush local={Local(control, Control.BorderBrushProperty)} Padding local={Local(control, Control.PaddingProperty)}");
                    Note($"    rest: bg={Brush(control.Background)} fg={Brush(control.Foreground)} border={Brush(control.BorderBrush)}" +
                         $" padding={control.Padding} corner={Read(element, "CornerRadius")} minH={control.MinHeight}");
                }

                Note(string.Empty);
                Note("  window (depth 4) after everything:");
                Dump("    ", window, 4);
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

    private static void TypeInventory()
    {
        var seen = new List<Type>();
        foreach (var assembly in new[] { typeof(Button).Assembly, typeof(Control).Assembly })
        {
            Type[] exported;
            try
            {
                exported = assembly.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                exported = [];
            }

            foreach (var type in exported.Where(type => type.Name.StartsWith("Tab", StringComparison.Ordinal)))
            {
                if (seen.Any(existing => existing.FullName == type.FullName))
                {
                    continue;
                }

                seen.Add(type);
                var chain = new List<string>();
                for (var walk = type.BaseType; walk is not null && chain.Count < 4; walk = walk.BaseType)
                {
                    chain.Add(walk.Name);
                }

                Note($"  {type.FullName} : {string.Join(" : ", chain)}  (abstract={type.IsAbstract}, sealed={type.IsSealed})");
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Select(property => property.Name)
                    .Order(StringComparer.Ordinal)
                    .ToList();
                Note($"    declared properties ({properties.Count}): { string.Join(", ", properties) }");
                var dependency = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(field => field.FieldType.Name.Contains("DependencyProperty", StringComparison.Ordinal))
                    .Select(field => field.Name[..^"Property".Length])
                    .Order(StringComparer.Ordinal)
                    .ToList();
                Note($"    dependency properties ({dependency.Count}): {string.Join(", ", dependency)}");
                var parts = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic)
                    .Where(field => field.Name.StartsWith("PART", StringComparison.OrdinalIgnoreCase))
                    .Select(field => $"{field.Name}={field.GetValue(null)}")
                    .ToList();
                Note($"    part-name constants: {(parts.Count == 0 ? "none" : string.Join(", ", parts))}");
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(method => method.Name.Contains("Render", StringComparison.Ordinal) || method.Name.Contains("Template", StringComparison.Ordinal))
                    .Select(method => method.Name)
                    .Distinct()
                    .Order(StringComparer.Ordinal)
                    .ToList();
                Note($"    render/template hooks: {(methods.Count == 0 ? "none" : string.Join(", ", methods))}");
            }
        }

        if (seen.Count == 0)
        {
            Note("  no exported Tab* type at all - the objective's own-type call is settled by that alone");
        }
    }

    private static string FrameworkRows(Application application)
    {
        var found = new List<string>();
        var dictionaries = new List<object>();
        var resources = application.Resources;
        if (resources is not null)
        {
            dictionaries.Add(resources);
            if (ReadList(resources, "MergedDictionaries") is { } merged)
            {
                dictionaries.AddRange(merged);
            }
        }

        foreach (var dictionary in dictionaries)
        {
            foreach (var key in EnumerateKeys(dictionary))
            {
                if (key.Contains("Tab", StringComparison.Ordinal))
                {
                    found.Add(key);
                }
            }
        }

        found.Sort(StringComparer.Ordinal);
        var probed = new[] { "TabViewBackground", "TabViewItemHeaderBackgroundSelected", "TabViewItemMinHeight", "TabViewHeaderPadding" }
            .Select(key => key + "=" + Describe(application.TryFindResource(key)));
        return $"  dictionaries walked={dictionaries.Count} keys={found.Count}: {string.Join(", ", found.Take(60))}" + Environment.NewLine +
               "  upstream names by lookup: " + string.Join("  ", probed);
    }

    private static List<string> EnumerateKeys(object dictionary)
    {
        var names = new List<string>();
        if (ReadList(dictionary, "Keys") is not { } keys)
        {
            return names;
        }

        foreach (var key in keys)
        {
            names.Add(key?.ToString() ?? "null");
        }

        return names;
    }

    private static List<FrameworkElement> ChildrenOfType(DependencyObject root, string simpleName)
    {
        var found = new List<FrameworkElement>();
        void Walk(DependencyObject node)
        {
            if (node.GetType().Name == simpleName && node is FrameworkElement element)
            {
                found.Add(element);
            }

            if (node is not Visual visual)
            {
                return;
            }

            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
            {
                if (VisualTreeHelper.GetChild(visual, index) is { } child)
                {
                    Walk(child);
                }
            }
        }

        Walk(root);
        return found;
    }

    private static List<string> BodyTexts(DependencyObject root)
    {
        var texts = new List<string>();
        foreach (var block in ChildrenOfType(root, "TextBlock"))
        {
            if (block is TextBlock text && !string.IsNullOrEmpty(text.Text) && text.IsVisible)
            {
                texts.Add($"\"{Trim(text.Text)}\"");
            }
        }

        return texts;
    }

    private static string SendKeyDown(UIElement target, string keyName)
    {
        var keyType = typeof(UIElement).Assembly.GetType("Jalium.UI.Input.Key")
                      ?? typeof(Button).Assembly.GetType("Jalium.UI.Input.Key");
        if (keyType is null || !Enum.TryParse(keyType, keyName, out var key))
        {
            return $"no Key enum for {keyName}";
        }

        var argsType = typeof(UIElement).Assembly.GetType("Jalium.UI.Input.KeyEventArgs")
                       ?? typeof(Button).Assembly.GetType("Jalium.UI.Input.KeyEventArgs");
        var device = typeof(Jalium.UI.Input.Keyboard).GetProperty("PrimaryDevice", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        if (argsType is null || device is null)
        {
            return $"KeyEventArgs={argsType?.Name ?? "missing"} PrimaryDevice={device?.GetType().Name ?? "missing"}";
        }

        var args = (RoutedEventArgs)Activator.CreateInstance(argsType, device, 0, key)!;
        var routed = typeof(UIElement).GetField("KeyDownEvent", BindingFlags.Public | BindingFlags.Static);
        if (routed?.GetValue(null) is not RoutedEvent routedDown)
        {
            return "no UIElement.KeyDownEvent";
        }

        args.RoutedEvent = routedDown;
        args.Source = target;
        target.RaiseEvent(args);
        return $"ok (handled={args.Handled})";
    }

    // ---------- helpers ----------

    private static void Note(string line) => Lines.Add(line);

    private static string Trim(string text) => text.Replace('\r', ' ').Replace('\n', ' ').Trim();

    private static string Describe(object? value) => value switch
    {
        null => "null",
        string text => $"\"{Trim(text)}\"",
        _ => Trim(value.ToString() ?? string.Empty),
    };

    private static string Read(DependencyObject target, string name) =>
        target.GetType().GetProperty(name)?.GetValue(target)?.ToString() ?? "null";

    private static object? ReadObject(DependencyObject target, string name) =>
        target.GetType().GetProperty(name)?.GetValue(target);

    private static List<object>? ReadList(object target, string name)
    {
        var property = target.GetType().GetProperty(name);
        if (property?.GetValue(target) is not IEnumerable sequence)
        {
            return null;
        }

        return sequence.Cast<object>().ToList();
    }

    private static string Try(Action action)
    {
        try
        {
            action();
            return "ok";
        }
        catch (Exception exception)
        {
            return exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message);
        }
    }

    private static bool SetProperty(object target, string name, object? value)
    {
        var property = target.GetType().GetProperty(name);
        if (property is null || !property.CanWrite)
        {
            throw new InvalidOperationException($"{target.GetType().Name} exposes no writable {name}");
        }

        property.SetValue(target, value);
        return true;
    }

    private static bool Local(DependencyObject target, DependencyProperty property) => target.HasLocalValue(property);

    private static FrameworkElement? Part(DependencyObject root, string name)
    {
        if (root is not Visual visual)
        {
            return null;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
        {
            var child = VisualTreeHelper.GetChild(visual, index);
            if (child is FrameworkElement element && element.Name == name)
            {
                return element;
            }

            if (Part(child, name) is { } nested)
            {
                return nested;
            }
        }

        return null;
    }

    private static string PartNames(DependencyObject element)
    {
        var found = new List<string>();
        foreach (var name in new[] { "HeaderPanel", "PART_SelectedContentHost", "SelectedContentPresenter", "ContentPanel", "Border", "ContentSite", "Site" })
        {
            if (Part(element, name) is not null)
            {
                found.Add(name);
            }
        }

        var names = new List<string>();
        void Walk(DependencyObject node)
        {
            if (node is FrameworkElement element && !string.IsNullOrEmpty(element.Name))
            {
                names.Add($"{element.Name}:{element.GetType().Name}");
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

        Walk(element);
        if (names.Count == 0)
        {
            return string.Join(", ", found);
        }

        var looked = found.Count == 0 ? "named" : "looked: " + string.Join(", ", found);
        return looked + " | " + string.Join(", ", names.Distinct());
    }

    private static void Report(string label, FrameworkElement element)
    {
        var control = element as Control;
        Note($"  {label} {element.GetType().Name}: tmpl={(control?.Template is null ? "null" : "set")}" +
             $" bg={Brush(control?.Background)} border={Brush(control?.BorderBrush)} fg={Brush(control?.Foreground)}" +
             $" padding={control?.Padding} minH={control?.MinHeight}" +
             $" desired={element.DesiredSize.Width:0.##}x{element.DesiredSize.Height:0.##} size={element.ActualWidth:0.##}x{element.ActualHeight:0.##}");
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
            builder.Append($" bg={Brush(control.Background)} fg={Brush(control.Foreground)} tmpl={(control.Template is null ? "null" : "set")} sel={ReadObject(node, "IsSelected")}");
        }
        else if (node is Border border)
        {
            builder.Append($" bg={Brush(border.Background)} radius={border.CornerRadius}");
        }
        else if (node is TextBlock textBlock)
        {
            builder.Append($" text=\"{Trim(textBlock.Text ?? string.Empty)}\"");
        }

        if (node is Popup popup)
        {
            builder.Append($" popup(open={popup.IsOpen})");
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

    private static string Brush(Brush? brush) =>
        brush is SolidColorBrush solid ? $"#{solid.Color.A:X2}{solid.Color.R:X2}{solid.Color.G:X2}{solid.Color.B:X2}"
        : brush is null ? "-"
        : brush.GetType().Name;

    private static int Pump(int frames = 6, int budgetMilliseconds = 1200)
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
