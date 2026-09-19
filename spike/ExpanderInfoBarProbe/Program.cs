using System.Reflection;
using System.Text;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Automation.Provider;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace ExpanderInfoBarProbe;

/// <summary>
/// Throwaway probe, pass 3. Pass 2 found that InfoBar paints itself in OnRender and steps aside
/// only when a template gives it a part named RootBorder, but that its Template never materialises -
/// Expander opts into template content management in its constructor and InfoBar does not.
/// UseTemplateContentManagement is protected on ContentControl, so a subclass can call it without
/// touching framework internals. This pass measures whether that makes the template real, whether
/// the framework then wires PART_CloseButton, and what the public RaiseEvent lever needs as arguments
/// (it is the only pointer-free way to drive a control's own input handlers).
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "expander-probe3.txt");

    /// <summary>The candidate adaptation: same control, template content management switched on.</summary>
    private sealed class TemplatedInfoBar : InfoBar
    {
        public TemplatedInfoBar() => UseTemplateContentManagement();
    }

    [STAThread]
    private static int Main()
    {
        InputSignatures();

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

    private static void InputSignatures()
    {
        Note("=== P. constructing routed events without an input device ===");
        foreach (var type in new[] { typeof(MouseButtonEventArgs), typeof(KeyEventArgs), typeof(PointerEventArgs), typeof(RoutedEventArgs) })
        {
            Note($"  --- {type.Name} ---");
            foreach (var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                Note($"    ({string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");
            }
        }

        var raise = typeof(UIElement).GetMethod("RaiseEvent", BindingFlags.Public | BindingFlags.Instance);
        Note($"  UIElement.RaiseEvent: {raise?.ReturnType.Name} ({string.Join(", ", raise?.GetParameters().Select(p => p.ParameterType.Name) ?? [])})");
        foreach (var name in new[] { "MouseDownEvent", "MouseUpEvent", "KeyDownEvent", "KeyUpEvent", "PointerPressedEvent" })
        {
            var field = typeof(UIElement).GetField(name, BindingFlags.Public | BindingFlags.Static);
            Note($"  UIElement.{name}: {(field is null ? "missing" : field.FieldType.Name)}");
        }
    }

    private static void Run(Application application)
    {
        var dictionary = (ResourceDictionary)XamlReader.Parse(Markup)!;
        ControlTemplate Template(string key) => (ControlTemplate)dictionary[key]!;

        var plain = new InfoBar { Title = "plain", Message = "message", Width = 360 };
        var notemplated = new TemplatedInfoBar { Title = "opt-in, no template", Message = "message", Width = 360 };
        var templated = new TemplatedInfoBar
        {
            Title = "opt-in + template",
            Message = "message",
            Content = "content",
            Width = 360,
            Template = Template("InfoBarTemplate"),
        };
        var expander = new Expander { Header = "header", Content = "body", Width = 320, Template = Template("ExpanderTemplate") };

        var root = new StackPanel { Margin = new Thickness(24) };
        foreach (var element in new FrameworkElement[] { plain, notemplated, templated, expander })
        {
            root.Children.Add(element);
        }

        var clicks = 0;
        var closed = 0;
        templated.CloseButtonClick += (_, _) => clicks++;
        templated.Closed += (_, _) => closed++;
        var expanded = 0;
        var collapsed = 0;
        expander.Expanded += (_, _) => expanded++;
        expander.Collapsed += (_, _) => collapsed++;

        var window = new Window { Content = root, Width = 980, Height = 900, Title = "Expander / InfoBar probe 3" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Pump();

                Note("=== Q. does the protected opt-in make InfoBar's template real? ===");
                foreach (var (label, bar) in new[] { ("plain", (InfoBar)plain), ("opt-in, no template", notemplated), ("opt-in + template", templated) })
                {
                    Note($"  {label}: visual children={Count(bar)} height={bar.ActualHeight:0.##} width={bar.ActualWidth:0.##} template={bar.Template?.GetType().Name ?? "null"}");
                    Dump("    ", bar, 9);
                }

                Note("=== Q2. PART_CloseButton wired by the base class? ===");
                if (Descendant(templated, "PART_CloseButton") is Button close)
                {
                    try
                    {
                        ((IInvokeProvider)new ButtonAutomationPeer(close)).Invoke();
                        Pump();
                        Note($"  after Invoke: CloseButtonClick={clicks} Closed={closed} IsOpen={templated.IsOpen}");
                    }
                    catch (Exception exception)
                    {
                        Note($"  invoke threw {exception.GetType().Name}: {Trim(exception.Message)}");
                    }
                }
                else
                {
                    Note("  PART_CloseButton absent");
                }

                Note("=== Q3. IsOpen=false with the template ===");
                templated.IsOpen = false;
                Pump();
                Note($"  height={templated.ActualHeight:0.##} visible={templated.IsVisible}");
                Dump("    ", templated, 6);
                templated.IsOpen = true;
                Pump();

                Note("=== R. public RaiseEvent: can a test drive the Expander's own handlers? ===");
                RaiseHeaderClick(expander);
                Pump();
                Note($"  after header MouseDown raise: IsExpanded={expander.IsExpanded} expanded={expanded} collapsed={collapsed} {State(expander, "PART_ContentBorder")}");
                RaiseKey(expander, Key.Enter);
                Pump();
                Note($"  after Enter raise: IsExpanded={expander.IsExpanded} expanded={expanded} collapsed={collapsed}");

                Note("=== R2. the same lever on a native (unretemplated) Expander ===");
                var native = new Expander { Header = "native", Content = "body", Width = 320 };
                root.Children.Add(native);
                Pump();
                var nativeExpanded = 0;
                native.Expanded += (_, _) => nativeExpanded++;
                RaiseHeaderClick(native);
                Pump();
                Note($"  native after header MouseDown raise: IsExpanded={native.IsExpanded} events={nativeExpanded} {State(native, "PART_ContentBorder")}");
            }
            catch (Exception exception)
            {
                Note("MOUNT threw " + exception.GetType().Name + ": " + Trim(exception.ToString()));
            }
            finally
            {
                application.Shutdown();
            }
        };

        window.Show();
        application.Run(window);
    }

    private static void RaiseHeaderClick(Expander expander)
    {
        if (Descendant(expander, "PART_HeaderBorder") is not { } header)
        {
            Note("  no PART_HeaderBorder to raise on");
            return;
        }

        try
        {
            var arguments = NewMouseButtonEventArgs(header);
            if (arguments is null)
            {
                return;
            }

            header.RaiseEvent(arguments);
            Note($"  raised MouseDownEvent on {header.GetType().Name} PART_HeaderBorder");
        }
        catch (Exception exception)
        {
            Note($"  RaiseEvent threw {exception.GetType().Name}: {Trim(exception.Message)}");
        }
    }

    private static MouseButtonEventArgs? NewMouseButtonEventArgs(UIElement target)
    {
        foreach (var ctor in typeof(MouseButtonEventArgs).GetConstructors(BindingFlags.Public | BindingFlags.Instance))
        {
            var parameters = ctor.GetParameters();
            var arguments = new object?[parameters.Length];
            var ok = true;
            for (var index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                arguments[index] = parameter.ParameterType switch
                {
                    var type when type == typeof(MouseDevice) => Mouse.PrimaryDevice,
                    var type when type == typeof(UIElement) => target,
                    var type when type == typeof(int) => 0,
                    var type when type == typeof(MouseButton) => MouseButton.Left,
                    var type when type == typeof(short) => (short)1,
                    var type when type == typeof(ModifierKeys) => ModifierKeys.None,
                    var type when type == typeof(Point) => new Point(4, 4),
                    var type when type == typeof(uint) => 0u,
                    var type when type == typeof(TimeSpan) => TimeSpan.Zero,
                    var type when type.IsValueType => Activator.CreateInstance(type),
                    _ => null,
                };

                if (arguments[index] is null && !parameter.IsOptional)
                {
                    ok = false;
                    break;
                }
            }

            if (!ok)
            {
                continue;
            }

            try
            {
                var created = (MouseButtonEventArgs?)ctor.Invoke(arguments);
                if (created is not null)
                {
                    created.RoutedEvent = UIElement.MouseDownEvent;
                    created.Source = target;
                    Note($"  built MouseButtonEventArgs via ({string.Join(", ", parameters.Select(p => p.ParameterType.Name))})");
                    return created;
                }
            }
            catch (Exception exception)
            {
                Note($"  ctor ({string.Join(", ", parameters.Select(p => p.ParameterType.Name))}) threw {exception.GetType().Name}: {Trim(exception.Message)}");
            }
        }

        Note("  no constructible MouseButtonEventArgs signature");
        return null;
    }

    private static void RaiseKey(Expander expander, Key key)
    {
        try
        {
            var arguments = (KeyEventArgs?)Activator.CreateInstance(
                typeof(KeyEventArgs),
                BindingFlags.Public | BindingFlags.Instance,
                null,
                [Mouse.PrimaryDevice, expander, 0, key, ModifierKeys.None],
                null);
            if (arguments is null)
            {
                Note("  KeyEventArgs: no 5-argument ctor");
                return;
            }

            arguments.RoutedEvent = UIElement.KeyDownEvent;
            arguments.Source = expander;
            expander.RaiseEvent(arguments);
            Note($"  raised KeyDown({key}) on Expander");
        }
        catch (Exception exception)
        {
            Note($"  RaiseKey threw {exception.GetType().Name}: {Trim(exception.Message)}");
        }
    }

    private static string State(DependencyObject root, string name)
    {
        if (Descendant(root, name) is not { } element)
        {
            return $"{name}:absent";
        }

        var visibility = element.GetType().GetProperty("Visibility")?.GetValue(element);
        var size = element is FrameworkElement frameworkElement
            ? $" {frameworkElement.ActualWidth:0.##}x{frameworkElement.ActualHeight:0.##}"
            : string.Empty;
        return $"{name}:vis={visibility}{size}";
    }

    private static int Count(DependencyObject node) => VisualTreeHelper.GetChildrenCount(node);

    private static FrameworkElement? Descendant(DependencyObject root, string name)
    {
        if (root is FrameworkElement element && element.Name == name)
        {
            return element;
        }

        var children = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < children; index++)
        {
            if (VisualTreeHelper.GetChild(root, index) is { } child && Descendant(child, name) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    private const string Markup = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <ControlTemplate x:Key='InfoBarTemplate' TargetType='InfoBar'>
            <Border x:Name='RootBorder' Background='#FFFF00FF' CornerRadius='4' MinHeight='48'>
              <Grid>
                <Grid.ColumnDefinitions>
                  <ColumnDefinition Width='Auto' />
                  <ColumnDefinition Width='*' />
                  <ColumnDefinition Width='Auto' />
                </Grid.ColumnDefinitions>
                <TextBlock Text='icon' />
                <StackPanel Grid.Column='1'>
                  <TextBlock Text='{TemplateBinding Title}' />
                  <TextBlock Text='{TemplateBinding Message}' />
                  <ContentPresenter Content='{TemplateBinding Content}' />
                </StackPanel>
                <Button x:Name='PART_CloseButton' Grid.Column='2' Content='x' Width='38' Height='38' />
              </Grid>
            </Border>
          </ControlTemplate>
          <ControlTemplate x:Key='ExpanderTemplate' TargetType='Expander'>
            <Border x:Name='RootBorder' Background='#FF202020'>
              <StackPanel>
                <Border x:Name='PART_HeaderBorder' Background='#FF303030' MinHeight='48' Padding='16,0,0,0'>
                  <Grid>
                    <Grid.ColumnDefinitions>
                      <ColumnDefinition Width='*' />
                      <ColumnDefinition Width='Auto' />
                    </Grid.ColumnDefinitions>
                    <ContentPresenter x:Name='PART_HeaderContent' Content='{TemplateBinding Header}' VerticalAlignment='Center' />
                    <Border Grid.Column='1' Width='32' Height='32' Background='#FF404040'>
                      <Path x:Name='PART_Chevron' Width='12' Height='12' Stretch='Uniform'
                            Data='M 0 0 L 6 8 L 12 0' Stroke='White' StrokeThickness='1.5' />
                    </Border>
                  </Grid>
                </Border>
                <Border x:Name='PART_ContentBorder' Background='#FF505050' Visibility='Collapsed' MinHeight='40'>
                  <ContentPresenter Content='{TemplateBinding Content}' />
                </Border>
              </StackPanel>
            </Border>
          </ControlTemplate>
        </ResourceDictionary>
        """;

    private static void Note(string line) => Lines.Add(line);

    private static string Trim(string text) => text.Replace('\r', ' ').Replace('\n', ' ').Trim();

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
            builder.Append($" bg={Brush(control.Background)} fg={Brush(control.Foreground)}");
        }
        else if (node is Border border)
        {
            builder.Append($" bg={Brush(border.Background)}");
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
        var children = VisualTreeHelper.GetChildrenCount(visual);
        for (var index = 0; index < children; index++)
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

    private static int Pump(int frames = 6, int budgetMilliseconds = 600)
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
        watchdog.Change(TimeSpan.FromMilliseconds(budgetMilliseconds * 2), System.Threading.Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
        return seen;
    }
}
