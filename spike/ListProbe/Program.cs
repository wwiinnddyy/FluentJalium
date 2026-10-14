using System.Reflection;
using System.Text;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace ListProbe;

/// <summary>
/// Answers the four questions the list batch cannot write product code without (stage 5, first slice):
///   A: what does the runtime actually export for lists - base types, the property surface a style can write,
///      the SelectionMode values, and whether a control arrives with a Style or only a framework-built Template
///      (adaptation/00 S0-h says "no default style" is not the same as "no default look").
///   B: which part names the shipped list templates use, read off a mounted list rather than guessed from WPF.
///   C: can ListBox take our template at all (the UseTemplateContentManagement lock, S0-m), and which name in it
///      is a functional contract for items to appear at all (the S0-i item-host contract, measured for popups
///      and Expanders so far, never for a list).
///   D: does the list virtualize, and what does the Auto scrollbar do to the item width - the asymmetry the user
///      reported twice (S0-u) has never been measured on a list surface.
///   E: how selection is driven: what the automation peers expose and whether keyboard handling exists at all.
///   F: which ScrollViewer switch removes the always-reserved 12 DIP right gutter (S0-u on a list surface).
///   G/H: whether an implicit ListBoxItem style reaches framework-generated containers at app or window scope, or
///      whether ItemContainerStyle has to be written by our list style.
/// Modes: types | mount | retmpl | virtual | select | gutter | itemstyle | theme | all. Reflection is used freely
/// here and only here: product code stays free of it, which is what the structural gate in AstraGateTests enforces.
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

        var path = Path.Combine(AppContext.BaseDirectory, $"list-probe-{_mode}.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, mode={_mode}, full log: {path}");
        return 0;
    }

    private static void Run(Application application)
    {
        var root = new StackPanel { Margin = new Thickness(24) };
        var window = new Window { Content = root, Width = 760, Height = 560, Title = "List probe" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Pump();
                Note($"theme dictionaries={FluentThemeManager.DictionaryNames.Count}");
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

                if (_mode is "all" or "virtual")
                {
                    Virtualized(root, window);
                }

                if (_mode is "all" or "select")
                {
                    Selection(root, window);
                }

                if (_mode is "all" or "gutter")
                {
                    Gutters(root, window);
                }

                if (_mode is "all" or "itemstyle")
                {
                    ItemStyle(root, window);
                }

                if (_mode is "all" or "theme")
                {
                    ContainerTheme(root, window);
                }

                if (_mode is "all" or "itemtmpl")
                {
                    ItemTemplate(root, window);
                }

                if (_mode is "all" or "attach")
                {
                    AttachedSetters(root, window);
                }
            }
            finally
            {
                application.Shutdown();
            }
        };
        application.Run(window);
    }

    // ---------- A. what the runtime exports ----------

    private static readonly string[] Candidates =
    [
        "ItemsControl", "ListBox", "ListBoxItem", "ListView", "ListViewItem", "GridView",
        "TreeView", "TreeViewItem", "VirtualizingStackPanel", "StackPanel", "ScrollViewer",
        "Selector", "DataGrid",
    ];

    private static readonly string[] PropertiesOfInterest =
    [
        "Items", "ItemsSource", "ItemTemplate", "ItemContainerStyle", "ItemsPanel", "SelectionMode",
        "SelectedIndex", "SelectedItem", "SelectedItems", "IsSynchronizedWithCurrentItem", "IsSelected",
        "IsHighlighted", "HorizontalScrollBarVisibility", "VerticalScrollBarVisibility", "Padding",
        "BorderBrush", "BorderThickness", "Background", "Foreground", "ScrollIntoViewOnSelectionChange",
        "ItemContainerTheme", "ChildrenTransitions", "IsVirtualizing", "VirtualizationMode",
    ];

    private static void TypeCensus()
    {
        Note(string.Empty);
        Note("=== A. what the runtime exports for lists (by simple name over the loaded Jalium.UI assemblies) ===");
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(static assembly => assembly.GetName().Name?.StartsWith("Jalium.UI", StringComparison.Ordinal) == true)
            .ToArray();
        foreach (var name in Candidates)
        {
            var found = assemblies
                .Select(assembly => SafeTypes(assembly).FirstOrDefault(type =>
                    type.Name == name && type.Namespace?.StartsWith("Jalium.UI", StringComparison.Ordinal) == true))
                .FirstOrDefault(static type => type is not null);
            if (found is null)
            {
                Note($"  {name}: NOT EXPORTED");
                continue;
            }

            var own = found
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(static field => field.FieldType.Name.Contains("DependencyProperty", StringComparison.Ordinal))
                .Select(static field => field.Name.EndsWith("Property", StringComparison.Ordinal)
                    ? field.Name[..^"Property".Length]
                    : field.Name)
                .ToHashSet(StringComparer.Ordinal);
            var inherited = found.GetProperties().Select(static property => property.Name).ToHashSet(StringComparer.Ordinal);
            var hit = PropertiesOfInterest.Where(inherited.Contains).ToArray();
            var declared = PropertiesOfInterest.Where(own.Contains).ToArray();
            Note($"  {name} = {found.FullName} : {found.BaseType?.Name}" +
                 $" | public={found.IsPublic} abstract={found.IsAbstract}" +
                 $" | ctor={found.GetConstructor(Type.EmptyTypes) is not null}" +
                 $"\n      see-surface: {string.Join(", ", hit.Length == 0 ? ["none"] : hit)}" +
                 $"\n      own DPs: {string.Join(", ", DeclaredDpNames(found))}");
        }

        Note(string.Empty);
        foreach (var enumName in new[] { "SelectionMode", "VirtualizationMode", "ScrollMode" })
        {
            var type = assemblies
                .Select(assembly => SafeTypes(assembly).FirstOrDefault(t => t.Name == enumName && t.IsEnum))
                .FirstOrDefault(static t => t is not null);
            Note($"  {enumName}: " + (type is null
                ? "NOT EXPORTED"
                : string.Join(", ", Enum.GetNames(type))));
        }
    }

    private static string[] DeclaredDpNames(Type type) => type
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(static field => field.FieldType.Name.Contains("DependencyProperty", StringComparison.Ordinal))
        .Select(static field => field.Name)
        .OrderBy(static name => name, StringComparer.Ordinal)
        .ToArray();

    private static IEnumerable<Type> SafeTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(static type => type is not null)!;
        }
    }

    // ---------- B. the shipped list tree ----------

    private static void Mounted(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== B. a mounted ListBox: what tree the framework builds, and what it calls its parts ===");
        var list = new ListBox { Width = 260, Height = 180 };
        AddItems(list, 5);
        root.Children.Add(list);
        window.UpdateLayout();
        Pump(8);
        Note($"  style={(list.Style is null ? "null" : "assigned")} template={list.Template is not null}" +
             $" items-panel={list.ItemsPanel is not null} container-style={(list.ItemContainerStyle is null ? "null" : "assigned")}" +
             $" list {list.ActualWidth:0.##}x{list.ActualHeight:0.##}");
        PrintTree(list, "  ");
        Note("  containers found: " + Read(() => CountContainers(list, typeof(ListBoxItem))));
        Note("  first item surface: " + Read(() =>
        {
            var item = FirstOfType(list, typeof(ListBoxItem));
            if (item is null)
            {
                return "none";
            }

            var element = (FrameworkElement)item;
            return $"{item.GetType().Name} {element.ActualWidth:0.##}x{element.ActualHeight:0.##} at " +
                   OffsetOf(element, list) + $" bg={Name(list, element)}";
        }));
        Note("  item own DPs: " + string.Join(", ", DeclaredDpNames(typeof(ListBoxItem))));
    }

    // ---------- C. can the list take our template, and which name matters ----------

    private static void Retemplated(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== C. assigning a parsed ControlTemplate to ListBox ===");
        Note($"  ContentControl.UseTemplateContentManagement declared on: " +
             Read(() => typeof(ContentControl).GetMethod("UseTemplateContentManagement",
                 BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.DeclaringType?.Name ?? "absent"));

        foreach (var (label, markup) in Variants)
        {
            var list = new ListBox { Width = 260, Height = 180 };
            AddItems(list, 4);
            var template = ParseTemplate(label, markup);
            if (template is null)
            {
                continue;
            }

            list.Template = template;
            root.Children.Add(list);
            window.UpdateLayout();
            Pump(8);
            var containers = Read(() => CountContainers(list, typeof(ListBoxItem)));
            var hosts = Read(() => CountNamed(list, "ItemsPresenter"));
            Note($"  {label}: assigned={list.Template is not null} tree={VisualChildren(list)}" +
                 $" items-presenters={hosts} containers={containers} list {list.ActualWidth:0.##}x{list.ActualHeight:0.##}");
            if (label is "upstream-shaped")
            {
                PrintTree(list, "      ");
            }
        }
    }

    private static readonly (string Label, string Markup)[] Variants =
    [
        ("bare ItemsPresenter", """
            <ControlTemplate xmlns="http://schemas.jalium.ui/2024" TargetType="ListBox">
              <Border Name="Shell" Background="#FFE1E1E1" BorderBrush="#FF707070" BorderThickness="1">
                <ItemsPresenter Name="ItemsPresenter" />
              </Border>
            </ControlTemplate>
            """),
        ("items host is a StackPanel", """
            <ControlTemplate xmlns="http://schemas.jalium.ui/2024" TargetType="ListBox">
              <Border Name="Shell" Background="#FFE1E1E1">
                <StackPanel Name="Host" />
              </Border>
            </ControlTemplate>
            """),
        ("renamed presenter", """
            <ControlTemplate xmlns="http://schemas.jalium.ui/2024" TargetType="ListBox">
              <Border Name="Shell" Background="#FFE1E1E1">
                <ItemsPresenter Name="NotTheItemsPresenter" />
              </Border>
            </ControlTemplate>
            """),
        ("upstream-shaped", """
            <ControlTemplate xmlns="http://schemas.jalium.ui/2024" TargetType="ListBox">
              <Border Name="Border" Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}" CornerRadius="4">
                <ScrollViewer Name="SV" Padding="{TemplateBinding Padding}" Focusable="False" HorizontalScrollBarVisibility="Disabled" VerticalScrollBarVisibility="Auto">
                  <ItemsPresenter Name="ItemsPresenter" />
                </ScrollViewer>
              </Border>
            </ControlTemplate>
            """),
    ];

    // ---------- D. virtualization and the scrollbar asymmetry ----------

    private static void Virtualized(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== D. 1000 items in a 260x180 list: realized containers and the width asymmetry ===");
        var list = new ListBox { Width = 260, Height = 180 };
        AddItems(list, 1000);
        root.Children.Add(list);
        window.UpdateLayout();
        Pump(10);
        Note($"  items={Read(() => list.Items.Count.ToString())}" +
             $" containers-in-tree={Read(() => CountContainers(list, typeof(ListBoxItem)))}" +
             $" scroll-viewer-children={Read(() => VisualChildren(list))}");
        Note("  is-virtualizing on the items panel: " + Read(() =>
        {
            var panel = FindOfType(list, typeof(VirtualizingStackPanel));
            return panel is null ? "no VirtualizingStackPanel in the tree" : "found " + panel.GetType().Name;
        }));
        Note("  width ledger (S0-u asymmetry):" + Read(() =>
        {
            var first = (FrameworkElement?)FirstOfType(list, typeof(ListBoxItem));
            if (first is null)
            {
                return " no container to measure";
            }

            var at = first.TranslatePoint(new Point(), list);
            return $" list={list.ActualWidth:0.##}x{list.ActualHeight:0.##}" +
                   $" item={first.ActualWidth:0.##}x{first.ActualHeight:0.##}" +
                   $" item-origin={at.X:0.##},{at.Y:0.##}" +
                   $" left-gap={at.X:0.##} right-gap={list.ActualWidth - at.X - first.ActualWidth:0.##}";
        }));
        var bars = BarsIn(list);
        Note("  ScrollBar parts: " + (bars.Count == 0
            ? "none found in the list subtree"
            : string.Join("; ", bars.Select(bar =>
                $"{bar.GetType().Name} {bar.ActualWidth:0.##}x{bar.ActualHeight:0.##} visible={bar.Visibility}"))));
        Note("  after scrolling down: " + Read(() =>
        {
            var viewer = (ScrollViewer?)FindOfType(list, typeof(ScrollViewer));
            if (viewer is null)
            {
                return "no ScrollViewer";
            }

            viewer.GetType().GetProperty("VerticalOffset");
            var method = viewer.GetType().GetMethod("ScrollToVerticalOffset") ??
                         viewer.GetType().GetMethod("SetVerticalOffset");
            if (method is null)
            {
                return "no scroll method to call";
            }

            method.Invoke(viewer, [400d]);
            window.UpdateLayout();
            Pump(6);
            var first = (FrameworkElement?)FirstOfType(list, typeof(ListBoxItem));
            var content = Read(() => viewer.Content is null ? "null" : viewer.Content.GetType().Name);
            return $"called {method.Name}(400); content={content}; " +
                   $"first container " + (first is null ? "gone" : $"{first.ActualWidth:0.##}x{first.ActualHeight:0.##}");
        }));
    }

    // ---------- E. how selection is driven ----------

    private static void Selection(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== E. selection surface: properties, keyboard handling, automation providers ===");
        var list = new ListBox { Width = 260, Height = 180 };
        AddItems(list, 6);
        root.Children.Add(list);
        window.UpdateLayout();
        Pump(6);

        Note("  SelectedIndex after add: " + Read(() => list.SelectedIndex.ToString()));
        Note("  set SelectedIndex=2 -> " + Try(() => list.SelectedIndex = 2));
        Pump(4);
        Note($"  reads SelectedIndex={list.SelectedIndex} SelectedItem={list.SelectedItem?.GetType().Name}");
        var selected = FirstOfType(list, typeof(ListBoxItem));
        Note("  container own DPs of interest: " + string.Join(", ", DeclaredDpNames(typeof(ListBoxItem))));
        Note("  SelectionMode on list: " + Read(() => list.GetType().GetProperty("SelectionMode")?.GetValue(list)?.ToString() ?? "no such property"));
        foreach (var value in new[] { "Single", "Multiple", "Extended" })
        {
            Note($"  set SelectionMode={value} -> " + Read(() =>
            {
                var property = list.GetType().GetProperty("SelectionMode");
                if (property is null)
                {
                    return "no property";
                }

                var target = Enum.Parse(property.PropertyType, value);
                property.SetValue(list, target);
                return "ok, reads " + property.GetValue(list);
            }));
        }

        Note("  handlers declared on ListBox: " + Read(() => string.Join(", ", typeof(ListBox)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(static method => method.Name)
            .Where(static name => name.Contains("Key", StringComparison.Ordinal) ||
                                  name.Contains("Pointer", StringComparison.Ordinal) ||
                                  name.Contains("Mouse", StringComparison.Ordinal) ||
                                  name.Contains("Select", StringComparison.Ordinal))
            .OrderBy(static name => name, StringComparer.Ordinal))));
        Note("  automation peer: " + Read(() =>
        {
            var peer = typeof(ListBox).GetMethod("OnCreateAutomationPeer",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.Invoke(list, null);
            return peer is null ? "null" : peer.GetType().Name;
        }));
        Note("  item peer: " + Read(() =>
        {
            var container = FirstOfType(list, typeof(ListBoxItem));
            if (container is null)
            {
                return "no container";
            }

            var method = container.GetType().GetMethod("OnCreateAutomationPeer",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var peer = method?.Invoke(container, null);
            if (peer is null)
            {
                return "null";
            }

            var patterns = peer.GetType()
                .GetInterfaces()
                .Where(static face => face.Name.EndsWith("Provider", StringComparison.Ordinal))
                .Select(static face => face.Name)
                .ToArray();
            return peer.GetType().Name + " implements " + string.Join(", ", patterns.Length == 0 ? ["none"] : patterns);
        }));
    }

    /// <summary>
    /// F. Which switch makes a list's left and right gap equal - the defect the user has reported twice as
    /// "flyout 左右边距不一样长" and which mode D just measured on a list at 3 versus 15. Every variant is our own
    /// template over the same shell, so only the ScrollViewer attributes differ, and the claim is read off the
    /// first container's offset rather than off a property.
    /// </summary>
    private static void Gutters(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== F. the 12 DIP gutter: what makes the two gaps equal ===");
        foreach (var (label, attributes, items) in GutterVariants)
        {
            var list = new ListBox { Width = 260, Height = 180 };
            AddItems(list, items);
            var markup = Shell.Replace("{V}", attributes);
            var template = ParseTemplate(label, markup);
            if (template is null)
            {
                continue;
            }

            list.Template = template;
            root.Children.Add(list);
            window.UpdateLayout();
            Pump(8);
            var first = (FrameworkElement?)FirstOfType(list, typeof(ListBoxItem));
            var at = first is null ? new Point(-1, -1) : first.TranslatePoint(new Point(), list);
            var bars = BarsIn(list);
            Note($"  {label} ({items} items): item=" + (first is null
                    ? "none"
                    : $"{first.ActualWidth:0.##}x{first.ActualHeight:0.##} at {at.X:0.##},{at.Y:0.##}") +
                $" left={at.X:0.##} right={(first is null ? 0 : list.ActualWidth - at.X - first.ActualWidth):0.##}" +
                " bars=" + (bars.Count == 0
                    ? "none"
                    : string.Join(" ", bars.Select(bar => $"{bar.ActualWidth:0.##}x{bar.ActualHeight:0.##}/{bar.Visibility}"))));
        }
    }

    private const string Shell = """
        <ControlTemplate xmlns="http://schemas.jalium.ui/2024" TargetType="ListBox">
          <Border Name="Border" Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}">
            <ScrollViewer Name="SV" Focusable="False" {V}>
              <ItemsPresenter Name="ItemsPresenter" />
            </ScrollViewer>
          </Border>
        </ControlTemplate>
        """;

    private static readonly (string Label, string Attributes, int Items)[] GutterVariants =
    [
        ("no attributes", "", 5),
        ("Auto vertical", "VerticalScrollBarVisibility=\"Auto\"", 5),
        ("Visible vertical", "VerticalScrollBarVisibility=\"Visible\"", 5),
        ("Auto + overlay bars", "VerticalScrollBarVisibility=\"Auto\" IsOverlayScrollBarEnabled=\"True\"", 5),
        ("Auto + autohide", "VerticalScrollBarVisibility=\"Auto\" IsScrollBarAutoHideEnabled=\"True\"", 5),
        ("overlay + autohide", "IsOverlayScrollBarEnabled=\"True\" IsScrollBarAutoHideEnabled=\"True\"", 5),
        ("Auto overflow 50", "VerticalScrollBarVisibility=\"Auto\"", 50),
        ("overlay overflow 50", "VerticalScrollBarVisibility=\"Auto\" IsOverlayScrollBarEnabled=\"True\"", 50),
        ("padding on viewer", "VerticalScrollBarVisibility=\"Auto\" IsOverlayScrollBarEnabled=\"True\" Padding=\"12,10,12,10\"", 5),
        // The two surfaces already shipped (the combo dropdown and the suggestion list) set Hidden, which this
        // sweep had never tried, so the carry-forward fix needs its own row - and the overlay switch's effect
        // under Hidden is what decides whether those templates can be equalized the same way.
        ("Hidden vertical", "VerticalScrollBarVisibility=\"Hidden\"", 50),
        ("Hidden + overlay", "VerticalScrollBarVisibility=\"Hidden\" IsOverlayScrollBarEnabled=\"True\"", 50),
        ("Disabled + overlay", "VerticalScrollBarVisibility=\"Disabled\" IsOverlayScrollBarEnabled=\"True\"", 50),
    ];

    /// <summary>
    /// G. The last question the list batch cannot answer from WPF habits: does an implicit
    /// <c>&lt;Style TargetType="ListBoxItem"&gt;</c> in the theme dictionaries reach the containers the framework
    /// generates, or must the list style also set <c>ItemContainerStyle</c>? Containers are not written by the app,
    /// so nothing in the stock templates proves either way. Each variant carries its own marker colour and padding so
    /// the winning route is attributable to a property read rather than to a look.
    /// </summary>
    private static void ItemStyle(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== G. how a container style arrives: implicit type key, ItemContainerStyle, or both ===");

        var implicitDictionary = (ResourceDictionary?)XamlReader.Parse(ImplicitItemDictionary);
        Note("  implicit dictionary parse: " + Show(implicitDictionary?.GetType().Name) +
             ", app lookup of typeof(ListBoxItem) before merge: " + AppItemStyle());
        var explicitStyle = (Style?)XamlReader.Parse(ExplicitItemStyle);
        Note("  standalone style parse: " + Show(explicitStyle?.GetType().Name));

        // 1: nothing assigned - what the stock container carries on its own.
        var plain = List("plain", root, window);
        Note("  baseline plain: " + ItemLine(plain));

        // 2: implicit style merged at app level, list untouched.
        if (implicitDictionary is not null)
        {
            _application.Resources.MergedDictionaries.Add(implicitDictionary);
        }

        var viaImplicit = List("implicit", root, window);
        Note("  app-implicit only: " + ItemLine(viaImplicit) + " | app lookup after merge: " + AppItemStyle());
        Note("  plain list re-read after the merge (already-realized containers): " + ItemLine(plain));

        // 3: implicit merged AND ItemContainerStyle set - which one paints the container?
        var both = List("both", root, window);
        if (explicitStyle is not null)
        {
            both.ItemContainerStyle = explicitStyle;
        }

        window.UpdateLayout();
        Pump(8);
        Note("  implicit + explicit: " + ItemLine(both));

        if (implicitDictionary is not null)
        {
            _application.Resources.MergedDictionaries.Remove(implicitDictionary);
        }

        // 4: explicit alone, with the dictionaries out of the app again.
        var only = List("explicit", root, window);
        if (explicitStyle is not null)
        {
            only.ItemContainerStyle = explicitStyle;
        }

        window.UpdateLayout();
        Pump(8);
        Note("  explicit only: " + ItemLine(only));
        Note("  implicit dictionary gone, re-read of the implicit list: " + ItemLine(viaImplicit));
    }

    private const string ImplicitItemDictionary = """
        <ResourceDictionary xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
          <Style TargetType="ListBoxItem">
            <Setter Property="Background" Value="#FFAD1457" />
            <Setter Property="Padding" Value="31,7,31,7" />
          </Style>
        </ResourceDictionary>
        """;

    private const string ExplicitItemStyle = """
        <Style xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" TargetType="ListBoxItem">
          <Setter Property="Background" Value="#FF00695C" />
          <Setter Property="Padding" Value="5,1,5,1" />
        </Style>
        """;

    private static ListBox List(string label, Panel root, Window window)
    {
        var list = new ListBox { Width = 260, Height = 180 };
        AddItems(list, 3);
        var template = ParseTemplate(label, Shell.Replace("{V}", "VerticalScrollBarVisibility=\"Auto\" IsOverlayScrollBarEnabled=\"True\""));
        if (template is not null)
        {
            list.Template = template;
        }

        root.Children.Add(list);
        window.UpdateLayout();
        Pump(8);
        return list;
    }

    /// <summary>Whether the application resource space answers the container's type key - the lookup a gate test can use.</summary>
    private static string AppItemStyle() => Read(() =>
        Show(_application.TryFindResource(typeof(ListBoxItem))?.GetType().Name));

    /// <summary>What the first generated container actually carries: the style it was given, and the two markers in it.</summary>
    private static string ItemLine(ListBox list) => Read(() =>
    {
        var item = (FrameworkElement?)FirstOfType(list, typeof(ListBoxItem));
        if (item is null)
        {
            return "no container";
        }

        var style = item.GetType().GetProperty("Style")?.GetValue(item);
        var background = item.GetType().GetProperty("Background")?.GetValue(item) as Brush;
        var padding = item.GetType().GetProperty("Padding")?.GetValue(item);
        return $"style={(style is null ? "null" : "assigned")} bg={(background is null ? "null" : Trim(background.ToString() ?? "?"))}" +
               $" padding={Show(padding)} size={item.ActualWidth:0.##}x{item.ActualHeight:0.##}";
    });

    /// <summary>
    /// H. The rest of the container contract: whether WinUI's <c>ItemContainerTheme</c> exists here at all, whether a
    /// dictionary merged on the <c>Window</c> rather than the application reaches containers, and whether the type key
    /// is what a test can assert on.
    /// </summary>
    private static void ContainerTheme(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== H. container-route leftovers: ItemContainerTheme, window-scope dictionaries, type-key lookup ===");
        Note("  ItemContainerTheme on ListBox: " + Read(() =>
            typeof(ListBox).GetProperty("ItemContainerTheme") is null
                ? "not exported"
                : typeof(ListBox).GetProperty("ItemContainerTheme")!.PropertyType.Name));
        Note("  ItemContainerStyle declared on: " + Read(() => typeof(ListBox)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .FirstOrDefault(static property => property.Name == "ItemContainerStyle")?.DeclaringType?.Name ?? "inherited, not declared"));

        var list = List("window-scope", root, window);
        var dictionary = (ResourceDictionary?)XamlReader.Parse(ImplicitItemDictionary);
        Note("  before: " + ItemLine(list));
        if (dictionary is not null)
        {
            window.Resources.MergedDictionaries.Add(dictionary);
        }

        var fresh = List("after-window-merge", root, window);
        Note("  window-merged, new list: " + ItemLine(fresh));
        Note("  window-merged, list mounted before the merge: " + ItemLine(list));
        if (dictionary is not null)
        {
            window.Resources.MergedDictionaries.Remove(dictionary);
        }

        var after = List("after-removal", root, window);
        Note("  after removing the window dictionary: " + ItemLine(after));
        Note("  TryFindResource(typeof(ListBoxItem)) on the container: " + Read(() =>
        {
            var item = FirstOfType(after, typeof(ListBoxItem));
            var method = item?.GetType().GetMethod("TryFindResource", new[] { typeof(object) });
            return method is null
                ? "no TryFindResource(object) to call"
                : Show(method.Invoke(item, [typeof(ListBoxItem)])?.GetType().Name);
        }));
    }

    /// <summary>
    /// I. The item template itself: can ListBoxItem take ours at all (the lock question mode C answered for the
    /// list), does the generated container still show its content, what does selection write - and does the
    /// framework put a LOCAL value on the container when an item is selected, which is the landmine the ComboBox
    /// batch stepped on (a local value outranks every setter and cell).
    /// </summary>
    private static void ItemTemplate(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== I. our own ListBoxItem template on a generated container ===");

        // Stock container first, so the selected-state reads have something to be compared against. Index 0 is
        // selected on purpose: every read below takes the FIRST container, and with a virtualizing panel that is
        // item 0 - selecting another row would leave the read looking at an untouched item.
        var stock = List("stock-item", root, window);
        Note("  stock resting: " + ItemLine(stock) + " | fill part: " + PartBrush(stock, "PART_BackgroundBorder"));
        Try(() => stock.SelectedIndex = 0);
        window.UpdateLayout();
        Pump(6);
        Note("  stock selected: " + ItemLine(stock) + " | fill part: " + PartBrush(stock, "PART_BackgroundBorder") +
             " | list SelectedIndex=" + Read(() => Show(stock.GetType().GetProperty("SelectedIndex")?.GetValue(stock))) +
             " | container IsSelected=" + ContainerSelected(stock));

        var dictionary = (ResourceDictionary?)XamlReader.Parse(ItemTemplateDictionary);
        Note("  item template dictionary parse: " + Show(dictionary?.GetType().Name));
        if (dictionary is not null)
        {
            _application.Resources.MergedDictionaries.Add(dictionary);
        }

        var list = List("our-item", root, window);
        window.UpdateLayout();
        Pump(8);
        var container = (FrameworkElement?)FirstOfType(list, typeof(ListBoxItem));
        Note("  ours resting: " + ItemLine(list) + " | highlight part: " + PartBrush(list, "LayoutRoot"));
        if (container is not null)
        {
            PrintTree(container, "      ");
        }

        Try(() => list.SelectedIndex = 0);
        window.UpdateLayout();
        Pump(6);
        Note("  ours selected: " + ItemLine(list) + " | highlight part: " + PartBrush(list, "LayoutRoot") +
             " | container IsSelected=" + ContainerSelected(list));
        Note("  container surface after select: is-selected=" + Read(() => Show(container?.GetType()
                .GetProperty("IsSelected")?.GetValue(container))) +
             " content=" + Read(() => Show(((ContentPresenter?)FirstOfType(list!, typeof(ContentPresenter)))?.Content)) +
             " text=" + Read(() => Show(((TextBlock?)FirstOfType(list!, typeof(TextBlock)))?.Text)) +
             " padding=" + Read(() => Show(container?.GetType().GetProperty("Padding")?.GetValue(container))) +
             " corner=" + Read(() => Show(container?.GetType().GetProperty("CornerRadius")?.GetValue(container))) +
             " min-height=" + Read(() => Show(container?.GetType().GetProperty("MinHeight")?.GetValue(container))));
        var at = container is null ? new Point(-1, -1) : container.TranslatePoint(new Point(), list);
        Note("  geometry: list=" + $"{list.ActualWidth:0.##}" +
             " item=" + (container is null ? "none" : $"{container.ActualWidth:0.##}x{container.ActualHeight:0.##}") +
             " left=" + $"{at.X:0.##}" +
             " right=" + (container is null ? "0" : $"{list.ActualWidth - at.X - container.ActualWidth:0.##}"));
        Note("  highlight fill geometry: " + PartGeometry(list, "LayoutRoot"));
        Note("  IsHighlighted on ListBoxItem: " + Read(() => Show(typeof(ListBoxItem).GetProperty("IsHighlighted")?.Name ?? "not exported")));
        Note("  IsSelectionActive on ListBoxItem: " + Read(() => Show(typeof(ListBoxItem).GetProperty("IsSelectionActive")?.Name ?? "not exported")));
        Note("  IsPressed on ListBoxItem: " + Read(() => Show(typeof(ListBoxItem).GetProperty("IsPressed")?.Name ?? "not exported")));
        if (dictionary is not null)
        {
            _application.Resources.MergedDictionaries.Remove(dictionary);
        }
    }

    private const string ItemTemplateDictionary = """
        <ResourceDictionary xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
          <Style TargetType="ListBoxItem">
            <Setter Property="Background" Value="Transparent" />
            <Setter Property="BorderThickness" Value="1" />
            <Setter Property="Padding" Value="12,9,12,12" />
            <Setter Property="CornerRadius" Value="4" />
            <Setter Property="MinHeight" Value="34" />
            <Setter Property="HorizontalContentAlignment" Value="Stretch" />
            <Setter Property="VerticalContentAlignment" Value="Center" />
            <Setter Property="Template">
              <ControlTemplate TargetType="ListBoxItem">
                <Grid>
                  <Border Name="LayoutRoot" Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}" CornerRadius="{TemplateBinding CornerRadius}" />
                  <ContentPresenter Name="ContentPresenter" Content="{TemplateBinding Content}" Margin="{TemplateBinding Padding}" />
                </Grid>
                <ControlTemplate.Triggers>
                  <Trigger Property="IsSelected" Value="True">
                    <Setter TargetName="LayoutRoot" Property="Background" Value="#FF0078D4" />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>
        </ResourceDictionary>
        """;

    /// <summary>The container's own selection flag - the only signal an item template cell can condition on.</summary>
    private static string ContainerSelected(ListBox list) => Read(() =>
        Show(((FrameworkElement?)FirstOfType(list, typeof(ListBoxItem)))?.GetType()
            .GetProperty("IsSelected")?.GetValue(FirstOfType(list, typeof(ListBoxItem)))));

    /// <summary>The brush a named template part is carrying, read off the mounted container.</summary>
    private static string PartBrush(ListBox list, string partName) => Read(() =>
    {
        var part = FindNamed(list, partName);
        return part is null
            ? $"no part named {partName}"
            : Show(part.GetType().GetProperty("Background")?.GetValue(part) is Brush brush
                ? Trim(brush.ToString() ?? "?")
                : "null");
    });

    private static string PartGeometry(ListBox list, string partName) => Read(() =>
    {
        var part = (FrameworkElement?)FindNamed(list, partName);
        return part is null
            ? $"no part named {partName}"
            : $"{partName} {part.ActualWidth:0.##}x{part.ActualHeight:0.##} at " + OffsetOf(part, list);
    });

    private static FrameworkElement? FindNamed(DependencyObject root, string name)
    {
        object? found = null;
        Walk(root, node =>
        {
            if (found is null && NameOf(node) == name)
            {
                found = node;
            }
        }, 40);
        return found as FrameworkElement;
    }

    /// <summary>
    /// J. Whether a style can carry the scroll-bar switches at all: an attached-name setter
    /// (<c>ScrollViewer.IsOverlayScrollBarEnabled</c>) is how upstream's list style hands those to its template,
    /// and this reader is known to drop markup it cannot resolve without a word. Two template shapes are tried -
    /// literal attributes on the part, and <c>{TemplateBinding ScrollViewer.X}</c> - against a style that does and
    /// does not set the attached properties, so a silent drop cannot be mistaken for a value that arrived.
    /// </summary>
    private static void AttachedSetters(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== J. attached ScrollViewer setters: do they reach the part? ===");
        var bound = ParseTemplate("bound shell", BoundShell);
        var literal = ParseTemplate("literal shell", Shell.Replace("{V}", "VerticalScrollBarVisibility=\"Auto\" IsOverlayScrollBarEnabled=\"False\""));
        var styleWith = XamlReader.Parse(AttachedStyleWith) as Style;
        var styleWithout = XamlReader.Parse(AttachedStyleWithout) as Style;
        Note("  bound shell parse: " + Show(bound?.GetType().Name) + " | literal shell parse: " + Show(literal?.GetType().Name));
        Note("  style with attached setters parse: " + Show(styleWith?.GetType().Name) +
             " | style without: " + Show(styleWithout?.GetType().Name));

        foreach (var (label, template, style) in new (string, ControlTemplate?, Style)[]
                 {
                     ("bound + setters", bound, styleWith),
                     ("bound + no setters", bound, styleWithout),
                     ("literal + setters", literal, styleWith),
                     ("literal + no setters", literal, styleWithout),
                 })
        {
            var list = new ListBox { Width = 260, Height = 180 };
            AddItems(list, 50);
            if (template is not null)
            {
                list.Template = template;
            }

            if (style is not null)
            {
                list.Style = style;
            }

            root.Children.Add(list);
            window.UpdateLayout();
            Pump(8);
            var first = (FrameworkElement?)FirstOfType(list, typeof(ListBoxItem));
            var at = first is null ? new Point(-1, -1) : first.TranslatePoint(new Point(), list);
            Note($"  {label}: " + ViewerReads(list) +
                 " item=" + (first is null ? "none" : $"{first.ActualWidth:0.##}") +
                 $" left={at.X:0.##} right={(first is null ? 0 : list.ActualWidth - at.X - first.ActualWidth):0.##}");
        }
    }

    private const string BoundShell = """
        <ControlTemplate xmlns="http://schemas.jalium.ui/2024" TargetType="ListBox">
          <Border Name="Border" Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}">
            <ScrollViewer Name="SV" Focusable="False"
                          VerticalScrollBarVisibility="{TemplateBinding ScrollViewer.VerticalScrollBarVisibility}"
                          IsOverlayScrollBarEnabled="{TemplateBinding ScrollViewer.IsOverlayScrollBarEnabled}">
              <ItemsPresenter Name="ItemsPresenter" />
            </ScrollViewer>
          </Border>
        </ControlTemplate>
        """;

    private const string AttachedStyleWith = """
        <Style xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" TargetType="ListBox">
          <Setter Property="ScrollViewer.VerticalScrollBarVisibility" Value="Auto" />
          <Setter Property="ScrollViewer.IsOverlayScrollBarEnabled" Value="True" />
        </Style>
        """;

    private const string AttachedStyleWithout = """
        <Style xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" TargetType="ListBox">
          <Setter Property="Padding" Value="0" />
        </Style>
        """;

    private static string ViewerReads(ListBox list) => Read(() =>
    {
        var viewer = (FrameworkElement?)FirstOfType(list, typeof(ScrollViewer));
        if (viewer is null)
        {
            return "no ScrollViewer part";
        }

        var type = viewer.GetType();
        string Get(string name) => Show(type.GetProperty(name)?.GetValue(viewer) ?? "no such property");
        return $"viewer.{Get("VerticalScrollBarVisibility")}/{Get("IsOverlayScrollBarEnabled")} style={(list.Style is null ? "null" : "assigned")}";
    });

    // ---------- helpers ----------

    private static void AddItems(ListBox list, int count)
    {
        for (var index = 0; index < count; index++)
        {
            list.Items.Add($"Item {index + 1}");
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

    private static object? FindOfType(DependencyObject root, Type type) => FirstOfType(root, type);

    /// <summary>Every element whose type name says ScrollBar, found without naming the type at compile time.</summary>
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

    private static string Name(ListBox list, FrameworkElement element) =>
        element.GetType().GetProperty("Background")?.GetValue(element) is Brush brush
            ? Trim(brush.ToString() ?? "?")
            : "?";

    private static string OffsetOf(FrameworkElement node, UIElement to)
    {
        try
        {
            var origin = node.TranslatePoint(new Point(0, 0), to);
            return $"x={origin.X:0.##} y={origin.Y:0.##}";
        }
        catch (Exception exception)
        {
            return "threw " + (exception.InnerException ?? exception).GetType().Name;
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

    private static int VisualChildren(DependencyObject node)
    {
        try
        {
            return VisualTreeHelper.GetChildrenCount(node);
        }
        catch
        {
            return -1;
        }
    }

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
