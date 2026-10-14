using System.Collections;
using System.Reflection;
using System.Text;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace FlyoutSurfaceProbe;

/// <summary>
/// Answers one question with numbers instead of a guess (docs/astra/audits/menu-flyout.md §5.11): when a menu
/// surface opens on 26.10.9, who actually paints it, what radius does that painter wear, and does the type
/// <c>MenuFlyoutPresenter</c> - which really exists - ever appear in the tree?
///   A: reflect the presenter type itself (base chain, own DPs, whether the theme gives it an implicit style),
///      and mount a hand-built instance to see whether our rows land on it at all.
///   B: open each of the three surfaces (MenuFlyout.ShowAt, ContextMenu.Open, MenuBarItem dropdown) and walk
///      the tree the popup really lands in, printing per node: colour identity (our row vs a framework literal),
///      border thickness, CornerRadius with local-value attribution, and the template's target type.
/// Modes: tree (A+B, no hold) | flyout | context | bar | presenter (hold one surface open for PrintWindow).
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];

    /// <summary>Rows whose brush instance a surface should hold if the theme pipeline reached it.</summary>
    private static readonly string[] RowNames =
    [
        "MenuFlyoutPresenterBackground", "MenuFlyoutPresenterBorderBrush", "MenuFlyoutPresenterBorderThemeThickness",
        "MenuFlyoutItemBackground", "MenuFlyoutItemBackgroundPointerOver", "MenuFlyoutItemForeground",
        "MenuFlyoutSubItemBackground", "MenuFlyoutSeparatorBackground",
        "OverlayCornerRadius", "ContextMenuBackground", "MenuBarBackground", "MenuBarItemBackground",
        "PopupBorderThemeThickness", "ControlCornerRadius",
    ];

    private static Application _application = null!;
    private static string _mode = "tree";
    private static string _logPath = "flyout-surface-tree.txt";

    [STAThread]
    private static int Main(string[] arguments)
    {
        _mode = arguments.Length > 0 ? arguments[0].ToLowerInvariant() : "tree";
        _logPath = Path.Combine(AppContext.BaseDirectory, $"flyout-surface-{_mode}.txt");
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

        File.WriteAllText(_logPath, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, mode={_mode}, full log: {_logPath}");
        return 0;
    }

    private static void Run(Application application)
    {
        var host = new Button { Content = "host", Width = 160 };

        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuFlyoutItem { Text = "cut", KeyboardAcceleratorTextOverride = "Ctrl+X" });
        flyout.Items.Add(new ToggleMenuFlyoutItem { Text = "wrap" });
        var subItem = new MenuFlyoutSubItem { Text = "more" };
        subItem.Items.Add(new MenuFlyoutItem { Text = "nested" });
        flyout.Items.Add(subItem);
        flyout.Items.Add(new MenuFlyoutSeparator());
        flyout.Items.Add(new MenuFlyoutItem { Text = "disabled", IsEnabled = false });

        var contextMenu = new ContextMenu();
        contextMenu.Items.Add(new MenuItem { Header = "cut" });
        contextMenu.Items.Add(new MenuItem { Header = "copy" });

        var menuBar = new MenuBar { Width = 420 };
        var barItem = new MenuBarItem { Title = "view" };
        ((IList)menuBar.Items).Add(barItem);
        Try(() => ((IList)barItem.GetType().GetProperty("Items")!.GetValue(barItem)!).Add(new MenuItem { Header = "options" }));

        var root = new StackPanel { Margin = new Thickness(24) };
        root.Children.Add(host);
        root.Children.Add(menuBar);
        root.Children.Add(new TextBlock { Text = "anchor", Width = 200 });

        var window = new Window { Content = root, Width = 900, Height = 700, Title = "Flyout surface probe" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Pump();
                Note($"theme dictionaries={FluentThemeManager.DictionaryNames.Count} presenterBackground={Res("MenuFlyoutPresenterBackground")} overlayCornerRadius={Res("OverlayCornerRadius")}");

                CensusPresenterType();

                switch (_mode)
                {
                    case "flyout":
                        OpenFlyout(flyout, host, window);
                        Hold();
                        break;
                    case "context":
                        OpenContext(contextMenu, window);
                        Hold();
                        break;
                    case "bar":
                        OpenBar(barItem, window);
                        Hold();
                        break;
                    case "context14":
                        OpenContext(contextMenu, window);
                        Note("  overriding the control radius back to 14 (the pre-fix state): " + Try(() => contextMenu.CornerRadius = new CornerRadius(14)));
                        Pump(8);
                        if (SurfaceBorder(contextMenu.Items[0] as DependencyObject) is FrameworkElement ten)
                        {
                            Note($"    surface now: {ten.ActualWidth:0.##}x{ten.ActualHeight:0.##} at {OffsetIn(ten, window)} radius={Raw(ten, "CornerRadius")}");
                        }
                        Hold();
                        break;
                    case "radius":
                        RadiusSource(contextMenu, window);
                        break;
                    case "sweep":
                        RadiusSweep(contextMenu, flyout, host, window);
                        break;
                    case "presenter":
                        MountPresenter(flyout, window);
                        Hold();
                        break;
                    default:
                        OpenFlyout(flyout, host, window);
                        OpenContext(contextMenu, window);
                        OpenBar(barItem, window);
                        MountPresenter(flyout, window);
                        break;
                }
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

    // ---------- A. the type itself ----------

    private static void CensusPresenterType()
    {
        Note(string.Empty);
        Note("=== A. what MenuFlyoutPresenter actually is on 26.10.9 ===");
        var type = typeof(MenuFlyout).Assembly.GetType("Jalium.UI.Controls.MenuFlyoutPresenter")
            ?? typeof(MenuFlyout).Assembly.GetType("Jalium.UI.Controls.Primitives.MenuFlyoutPresenter");
        if (type is null)
        {
            Note("  type not found by either namespace");
            return;
        }

        var chain = new List<string>();
        for (var current = type; current is not null; current = current.BaseType)
        {
            chain.Add(current.Name);
        }

        Note($"  {type.FullName} : {string.Join(" : ", chain.Skip(1))}");
        Note($"  accessibility: IsPublic={type.IsPublic} IsNestedPublic={type.IsNestedPublic} attributes={type.Attributes}");
        Note("  ctors: " + string.Join(" | ", type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(static constructor => $"({string.Join(",", constructor.GetParameters().Select(static p => p.ParameterType.Name))})"
                + (constructor.IsPublic ? " public" : " nonpublic"))));

        var properties = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static field => field.FieldType == typeof(DependencyProperty))
            .Select(static field => $"{field.Name}:{((DependencyProperty)field.GetValue(null)!).OwnerType.Name}")
            .ToList();
        Note("  own DPs: " + (properties.Count == 0 ? "none declared" : string.Join(", ", properties)));
        Note($"  implicit style by type key: {Read(() => _application.TryFindResource(type))}");
        Note($"  implicit style by name key: {Read(() => _application.TryFindResource("MenuFlyoutPresenter"))}");
        var template = type.GetProperty("Template", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Note($"  Template declared on: {template?.DeclaringType?.Name ?? "not found"}");
    }

    private static void MountPresenter(MenuFlyout flyout, Window window)
    {
        Note(string.Empty);
        Note("=== A2. mount a hand-built presenter: does anything style it? ===");
        var type = typeof(MenuFlyout).Assembly.GetType("Jalium.UI.Controls.MenuFlyoutPresenter");
        if (type is null)
        {
            Note("  type not found");
            return;
        }

        object? instance;
        try
        {
            instance = Activator.CreateInstance(type, [flyout]);
        }
        catch (Exception exception)
        {
            Note("  ctor threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            return;
        }

        if (instance is not FrameworkElement element)
        {
            Note("  instance is not a FrameworkElement: " + instance?.GetType().Name);
            return;
        }

        element.Width = 240;
        element.Height = 120;
        ((Panel)window.Content!).Children.Add(element);
        Try(() => ((Control)element).ApplyTemplate());
        Pump(10);
        Note($"  mounted: {Name(element)} size={element.ActualWidth:0.##}x{element.ActualHeight:0.##}");
        SurfaceLine("  ", element, 0);
        Note("  style: " + Read(() => element.GetType().GetProperty("Style")?.GetValue(element)?.GetType().Name ?? "null"));
        DumpTree("  ", element, 8);
    }

    // ---------- C. who feeds the radius ----------

    /// <summary>
    /// The ContextMenu surface arrives wearing CornerRadius 14 while upstream's presenter style asks for
    /// <c>OverlayCornerRadius</c> (which our own theme publishes as 8). The value is local on a Border the
    /// framework builds, so the only way to reach it is a property the framework copies from: enumerate every
    /// radius-shaped DP on the types in the path, then change-and-re-read instead of assuming.
    /// </summary>
    private static void RadiusSource(ContextMenu contextMenu, Window window)
    {
        Note(string.Empty);
        Note("=== C. who writes the popup radius ===");
        var types = new List<Type?>
        {
            typeof(ContextMenu), typeof(MenuFlyout), typeof(FlyoutBase), typeof(Popup),
            typeof(MenuFlyout).Assembly.GetType("Jalium.UI.Controls.Primitives.PopupRoot"),
            typeof(MenuFlyout).Assembly.GetType("Jalium.UI.Controls.PopupRoot"),
            typeof(MenuFlyout).Assembly.GetType("Jalium.UI.Controls.Primitives.MenuPopupScrollHost"),
            typeof(MenuFlyout).Assembly.GetType("Jalium.UI.Controls.MenuFlyoutPresenter"),
        };

        foreach (var type in types)
        {
            if (type is null)
            {
                continue;
            }

            var candidates = new List<string>();
            for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
            {
                foreach (var field in current.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    if (field.FieldType != typeof(DependencyProperty))
                    {
                        continue;
                    }

                    var property = (DependencyProperty)field.GetValue(null)!;
                    if (field.Name.Contains("Corner", StringComparison.OrdinalIgnoreCase)
                        || field.Name.Contains("Radius", StringComparison.OrdinalIgnoreCase)
                        || property.PropertyType.Name.Contains("CornerRadius"))
                    {
                        candidates.Add($"{current.Name}.{field.Name}:{property.PropertyType.Name}");
                    }
                }
            }

            Note($"  {type.FullName}: " + (candidates.Count == 0 ? "no radius-shaped DP on the chain" : string.Join(", ", candidates)));
        }

        Note("  Open: " + Try(() => contextMenu.Open(new Point(200, 200))));
        Pump(10);
        var border = SurfaceBorder(contextMenu.Items[0] as DependencyObject);
        if (border is null)
        {
            Note("  no Border ancestor found");
            return;
        }

        Note($"  surface Border {border.GetType().Name}: radius={Raw(border, "CornerRadius")} bg={Property(border, "Background")} bb={Property(border, "BorderBrush")}");
        Note("  ancestors: " + Ancestors(border));
        var radius = DependencyProperty.FromName(border.GetType(), "CornerRadius")!;
        foreach (var target in new DependencyObject[] { contextMenu, border })
        {
            var property = DependencyProperty.FromName(target.GetType(), "CornerRadius");
            if (property is null)
            {
                Note($"  {target.GetType().Name}: no CornerRadius property");
                continue;
            }

            Note($"  set {target.GetType().Name}.CornerRadius=3 -> " + Try(() => target.SetValue(property, new CornerRadius(3))));
            Pump(6);
            Note($"    surface now reads radius={Raw(border, "CornerRadius")}");
            Try(() => target.ClearValue(property));
            Pump(4);
            Note($"    after clearing: radius={Raw(border, "CornerRadius")}");

            // Does an explicit zero mean "square" or "unset"? Both readings explain every number above, and only
            // one of them lets a style write a corner this surface cannot have any other way.
            Note($"  set {target.GetType().Name}.CornerRadius=0 (explicit) -> " + Try(() => target.SetValue(property, new CornerRadius(0))));
            Pump(6);
            Note($"    surface now reads radius={Raw(border, "CornerRadius")}, control effective={Raw(contextMenu, "CornerRadius")}");
            Try(() => target.ClearValue(property));
            Pump(4);
        }
    }

    // ---------- D. the radius every popup surface actually wears ----------

    /// <summary>
    /// The ContextMenu measurement above showed the mechanism: the framework copies the control's own
    /// <c>CornerRadius</c> onto the Border it builds for the popup, as a local value. So the radius of a surface
    /// we cannot style is decided by a property we can. This is the sweep that turns "some controls' corners look
    /// wrong" into a table - each control's own value, whether our style wrote it, and what the opened surface
    /// really reads.
    /// </summary>
    private static void RadiusSweep(ContextMenu contextMenu, MenuFlyout flyout, Button host, Window window)
    {
        Note(string.Empty);
        Note("=== D. control-level CornerRadius vs the surface it produces (upstream flyout surfaces = OverlayCornerRadius 8) ===");
        var panel = (Panel)window.Content;
        var combo = new ComboBox { Width = 200 };
        combo.Items.Add("one");
        combo.Items.Add("two");
        combo.SelectedIndex = 0;
        var suggest = new AutoCompleteBox { Width = 200, ItemsSource = new[] { "Apple", "Banana" } };
        suggest.ItemFilter = (text, item) => item is string value && value.StartsWith(text, StringComparison.OrdinalIgnoreCase);
        var tip = new ToolTip { Content = "tip" };
        var menuItem = new MenuItem { Header = "submenu" };
        menuItem.Items.Add(new MenuItem { Header = "nested" });
        var menu = new Menu { Width = 240 };
        menu.Items.Add(menuItem);
        var barItem = new MenuBarItem { Title = "view" };
        var bar = new MenuBar { Width = 240 };
        ((IList)bar.Items).Add(barItem);

        foreach (var subject in new Control[] { contextMenu, menu, bar, barItem, tip, combo, suggest, host })
        {
            // A ToolTip that is parented into a panel can never open: measured, setting IsOpen throws
            // "The logical child already has a parent (child: ToolTip, current parent: StackPanel,
            // attempted parent: PopupRoot)". So it is read where it is and never mounted.
            if (!ReferenceEquals(subject, contextMenu) && !ReferenceEquals(subject, tip))
            {
                panel.Children.Add(subject);
            }

            var local = ReferenceEquals(subject.ReadLocalValue(Control.CornerRadiusProperty), DependencyProperty.UnsetValue) ? "from default/style" : "LOCAL";
            Note($"  {subject.GetType().Name}: radius={Raw(subject, "CornerRadius")} {local} bg={Property(subject, "Background")}");
        }

        Pump(8);
        Note(string.Empty);
        Note("  opened surfaces:");
        Note("  ContextMenu.Open: " + Try(() => contextMenu.Open(new Point(300, 300))));
        Pump(8);
        ReportAncestor("    ContextMenu", contextMenu.Items[0] as DependencyObject, node => node is Border);
        Note("  MenuFlyout.ShowAt: " + Try(() => flyout.ShowAt(host)));
        Pump(8);
        ReportAncestor("    MenuFlyout", flyout.Items[0] as DependencyObject, static node => node.GetType().Name == "MenuFlyoutPresenter");
        Note("  ComboBox.IsDropDownOpen=true: " + Try(() => combo.IsDropDownOpen = true));
        Pump(8);
        ReportAncestor("    ComboBox", combo.IsDropDownOpen ? FindInside(combo, static node => node.GetType().Name is "PopupRoot" or "Border") : null,
            static node => node.GetType().Name is "Border" or "PopupRoot");
        suggest.Text = "a";
        Pump(8);
        ReportAncestor("    AutoCompleteBox", suggest.IsDropDownOpen ? FindInside(suggest, static node => node.GetType().Name == "SuggestionsContainer") : null,
            static node => node.GetType().Name is "SuggestionsContainer" or "Border");
        Note("  ToolTip PlacementTarget=host: " + Try(() => tip.SetValue(GetProperty(tip, "PlacementTarget")!, host)));
        Note("  closing the context menu first: " + Try(() => contextMenu.SetValue(GetProperty(contextMenu, "IsOpen")!, false)));
        Pump(6);
        Note("  ToolTip IsOpen=true: " + Try(() => tip.SetValue(GetProperty(tip, "IsOpen")!, true)));
        Pump(8);
        Note("  MenuItem submenu (synth MouseDown on the item): " + Try(() => menuItem.RaiseEvent(new Jalium.UI.Input.MouseButtonEventArgs(
            Jalium.UI.Input.Mouse.PrimaryDevice, 0, Jalium.UI.Input.MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseDownEvent,
            Source = menuItem,
        })));
        Pump(8);
        Note($"    submenu open={Read(() => typeof(MenuItem).GetProperty("IsSubmenuOpen")?.GetValue(menuItem))}");
        Note(string.Empty);
        Note("  every PopupRoot still grafted into the host window (a MenuFlyout lives in a PopupWindow of its own, so it is not here):");
        var roots = new List<DependencyObject>();
        Walk(window, node =>
        {
            if (node.GetType().Name == "PopupRoot")
            {
                roots.Add(node);
            }
        }, 20);
        Note($"    count={roots.Count}");
        foreach (var root in roots)
        {
            DumpTree("      ", root, 4);
        }
    }

    private static void ReportAncestor(string label, DependencyObject? inside, Func<DependencyObject, bool> match)
    {
        if (inside is null)
        {
            Note($"{label}: nothing to read (surface not found in the tree)");
            return;
        }

        var current = inside;
        for (var hops = 0; hops < 20 && current is not null; hops++)
        {
            if (match(current))
            {
                Note($"{label}: {current.GetType().Name} radius={Raw(current, "CornerRadius")} bg={Property(current, "Background")} size=" +
                     $"{(current is FrameworkElement element ? $"{element.ActualWidth:0.##}x{element.ActualHeight:0.##}" : "-")}");
                return;
            }

            if (!TryParent(current, out var parent))
            {
                break;
            }

            current = parent;
        }

        Note($"{label}: no matching node above the starting element");
    }

    private static DependencyObject? FindInside(DependencyObject root, Func<DependencyObject, bool> match)
    {
        DependencyObject? found = null;
        void Visit(DependencyObject node, int depth)
        {
            if (found is not null || depth > 12)
            {
                return;
            }

            if (match(node))
            {
                found = node;
                return;
            }

            if (node is not Visual)
            {
                return;
            }

            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
            {
                Visit(VisualTreeHelper.GetChild(node, index), depth + 1);
            }
        }

        if (root is Visual)
        {
            Visit(root, 0);
        }

        return found;
    }

    private static DependencyProperty? GetProperty(DependencyObject node, string name) => DependencyProperty.FromName(node.GetType(), name);

    private static DependencyObject? SurfaceBorder(DependencyObject? inside)
    {
        var current = inside;
        for (var hops = 0; hops < 20 && current is not null; hops++)
        {
            if (current is Border)
            {
                return current;
            }

            if (!TryParent(current, out var parent))
            {
                return null;
            }

            current = parent;
        }

        return null;
    }

    // ---------- B. the three surfaces ----------
    private static void OpenFlyout(MenuFlyout flyout, Button host, Window window)
    {
        Note(string.Empty);
        Note("=== B1. MenuFlyout.ShowAt ===");
        Note("  ShowAt: " + Try(() => flyout.ShowAt(host)));
        Pump(12);
        Note($"  IsOpen={Read(() => typeof(MenuFlyout).GetProperty("IsOpen")?.GetValue(flyout))}");
        Note("  report: " + TryDeep(() => ReportSurface(flyout.Items[0] as DependencyObject, window)));
    }

    private static void OpenContext(ContextMenu contextMenu, Window window)
    {
        Note(string.Empty);
        Note("=== B2. ContextMenu.Open(new Point(200,200)) ===");
        Note("  Open: " + Try(() => contextMenu.Open(new Point(200, 200))));
        Pump(12);
        Note($"  IsOpen={Read(() => contextMenu.IsOpen)}");
        var first = contextMenu.Items[0] as DependencyObject;
        Note("  report: " + TryDeep(() => ReportSurface(first, window)));
        if (SurfaceBorder(first) is FrameworkElement border)
        {
            Note($"  surface for the capture: {border.ActualWidth:0.##}x{border.ActualHeight:0.##} at {OffsetIn(border, window)}" +
                 $" radius={Raw(border, "CornerRadius")} fill={HexOf(border, "Background")} stroke={HexOf(border, "BorderBrush")}");
        }
    }

    /// <summary>The literal colour a surface holds right now, so a captured frame can be searched for it.</summary>
    private static string HexOf(DependencyObject node, string propertyName)
    {
        var property = DependencyProperty.FromName(node.GetType(), propertyName);
        return property is null ? "-" : Brush(node.GetValue(property) as Brush);
    }

    /// <summary>Where the surface sits inside the window, so a captured frame can be read at the right row.</summary>
    private static string OffsetIn(FrameworkElement element, Visual ancestor)
    {
        try
        {
            var general = typeof(FrameworkElement).GetMethod("TransformToVisual")?.Invoke(element, [ancestor]);
            var point = general?.GetType().GetMethod("Transform")?.Invoke(general, [new Point(0, 0)]);
            return point?.ToString() ?? "no transform";
        }
        catch (Exception exception)
        {
            return "threw " + exception.GetType().Name;
        }
    }

    private static void OpenBar(MenuBarItem barItem, Window window)
    {
        Note(string.Empty);
        Note("=== B3. MenuBarItem dropdown (synthesized MouseDown) ===");
        Note("  MouseDown: " + Try(() => barItem.RaiseEvent(new Jalium.UI.Input.MouseButtonEventArgs(
            Jalium.UI.Input.Mouse.PrimaryDevice, 0, Jalium.UI.Input.MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseDownEvent,
            Source = barItem,
        })));
        Pump(12);
        Note($"  IsSubmenuOpen={Read(() => typeof(MenuBarItem).GetProperty("IsSubmenuOpen")?.GetValue(barItem))}");
        Note("  report: " + TryDeep(() => ReportSurface(barItem, window)));
    }

    /// <summary>
    /// Climb from a node the surface definitely contains to the root that hosts it, then walk back down: a popup
    /// can be grafted into the window's overlay or live in a root of its own, and which one it is part of the answer.
    /// </summary>
    private static void ReportSurface(DependencyObject? inside, Window window)
    {
        if (inside is null)
        {
            Note("  nothing to climb from");
            return;
        }

        var current = inside;
        var hops = 0;
        while (hops < 60 && TryParent(current, out var parent))
        {
            current = parent;
            hops++;
        }

        Note($"  climbed {hops} parents to {current.GetType().Name} (window content root reached: {ReferenceEquals(current, window.Content is DependencyObject content ? content : window)})");
        Note("  ancestors: " + Ancestors(inside));
        DumpTree("    ", current, 12);
        Note("  PopupRoots under the host window: " + CountNamed(window, ["PopupRoot", "Popup", "MenuPopupScrollHost", "MenuFlyoutPresenter"]));
    }

    private static string Ancestors(DependencyObject node)
    {
        var chain = new List<string>();
        var current = node;
        for (var hops = 0; hops < 24; hops++)
        {
            if (!TryParent(current, out var parent))
            {
                break;
            }

            chain.Add(TypeOf(parent) + (Name(parent).Length > 0 ? $"'{Name(parent)}'" : string.Empty));
            current = parent;
        }

        return string.Join(" < ", chain);
    }

    private static bool TryParent(DependencyObject node, out DependencyObject parent)
    {
        try
        {
            var candidate = VisualTreeHelper.GetParent(node);
            parent = candidate ?? null!;
            return candidate is not null;
        }
        catch (Exception)
        {
            parent = null!;
            return false;
        }
    }

    private static string CountNamed(DependencyObject root, string[] names)
    {
        var counts = new int[names.Length];
        Walk(root, node =>
        {
            for (var index = 0; index < names.Length; index++)
            {
                if (node.GetType().Name == names[index])
                {
                    counts[index]++;
                }
            }
        }, 14);
        return string.Join(" ", names.Select((name, index) => $"{name}={counts[index]}"));
    }

    private static void DumpTree(string prefix, DependencyObject root, int maxDepth)
    {
        var depth = 0;
        void Visit(DependencyObject node)
        {
            SurfaceLine(prefix + new string(' ', depth * 2), node, depth);
            if (depth >= maxDepth || node is not Visual)
            {
                return;
            }

            depth++;
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
            {
                Visit(VisualTreeHelper.GetChild(node, index));
            }

            depth--;
        }

        Visit(root);
    }

    private static void SurfaceLine(string prefix, DependencyObject node, int depth)
    {
        var builder = new StringBuilder($"{prefix}{node.GetType().Name}");
        var name = Name(node);
        if (name.Length > 0)
        {
            builder.Append($" '{name}'");
        }

        if (node is FrameworkElement element)
        {
            builder.Append($" {element.ActualWidth:0.##}x{element.ActualHeight:0.##}");
        }

        builder.Append($" bg={Property(node, "Background")}");
        builder.Append($" bb={Property(node, "BorderBrush")}");
        builder.Append($" bt={Raw(node, "BorderThickness")}");
        builder.Append($" radius={Raw(node, "CornerRadius")}");
        builder.Append($" margin={Raw(node, "Margin")}");
        builder.Append($" clip={Raw(node, "ClipToBounds")}");
        if (node is Control control)
        {
            builder.Append($" tmpl={(control.Template is null ? "null" : Read(() => (control.Template.GetType().GetProperty("TargetType")?.GetValue(control.Template) as Type)?.Name ?? "?"))}");
            builder.Append($" style={Read(() => control.GetType().GetProperty("Style")?.GetValue(control)?.GetType().Name ?? "null")}");
        }
        else if (node is Border)
        {
            builder.Append(" (Border)");
        }

        if (node is Popup popup)
        {
            builder.Append($" popup(open={popup.IsOpen})");
        }

        if (node is UIElement view && !view.IsVisible)
        {
            builder.Append(" [hidden]");
        }

        Note(builder.ToString());
    }

    /// <summary>Read a DependencyProperty by name on any node, naming the brush row when it is one of ours.</summary>
    private static string Property(DependencyObject node, string propertyName)
    {
        var property = DependencyProperty.FromName(node.GetType(), propertyName);
        if (property is null)
        {
            return "-";
        }

        var value = node.GetValue(property);
        if (value is not Brush brush)
        {
            return value is null ? "-" : Trim(value.ToString() ?? "-");
        }

        var local = node.ReadLocalValue(property);
        var suffix = ReferenceEquals(local, DependencyProperty.UnsetValue) ? string.Empty : " LOCAL";
        foreach (var name in RowNames)
        {
            if (Res(name) is Brush row && ReferenceEquals(row, brush))
            {
                return "=" + name + suffix;
            }
        }

        return Brush(brush) + suffix;
    }

    private static string Raw(DependencyObject node, string propertyName)
    {
        var property = DependencyProperty.FromName(node.GetType(), propertyName);
        if (property is null)
        {
            return "-";
        }

        var value = node.GetValue(property);
        var local = node.ReadLocalValue(property);
        var text = value switch
        {
            CornerRadius radius => $"{radius.TopLeft:0.##},{radius.TopRight:0.##},{radius.BottomRight:0.##},{radius.BottomLeft:0.##}",
            Thickness thickness => thickness.ToString(),
            null => "-",
            _ => Trim(value.ToString() ?? "-"),
        };

        return ReferenceEquals(local, DependencyProperty.UnsetValue) ? text : text + " LOCAL";
    }

    private static string TypeOf(DependencyObject node) => node.GetType().Name;

    private static string Name(DependencyObject node) => (node as FrameworkElement)?.Name ?? string.Empty;

    private static void Walk(DependencyObject node, Action<DependencyObject> visit, int maxDepth)
    {
        visit(node);
        if (maxDepth <= 0 || node is not Visual)
        {
            return;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            Walk(VisualTreeHelper.GetChild(node, index), visit, maxDepth - 1);
        }
    }

    private static void Hold()
    {
        Note(string.Empty);
        Note($"pid={Environment.ProcessId} holding for the capture");
        Console.WriteLine($"pid={Environment.ProcessId} holding");
        Thread.Sleep(20_000);
    }

    private static string Read(Func<object?> reader)
    {
        try
        {
            return reader()?.ToString() ?? "null";
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

    /// <summary>Same, but with the frame that threw: a probe that cannot say where it failed is a probe that learned nothing.</summary>
    private static string TryDeep(Action action)
    {
        try
        {
            action();
            return "ok";
        }
        catch (Exception exception)
        {
            var top = (exception.StackTrace ?? string.Empty).Split('\n').FirstOrDefault(string.Empty).Trim();
            return exception.GetType().Name + ": " + Trim(exception.Message) + " @" + Trim(top);
        }
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

    private static string Brush(Brush? brush) =>
        brush is SolidColorBrush solid ? $"#{solid.Color.A:X2}{solid.Color.R:X2}{solid.Color.G:X2}{solid.Color.B:X2}"
        : brush is null ? "-"
        : brush.GetType().Name;

    private static void Note(string line) => Lines.Add(line);

    private static string Trim(string text) => text.Replace('\r', ' ').Replace('\n', ' ').Trim();

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
