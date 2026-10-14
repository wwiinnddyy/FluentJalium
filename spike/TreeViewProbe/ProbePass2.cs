using System.Reflection;
using System.Text;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace TreeViewProbe;

/// <summary>
/// Pass 2 of the same probe. The first pass answered the structural questions and left two open that product code
/// cannot be written over: the runtime shifts the row's header 16 DIP per nesting level while the row itself stays
/// at the same x (so something invisible carries the depth, and a re-template that loses it turns a tree into a
/// flat list), and a nested host resting on a template attribute survived an IsExpanded cell (so the collapse
/// route has to be picked by measurement, not by WPF habit).
/// </summary>
internal static partial class Program
{
    // ---------- I. who carries the per-level indent ----------

    private static void IndentCarrier(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== I. the indent carrier: what differs between a level-1 row and a level-2 row ===");
        if (ItemType is null)
        {
            Note("  no TreeViewItem type");
            return;
        }

        var tree = Tree(Node("L1", Node("L2", Node("L3"))), Node("Other root"));
        root.Children.Add(tree);
        window.UpdateLayout();
        Pump(10);
        Note("  expand the first two levels: " + Read(() =>
        {
            foreach (var row in AllOfType(tree, ItemType).Take(2))
            {
                Set(row, "IsExpanded", true);
            }

            window.UpdateLayout();
            Pump(12);
            return "containers=" + CountContainers(tree, ItemType);
        }));

        var rows = AllOfType(tree, ItemType).Cast<FrameworkElement>().ToList();
        Note($"  rows realised: {rows.Count}");
        var snapshot = rows.Select(Snapshot).ToList();
        for (var index = 0; index < rows.Count; index++)
        {
            var at = rows[index].TranslatePoint(new Point(), tree);
            var header = AllOfType(rows[index], typeof(ContentPresenter))
                .OfType<FrameworkElement>()
                .FirstOrDefault(element => NameOf(element) == "PART_Header");
            var headerAt = header is null ? "none" : header.TranslatePoint(new Point(), tree).X.ToString("0.##");
            Note($"  [{index}] header={Show(Prop(rows[index], "Header"))} row@{at.X:0.##},{at.Y:0.##} " +
                 $"{rows[index].ActualWidth:0.##}x{rows[index].ActualHeight:0.##} part_Header@x={headerAt}");
            Note($"      {PartSize(rows[index], "PART_IndentSpacer")} | {PartSize(rows[index], "PART_ExpanderBorder")} | {PartSize(rows[index], "PART_ItemsHost")}");
        }

        if (snapshot.Count >= 2)
        {
            Note("  every readable value that differs between row 0 and row 1:");
            foreach (var key in snapshot[0].Keys.Union(snapshot[1].Keys, StringComparer.Ordinal))
            {
                var a = snapshot[0].TryGetValue(key, out var left) ? left : "-";
                var b = snapshot[1].TryGetValue(key, out var right) ? right : "-";
                if (a != b)
                {
                    Note($"    {key}: row0={a} row1={b}");
                }
            }

            if (snapshot.Count >= 3)
            {
                Note("  every readable value that differs between row 0 and the leaf root:");
                foreach (var key in snapshot[0].Keys.Union(snapshot[^1].Keys, StringComparer.Ordinal))
                {
                    var a = snapshot[0].TryGetValue(key, out var left) ? left : "-";
                    var c = snapshot[^1].TryGetValue(key, out var third) ? third : "-";
                    if (a != c)
                    {
                        Note($"    {key}: row0={a} leafRoot={c}");
                    }
                }
            }
        }

        Note("  DependencyProperty fields declared by the chain and by the host:");
        foreach (var type in new[] { ItemType, ItemType.BaseType, typeof(TreeView) }.Where(candidate => candidate is not null))
        {
            var names = type!
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(field => field.Name.EndsWith("Property", StringComparison.Ordinal))
                .Select(field => field.Name[..^"Property".Length])
                .Order(StringComparer.Ordinal);
            Note($"    {type.Name}: {string.Join(", ", names)}");
        }

        Note(string.Empty);
        Note("  our own item template on the same content (does the survive, do children gate):");
        var style = ParseStyle("our candidate", """
            <Style TargetType='TreeViewItem' xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
              <Setter Property='Padding' Value='0,3,0,5'/>
              <Setter Property='MinHeight' Value='28'/>
              <Setter Property='Template'>
                <ControlTemplate TargetType='TreeViewItem'>
                  <Border Name='ContentBorder' Background='{TemplateBinding Background}'>
                    <StackPanel Orientation='Horizontal'>
                      <ToggleButton Name='IndentSpacer' Width='16' Focusable='False' Background='Transparent' IsHitTestVisible='False'/>
                      <ToggleButton Name='Chevron' Width='16' Focusable='False' Background='Transparent'
                                    IsChecked='{Binding IsExpanded, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}'/>
                      <ContentPresenter Name='HeaderHost' Content='{TemplateBinding Header}' VerticalAlignment='Center'/>
                    </StackPanel>
                  </Border>
                </ControlTemplate>
              </Setter>
            </Style>
            """);
        if (style is null)
        {
            root.Children.Clear();
            return;
        }

        var dictionary = new ResourceDictionary();
        dictionary.Add(ItemType, style);
        Note("    merge before building: " + Try(() => _application.Resources.MergedDictionaries.Add(dictionary)));
        var ours = Tree(Node("L1", Node("L2", Node("L3"))), Node("Other root"));
        ours.Template = OverlayTemplate("True");
        root.Children.Add(ours);
        window.UpdateLayout();
        Pump(14);
        Note("    containers before expanding: " + CountContainers(ours, ItemType) +
             " (a nested template with no ItemsPresenter at all: can children ever appear?)");
        foreach (var row in AllOfType(ours, ItemType).Take(3))
        {
            Set(row, "IsExpanded", true);
        }

        window.UpdateLayout();
        Pump(14);
        foreach (var row in AllOfType(ours, ItemType).Cast<FrameworkElement>())
        {
            var at = row.TranslatePoint(new Point(), ours);
            var presenter = AllOfType(row, typeof(ContentPresenter)).OfType<FrameworkElement>().ElementAtOrDefault(0);
            var presenterAt = presenter is null ? "none" : presenter.TranslatePoint(new Point(), ours).X.ToString("0.##");
            Note($"    {Show(Prop(row, "Header"))} row@{at.X:0.##},{at.Y:0.##} {row.ActualWidth:0.##}x{row.ActualHeight:0.##} presenter@x={presenterAt} min-height={Show(Prop(row, "MinHeight"))} padding={Show(Prop(row, "Padding"))}");
        }

        Note("    parts our template puts on a row: " + PartNames((DependencyObject)(AllOfType(ours, ItemType).ElementAtOrDefault(0) ?? (object)ours)));
        Try(() => _application.Resources.MergedDictionaries.Remove(dictionary));
        root.Children.Clear();
        window.UpdateLayout();
        Pump(4);
    }

    // ---------- K. can a state cell reach the text at all ----------

    private static void ForegroundRoutes(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== K. the text colour: which element a state cell can actually write ===");
        if (ItemType is null)
        {
            Note("  no TreeViewItem type");
            return;
        }

        var tree = Tree(Node("Parent", Node("Child 1")), Node("Disabled leaf"));
        root.Children.Add(tree);
        window.UpdateLayout();
        Pump(12);
        var parent = (FrameworkElement)AllOfType(tree, ItemType).ElementAtOrDefault(0)!;
        var presenter = (ContentPresenter?)AllOfType(parent, typeof(ContentPresenter)).ElementAtOrDefault(0);
        var text = AllOfType(parent, typeof(TextBlock)).OfType<TextBlock>().FirstOrDefault();

        Note("  does a ContentPresenter carry Foreground at all? " + Read(() =>
        {
            if (presenter is null)
            {
                return "no presenter";
            }

            var property = presenter.GetType().GetProperty("Foreground", PublicInstance);
            return property is null
                ? $"NO - {presenter.GetType().Name} has no Foreground member, so a template cell aimed at it has nothing to write"
                : $"yes, type={property.PropertyType.Name}, value={Show(property.GetValue(presenter))}";
        }));
        Note("  what the shipped template's cell produced: " + Read(() =>
            text is null ? "no text block" : $"TextBlock.Foreground={Hex(text.Foreground)} inherited-from-item={Hex((Brush?)Prop(parent, "Foreground"))}"));

        Note("  write Foreground on the item (the style-trigger route): " + Read(() =>
        {
            SetOrThrow(parent, "Foreground", new SolidColorBrush(Color.FromRgb(0x11, 0x22, 0x33)));
            window.UpdateLayout();
            Pump(6);
            return $"item={Hex((Brush)Prop(parent, "Foreground")!)} text={Hex(text!.Foreground)}";
        }));

        var glyph = AllOfType(parent, typeof(Path)).OfType<FrameworkElement>().ElementAtOrDefault(0);
        Note("  the glyph's Stroke route: " + Read(() => glyph is null
            ? "no path"
            : $"path.Stroke={Hex((Brush?)Prop(glyph, "Stroke"))}"));

        // A Style trigger writes the control's own property, which the generated text then inherits; a
        // ControlTemplate trigger can only aim at a named part. Same dictionary, both routes, one row each.
        var dictionary = ParseDictionary("""
            <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
              <Style TargetType='TreeViewItem'>
                <Setter Property='Foreground' Value='#FF445566'/>
                <Style.Triggers>
                  <Trigger Property='IsEnabled' Value='False'>
                    <Setter Property='Foreground' Value='#FF778899'/>
                  </Trigger>
                </Style.Triggers>
              </Style>
            </ResourceDictionary>
            """);
        if (dictionary is null)
        {
            Note("  the style-trigger dictionary did not parse; the style route is not available");
            root.Children.Clear();
            return;
        }

        Note("  merge a style with a Style.Triggers cell: " + Try(() => _application.Resources.MergedDictionaries.Add(dictionary)));
        var second = Tree(Node("Another parent", Node("Kid")), Node("Another leaf"));
        root.Children.Add(second);
        window.UpdateLayout();
        Pump(12);
        var rows = AllOfType(second, ItemType).Cast<FrameworkElement>().ToList();
        Note("  resting foreground through the style setter: " + Read(() =>
        {
            var row = rows[0];
            var block = AllOfType(row, typeof(TextBlock)).OfType<TextBlock>().FirstOrDefault();
            return $"item={Hex((Brush)Prop(row, "Foreground")!)} text={(block is null ? "none" : Hex(block.Foreground))}";
        }));
        Note("  the disabled cell fires: " + Read(() =>
        {
            SetOrThrow(rows[0], "IsEnabled", false);
            window.UpdateLayout();
            Pump(8);
            var block = AllOfType(rows[0], typeof(TextBlock)).OfType<TextBlock>().FirstOrDefault();
            return $"item={Hex((Brush)Prop(rows[0], "Foreground")!)} text={(block is null ? "none" : Hex(block.Foreground))}";
        }));
        Note("  and the second row is untouched: " + Read(() =>
        {
            var block = AllOfType(rows[^1], typeof(TextBlock)).OfType<TextBlock>().FirstOrDefault();
            return $"item={Hex((Brush)Prop(rows[^1], "Foreground")!)} text={(block is null ? "none" : Hex(block.Foreground))}";
        }));
        Try(() => _application.Resources.MergedDictionaries.Remove(dictionary));
        root.Children.Clear();
    }

    private static Dictionary<string, string> Snapshot(FrameworkElement row)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in row.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            values[property.Name] = Read(() => Show(property.GetValue(row)));
        }

        // Inherited and attached dependency properties are only readable through the object, so walk the public
        // static DependencyProperty fields of the whole chain. The carrier of an indent has to show up here.
        for (Type? owner = row.GetType(); owner is not null; owner = owner.BaseType)
        {
            foreach (var field in owner.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.Name.EndsWith("Property", StringComparison.Ordinal) &&
                    field.GetValue(null) is DependencyProperty dp)
                {
                    values[$"{owner.Name}.{dp.Name}"] = Read(() => Show(row.GetValue(dp)));
                }
            }
        }

        return values;
    }

    private static string PartSize(DependencyObject root, string name)
    {
        var part = AllOfType(root, typeof(FrameworkElement))
            .OfType<FrameworkElement>()
            .FirstOrDefault(element => NameOf(element) == name);
        if (part is null)
        {
            return $"{name}: absent";
        }

        var at = part.TranslatePoint(new Point(), (UIElement)root);
        return $"{name} {part.ActualWidth:0.##}x{part.ActualHeight:0.##}@{at.X:0.##},{at.Y:0.##} vis={part.Visibility}";
    }

    private static Style? ParseStyle(string label, string markup)
    {
        try
        {
            return (Style)Jalium.UI.Markup.XamlReader.Parse(markup)!;
        }
        catch (Exception exception)
        {
            Note($"  {label}: style parse THREW {exception.GetType().Name}: {Trim(exception.Message)}");
            return null;
        }
    }

    // ---------- J. three resting values for the nested host ----------

    private static void CollapseGates(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== J. gating the nested ItemsPresenter on IsExpanded, three resting values ===");
        if (ItemType is null)
        {
            Note("  no TreeViewItem type");
            return;
        }

        foreach (var (label, attributeRests, gateTarget, cells) in new[]
                 {
                     ("inverse cell only, host rests visible", false, "ItemsHost", @"
                        <Trigger Property='IsExpanded' Value='False'>
                          <Setter TargetName='ItemsHost' Property='Visibility' Value='Collapsed'/>
                        </Trigger>"),
                     ("host rests collapsed, one true cell", true, "ItemsHost", @"
                        <Trigger Property='IsExpanded' Value='True'>
                          <Setter TargetName='ItemsHost' Property='Visibility' Value='Visible'/>
                        </Trigger>"),
                     ("host rests collapsed, both cells", true, "ItemsHost", @"
                        <Trigger Property='IsExpanded' Value='True'>
                          <Setter TargetName='ItemsHost' Property='Visibility' Value='Visible'/>
                        </Trigger>
                        <Trigger Property='IsExpanded' Value='False'>
                          <Setter TargetName='ItemsHost' Property='Visibility' Value='Collapsed'/>
                        </Trigger>"),
                     ("a wrapper Border carries the gate", true, "ChildHostBorder", @"
                        <Trigger Property='IsExpanded' Value='True'>
                          <Setter TargetName='ChildHostBorder' Property='Visibility' Value='Visible'/>
                        </Trigger>")
                 })
        {
            var host = gateTarget == "ItemsHost"
                ? $"<ItemsPresenter Name='ItemsHost' Margin='16,0,0,0'{(attributeRests ? " Visibility='Collapsed'" : string.Empty)}/>"
                : $"<Border Name='ChildHostBorder'{(attributeRests ? " Visibility='Collapsed'" : string.Empty)}>" +
                  "<ItemsPresenter Name='ItemsHost' Margin='16,0,0,0'/></Border>";
            // Placeholders instead of interpolation: the Binding markup below needs three consecutive braces, and
            // an interpolated raw string would read those as a hole.
            var markup = """
                <Style TargetType='TreeViewItem' xmlns='https://schemas.jalium.dev/jalxaml/presentation'>
                  <Setter Property='Padding' Value='0,3,0,5'/>
                  <Setter Property='Template'>
                    <ControlTemplate TargetType='TreeViewItem'>
                      <Grid Background='Transparent'>
                        <StackPanel Orientation='Horizontal'>
                          <ToggleButton Name='Chevron' Width='16' Focusable='False' Content='+' Background='Transparent'
                                        IsChecked='{Binding IsExpanded, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}'/>
                          <ContentPresenter Name='HeaderHost' Content='{TemplateBinding Header}' VerticalAlignment='Center'/>
                        </StackPanel>
                        __HOST__
                      </Grid>
                      <ControlTemplate.Triggers>
                        __CELLS__
                        <Trigger Property='HasItems' Value='False'>
                          <Setter TargetName='Chevron' Property='Visibility' Value='Hidden'/>
                        </Trigger>
                      </ControlTemplate.Triggers>
                    </ControlTemplate>
                  </Setter>
                </Style>
                """.Replace("__HOST__", host).Replace("__CELLS__", cells);
            var style = ParseStyle(label, markup);
            if (style is null)
            {
                continue;
            }

            var dictionary = new ResourceDictionary();
            dictionary.Add(ItemType, style);
            Note($"  --- gate: {label}");
            Note("    merge: " + Try(() => _application.Resources.MergedDictionaries.Add(dictionary)));

            var tree = Tree(Node("Parent", Node("Child 1"), Node("Child 2")), Node("Leaf root"));
            root.Children.Add(tree);
            window.UpdateLayout();
            Pump(12);
            var parent = (FrameworkElement?)AllOfType(tree, ItemType).ElementAtOrDefault(0);
            if (parent is null)
            {
                Note("    no container generated; nothing to measure");
                root.Children.Clear();
                Try(() => _application.Resources.MergedDictionaries.Remove(dictionary));
                continue;
            }

            Note($"    at rest (IsExpanded=False): containers={CountContainers(tree, ItemType)} {PartSize(parent, "ItemsHost")} {PartSize(parent, "ChildHostBorder")}");
            Note("    expand through the item property: " + Read(() =>
            {
                SetOrThrow(parent, "IsExpanded", true);
                window.UpdateLayout();
                Pump(12);
                return $"containers={CountContainers(tree, ItemType)} {PartSize(parent, "ItemsHost")} {PartSize(parent, "ChildHostBorder")}";
            }));
            Note("    collapse again: " + Read(() =>
            {
                SetOrThrow(parent, "IsExpanded", false);
                window.UpdateLayout();
                Pump(12);
                return $"containers={CountContainers(tree, ItemType)} {PartSize(parent, "ItemsHost")} {PartSize(parent, "ChildHostBorder")}";
            }));
            Note("    expand through the chevron click path: " + Read(() =>
            {
                var chevron = ToggleType is null
                    ? null
                    : AllOfType(parent, ToggleType).ElementAtOrDefault(0);
                if (chevron is null)
                {
                    return "no chevron";
                }

                Note2($"      via={Click(chevron)}");
                window.UpdateLayout();
                Pump(12);
                return $"item.IsExpanded={Show(Prop(parent, "IsExpanded"))} containers={CountContainers(tree, ItemType)} {PartSize(parent, "ItemsHost")} {PartSize(parent, "ChildHostBorder")}";
            }));
            Note("    a leaf rests with its chevron hidden? " + Read(() =>
            {
                var leaf = (FrameworkElement?)AllOfType(tree, ItemType).ElementAtOrDefault(1);
                return leaf is null
                    ? "no second row"
                    : $"header={Show(Prop(leaf, "Header"))} HasItems={Show(Prop(leaf, "HasItems"))} {PartSize(leaf, "Chevron")}";
            }));
            Note("    row geometry at the top level: " + Read(() =>
                $"{parent.ActualWidth:0.##}x{parent.ActualHeight:0.##}"));

            root.Children.Clear();
            Try(() => _application.Resources.MergedDictionaries.Remove(dictionary));
            window.UpdateLayout();
            Pump(6);
        }
    }
}
