using System.Diagnostics;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

namespace BreadcrumbProbe;

/// <summary>
/// The readings the BreadcrumbBar slice cannot pick a base type or a collapse mechanism without.
///   api    : upstream is BreadcrumbBar : Control + BreadcrumbBarItem : ContentControl over an ItemsRepeater,
///            a custom NonVirtualizingLayout, FocusManager.TryMoveFocus and AutomationProperties.LandmarkType.
///            The census says which of those names exist here, so the port is written against the runtime's
///            surface rather than WinUI's.
///   layout : upstream collapses crumbs by arranging them into a zero rect (BreadcrumbLayout.cpp:89-93), never
///            by Visibility, and decides inside the panel from the width it was given. Three questions follow,
///            and no type declaration answers them: does a constrained width actually reach a custom items
///            panel, does a zero-rect arrange really produce a zero-size child that stops painting, and can the
///            panel find the host it serves (the RadioButtons panel needed the Parent walk rather than
///            TemplatedParent - s1l).
/// Modes: api | layout | all.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];

    private static string _mode = "all";

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

        var path = Path.Combine(AppContext.BaseDirectory, $"breadcrumb-probe-{_mode}.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, mode={_mode}, full log: {path}");
        return 0;
    }

    private static void Run(Application application)
    {
        var root = new StackPanel { Margin = new Thickness(24) };
        var window = new Window { Content = root, Width = 900, Height = 700, Title = "Breadcrumb probe" };
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

                if (_mode is "all" or "layout")
                {
                    Layout(root);
                }

                if (_mode is "all" or "ink")
                {
                    Ink(root, collapsedInstead: false);
                }

                if (_mode is "all" or "hidden")
                {
                    Ink(root, collapsedInstead: true);
                }

                if (_mode == "style")
                {
                    Surface(root);
                }

                if (_mode == "narrow")
                {
                    Narrow(root);
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

    // ---------- A. what the runtime actually has ----------

    private static void Api()
    {
        Note("");
        Note("=== A. the names upstream's markup uses ===");
        foreach (var name in new[]
                 {
                     "BreadcrumbBar", "BreadcrumbBarItem", "ItemsRepeater", "StackLayout", "NonVirtualizingLayout",
                     "LayoutPanel", "ItemsControl", "ItemsPanelTemplate", "ContentControl", "FocusManager",
                     "AutomationProperties", "MenuFlyout", "MenuFlyoutPresenter", "Flyout", "FlyoutBase", "Popup",
                     "Path", "TextBlock", "Button", "ToolTipService",
                 })
        {
            var found = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => SafeTypes(assembly).FirstOrDefault(type => type.Name == name))
                .FirstOrDefault(type => type is not null);
            Note($"  {name}: {(found is null ? "ABSENT" : $"{found.FullName} : {found.BaseType?.Name}")}");
        }

        Note("");
        Note("=== A2. the members the port would lean on ===");
        MembersByName("FocusManager", "TryMoveFocus", "GetFocusedElement", "FindFirstFocusableElement");
        Members(typeof(AutomationProperties), "AutomationProperties", "LandmarkTypeProperty", "PositionInSetProperty",
            "SizeOfSetProperty", "AccessibilityViewProperty", "NameProperty", "SetPositionInSet", "SetName");
        Members(typeof(ItemsControl), "ItemsControl", "ItemsSource", "ItemTemplate", "ItemsPanel",
            "GetContainerForItemOverride", "IsItemItsOwnContainerOverride", "PrepareContainerForItemOverride",
            "ClearContainerForItemOverride", "ContainerFromItem", "ItemContainerStyle");
        Members(typeof(Panel), "Panel", "MeasureOverride", "ArrangeOverride", "Children", "InternalChildren");
        Members(typeof(FrameworkElement), "FrameworkElement", "Parent", "TemplatedParent", "FlowDirection", "Focus");
        Members(typeof(UIElement), "UIElement", "IsVisible", "Visibility", "GotFocus", "PreviewKeyDown", "CapturePointer");
    }

    // ---------- B. the collapse mechanism ----------

    private static void Layout(StackPanel root)
    {
        Note("");
        Note("=== B. custom items panel: width it is given, zero-rect arrange, host lookup ===");

        var bare = new CollapsePanel { Width = 400, Height = 32 };
        var crumbs = new List<TextBlock>();
        for (var index = 0; index < 6; index++)
        {
            var crumb = new TextBlock { Text = new string('a', 6 + (index * 6)), FontSize = 14 };
            crumbs.Add(crumb);
            bare.Children.Add(crumb);
        }

        root.Children.Add(bare);

        var host = new ItemsControl
        {
            Width = 400,
            ItemsPanel = new ItemsPanelTemplate { PanelType = typeof(CollapsePanel) },
            ItemsSource = new[] { "one", "two", "three", "four", "five", "six" },
        };
        root.Children.Add(host);

        Note($"  frames pumped={Pump(20, 1500)}");

        Note($"  bare panel: final {Round(bare.ActualWidth)}x{Round(bare.ActualHeight)} " +
             $"desired={Round(bare.DesiredSize.Width)} measureCalls={bare.MeasureCalls} " +
             $"lastAvailable={Round(bare.LastAvailableWidth)} arrangeCalls={bare.ArrangeCalls} " +
             $"lastFinal={Round(bare.LastFinalWidth)}");
        for (var index = 0; index < crumbs.Count; index++)
        {
            Note($"    crumb[{index}] actual={Round(crumbs[index].ActualWidth)}x{Round(crumbs[index].ActualHeight)} " +
                 $"isVisible={crumbs[index].IsVisible} visibility={crumbs[index].Visibility}");
        }

        var realized = Find(host, node => node is CollapsePanel) as CollapsePanel;
        if (realized is null)
        {
            Note("  ItemsPanel assignment realized no CollapsePanel under the ItemsControl (presenter ignored it).");
            return;
        }

        Note($"  realized panel: final={Round(realized.ActualWidth)}x{Round(realized.ActualHeight)} " +
             $"children={realized.Children.Count} measureCalls={realized.MeasureCalls} " +
             $"lastAvailable={Round(realized.LastAvailableWidth)} arrangeCalls={realized.ArrangeCalls} " +
             $"templatedParent={(realized.TemplatedParentType ?? "<null>")} host={realized.HostName ?? "<none>"}");
        foreach (var child in realized.Children.OfType<FrameworkElement>())
        {
            Note($"    realized child {child.GetType().Name} actual={Round(child.ActualWidth)} isVisible={child.IsVisible}");
        }
    }

    /// <summary>Arranges left to right inside the width it was given and zero-rects everything past it.</summary>
    private sealed class CollapsePanel : Panel
    {
        public int MeasureCalls;

        public int ArrangeCalls;

        public double LastAvailableWidth = -1;

        public double LastFinalWidth = -1;

        public string? HostName;

        public string? TemplatedParentType;

        /// <summary>Hide the overflow with Visibility instead of the zero rect, for pass D.</summary>
        public bool UseCollapsedInstead;

        protected override Size MeasureOverride(Size availableSize)
        {
            MeasureCalls++;
            LastAvailableWidth = availableSize.Width;
            double width = 0;
            double height = 0;
            foreach (UIElement child in Children)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                width += child.DesiredSize.Width;
                height = Math.Max(height, child.DesiredSize.Height);
            }

            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            ArrangeCalls++;
            LastFinalWidth = finalSize.Width;
            TemplatedParentType = TemplatedParent?.GetType().Name;
            HostName = DescribeHost();
            var x = 0d;
            var overflowed = false;
            foreach (UIElement child in Children)
            {
                if (!overflowed && (x + child.DesiredSize.Width) > finalSize.Width)
                {
                    overflowed = true;
                }

                if (overflowed)
                {
                    if (UseCollapsedInstead)
                    {
                        child.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        child.Arrange(new Rect(0, 0, 0, 0));
                    }
                }
                else
                {
                    child.Arrange(new Rect(x, 0, child.DesiredSize.Width, finalSize.Height));
                    x += child.DesiredSize.Width;
                }
            }

            return finalSize;
        }

        private string DescribeHost()
        {
            if (TemplatedParent is FrameworkElement templated)
            {
                return $"via TemplatedParent: {templated.GetType().Name}";
            }

            var node = Parent;
            for (var step = 0; step < 12 && node is not null; step++)
            {
                if (node is ItemsControl control)
                {
                    return $"via Parent[{step}]: {control.GetType().Name}";
                }

                node = (node as FrameworkElement)?.Parent;
            }

            return "<none>";
        }
    }

    /// <summary>
    /// "It is hidden" is a claim about pixels, so both hiding mechanisms get a capture. Four 100x40 solid boxes in
    /// a 250-wide panel; the rule hides the third and fourth, so the prediction was red 4000 + lime 4000 + 2000
    /// unpainted. Zero-rect arrange failed it: blue 0, yellow 4000, because the arranged rect sets the offset but
    /// not the extent - a child with its own Width/Height keeps 100x40 and re-stacks at the panel origin, so the
    /// last-drawn "hidden" box is what paints there. <c>Visibility=Collapsed</c> (pass D) passed as predicted:
    /// red 4000 + lime 4000, blue and yellow 0, and the hidden children report actual=0x0.
    /// </summary>
    /// <summary>
    /// Whether the two named styles reach the control at all, and what the applied template's own tree looks like.
    /// Asked because the behaviour tests read "no PART_ItemButton under FluentBreadcrumbBarItem", which is a claim
    /// about style lookup, about template materialisation, or about the markup inside the template - and only the
    /// loaded resource plus the walked tree tell those apart.
    /// </summary>
    private static void Surface(StackPanel root)
    {
        Note("");
        Note("=== E. does the breadcrumb style reach the control ===");
        foreach (var key in new[]
                 {
                     "DefaultBreadcrumbBarStyle", "DefaultBreadcrumbBarItemStyle", "BreadcrumbBarItemInlineButtonStyle",
                     "DefaultRadioButtonsStyle", "DefaultPipsPagerStyle",
                 })
        {
            Note($"  TryFindResource({key}) => {Application.Current?.TryFindResource(key)?.GetType().Name ?? "<null>"}");
        }

        var bar = new FluentJalium.Controls.FluentBreadcrumbBar { Width = 260, Height = 40 };
        for (var index = 0; index < 3; index++)
        {
            bar.Items.Add(new FluentJalium.Controls.FluentBreadcrumbBarItem { Content = $"Crumb {index}" });
        }

        root.Children.Add(new Border { Width = 260, Height = 40, Background = new SolidColorBrush(Colors.White), Child = bar });
        Note($"  frames pumped={Pump(20, 1500)}");
        Note($"  bar.Style={Name(bar.Style)} Template={Name(bar.Template)} children={VisualTreeHelper.GetChildrenCount(bar)}");
        Walk(bar, 1, 6);
        foreach (var type in new[]
                 {
                     typeof(FluentJalium.Controls.FluentBreadcrumbBarItem), typeof(FluentJalium.Controls.FluentTabViewItem),
                     typeof(FluentJalium.Controls.FluentBreadcrumbBar), typeof(Jalium.UI.Controls.ContentControl),
                 })
        {
            Note($"  implicit style for {type.Name} => {Name(Application.Current?.TryFindResource(type))}");
        }

        var crumb = bar.Items.Count > 0 ? bar.Items[0] as DependencyObject : null;
        Note($"  item[0]={Name(crumb)}");
        if (crumb is Control control)
        {
            Note($"    Style={Name(control.Style)} Template={Name(control.Template)} children={VisualTreeHelper.GetChildrenCount(control)}");
            Walk(control, 1, 6);
            if (control.Template is Jalium.UI.Controls.ControlTemplate template)
            {
                Note($"    template target={Name(template.TargetType)}");
                try
                {
                    var content = template.LoadContent();
                    Note($"    LoadContent => {content.GetType().Name}, children={VisualTreeHelper.GetChildrenCount(content)}");
                    Walk(content, 1, 4);
                }
                catch (Exception exception)
                {
                    Note($"    LoadContent threw {exception.GetType().Name}: {Trim(exception.InnerException?.Message ?? exception.Message)}");
                }
            }
        }
    }

    /// <summary>
    /// The numbers the fit actually saw, because "it did not collapse" and "it collapsed the wrong prefix" look
    /// identical from the outside and different from the row's own readings.
    /// </summary>
    private static void Narrow(Panel root)
    {
        Note("");
        Note("=== F. what the fit sees at 260 ===");
        foreach (var width in new[] { 600d, 260d })
        {
            var bar = new FluentJalium.Controls.FluentBreadcrumbBar { Width = width, Height = 40 };
            for (var index = 0; index < 4; index++)
            {
                var tag = new Border { Width = 100, Height = 24, Background = new SolidColorBrush(Colors.Gray) };
                bar.Items.Add($"Crumb {index}");
            }

            root.Children.Add(new Border { Width = width, Height = 40, Background = new SolidColorBrush(Colors.White), Child = bar });
            Note($"  frames pumped={Pump(20, 1500)}");
            var panel = FindPanel(bar);
            Note($"  width={width} bar.Actual={Round(bar.ActualWidth)} panel={(panel is null ? "<none>" : $"{Round(panel.ActualWidth)} measured={Round(panel.MeasuredWidth)} arranged={Round(panel.ArrangedWidth)} against={Round(panel.ComparedAgainst)} first={panel.FirstVisibleIndex}")}");
            var ellipsis = FindPart(bar, "PART_EllipsisButton");
            Note($"    ellipsis desired={Round(ellipsis?.DesiredSize.Width ?? 0)} actual={Round(ellipsis?.ActualWidth ?? 0)}");
            for (var index = 0; index < 4; index++)
            {
                var crumb = index < 4 ? FindCrumb(bar, index) : null;
                Note($"    crumb[{index}] desired={Round(crumb?.DesiredSize.Width ?? 0)} actual={Round(crumb?.ActualWidth ?? 0)} visible={crumb?.Visibility}");
            }
        }
    }

    private static FluentBreadcrumbPanel? FindPanel(DependencyObject root) => WalkFor<FluentBreadcrumbPanel>(root);

    private static FrameworkElement? FindPart(DependencyObject root, string name)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is FrameworkElement element && element.Name == name)
            {
                return element;
            }

            if (child is null)
            {
                continue;
            }

            if (FindPart(child, name) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    private static FrameworkElement? FindCrumb(DependencyObject root, int order)
    {
        var found = new List<FrameworkElement>();
        CollectCrumbs(root, found);
        return order < found.Count ? found[order] : null;
    }

    private static void CollectCrumbs(DependencyObject root, List<FrameworkElement> sink)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is FluentJalium.Controls.FluentBreadcrumbBarItem item)
            {
                sink.Add(item);
            }

            if (child is not null)
            {
                CollectCrumbs(child, sink);
            }
        }
    }

    private static T? WalkFor<T>(DependencyObject root) where T : DependencyObject
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                return match;
            }

            if (child is not null && WalkFor<T>(child) is { } deeper)
            {
                return deeper;
            }
        }

        return null;
    }

    private static string Name(object? value) => value?.GetType().Name ?? "<null>";

    private static void Walk(Visual node, int depth, int maxDepth)
    {
        if (depth > maxDepth)
        {
            return;
        }

        var count = VisualTreeHelper.GetChildrenCount(node);
        for (var index = 0; index < count; index++)
        {
            if (VisualTreeHelper.GetChild(node, index) is not Visual child)
            {
                continue;
            }

            var element = child as FrameworkElement;
            Note($"  {new string(' ', depth * 2)}{child.GetType().Name} " +
                 $"{(string.IsNullOrEmpty(element?.Name) ? string.Empty : $"part={element!.Name} ")}" +
                 $"{Round(element?.ActualWidth ?? 0)}x{Round(element?.ActualHeight ?? 0)}");
            Walk(child, depth + 1, maxDepth);
        }
    }

    private static void Ink(StackPanel root, bool collapsedInstead)
    {
        Note("");
        Note(collapsedInstead
            ? "=== D. the replacement mechanism: does Visibility=Collapsed hide a sized child ==="
            : "=== C. does a zero-rect child stop painting ===");
        var panel = new CollapsePanel { Width = 250, Height = 40 };
        var colors = new[] { Colors.Red, Colors.Lime, Colors.Blue, Colors.Yellow };
        foreach (var color in colors)
        {
            panel.Children.Add(new Border { Width = 100, Height = 40, Background = new SolidColorBrush(color) });
        }

        panel.UseCollapsedInstead = collapsedInstead;

        root.Children.Add(panel);
        Note($"  frames pumped={Pump(20, 1500)}");
        var order = 0;
        foreach (UIElement child in panel.Children)
        {
            var element = (FrameworkElement)child;
            var at = child.TranslatePoint(new Point(0, 0), panel);
            Note($"    child[{order++}] {element.GetType().Name} actual={Round(element.ActualWidth)}x{Round(element.ActualHeight)} " +
                 $"at=({Round(at.X)},{Round(at.Y)}) brush={(element is Border border ? border.Background : null)?.GetType().Name ?? "-"} " +
                 $"color={((element as Border)?.Background as SolidColorBrush)?.Color.R},{(element as Border)?.Background as SolidColorBrush != null}");
        }

        const int Width = 250;
        const int Height = 40;
        var bitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(panel);
        var buffer = new byte[Width * Height * 4];
        bitmap.CopyPixels(buffer, Width * 4, 0);
        var counts = new Dictionary<uint, int>();
        for (var pixel = 0; pixel < Width * Height; pixel++)
        {
            var offset = pixel * 4;
            var key = (uint)(buffer[offset + 2] << 16 | buffer[offset + 1] << 8 | buffer[offset]);
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }

        foreach (var (name, color) in new[]
                 {
                     ("red", Colors.Red), ("lime", Colors.Lime), ("blue", Colors.Blue), ("yellow", Colors.Yellow),
                 })
        {
            var key = (uint)(color.R << 16 | color.G << 8 | color.B);
            Note($"  {name}: {counts.GetValueOrDefault(key)} pixels");
        }

        Note($"  histogram: {string.Join(" ", counts.OrderByDescending(pair => pair.Value).Select(pair => $"{pair.Key:X6}x{pair.Value}"))}");
    }

    // ---------- shared ----------

    private static DependencyObject? Find(DependencyObject root, Func<DependencyObject, bool> match)
    {
        var queue = new Queue<DependencyObject>();
        queue.Enqueue(root);
        for (var guard = 0; guard < 600 && queue.Count > 0; guard++)
        {
            var node = queue.Dequeue();
            if (match(node))
            {
                return node;
            }

            if (node is Visual visual)
            {
                for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
                {
                    queue.Enqueue((DependencyObject)VisualTreeHelper.GetChild(visual, index));
                }
            }
        }

        return null;
    }

    private static void MembersByName(string typeName, params string[] names)
    {
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => SafeTypes(assembly).FirstOrDefault(candidate => candidate.Name == typeName))
            .FirstOrDefault(candidate => candidate is not null);
        if (type is null)
        {
            Note($"  {typeName}: <type ABSENT>, {names.Length} members uncheckable");
            return;
        }

        Members(type, typeName, names);
    }

    private static void Members(Type type, string label, params string[] names)
    {
        const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                   BindingFlags.Static | BindingFlags.FlattenHierarchy;
        var found = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in type.GetMembers(Flags))
        {
            found.Add(member.Name);
        }

        foreach (var property in type.GetProperties(Flags))
        {
            found.Add(property.Name + "Property");
        }

        foreach (var name in names)
        {
            Note($"  {label}.{name}: {(found.Contains(name) ? "present" : "ABSENT")}");
        }
    }

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

    /// <summary>
    /// Pumps real rendered frames, copied from tests/FluentJalium.Tests/Pixel/PixelHarness.cs:269 - a nested
    /// <see cref="Dispatcher.PushFrame"/> plus a CompositionTarget.Rendering counter and a watchdog that hands the
    /// release back to THIS dispatcher. A bare dispatcher.Invoke or a blocking wait never yields a frame, which is
    /// how the first run of pass B read "measureCalls=0" on a panel that simply had not been measured yet.
    /// </summary>
    private static int Pump(int frames = 10, int budgetMilliseconds = 800)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var seen = 0;
        var deadline = Stopwatch.GetTimestamp() + (Stopwatch.Frequency * budgetMilliseconds / 1000);
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames || Stopwatch.GetTimestamp() > deadline)
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

    private static string Round(double value) => value.ToString("0.##");

    private static void Note(string line)
    {
        Lines.Add(line);
        Console.WriteLine(line);
    }

    private static string Trim(string text) => text.Length > 240 ? text[..240] : text;
}
