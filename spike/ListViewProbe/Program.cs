using System.Reflection;
using System.Text;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace ListViewProbe;

/// <summary>
/// The questions the ListView slice cannot write product code without, after ListBox closed (adaptation/00 S1-c):
///   A types: does the runtime export ListView / ListViewItem / ListViewBase / GridView / GridViewItem, what is
///     each base chain, which DP names does the control and its container actually carry, what are the
///     SelectionMode values, and which WinUI ListView properties (IsItemClickEnabled, ShowSelectionChecks,
///     MultiSelect, grouping, transitions, swipe) exist at all here - that decides how much of the 76-row
///     upstream ListViewItem key list can even have a consumer.
///   B mount: the shipped tree, part names, container type, resting padding and the brush the framework paints
///     for a selected row - the same two numbers that justified the ListBox re-template.
///   C retmpl: can ListView take our template without the UseTemplateContentManagement lock (S0-m), does the
///     item-host contract stay "by type, not by name" (S1-c 2), and is there an ItemsPanel knob for a grid.
///   D itemstyle: does the implicit container style route from S1-c hold for ListViewItem too, or does this
///     control ship an ItemContainerStyle of its own that outranks it.
///   E states: which of the flags the upstream cell matrix conditions on exist on this container
///     (IsSelected, IsPointerOver vs IsMouseOver, selection-active, check-mode, dragging, ...).
///   F gutter: the 12 DIP right gutter (S0-u, S1-c 7) on the list and on the grid surface, and whether the
///     overlay switch equalises both.
///   G grid: whether GridView is a styleable control at all, what panel it lays items out with, and what its
///     container type is - the objective lists GridView next to ListView, so the shape has to be measured.
/// Reflection lives here and only here; the structural gate in AstraGateTests keeps it out of src/FluentJalium.
/// Modes: types | mount | retmpl | itemstyle | states | gutter | grid | all.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];

    private static Application _application = null!;
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
            _application = application;
            FluentThemeManager.Apply(application);
            Run(application);
        }
        catch (Exception exception)
        {
            Note("BOOT threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }

        var path = Path.Combine(AppContext.BaseDirectory, $"listview-probe-{_mode}.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, mode={_mode}, full log: {path}");
        return 0;
    }

    private static void Run(Application application)
    {
        var root = new StackPanel { Margin = new Thickness(24) };
        var window = new Window { Content = root, Width = 780, Height = 600, Title = "ListView probe" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Pump();
                Note($"theme dictionaries={FluentThemeManager.DictionaryNames.Count}");
                Note($"our ListBox styles are live: ListBoxStyle={FluentThemeManager.GetStyle("DefaultListBoxStyle") is not null}");

                if (_mode is "all" or "types")
                {
                    TypeCensus();
                }

                if (_mode is "all" or "mount")
                {
                    Mounted(root, window);
                }

                if (_mode is "all" or "retmpl")
                {
                    Retemplated(root, window);
                }

                if (_mode is "all" or "itemstyle")
                {
                    ImplicitItemStyle(root, window);
                }

                if (_mode is "all" or "states")
                {
                    StateSurface(root, window);
                }

                if (_mode is "all" or "gutter")
                {
                    Gutters(root, window);
                }

                if (_mode is "all" or "grid")
                {
                    GridViewShape(root, window);
                }

                if (_mode is "all" or "cells")
                {
                    ContainerPartContract(root, window);
                }

                if (_mode is "all" or "bar")
                {
                    OverlayBarShape(root, window);
                }

                if (_mode is "all" or "panel")
                {
                    ItemsPanelSwap(root, window);
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

    // ---------- A. what the runtime exports for lists ----------

    private static void TypeCensus()
    {
        Note(string.Empty);
        Note("=== A. exported list types, base chains and property surfaces ===");
        var assembly = typeof(ListView).Assembly;
        var byName = assembly.GetTypes()
            .Where(type => !type.IsNested)
            .GroupBy(type => type.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        foreach (var name in new[]
                 {
                     "ListView", "ListViewBase", "ListViewItem", "GridView", "GridViewItem", "SemanticZoom",
                     "ListViewItemPresenter", "GroupStyle", "ItemsRepeater", "ItemsView", "WrapPanel",
                     "VirtualizingStackPanel", "Selector", "ListBox", "TreeView", "TreeViewItem"
                 })
        {
            if (!byName.TryGetValue(name, out var type))
            {
                Note($"  {name}: NOT EXPORTED by {assembly.GetName().Name}");
                continue;
            }

            Note($"  {name}: {type.FullName} abstract={type.IsAbstract} base={Chain(type)}");
        }

        Note("  ListView own DPs: " + string.Join(", ", DeclaredDpNames(typeof(ListView))));
        Note("  ListViewItem own DPs: " + string.Join(", ", DeclaredDpNames(typeof(ListViewItem))));
        Note("  ListView public properties: " + Join(Properties(typeof(ListView))));
        Note("  ListViewItem public properties: " + Join(Properties(typeof(ListViewItem))));

        Note(string.Empty);
        Note("  WinUI ListView/GridView members this runtime does or does not carry:");
        foreach (var name in new[]
                 {
                     "IsItemClickEnabled", "ItemClick", "ShowSelectionChecks", "MultiSelect", "IsMultiSelectButtonEnabled",
                     "IsSwipeEnabled", "IsItemSwipeEnabled", "ItemContainerTransitions", "GroupStyleSource",
                     "IsGroupItem", "Group", "ContainerContentChanging", "ChoosingItemContainer", "RecyclingRatio",
                     "IsVirtualizing", "VirtualizationMode", "IsItemClickEnabled", "SelectionMode", "SelectionDisabledvis",
                     "ShowsScrollingPlaceholders", "IsFocusEngagementEnabled", "IsTabStop", "CornerRadius", "Padding",
                     "MaxSelectedItemCount", "ItemHeight", "ItemWidth", "IsItemClick", "ContinuumNavigationTransitionInfo"
                 }.Distinct(StringComparer.Ordinal))
        {
            var property = typeof(ListView).GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            var dp = FindDp(typeof(ListView), name);
            Note($"    {name}: property={(property is null ? "-" : property.PropertyType.Name)}" +
                 $" dp={(dp is null ? "-" : dp.Name)}" +
                 $" event={(typeof(ListView).GetEvent(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy) is null ? "-" : "yes")}");
        }
    }

    private static string Chain(Type type)
    {
        var names = new List<string>();
        for (var current = type; current is not null && names.Count < 6; current = current.BaseType)
        {
            names.Add(current.Name);
        }

        return string.Join(" : ", names);
    }

    // ---------- B. the shipped ListView tree ----------

    private static void Mounted(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== B. a mounted ListView: tree, parts, container reads, the stock selected brush ===");
        var list = new ListView { Width = 300, Height = 220 };
        AddItems(list, 5);
        root.Children.Add(list);
        window.UpdateLayout();
        Pump(10);

        Note($"  base={Chain(typeof(ListView))}");
        Note($"  Style={(list.Style is null ? "null" : list.Style.GetType().Name)} Template={Read(() => list.Template?.GetType().Name)}");
        Note($"  own theme-style key: " + Read(() =>
        {
            var style = FluentThemeManager.GetStyle("DefaultListViewStyle");
            return style is null ? "no DefaultListViewStyle published" : style.GetType().Name;
        }));
        PrintTree(list, "    ");

        var container = FirstOfType(list, typeof(ListViewItem)) as FrameworkElement;
        Note("  container: " + (container is null
            ? "NONE GENERATED"
            : $"{container.GetType().Name} style={(container.Style is null ? "null" : "set")}" +
              $" size={container.ActualWidth:0.##}x{container.ActualHeight:0.##}" +
              $" padding={Read(() => Prop(container, "Padding"))}" +
              $" background={Read(() => Prop(container, "Background"))}" +
              $" min-height={Read(() => Prop(container, "MinHeight"))}"));
        if (container is not null)
        {
            Note("  container template parts: " + PartNames(container));
            Note("  content host: " + Read(() => FirstOfType(container, typeof(ContentPresenter))?.GetType().Name ?? "no ContentPresenter"));
        }

        Note("  selected row reads: " + Read(() =>
        {
            Note2(Set(list, "SelectedIndex", 0));
            window.UpdateLayout();
            Pump(6);
            var row = (FrameworkElement?)FirstOfType(list, typeof(ListViewItem));
            if (row is null)
            {
                return "no container after selecting";
            }

            var background = new StringBuilder();
            Walk(row, node =>
            {
                var brush = node.GetType().GetProperty("Background")?.GetValue(node) as Brush;
                if (brush is not null)
                {
                    background.Append($"{node.GetType().Name}.{NameOf(node)}={Hex(brush)}; ");
                }
            }, 24);
            return $"SelectedIndex={Show(Prop(list, "SelectedIndex"))} parts=[{Trim(background.ToString())}]";
        }));
        Note("  accent in effect: " + Read(() => Hex(_application.TryFindResource("AccentFillColorDefaultBrush") as Brush)));
    }

    // ---------- C. can ListView take our template ----------

    private static void Retemplated(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== C. our template on a ListView: lock, item-host contract, panel knob ===");
        var list = new ListView { Width = 300, Height = 220 };
        AddItems(list, 4);
        root.Children.Add(list);
        window.UpdateLayout();
        Pump(6);

        var plain = ParseTemplate("ours", @"
            <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
              <Border Name='LayoutRoot' Background='Transparent' BorderThickness='0' CornerRadius='4'>
                <ScrollViewer Name='ScrollViewer' Focusable='False' HorizontalScrollBarVisibility='Disabled'
                              VerticalScrollBarVisibility='Auto' IsOverlayScrollBarEnabled='True'>
                  <ItemsPresenter Name='ItemsPresenter'/>
                </ScrollViewer>
              </Border>
            </ControlTemplate>");
        if (plain is not null)
        {
            Note("  assign ours: " + Try(() => list.Template = plain));
            window.UpdateLayout();
            Pump(8);
            Note($"  after assign: containers={CountContainers(list, typeof(ListViewItem))}" +
                 $" layoutRoot={CountNamed(list, "LayoutRoot")} scrollViewer={CountNamed(list, "ScrollViewer")}" +
                 $" itemsPresenter={CountNamed(list, "ItemsPresenter")}");
            Note("  row size now: " + Read(() =>
            {
                var row = (FrameworkElement?)FirstOfType(list, typeof(ListViewItem));
                return row is null ? "none" : $"{row.ActualWidth:0.##}x{row.ActualHeight:0.##}";
            }));
        }

        var renamed = ParseTemplate("renamed-host", @"
            <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
              <Border Name='LayoutRoot'>
                <ScrollViewer Name='ScrollViewer' IsOverlayScrollBarEnabled='True'>
                  <ItemsPresenter Name='Whatever'/>
                </ScrollViewer>
              </Border>
            </ControlTemplate>");
        if (renamed is not null)
        {
            Note("  assign renamed ItemsPresenter: " + Try(() => list.Template = renamed));
            window.UpdateLayout();
            Pump(8);
            Note($"  containers with the host renamed: {CountContainers(list, typeof(ListViewItem))}");
        }

        var noHost = ParseTemplate("no-host", @"
            <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
              <Border Name='LayoutRoot'>
                <StackPanel Name='Plain'/>
              </Border>
            </ControlTemplate>");
        if (noHost is not null)
        {
            Note("  assign a template with no item host: " + Try(() => list.Template = noHost));
            window.UpdateLayout();
            Pump(8);
            Note($"  containers without an ItemsPresenter: {CountContainers(list, typeof(ListViewItem))}");
        }

        Note("  ItemsPanel knob: " + Read(() =>
        {
            var property = typeof(ListView).GetProperty("ItemsPanel", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (property is null)
            {
                return "ListView exposes no ItemsPanel property";
            }

            return $"ItemsPanel present, type={property.PropertyType.Name}, current={Show(property.GetValue(list)?.GetType().Name)}";
        }));
        object? panelTemplate = null;
        try
        {
            panelTemplate = XamlReader.Parse("<ItemsPanelTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'><StackPanel/></ItemsPanelTemplate>");
        }
        catch (Exception exception)
        {
            Note("  ItemsPanelTemplate parse THREW " + exception.GetType().Name + ": " + Trim(exception.Message));
        }

        Note("  assign a StackPanel ItemsPanelTemplate (mode J does the WrapPanel case): " + (panelTemplate is null
            ? "parse failed"
            : Read(() =>
            {
                var property = typeof(ListView).GetProperty("ItemsPanel", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                property?.SetValue(list, panelTemplate);
                window.UpdateLayout();
                Pump(8);
                return "ok, containers=" + CountContainers(list, typeof(ListViewItem)) +
                       " panelInTree=" + (FirstOfType(list, TypeByName("StackPanel"))?.GetType().Name ?? "none");
            })));
    }

    // ---------- D. does the implicit container style route hold ----------

    private static void ImplicitItemStyle(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== D. an application-level implicit ListViewItem style on generated containers ===");
        var list = new ListView { Width = 300, Height = 220 };
        AddItems(list, 3);
        root.Children.Add(list);
        window.UpdateLayout();
        Pump(8);

        var before = Read(() => Prop((object?)FirstOfType(list, typeof(ListViewItem))!, "Padding"));
        Note("  resting padding before: " + before);

        var dictionary = ParseDictionary(@"
            <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
              <Style TargetType='ListViewItem'>
                <Setter Property='Padding' Value='12,9,12,12'/>
                <Setter Property='Background' Value='#FF00FF'/>
              </Style>
            </ResourceDictionary>");
        if (dictionary is null)
        {
            Note("  the implicit item dictionary did not parse; nothing to conclude");
            return;
        }

        var added = Try(() => _application.Resources.MergedDictionaries.Add(dictionary));
        Note("  merge into application resources: " + added);
        window.UpdateLayout();
        Pump(8);
        var row = (FrameworkElement?)FirstOfType(list, typeof(ListViewItem));
        Note("  container after merge: " + (row is null
            ? "no container"
            : $"Style={(row.Style is null ? "null" : "set")} padding={Read(() => Prop(row, "Padding"))}" +
              $" background={Read(() => Prop(row, "Background"))}"));

        Note("  ItemContainerStyle on the list: " + Read(() =>
        {
            var property = typeof(ListView).GetProperty("ItemContainerStyle", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            return property is null ? "no such property" : Show(property.GetValue(list));
        }));

        Try(() => _application.Resources.MergedDictionaries.Remove(dictionary));
        window.UpdateLayout();
        Pump(6);
        Note("  container after removal: " + (row is null
            ? "no container"
            : $"padding={Read(() => Prop(row, "Padding"))} background={Read(() => Prop(row, "Background"))}"));
    }

    // ---------- E. the state flags a cell can condition on ----------

    private static void StateSurface(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== E. which upstream ListViewItem state signals exist on this container ===");
        var list = new ListView { Width = 300, Height = 200 };
        AddItems(list, 3);
        root.Children.Add(list);
        window.UpdateLayout();
        Pump(8);
        var row = FirstOfType(list, typeof(ListViewItem));
        if (row is null)
        {
            Note("  no container to inspect");
            return;
        }

        foreach (var name in new[]
                 {
                     "IsSelected", "IsMouseOver", "IsPointerOver", "IsPressed", "IsEnabled", "IsFocused",
                     "IsSelectionActive", "IsHighlighted", "CheckMode", "IsMultiSelectCheckBoxVisible",
                     "ShowsCheckHint", "IsDragging", "IsReorderHint", " isSelected", "IsHalfSelected",
                     "IsDragSource", "IsDropTarget", "IsGhosting", "Placement", "SemanticState", "IsExpanded"
                 })
        {
            var dp = FindDp(row.GetType(), name) is not null;
            var property = row.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (dp || property is not null)
            {
                Note($"  {name}: dp={dp} clr={property?.PropertyType.Name ?? "-"} read=" +
                     Read(() => Show(property?.GetValue(row))));
            }
        }

        Note("  missing from the above list: " + string.Join(", ", new[]
        {
            "IsPointerOver", "IsSelectionActive", "IsHighlighted", "CheckMode", "IsMultiSelectCheckBoxVisible",
            "ShowsCheckHint", "IsDragging", "IsReorderHint", "IsDragSource", "IsDropTarget", "IsGhosting",
            "SemanticState", "IsExpanded"
        }.Where(name => FindDp(row.GetType(), name) is null &&
                        row.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy) is null)));

        Note("  writable from a trigger? IsSelected set: " + Read(() =>
        {
            Note2(Set(row, "IsSelected", true));
            window.UpdateLayout();
            Pump(4);
            return $"container.IsSelected={Show(Prop(row, "IsSelected"))} list.SelectedIndex={Show(Prop(list, "SelectedIndex"))}";
        }));
        Note("  SelectionMode values on the list: " + Read(() =>
        {
            var property = typeof(ListView).GetProperty("SelectionMode", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            return property is null
                ? "no SelectionMode"
                : string.Join("/", Enum.GetNames(property.PropertyType));
        }));
        foreach (var value in EnumStrings(typeof(ListView), "SelectionMode"))
        {
            Note($"  set SelectionMode={value}: " + Read(() =>
            {
                var property = typeof(ListView).GetProperty("SelectionMode", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)!;
                property.SetValue(list, Enum.Parse(property.PropertyType, value));
                SetOrThrow(list, "SelectedIndex", 1);
                window.UpdateLayout();
                Pump(4);
                return $"selected-index={Show(Prop(list, "SelectedIndex"))} selected-items={Show(Prop(list, "SelectedItems"))}";
            }));
        }
    }

    // ---------- F. the gutter on both surfaces ----------

    private static void Gutters(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== F. the 12 DIP gutter on a stock list and on our own template ===");
        foreach (var (label, count) in new[] { ("5 items", 5), ("50 items", 50) })
        {
            var stock = new ListView { Width = 300, Height = 220 };
            AddItems(stock, count);
            root.Children.Add(stock);
            window.UpdateLayout();
            Pump(10);
            Note($"  stock ListView, {label}: " + Ledger(stock, typeof(ListViewItem), window));

            var ours = new ListView { Width = 300, Height = 220, Template = OverlayTemplate("True") };
            AddItems(ours, count);
            root.Children.Add(ours);
            window.UpdateLayout();
            Pump(10);
            Note($"  our template + overlay, {label}: " + Ledger(ours, typeof(ListViewItem), window));

            var noverlay = new ListView { Width = 300, Height = 220, Template = OverlayTemplate("False") };
            AddItems(noverlay, count);
            root.Children.Add(noverlay);
            window.UpdateLayout();
            Pump(10);
            Note($"  our template, overlay off, {label}: " + Ledger(noverlay, typeof(ListViewItem), window));

            root.Children.Clear();
            window.UpdateLayout();
            Pump(4);
        }
    }

    private static ControlTemplate? OverlayTemplate(string overlay) => ParseTemplate($"overlay={overlay}", $@"
        <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
          <Border Name='LayoutRoot' Background='Transparent' BorderThickness='0'>
            <ScrollViewer Name='ScrollViewer' Focusable='False' HorizontalScrollBarVisibility='Disabled'
                          VerticalScrollBarVisibility='Auto' IsOverlayScrollBarEnabled='{overlay}'>
              <ItemsPresenter Name='ItemsPresenter'/>
            </ScrollViewer>
          </Border>
        </ControlTemplate>");

    private static string Ledger(DependencyObject root, Type containerType, Window window)
    {
        var first = (FrameworkElement?)FirstOfType(root, containerType);
        if (first is null)
        {
            return "no container to measure";
        }

        var host = (FrameworkElement)root;
        var at = first.TranslatePoint(new Point(), host);
        var bars = BarsIn(host);
        return $"host={host.ActualWidth:0.##}x{host.ActualHeight:0.##}" +
               $" item={first.ActualWidth:0.##}x{first.ActualHeight:0.##}" +
               $" left={at.X:0.##} right={host.ActualWidth - at.X - first.ActualWidth:0.##}" +
               $" bars=[" + string.Join(" ", bars.Select(bar =>
                   $"{bar.GetType().Name} {bar.ActualWidth:0.##}x{bar.ActualHeight:0.##} vis={bar.Visibility}")) + "]";
    }

    // ---------- G. is there a GridView to style ----------

    private static void GridViewShape(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== G. GridView / GridViewItem: is either a styleable control here ===");
        var gridType = TypeByName("GridView");
        var itemType = TypeByName("GridViewItem");
        Note("  GridView type: " + (gridType is null ? "NOT EXPORTED" : $"{gridType.FullName} base={Chain(gridType)}"));
        Note("  GridViewItem type: " + (itemType is null ? "NOT EXPORTED" : $"{itemType.FullName} base={Chain(itemType)}"));
        if (gridType is null)
        {
            return;
        }

        Note($"  is Control={typeof(Control).IsAssignableFrom(gridType)} is ItemsControl={typeof(ItemsControl).IsAssignableFrom(gridType)}" +
             $" is ListViewBase={typeof(ListView).BaseType.Name} assignable={AssignableToNamed(gridType, "ListViewBase")}");
        Note("  GridView own DPs: " + string.Join(", ", DeclaredDpNames(gridType)));

        object? grid = null;
        Note("  construct: " + Try(() =>
        {
            grid = Activator.CreateInstance(gridType)!;
            ((FrameworkElement)grid).Width = 320;
            ((FrameworkElement)grid).Height = 220;
            var items = gridType.GetProperty("Items", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)?.GetValue(grid);
            var collection = items as System.Collections.IList;
            for (var index = 0; index < 4; index++)
            {
                collection?.Add($"Cell {index + 1}");
            }

            root.Children.Add((UIElement)grid);
            window.UpdateLayout();
            Pump(12);
        }));
        if (grid is null)
        {
            return;
        }

        Note("  Style/Template: " + Read(() =>
        {
            var style = gridType.GetProperty("Style", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)?.GetValue(grid);
            var template = gridType.GetProperty("Template", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)?.GetValue(grid);
            return $"Style={Show(style)} Template={Show(template?.GetType().Name)}";
        }));
        PrintTree((DependencyObject)grid, "    ");
        Note("  containers: " + Read(() =>
        {
            var count = itemType is null ? -1 : CountContainers((DependencyObject)grid, itemType);
            var generic = CountContainers((DependencyObject)grid, typeof(ListViewItem));
            return itemType is null
                ? $"no GridViewItem type; ListViewItem-in-tree={generic}"
                : $"GridViewItem={count} ListViewItem={generic}";
        }));
        Note("  item panel in tree: " + Read(() =>
        {
            var names = new List<string>();
            Walk((DependencyObject)grid, node =>
            {
                if (node is Panel)
                {
                    names.Add(node.GetType().Name);
                }
            }, 30);
            return string.Join(", ", names.Distinct());
        }));
        Note("  gutter: " + Ledger((DependencyObject)grid, itemType ?? typeof(ListViewItem), window));
    }

    // ---------- H. is the container's part name a functional contract ----------

    private static void ContainerPartContract(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== H. ListViewItem template parts: what has to keep its name for content and selection to work ===");
        // The stock container paints selection into a Border named PART_BackgroundBorder and lays its content
        // inside a StackPanel named PART_CellsPanel. Neither is a WinUI name. Three variants of a template with
        // no state trigger of its own say which of the two the framework itself depends on.
        foreach (var (label, markup) in new[]
                 {
                     ("parts kept, no trigger", @"
                        <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
                          <Grid Background='Transparent'>
                            <Border Name='PART_BackgroundBorder' Background='Transparent'>
                              <StackPanel Name='PART_CellsPanel'>
                                <ContentPresenter Name='ContentPresenter'/>
                              </StackPanel>
                            </Border>
                          </Grid>
                        </ControlTemplate>"),
                     ("renamed", @"
                        <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
                          <Grid Background='Transparent'>
                            <Border Name='OtherBorder' Background='Transparent'>
                              <StackPanel Name='OtherPanel'>
                                <ContentPresenter Name='ContentPresenter'/>
                              </StackPanel>
                            </Border>
                          </Grid>
                        </ControlTemplate>"),
                     ("no cells panel", @"
                        <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
                          <Grid Background='Transparent'>
                            <Border Name='PART_BackgroundBorder' Background='Transparent'>
                              <ContentPresenter Name='ContentPresenter'/>
                            </Border>
                          </Grid>
                        </ControlTemplate>"),
                     ("presenter only", @"
                        <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
                          <Grid Background='Transparent'>
                            <ContentPresenter Name='ContentPresenter'/>
                          </Grid>
                        </ControlTemplate>"),
                 })
        {
            var template = ParseTemplate(label, markup);
            if (template is null)
            {
                continue;
            }

            var style = new Style(typeof(ListViewItem));
            style.Setters.Add(new Setter(Control.TemplateProperty, template));
            var dictionary = new ResourceDictionary();
            dictionary.Add(typeof(ListViewItem), style);
            Note($"  --- variant: {label}");
            Note("    merge: " + Try(() => _application.Resources.MergedDictionaries.Add(dictionary)));

            var list = new ListView { Width = 300, Height = 160 };
            AddItems(list, 3);
            root.Children.Add(list);
            window.UpdateLayout();
            Pump(8);
            Note("    select row 0: " + Set(list, "SelectedIndex", 0));
            window.UpdateLayout();
            Pump(6);

            var row = (FrameworkElement?)FirstOfType(list, typeof(ListViewItem));
            Note("    container IsSelected after the list selects it: " + Read(() =>
                row is null ? "no container" : Show(Prop(row, "IsSelected"))));
            Note("    container: " + (row is null
                ? "NONE GENERATED"
                : $"{row.ActualWidth:0.##}x{row.ActualHeight:0.##} text=" + Read(() =>
                {
                    var presenter = FirstOfType(row, typeof(ContentPresenter));
                    return presenter is null
                        ? "no ContentPresenter"
                        : Show(Prop(presenter, "Content")) + "/" + Show(Prop(presenter, "Text"));
                })));
            Note("    brushes after select: " + Read(() =>
            {
                if (row is null)
                {
                    return "no container";
                }

                var builder = new StringBuilder();
                Walk(row, node =>
                {
                    var brush = node.GetType().GetProperty("Background")?.GetValue(node) as Brush;
                    if (brush is not null)
                    {
                        builder.Append($"{node.GetType().Name}.{NameOf(node)}={Hex(brush)}; ");
                    }
                }, 20);
                return Trim(builder.ToString());
            }));
            root.Children.Clear();
            Note("    unmerge: " + Try(() => _application.Resources.MergedDictionaries.Remove(dictionary)));
            window.UpdateLayout();
            Pump(4);
        }
    }

    // ---------- I. how wide is the overlay bar that floats over the rows ----------

    private static void OverlayBarShape(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== I. the overlay scrollbar geometry on an overflowing list ===");
        Note("  (the F ledger says the rows keep the full 300 DIP with the overlay switch on, yet a bar reports" +
             " 40 DIP wide - this says what that element actually is.)");

        foreach (var (label, control, container) in new[]
                 {
                     ("ListView, our template + overlay", (Control)new ListView(), typeof(ListViewItem)),
                     ("shipped ListBox style", (Control)new ListBox(), typeof(ListBoxItem))
                 })
        {
            var element = (FrameworkElement)control;
            element.Width = 300;
            element.Height = 200;
            if (control is ListView listView)
            {
                listView.Template = OverlayTemplate("True");
                for (var index = 0; index < 50; index++)
                {
                    listView.Items.Add($"Row {index + 1}");
                }
            }
            else
            {
                for (var index = 0; index < 50; index++)
                {
                    ((ListBox)control).Items.Add($"Row {index + 1}");
                }
            }

            root.Children.Add(element);
            window.UpdateLayout();
            Pump(12);
            Note($"  {label}: host={element.ActualWidth:0.##}x{element.ActualHeight:0.##}");
            foreach (var bar in BarsIn(element))
            {
                Note($"    {bar.GetType().Name} '{NameOf(bar)}' {bar.ActualWidth:0.##}x{bar.ActualHeight:0.##}" +
                     $" vis={bar.Visibility} opacity={Read(() => Show(Prop(bar, "Opacity")))}" +
                     $" width={Read(() => Show(Prop(bar, "Width")))} margin={Read(() => Show(Prop(bar, "Margin")))}" +
                     $" hit={Read(() => Show(Prop(bar, "IsHitTestVisible")))}");
                // 40 DIP on the bar element says nothing about what paints. The subtree says which part is the
                // hit area and which one carries the fill.
                var subtree = new StringBuilder();
                Walk(bar, node =>
                {
                    if (node is not FrameworkElement part || ReferenceEquals(part, bar))
                    {
                        return;
                    }

                    var brush = node.GetType().GetProperty("Background")?.GetValue(node) as Brush
                                ?? node.GetType().GetProperty("Fill")?.GetValue(node) as Brush;
                    subtree.Append($"\n      {new string(' ', Depth(bar, node) * 2)}{node.GetType().Name}" +
                                   $" '{NameOf(node)}' {part.ActualWidth:0.##}x{part.ActualHeight:0.##}" +
                                   $" vis={part.Visibility} op={Read(() => Show(Prop(part, "Opacity")))}" +
                                   $" brush={(brush is null ? "-" : Hex(brush))}");
                }, 10);
                Note(subtree.ToString());
            }

            var first = (FrameworkElement?)FirstOfType(element, container);
            Note($"    first row {first?.ActualWidth:0.##}x{first?.ActualHeight:0.##}");
            root.Children.Clear();
            window.UpdateLayout();
            Pump(4);
        }
    }

    // ---------- J. ItemsPanel and the WPF column view ----------

    private static void ItemsPanelSwap(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== J. an ItemsPanel swap on a ListView, and what ListView.View is ===");
        var list = new ListView { Width = 300, Height = 200 };
        AddItems(list, 6);
        root.Children.Add(list);
        window.UpdateLayout();
        Pump(8);
        Note("  panel before: " + PanelNames(list));

        object? wrap = null;
        Note("  parse an ItemsPanelTemplate with a WrapPanel: " + Read(() =>
        {
            wrap = XamlReader.Parse(
                "<ItemsPanelTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'><WrapPanel/></ItemsPanelTemplate>");
            return wrap?.GetType().Name ?? "null";
        }));
        Note("  assign ItemsPanel: " + Try(() =>
        {
            typeof(ListView).GetProperty("ItemsPanel", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)!
                .SetValue(list, wrap);
            window.UpdateLayout();
            Pump(10);
        }));
        Note("  panel after: " + PanelNames(list));
        Note("  rows after: " + Read(() =>
        {
            var builder = new StringBuilder();
            var index = 0;
            Walk(list, node =>
            {
                if (node is ListViewItem row && index < 6)
                {
                    var at = row.TranslatePoint(new Point(), list);
                    builder.Append($"[{index} {row.ActualWidth:0.##}x{row.ActualHeight:0.##}@{at.X:0.##},{at.Y:0.##}] ");
                    index++;
                }
            }, 40);
            return Trim(builder.ToString());
        }));

        Note("  ListView.View: " + Read(() =>
        {
            var property = typeof(ListView).GetProperty("View", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            return property is null
                ? "no View property"
                : $"type={property.PropertyType.Name} current={Show(property.GetValue(list))}";
        }));
        Note("  stock header parts on a fresh list: " + Read(() =>
        {
            var plain = new ListView { Width = 300, Height = 160 };
            AddItems(plain, 2);
            root.Children.Add(plain);
            window.UpdateLayout();
            Pump(8);
            var builder = new StringBuilder();
            Walk(plain, node =>
            {
                var name = NameOf(node);
                if (name is not null && name.Contains("ColumnHeader", StringComparison.Ordinal))
                {
                    builder.Append($"{node.GetType().Name} '{name}' size=" +
                                   $"{((FrameworkElement)node).ActualWidth:0.##}x{((FrameworkElement)node).ActualHeight:0.##}; ");
                }
            }, 30);
            root.Children.Clear();
            return Trim(builder.ToString());
        }));
    }

    private static string PanelNames(DependencyObject root)
    {
        var names = new List<string>();
        Walk(root, node =>
        {
            if (node is Panel panel)
            {
                names.Add($"{panel.GetType().Name} '{NameOf(panel)}' {panel.ActualWidth:0.##}x{panel.ActualHeight:0.##}");
            }
        }, 30);
        return names.Count == 0 ? "no panel" : string.Join(" | ", names);
    }

    // ---------- markup and reflection helpers ----------

    private static ControlTemplate? ParseTemplate(string label, string markup)
    {
        try
        {
            return (ControlTemplate)XamlReader.Parse(markup)!;
        }
        catch (Exception exception)
        {
            Note($"  {label}: parse THREW {exception.GetType().Name}: {Trim(exception.Message)}");
            return null;
        }
    }

    private static ResourceDictionary? ParseDictionary(string markup)
    {
        try
        {
            return (ResourceDictionary)XamlReader.Parse(markup)!;
        }
        catch (Exception exception)
        {
            Note($"  dictionary parse THREW {exception.GetType().Name}: {Trim(exception.Message)}");
            return null;
        }
    }

    private static Type? TypeByName(string name) => typeof(ListView).Assembly.GetTypes()
        .FirstOrDefault(type => !type.IsNested && type.Name == name);

    private static bool AssignableToNamed(Type type, string baseName)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.Name == baseName)
            {
                return true;
            }
        }

        return false;
    }

    private static List<string> EnumStrings(Type owner, string propertyName)
    {
        var property = owner.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        return property?.PropertyType.IsEnum == true
            ? Enum.GetNames(property.PropertyType).ToList()
            : [];
    }

    private static DependencyProperty? FindDp(Type type, string name)
    {
        try
        {
            var field = type.GetField(name + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return field?.GetValue(null) as DependencyProperty;
        }
        catch
        {
            return null;
        }
    }

    private static IEnumerable<string> DeclaredDpNames(Type type)
    {
        var names = new List<string>();
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            if (field.Name.EndsWith("Property", StringComparison.Ordinal) &&
                field.GetValue(null) is DependencyProperty dp)
            {
                names.Add(dp.Name);
            }
        }

        names.Sort(StringComparer.Ordinal);
        return names;
    }

    private static IEnumerable<string> Properties(Type type) => type
        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Select(property => property.Name)
        .Order(StringComparer.Ordinal);

    private static string Join(IEnumerable<string> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? "none declared" : $"{list.Count}: " + string.Join(", ", list);
    }

    private static object? Prop(object target, string name) =>
        target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)?.GetValue(target);

    /// <summary>Write through the CLR property so a member this runtime does not have costs a log line, not a build.</summary>
    private static string Set(object target, string name, object value)
    {
        var property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        if (property is null)
        {
            return $"no {name} property on {target.GetType().Name}";
        }

        return Read(() =>
        {
            property.SetValue(target, value);
            return $"set {name}={value} -> {Show(property.GetValue(target))}";
        });
    }

    private static void SetOrThrow(object target, string name, object value)
    {
        var property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                       ?? throw new InvalidOperationException($"no {name} property on {target.GetType().Name}");
        property.SetValue(target, value);
    }

    private static void Note2(string line) => Note("    " + Trim(line));

    private static string PartNames(DependencyObject root)
    {
        var names = new List<string>();
        Walk(root, node =>
        {
            var name = NameOf(node);
            if (!string.IsNullOrEmpty(name))
            {
                names.Add($"{node.GetType().Name}:{name}");
            }
        }, 24);
        return names.Count == 0 ? "no named part" : string.Join(", ", names);
    }

    private static string Hex(Brush? brush)
    {
        if (brush is null)
        {
            return "null";
        }

        try
        {
            var color = (Color?)Prop(brush, "Color");
            return color is null ? Trim(brush.ToString() ?? "?") : $"#{color.Value.A:X2}{color.Value.R:X2}{color.Value.G:X2}{color.Value.B:X2}";
        }
        catch
        {
            return Trim(brush.ToString() ?? "?");
        }
    }

    private static void AddItems(ListView list, int count)
    {
        for (var index = 0; index < count; index++)
        {
            list.Items.Add($"Item {index + 1}");
        }
    }

    private static int CountContainers(DependencyObject root, Type containerType)
    {
        var count = 0;
        Walk(root, node =>
        {
            if (containerType.IsInstanceOfType(node))
            {
                count++;
            }
        }, 40);
        return count;
    }

    private static int CountNamed(DependencyObject root, string name)
    {
        var count = 0;
        Walk(root, node =>
        {
            if (NameOf(node) == name)
            {
                count++;
            }
        }, 40);
        return count;
    }

    private static object? FirstOfType(DependencyObject root, Type type)
    {
        object? found = null;
        Walk(root, node =>
        {
            if (found is null && type.IsInstanceOfType(node))
            {
                found = node;
            }
        }, 40);
        return found;
    }

    private static List<FrameworkElement> BarsIn(DependencyObject root)
    {
        var bars = new List<FrameworkElement>();
        Walk(root, node =>
        {
            if (node.GetType().Name.Contains("ScrollBar", StringComparison.Ordinal) && node is FrameworkElement element)
            {
                bars.Add(element);
            }
        }, 40);
        return bars;
    }

    private static void PrintTree(DependencyObject root, string indent)
    {
        var builder = new StringBuilder();
        Walk(root, node =>
        {
            var depth = Depth(root, node);
            var name = NameOf(node);
            var size = node is FrameworkElement element
                ? $" {element.ActualWidth:0.##}x{element.ActualHeight:0.##}"
                : string.Empty;
            builder.Append($"\n{indent}{new string(' ', depth * 2)}{node.GetType().Name}" +
                           (string.IsNullOrEmpty(name) ? string.Empty : $" '{name}'") + size);
        }, 40);
        Note(builder.ToString());
    }

    private static int Depth(DependencyObject root, DependencyObject node)
    {
        var depth = 0;
        DependencyObject? current = node;
        while (current is not null && !ReferenceEquals(current, root) && depth < 40)
        {
            depth++;
            current = VisualTreeHelper.GetParent(current) ?? LogicalParent(current);
        }

        return depth;
    }

    private static DependencyObject? LogicalParent(DependencyObject node) =>
        node.GetType().GetProperty("Parent")?.GetValue(node) as DependencyObject;

    private static string? NameOf(DependencyObject node)
    {
        try
        {
            return node.GetType().GetProperty("Name")?.GetValue(node) as string;
        }
        catch
        {
            return null;
        }
    }

    private static void Walk(DependencyObject node, Action<DependencyObject> visit, int maxDepth)
    {
        visit(node);
        if (maxDepth <= 0)
        {
            return;
        }

        var count = 0;
        try
        {
            count = VisualTreeHelper.GetChildrenCount(node);
        }
        catch
        {
            return;
        }

        for (var index = 0; index < count; index++)
        {
            try
            {
                Walk(VisualTreeHelper.GetChild(node, index), visit, maxDepth - 1);
            }
            catch
            {
                // A child that cannot be walked is not worth the probe; the parent line already says it exists.
            }
        }
    }

    private static string Show(object? value) => value is null ? "null" : Trim(value.ToString() ?? "?");

    private static string Read(Func<object?> reader)
    {
        try
        {
            return reader()?.ToString() ?? "null";
        }
        catch (Exception exception)
        {
            return "threw " + (exception.InnerException ?? exception).GetType().Name + ": " +
                   Trim((exception.InnerException ?? exception).Message);
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
