using System.Reflection;
using System.Text;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace TreeViewProbe;

/// <summary>
/// The questions the TreeView slice cannot write product code without (stage 5, third slice). WinUI's TreeView is
/// NOT a nested control: its template is one <c>TreeViewList</c> (a ListViewBase) and its item style is
/// <c>BasedOn DefaultListViewItemStyle</c> with the indentation coming from a TemplateSettings binding
/// (controls/dev/TreeView/TreeViewItem.xaml:131,15-27). This runtime declares
/// <c>TreeView : ItemsControl</c> and <c>TreeViewItem : HeaderedItemsControl</c> per the ListView probe's type
/// census - the WPF nested shape - so every load-bearing claim for the slice has to be measured here first:
///   A types: which tree types are exported (TreeViewList, TreeViewNode, TreeViewItemPresenter,
///     TreeViewItemTemplateSettings, HeaderedItemsControl, HierarchicalDataTemplate), the base chains and own DPs,
///     and which WinUI TreeView/TreeViewItem members exist at all - that decides how much of the 39-row upstream
///     key list can even have a consumer.
///   B mount: the shipped tree with real nesting - do child containers appear, what does collapse do to them,
///     which named parts exist, what padding/brush the framework gives a row and a selected row.
///   C retmpl: can TreeView take our host template (no UseTemplateContentManagement lock - S0-m), is the item host
///     still a contract by TYPE (S1-c 2, S1-d 2), and does it need a ScrollViewer at all.
///   D itemstyle: does the implicit container style route (S1-c 1) reach BOTH nesting levels, and does an
///     ItemContainerStyle on the tree outrank it the way it does on a list.
///   E states: which flags a cell can condition on (IsExpanded, IsSelected, HasItems, IsMouseOver,
///     IsMouseCaptureWithin, ...) and what writing each one actually does.
///   F variants: the expand/collapse route. Five candidate item templates measured one at a time: does a
///     ToggleButton named "Expander" drive IsExpanded by framework hook, does a two-way TemplatedParent binding
///     parse and carry both directions, is a named ItemsHost required, does the nested host have to be gated by
///     an IsExpanded trigger, and does content still render when the stock parts are gone.
///   G indent / H gutter: where the per-level indent comes from on the nested shape, and whether the 12 DIP
///     right gutter (S0-u, S1-c 7, S1-d 5) is on this host too.
/// Reflection lives here and only here; the structural gate in AstraGateTests keeps it out of src/FluentJalium.
/// Modes: types | mount | retmpl | itemstyle | states | variants | indent | gutter | all.
/// Pass 2 adds "depth" (which DP carries the per-level indent - TreeViewItem declares no indentation property
/// and the row itself does not move, yet the stock header shifts 16 DIP per level) and "gate" (three resting
/// values for hiding the nested host from an IsExpanded cell, because the first pass showed a template-attribute
/// resting value surviving a trigger write).
/// </summary>
internal static partial class Program
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

        var path = Path.Combine(AppContext.BaseDirectory, $"treeview-probe-{_mode}.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, mode={_mode}, full log: {path}");
        return 0;
    }

    private static void Run(Application application)
    {
        var root = new StackPanel { Margin = new Thickness(24) };
        var window = new Window { Content = root, Width = 780, Height = 620, Title = "TreeView probe" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Pump();
                Note($"theme dictionaries={FluentThemeManager.DictionaryNames.Count}");
                Note($"our ListView styles are live: ListViewItemStyle={FluentThemeManager.GetStyle("DefaultListViewItemStyle") is not null}");
                Note($"our ListBox styles are live: ListBoxStyle={FluentThemeManager.GetStyle("DefaultListBoxStyle") is not null}");
                Note($"TreeViewItem type: {(ItemType?.FullName ?? "NOT EXPORTED")} base={(ItemType is null ? "-" : Chain(ItemType))}");

                if (_mode is "all" or "types")
                {
                    TypeCensus();
                }

                if (_mode is "all" or "mount")
                {
                    StockTree(root, window);
                }

                if (_mode is "all" or "retmpl")
                {
                    HostRetemplate(root, window);
                }

                if (_mode is "all" or "itemstyle")
                {
                    ImplicitItemStyle(root, window);
                }

                if (_mode is "all" or "states")
                {
                    StateSurface(root, window);
                }

                if (_mode is "all" or "variants")
                {
                    ItemTemplateVariants(root, window);
                }

                if (_mode is "all" or "indent")
                {
                    IndentGeometry(root, window);
                }

                if (_mode is "all" or "gutter")
                {
                    Gutters(root, window);
                }

                if (_mode is "depth")
                {
                    IndentCarrier(root, window);
                }

                if (_mode is "gate")
                {
                    CollapseGates(root, window);
                }

                if (_mode is "fg")
                {
                    ForegroundRoutes(root, window);
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

    private static string Chain(Type type)
    {
        var names = new List<string>();
        for (var current = type; current is not null && names.Count < 7; current = current.BaseType)
        {
            names.Add(current.Name);
        }

        return string.Join(" : ", names);
    }

    // ---------- A. what the runtime exports for trees ----------

    private static void TypeCensus()
    {
        Note(string.Empty);
        Note("=== A. exported tree types, base chains and property surfaces ===");
        var byName = typeof(TreeView).Assembly.GetTypes()
            .Where(type => !type.IsNested)
            .GroupBy(type => type.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        foreach (var name in new[]
                 {
                     "TreeView", "TreeViewItem", "TreeViewList", "TreeViewNode", "TreeViewItemPresenter",
                     "TreeViewItemTemplateSettings", "HeaderedItemsControl", "ItemsControl", "Selector", "ListBox",
                     "HierarchicalDataTemplate", "DataTemplate", "ToggleButton", "CheckBox", "Expander",
                     "VirtualizingStackPanel", "ItemsPresenter", "ContentPresenter"
                 })
        {
            if (!byName.TryGetValue(name, out var type))
            {
                Note($"  {name}: NOT EXPORTED by {typeof(TreeView).Assembly.GetName().Name}");
                continue;
            }

            Note($"  {name}: {type.FullName} abstract={type.IsAbstract} base={Chain(type)}");
        }

        Note("  TreeView own DPs: " + string.Join(", ", DeclaredDpNames(typeof(TreeView))));
        Note("  TreeView public properties (declared only): " + Join(Properties(typeof(TreeView))));
        if (ItemType is not null)
        {
            Note("  TreeViewItem own DPs: " + string.Join(", ", DeclaredDpNames(ItemType)));
            Note("  TreeViewItem public properties (declared only): " + Join(Properties(ItemType)));
        }

        Note(string.Empty);
        Note("  WinUI TreeView / TreeViewItem members this runtime does or does not carry:");
        foreach (var (typeName, names) in new[]
                 {
                     (typeof(TreeView), new[]
                      {
                          "ItemsSource", "SelectionMode", "SelectedNode", "ExpandAll", "CollapseAll",
                          "FlattenOutsideScrollViewer", "GetContainerFromItem", "ContainerFromIndex",
                          "ItemInvoked", "NodeExpanding", "NodeCollapsed", "NodeExpanded", "NodeInvoking",
                          "SelectionChanged", "MultiSelect", "IsMultiSelectEnabled", "ItemContainer",
                          "IsTabStop", "Background", "BorderBrush", "BorderThickness", "Padding", "CornerRadius"
                      }),
                     (ItemType ?? typeof(TreeView), new[]
                      {
                          "IsExpanded", "HasItems", "Header", "HeaderTemplate", "IsSelected", "Expanded",
                          "Collapsed", "PreviewExpanded", "CollapsedGlyph", "ExpandedGlyph", "GlyphBrush",
                          "GlyphOpacity", "GlyphSize", "MultiSelectIsEnabled", "TreeViewItemTemplateSettings",
                          "SelectOnExpansion", "Marker", "Content", "Items", "Padding", "MinHeight"
                      })
                 })
        {
            Note($"  {typeName.Name}:");
            foreach (var name in names.Distinct(StringComparer.Ordinal))
            {
                var property = typeName.GetProperty(name, PublicInstance);
                var dp = FindDp(typeName, name);
                var events = typeName.GetEvent(name, PublicInstance) is not null ? "yes" : "-";
                if (property is null && dp is null && events == "-")
                {
                    continue;
                }

                Note($"    {name}: property={(property is null ? "-" : property.PropertyType.Name)}" +
                     $" dp={(dp is null ? "-" : dp.Name)} event={events}");
            }

            Note("    absent: " + string.Join(", ", names.Distinct(StringComparer.Ordinal).Where(name =>
                typeName.GetProperty(name, PublicInstance) is null && FindDp(typeName, name) is null &&
                typeName.GetEvent(name, PublicInstance) is null)));
        }

        Note("  implicit style lookup for the tree types: " + Read(() =>
            $"TreeView={{0}} TreeViewItem={{1}}".Replace("{0}", Show(ApplicationResource(typeof(TreeView))))
                .Replace("{1}", Show(ApplicationResource(ItemType)))));
        Note("  stock TreeView theme-style key: " + Read(() =>
            Show(FluentThemeManager.GetStyle("DefaultTreeViewStyle"))));
        Note("  our ListView item style as a reuse candidate: " + Read(() =>
            Show(FluentThemeManager.GetStyle("DefaultListViewItemStyle")?.TargetType?.Name)));
    }

    // ---------- B. the shipped tree ----------

    private static void StockTree(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== B. a mounted stock tree: nesting, collapse, parts, containers ===");
        if (ItemType is null)
        {
            Note("  no TreeViewItem type; nothing to measure");
            return;
        }

        var grandChildA = Node("Grandchild A");
        var grandChildB = Node("Grandchild B");
        var childTwo = Node("Child 2", grandChildA, grandChildB);
        var parent = Node("Parent", Node("Child 1"), childTwo, Node("Child 3"));
        var tree = Tree(parent, Node("Sibling root"));
        root.Children.Add(tree);
        window.UpdateLayout();
        Pump(10);

        PrintTree(tree, "    ");
        Note("  named parts in tree: " + PartNames(tree));
        Note("  containers in the visual tree (collapsed roots): " + CountContainers(tree, ItemType));
        var first = (FrameworkElement?)FirstOfType(tree, ItemType);
        Note("  first container: " + (first is null
                   ? "NONE GENERATED"
                   : $"{first.GetType().Name} {first.ActualWidth:0.##}x{first.ActualHeight:0.##} style={Show(first.Style)} " +
                     $"padding={Read(() => Prop(first, "Padding"))} min-height={Read(() => Prop(first, "MinHeight"))} " +
                     $"background={Read(() => Prop(first, "Background") as Brush is Brush brush ? Hex(brush) : "null")} " +
                     $"header={Read(() => Prop(first, "Header"))}"));
        Note("  header text host: " + Read(() =>
        {
            var presenter = first is null ? null : (DependencyObject?)FirstOfType(first, typeof(ContentPresenter));
            var text = presenter is null ? null : FirstOfType(presenter, typeof(TextBlock)) as TextBlock;
            return presenter is null
                ? "no ContentPresenter"
                : $"presenter Content={Show(Prop(presenter, "Content"))} textBlock={(text is null ? "none" : Show(text.Text))}";
        }));

        Note("  expand parent (IsExpanded=true): " + Read(() =>
        {
            Note2(Set(first!, "IsExpanded", true));
            window.UpdateLayout();
            Pump(10);
            return $"containers={CountContainers(tree, ItemType)} childTwoExpanded={Show(Prop(first!, "IsExpanded"))}";
        }));
        Note("  expand grandchild level: " + Read(() =>
        {
            var level2 = AllOfType(tree, ItemType).Skip(1).Cast<FrameworkElement>().ToList();
            foreach (var row in level2)
            {
                Set(row, "IsExpanded", true);
            }

            window.UpdateLayout();
            Pump(10);
            return $"containers={CountContainers(tree, ItemType)} rows={level2.Count}";
        }));
        Note("  collapse parent again: " + Read(() =>
        {
            Set(first!, "IsExpanded", false);
            window.UpdateLayout();
            Pump(10);
            return $"containers={CountContainers(tree, ItemType)}";
        }));
        Note("  after collapse: " + Read(() =>
        {
            Set(first!, "IsExpanded", true);
            window.UpdateLayout();
            Pump(8);
            return $"containers={CountContainers(tree, ItemType)}";
        }));

        Note("  select the first row through its own property: " + Read(() =>
        {
            Set(first!, "IsSelected", true);
            window.UpdateLayout();
            Pump(6);
            return $"container.IsSelected={Show(Prop(first!, "IsSelected"))} tree.SelectedItem={Show(Prop(tree, "SelectedItem"))}";
        }));
        Note("  selected row brushes: " + Read(() => first is null ? "no container" : BrushLedger(first)));
        Note("  accent in effect: " + Hex(ApplicationResource("AccentFillColorDefaultBrush") as Brush));
        Note("  item panels in the stock tree: " + PanelNames(tree));
        root.Children.Clear();
    }

    // ---------- C. the host template ----------

    private static void HostRetemplate(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== C. our host template on a TreeView: lock, item host, scroll host ===");
        var tree = Tree(Node("Parent", Node("Child 1"), Node("Child 2")), Node("Root 2"), Node("Root 3"));
        tree.Template = OverlayTemplate("True");
        Note("  assign ours: " + Try(() =>
        {
            root.Children.Add(tree);
            window.UpdateLayout();
            Pump(10);
        }));
        Note("  after assign: containers=" + CountContainers(tree, ItemType) +
             " layoutRoot=" + CountNamed(tree, "LayoutRoot") +
             " scrollViewer=" + CountOfTypeName(tree, "ScrollViewer") +
             " itemsPresenter=" + CountOfTypeName(tree, "ItemsPresenter"));
        Note("  expand first: " + Read(() =>
        {
            var first = FirstOfType(tree, ItemType);
            Set(first!, "IsExpanded", true);
            window.UpdateLayout();
            Pump(8);
            return $"containers={CountContainers(tree, ItemType)}";
        }));
        root.Children.Clear();

        var renamed = Tree(Node("Parent", Node("Child 1")), Node("Root 2"));
        renamed.Template = ParseTemplate("renamed host", @"
            <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
              <Border Name='LayoutRoot'>
                <ScrollViewer Name='ScrollViewer' Focusable='False' IsOverlayScrollBarEnabled='True'>
                  <ItemsPresenter Name='Whatever'/>
                </ScrollViewer>
              </Border>
            </ControlTemplate>");
        Note("  assign a template with the ItemsPresenter renamed: " + (renamed.Template is null
            ? "parse failed"
            : Try(() =>
            {
                root.Children.Add(renamed);
                window.UpdateLayout();
                Pump(10);
            })));
        Note("  containers with the host renamed: " + CountContainers(renamed, ItemType));
        root.Children.Clear();

        var noHost = Tree(Node("Parent", Node("Child 1")), Node("Root 2"));
        noHost.Template = ParseTemplate("no item host", @"
            <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
              <Border Name='LayoutRoot'>
                <StackPanel Name='NoPresenter'/>
              </Border>
            </ControlTemplate>");
        Note("  assign a template with no item host: " + (noHost.Template is null
            ? "parse failed"
            : Try(() =>
            {
                root.Children.Add(noHost);
                window.UpdateLayout();
                Pump(10);
            })));
        Note("  containers without an ItemsPresenter: " + CountContainers(noHost, ItemType));
        root.Children.Clear();

        var noScroll = Tree(Node("Parent", Node("Child 1")), Node("Root 2"));
        noScroll.Template = ParseTemplate("no scrollviewer", @"
            <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
              <Border Name='LayoutRoot'>
                <ItemsPresenter Name='ItemsPresenter'/>
              </Border>
            </ControlTemplate>");
        Note("  assign a template with no ScrollViewer: " + (noScroll.Template is null
            ? "parse failed"
            : Try(() =>
            {
                root.Children.Add(noScroll);
                window.UpdateLayout();
                Pump(10);
            })));
        Note("  containers without a ScrollViewer: " + CountContainers(noScroll, ItemType) +
             " host=" + $"{noScroll.ActualWidth:0.##}x{noScroll.ActualHeight:0.##}" +
             " first=" + Read(() =>
             {
                 var first = (FrameworkElement?)FirstOfType(noScroll, ItemType);
                 return first is null ? "none" : $"{first.ActualWidth:0.##}x{first.ActualHeight:0.##}";
             }));
        root.Children.Clear();

        Note("  ItemsPanel knob: " + Read(() =>
        {
            var property = typeof(TreeView).GetProperty("ItemsPanel", PublicInstance);
            return property is null
                ? "TreeView exposes no ItemsPanel property"
                : $"type={property.PropertyType.Name} current={Show(property.GetValue(tree)?.GetType().Name)}";
        }));
        Note("  attached ScrollViewer setters in a style: " + Read(() =>
        {
            var dictionary = ParseDictionary(@"
                <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
                  <Style TargetType='TreeView'>
                    <Setter Property='ScrollViewer.VerticalScrollBarVisibility' Value='Disabled'/>
                  </Style>
                </ResourceDictionary>");
            if (dictionary is null)
            {
                return "style with an attached setter did not parse";
            }

            var parsed = Try(() => _application.Resources.MergedDictionaries.Add(dictionary));
            window.UpdateLayout();
            Pump(6);
            Try(() => _application.Resources.MergedDictionaries.Remove(dictionary));
            return "merge=" + parsed;
        }));
    }

    // ---------- D. the implicit container style route ----------

    private static void ImplicitItemStyle(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== D. an application-level implicit TreeViewItem style, both nesting levels ===");
        if (ItemType is null)
        {
            Note("  no TreeViewItem type");
            return;
        }

        var tree = Tree(Node("Parent", Node("Child 1")));
        root.Children.Add(tree);
        window.UpdateLayout();
        Pump(8);
        Note("  resting padding before: " + Read(() =>
        {
            var first = FirstOfType(tree, ItemType);
            return first is null ? "no container" : Show(Prop(first, "Padding"));
        }));

        var dictionary = ParseDictionary(@"
            <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
              <Style TargetType='TreeViewItem'>
                <Setter Property='Padding' Value='12,9,12,12'/>
                <Setter Property='Background' Value='#FFFF00FF'/>
              </Style>
            </ResourceDictionary>");
        if (dictionary is null)
        {
            Note("  the implicit item dictionary did not parse; nothing to conclude");
            return;
        }

        Note("  merge into application resources: " + Try(() => _application.Resources.MergedDictionaries.Add(dictionary)));
        window.UpdateLayout();
        Pump(10);
        Note("  expand so level 2 exists: " + Read(() =>
        {
            var first = FirstOfType(tree, ItemType);
            Set(first!, "IsExpanded", true);
            window.UpdateLayout();
            Pump(8);
            return $"containers={CountContainers(tree, ItemType)}";
        }));
        foreach (var (label, index) in new[] { ("level 1", 0), ("level 2", 1) })
        {
            Note($"  {label} container after merge: " + Read(() =>
            {
                var row = AllOfType(tree, ItemType).ElementAtOrDefault(index) as FrameworkElement;
                return row is null
                    ? "no container at this index"
                    : $"{row.GetType().Name} Style={(row.Style is null ? "null" : "set")}" +
                      $" padding={Show(Prop(row, "Padding"))} background={Show(Prop(row, "Background") as Brush is Brush brush ? Hex(brush) : "null")}" +
                      $" header={Show(Prop(row, "Header"))}";
            }));
        }

        Note("  ItemContainerStyle on the tree: " + Read(() =>
        {
            var property = ItemType!.BaseType!.GetProperty("ItemContainerStyle", PublicInstance)
                           ?? typeof(TreeView).GetProperty("ItemContainerStyle", PublicInstance);
            return property is null ? "no such property" : Show(property.GetValue(tree));
        }));
        Note("  ItemContainerStyle wins over the implicit route? " + Read(() =>
        {
            var margin = FindDp(typeof(FrameworkElement), "Margin")
                         ?? throw new InvalidOperationException("no Margin property on FrameworkElement");
            var style = new Style(ItemType);
            style.Setters.Add(new Setter(margin, new Thickness(31)));
            var property = typeof(TreeView).GetProperty("ItemContainerStyle", PublicInstance);
            if (property is null)
            {
                return "no ItemContainerStyle property on TreeView";
            }

            property.SetValue(tree, style);
            window.UpdateLayout();
            Pump(8);
            var row = AllOfType(tree, ItemType).ElementAtOrDefault(0);
            return row is null
                ? "no container"
                : $"margin={Show(Prop(row, "Margin"))} padding={Show(Prop(row, "Padding"))}";
        }));

        Try(() => _application.Resources.MergedDictionaries.Remove(dictionary));
        window.UpdateLayout();
        Pump(6);
        Note("  level 1 after removal: " + Read(() =>
        {
            var row = AllOfType(tree, ItemType).ElementAtOrDefault(0);
            return row is null ? "no container" : $"padding={Show(Prop(row, "Padding"))}";
        }));
        root.Children.Clear();
    }

    // ---------- E. the state flags a cell can condition on ----------

    private static void StateSurface(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== E. which upstream TreeViewItem signals exist on this container ===");
        if (ItemType is null)
        {
            Note("  no TreeViewItem type");
            return;
        }

        var tree = Tree(Node("Parent", Node("Child 1")), Node("Leaf root"));
        root.Children.Add(tree);
        window.UpdateLayout();
        Pump(8);
        var row = FirstOfType(tree, ItemType);
        if (row is null)
        {
            Note("  no container to inspect");
            return;
        }

        foreach (var name in new[]
                 {
                     "IsExpanded", "IsSelected", "HasItems", "IsMouseOver", "IsMouseCaptureWithin", "IsEnabled",
                     "IsFocused", "IsSelectionActive", "IsPointerOver", "IsPressed", "Header", "Items",
                     "HorizontalContentAlignment", "VerticalContentAlignment", "CornerRadius", "Padding",
                     "MinHeight", "Marker", "SelectOnExpansion"
                 })
        {
            var dp = FindDp(row.GetType(), name) is not null;
            var property = row.GetType().GetProperty(name, PublicInstance);
            if (dp || property is not null)
            {
                Note($"  {name}: dp={dp} clr={property?.PropertyType.Name ?? "-"} read=" +
                     Read(() => Show(Prop(row, name))));
            }
        }

        Note("  absent: " + string.Join(", ", new[]
        {
            "IsSelectionActive", "IsPointerOver", "IsPressed", "Marker", "SelectOnExpansion", "IsExpanded"
        }.Where(name => FindDp(row.GetType(), name) is null && row.GetType().GetProperty(name, PublicInstance) is null)));

        Note("  HasItems on a parent vs a leaf: " + Read(() =>
        {
            var rows = AllOfType(tree, ItemType).Cast<FrameworkElement>().ToList();
            return rows.Count < 2
                ? $"only {rows.Count} container(s)"
                : $"parent={Show(Prop(rows[0], "HasItems"))} leaf={Show(Prop(rows[1], "HasItems"))}";
        }));
        Note("  IsExpanded both directions: " + Read(() =>
        {
            var parent = AllOfType(tree, ItemType).First();
            var before = CountContainers(tree, ItemType);
            SetOrThrow(parent, "IsExpanded", true);
            window.UpdateLayout();
            Pump(8);
            var expanded = CountContainers(tree, ItemType);
            SetOrThrow(parent, "IsExpanded", false);
            window.UpdateLayout();
            Pump(8);
            return $"collapsed={before} expanded={expanded} recollapsed={CountContainers(tree, ItemType)}";
        }));
        Note("  selection exclusivity through the control: " + Read(() =>
        {
            var rows = AllOfType(tree, ItemType).Cast<FrameworkElement>().ToList();
            Set(rows[0], "IsSelected", true);
            window.UpdateLayout();
            Pump(6);
            var afterFirst = rows.Select(r => Show(Prop(r, "IsSelected"))).ToList();
            Set(rows[^1], "IsSelected", true);
            window.UpdateLayout();
            Pump(6);
            var afterLast = rows.Select(r => Show(Prop(r, "IsSelected"))).ToList();
            return $"after row0={string.Join(",", afterFirst)} after rowLast={string.Join(",", afterLast)}" +
                   $" tree.SelectedItem={Show(Prop(tree, "SelectedItem"))}";
        }));
        Note("  keyboard-ish focus flag: " + Read(() =>
        {
            SetOrThrow(row, "IsEnabled", false);
            window.UpdateLayout();
            Pump(6);
            var disabled = BrushLedger((FrameworkElement)row);
            SetOrThrow(row, "IsEnabled", true);
            window.UpdateLayout();
            Pump(6);
            return $"row disabled opacity ledger: {disabled}";
        }));
        root.Children.Clear();
    }

    // ---------- F. the expand/collapse route, template by template ----------

    private static void ItemTemplateVariants(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== F. five TreeViewItem templates: who drives IsExpanded, who hosts children ===");
        if (ItemType is null)
        {
            Note("  no TreeViewItem type");
            return;
        }

        foreach (var (label, markup) in new[]
                 {
                     ("header only", @"
                        <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
                          <Grid Background='Transparent'>
                            <ContentPresenter Name='HeaderHost' Content='{TemplateBinding Header}'/>
                          </Grid>
                        </ControlTemplate>"),
                     ("named Expander, no gate", @"
                        <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
                          <Grid Background='Transparent'>
                            <StackPanel Orientation='Horizontal'>
                              <ToggleButton Name='Expander' Content='+' Focusable='False'/>
                              <ContentPresenter Name='HeaderHost' Content='{TemplateBinding Header}'/>
                            </StackPanel>
                            <ItemsPresenter Name='ItemsPresenter' Margin='16,0,0,0'/>
                          </Grid>
                        </ControlTemplate>"),
                     ("two-way binding on IsExpanded", @"
                        <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
                          <Grid Background='Transparent'>
                            <StackPanel Orientation='Horizontal'>
                              <ToggleButton Name='Chevron' Focusable='False' Content='+'
                                            IsChecked='{Binding IsExpanded, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}'/>
                              <ContentPresenter Name='HeaderHost' Content='{TemplateBinding Header}'/>
                            </StackPanel>
                            <ItemsPresenter Name='ItemsPresenter' Margin='16,0,0,0'/>
                          </Grid>
                        </ControlTemplate>"),
                     ("template trigger gates the child host", @"
                        <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
                          <Grid Background='Transparent'>
                            <StackPanel Orientation='Horizontal'>
                              <ToggleButton Name='Chevron' Focusable='False' Content='+'
                                            IsChecked='{Binding IsExpanded, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}'/>
                              <ContentPresenter Name='HeaderHost' Content='{TemplateBinding Header}'/>
                            </StackPanel>
                            <ItemsPresenter Name='ItemsPresenter' Margin='16,0,0,0' Visibility='Collapsed'/>
                          </Grid>
                          <ControlTemplate.Triggers>
                            <Trigger Property='IsExpanded' Value='True'>
                              <Setter TargetName='ItemsPresenter' Property='Visibility' Value='Visible'/>
                            </Trigger>
                          </ControlTemplate.Triggers>
                        </ControlTemplate>"),
                     ("no child host at all", @"
                        <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
                          <Grid Background='Transparent'>
                            <ContentPresenter Name='HeaderHost' Content='{TemplateBinding Header}'/>
                          </Grid>
                        </ControlTemplate>"),
                     ("ItemsPresenter renamed", @"
                        <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
                          <Grid Background='Transparent'>
                            <ContentPresenter Name='HeaderHost' Content='{TemplateBinding Header}'/>
                            <ItemsPresenter Name='ItemsHost' Margin='16,0,0,0'/>
                          </Grid>
                        </ControlTemplate>"),
                 })
        {
            var template = ParseTemplate(label, markup);
            if (template is null)
            {
                continue;
            }

            var style = new Style(ItemType);
            style.Setters.Add(new Setter(Control.TemplateProperty, template));
            var dictionary = new ResourceDictionary();
            dictionary.Add(ItemType, style);
            Note($"  --- variant: {label}");
            Note("    merge: " + Try(() => _application.Resources.MergedDictionaries.Add(dictionary)));

            var tree = Tree(Node("Parent", Node("Child 1"), Node("Child 2")), Node("Leaf root"));
            root.Children.Add(tree);
            window.UpdateLayout();
            Pump(12);

            var parent = (FrameworkElement?)AllOfType(tree, ItemType).ElementAtOrDefault(0);
            Note("    containers at rest: " + CountContainers(tree, ItemType) +
                 " parentHeader=" + Read(() => parent is null ? "no container" : Show(Prop(parent, "Header"))));
            Note("    header text rendered: " + Read(() =>
            {
                if (parent is null)
                {
                    return "no container";
                }

                var text = AllOfType(parent, typeof(TextBlock)).OfType<TextBlock>().Select(t => t.Text).ToList();
                return text.Count == 0 ? "NO TEXT" : string.Join("|", text);
            }));
            Note("    HasItems/IsExpanded reads: " + Read(() => parent is null
                ? "no container"
                : $"HasItems={Show(Prop(parent, "HasItems"))} IsExpanded={Show(Prop(parent, "IsExpanded"))}"));

            Note("    set IsExpanded=true directly: " + Read(() =>
            {
                Note2(Set(parent!, "IsExpanded", true));
                window.UpdateLayout();
                Pump(10);
                return $"containers={CountContainers(tree, ItemType)} IsExpanded={Show(Prop(parent!, "IsExpanded"))}";
            }));
            Note("    set IsExpanded=false again: " + Read(() =>
            {
                Note2(Set(parent!, "IsExpanded", false));
                window.UpdateLayout();
                Pump(10);
                return $"containers={CountContainers(tree, ItemType)}";
            }));

            var chevron = parent is null || ToggleType is null ? null : AllOfType(parent, ToggleType).ElementAtOrDefault(0);
            Note($"    a ToggleButton exists in the template: {(ToggleType is null ? "type not exported" : chevron is null ? "NO" : "yes")}");
            if (chevron is not null)
            {
                Note("    toggle the chevron through its click path: " + Read(() =>
                {
                    var before = Show(Prop(parent!, "IsExpanded"));
                    var result = Click(chevron);
                    window.UpdateLayout();
                    Pump(10);
                    return $"via={result} isCheckedBefore={before} toggle.IsChecked={Show(Prop(chevron, "IsChecked"))}" +
                           $" item.IsExpanded={Show(Prop(parent!, "IsExpanded"))} containers={CountContainers(tree, ItemType)}";
                }));
                Note("    item -> toggle direction: " + Read(() =>
                {
                    SetOrThrow(parent!, "IsExpanded", true);
                    window.UpdateLayout();
                    Pump(8);
                    return $"item.IsExpanded={Show(Prop(parent!, "IsExpanded"))} toggle.IsChecked={Show(Prop(chevron, "IsChecked"))}";
                }));
                Note("    toggle.IsChecked written directly: " + Read(() =>
                {
                    SetOrThrow(chevron, "IsChecked", false);
                    window.UpdateLayout();
                    Pump(8);
                    return $"toggle.IsChecked={Show(Prop(chevron, "IsChecked"))} item.IsExpanded={Show(Prop(parent!, "IsExpanded"))}" +
                           $" containers={CountContainers(tree, ItemType)}";
                }));
            }

            Note("    select the row through its own property: " + Read(() =>
            {
                Note2(Set(parent!, "IsSelected", true));
                window.UpdateLayout();
                Pump(8);
                return "brushes: " + BrushLedger(parent!);
            }));
            Note("    row geometry: " + Read(() => parent is null
                ? "no container"
                : $"{parent.ActualWidth:0.##}x{parent.ActualHeight:0.##}"));

            root.Children.Clear();
            Try(() => _application.Resources.MergedDictionaries.Remove(dictionary));
            window.UpdateLayout();
            Pump(6);
        }
    }

    // ---------- G. where the indent comes from ----------

    private static void IndentGeometry(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== G. the per-level indent on the nested shape ===");
        if (ItemType is null)
        {
            Note("  no TreeViewItem type");
            return;
        }

        var leaf = Node("Grandchild A");
        var tree = Tree(Node("L1", Node("L2", leaf)), Node("Other root"));
        root.Children.Add(tree);
        window.UpdateLayout();
        Pump(10);
        Note("  expand the root: " + Read(() =>
        {
            SetOrThrow(AllOfType(tree, ItemType).ElementAtOrDefault(0)!, "IsExpanded", true);
            window.UpdateLayout();
            Pump(8);
            return "containers=" + CountContainers(tree, ItemType);
        }));
        Note("  stock tree, rows expanded: " + Read(() =>
        {
            var builder = new StringBuilder();
            foreach (var row in AllOfType(tree, ItemType).Cast<FrameworkElement>())
            {
                var at = row.TranslatePoint(new Point(), tree);
                var header = FirstOfType(row, typeof(ContentPresenter));
                var headerAt = header is null ? new Point() : ((FrameworkElement)header).TranslatePoint(new Point(), tree);
                builder.Append($"\n    {Show(Prop(row, "Header"))} row@{at.X:0.##},{at.Y:0.##} {row.ActualWidth:0.##}x{row.ActualHeight:0.##}" +
                               $" header@{headerAt.X:0.##},{headerAt.Y:0.##}");
            }

            return builder.ToString();
        }));
        Note("  named parts: " + PartNames((DependencyObject)tree));
        Note("  toggle-button parts in the tree: " + (ToggleType is null ? "type not exported" : AllOfType(tree, ToggleType).Count().ToString()));
        root.Children.Clear();
        window.UpdateLayout();
        Pump(4);

        var ours = Tree(Node("L1", Node("L2", Node("L3"))), Node("Other root"));
        ours.Template = OverlayTemplate("True");
        root.Children.Add(ours);
        window.UpdateLayout();
        Pump(10);
        Note("  expand the roots: " + Read(() =>
        {
            foreach (var row in AllOfType(ours, ItemType).Take(2))
            {
                Set(row, "IsExpanded", true);
            }

            window.UpdateLayout();
            Pump(10);
            return "containers=" + CountContainers(ours, ItemType);
        }));
        Note("  our host template, same content: " + Read(() =>
        {
            var builder = new StringBuilder();
            foreach (var row in AllOfType(ours, ItemType).Cast<FrameworkElement>())
            {
                var at = row.TranslatePoint(new Point(), ours);
                builder.Append($"\n    {Show(Prop(row, "Header"))} row@{at.X:0.##},{at.Y:0.##} {row.ActualWidth:0.##}x{row.ActualHeight:0.##}");
            }

            return builder.ToString();
        }));
        root.Children.Clear();
    }

    // ---------- H. the gutter ----------

    private static void Gutters(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== H. the 12 DIP right gutter on a stock tree and on our own template ===");
        foreach (var (label, count) in new[] { ("5 roots", 5), ("50 roots", 50) })
        {
            var stock = Tree(Enumerable.Range(1, count).Select(index => Node($"Row {index}")).ToArray());
            root.Children.Add(stock);
            window.UpdateLayout();
            Pump(12);
            Note($"  stock TreeView, {label}: " + Ledger(stock, window));

            var ours = Tree(Enumerable.Range(1, count).Select(index => Node($"Row {index}")).ToArray());
            ours.Template = OverlayTemplate("True");
            root.Children.Add(ours);
            window.UpdateLayout();
            Pump(12);
            Note($"  our template + overlay, {label}: " + Ledger(ours, window));

            var noverlay = Tree(Enumerable.Range(1, count).Select(index => Node($"Row {index}")).ToArray());
            noverlay.Template = OverlayTemplate("False");
            root.Children.Add(noverlay);
            window.UpdateLayout();
            Pump(12);
            Note($"  our template, overlay off, {label}: " + Ledger(noverlay, window));

            root.Children.Clear();
            window.UpdateLayout();
            Pump(4);
        }
    }

    private static ControlTemplate? OverlayTemplate(string overlay) => ParseTemplate($"overlay={overlay}", $@"
        <ControlTemplate xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
          <Border Name='LayoutRoot' Background='{{TemplateBinding Background}}' BorderThickness='{{TemplateBinding BorderThickness}}'>
            <ScrollViewer Name='ScrollViewer' Focusable='False' HorizontalScrollBarVisibility='Disabled'
                          VerticalScrollBarVisibility='Auto' IsOverlayScrollBarEnabled='{overlay}'>
              <ItemsPresenter Name='ItemsPresenter'/>
            </ScrollViewer>
          </Border>
        </ControlTemplate>");

    private static string Ledger(DependencyObject root, Window window)
    {
        if (ItemType is null)
        {
            return "no TreeViewItem type";
        }

        var first = (FrameworkElement?)FirstOfType(root, ItemType);
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

    /// <summary>Every brush the container's template parts carry, so a state write is visible in the log.</summary>
    private static string BrushLedger(FrameworkElement row)
    {
        var builder = new StringBuilder();
        Walk(row, node =>
        {
            if (ReferenceEquals(node, row))
            {
                return;
            }

            var brush = node.GetType().GetProperty("Background")?.GetValue(node) as Brush
                        ?? node.GetType().GetProperty("Fill")?.GetValue(node) as Brush
                        ?? node.GetType().GetProperty("BorderBrush")?.GetValue(node) as Brush;
            var foreground = node is TextBlock text ? text.Foreground : null;
            if (brush is not null || foreground is not null)
            {
                builder.Append($"{node.GetType().Name}.{NameOf(node)}={Hex(brush ?? foreground)}; ");
            }

            if (node is FrameworkElement element)
            {
                builder.Append($"[op={Read(() => Show(Prop(element, "Opacity")))} vis={element.Visibility}] ");
            }
        }, 20);
        return builder.Length == 0 ? "no brush" : builder.ToString();
    }

    // ---------- tree builders ----------

    private static Type? ItemType => TypeByName("TreeViewItem");

    /// <summary>Resolved by name: this runtime may declare the toggle in a primitives namespace, and a typo there is a log line, not a build break.</summary>
    private static Type? ToggleType => TypeByName("ToggleButton");

    private static object? Node(string header, params object?[] children)
    {
        var type = ItemType;
        if (type is null)
        {
            return null;
        }

        object? created;
        try
        {
            created = Activator.CreateInstance(type);
        }
        catch (Exception exception)
        {
            Note($"  Node({header}): ctor threw {exception.GetType().Name}: {Trim((exception.InnerException ?? exception).Message)}");
            return null;
        }

        if (created is not FrameworkElement element)
        {
            Note($"  Node({header}): not a FrameworkElement ({Show(created?.GetType().Name)})");
            return null;
        }

        Set(element, "Header", header);
        if (children.Length == 0)
        {
            return element;
        }

        // The nested collection is reached through whatever shape this runtime exposes: an IList, else an Add
        // method. Both routes are logged so a missing child host cannot be mistaken for a template defect.
        if (Prop(element, "Items") is System.Collections.IList items)
        {
            foreach (var child in children)
            {
                items.Add(child);
            }

            return element;
        }

        var add = Prop(element, "Items")?.GetType().GetMethod("Add", [typeof(object)]);
        if (add is null)
        {
            Note($"  Node({header}): no usable Items collection ({Show(Prop(element, "Items"))}); children not added");
            return element;
        }

        foreach (var child in children)
        {
            add.Invoke(Prop(element, "Items"), [child]);
        }

        return element;
    }

    private static TreeView Tree(params object?[] roots)
    {
        var tree = new TreeView { Width = 300, Height = 220 };
        foreach (var root in roots)
        {
            if (root is not null)
            {
                tree.Items.Add(root);
            }
        }

        return tree;
    }

    // ---------- plumbing ----------

    private static readonly BindingFlags PublicInstance =
        BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

    private static object? ApplicationResource(object key) => _application.TryFindResource(key);

    private static string Click(object target)
    {
        var peerRoute = Read(() =>
        {
            var peerType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => SafeTypes(assembly))
                .FirstOrDefault(type => !type.IsNested && type.Name == "ToggleButtonAutomationPeer" && !type.IsAbstract);
            if (peerType is null)
            {
                return "no ToggleButtonAutomationPeer type";
            }

            var peer = Activator.CreateInstance(peerType, new[] { target })!;
            var getPattern = peer.GetType().GetMethod("GetPattern");
            var patternInterface = peer.GetType().Assembly.GetType("Jalium.UI.Automation.Peers.PatternInterface")
                                   ?? TypeByName("PatternInterface");
            var pattern = getPattern?.Invoke(peer, new object?[]
            {
                patternInterface is null ? null : Enum.Parse(patternInterface, "Toggle")
            });
            if (pattern is null)
            {
                return "peer.GetPattern(Toggle) returned null";
            }

            pattern.GetType().GetMethod("Toggle")?.Invoke(pattern, null);
            return "peer";
        });
        if (peerRoute.StartsWith("peer", StringComparison.Ordinal))
        {
            return peerRoute;
        }

        var clickRoute = Read(() =>
        {
            var method = target.GetType().GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method is null)
            {
                return "no OnClick method";
            }

            method.Invoke(target, null);
            return "OnClick";
        });
        return $"peer failed ({Trim(peerRoute)}); {clickRoute}";
    }

    private static IEnumerable<Type> SafeTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch
        {
            return [];
        }
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
            var text = node is TextBlock block ? $" \"{Trim(block.Text)}\"" : string.Empty;
            builder.Append($"\n{indent}{new string(' ', depth * 2)}{node.GetType().Name}" +
                           (string.IsNullOrEmpty(name) ? string.Empty : $" '{name}'") + text + size);
        }, 40);
        Note(builder.ToString());
    }

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
        return names.Count == 0 ? "no named part" : string.Join(", ", names.Distinct(StringComparer.Ordinal));
    }

    private static string PanelNames(DependencyObject root)
    {
        var names = new List<string>();
        Walk(root, node =>
        {
            if (node.GetType().Name.EndsWith("Panel", StringComparison.Ordinal) ||
                node.GetType().Name.EndsWith("StackPanel", StringComparison.Ordinal))
            {
                var size = node is FrameworkElement element ? $"{element.ActualWidth:0.##}x{element.ActualHeight:0.##}" : "-";
                names.Add($"{node.GetType().Name}({size})");
            }
        }, 24);
        return names.Count == 0 ? "no panel" : string.Join(", ", names);
    }

    private static int CountContainers(DependencyObject root, Type? containerType) =>
        containerType is null ? -1 : AllOfType(root, containerType).Count();

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

    private static int CountOfTypeName(DependencyObject root, string typeName)
    {
        var count = 0;
        Walk(root, node =>
        {
            if (node.GetType().Name == typeName)
            {
                count++;
            }
        }, 40);
        return count;
    }

    private static IEnumerable<DependencyObject> AllOfType(DependencyObject root, Type type)
    {
        var found = new List<DependencyObject>();
        Walk(root, node =>
        {
            if (type.IsInstanceOfType(node))
            {
                found.Add(node);
            }
        }, 40);
        return found;
    }

    private static object? FirstOfType(DependencyObject root, Type type) => AllOfType(root, type).FirstOrDefault();

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

    private static Type? TypeByName(string name) => typeof(TreeView).Assembly.GetTypes()
        .FirstOrDefault(type => !type.IsNested && type.Name == name);

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

    private static object? Prop(object? target, string name) => target is null
        ? null
        : target.GetType().GetProperty(name, PublicInstance)?.GetValue(target);

    /// <summary>Write through the CLR property so a member this runtime does not have costs a log line, not a build.</summary>
    private static string Set(object target, string name, object value)
    {
        var property = target.GetType().GetProperty(name, PublicInstance);
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
        var property = target.GetType().GetProperty(name, PublicInstance)
                       ?? throw new InvalidOperationException($"no {name} property on {target.GetType().Name}");
        property.SetValue(target, value);
    }

    private static void Note2(string line) => Note("    " + Trim(line));

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
