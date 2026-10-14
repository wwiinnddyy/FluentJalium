using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace DataGridProbe;

/// <summary>
/// The questions the DataGrid / TreeDataGrid slice cannot write product code without (stage 5, fifth slice).
/// <c>adaptation/00</c> S0-s records that <c>ThemeManager.RegisterBuiltins</c> ships a default style for
/// TreeDataGrid and NOT for DataGrid, and reads that as "DataGrid has no template to hand back". Nobody in this
/// repo has ever mounted a DataGrid, so that is an unmeasured inference about the one control family whose
/// appearance matters most - and S0-s's own window-shell row is the cautionary case: it was written, fed a
/// key-deletion rationale, and reversed a session later after the shell was actually measured. The TabView batch
/// (S1-h) made the same question decisive again: <c>TabControl</c> has a <c>Template</c> property and takes no
/// template, because the parts are built in code. So every route below is decided by what a mounted control
/// reports about itself, never by the type graph.
///   A mount   : does an unmounted DataGrid have Template/Style, what does a mounted one report, and is the visual
///               tree a real control template or a code-built graph? Positive controls: ListBox (takes our template,
///               S1-c) and TabControl (does not, S1-h) measured in the same run and the same window.
///   B retmpl  : can a DataGrid be handed a .jalxaml ControlTemplate at all - implicit style, explicit Style with a
///               Template setter, and a direct Template local value - and does the tree then come from ours?
///               This is the "native re-template or own type" gate for the whole slice.
///   C keys    : every {ThemeResource} name the framework's own DataGrid / TreeDataGrid templates consume - the only
///               reading that answers "is this control themeable" (S0-s method rule: enumerate the names the
///               framework's style consumes, do not ask whether our dictionary has a row for it).
///   D tree    : TreeDataGrid, which S0-s says DOES have a registered default style: same four readings, plus which
///               nodes/rows the framework builds.
///   E gridline/column parts: where a header, a grid line and a resize drag live, and whether a GridSplitter type
///               exists at all (WinUI puts column resize on the header; our Button batch already knows a
///               RepeatButton is what this runtime drives).
/// Reflection lives here and only here; the structural gates in AstraGateTests keep it out of src/FluentJalium.
/// Modes: types | mount | retmpl | keys | tree | parts | all.
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
            // pass 2 boots without Astra's dictionaries on purpose: the framework's own grid theme is the baseline
            // the readings are about, and Apply is called from inside the mode so the same instance can be re-read
            // after it lands.
            if (_mode is not ("pass2" or "pass3" or "content" or "feedbare"))
            {
                FluentThemeManager.Apply(application);
            }
            Run(application);
        }
        catch (Exception exception)
        {
            Note("BOOT threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }

        var path = Path.Combine(AppContext.BaseDirectory, $"datagrid-probe-{_mode}.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, mode={_mode}, full log: {path}");
        return 0;
    }

    private static void Run(Application application)
    {
        var root = new StackPanel { Margin = new Thickness(24) };
        Root = root;
        var window = new Window { Content = root, Width = 900, Height = 700, Title = "DataGrid probe" };
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
                    MountShape(root, window);
                }

                if (_mode is "all" or "retmpl")
                {
                    Retemplate(root, window);
                }

                if (_mode is "all" or "keys")
                {
                    ThemeKeys();
                }

                if (_mode is "all" or "tree")
                {
                    TreeDataGridShape(root, window);
                }

                if (_mode is "all" or "feed" or "feedbare")
                {
                    TreeDataGridFeed(root, window);
                }

                if (_mode is "cols")
                {
                    ColumnWidths(root, window);
                }

                if (_mode is "all" or "parts")
                {
                    ColumnParts(root, window);
                }

                if (_mode is "pass2")
                {
                    TokenRoute(root, window);
                    OverrideCost(root, window);
                }

                if (_mode is "cells" or "content")
                {
                    CellContent();
                }

                if (_mode is "pass3")
                {
                    StyleOwnership(root, window, "before Astra");
                    Note($"  FluentThemeManager.Apply -> {Try(() => FluentThemeManager.Apply(_application))}");
                    Pump(6);
                    StyleOwnership(root, window, "after Astra");
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

    // ---------- A. what the runtime exports for grids ----------

    private static void TypeCensus()
    {
        Note("");
        Note("=== A. grid types ===");
        var assemblies = LoadedAssemblies();
        Note($"assemblies={string.Join(", ", assemblies.Select(a => a.GetName().Name))}");
        foreach (var assembly in assemblies)
        {
            var hits = SafeTypes(assembly)
                .Where(t => !t.IsNested && (t.Name.Contains("DataGrid", StringComparison.Ordinal) ||
                                             t.Name.Contains("GridSplitter", StringComparison.Ordinal) ||
                                             t.Name.Contains("TreeDataGrid", StringComparison.Ordinal)))
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .ToList();
            if (hits.Count == 0)
            {
                continue;
            }

            Note($"{assembly.GetName().Name}: {hits.Count}");
            foreach (var type in hits)
            {
                Note($"  {type.FullName}  abstract={type.IsAbstract} public={type.IsPublic}  chain={Chain(type)}");
            }
        }

        foreach (var name in new[] { "DataGrid", "DataGridColumn", "DataGridTextColumn", "DataGridCell", "DataRow", "ColumnHeader", "GridSplitter", "TreeDataGrid", "TreeNode" })
        {
            var type = TypeByName(name);
            Note($"lookup {name}: {(type?.FullName ?? "NOT EXPORTED")}");
        }
    }

    // ---------- B. the headline reading: what a mounted DataGrid reports ----------

    private static void MountShape(Panel root, Window window)
    {
        Note("");
        Note("=== B. mount shape: does a DataGrid have a replaceable template? ===");
        var gridType = TypeByName("DataGrid");
        if (gridType is null)
        {
            Note("DataGrid NOT EXPORTED - nothing else in this section can run.");
            return;
        }

        // Unmounted: what does the type hand back before it is ever placed? S0-s predicts "no template" because no
        // builtin style is registered for it. The NumberBox batch (adaptation/00) proved Style==null and
        // Template!=null can coexist, so both are read, and LocalValue reads decide whether the framework wrote
        // them or a style did.
        var bare = Read(() => Activator.CreateInstance(gridType));
        if (bare is null || bare is string)
        {
            Note($"  new DataGrid() THREW/absent: {bare}");
            return;
        }

        Note($"  unmounted new DataGrid(): Template={(Prop(bare, "Template") is null ? "null" : "SET")} " +
             $"Style={(Prop(bare, "Style") is null ? "null" : "SET")} " +
             $"localTemplate={HasLocal(bare, "Template")} localStyle={HasLocal(bare, "Style")}");

        // Positive controls, in the same window and the same frame pump, so a null reading cannot be blamed on
        // the harness: ListBox takes our template (S1-c) and TabControl refuses every template (S1-h).
        foreach (var (label, factory) in new (string, Func<object?>)[]
                 {
                     ("ListBox", () => new ListBox { ItemsSource = new[] { "a", "b" } }),
                     ("TabControl", () => new TabControl { ItemsSource = new[] { "a", "b" } }),
                 })
        {
            var control = factory();
            if (control is not null)
            {
                Note($"  control {label} unmounted: Template={(Prop(control, "Template") is null ? "null" : "SET")} Style={(Prop(control, "Style") is null ? "null" : "SET")}");
            }
        }

        // Mounted with real columns and rows. Two routes because the runtime may or may not auto-generate: an
        // explicit column collection when the type offers one, and AutoGenerateColumns otherwise.
        var grid = Read(() => Activator.CreateInstance(gridType));
        if (grid is null or string)
        {
            return;
        }

        Note($"  set AutoGenerateColumns=true -> {Set(grid, "AutoGenerateColumns", true)}");
        Note($"  set ItemsSource=3 rows -> {Set(grid, "ItemsSource", Rows(3))}");
        Note($"  columns route -> {AddColumns(grid)}");
        Note($"  set Width/Height=560x180 -> {Set(grid, "Width", 560d)}, {Set(grid, "Height", 180d)}");
        var element = (FrameworkElement)grid!;
        ((Panel)root).Children.Add(element);
        var frames = Pump(10);
        Note($"  mounted after {frames} frames: rendered={Rendered(element)} size={SizeOf(element)}");
        Note($"  mounted: Template={(Prop(element, "Template") is null ? "null" : "SET")} Style={(Prop(element, "Style") is null ? "null" : "SET")} localTemplate={HasLocal(element, "Template")}");
        var style = Prop(element, "Style");
        Note($"  mounted style: {(style is null ? "null" : $"type={style.GetType().Name} x:{Prop(style, "Name")}/{Prop(style, "Key")} target={Prop(style, "TargetType")}")}");
        var template = Prop(element, "Template") as ControlTemplate;
        if (template is null)
        {
            Note("  mounted template: null - the framework builds no ControlTemplate object for DataGrid.");
        }
        else
        {
            var xaml = TemplateXaml(template);
            Note($"  mounted template: type={template.GetType().Name} VisualTreeXaml chars={(xaml.Length)} triggers={Read(() => template.Triggers.Count)}");
            Note("  template xaml follows:");
            foreach (var line in Wrap(xaml, 150))
            {
                Note2(line);
            }
        }

        Note("  visual tree:");
        PrintTree(element, "    ", 14);
        Note($"  named parts: {PartNames(element)}");
        var rows = CountTypeName(element, "DataGridRow");
        var cells = CountTypeName(element, "DataGridCell");
        var presenters = CountTypeName(element, "Presenter");
        Note($"  counts: DataGridRow={rows} DataGridCell={cells} *Presenter*={presenters} ScrollViewer={CountTypeName(element, "ScrollViewer")}");
        root.Children.Remove(element);
        Pump(2);
    }

    // ---------- C. can it be handed OUR template? ----------

    private static void Retemplate(Panel root, Window window)
    {
        Note("");
        Note("=== C. re-templatability: three routes, same frame pump ===");
        var gridType = TypeByName("DataGrid");
        if (gridType is null)
        {
            Note("DataGrid NOT EXPORTED.");
            return;
        }

        // A trivially identifiable marker brush is painted by our template's root so a pixel or a tree reading can
        // tell "ours ran" from "the framework kept its own graph".
        var markup = @"
<ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <ControlTemplate x:Key='ProbeGridTemplate'>
    <Border x:Name='ProbeTemplateRoot' Background='#FFFF00FF' CornerRadius='4'>
      <StackPanel>
        <TextBlock x:Name='PART_HeaderHost' Text='PROBE-HEADER'/>
        <ItemsPresenter x:Name='ProbeItems'/>
      </StackPanel>
    </Border>
  </ControlTemplate>
</ResourceDictionary>";
        var dictionary = ParseDictionary(markup);
        var template = dictionary?["ProbeGridTemplate"] as ControlTemplate;
        Note($"  probe template parsed: {(template is null ? "FAILED" : "ok")} triggers={Read(() => template!.Triggers.Count)}");
        if (template is null)
        {
            return;
        }

        // route 1: implicit application-level style (the mechanism Astra's whole library rides on, S0-b).
        var style = new Style(gridType);
        style.Setters.Add(new Setter(Control.TemplateProperty, template));
        _application.Resources[gridType] = style;
        var one = NewGrid();
        if (one is not null)
        {
            MountAndReport("route1 implicit app style", one, root);
        }

        _application.Resources.Remove(gridType);
        Pump(2);

        // route 2: explicit Style with a Template setter.
        var two = NewGrid();
        if (two is not null)
        {
            Note($"    style assignment -> {Set(two, "Style", style)}");
            MountAndReport("route2 explicit style + template setter", two, root);
        }

        // route 3: Template as a local value.
        var three = NewGrid();
        if (three is not null)
        {
            Note($"    template assignment -> {Set(three, "Template", template)}");
            MountAndReport("route3 template local value", three, root);
        }

        // Positive control in the same run: a ListBox takes the same kind of template (S1-c), so a null here
        // cannot be blamed on the markup or the parser.
        var four = new ListBox { ItemsSource = new[] { "a", "b" } };
        four.Template = template;
        MountAndReport("control ListBox + same template", four, root);
    }

    private static void MountAndReport(string label, object candidate, Panel root)
    {
        Note($"  --- {label} ---");
        var element = (FrameworkElement)candidate;
        root.Children.Add(element);
        var frames = Pump(8);
        var template = Prop(element, "Template") as ControlTemplate;
        Note($"    after {frames} frames: Template={(template is null ? "null" : "SET")} localTemplate={HasLocal(element, "Template")} rendered={Rendered(element)} size={SizeOf(element)}");
        var marker = FirstNamed(element, "ProbeTemplateRoot");
        Note($"    our marker ProbeTemplateRoot present={marker is not null} type={marker?.GetType().Name ?? "-"}");
        var items = FirstNamed(element, "ProbeItems");
        Note($"    our ItemsPresenter ProbeItems present={items is not null}");
        Note($"    named parts: {PartNames(element)}");
        Note($"    counts: DataGridRow={CountTypeName(element, "DataGridRow")} DataGridCell={CountTypeName(element, "DataGridCell")} ScrollViewer={CountTypeName(element, "ScrollViewer")} TextBlock={CountTypeName(element, "TextBlock")}");
        Note("    tree:");
        PrintTree(element, "      ", 12);
        root.Children.Remove(element);
        Pump(2);
    }

    private static object? NewGrid()
    {
        var gridType = TypeByName("DataGrid");
        var grid = Read(() => Activator.CreateInstance(gridType!));
        if (grid is null or string)
        {
            Note("    new DataGrid() THREW/absent: " + grid);
            return null;
        }

        _ = Set(grid, "AutoGenerateColumns", true);
        _ = Set(grid, "ItemsSource", Rows(2));
        _ = Set(grid, "Width", 480d);
        _ = Set(grid, "Height", 140d);
        return grid;
    }

    // ---------- D. which palette names does the framework consume? ----------

    private static void ThemeKeys()
    {
        Note("");
        Note("=== D. {ThemeResource} names consumed by the framework's own grid templates ===");
        foreach (var name in new[] { "DataGrid", "TreeDataGrid", "DataGridCell", "DataGridColumnHeader", "ColumnHeader", "DataGridRow", "RowHeader", "DataGridSplitter", "GridSplitter" })
        {
            var type = TypeByName(name);
            if (type is null)
            {
                Note($"  {name}: NOT EXPORTED");
                continue;
            }

            var control = Read(() => Activator.CreateInstance(type));
            if (control is null or string)
            {
                Note($"  {name}: new() -> {control}");
                continue;
            }

            var template = Prop(control, "Template") as ControlTemplate;
            if (template is null)
            {
                Note($"  {name}: unmounted Template=null (chain {Chain(type)})");
                continue;
            }

            var xaml = TemplateXaml(template);
            Note($"  {name}: unmounted Template SET, {xaml.Length} chars");
            foreach (var key in ThemeResourceNames(xaml).OrderBy(k => k, StringComparer.Ordinal))
            {
                Note2($"{key} -> resolves={(ApplicationResource(key) is null ? "MISSING" : "present")}");
            }
        }
    }

    private static IEnumerable<string> ThemeResourceNames(string text) =>
        Regex.Matches(text, @"\{ThemeResource\s+([A-Za-z0-9_.]+)\}").Select(m => m.Groups[1].Value).Distinct(StringComparer.Ordinal);

    // ---------- E. TreeDataGrid, which S0-s says has a registered style ----------

    private static void TreeDataGridShape(Panel root, Window window)
    {
        Note("");
        Note("=== E. TreeDataGrid ===");
        var type = TypeByName("TreeDataGrid");
        if (type is null)
        {
            Note("TreeDataGrid NOT EXPORTED.");
            return;
        }

        Note($"  chain={Chain(type)}");
        var control = Read(() => Activator.CreateInstance(type));
        if (control is null or string)
        {
            Note($"  new TreeDataGrid() -> {control}");
            return;
        }

        Note($"  unmounted: Template={(Prop(control, "Template") is null ? "null" : "SET")} Style={(Prop(control, "Style") is null ? "null" : "SET")}");
        _ = Set(control, "AutoGenerateColumns", true);
        _ = Set(control, "ItemsSource", Rows(2));
        _ = Set(control, "Width", 480d);
        _ = Set(control, "Height", 160d);
        var element = (FrameworkElement)control;
        root.Children.Add(element);
        var frames = Pump(10);
        var style = Prop(element, "Style");
        Note($"  mounted after {frames}: Template={(Prop(element, "Template") is null ? "null" : "SET")} " +
             $"Style={(style is null ? "null" : $"key={Show(Prop(style, "Name"))}/{Show(Prop(style, "Key"))} target={Show(Prop(style, "TargetType"))}")}");
        var template = Prop(element, "Template") as ControlTemplate;
        if (template is not null)
        {
            var xaml = TemplateXaml(template);
            Note($"  mounted template: {xaml.Length} chars, triggers={Read(() => template.Triggers.Count)}");
            foreach (var line in Wrap(xaml, 150))
            {
                Note2(line);
            }
        }
        else
        {
            Note("  mounted template: null");
        }

        Note("  tree:");
        PrintTree(element, "    ", 14);
        Note($"  named parts: {PartNames(element)}");
        Note($"  counts: TreeDataGridRow={CountTypeName(element, "TreeDataGridRow")} Cell={CountTypeName(element, "Cell")} Expander={CountTypeName(element, "Expander")} ToggleButton={CountTypeName(element, "ToggleButton")} ScrollViewer={CountTypeName(element, "ScrollViewer")}");
        root.Children.Remove(element);
        Pump(2);
    }

    // ---------- H. TreeDataGrid feed path ----------

    /// <summary>
    /// audits/datagrid.md §4-5 closed the previous slice on an inference: "TreeDataGridNode is internal, so how
    /// hierarchical data gets in is unmeasured". That inference has a decisive consequence - if a consumer cannot
    /// build the control's data, the control cannot be styled, galleryed or tested and the whole row would have to
    /// be declared out of scope. The reference tree claims the feed is a plain IEnumerable plus a children path,
    /// with the node type staying private; the shipped assembly is the authority, so the names are read here.
    /// Markup and Bindings are used because that is the shape Gallery will mount, and a bare object handed to a
    /// probe measures a different code path (the keyed-template lesson).
    /// </summary>
    private static void TreeDataGridFeed(Panel root, Window window)
    {
        Note("");
        Note("=== H. TreeDataGrid feed path ===");
        var type = TypeByName("TreeDataGrid");
        if (type is null)
        {
            Note("TreeDataGrid NOT EXPORTED.");
            return;
        }

        static string Names(IEnumerable<string> values) => Join(values.OrderBy(n => n, StringComparer.Ordinal));
        Note($"  declared public props: {Names(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(p => $"{p.Name}:{p.PropertyType.Name}"))}");
        Note($"  declared public DPs: {Names(DeclaredDpNames(type))}");
        Note($"  expand/collapse members: {Join(type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name.Contains("Expand", StringComparison.Ordinal) || m.Name.Contains("Collapse", StringComparison.Ordinal)).Select(m => $"{m.Name}({string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name))})").Distinct(StringComparer.Ordinal).OrderBy(n => n, StringComparer.Ordinal))}");
        Note($"  public events: {Names(type.GetEvents(BindingFlags.Public | BindingFlags.Instance).Select(e => e.Name))}");

        var node = TypeByName("TreeDataGridNode");
        Note($"  TreeDataGridNode: exported={node is not null} public={(node is null ? "-" : node.IsPublic.ToString().ToLowerInvariant())} publicCtors={(node?.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length ?? -1)}");
        var rowType = TypeByName("TreeDataGridRow");
        if (rowType is null)
        {
            Note("  TreeDataGridRow NOT EXPORTED - no container to build, so the feed cannot be measured.");
            return;
        }

        Note($"  TreeDataGridRow ({Chain(rowType)}) declared public props: {Names(rowType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(p => $"{p.Name}:{p.PropertyType.Name}"))}");
        Note($"  TreeDataGridRow expand members: {Join(rowType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name.Contains("Expand", StringComparison.Ordinal) || m.Name.Contains("Collapse", StringComparison.Ordinal)).Select(m => m.Name).Distinct(StringComparer.Ordinal))}");

        // Mount in Gallery's shape: columns and bindings in markup, only the rows coming from code. The namespace
        // matters and was measured, not copied from the other probes: with
        // 'https://schemas.jalium.dev/jalxaml/presentation' the TreeDataGrid itself resolves but
        // DataGridTextColumn throws XamlParseException "Cannot resolve type", while Gallery's
        // 'http://schemas.jalium.ui/2024' resolves both. Two "default" namespaces, two type tables.
        const string Markup = """
            <TreeDataGrid xmlns='http://schemas.jalium.ui/2024'
                          AutoGenerateColumns='False' TreeColumnIndex='0' Width='460' Height='220'>
              <TreeDataGrid.Columns>
                <DataGridTextColumn Header='Name' Binding='{Binding Name}' Width='220' />
                <DataGridTextColumn Header='Owner' Binding='{Binding Owner}' Width='140' />
              </TreeDataGrid.Columns>
            </TreeDataGrid>
            """;
        var parsed = Read(() => XamlReader.Parse(Markup));
        var element = parsed as FrameworkElement;
        if (element is null)
        {
            Note($"  markup parse -> {Text(() => parsed)}");
            return;
        }

        Note($"  parsed: {element.GetType().Name}");
        Note($"  columns from markup: {Show(Prop(Prop(element, "Columns"), "Count"))}");

        // Does the parser honour a name the type does not have? TreeDataGrid has no AutoGenerateColumns property in
        // this build (its 26 declared names are above), yet the markup carries the attribute and parsed without an
        // error - so either the parser drops unknown names silently or the attribute lands elsewhere. A number on a
        // column's Width behaving like this is the reason the question matters.
        const string UnknownMarkup = """
            <TreeDataGrid xmlns='http://schemas.jalium.ui/2024' DefinitelyNotAProperty='42' Width='460' Height='60'>
              <TreeDataGrid.Columns>
                <DataGridTextColumn Header='Name' Binding='{Binding Name}' MinWidth='200' />
              </TreeDataGrid.Columns>
            </TreeDataGrid>
            """;
        var unknown = Read(() => XamlReader.Parse(UnknownMarkup));
        Note($"  unknown attribute markup -> {(unknown is string failure ? failure : $"{unknown.GetType().Name}, columns={Show(Prop(Prop(unknown, "Columns"), "Count"))}")}");
        if (unknown is not string)
        {
            root.Children.Add((FrameworkElement)unknown);
            Pump(8);
            Note($"    unknown-attribute grid mounted: size={SizeOf((FrameworkElement)unknown)} cells={CountTypeName((FrameworkElement)unknown, "DataGridCell")}");
            root.Children.Remove((FrameworkElement)unknown);
            Pump(2);
        }

        foreach (var path in new[] { "ChildrenPropertyPath", "ChildrenSelector", "HasChildrenPath", "HasChildrenSelector", "Indentation", "ChildItemsPath" })
        {
            Note($"  set {path}=\"Children\" -> {Set(element, path, "Children")}");
        }

        _ = Set(element, "ItemsSource", TreeNodes());
        root.Children.Add(element);
        var frames = Pump(12);
        Note($"  mounted after {frames} frames: size={SizeOf(element)} template={(Prop(element, "Template") is null ? "null" : "SET")}");
        Note($"  parts: {PartNames(element)}");
        var rows = AllOfType(element, rowType).OfType<FrameworkElement>().ToList();
        Note($"  TreeDataGridRow count={rows.Count} (2 roots + 4 children if the path landed)");
        foreach (var row in rows)
        {
            var border = FirstNamed(row, "PART_RowBorder");
            Note($"    row \"{Show(Prop(row, "Content"))}\" index={Show(Prop(row, "Index"))} level={Show(Prop(row, "Level"))} " +
                 $"height={SizeOf(row)} localBG={HasLocal(row, "Background")} bg={Hex(Prop(row, "Background") as Brush)} " +
                 $"borderBG={(border is null ? "-" : Hex(Prop(border, "Background") as Brush))} margin={Show(Prop(row, "Margin"))}");
            Note($"      parts={PartNames(row)} cells={CountTypeName(row, "DataGridCell")} path={CountTypeName(row, "Path")} toggle={CountTypeName(row, "ToggleButton")} expander={CountTypeName(row, "Expander")}");
        }

        // The defect this mode exists to locate: the rows realize, the text measures, but every cell arranges to
        // ActualWidth 0. Either the column never got an ActualWidth (framework layout) or our implicit DataGridCell
        // / DataGridColumnHeader styles - which TreeDataGrid reuses, because the framework has no tree-only cell -
        // are what collapse it. Both are read here, and the same mode runs a second time with Astra off.
        Note($"  astra dictionaries={Text(() => FluentThemeManager.DictionaryNames.Count.ToString())}");
        var columns = Prop(element, "Columns");
        var columnCount = Prop(columns, "Count") is int length ? length : 0;
        var indexer = columns?.GetType().GetProperty("Item");
        for (var index = 0; index < columnCount; index++)
        {
            var column = Read(() => indexer?.GetValue(columns, [index]));
            Note($"    col{index} header={Show(Prop(column, "Header"))} width={Show(Prop(column, "Width"))} actualWidth={Show(Prop(column, "ActualWidth"))} " +
                 $"minWidth={Show(Prop(column, "MinWidth"))} visibility={Show(Prop(column, "Visibility"))} cellStyle={Show(Prop(Prop(column, "CellStyle"), "TargetType"))}");
        }

        foreach (var cell in AllOfType(element, typeof(DataGridCell)).Take(2).Cast<FrameworkElement>())
        {
            var style = Prop(cell, "Style");
            var styleName = style is null ? "null" : $"{Show(Prop(style, "Name"))}/{Show(Prop(style, "TargetType"))}";
            Note($"    cell size={SizeOf(cell)} desired={Text(() => Prop(cell, "DesiredSize"))} style={styleName} " +
                 $"localWidth={HasLocal(cell, "Width")} actualWidthValue={Show(Prop(cell, "ActualWidth"))} padding={Show(Prop(cell, "Padding"))} minH={Show(Prop(cell, "MinHeight"))}");
        }

        foreach (var (name, value) in new (string, object)[]
                 {
                     ("EnableColumnVirtualization", false),
                     ("EnableRowVirtualization", false),
                     ("ColumnHeaderHeight", 34d),
                     ("RowHeight", 30d),
                 })
        {
            _ = Set(element, name, value);
            Pump(8);
            var wide = AllOfType(element, typeof(DataGridCell)).Cast<FrameworkElement>().FirstOrDefault();
            Note($"  after {name}={value}: cellActualWidth={(wide is null ? "-" : wide.ActualWidth.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture))} gridDesired={Text(() => Prop(element, "DesiredSize"))}");
        }

        // The DataGrid slice's teeth: does the bound text reach a real TextBlock with real size, or sit stranded?
        var texts = AllOfType(element, typeof(TextBlock)).Cast<TextBlock>().Where(t => !string.IsNullOrEmpty(t.Text)).ToList();
        Note($"  realized TextBlocks={texts.Count}: {Join(texts.Take(12).Select(t => $"\"{t.Text}\"@{SizeOf(t)}"))}");
        if (_mode != "feedbare")
        {
            PrintTree(element, "    ", 10);
        }

        // Expanding without pointer input: the control publishes ExpandAll/CollapseAll/IsExpanded(int) and a
        // FlattenedCount, so the expand cycle is assertable in the gate the way the ToggleButton batch drives a
        // toggle - no OS input, and no reflection into the internal node.
        Note($"  FlattenedCount before expand={Show(Prop(element, "FlattenedCount"))} rowHeight={Show(Prop(element, "RowHeight"))} indentSize={Show(Prop(element, "IndentSize"))}");
        if (rows.Count > 0)
        {
            var first = rows[0];
            Note($"  row 0 public surface: IsSelected={Show(Prop(first, "IsSelected"))} localIsSelected={HasLocal(first, "IsSelected")} Node={Show(Prop(first, "Node"))}");
            Note($"  grid.IsExpanded(0)={Text(() => type.GetMethod("IsExpanded", [typeof(int)])?.Invoke(element, [0]))}");
            Note($"  ExpandAll() -> {Text(() => { element.GetType().GetMethod("ExpandAll")?.Invoke(element, []); return (object)"invoked"; })}");
            Pump(12);
            var expandedRows = AllOfType(element, rowType).OfType<FrameworkElement>().ToList();
            Note($"  rows after ExpandAll={expandedRows.Count} FlattenedCount={Show(Prop(element, "FlattenedCount"))}");
            foreach (var row in expandedRows)
            {
                Note($"    row height={SizeOf(row)} margin={Show(Prop(row, "Margin"))} localBG={HasLocal(row, "Background")} bg={Hex(Prop(row, "Background") as Brush)} cells={CountTypeName(row, "DataGridCell")}");
            }

            Note($"  realized TextBlocks after expand={AllOfType(element, typeof(TextBlock)).Cast<TextBlock>().Count(t => !string.IsNullOrEmpty(t.Text))}");
            Note($"  collapse back -> {Text(() => { element.GetType().GetMethod("CollapseAll")?.Invoke(element, []); return (object)"invoked"; })}");
            Pump(8);
            Note($"  rows after CollapseAll={AllOfType(element, rowType).Count()} FlattenedCount={Show(Prop(element, "FlattenedCount"))}");
            _ = Text(() => { element.GetType().GetMethod("ExpandAll")?.Invoke(element, []); return (object)"ok"; });
            Pump(8);
        }

        // What the framework's own row and grid templates consume - the method that decides themeability.
        foreach (var label in new[] { "TreeDataGrid", "TreeDataGridRow" })
        {
            var candidate = label == "TreeDataGrid" ? element : rows.FirstOrDefault();
            var template = candidate is null ? null : Prop(candidate, "Template") as ControlTemplate;
            if (template is null)
            {
                Note($"  {label}: mounted Template null");
                continue;
            }

            var xaml = TemplateXaml(template);
            Note($"  {label}: template {xaml.Length} chars triggers={Read(() => template.Triggers.Count)} keys=[{Join(ThemeResourceNames(xaml).OrderBy(k => k, StringComparer.Ordinal))}]");
        }

        root.Children.Remove(element);
        Pump(2);
    }

    private static List<TreeNode> TreeNodes() =>
    [
        new("Ship train", "Mara", [new("Astra styles", "Mara", []), new("Gallery pages", "Ivo", [new("Selection", "Ivo", [])])]),
        new("Test base", "Nils", [new("Pixel harness", "Nils", [])]),
    ];

    private sealed record TreeNode(string Name, string Owner, IReadOnlyList<TreeNode> Children);

    // ---------- I. why a column's Width='220' reads back Auto ----------

    /// <summary>
    /// The feed probe found rows that realize, text that measures and cells that all arrange to width 0, with
    /// <c>column.Width</c> reading back as <c>Auto</c> although the markup said 220 - and the same shape with Astra
    /// switched off, so it is not our styles. Two grids share DataGridColumn, so this either indicts the Gallery's
    /// shipping grid or the markup path specifically. Four configurations are compared in one run: markup Width,
    /// markup MinWidth, and a code-side set through whatever type the property actually wants.
    /// </summary>
    private static void ColumnWidths(Panel root, Window window)
    {
        Note("");
        Note("=== I. column width: markup vs code, DataGrid vs TreeDataGrid ===");
        ReportColumns(root, "DataGrid markup Width=200", GridMarkup("DataGrid", false, "Width='200'"), 0);
        ReportColumns(root, "TreeDataGrid markup Width=200", GridMarkup("TreeDataGrid", true, "Width='200'"), 0);
        ReportColumns(root, "DataGrid markup MinWidth=200", GridMarkup("DataGrid", false, "MinWidth='200'"), 0);
        ReportColumns(root, "TreeDataGrid markup MinWidth=200", GridMarkup("TreeDataGrid", true, "MinWidth='200'"), 0);
        ReportColumns(root, "DataGrid code Width=200", GridMarkup("DataGrid", false, string.Empty), 1);
        ReportColumns(root, "TreeDataGrid code Width=200", GridMarkup("TreeDataGrid", true, string.Empty), 1);
        ReportColumns(root, "DataGrid code Width=200 AFTER mount", GridMarkup("DataGrid", false, string.Empty), 2);
        ReportColumns(root, "TreeDataGrid code Width=200 AFTER mount", GridMarkup("TreeDataGrid", true, string.Empty), 2);
    }

    private static string GridMarkup(string control, bool tree, string columnAttributes) => $$"""
        <{{control}} xmlns='http://schemas.jalium.ui/2024' Height='160' Width='460'
                     AutoGenerateColumns='False'{{(tree ? " TreeColumnIndex='0' ChildrenPropertyPath='Children'" : string.Empty)}}>
          <{{control}}.Columns>
            <DataGridTextColumn Header='Name' Binding='{Binding Name}' {{columnAttributes}} />
            <DataGridTextColumn Header='Owner' Binding='{Binding Owner}' {{columnAttributes}} />
          </{{control}}.Columns>
        </{{control}}>
        """;

    private static void ReportColumns(Panel root, string label, string markup, int when)
    {
        var parsed = Read(() => XamlReader.Parse(markup));
        if (parsed is not FrameworkElement element)
        {
            Note($"  {label}: parse -> {Text(() => parsed)}");
            return;
        }

        _ = Set(element, "ItemsSource", TreeNodes());
        var columns = Prop(element, "Columns");
        var count = Prop(columns, "Count") is int length ? length : 0;
        var indexer = columns?.GetType().GetProperty("Item");

        void SetWidthFromCode()
        {
            for (var index = 0; index < count; index++)
            {
                var column = Read(() => indexer?.GetValue(columns, [index]));
                if (column is null or string)
                {
                    Note($"  {label}: column {index} -> {column}");
                    continue;
                }

                var property = column.GetType().GetProperty("Width", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                var widthType = property?.PropertyType;
                if (property is null || widthType is null)
                {
                    Note($"  {label}: no readable Width on {column.GetType().Name}");
                    continue;
                }

                var built = Read(() => widthType.IsValueType ? Activator.CreateInstance(widthType, 200d)! : (object)200d);
                if (built is string failure)
                {
                    Note($"  {label}: {widthType.Name}(200) -> {failure}");
                    continue;
                }

                Note($"  {label}: code col{index} widthType={widthType.Name} -> {Text(() => { property.SetValue(column, built); return (object)$"read back {Show(property.GetValue(column))}"; })}");
            }
        }

        if (when == 1)
        {
            SetWidthFromCode();
        }

        root.Children.Add(element);
        if (when == 2)
        {
            SetWidthFromCode();
        }

        Pump(12);
        Note($"  {label}: grid size={SizeOf(element)} desired={Text(() => Prop(element, "DesiredSize"))} cells={CountTypeName(element, "DataGridCell")}");
        for (var index = 0; index < count; index++)
        {
            var column = Read(() => indexer?.GetValue(columns, [index]));
            Note($"    col{index} width={Show(Prop(column, "Width"))} actualWidth={Show(Prop(column, "ActualWidth"))} minWidth={Show(Prop(column, "MinWidth"))} maxWidth={Show(Prop(column, "MaxWidth"))}");
        }

        foreach (var cell in AllOfType(element, typeof(DataGridCell)).Cast<FrameworkElement>().Take(4))
        {
            Note($"    cell size={SizeOf(cell)} desired={Text(() => Prop(cell, "DesiredSize"))} localWidth={HasLocal(cell, "Width")}");
        }

        var texts = AllOfType(element, typeof(TextBlock)).Cast<TextBlock>().Where(t => !string.IsNullOrEmpty(t.Text)).ToList();
        Note($"    realized text: {Join(texts.Take(6).Select(t => $"\"{t.Text}\"@{SizeOf(t)}"))}");

        if (label.StartsWith("DataGrid markup Width", StringComparison.Ordinal))
        {
            // One of those realized strings was the whole item's ToString. Which part holds it decides whether the
            // shipping Gallery grid paints a stray row of model text, so the path is printed rather than guessed.
            var steps = new List<string>();
            var hits = new List<string>();
            void Find(DependencyObject node, int depth)
            {
                var label2 = node.GetType().Name + (string.IsNullOrEmpty(NameOf(node)) ? string.Empty : $"'{NameOf(node)}'");
                steps.Add(label2);
                if (Prop(node, "Content") is TreeNode item)
                {
                    hits.Add($"{string.Join(" > ", steps)} Content={Trim(item.ToString() ?? "?")}");
                }

                if (depth < 12)
                {
                    var children = 0;
                    try
                    {
                        children = VisualTreeHelper.GetChildrenCount(node);
                    }
                    catch
                    {
                        children = 0;
                    }

                    for (var index = 0; index < children; index++)
                    {
                        try
                        {
                            Find(VisualTreeHelper.GetChild(node, index), depth + 1);
                        }
                        catch
                        {
                            // unreadable child, already covered by its parent line
                        }
                    }
                }

                steps.RemoveAt(steps.Count - 1);
            }

            Find(element, 0);
            foreach (var hit in hits.Take(3))
            {
                Note($"    item-content path: {hit}");
            }
        }

        root.Children.Remove(element);
        Pump(2);
    }

    // ---------- F. headers, gridlines, resize ----------
    private static void ColumnParts(Panel root, Window window)
    {
        Note("");
        Note("=== F. header / gridline / resize surface ===");
        foreach (var name in new[] { "DataGridColumnHeader", "ColumnHeader", "DataGridCell", "DataGridRow", "DataGridSplitter", "GridSplitter", "DataGridColumn", "DataGridTextColumn", "DataGridBoundColumn" })
        {
            var type = TypeByName(name);
            Note($"  {name}: {(type is null ? "NOT EXPORTED" : Chain(type))}");
            if (type is null)
            {
                continue;
            }

            Note($"    DPs={Join(DeclaredDpNames(type))}");
            Note($"    props={Join(Properties(type))}");
        }

        // A mounted grid with explicit columns: which named parts carry the header row and the lines, and what
        // brushes do they rest on (the reading Astra's key rows have to be written against).
        var grid = NewGrid();
        if (grid is not null)
        {
            _ = AddColumns(grid);
            var element = (FrameworkElement)grid;
            root.Children.Add(element);
            Pump(8);
            Note("  explicit-column grid tree:");
            PrintTree(element, "    ", 14);
            foreach (var brushHost in AllOfType(element, typeof(Border)).Take(24))
            {
                var border = (Border)brushHost;
                Note($"    Border name={NameOf(border) ?? "-"} bg={Hex(border.Background)} bw={border.BorderThickness} bc={Hex(border.BorderBrush)} r={border.CornerRadius} size={SizeOf(border)}");
            }

            root.Children.Remove(element);
            Pump(2);
        }
    }

    // ---------- G. does a bound column put text into a cell under our style? ----------

    private static void CellContent()
    {
        Note("");
        Note("=== G. bound cell content under Astra's style ===");
        var gridType = TypeByName("DataGrid")!;
        var grid = Activator.CreateInstance(gridType)!;
        var columnType = TypeByName("DataGridTextColumn")!;
        foreach (var (header, path) in new[] { ("Alpha", "Name"), ("Beta", "Value") })
        {
            var column = Activator.CreateInstance(columnType)!;
            _ = Set(column, "Header", header);
            _ = Set(column, "Width", 160d);
            var bindingType = TypeByName("Binding")!;
            var binding = Activator.CreateInstance(bindingType, [path]);
            _ = Set(column, "Binding", binding);
            var columns = Prop(grid, "Columns");
            columns!.GetType().GetMethod("Add")!.Invoke(columns, [column]);
        }

        _ = Set(grid, "ItemsSource", Rows(2));
        _ = Set(grid, "Width", 420d);
        _ = Set(grid, "Height", 160d);
        var element = (FrameworkElement)grid;
        ((Panel)Root).Children.Add(element);
        Pump(10);
        Note($"  our DataGrid style live={Read(() => FluentThemeManager.GetStyle("DefaultDataGridStyle") is not null)}");
        foreach (var cell in AllOfType(element, TypeByName("DataGridCell")!))
        {
            var content = Prop(cell, "Content");
            var text = content as TextBlock;
            Note($"  cell content={(content?.GetType().Name ?? "null")} size={SizeOf((FrameworkElement)cell)} selected={Show(Prop(cell, "IsSelected"))} " +
                 $"padding={Show(Prop(cell, "Padding"))} minH={Show(Prop(cell, "MinHeight"))} hca={Show(Prop(cell, "HorizontalContentAlignment"))} " +
                 (text is null
                     ? $"text=\"{Show(content)}\""
                     : $"text=\"{Trim(text.Text)}\" textSize={SizeOf(text)} textFg={Hex(text.Foreground)} textFs={text.FontSize}"));
        }

        var firstCell = AllOfType(element, TypeByName("DataGridCell")!).FirstOrDefault();
        if (firstCell is not null)
        {
            // Where does the cell's content element actually live? ContentPresenter caches the visual it built and
            // the runtime has an explicit release hook for the case where a re-templated control leaves that visual
            // parented into the retired tree (ContentPresenter.ReleaseContentElementForTemplateTeardown). If the
            // TextBlock's parent chain does not reach the live presenter, the 0x0 is stranding, not sizing.
            var parentChain = new List<string>();
            object? walk = Prop(firstCell, "Content");
            for (var hop = 0; hop < 6 && walk is not null; hop++)
            {
                var visualParent = Read(() => Prop(walk!, "VisualParent"));
                parentChain.Add($"{walk.GetType().Name}#{walk.GetHashCode() % 100000}");
                walk = visualParent;
            }

            var presenter = AllOfType(firstCell, typeof(ContentPresenter)).FirstOrDefault();
            Note("  content parent chain: " + string.Join(" <- ", parentChain));
            Note($"  live presenter={(presenter is null ? "none" : $"{presenter.GetType().Name}#{presenter.GetHashCode() % 100000}")} " +
                 $"itsContentSame={(presenter is null ? "-" : ReferenceEquals(Prop(presenter, "Content"), Prop(firstCell, "Content")) ? "yes" : "no")}");
            var cellTemplate = Prop(firstCell, "Template") as ControlTemplate;
            var cellTarget = cellTemplate is null ? "null" : Show(Prop(cellTemplate, "TargetType"));
            Note($"  cell template: type={(cellTemplate?.GetType().FullName ?? "null")} hash={cellTemplate?.GetHashCode()} target={cellTarget}");
            Note($"    xaml={(cellTemplate is null ? "none" : Trim(TemplateXaml(cellTemplate)))}");
        }

        var firstRow = AllOfType(element, TypeByName("DataGridRow")!).FirstOrDefault();
        if (firstRow is not null)
        {
            var rowTemplate = Prop(firstRow, "Template") as ControlTemplate;
            Note($"  row template: type={(rowTemplate?.GetType().FullName ?? "null")} hash={rowTemplate?.GetHashCode()}");
            Note($"    xaml={(rowTemplate is null ? "none" : Trim(TemplateXaml(rowTemplate)))}");
        }

        // A/B: the same grid with the columns generated by the control instead of supplied by us. A generated
        // column hands the cell a string, a bound one hands it a TextBlock the control built (ElementStyle), and
        // only the first shape is what the framework's own template was ever measured on.
        var auto = Activator.CreateInstance(gridType)!;
        _ = Set(auto, "AutoGenerateColumns", true);
        _ = Set(auto, "ItemsSource", Rows(2));
        _ = Set(auto, "Width", 420d);
        _ = Set(auto, "Height", 160d);
        var autoElement = (FrameworkElement)auto;
        ((Panel)Root).Children.Add(autoElement);
        Pump(10);
        var autoCells = AllOfType(autoElement, TypeByName("DataGridCell")!).ToList();
        Note($"  auto-generated grid: cells={autoCells.Count} presenters={CountTypeName(autoElement, "ContentPresenter")} texts={CountTypeName(autoElement, "TextBlock")}");
        if (autoCells.Count > 0)
        {
            Note($"    cell[0] content={(Prop(autoCells[0], "Content")?.GetType().Name ?? "null")}");
            PrintTree(autoCells[0], "    ", 8);
        }

        ((Panel)Root).Children.Remove(autoElement);
        Note("  first cell tree (bound columns):");
        var first = AllOfType(element, TypeByName("DataGridCell")!).FirstOrDefault();
        if (first is not null)
        {
            PrintTree(first, "    ", 10);
        }

        var boundRow = AllOfType(element, TypeByName("DataGridRow")!).FirstOrDefault();
        if (boundRow is not null)
        {
            Note("  first row tree (bound columns):");
            PrintTree(boundRow, "    ", 12);
        }

        ((Panel)Root).Children.Remove(element);
    }

    private static Panel Root = null!;

    // ---------- plumbing ----------

    private static string AddColumns(object grid)
    {
        var columns = Prop(grid, "Columns");
        if (columns is null)
        {
            return "no Columns collection";
        }

        var columnType = TypeByName("DataGridTextColumn") ?? TypeByName("DataGridColumn");
        if (columnType is null)
        {
            return "no column type exported";
        }

        var added = 0;
        var errors = new List<string>();
        foreach (var header in new[] { "Alpha", "Beta" })
        {
            var column = Read(() => Activator.CreateInstance(columnType));
            if (column is null or string)
            {
                errors.Add($"new {columnType.Name} -> {column}");
                continue;
            }

            _ = Set(column, "Header", header);
            _ = Set(column, "Width", 140d);
            var result = Text(() =>
            {
                columns.GetType().GetMethod("Add")?.Invoke(columns, [column]);
                return (object)"ok";
            });
            if (result == "ok")
            {
                added++;
            }
            else
            {
                errors.Add($"Add -> {result}");
            }
        }

        return $"{columnType.Name} added={added} count={Show(Prop(columns, "Count"))} {(errors.Count == 0 ? "" : string.Join(" | ", errors))}";
    }

    private static List<object> Rows(int count)
    {
        var rows = new List<object>();
        for (var index = 1; index <= count; index++)
        {
            rows.Add(new Row($"Row {index}", index * 10));
        }

        return rows;
    }

    private sealed record Row(string Name, int Value);

    private static string Chain(Type type)
    {
        var names = new List<string>();
        for (var current = type; current is not null && names.Count < 7; current = current.BaseType)
        {
            names.Add(current.Name);
        }

        return string.Join(" : ", names);
    }

    private static bool HasLocal(object target, string propertyName)
    {
        try
        {
            var field = target.GetType().GetField(propertyName + "Property",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (field?.GetValue(null) is not DependencyProperty dp || target is not DependencyObject node)
            {
                return false;
            }

            return !ReferenceEquals(node.ReadLocalValue(dp), DependencyProperty.UnsetValue);
        }
        catch
        {
            return false;
        }
    }

    private static string SizeOf(FrameworkElement element) => $"{element.ActualWidth:0.##}x{element.ActualHeight:0.##}";

    private static string Rendered(FrameworkElement element) => Text(() => Prop(element, "IsRendered"));

    private static string NameOf(DependencyObject node)
    {
        try
        {
            return node.GetType().GetProperty("Name")?.GetValue(node) as string ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void PrintTree(DependencyObject root, string pad, int maxDepth)
    {
        void Visit(DependencyObject node)
        {
            var name = NameOf(node);
            var extra = node switch
            {
                Border border => $"bg={Hex(border.Background)} r={border.CornerRadius} bt={border.BorderThickness}",
                TextBlock text => $"text=\"{Trim(text.Text)}\"",
                ContentPresenter presenter => $"content={Show(presenter.Content)} vis={presenter.Visibility}",
                _ => string.Empty,
            };
            var measure = node is FrameworkElement framed
                ? $" size={SizeOf(framed)} desired={Read(() => Prop(framed, "DesiredSize"))}"
                : string.Empty;
            Note($"{pad}{node.GetType().Name}{(string.IsNullOrEmpty(name) ? "" : " '" + name + "'")} {extra}{measure}");
        }

        Walk(root, Visit, maxDepth);
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

    private static string PartNames(DependencyObject root)
    {
        var names = new List<string>();
        Walk(root, node =>
        {
            var name = NameOf(node);
            if (name.Length > 0)
            {
                names.Add($"{name}({node.GetType().Name})");
            }
        }, 16);
        return names.Count == 0 ? "none" : string.Join(", ", names.Distinct(StringComparer.Ordinal));
    }

    private static DependencyObject? FirstNamed(DependencyObject root, string name)
    {
        DependencyObject? found = null;
        Walk(root, node =>
        {
            if (found is null && string.Equals(NameOf(node), name, StringComparison.Ordinal))
            {
                found = node;
            }
        }, 16);
        return found;
    }

    private static int CountTypeName(DependencyObject root, string typeName)
    {
        var count = 0;
        Walk(root, node =>
        {
            if (node.GetType().Name.Contains(typeName, StringComparison.Ordinal))
            {
                count++;
            }
        }, 16);
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
        }, 16);
        return found;
    }

    private static List<Assembly> LoadedAssemblies()
    {
        foreach (var name in new[] { "Jalium.Data", "Jalium.UI.Controls", "Jalium.UI.Controls.DataGrid" })
        {
            try
            {
                Assembly.Load(name);
            }
            catch
            {
                // Not every probe build carries every assembly; the census just reports what is loaded.
            }
        }

        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("Jalium", StringComparison.Ordinal) == true)
            .ToList();
    }

    private static Type? TypeByName(string name) => LoadedAssemblies()
        .SelectMany(SafeTypes)
        .FirstOrDefault(type => !type.IsNested && type.Name == name);

    private static IEnumerable<Type> SafeTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(t => t is not null)!;
        }
        catch
        {
            return [];
        }
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
            if (field.Name.EndsWith("Property", StringComparison.Ordinal) && field.GetValue(null) is DependencyProperty dp)
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
        : target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)?.GetValue(target);

    private static string Set(object target, string name, object value)
    {
        var property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        if (property is null || !property.CanWrite)
        {
            return $"no writable {name} on {target.GetType().Name}";
        }

        return Text(() =>
        {
            property.SetValue(target, value);
            return (object)$"{name}={Show(property.GetValue(target))}";
        });
    }

    /// <summary>
    /// The markup the framework template was built from. Recorded as internal on FrameworkTemplate in the sibling
    /// tree, so it is read through whatever accessors this build exposes rather than assumed public.
    /// </summary>
    private static string TemplateXaml(ControlTemplate template) => Text(() => template
        .GetType()
        .GetProperty("VisualTreeXaml",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)?
        .GetValue(template));

    private static string Text(Func<object?> reader) => Read(reader) switch
    {
        string value => value,
        null => string.Empty,
        var other => Show(other),
    };

    private static object? Read(Func<object?> reader)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            return "threw " + (exception.InnerException ?? exception).GetType().Name + ": " +
                   Trim((exception.InnerException ?? exception).Message);
        }
    }

    private static string Show(object? value) => value is null ? "null" : Trim(value.ToString() ?? "?");

    private static string Hex(Brush? brush)
    {
        if (brush is null)
        {
            return "null";
        }

        var color = Prop(brush, "Color") as Color?;
        return color is null ? Trim(brush.ToString() ?? "?") : $"#{color.Value.A:X2}{color.Value.R:X2}{color.Value.G:X2}{color.Value.B:X2}";
    }

    private static object? ApplicationResource(object key) => _application.TryFindResource(key);

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

    private static IEnumerable<string> Wrap(string text, int width)
    {
        for (var start = 0; start < text.Length; start += width)
        {
            yield return text.Substring(start, Math.Min(width, text.Length - start));
        }
    }

    private static void Note(string line) => Lines.Add(line);

    private static void Note2(string line) => Note("    " + Trim(line));

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
