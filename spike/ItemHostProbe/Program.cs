using System.Reflection;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

namespace ItemHostProbe;

/// <summary>
/// The questions the stage-5 tail (GridView judgement + BreadcrumbBar / RadioButtons / PipsPager) cannot write
/// product code without. <c>adaptation/09-item-host-base.md</c> records one hard fact and one inference: a bare
/// <c>ItemsControl</c> mounted on screen stops the frame pump (its <c>Template</c> is null), and from that the
/// repo concluded self-authored item hosts must hand-place children into named panels - which is what
/// FluentNavigationView and FluentTabView do. That conclusion costs a lot: every one of the three remaining
/// controls would re-implement container generation. The decisive reading is whether an <c>ItemsControl</c> that
/// is handed OUR template realizes containers and keeps pumping, because "no default template" and "no usable
/// item pipeline" are different claims and only the second one forces a hand-placed host.
///   A census  : which names the tail needs the runtime actually exports (the three controls are absent per
///               adaptation/00 S1-l; the host primitives are the open question), and what GridView's family
///               really is - the name alone cannot tell WPF's GridViewColumn view from WinUI's wrapping items host.
///   B api     : the container-generation surface a derived host would override, with declared types, so the
///               product code is written against names that exist in 26.10.9 rather than against WPF's shape.
///   C mount   : positive control (ListBox realizes per-control containers), then ItemsControl + our own
///               ControlTemplate, then a derived subclass, then the bare instance last with a short budget so a
///               hang can only cost the final block. Each block reads frames, container count, panel type and tree.
///   D panel   : can the items panel be replaced (WrapPanel / UniformGrid) - the difference between a wrapping
///               RadioButtons grid and a vertical stack, and between a paged PipsPager and a fixed row.
///   E capture : which cost adaptation/09 attributed to "no frames" actually belongs to the rasteriser instead
///               (run separately: a text-only capture is the expensive candidate).
///   F radio   : a derived host that generates real RadioButton containers - does OUR implicit RadioButton style
///               reach a generated container, and do the radios exclude each other without a hand-written group?
/// Reflection lives here and only here; the structural gates in AstraGateTests keep it out of src/FluentJalium.
/// Modes: census | api | mount | panel | capture | radio | all.
/// </summary>
internal static partial class Program
{
    private static readonly List<string> Lines = [];

    private static Application _application = null!;
    private static string _mode = "all";

    /// <summary>A derived host, exactly the shape a product control would have.</summary>
    private sealed class ProbeItemsControl : ItemsControl;

    /// <summary>
    /// The shape FluentRadioButtons would have: derive from the host's item pipeline, generate real
    /// <see cref="RadioButton"/> containers, and let the style layer decorate them. Whether the framework
    /// gives those generated containers OUR implicit RadioButton style, and whether they exclude each other
    /// without a group name written by hand, are the two readings this whole mode exists for - the control is
    /// absent from the runtime, so the design has to be measured before it is written.
    /// </summary>
    private sealed class ProbeRadioHost : ItemsControl
    {
        protected override bool IsItemItsOwnContainerOverride(object item) => item is RadioButton;

        protected override DependencyObject GetContainerForItemOverride() => new RadioButton();

        protected override void PrepareContainerForItemOverride(DependencyObject container, object item)
        {
            base.PrepareContainerForItemOverride(container, item);
            if (container is RadioButton radio)
            {
                radio.Content = item;
            }
        }
    }

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

        var path = Path.Combine(AppContext.BaseDirectory, $"itemhost-probe-{_mode}.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, mode={_mode}, full log: {path}");
        return 0;
    }

    private static void Run(Application application)
    {
        var root = new StackPanel { Margin = new Thickness(24) };
        var window = new Window { Content = root, Width = 900, Height = 700, Title = "Item host probe" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Pump();
                Note($"theme dictionaries={FluentThemeManager.DictionaryNames.Count}");
                if (_mode is "all" or "census")
                {
                    Census();
                }

                if (_mode is "all" or "api")
                {
                    Api();
                }

                if (_mode is "all" or "mount")
                {
                    Mount(root);
                }

                if (_mode is "all" or "panel")
                {
                    PanelRoute(root);
                }

                if (_mode is "capture")
                {
                    CaptureCost(root);
                }

                if (_mode is "all" or "radio")
                {
                    RadioHost(root);
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

    // ---------- A. what the tail has to work with ----------

    private static void Census()
    {
        Note("");
        Note("=== A. stage-5 tail census ===");
        foreach (var name in new[]
                 {
                     "BreadcrumbBar", "PipsPager", "RadioButtons", "ItemsRepeater", "CardAction",
                     "ItemsControl", "ItemsPresenter", "ItemsPanelTemplate", "Panel", "WrapPanel", "UniformGrid",
                     "StackPanel", "VirtualizingStackPanel", "ListBox", "ListBoxItem", "ListView", "ListViewItem",
                     "GridView", "GridViewRow", "GridViewColumn", "GridViewHeaderRowPresenter", "GridViewRowPresenter",
                 })
        {
            var type = TypeByName(name);
            Note($"  {name,-28} {(type is null ? "ABSENT" : Chain(type))}");
        }

        Note("");
        Note("  every exported type whose name contains \"GridView\" (WPF's view family or WinUI's wrapping host?):");
        foreach (var type in LoadedAssemblies()
                     .SelectMany(SafeTypes)
                     .Where(type => !type.IsNested && type.Name.Contains("GridView", StringComparison.Ordinal))
                     .OrderBy(type => type.Name, StringComparer.Ordinal))
        {
            Note($"    {type.FullName} : {Chain(type)}");
        }

        Note("");
        Note("  who derives from ItemsControl (the framework's own item hosts):");
        var itemsControl = TypeByName("ItemsControl");
        if (itemsControl is not null)
        {
            foreach (var type in LoadedAssemblies()
                         .SelectMany(SafeTypes)
                         .Where(type => !type.IsNested && type != itemsControl && type.IsSubclassOf(itemsControl))
                         .OrderBy(type => type.Name, StringComparer.Ordinal))
            {
                Note($"    {type.Name,-28} declaredDp={Join(DeclaredDpNames(type).Take(8))}");
            }
        }
    }

    // ---------- B. the container-generation surface ----------

    private static void Api()
    {
        Note("");
        Note("=== B. ItemsControl surface a derived host would use ===");
        var type = TypeByName("ItemsControl");
        if (type is null)
        {
            Note("ItemsControl ABSENT.");
            return;
        }

        for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
        {
            Note($"  {current.Name} declared properties:");
            foreach (var property in current.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                         .OrderBy(property => property.Name, StringComparer.Ordinal))
            {
                var accessor = property.GetMethod is null ? string.Empty :
                    $"  <{property.GetMethod.Name}={(property.GetMethod.IsVirtual ? "virtual" : "final")}{(property.GetMethod.IsPublic ? "" : " nonpublic")}>";
                Note($"    {property.Name,-32} {property.PropertyType.Name}{accessor}");
            }
        }

        Note("");
        Note("  method accessibility (can OUR assembly override it?):");
        foreach (var name in new[]
                 {
                     "GetContainerForItemOverride", "IsItemItsOwnContainerOverride", "ClearContainerForItemOverride",
                     "PrepareContainerForItemOverride", "GetContainerForItem", "OnItemsChanged",
                 })
        {
            var method = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (method is null)
            {
                Note($"    {name,-34} ABSENT");
                continue;
            }

            var shape = string.Join(", ", method.GetParameters().Select(parameter => $"{parameter.ParameterType.Name} {parameter.Name}"));
            Note($"    {name,-34} {(method.IsFamily ? "protected" : method.IsPrivate ? "private" : method.IsAssembly ? "internal" : method.IsFamilyOrAssembly ? "protected internal" : "public")}" +
                 $"/{(method.IsVirtual ? "virtual" : "final")} ({shape}) -> {method.ReturnType.Name}");
        }
    }

    // ---------- C2. what actually costs the tens of seconds ----------

    /// <summary>
    /// <c>adaptation/09</c> records that a bare <c>ItemsControl</c> "hangs the frame pump for 60 seconds" and reads
    /// the cause as "its Template is null, so the framework walks a path that produces no frames". Mount 5 above
    /// contradicts the mechanism: the instance renders through a fallback host and frames keep arriving. The
    /// remaining candidate is the capture, and <c>PixelHarness</c> already carries a note that rasterising a
    /// text-only visual costs tens of seconds. A bare ItemsControl's fallback host is exactly that - three
    /// ContentPresenters over three TextBlocks, no opaque surface - so the two claims can be told apart by timing
    /// a RenderTargetBitmap.Render of each. This mode is run separately and under a hard timeout on purpose: if the
    /// text-only capture is the cost, the run is the evidence and the missing log is the reading.
    /// </summary>
    private static void CaptureCost(Panel root)
    {
        Note("");
        Note("=== C2. where the tens of seconds actually sit ===");
        foreach (var (label, element) in new[]
                 {
                     ("ListBox (opaque Borders + Rectangles)", (FrameworkElement)new ListBox { ItemsSource = Items(), Width = 200, Height = 120 }),
                     ("bare ItemsControl (text-only fallback host)", new ItemsControl { ItemsSource = Items(), Width = 200, Height = 120 }),
                 })
        {
            root.Children.Add(element);
            Pump(6);
            var started = Environment.TickCount64;
            var bitmap = new RenderTargetBitmap(200, 120, 96, 96, PixelFormat.Bgr32);
            bitmap.Render(element);
            Note($"  {label}: RenderTargetBitmap.Render took {(Environment.TickCount64 - started) / 1000.0:0.##}s, pixels={bitmap.PixelWidth}x{bitmap.PixelHeight}");
            root.Children.Remove(element);
            Pump(2);
        }
    }

    // ---------- C. can an ItemsControl be given OUR template? ----------

    private const string HostTemplate = @"
<ResourceDictionary xmlns='http://schemas.jalium.ui/2024'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <ControlTemplate x:Key='ProbeHostTemplate'>
    <Border x:Name='ProbeHostRoot' Background='#FFFF00FF' CornerRadius='4'>
      <ItemsPresenter x:Name='ProbeItems'/>
    </Border>
  </ControlTemplate>
  <Style x:Key='ProbeHostStyle' TargetType='ItemsControl'>
    <Setter Property='Template' Value='{StaticResource ProbeHostTemplate}'/>
  </Style>
</ResourceDictionary>";

    private static void Mount(Panel root)
    {
        Note("");
        Note("=== C. item pipeline: five mounts, same window, same pump ===");

        // 1. Positive control. adaptation/09 measured this tree; re-measured here so a null below cannot be blamed
        //    on the frame pump or the mount helper.
        MountAndReport("1 ListBox (positive control, no template of ours)",
            new ListBox { ItemsSource = Items(), Width = 420, Height = 140 }, root, 10);

        var dictionary = Read(() => XamlReader.Parse(HostTemplate)) as ResourceDictionary;
        var template = Read(() => dictionary?["ProbeHostTemplate"]) as ControlTemplate;
        var style = Read(() => dictionary?["ProbeHostStyle"]) as Style;
        Note($"  probe dictionary={(dictionary is null ? "PARSE FAILED" : "ok")} template={(template is null ? "null" : "ok")} style={(style is null ? "null" : "ok")}");
        if (dictionary is null)
        {
            Note("  template markup that failed: " + Trim(HostTemplate));
        }

        // 2. ItemsControl handed our template as a local value. The decisive block: if containers appear under
        //    ItemsPresenter and the pump returns, the tail's three controls can derive from ItemsControl.
        var local = new ItemsControl { ItemsSource = Items(), Width = 420, Height = 140 };
        if (template is not null)
        {
            local.Template = template;
        }

        MountAndReport("2 ItemsControl + our ControlTemplate (local value)", local, root, 10);

        // 3. Same, through an explicit Style - the route an Astra dictionary actually uses.
        var styled = new ItemsControl { ItemsSource = Items(), Width = 420, Height = 140 };
        if (style is not null)
        {
            styled.Style = style;
        }

        MountAndReport("3 ItemsControl + our Style (Template setter)", styled, root, 10);

        // 4. A derived host with nothing set: the shape a product control has before its own dictionary lands.
        MountAndReport("4 derived ProbeItemsControl, no template",
            new ProbeItemsControl { ItemsSource = Items(), Width = 420, Height = 140 }, root, 10);

        // 5. The hang case from adaptation/09, kept last with a short budget so it can only cost this block.
        MountAndReport("5 bare ItemsControl, no template (adaptation/09 hang case)",
            new ItemsControl { ItemsSource = Items(), Width = 420, Height = 140 }, root, 4, 700);
    }

    private static void MountAndReport(string label, ItemsControl element, Panel root, int frames, int budget = 1500)
    {
        Note("");
        Note($"  --- {label} ---");
        Note($"    pre-mount: Template={(element.Template is null ? "null" : "SET")} Style={(element.Style is null ? "null" : "SET")} " +
             $"Items={Show(Prop(Prop(element, "Items"), "Count"))}");
        root.Children.Add(element);
        var seen = Pump(frames, budget);
        Note($"    after {seen} frames: rendered={Rendered(element)} size={SizeOf(element)} desired={Text(() => Prop(element, "DesiredSize"))}");
        Note($"    post-mount: Template={(element.Template is null ? "null" : "SET")} Style={(element.Style is null ? "null" : "SET")}");
        var presenter = AllOfType(element, "ItemsPresenter").FirstOrDefault();
        Note($"    ItemsPresenter present={presenter is not null} childCount={(presenter is null ? "-" : Show(Prop(presenter, "VisualChildrenCount")))}");
        var panels = AllWhere(element, node => node is Panel panel && !ReferenceEquals(panel, root));
        Note($"    panels under the host: {(panels.Count == 0 ? "none" : Join(panels.Select(p => $"{p.GetType().Name}(children={((Panel)p).Children.Count})"))) }");
        Note($"    container counts: ContentPresenter={CountTypeName(element, "ContentPresenter")} ListBoxItem={CountTypeName(element, "ListBoxItem")} ItemsControl={CountTypeName(element, "ItemsControl")} TextBlock={CountTypeName(element, "TextBlock")}");
        Note("    tree:");
        PrintTree(element, "      ", 10);
        root.Children.Remove(element);
        Pump(2);
    }

    // ---------- D. replacing the items panel ----------

    private const string PanelTemplates = @"
<ResourceDictionary xmlns='http://schemas.jalium.ui/2024'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <ItemsPanelTemplate x:Key='ProbeWrap'><WrapPanel/></ItemsPanelTemplate>
  <ItemsPanelTemplate x:Key='ProbeUniform'><UniformGrid Columns='4'/></ItemsPanelTemplate>
  <ItemsPanelTemplate x:Key='ProbeStack'><StackPanel Orientation='Horizontal'/></ItemsPanelTemplate>
</ResourceDictionary>";

    private static void PanelRoute(Panel root)
    {
        Note("");
        Note("=== D. items panel replacement (wrapping vs stacking) ===");
        var dictionary = Read(() => XamlReader.Parse(PanelTemplates)) as ResourceDictionary;
        Note($"  panel markup dictionary={(dictionary is null ? "PARSE FAILED" : "ok")}");
        if (dictionary is null)
        {
            return;
        }

        foreach (var key in new[] { "ProbeWrap", "ProbeUniform", "ProbeStack" })
        {
            var template = Read(() => dictionary[key]);
            Note($"  {key}: {(template is null ? "null" : template.GetType().Name)}");
            var list = new ListBox { ItemsSource = Items(8), Width = 420, Height = 160 };
            var assigned = Set(list, "ItemsPanel", template!);
            Note($"    assignment -> {assigned}");
            root.Children.Add(list);
            var seen = Pump(8);
            var panels = AllWhere(list, node => node is Panel panel && !ReferenceEquals(panel, root));
            Note($"    after {seen} frames: panels={(panels.Count == 0 ? "none" : Join(panels.Select(p => p.GetType().Name)))} size={SizeOf(list)}");
            var rows = AllOfType(list, "ListBoxItem").OfType<FrameworkElement>().ToList();
            Note($"    items realized={rows.Count} sizes={Join(rows.Take(8).Select(SizeOf))}");
            root.Children.Remove(list);
            Pump(2);
        }
    }

    private static void RadioHost(Panel root)
    {
        Note("");
        Note("=== F. a derived host that generates RadioButtons (the FluentRadioButtons shape) ===");
        var dictionary = Read(() => XamlReader.Parse(HostTemplate)) as ResourceDictionary;
        var panels = Read(() => XamlReader.Parse(PanelTemplates)) as ResourceDictionary;
        var template = Read(() => dictionary?["ProbeHostTemplate"]) as ControlTemplate;
        var wrap = Read(() => panels?["ProbeWrap"]);
        var host = new ProbeRadioHost { ItemsSource = Items(4), Width = 420, Height = 120 };
        if (template is not null)
        {
            host.Template = template;
        }

        Note($"  ItemsPanel assignment -> {Set(host, "ItemsPanel", wrap!)}");
        root.Children.Add(host);
        var seen = Pump(10);
        var radios = AllOfType(host, "RadioButton").OfType<RadioButton>().ToList();
        Note($"  after {seen} frames: size={SizeOf(host)} containers={radios.Count}");
        Note("  tree:");
        PrintTree(host, "    ", 8);
        foreach (var (radio, index) in radios.WithIndex())
        {
            var style = radio.Style;
            Note($"    radio[{index}] content=\"{Show(Prop(radio, "Content"))}\" size={SizeOf(radio)} " +
                 $"style={(style is null ? "null -> NO implicit Astra style reached the generated container" : $"{Show(Prop(style, "TargetType"))}/{Show(Prop(style, "Key"))}")} " +
                 $"padding={Show(Prop(radio, "Padding"))} minHeight={Show(Prop(radio, "MinHeight"))} foreground={Hex(Prop(radio, "Foreground") as Brush)} " +
                 $"groupName=\"{Show(Prop(radio, "GroupName"))}\" template={(radio.Template is null ? "null" : "SET")}");
        }

        // Mutual exclusion without a hand-written group name: the whole question for a RadioButtons host.
        if (radios.Count >= 3)
        {
            radios[1].IsChecked = true;
            Pump(4);
            Note($"    after checking [1]: {Join(radios.Select(r => Show(Prop(r, "IsChecked"))))}");
            radios[2].IsChecked = true;
            Pump(4);
            Note($"    after checking [2]: {Join(radios.Select(r => Show(Prop(r, "IsChecked"))))} (only [2] true means they exclude each other)");
            Note($"    group names now: {Join(radios.Select(r => Show(Prop(r, "GroupName"))))}");
        }

        root.Children.Remove(host);
        Pump(2);
    }

    // ---------- helpers ----------

    private static IEnumerable<(T value, int index)> WithIndex<T>(this IEnumerable<T> values)
    {
        var index = 0;
        foreach (var value in values)
        {
            yield return (value, index++);
        }
    }

    private static IEnumerable<string> Items(int count = 3) => Enumerable.Range(1, count).Select(index => $"item {index}");

    private static List<DependencyObject> AllOfType(DependencyObject root, string typeName) =>
        AllWhere(root, node => string.Equals(node.GetType().Name, typeName, StringComparison.Ordinal));

    private static List<DependencyObject> AllWhere(DependencyObject root, Func<DependencyObject, bool> predicate)
    {
        var found = new List<DependencyObject>();
        Walk(root, node =>
        {
            if (predicate(node))
            {
                found.Add(node);
            }
        }, 16);
        return found;
    }

    private static int CountTypeName(DependencyObject root, string typeName) => AllOfType(root, typeName).Count;

    private static string Chain(Type type)
    {
        var names = new List<string>();
        for (var current = type; current is not null && names.Count < 7; current = current.BaseType)
        {
            names.Add(current.Name);
        }

        return string.Join(" : ", names);
    }

    private static string SizeOf(FrameworkElement element) => $"{element.ActualWidth:0.##}x{element.ActualHeight:0.##}";

    private static string Rendered(FrameworkElement element) => Text(() => Prop(element, "IsRendered"));

    private static string NameOf(DependencyObject node) => Read(() => node.GetType().GetProperty("Name")?.GetValue(node)) as string ?? string.Empty;

    private static void PrintTree(DependencyObject root, string pad, int maxDepth)
    {
        void Visit(DependencyObject node, string indent, int depth)
        {
            if (depth > maxDepth)
            {
                return;
            }

            var name = NameOf(node);
            var extra = node switch
            {
                Border border => $"bg={Hex(border.Background)} r={border.CornerRadius}",
                TextBlock text => $"text=\"{Trim(text.Text)}\"",
                ContentPresenter presenter => $"content={Show(presenter.Content)} vis={presenter.Visibility}",
                _ => string.Empty,
            };
            var measure = node is FrameworkElement framed ? $" size={SizeOf(framed)}" : string.Empty;
            Note($"{indent}{node.GetType().Name}{(name.Length > 0 ? $" #{name}" : string.Empty)}{measure} {extra}");
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
                    Visit(VisualTreeHelper.GetChild(node, index), indent + "  ", depth + 1);
                }
                catch
                {
                    // A child that cannot be walked is not worth the probe; the parent line already says it exists.
                }
            }
        }

        Visit(root, pad, 0);
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
                // Same rule as PrintTree.
            }
        }
    }

    private static List<Assembly> LoadedAssemblies()
    {
        foreach (var name in new[] { "Jalium.UI.Controls", "Jalium.UI.Controls.DataGrid" })
        {
            try
            {
                Assembly.Load(name);
            }
            catch
            {
                // Not every probe build carries every assembly; the census reports what is loaded.
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

    private static string Join(IEnumerable<string> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? "none" : string.Join(" | ", list);
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
