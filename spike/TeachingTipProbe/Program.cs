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

namespace TeachingTipProbe;

/// <summary>
/// Answers the three questions that decide how a TeachingTip can be built (docs/astra/audits/teachingtip.md §5),
/// by measurement instead of assumption:
///   A: does the runtime have the primitives upstream's template leans on - a Polygon whose Points a state can
///      write, a Shape with Fill/Stroke/StrokeThickness, a Popup with the properties the placement needs?
///   B: which resource ROW TYPES our markup loader turns into entries. Upstream keeps two GridLength rows and
///      eight x:Double rows; S0-b only ever proved x:Double unreachable, so GridLength and x:Int32 are new.
///   C: can a ContentControl-derived own type host a Popup INSIDE its ControlTemplate and have that popup
///      realize with a size - the route ModernWpf took because WinUI builds the popup in code instead.
/// Modes: types | rows | popup | tail | place | tip | room. 'room' exists because a claim about what a control
/// contributes to layout has to be read off a neighbour, not off the control's own slot. Nothing here is typed
/// against a guessed class: the shape types are found
/// by reflection and the template is parsed from markup, which is the route Astra's own dictionaries take.
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

        var path = Path.Combine(AppContext.BaseDirectory, $"teachingtip-probe-{_mode}.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, mode={_mode}, full log: {path}");
        return 0;
    }

    private static void Run(Application application)
    {
        var root = new StackPanel { Margin = new Thickness(24) };
        var window = new Window { Content = root, Width = 720, Height = 520, Title = "TeachingTip probe" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Pump();
                Note($"theme dictionaries={FluentThemeManager.DictionaryNames.Count} " +
                     $"overlayCornerRadius={Show(Res("OverlayCornerRadius"))} " +
                     $"textPrimary={Show(Res("TextFillColorPrimaryBrush"))}");
                if (_mode is "all" or "types")
                {
                    TypeCensus();
                }

                if (_mode is "all" or "rows")
                {
                    RowTypes();
                }

                if (_mode is "all" or "popup")
                {
                    PopupInTemplate(root, window);
                }

                if (_mode is "all" or "tail")
                {
                    TailGeometry(root, window);
                }

                if (_mode is "all" or "place")
                {
                    PlacementAnchors(root, window);
                }

                if (_mode is "tip")
                {
                    RealTip(root, window);
                }

                if (_mode is "room")
                {
                    LayoutRoom(root, window);
                }

                if (_mode is "parent")
                {
                    ParentWalk(root, window);
                }
            }
            finally
            {
                application.Shutdown();
            }
        };
        application.Run(window);
    }

    // ---------- A. the primitives upstream's template needs ----------

    private static readonly string[] Candidates =
    [
        "Polygon", "Polyline", "Path", "Line", "Rectangle", "Ellipse", "Shape", "Popup", "PopupRoot",
        "ContentControl", "Grid", "StackPanel", "ScrollViewer", "TextBlock", "Border", "Button",
    ];

    private static readonly string[] PropertiesOfInterest =
    [
        "Points", "Data", "Fill", "Stroke", "StrokeThickness", "IsOpen", "Child", "Placement", "PlacementTarget",
        "PlacementMode", "HorizontalOffset", "VerticalOffset", "AllowsTransparency", "WindowLocation", "Width",
        "Height", "CornerRadius", "StaysOpen", "Topmost", "Opacity", "Tag",
    ];

    private static void TypeCensus()
    {
        Note(string.Empty);
        Note("=== A. primitives a TeachingTip template would need (by simple name, across the loaded UI assemblies) ===");
        var assemblies = new[] { typeof(Button).Assembly, typeof(Popup).Assembly, typeof(UIElement).Assembly }
            .Where(a => a is not null)
            .Distinct()
            .ToArray();
        foreach (var name in Candidates)
        {
            Type? found = null;
            foreach (var assembly in assemblies)
            {
                found = assembly.GetTypes().FirstOrDefault(t => t.Name == name && t.Namespace?.StartsWith("Jalium.UI", StringComparison.Ordinal) == true);
                if (found is not null)
                {
                    break;
                }
            }

            if (found is null)
            {
                Note($"  {name}: NOT PRESENT in {string.Join(", ", assemblies.Select(a => a.GetName().Name))}");
                continue;
            }

            var own = found
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(f => f.FieldType.Name.Contains("DependencyProperty", StringComparison.Ordinal))
                .Select(f => f.Name.EndsWith("Property", StringComparison.Ordinal) ? f.Name[..^"Property".Length] : f.Name)
                .ToHashSet(StringComparer.Ordinal);
            var hit = PropertiesOfInterest.Where(p => own.Contains(p)).ToArray();
            var ctor = found.GetConstructor(Type.EmptyTypes);
            Note($"  {name} = {found.FullName} : {found.BaseType?.Name}" +
                 $" | public={found.IsPublic} abstract={found.IsAbstract}" +
                 $" | own DPs[{string.Join(",", hit.Length == 0 ? Array.Empty<string>() : hit)}]" +
                 $" | parameterless ctor={(ctor is null ? "none" : "yes")}");
        }
    }

    // ---------- B. which row types survive our markup loader ----------

    private static void RowTypes()
    {
        Note(string.Empty);
        Note("=== B. resource row types through XamlReader.Parse (the route Astra's dictionaries take) ===");
        Note("    one row per parse: a dictionary that throws once loses every row in it, which is what S0-b's");
        Note("    'x:Double is unreachable' reading hid - through this loader it is not dropped, it is fatal.");
        foreach (var (name, row) in Rows)
        {
            const string Open = "<ResourceDictionary xmlns=\"http://schemas.jalium.ui/2024\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">";
            try
            {
                if (XamlReader.Parse(Open + row + "</ResourceDictionary>") is not ResourceDictionary dictionary)
                {
                    Note($"  {name}: parse returned something that is not a dictionary");
                    continue;
                }

                var found = dictionary.TryGetValue("ProbeRow", out var value) ? value : null;
                Note($"  {name}: " + (found is null ? "parsed, but the key is MISSING from the dictionary" : $"{found.GetType().Name} = {Trim(found.ToString() ?? "?")}"));
            }
            catch (Exception exception)
            {
                Note($"  {name}: parse THREW {exception.GetType().Name}: {Trim(exception.Message)}");
            }
        }

        Note("  through the theme pipeline for comparison: " +
             $"OverlayCornerRadius={Show(Res("OverlayCornerRadius"))} " +
             $"TextFillColorPrimaryBrush={Show(Res("TextFillColorPrimaryBrush"))}");
    }

    private static readonly (string Name, string Row)[] Rows =
    [
        ("GridLength", "<GridLength x:Key=\"ProbeRow\">8</GridLength>"),
        ("x:Double", "<x:Double x:Key=\"ProbeRow\">40</x:Double>"),
        ("x:Int32", "<x:Int32 x:Key=\"ProbeRow\">4</x:Int32>"),
        ("Thickness", "<Thickness x:Key=\"ProbeRow\">0,12,0,0</Thickness>"),
        ("CornerRadius", "<CornerRadius x:Key=\"ProbeRow\">8,8,8,8</CornerRadius>"),
        ("SolidColorBrush", "<SolidColorBrush x:Key=\"ProbeRow\" Color=\"#0DFFFFFF\" />"),
        ("StaticResource alias", "<StaticResource x:Key=\"ProbeRow\" ResourceKey=\"TextFillColorPrimaryBrush\" />"),
        ("FontFamily", "<FontFamily x:Key=\"ProbeRow\">Segoe UI Symbol</FontFamily>"),
        ("GridLength star", "<GridLength x:Key=\"ProbeRow\">*</GridLength>"),
    ];

    // ---------- C. a Popup inside a template on our own type ----------

    /// <summary>
    /// Stand-in for the control the audit proposes (FluentTeachingTip : ContentControl). The opt-in call is the
    /// whole reason this probe exists in this shape: without it the template is stored and never built, which is
    /// exactly what the InfoBar batch measured (docs/astra/audits/infobar.md, Controls/Layout/FluentInfoBar.cs:51).
    /// </summary>
    private sealed class ProbeTip : ContentControl
    {
        public ProbeTip() => UseTemplateContentManagement();
    }

    private static void PopupInTemplate(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== C. can a ContentControl-derived type host a Popup inside its template? ===");
        var tip = new ProbeTip { Width = 200, Height = 60 };
        var template = ParseTemplate("""
            <ControlTemplate xmlns="http://schemas.jalium.ui/2024" TargetType="ContentControl">
              <Grid>
                <Border Name="Card" Background="#FF112233" Width="200" Height="60" />
                <Popup Name="PART_Popup">
                  <Border Name="PopupCard" Background="#FF00AA00" Width="140" Height="44" />
                </Popup>
              </Grid>
            </ControlTemplate>
            """);
        if (template is null)
        {
            Note("  no template to test; see the parse error above");
            return;
        }

        tip.Template = template;
        root.Children.Add(tip);
        window.UpdateLayout();
        tip.Measure(new Size(400, 200));
        Note($"  template assigned: {tip.Template is not null}; measured desired {tip.DesiredSize:0.##}");
        Pump(8);
        Note($"  after pump: tip {tip.ActualWidth:0.##}x{tip.ActualHeight:0.##}, visual children {CountChildren(tip)}");
        ReportTree(window);

        Note(string.Empty);
        Note("  now opening the popup that the template holds:");
        var popup = FindByName(window, "PART_Popup");
        if (popup is null)
        {
            Note("    PART_Popup is not in the window tree at all");
        }
        else
        {
            Note($"    found {popup.GetType().FullName}");
            var isOpen = popup.GetType().GetProperty("IsOpen");
            Note("    set IsOpen=true -> " + Try(() => isOpen!.SetValue(popup, true)));
            Pump(8);
            Note($"    IsOpen reads {Read(() => isOpen?.GetValue(popup))}");
            var child = FindByName(window, "PopupCard");
            if (child is FrameworkElement element)
            {
                Note($"    PopupCard {element.ActualWidth:0.##}x{element.ActualHeight:0.##}, ancestors: {Ancestors(element)}");
            }
            else
            {
                Note("    PopupCard " + (child is null ? "not found anywhere in the window tree" : "found but not a FrameworkElement"));
            }

            ReportTree(window);
        }
    }

    // ---------- F. the shipping control, in a window of its own ----------

    /// <summary>
    /// The same subject the xunit class builds, in a window the probe owns. The suite reports "no card" for every
    /// part lookup, and the only way to tell a product defect from the shared host's popup limit is to open a real
    /// tip where a popup demonstrably does realize (mode popup).
    /// </summary>
    private static void RealTip(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== F. FluentTeachingTip built by its own style ===");
        var anchor = new Border
        {
            Width = 100,
            Height = 40,
            Background = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80)),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(40, 120, 0, 0),
        };
        var tip = new FluentJalium.Controls.FluentTeachingTip
        {
            Target = anchor,
            PreferredPlacement = FluentJalium.Controls.FluentTeachingTipPlacementMode.Top,
            Title = "Three shortcuts",
            Subtitle = "Hold Ctrl.",
            Content = "The editor keeps a list.",
            ActionButtonContent = "Got it",
            CloseButtonContent = "Dismiss",
        };
        var surface = new Grid();
        surface.Children.Add(anchor);
        surface.Children.Add(tip);
        root.Children.Add(surface);
        window.UpdateLayout();
        Pump(8);
        Report(window, tip, anchor, "closed");

        tip.IsOpen = true;
        window.UpdateLayout();
        Pump(10);
        Report(window, tip, anchor, "open");
    }

    /// <summary>
    /// G. Does the measure override move anything at all? A tip sits between two 20 DIP rules in a StackPanel and
    /// the claim is read off where the lower rule lands, closed and then open, because ActualWidth is the slot a
    /// stretching panel hands out rather than the room taken from a neighbour. Run against both builds of the
    /// override to tell a fix from a no-op.
    /// </summary>
    private static void LayoutRoom(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== G. layout room: where the rule under a stacked tip lands ===");
        var above = new Border { Height = 20, Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)) };
        var below = new Border { Height = 20, Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)) };
        var tip = new FluentJalium.Controls.FluentTeachingTip
        {
            Title = "Three shortcuts",
            Subtitle = "Hold Ctrl.",
            Content = "The editor keeps a list of them under Help, long enough to want 168 DIP of height.",
        };
        var stack = new StackPanel { Width = 500, Margin = new Thickness(0, 160, 0, 0) };
        stack.Children.Add(above);
        stack.Children.Add(tip);
        stack.Children.Add(below);
        root.Children.Add(stack);
        window.UpdateLayout();
        Pump(8);
        Note($"  closed: tip desired {tip.DesiredSize.Width:0.##}x{tip.DesiredSize.Height:0.##}" +
             $" actual {tip.ActualWidth:0.##}x{tip.ActualHeight:0.##} | below {OffsetOf(below, stack)}" +
             $" | stack {OffsetOf(stack, window)}");

        tip.IsOpen = true;
        window.UpdateLayout();
        Pump(12);
        Note($"  open:   tip desired {tip.DesiredSize.Width:0.##}x{tip.DesiredSize.Height:0.##}" +
             $" actual {tip.ActualWidth:0.##}x{tip.ActualHeight:0.##} | below {OffsetOf(below, stack)}");

        tip.Target = above;
        tip.IsOpen = false;
        window.UpdateLayout();
        Pump(8);
        tip.IsOpen = true;
        window.UpdateLayout();
        Pump(12);
        Note($"  re-opened with a target: tip desired {tip.DesiredSize.Width:0.##}x{tip.DesiredSize.Height:0.##}" +
             $" actual {tip.ActualWidth:0.##}x{tip.ActualHeight:0.##} | below {OffsetOf(below, stack)}");
    }

    /// <summary>
    /// H. Can a control find its own window without walking the visual tree? The gate in
    /// AstraGateTests.Theme_kernel_stays_free_of_repair_loops_and_reflection bans VisualTreeHelper in product
    /// code, so the placement fit check needs another route. Reports every candidate the compiler accepts and
    /// where it actually lands for a mounted tip.
    /// </summary>
    private static void ParentWalk(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== H. reaching the host window from inside a control ===");
        var anchor = new Border { Width = 100, Height = 40 };
        var tip = new FluentJalium.Controls.FluentTeachingTip { Target = anchor, Title = "T", Content = "C" };
        var surface = new Grid();
        surface.Children.Add(anchor);
        surface.Children.Add(tip);
        root.Children.Add(surface);
        window.UpdateLayout();
        Pump(6);

        Note("  FrameworkElement.Parent: " + Read(() =>
        {
            var chain = new List<string>();
            DependencyObject? node = tip;
            for (var steps = 0; steps < 12 && node is not null; steps++)
            {
                chain.Add(node.GetType().Name);
                node = ((FrameworkElement)node).Parent;
            }

            return string.Join(" < ", chain);
        }));
        Note("  Application.Current.MainWindow: " + Show(_application.MainWindow?.GetType().Name));
        Note("  same instance as the shown window: " + (ReferenceEquals(_application.MainWindow, window)));
        Note("  tip is inside MainWindow.Content: " + Read(() =>
        {
            var found = false;
            Walk(_application.MainWindow!, node => found |= ReferenceEquals(node, tip), 40);
            return found;
        }));
    }

    private static void Report(Window windowRoot, FluentJalium.Controls.FluentTeachingTip tip, Border anchor, string stage)
    {
        Note(string.Empty);
        Note($"  {stage}: style={(tip.Style is null ? "null" : "assigned")} template={tip.Template is not null}" +
             $" effective={tip.EffectivePlacement} border={tip.BorderThickness}" +
             $" tip {tip.ActualWidth:0.##}x{tip.ActualHeight:0.##} children={CountChildren(tip)}");
        var names = new List<string>();
        Walk(windowRoot, node =>
        {
            if (NameOf(node) is { Length: > 0 } name)
            {
                var size = node is FrameworkElement element ? $" {element.ActualWidth:0.##}x{element.ActualHeight:0.##}" : string.Empty;
                names.Add($"{name}:{node.GetType().Name}{size}");
            }
        }, 40);
        Note("    named nodes: " + (names.Count == 0 ? "none" : string.Join(", ", names)));
        if (FindByName(windowRoot, "PART_Popup") is { } popup)
        {
            Note($"    popup IsOpen={Read(() => popup.GetType().GetProperty("IsOpen")?.GetValue(popup))}" +
                 $" offsets={Read(() => popup.GetType().GetProperty("HorizontalOffset")?.GetValue(popup))}," +
                 $"{Read(() => popup.GetType().GetProperty("VerticalOffset")?.GetValue(popup))}" +
                 $" child={Read(() => popup.GetType().GetProperty("Child")?.GetValue(popup)?.GetType().Name)}");
            if (FindByName(windowRoot, "Container") is FrameworkElement container)
            {
                Note($"    container {container.ActualWidth:0.##}x{container.ActualHeight:0.##} at " +
                     OffsetOf(container, anchor));
            }
            else
            {
                // The popup element is in this window's tree but its realized content is not, so walk the child
                // object itself and report where that content ended up.
                if (popup.GetType().GetProperty("Child")?.GetValue(popup) is DependencyObject child)
                {
                    var inner = new List<string>();
                    Walk(child, node =>
                    {
                        if (NameOf(node) is { Length: > 0 } name)
                        {
                            var size = node is FrameworkElement element
                                ? $" {element.ActualWidth:0.##}x{element.ActualHeight:0.##}"
                                : string.Empty;
                            inner.Add($"{name}:{node.GetType().Name}{size}");
                        }
                    }, 40);
                    Note($"    the popup's own child holds {(inner.Count == 0 ? "nothing named" : string.Join(", ", inner))}");
                    Note("    ancestors of that child: " + Ancestors(child));
                }
                else
                {
                    Note("    the popup has no Child at all");
                }
            }
        }
        else
        {
            Note("    no PART_Popup in the window tree");
        }
    }

    private static ControlTemplate? ParseTemplate(string markup)
    {
        try
        {
            return (ControlTemplate)XamlReader.Parse(markup)!;
        }
        catch (Exception exception)
        {
            Note("  template parse THREW " + exception.GetType().Name + ": " + Trim(exception.Message));
            return null;
        }
    }

    // ---------- D. the four things a tail needs, measured in the production markup shape ----------

    /// <summary>
    /// A dictionary in the exact shape Styles/TeachingTip.jalxaml will have: real rows, a named style, a
    /// template whose first node is the 5-column tail grid upstream uses, and a trigger set that writes the
    /// tail's geometry the way upstream's PlacementStates do. Four questions come out of one run:
    ///   D1 does a Polygon accept inline Points at all, and do Fill/Stroke/StrokeThickness survive on it;
    ///   D2 does a ThemeResource row carrying a GridLength reach a ColumnDefinition's Width;
    ///   D3 can a template trigger write Points and an attached Grid.Row onto a named part;
    ///   D4 what type/spellings does Popup.Placement want, since the tip has to place itself by them.
    /// </summary>
    private const string TailMarkup = """
        <ResourceDictionary xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
          <GridLength x:Key="TailShort">8</GridLength>
          <GridLength x:Key="TailMargin">10</GridLength>
          <Style x:Key="TailStyle" TargetType="ContentControl">
            <Setter Property="Template">
              <ControlTemplate TargetType="ContentControl">
                <Grid>
                  <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="{ThemeResource TailShort}" />
                    <ColumnDefinition Width="{ThemeResource TailMargin}" />
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="{ThemeResource TailMargin}" />
                    <ColumnDefinition Width="{ThemeResource TailShort}" />
                  </Grid.ColumnDefinitions>
                  <Border Background="#FF202020" Grid.Row="0" Grid.RowSpan="5" Grid.Column="0" Grid.ColumnSpan="5" />
                  <Polygon Name="Tail" Grid.Row="4" Grid.Column="2" Points="0,0 10,10 20,0"
                           Fill="#FF336699" Stroke="#FFAABBCC" StrokeThickness="1" />
                  <Popup Name="TailPopup" Placement="Bottom" HorizontalOffset="3" VerticalOffset="-4" AllowsTransparency="True">
                    <Border Name="TailPopupCard" Background="#FF00AA00" Width="120" Height="36" />
                  </Popup>
                </Grid>
                <ControlTemplate.Triggers>
                  <Trigger Property="Tag" Value="X">
                    <Setter TargetName="Tail" Property="Points" Value="10,0 0,10 10,20" />
                    <Setter TargetName="Tail" Property="Grid.Row" Value="2" />
                    <Setter TargetName="Tail" Property="Grid.Column" Value="0" />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>
        </ResourceDictionary>
        """;

    private static void TailGeometry(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== D. tail geometry, GridLength rows, trigger writes and popup placement ===");

        ResourceDictionary? dictionary;
        try
        {
            dictionary = (ResourceDictionary)XamlReader.Parse(TailMarkup)!;
            Note("  dictionary parsed; rows: " + string.Join(", ", new[] { "TailShort", "TailMargin" }.Select(k => $"{k}={Show(dictionary[k])}")));
        }
        catch (Exception exception)
        {
            Note("  dictionary parse THREW " + exception.GetType().Name + ": " + Trim(exception.Message));
            return;
        }

        Note("  merge into a shown tree -> " + Try(() => root.Resources.MergedDictionaries.Add(dictionary)));
        var tip = new ProbeTip { Width = 260, Height = 120, Style = (Style)dictionary["TailStyle"]! };
        root.Children.Add(tip);
        window.UpdateLayout();
        Pump(8);

        // D1: the Polygon itself.
        var tail = FindByName(window, "Tail");
        if (tail is null)
        {
            Note("  D1: no part named Tail - the template never built; stop here");
            return;
        }

        Note($"  D1: {tail.GetType().FullName} {tail.GetType().BaseType?.Name} built");
        foreach (var name in new[] { "Points", "Fill", "Stroke", "StrokeThickness" })
        {
            var property = tail.GetType().GetProperty(name);
            var value = Read(() => property?.GetValue(tail));
            Note($"      .{name} = {(property is null ? "no such CLR property" : Trim(value))}");
        }

        // D2: did the two GridLength rows land on the columns they were bound to?
        var grid = VisualTreeHelper.GetParent(tail);
        var columns = grid?.GetType().GetProperty("ColumnDefinitions")?.GetValue(grid);
        if (columns is null)
        {
            Note("  D2: no ColumnDefinitions readable on the template's Grid");
        }
        else
        {
            var widths = ((System.Collections.IEnumerable)columns).Cast<object>()
                .Select(c => c.GetType().GetProperty("Width")?.GetValue(c) is { } w ? Trim(w.ToString() ?? "?") : "?")
                .ToArray();
            Note($"  D2: column widths via {{ThemeResource}} = {string.Join(" | ", widths)}   (upstream wants 8 | 10 | * | 10 | 8)");
        }

        // D3: a trigger writing the shape and the attached row/column.
        Note($"  D3: resting Points={Trim(Read(() => tail.GetType().GetProperty("Points")?.GetValue(tail)) ?? "?")}" +
             $" Grid.Row={ReadAttached(tail, "Row")} Grid.Column={ReadAttached(tail, "Column")}");
        Note("  D3: " + Try(() =>
        {
            tip.Tag = "X";
        }));
        Pump(6);
        Note($"      after Tag=X -> Points={Trim(Read(() => tail.GetType().GetProperty("Points")?.GetValue(tail)) ?? "?")}" +
             $" Grid.Row={ReadAttached(tail, "Row")} Grid.Column={ReadAttached(tail, "Column")}");

        // D4: how the popup spells placement, and whether the offsets survive.
        var popup = FindByName(window, "TailPopup");
        if (popup is null)
        {
            Note("  D4: no TailPopup in the tree");
            return;
        }

        var placement = popup.GetType().GetProperty("Placement");
        var placementType = placement?.PropertyType;
        Note($"  D4: Popup.Placement is {placementType?.FullName ?? "?"}" +
             (placementType?.IsEnum == true ? $" with {string.Join(", ", Enum.GetNames(placementType))}" : string.Empty));
        Note($"      reads back {Trim(Read(() => placement?.GetValue(popup)) ?? "?")}");
        foreach (var name in new[] { "HorizontalOffset", "VerticalOffset", "AllowsTransparency", "PlacementTarget" })
        {
            var property = popup.GetType().GetProperty(name);
            Note($"      .{name} = {(property is null ? "no such CLR property" : Trim(Read(() => property.GetValue(popup)) ?? "?"))}");
        }

        var open = FindByName(window, "TailPopupCard");
        Note("      card: " + (open is FrameworkElement fe
            ? $"{fe.ActualWidth:0.##}x{fe.ActualHeight:0.##} under {Ancestors(fe)}"
            : "not realized (Popup.Placement=Bottom was set from markup, IsOpen was not)"));
    }

    // ---------- E. what each Popup placement anchor is worth to a tip ----------

    /// <summary>
    /// The tip has to land next to its target, and the runtime's own PlacementMode has none of WinUI's
    /// per-edge/per-corner values (D4), so the placement code is Relative + two offsets. That only works if
    /// Relative anchors on the target's origin and if an offset is a plain additive DIP - neither of which is
    /// documented, so both are read here, together with what the four cardinal built-ins do for comparison.
    /// </summary>
    private static void PlacementAnchors(Panel root, Window window)
    {
        Note(string.Empty);
        Note("=== E. popup placement anchors, offsets and auto-sized child ===");

        var canvas = new Canvas { Width = 620, Height = 420, Background = new SolidColorBrush(Color.FromRgb(0x10, 0x10, 0x10)) };
        var target = new Border { Width = 100, Height = 40, Background = new SolidColorBrush(Color.FromRgb(0x40, 0x40, 0x40)) };
        Canvas.SetLeft(target, 200);
        Canvas.SetTop(target, 150);
        canvas.Children.Add(target);
        root.Children.Add(canvas);
        window.UpdateLayout();
        Pump(6);
        Note($"  target at ({target.ActualWidth:0.##}x{target.ActualHeight:0.##}) reads origin " + OffsetOf(target, canvas));

        foreach (var (name, mode, horizontal, vertical) in PlacementCases)
        {
            var card = new Border
            {
                Width = 120,
                Height = 50,
                Background = new SolidColorBrush(Color.FromRgb(0x00, 0xAA, 0x00)),
            };
            var popup = new Popup
            {
                PlacementTarget = target,
                Placement = mode,
                HorizontalOffset = horizontal,
                VerticalOffset = vertical,
                AllowsTransparency = true,
                Child = card,
                IsOpen = true,
            };
            Note(string.Empty);
            Note($"  {name}: " + Try(() => root.Children.Add(popup)));
            window.UpdateLayout();
            Pump(8);
            Note($"      card {card.ActualWidth:0.##}x{card.ActualHeight:0.##}; relative to target -> {OffsetOf(card, target)}");
            Try(() =>
            {
                popup.IsOpen = false;
                root.Children.Remove(popup);
            });
            Pump(2);
        }
    }

    private static readonly (string Name, PlacementMode Mode, double Horizontal, double Vertical)[] PlacementCases =
    [
        ("Relative 0,0", PlacementMode.Relative, 0, 0),
        ("Relative 50,50", PlacementMode.Relative, 50, 50),
        ("Relative -120,-50 (one card away, up-left)", PlacementMode.Relative, -120, -50),
        ("Bottom 0,0", PlacementMode.Bottom, 0, 0),
        ("Top 0,0", PlacementMode.Top, 0, 0),
        ("Left 0,0", PlacementMode.Left, 0, 0),
        ("Right 0,0", PlacementMode.Right, 0, 0),
        ("Center 0,0", PlacementMode.Center, 0, 0),
    ];

    /// <summary>Translation of <paramref name="node"/>'s origin in <paramref name="to"/>'s coordinate space.</summary>
    private static string OffsetOf(FrameworkElement node, UIElement to)
    {
        try
        {
            var origin = node.TranslatePoint(new Point(0, 0), to);
            return $"x={origin.X:0.##} y={origin.Y:0.##}";
        }
        catch (Exception exception)
        {
            var inner = exception.InnerException ?? exception;
            return "threw " + inner.GetType().Name + ": " + Trim(inner.Message);
        }
    }

    private static string ReadAttached(DependencyObject node, string name)
    {
        try
        {
            var dp = typeof(Grid).GetField(name + "Property", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (dp is null)
            {
                return $"no Grid.{name}Property";
            }

            var value = typeof(DependencyObject).GetMethod("GetValue")!.Invoke(node, new object[] { dp });
            return Trim(value?.ToString() ?? "null");
        }
        catch (Exception exception)
        {
            return "threw " + exception.GetType().Name;
        }
    }

    private static void ReportTree(DependencyObject root)
    {
        var builder = new StringBuilder("  tree:");
        var nodes = 0;
        Walk(root, node =>
        {
            nodes++;
            if (node is Visual && node.GetType().Name.Contains("Popup", StringComparison.Ordinal))
            {
                var size = node is FrameworkElement fe ? $" {fe.ActualWidth:0.##}x{fe.ActualHeight:0.##}" : string.Empty;
                builder.Append($"\n    popup-shaped: {node.GetType().Name}{size}");
            }
        }, 40);
        builder.Append($"\n    total nodes walked={nodes}");
        Note(builder.ToString());
    }

    // ---------- helpers ----------

    private static int CountChildren(DependencyObject node)
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

    private static DependencyObject? FindByName(DependencyObject root, string name)
    {
        DependencyObject? found = null;
        Walk(root, node =>
        {
            if (found is not null)
            {
                return;
            }

            if (NameOf(node) == name)
            {
                found = node;
            }
        }, 40);
        return found;
    }

    private static string? NameOf(DependencyObject node)
    {
        var property = node.GetType().GetProperty("Name");
        try
        {
            return property?.GetValue(node) as string;
        }
        catch
        {
            return null;
        }
    }

    private static string Ancestors(DependencyObject node)
    {
        var chain = new List<string>();
        var current = node;
        for (var steps = 0; steps < 20 && current is not null; steps++)
        {
            chain.Add(current.GetType().Name);
            current = VisualTreeHelper.GetParent(current) ?? LogicalParent(current);
            if (current is Window)
            {
                break;
            }
        }

        return string.Join(" < ", chain);
    }

    private static DependencyObject? LogicalParent(DependencyObject node)
    {
        var property = node.GetType().GetProperty("Parent");
        try
        {
            return property?.GetValue(node) as DependencyObject;
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

        for (var i = 0; i < count; i++)
        {
            try
            {
                Walk(VisualTreeHelper.GetChild(node, i), visit, maxDepth - 1);
            }
            catch
            {
                // A child that cannot be walked is not worth the probe; the parent row already says it exists.
            }
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

    private static string Show(object? value) => value is null ? "null" : Trim(value.ToString() ?? "?");

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
            var top = (exception.StackTrace ?? string.Empty).Split('\n').FirstOrDefault(string.Empty).Trim();
            return exception.GetType().Name + ": " + Trim(exception.Message) + " @" + Trim(top);
        }
    }

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
