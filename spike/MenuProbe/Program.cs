using System.Collections;
using System.Reflection;
using System.Text;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Input;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace MenuProbe;

/// <summary>
/// Throwaway probe for the stage-4 menu batch, pass 6 - the acceptance run for the markup that now ships.
/// Passes 1-5 settled what the runtime has; this pass boots the real Astra theme, so Styles/Menus.jalxaml and
/// the two new dictionaries are what gets measured rather than a probe-local copy, and it answers the four
/// questions that decide whether the batch can be claimed:
///   Q: does each menu type get its template from the theme, and are its brushes OUR palette instances?
///   R: does the hosted tree still build - items inside Menu, MenuBar and an opened flyout - once the
///      templates are ours, and are the upstream part names present where we claim they are?
///   S: do the two value cells (Icon, KeyboardAcceleratorTextOverride) and the toggle's IsChecked cell fire?
///   T: can a submenu or a bar flyout open without a pointer, and does a Popup-less sub-item template break it?
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "menu-probe6.txt");

    /// <summary>Rows whose brush instance a control should be holding if our style reached it.</summary>
    private static readonly string[] RowNames =
    [
        "MenuFlyoutItemBackground", "MenuFlyoutItemBackgroundPointerOver", "MenuFlyoutItemBackgroundDisabled",
        "MenuFlyoutItemForeground", "MenuFlyoutItemForegroundDisabled",
        "MenuFlyoutSubItemBackground", "MenuFlyoutSubItemForeground", "MenuFlyoutSubItemChevron",
        "MenuFlyoutSeparatorBackground", "MenuFlyoutPresenterBackground", "MenuFlyoutPresenterBorderBrush",
        "MenuBarBackground", "MenuBarItemBackground", "MenuBarItemForeground", "MenuBarItemBorderBrush",
        "SubtleFillColorTransparentBrush", "SubtleFillColorSecondaryBrush", "TextFillColorPrimaryBrush", "TextFillColorDisabledBrush",
    ];

    private static Application _application = null!;

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
        Note("=== 0. did the theme load, and are the new rows in it? ===");
        Note($"  dictionaries={FluentThemeManager.DictionaryNames.Count} MenuFlyoutItemBackground={Res("MenuFlyoutItemBackground")}");
        Note($"  MenuFlyoutPresenterBackground={Res("MenuFlyoutPresenterBackground")} MenuBarItemMargin={Res("MenuBarItemMargin")}");

        var menu = new Menu { Width = 420 };
        menu.Items.Add(new MenuItem { Header = "file" });
        var submenuHost = new MenuItem { Header = "edit" };
        submenuHost.Items.Add(new MenuItem { Header = "copy" });
        menu.Items.Add(submenuHost);

        var menuBar = new MenuBar { Width = 420 };
        var barItem = new MenuBarItem { Title = "view" };
        ((IList)menuBar.Items).Add(barItem);

        var contextMenu = new ContextMenu();
        contextMenu.Items.Add(new MenuItem { Header = "cut" });

        var item = new MenuFlyoutItem { Text = "item", KeyboardAcceleratorTextOverride = "Ctrl+I" };
        var iconItem = new MenuFlyoutItem { Text = "icon", Icon = new TextBlock { Text = "#" } };
        var noKeyItem = new MenuFlyoutItem { Text = "no keys" };
        var subItem = new MenuFlyoutSubItem { Text = "sub" };
        subItem.Items.Add(new MenuFlyoutItem { Text = "nested" });
        var toggle = new ToggleMenuFlyoutItem { Text = "toggle" };
        var disabled = new MenuFlyoutItem { Text = "disabled", IsEnabled = false };
        var separator = new MenuFlyoutSeparator();

        var flyout = new MenuFlyout();
        flyout.Items.Add(item);
        flyout.Items.Add(iconItem);
        flyout.Items.Add(noKeyItem);
        flyout.Items.Add(subItem);
        flyout.Items.Add(toggle);
        flyout.Items.Add(disabled);
        flyout.Items.Add(separator);

        var host = new Button { Content = "host", Width = 140 };
        var root = new StackPanel { Margin = new Thickness(24) };
        root.Children.Add(host);
        root.Children.Add(menu);
        root.Children.Add(menuBar);
        root.Children.Add(new TextBlock { Text = "anchor", Width = 200 });

        var window = new Window { Content = root, Width = 900, Height = 700, Title = "Menu probe 6" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Pump();

                Note(string.Empty);
                Note("=== Q. theme-supplied template and brush identity, before anything opens ===");
                Report("Menu", menu);
                Report("MenuItem (first, in the bar)", (MenuItem)menu.Items[0]);
                Report("MenuBar", menuBar);
                Report("MenuBarItem", barItem);

                Note(string.Empty);
                Note("  Menu visual tree:");
                Dump("      ", menu, 7);
                Note("  MenuBar visual tree:");
                Dump("      ", menuBar, 7);

                Note(string.Empty);
                Note("=== R. the opened flyout: do our templates build, and are the part names there? ===");
                Note("  ShowAt: " + Try(() => flyout.ShowAt(host)));
                Pump(12);
                Note($"  flyout IsOpen={Read(flyout, "IsOpen")}");
                foreach (var element in new FrameworkElement[] { item, iconItem, noKeyItem, subItem, toggle, disabled, separator })
                {
                    Report($"  {element.GetType().Name} '{Read(element, "Text")}'", element);
                    Note("    parts: " + PartNames(element));
                    Dump("      ", element, 6);
                }

                Note(string.Empty);
                Note("=== S. value-driven cells: Icon, KeyboardAcceleratorTextOverride, IsChecked ===");
                Note($"  icon slot: with icon IconRoot={Visible(Part(iconItem, "IconRoot"))}, without={Visible(Part(item, "IconRoot"))}");
                Note($"  accelerator: with text={Visible(Part(item, "KeyboardAcceleratorTextBlock"))}, without={Visible(Part(noKeyItem, "KeyboardAcceleratorTextBlock"))}");
                Note($"  toggle rest: CheckGlyph opacity={(Part(toggle, "CheckGlyph") as UIElement)?.Opacity.ToString() ?? "missing"}");
                toggle.IsChecked = true;
                Pump(4);
                Note($"  toggle IsChecked=true: opacity={(Part(toggle, "CheckGlyph") as UIElement)?.Opacity.ToString() ?? "missing"}");
                toggle.IsChecked = false;
                Pump(2);
                Note($"  toggle back to false: opacity={(Part(toggle, "CheckGlyph") as UIElement)?.Opacity.ToString() ?? "missing"}");
                Note($"  disabled item fg={KeyOf(disabled.Foreground)} vs enabled fg={KeyOf(item.Foreground)}");
                Note("  disabled inner TextBlock: " + (Part(disabled, "TextBlock") is TextBlock block ? KeyOf(block.Foreground) : "missing"));

                Note(string.Empty);
                Note("=== T. submenu without a pointer: what is reachable ===");
                foreach (var name in new[] { "OpenSubMenuAndFocusFirstItem", "EnsureSubPopup", "FocusFirstSubMenuItem" })
                {
                    var method = typeof(MenuFlyoutSubItem).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (method is null)
                    {
                        Note($"  {name}: missing");
                        continue;
                    }

                    Note($"  {name}({string.Join(", ", method.GetParameters().Select(static p => p.ParameterType.Name))})");
                    var arguments = method.GetParameters().Select(static parameter => parameter.ParameterType.IsValueType
                        ? Activator.CreateInstance(parameter.ParameterType)
                        : null).ToArray();
                    Note("    invoke: " + Try(() => method.Invoke(subItem, arguments)));
                    Pump(6);
                    Note($"    after: IsSubMenuOpen={Read(subItem, "IsSubMenuOpen")} popups under the item={CountPopups(subItem)}");
                    Dump("      ", subItem, 5);
                }

                Note(string.Empty);
                Note("=== U. MenuBar: does the bar still open its item's flyout ===");
                foreach (var name in new[] { "OpenMenuAndFocusFirstItem", "OpenFromKeyboard", "FocusFirstMenuItem" })
                {
                    var method = typeof(MenuBar).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        ?? typeof(MenuBarItem).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    var owner = method?.DeclaringType?.Name ?? "-";
                    Note($"  {owner}.{name}: " + (method is null ? "missing on both" : Try(() => method.Invoke(method.DeclaringType == typeof(MenuBar) ? menuBar : barItem, []))));
                    Pump(6);
                    Note($"    bar item after: tmpl={(barItem.Template is null ? "null" : "set")} bg={KeyOf(barItem.Background)} submenu open={Read(barItem, "IsSubmenuOpen")}");
                }

                Note("  synthesized MouseDown on the bar item: " + Try(() => barItem.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = UIElement.MouseDownEvent,
                    Source = barItem,
                })));
                Pump(8);
                Note($"  after: submenu open={Read(barItem, "IsSubmenuOpen")} pressed={Read(barItem, "IsPressed")} bg={KeyOf(barItem.Background)}");
                Note("  ContentButton of the bar item: " + (Part(barItem, "ContentButton") is Button button ? $"found, tmpl={(button.Template is null ? "null" : "set")}" : "missing"));

                Note(string.Empty);
                Note("  window (depth 5) with everything driven:");
                Dump("    ", window, 5);

                Note(string.Empty);
                Note("=== V. ContextMenu.Open on a point ===");
                Note("  Open: " + Try(() => contextMenu.Open(new Point(200, 200))));
                Pump(10);
                Note($"  IsOpen={Read(contextMenu, "IsOpen")} template={(contextMenu.Template is null ? "null" : "set")} bg={KeyOf(contextMenu.Background)}");
                Dump("    ", window, 5);

                Note(string.Empty);
                Note("=== W. MenuItem's own state, driven through the control's handler ===");
                var target = (MenuItem)menu.Items[0];
                Note($"  rest: bg={KeyOf(target.Background)} fg={KeyOf(target.Foreground)} highlighted={Read(target, "IsHighlighted")}");
                Note("  synthesized MouseDown: " + Try(() => target.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = UIElement.MouseDownEvent,
                    Source = target,
                })));
                Pump(6);
                Note($"  after: bg={KeyOf(target.Background)} fg={KeyOf(target.Foreground)} highlighted={Read(target, "IsHighlighted")} pressed={Read(target, "IsPressed")}");
                Note("  local value on Background? " + (target.HasLocalValue(Control.BackgroundProperty) ? "yes (framework wrote it)" : "no"));
                target.IsEnabled = false;
                Pump(4);
                Note($"  IsEnabled=false: fg={KeyOf(target.Foreground)}");
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

    private static void Report(string label, FrameworkElement element)
    {
        var control = element as Control;
        Note($"  {label}: tmpl={(control?.Template is null ? "null" : "set")} bg={KeyOf(control?.Background)} border={KeyOf(control?.BorderBrush)} fg={KeyOf(control?.Foreground)}" +
             $" padding={control?.Padding} margin={element.Margin} minH={control?.MinHeight}" +
             $" desired={element.DesiredSize.Width:0.##}x{element.DesiredSize.Height:0.##} size={element.ActualWidth:0.##}x{element.ActualHeight:0.##}" +
             (control is null ? string.Empty : $" font={control.FontSize}"));
    }

    private static string PartNames(FrameworkElement element)
    {
        var found = new List<string>();
        foreach (var name in new[] { "LayoutRoot", "IconRoot", "IconContent", "TextBlock", "KeyboardAcceleratorTextBlock", "CheckGlyph", "CheckPlaceholder", "SubItemChevron", "ContentRoot", "ContentButton", "Background" })
        {
            if (Part(element, name) is not null)
            {
                found.Add(name);
            }
        }

        return found.Count == 0 ? "none" : string.Join(", ", found);
    }

    private static FrameworkElement? Part(FrameworkElement element, string name)
    {
        var method = typeof(Control).GetMethod("GetTemplateChild", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        try
        {
            if (method?.Invoke(element, [name]) is FrameworkElement direct)
            {
                return direct;
            }
        }
        catch (Exception)
        {
            // The lookup failing is not the answer; the tree walk below may still find the part.
        }

        return Descendant(element, name);
    }

    private static string Visible(DependencyObject? element) => element switch
    {
        null => "missing",
        UIElement ui => ui.IsVisible ? "visible" : "not visible",
        _ => element.GetType().Name,
    };

    private static int CountPopups(DependencyObject root)
    {
        var count = root is Popup ? 1 : 0;
        if (root is Visual visual)
        {
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
            {
                count += CountPopups(VisualTreeHelper.GetChild(visual, index));
            }
        }

        return count;
    }

    private static FrameworkElement? Descendant(DependencyObject root, string name)
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

            if (Descendant(child, name) is { } match)
            {
                return match;
            }
        }

        return null;
    }

    private static object? Res(string key)
    {
        try
        {
            return _application.TryFindResource(key);
        }
        catch (Exception exception)
        {
            return "threw " + exception.GetType().Name;
        }
    }

    /// <summary>Names the row whose brush instance this is, so "our style applied" is an identity and not a colour match.</summary>
    private static string KeyOf(Brush? brush)
    {
        if (brush is null)
        {
            return "-";
        }

        foreach (var name in RowNames)
        {
            if (Res(name) is Brush candidate && ReferenceEquals(candidate, brush))
            {
                return "=" + name;
            }
        }

        return Brush(brush);
    }

    private static string Patterns(FrameworkElement element)
    {
        var peer = UIElementAutomationPeer.CreatePeerForElement(element);
        if (peer is null)
        {
            return "no peer";
        }

        var found = new List<string>();
        foreach (var pattern in Enum.GetValues<PatternInterface>())
        {
            try
            {
                if (peer.GetPattern(pattern) is { } provider)
                {
                    found.Add($"{pattern}({provider.GetType().Name})");
                }
            }
            catch (Exception)
            {
                // Unsupported: that is the answer.
            }
        }

        return $"{peer.GetType().Name}: {(found.Count == 0 ? "none" : string.Join(", ", found))}";
    }

    private static string Read(object target, string name)
    {
        try
        {
            return target.GetType().GetProperty(name)?.GetValue(target)?.ToString() ?? "null";
        }
        catch (Exception exception)
        {
            return "threw " + exception.GetType().Name;
        }
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
            return exception.GetType().Name + ": " + Trim(exception.Message);
        }
    }

    private const string Markup = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <ControlTemplate x:Key='SubItemTemplate' TargetType='MenuFlyoutSubItem'>
            <Border x:Name='LayoutRoot' Background='#FFFF00FF' MinHeight='32'>
              <StackPanel Orientation='Horizontal'>
                <TextBlock Text='{TemplateBinding Text}' />
                <Path x:Name='SubItemChevron' Data='M 0 0 L 4 4 L 0 8' />
                <Popup x:Name='Popup'>
                  <Border Background='#FF00FF00'>
                    <ItemsPresenter x:Name='ItemsPresenter' />
                  </Border>
                </Popup>
              </StackPanel>
            </Border>
          </ControlTemplate>
          <ControlTemplate x:Key='SeparatorTemplate' TargetType='MenuFlyoutSeparator'>
            <Border x:Name='ProbeSeparator' Background='#FF00FFFF' Height='10' />
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
            builder.Append($" bg={Brush(control.Background)} fg={Brush(control.Foreground)} tmpl={(control.Template is null ? "null" : "set")}");
        }
        else if (node is Border border)
        {
            builder.Append($" bg={Brush(border.Background)}");
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
