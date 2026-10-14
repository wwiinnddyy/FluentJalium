// Census: can the native tab pair be retemplated at all, and through which route?
//
// The shape of the TabView batch turns on one answer. Pass 1 wrote a ControlTemplate straight onto TabControl and
// TabItem as a local value and got "set, and never built". Astra's working list styles never do that: they carry
// <Setter Property="Template"> inside an implicit style (Styles/ListBoxes.jalxaml:17-36), which is a different
// application path. So this pass merges that style route one type at a time - host alone, item alone, both - with a
// ListBox in the same window as the positive control that proves the probe can see a built template when one exists.
// It also walks the runtime's own type chain to name where UseTemplateContentManagement is declared, because a
// subclass can call a protected member that no outside caller can, and that difference is what decides between a
// subclass of the native pair and a type written from scratch.
using System.Reflection;
using System.Text;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

namespace TabViewStyle;

internal static class Program
{
    private static readonly List<string> Lines = [];
    private static readonly List<ResourceDictionary> Probes = [];

    // Styles for the two subclasses the shipping design is considering. The item one carries a state trigger on
    // purpose: the whole WinUI VisualState map has to travel as ControlTemplate.Triggers here, so the probe has
    // to say whether a trigger reaches pixels on a template that was built through the opt-in, not just whether
    // the template exists.
    private const string SubStyles = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                            xmlns:local='clr-namespace:TabViewStyle;assembly=TabViewStyle'>
          <Style TargetType='local:ProbeItemSub'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='local:ProbeItemSub'>
                <Border Name='ProbeSubRoot' Background='#FF00FF00' Padding='14,8'>
                  <ContentPresenter Name='ProbeSubHeader' Content='{TemplateBinding Header}' />
                </Border>
                <ControlTemplate.Triggers>
                  <Trigger Property='IsSelected' Value='True'>
                    <Setter TargetName='ProbeSubRoot' Property='Background' Value='#FF0000FF' />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>
          <Style TargetType='local:ProbeViewSub'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='local:ProbeViewSub'>
                <Border Name='ProbeViewRoot' Background='#FFFF00FF'>
                  <StackPanel>
                    <StackPanel Name='ProbeStripHost' Orientation='Horizontal' />
                    <ContentPresenter Name='ProbeBodyHost' Content='{TemplateBinding Content}' />
                  </StackPanel>
                </Border>
              </ControlTemplate>
            </Setter>
          </Style>
        </ResourceDictionary>
        """;

    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "tab-style1.txt");

    private const string TabsMarkup = """
        <TabControl xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                    Width='420' Height='130'>
          <TabItem Header='one'><TextBlock Text='body one' /></TabItem>
          <TabItem Header='two'><TextBlock Text='body two' /></TabItem>
        </TabControl>
        """;

    private const string ListsMarkup = """
        <ListBox xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                 xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                 Width='420' Height='130'>
          <ListBoxItem Content='alpha' /><ListBoxItem Content='beta' />
        </ListBox>
        """;

    // Three independent dictionaries, so a phase can merge exactly one and the host's failure cannot hide the
    // item's success. Each template root carries its own bright tint: an exact-zero count says the tree was never
    // built, a count with a bounding box says it was, and where the box sits says who painted it.
    private const string HostStyles = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <Style TargetType='TabControl'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='TabControl'>
                <Border Name='ProbeHostRoot' Background='#FFFF00FF'>
                  <StackPanel>
                    <ItemsPresenter Name='ProbeStrip' />
                    <ContentPresenter Name='ProbeBody' Content='{TemplateBinding SelectedContent}' />
                  </StackPanel>
                </Border>
              </ControlTemplate>
            </Setter>
          </Style>
        </ResourceDictionary>
        """;

    private const string ItemStyles = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <Style TargetType='TabItem'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='TabItem'>
                <Border Name='ProbeItemRoot' Background='#FF00FF00'>
                  <ContentPresenter Name='ProbeItemHeader' Content='{TemplateBinding Header}' />
                </Border>
              </ControlTemplate>
            </Setter>
          </Style>
        </ResourceDictionary>
        """;

    private const string ListStyles = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <Style TargetType='ListBox'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ListBox'>
                <Border Name='ProbeListRoot' Background='#FF0000FF'>
                  <ItemsPresenter Name='ProbeListItems' />
                </Border>
              </ControlTemplate>
            </Setter>
          </Style>
        </ResourceDictionary>
        """;

    private const string HostTemplate = """
        <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                         TargetType='TabControl'>
          <Border Name='ProbeHostRoot' Background='#FFFF00FF'>
            <StackPanel>
              <ItemsPresenter Name='ProbeStrip' />
              <ContentPresenter Name='ProbeBody' Content='{TemplateBinding SelectedContent}' />
            </StackPanel>
          </Border>
        </ControlTemplate>
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
            FluentThemeManager.Apply(application);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            Chain();
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

    /// <summary>Where the template-content opt-in is declared, read off the runtime's own chains.</summary>
    private static void Chain()
    {
        Note("=== A. the opt-in member, and what can reach it ===");
        foreach (var start in new[] { typeof(TabControl), typeof(TabItem), typeof(ListBox), typeof(ListBoxItem), typeof(ContentControl), typeof(ItemsControl) })
        {
            var chain = new List<string>();
            for (var type = start; type is not null && type != typeof(object); type = type.BaseType)
            {
                var declared = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(method => method.Name.Contains("TemplateContent", StringComparison.Ordinal)
                                     || method.Name.Contains("UseTemplate", StringComparison.Ordinal))
                    .Select(method => $"{method.Name}[{(method.IsFamily ? "protected" : method.IsPublic ? "public" : "private")}]")
                    .Distinct();
                chain.Add(type.Name + (declared.Any() ? " " + string.Join(",", declared) : string.Empty));
            }

            Note($"  {start.Name}: " + string.Join(" < ", chain));
        }

        Note(string.Empty);
        Note("=== B. what the tab pair does with its template, by member name ===");
        foreach (var type in new[] { typeof(TabControl), typeof(TabItem) })
        {
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                         .Where(method => method.Name.Contains("Template", StringComparison.Ordinal)
                                          || method.Name.Contains("Render", StringComparison.Ordinal)
                                          || method.Name.Contains("Content", StringComparison.Ordinal))
                         .OrderBy(method => method.Name, StringComparer.Ordinal))
            {
                var guard = method.IsPublic ? "public" : method.IsFamily ? "protected" : "private";
                Note($"  {type.Name}.{method.Name}({string.Join(",", method.GetParameters().Select(parameter => parameter.ParameterType.Name))}) {guard}");
            }
        }
    }

    private static void Run(Application application)
    {
        var root = new StackPanel { Margin = new Thickness(12) };
        var window = new Window { Content = root, Width = 520, Height = 1500, Title = "TabView style route" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Note(string.Empty);
                Note("=== C. positive control: does a style-setter template build for ListBox? ===");
                Phase(application, root, "ListBox via style", Merge(application, ListStyles), () => (FrameworkElement)XamlReader.Parse(ListsMarkup)!,
                    new[] { "ProbeListRoot", "ProbeListItems" }, 0xFF0000FF);

                Note(string.Empty);
                Note("=== D. TabControl alone, through the same route ===");
                Phase(application, root, "TabControl host style only", Merge(application, HostStyles), () => (TabControl)XamlReader.Parse(TabsMarkup)!,
                    new[] { "ProbeHostRoot", "ProbeStrip", "ProbeBody" }, 0xFFFF00FF);

                Note(string.Empty);
                Note("=== E. TabItem alone ===");
                Phase(application, root, "TabItem style only", Merge(application, ItemStyles), () => (TabControl)XamlReader.Parse(TabsMarkup)!,
                    new[] { "ProbeItemRoot", "ProbeItemHeader" }, 0xFF00FF00);

                Note(string.Empty);
                Note("=== F. both at once, then a selection flip ===");
                Merge(application, HostStyles);
                Merge(application, ItemStyles);
                var both = (TabControl)XamlReader.Parse(TabsMarkup)!;
                root.Children.Add(both);
                Pump(14);
                Note("  parts: " + string.Join("  ", new[] { "ProbeHostRoot", "ProbeStrip", "ProbeBody", "ProbeItemRoot" }
                    .Select(name => $"{name}={Named(both, name) is not null}")));
                Shot("F both", both, 0xFFFF00FF, 0xFF00FF00);
                Dump("    ", both, 6);
                Clear(root);
                Note("  SelectedIndex 0 -> 1: " + Try(() =>
                {
                    both.SelectedIndex = 1;
                    Pump(10);
                    var bodies = Descendants(both, "TextBlock").Select(Text).Where(text => text.StartsWith("body", StringComparison.Ordinal));
                    Note($"    visible body texts: {string.Join(",", bodies.DefaultIfEmpty("(none)"))}  SelectedContent={Get(both, "SelectedContent")?.GetType().Name ?? "null"}");
                    return "ok";
                }));
                Shot("F after flip", both, 0xFFFF00FF, 0xFF00FF00);
                Dump("    ", both, 6);
                foreach (var dictionary in Probes.ToList())
                {
                    Unmerge(application, dictionary);
                }

                Clear(root);

                Note(string.Empty);
                Note("=== G. local value again, in this same build, for the record ===");
                var local = (TabControl)XamlReader.Parse(TabsMarkup)!;
                var template = (ControlTemplate)XamlReader.Parse(HostTemplate)!;
                Note("  parsed template: " + template.GetType().Name);
                local.Template = template;
                root.Children.Add(local);
                Pump(14);
                Note("  parts: " + string.Join("  ", new[] { "ProbeHostRoot", "ProbeStrip", "ProbeBody" }
                    .Select(name => $"{name}={Named(local, name) is not null}")));
                Shot("G local value", local, 0xFFFF00FF);
                Dump("    ", local, 6);

                Note(string.Empty);
                Note("=== H. a TabItem subclass that opts in: does the template build, and does a trigger land? ===");
                Merge(application, SubStyles);
                var sub = new ProbeItemSub { Header = "one" };
                root.Children.Add(sub);
                Pump(14);
                Note("  parts: " + string.Join("  ", new[] { "ProbeSubRoot", "ProbeSubHeader" }
                    .Select(name => $"{name}={Named(sub, name) is not null}")));
                Shot("H rest", sub, 0xFF00FF00, 0xFF0000FF);
                Dump("    ", sub, 5);
                sub.IsSelected = true;
                Pump(12);
                Shot("H IsSelected=true", sub, 0xFF00FF00, 0xFF0000FF);
                Dump("    ", sub, 5);
                Clear(root);

                Note(string.Empty);
                Note("=== I. the same through a ContentControl subclass, the route the own type will take ===");
                var view = new ProbeViewSub { Content = new TextBlock { Text = "body one" } };
                root.Children.Add(view);
                Pump(14);
                Note("  parts: " + string.Join("  ", new[] { "ProbeViewRoot", "ProbeStripHost", "ProbeBodyHost" }
                    .Select(name => $"{name}={Named(view, name) is not null}")));
                Shot("I view subclass", view, 0xFFFF00FF);
                Dump("    ", view, 5);
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

    /// <summary>One phase: merge a dictionary, build a fresh control under it, read the parts, paint and count.</summary>
    private static void Phase(Application application, StackPanel root, string label, ResourceDictionary? merged,
        Func<FrameworkElement> build, string[] parts, uint tint)
    {
        Note($"  merged: {(merged is null ? "failed" : "ok")}");
        var control = build();
        root.Children.Add(control);
        Pump(14);
        Note("  parts: " + string.Join("  ", parts.Select(name => $"{name}={Named(control, name) is not null}")));
        Note($"  control read: {(control is TabControl tabs ? $"SelectedIndex={tabs.SelectedIndex} items={tabs.Items.Count}" : $"items={((ItemsControl)control).Items.Count}")}");
        Shot(label, control, tint);
        Dump("    ", control, 5);
        Clear(root);
        Unmerge(application, merged);
    }

    private static ResourceDictionary? Merge(Application application, string markup)
    {
        if (TryParse(markup) is not { } dictionary)
        {
            return null;
        }

        Probes.Add(dictionary);
        application.Resources.MergedDictionaries.Add(dictionary);
        return dictionary;
    }

    private static void Unmerge(Application application, ResourceDictionary? dictionary)
    {
        if (dictionary is null)
        {
            return;
        }

        Probes.Remove(dictionary);
        application.Resources.MergedDictionaries.Remove(dictionary);
    }

    private static void Clear(Panel panel)
    {
        panel.Children.Clear();
        Pump(6);
    }

    private static ResourceDictionary? TryParse(string markup)
    {
        try
        {
            return (ResourceDictionary)XamlReader.Parse(markup)!;
        }
        catch (Exception exception)
        {
            Note("  PARSE threw " + exception.GetType().Name + ": " + Trim(exception.GetBaseException().Message));
            return null;
        }
    }

    private static FrameworkElement? Named(DependencyObject root, string name)
    {
        // Matching by type name was vacuous: the probe reported the part absent on the ListBox positive control
        // while the tree dump two lines below showed it realized. A name search has to read FrameworkElement.Name.
        FrameworkElement? Visit(DependencyObject node)
        {
            if (node is FrameworkElement element && element.Name == name)
            {
                return element;
            }

            if (node is not Visual visual)
            {
                return null;
            }

            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
            {
                if (VisualTreeHelper.GetChild(visual, index) is { } child && Visit(child) is { } found)
                {
                    return found;
                }
            }

            return null;
        }

        return Visit(root);
    }

    private static List<DependencyObject> Descendants(DependencyObject root, string simpleName)
    {
        var found = new List<DependencyObject>();
        void Visit(DependencyObject node)
        {
            if (node is Visual visual)
            {
                if (node.GetType().Name == simpleName)
                {
                    found.Add(node);
                }

                for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
                {
                    if (VisualTreeHelper.GetChild(visual, index) is { } child)
                    {
                        Visit(child);
                    }
                }
            }
        }

        Visit(root);
        return found;
    }

    private static void Shot(string label, FrameworkElement surface, params uint[] tints)
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
        var colors = new SortedDictionary<uint, int>();
        for (var offset = 0; offset + 3 < buffer.Length; offset += 4)
        {
            var argb = 0xFF000000u | ((uint)buffer[offset + 2] << 16) | ((uint)buffer[offset + 1] << 8) | buffer[offset];
            colors[argb] = colors.GetValueOrDefault(argb) + 1;
        }

        Note($"  {label}: {width}x{height} distinct={colors.Count} " + string.Join("  ", tints
            .Select(tint => $"tint{Hex(tint)}={CountWithin(buffer, width, height, tint)}")));
        Note("    top: " + string.Join("  ", colors.OrderByDescending(pair => pair.Value).Take(6)
            .Select(pair => $"#{pair.Key & 0xFFFFFF:X6}x{pair.Value}")));
    }

    private static int CountWithin(byte[] buffer, int width, int height, uint argb)
    {
        var targetR = (int)((argb >> 16) & 0xFF);
        var targetG = (int)((argb >> 8) & 0xFF);
        var targetB = (int)(argb & 0xFF);
        var near = 0;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 4;
                var b = buffer[offset];
                var g = buffer[offset + 1];
                var r = buffer[offset + 2];
                if (Math.Abs(r - targetR) + Math.Abs(g - targetG) + Math.Abs(b - targetB) <= 96)
                {
                    near++;
                }
            }
        }

        return near;
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
            builder.Append($" bg={Hex(control.Background)} sel={Get(node, "IsSelected")}");
        }
        else if (node is TextBlock textBlock)
        {
            builder.Append($" text=\"{Trim(textBlock.Text ?? string.Empty)}\"");
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

    private static string Text(DependencyObject node) => (Get(node, "Text") as string) ?? string.Empty;

    private static object? Get(object target, string name) => target.GetType().GetProperty(name)?.GetValue(target);

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

    private static string Hex(Brush? brush)
    {
        if (brush is not SolidColorBrush solid)
        {
            return brush is null ? "-" : brush.GetType().Name;
        }

        var color = solid.Color;
        return $"#{ColorToRgb(color):X6}";
    }

    private static string Hex(uint argb) => $"#{argb & 0xFFFFFF:X6}";

    private static uint ColorToRgb(object color)
    {
        var r = Convert.ToByte(color.GetType().GetProperty("R")!.GetValue(color)!);
        var g = Convert.ToByte(color.GetType().GetProperty("G")!.GetValue(color)!);
        var b = Convert.ToByte(color.GetType().GetProperty("B")!.GetValue(color)!);
        return ((uint)r << 16) | ((uint)g << 8) | b;
    }

    private static string Trim(string text) => text.Length <= 180 ? text : text[..180] + "...";

    private static void Note(string line) => Lines.Add(line);

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

/// <summary>A TabItem subclass that flips the framework's template-content switch, which its chain allows.</summary>
public sealed class ProbeItemSub : TabItem
{
    public ProbeItemSub() => UseTemplateContentManagement();
}

/// <summary>A ContentControl subclass on the same route, standing in for the host an own type would be.</summary>
public sealed class ProbeViewSub : ContentControl
{
    public ProbeViewSub() => UseTemplateContentManagement();
}
