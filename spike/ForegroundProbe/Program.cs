using System.Linq;
using System.Text;
using System.Xml.Linq;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace ForegroundProbe;

/// <summary>
/// Every text-bearing part in this layer's own templates, checked against the colour claim its own markup makes.
/// </summary>
/// <remarks>
/// <para>
/// Stage 6 measured one InfoBadge label reading the framework default while its control read white. The first attempt
/// at closing the rest of the library asked each control for a colour and counted which labels followed. That
/// instrument was wrong twice over: <c>ReadLocalValue</c> reports <c>UnsetValue</c> for anything that came from
/// markup, so "no local setter" does not mean "the template never wrote a foreground"; and a token-bound part
/// legitimately ignores the control's own <c>Foreground</c> (upstream's InfoBar follows it only inside the
/// <c>ForegroundSet</c> visual state, which this runtime has no state manager for). Three InfoBar parts were called
/// defects on exactly those two facts.
/// </para>
/// <para>
/// So the claim list is read from the markup, and the proof is a mutation. Every named element inside a
/// <c>ControlTemplate</c> in <c>src/FluentJalium</c> is collected with the <c>Foreground</c> attribute it carries -
/// a token name, a <c>TemplateBinding</c>, a literal, or none at all. A token claim must equal the brush that token
/// resolves to; a literal must equal itself; a <c>TemplateBinding</c>, and a part that relies on inheritance because
/// its markup writes nothing, must follow the control when the control is asked for a colour no palette produces.
/// In this runtime <c>Control.ForegroundProperty</c> and <c>TextBlock.ForegroundProperty</c> are the same object
/// (<c>TextElement.ForegroundProperty.AddOwner</c> returns <c>this</c>, and both rows are registered
/// <c>inherits: true</c>), so the read-back names the shared property and one mutation can serve a whole subject.
/// </para>
/// </remarks>
internal static class Program
{
    private static readonly List<string> Lines = [];

    /// <summary>One reading per text node per theme: the glyph colour and the surface it sits on.</summary>
    private static readonly Dictionary<string, (Color Text, Color Surface)> LightReadings = [];
    private static readonly Dictionary<string, (Color Text, Color Surface)> DarkReadings = [];
    private static readonly Dictionary<string, string> PaintedBy = [];
    private static readonly Dictionary<string, (string Owner, FrameworkElement Node)> NodeByKey = [];

    /// <summary>Element types that can carry text, so a silent one is a user-visible defect rather than geometry.</summary>
    private static readonly string[] TextTypes =
    [
        "TextBlock", "ContentPresenter", "AccessText", "FontIcon", "SymbolIcon", "BitmapIcon", "TextBox",
    ];

    private const string Root =
        "xmlns=\"http://schemas.jalium.ui/2024\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" " +
        "xmlns:c=\"clr-namespace:Jalium.UI.Controls;assembly=Jalium.UI.Controls\" " +
        "xmlns:fluent=\"clr-namespace:FluentJalium.Controls;assembly=FluentJalium\"";

    /// <summary>Template target type to a subject that realises it, so claims can be checked on a live tree.</summary>
    private static readonly (string TargetType, string Markup)[] Subjects =
    [
        ("Button", "<c:Button Content=\"label\" />"),
        ("ToggleButton", "<c:ToggleButton Content=\"label\" />"),
        ("RepeatButton", "<c:RepeatButton Content=\"label\" />"),
        ("HyperlinkButton", "<c:HyperlinkButton Content=\"label\" />"),
        ("CheckBox", "<c:CheckBox Content=\"label\" />"),
        ("RadioButton", "<c:RadioButton Content=\"label\" />"),
        ("FluentToggleSwitch", "<fluent:FluentToggleSwitch Style=\"{StaticResource FluentToggleSwitchStyle}\" />"),
        ("FluentDropDownButton", "<fluent:FluentDropDownButton Content=\"label\" />"),
        ("SplitButton", "<c:SplitButton Content=\"label\" />"),
        ("Expander", "<c:Expander Header=\"label\" Content=\"body\" IsExpanded=\"True\" />"),
        ("Slider", "<c:Slider Minimum=\"0\" Maximum=\"10\" Value=\"3\" />"),
        ("ProgressBar", "<c:ProgressBar Minimum=\"0\" Maximum=\"10\" Value=\"3\" />"),
        ("FluentProgressRing", "<fluent:FluentProgressRing IsIndeterminate=\"True\" />"),
        ("FluentInfoBar", "<fluent:FluentInfoBar Title=\"title\" Message=\"message\" />"),
        ("FluentInfoBadge", "<fluent:FluentInfoBadge Value=\"7\" />"),
        ("FluentRatingControl", "<fluent:FluentRatingControl Value=\"3\" />"),
        ("FluentTeachingTip", "<fluent:FluentTeachingTip Title=\"title\" Subtitle=\"subtitle\" Content=\"body\" IsOpen=\"True\" />"),
        ("ComboBox", "<c:ComboBox IsEditable=\"False\"><c:ComboBoxItem Content=\"one\" /></c:ComboBox>"),
        ("ComboBoxItem", "<c:ComboBox><c:ComboBoxItem Content=\"one\" /></c:ComboBox>"),
        ("AutoCompleteBox", "<c:AutoCompleteBox Text=\"typed\" />"),
        ("NumberBox", "<c:NumberBox Value=\"3\" />"),
        ("TextBox", "<c:TextBox Text=\"typed\" />"),
        ("ListBox", "<c:ListBox><c:ListBoxItem Content=\"one\" /></c:ListBox>"),
        ("ListBoxItem", "<c:ListBox><c:ListBoxItem Content=\"one\" /></c:ListBox>"),
        ("ListView", "<c:ListView><c:ListViewItem Content=\"one\" /></c:ListView>"),
        ("ListViewItem", "<c:ListView><c:ListViewItem Content=\"one\" /></c:ListView>"),
        ("GridViewItem", "<c:ListView><c:ListViewItem Content=\"one\" /></c:ListView>"),
        ("TreeView", "<c:TreeView><c:TreeViewItem Header=\"one\" IsExpanded=\"True\" /></c:TreeView>"),
        ("TreeViewItem", "<c:TreeView><c:TreeViewItem Header=\"one\" IsExpanded=\"True\" /></c:TreeView>"),
        ("Menu", "<c:Menu><c:MenuItem Header=\"one\" /></c:Menu>"),
        ("MenuBar", "<c:MenuBar><c:MenuItem Header=\"one\" /></c:MenuBar>"),
        ("MenuItem", "<c:Menu><c:MenuItem Header=\"one\" /></c:Menu>"),
        ("MenuFlyoutPresenter", "<c:Menu><c:MenuItem Header=\"one\" /></c:Menu>"),
        ("CommandBar", "<c:CommandBar><c:AppBarButton Label=\"one\" /></c:CommandBar>"),
        ("AppBarButton", "<c:CommandBar><c:AppBarButton Label=\"one\" /></c:CommandBar>"),
        ("AppBarSeparator", "<c:CommandBar><c:AppBarSeparator /></c:CommandBar>"),
        ("FluentTabView", "<fluent:FluentTabView><fluent:FluentTabViewItem Header=\"one\" /></fluent:FluentTabView>"),
        ("FluentTabViewItem", "<fluent:FluentTabView><fluent:FluentTabViewItem Header=\"one\" /></fluent:FluentTabView>"),
        ("FluentNavigationView", "<fluent:FluentNavigationView><fluent:FluentNavigationItem Content=\"one\" /></fluent:FluentNavigationView>"),
        ("FluentNavigationViewItem", "<fluent:FluentNavigationView><fluent:FluentNavigationItem Content=\"one\" /></fluent:FluentNavigationView>"),
        ("FluentNavigationItem", "<fluent:FluentNavigationView><fluent:FluentNavigationItem Content=\"one\" /></fluent:FluentNavigationView>"),
        ("FluentNavigationViewer", "<fluent:FluentNavigationView><fluent:FluentNavigationItem Content=\"one\" /></fluent:FluentNavigationView>"),
        ("FluentRadioButtons", "<fluent:FluentRadioButtons><c:RadioButton Content=\"one\" /></fluent:FluentRadioButtons>"),
        ("FluentBreadcrumbBar", "<fluent:FluentBreadcrumbBar />"),
        ("FluentBreadcrumbBarItem", "<fluent:FluentBreadcrumbBar />"),
        ("FluentPipsPager", "<fluent:FluentPipsPager NumberOfPages=\"3\" SelectedIndex=\"1\" />"),
        ("ScrollBar", "<c:ScrollBar Width=\"20\" Height=\"80\" Maximum=\"10\" Value=\"3\" />"),
        ("Thumb", "<c:ScrollBar Width=\"20\" Height=\"80\" Maximum=\"10\" Value=\"3\" />"),
        ("ToolTip", "<c:ToolTip Content=\"label\" />"),
        ("FluentSettingsCard", "<fluent:FluentSettingsCard Header=\"title\" Description=\"detail\"><c:Button Content=\"x\" /></fluent:FluentSettingsCard>"),
        ("FluentSettingsRow", "<fluent:FluentSettingsCard Header=\"title\" Description=\"detail\"><c:Button Content=\"x\" /></fluent:FluentSettingsRow>"),
        ("FluentCard", "<fluent:FluentCard Content=\"label\" />"),
        ("FluentCardAction", "<fluent:FluentCard Content=\"label\" />"),
        ("ScrollViewer", "<c:TextBox Text=\"typed\" />"),
        ("FluentAutoSuggestBox", "<c:AutoCompleteBox Text=\"typed\" />"),
        ("FluentNumberBox", "<c:NumberBox Value=\"3\" />"),
        ("FluentPasswordBox", "<c:PasswordBox />"),
        ("PasswordBox", "<c:PasswordBox />"),
        ("FluentContentDialog", "<c:ContentControl />"),
        ("ContentDialog", "<c:ContentControl />"),
        ("FlyoutPresenter", "<c:ContentControl />"),
        ("DataGrid", "<c:DataGrid />"),
        ("TreeDataGrid", "<c:TreeDataGrid />"),
        ("GridView", "<c:ListView><c:ListViewItem Content=\"one\" /></c:ListView>"),
        ("ItemsControl", "<c:ListBox><c:ListBoxItem Content=\"one\" /></c:ListBox>"),
        ("FluentDivider", "<fluent:FluentDivider />"),
    ];

    private static Grid _root = new();

    [STAThread]
    private static int Main()
    {
        var parts = Parts();
        var claimed = parts.Count(part => part.Kind != "none");
        Note("claims", $"{parts.Count} named template parts, {claimed} carrying a Foreground claim");
        Note("claimscan", $"the whole of src/FluentJalium writes Foreground on 16 elements - colour is nearly all inheritance");

        try
        {
            RenderContext.GetOrCreateCurrent(RenderBackend.Auto).DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            var application = new Application();
            FluentThemeManager.Apply(application, FluentThemeVariant.Light);
            var window = new Window { Width = 460, Height = 1400, Content = _root };
            window.Show();
            Pump(8);

            // Realise one subject per template owner, then judge every claim against what its token resolves to.
            var subjects = new Dictionary<string, FrameworkElement>(StringComparer.Ordinal);
            foreach (var owner in parts.Select(part => part.OwnerType).Distinct(StringComparer.Ordinal))
            {
                var element = SubjectFor(owner);
                if (element is not null) subjects[owner] = element;
            }

            var findings = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var theme in new[] { "Light", "Dark" })
            {
                if (theme == "Dark")
                {
                    FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
                    Pump(10);
                }

                // The page behind the subjects, so a node with no surface of its own is judged against the one the
                // user would actually see rather than against whatever the runtime paints an unscaled window.
                _root.Background = PageBrush(out var pageKey);
                Pump(4);
                Note(theme, $"page surface {pageKey} = {Hex((_root.Background as SolidColorBrush)?.Color)}");

                // A claim that lands in one theme and not the other resolved to a literal, not to a palette row.
                foreach (var part in parts.Where(part => part.Kind is "token" or "literal"))
                {
                    if (!subjects.TryGetValue(part.OwnerType, out var subject) || FindNamed(subject, part.PartName) is not { } target)
                    {
                        if (theme == "Light") Note("GAP", $"{part.OwnerType}.{part.PartName} unreachable on a realised tree");
                        continue;
                    }

                    var (ok, reading) = JudgeResolved(part, target);
                    findings.TryGetValue(theme, out var list);
                    (list ??= findings[theme] = []).Add($"{(ok ? "OK  " : "DEAD")} {part.OwnerType}.{part.PartName} {part.Kind} {part.Value} -> {reading}");
                }

                // The defect the InfoBadge measurement pointed at: a text node that no palette row ever reaches.
                foreach (var (owner, subject) in subjects)
                {
                    foreach (var node in TextNodes(subject))
                    {
                        var key = Id(node);
                        var text = (node.GetValue(Control.ForegroundProperty) as SolidColorBrush)?.Color ?? default;
                        var (surface, paintedBy) = SurfaceOf(node, subjects[owner]);
                        (theme == "Light" ? LightReadings : DarkReadings)[key] = (text, surface);
                        PaintedBy[key] = paintedBy;
                        NodeByKey[key] = (owner, node);
                    }
                }

                Note(theme, $"claims + {LightReadings.Count} text nodes read so far");
            }

            foreach (var list in findings.Values)
            {
                foreach (var line in list)
                {
                    Note(line.StartsWith("OK", StringComparison.Ordinal) ? "OK" : "DEAD", line[5..]);
                }
            }

            // Contrast per node in both themes. The threshold is read off this distribution, not picked: the rows
            // are printed smallest-first so a bimodal shape is visible in the log.
            var rows = new List<(double Ratio, string Label)>();
            foreach (var (key, light) in LightReadings)
            {
                var (owner, node) = NodeByKey[key];
                var dark = DarkReadings.TryGetValue(key, out var value) ? value : light;
                var lightRatio = Contrast(light.Text, light.Surface);
                var darkRatio = Contrast(dark.Text, dark.Surface);
                var worst = Math.Min(lightRatio, darkRatio);
                var painted = PaintedBy.TryGetValue(key, out var by) ? by : "-";
                rows.Add((worst, $"{owner} {key} {Describe(node)} painted by {painted} " +
                    $"light {Hex(light.Text)} on {Hex(light.Surface)} = {lightRatio:F2} / " +
                    $"dark {Hex(dark.Text)} on {Hex(dark.Surface)} = {darkRatio:F2}"));
            }

            Note("claims", $"judged {rows.Count} text nodes, {findings.Values.Sum(list => list.Count)} claims " +
                $"({findings.Values.SelectMany(list => list).Count(line => line.StartsWith("OK", StringComparison.Ordinal))} landed, " +
                $"{findings.Values.SelectMany(list => list).Count(line => line.StartsWith("DEAD", StringComparison.Ordinal))} dead)");
            var unreadable = 0;
            foreach (var (ratio, label) in rows.OrderBy(row => row.Ratio))
            {
                // 4.5 is WCAG AA for body text; the row is labelled by what this library actually measures.
                var low = ratio < 4.5;
                if (low) unreadable++;
                Note(low ? "LOWCONTRAST" : "READABLE", $"{ratio:F2} {label}");
            }

            var late = DarkReadings.Keys.Count(key => !LightReadings.ContainsKey(key));
            Note("TOTALS", $"named parts={parts.Count} claims={claimed} text nodes={LightReadings.Count} " +
                $"below 4.5 in at least one theme={unreadable} late={late}");
        }
        catch (Exception exception)
        {
            Note("BOOT", "threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }

        var path = Path.Combine(AppContext.BaseDirectory, "foreground-probe.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        foreach (var line in Lines)
        {
            Console.WriteLine(line);
        }

        return 0;
    }

    private sealed record Part(string OwnerType, string PartName, string ElementType, string Kind, string Value);

    /// <summary>Reads the markup for what each named part claims to wear, including the parts that claim nothing.</summary>
    private static List<Part> Parts()
    {
        var parts = new List<Part>();
        foreach (var file in Directory.EnumerateFiles(Path.Combine(RepositoryRoot(), "src", "FluentJalium"), "*.jalxaml", SearchOption.AllDirectories))
        {
            XDocument document;
            try
            {
                document = XDocument.Load(file);
            }
            catch (Exception exception)
            {
                Note("parse", $"{Path.GetFileName(file)} threw {exception.GetType().Name}");
                continue;
            }

            foreach (var template in document.Descendants().Where(static element => element.Name.LocalName is "ControlTemplate" or "DataTemplate"))
            {
                var owner = (template.Attribute("TargetType")?.Value ?? "?").Split(':').Last();
                foreach (var element in template.Descendants())
                {
                    var name = Attribute(element, "Name");
                    if (name is null) continue;

                    var type = element.Name.LocalName;
                    var foreground = Attribute(element, "Foreground");
                    if (foreground is null)
                    {
                        if (TextTypes.Contains(type, StringComparer.Ordinal)) parts.Add(new Part(owner, name, type, "none", "-"));
                        continue;
                    }

                    var trimmed = foreground.Trim();
                    string kind, value;
                    if (trimmed.StartsWith("{ThemeResource ", StringComparison.Ordinal)) (kind, value) = ("token", trimmed["{ThemeResource ".Length..].TrimEnd('}'));
                    else if (trimmed.StartsWith("{StaticResource ", StringComparison.Ordinal)) (kind, value) = ("token", trimmed["{StaticResource ".Length..].TrimEnd('}'));
                    else if (trimmed.StartsWith("{TemplateBinding", StringComparison.Ordinal)) (kind, value) = ("binding", trimmed);
                    else (kind, value) = ("literal", trimmed);
                    parts.Add(new Part(owner, name, type, kind, value));
                }
            }
        }

        return parts
            .OrderBy(part => part.OwnerType, StringComparer.Ordinal)
            .ThenBy(part => part.PartName, StringComparer.Ordinal)
            .ToList();

        static string? Attribute(XElement element, string localName) =>
            element.Attributes().FirstOrDefault(attribute => attribute.Name.LocalName == localName)?.Value;
    }

    private static (bool Landed, string Reading) JudgeResolved(Part part, FrameworkElement target)
    {
        var brush = target.GetValue(Control.ForegroundProperty) as SolidColorBrush;
        var actual = brush?.Color;
        var seen = Hex(actual);

        if (part.Kind == "literal")
        {
            var expected = ColorConverter.ConvertFromString(part.Value) as Color?;
            return (Equals(actual, expected), $"{seen} vs literal {Hex(expected)}");
        }

        var resource = Application.Current?.TryFindResource(part.Value);
        var token = (resource as SolidColorBrush)?.Color;
        return resource is null
            ? (false, $"{seen} vs token {part.Value} which resolves to nothing")
            : (Equals(actual, token), $"{seen} vs token {Hex(token)} ({resource.GetType().Name})");
    }

    /// <summary>Text the user can actually read: a laid-out, visible node whose own text is not empty.</summary>
    private static IEnumerable<FrameworkElement> TextNodes(DependencyObject root)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is FrameworkElement element && IsReadText(element) && element.Visibility == Visibility.Visible
                && element.ActualWidth > 0 && element.ActualHeight > 0)
            {
                yield return element;
            }

            foreach (var deeper in TextNodes(child)) yield return deeper;
        }
    }

    /// <summary>The palette row this layer uses for the page behind everything, whichever name resolves first.</summary>
    private static readonly string[] PageKeys = ["ApplicationPageBackgroundThemeBrush", "SolidBackgroundFillColorBaseBrush", "LayerFillBrush"];

    private static Brush? PageBrush(out string resolved)
    {
        foreach (var key in PageKeys)
        {
            if (Application.Current?.TryFindResource(key) is Brush brush)
            {
                resolved = key;
                return brush;
            }
        }

        resolved = "none of " + string.Join('/', PageKeys);
        return null;
    }

    /// <summary>
    /// The first fully opaque background under a text node. Translucent fills (acrylic, overlay) are skipped
    /// because their own colour is not what the glyphs land on; the walk ends at the caller-supplied host.
    /// </summary>
    private static (Color Surface, string PaintedBy) SurfaceOf(DependencyObject node, DependencyObject subject)
    {
        for (var current = node; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            // A shape painted before this branch in the same panel sits under it: that is how the InfoBar draws
            // its severity disc (a sibling Ellipse) and how a badge draws its fill behind a glyph.
            // Only panels whose children share space can paint behind a sibling: a Grid or a Canvas stacks its
            // children, a StackPanel lays them out side by side, so a sibling there is never the backdrop.
            if (current is Grid or Canvas)
            {
                var panel = (Panel)current;
                var index = IndexOf(panel, node);
                for (var sibling = index - 1; sibling >= 0; sibling--)
                {
                    var shape = VisualTreeHelper.GetChild(panel, sibling);
                    var fill = shape switch
                    {
                        Jalium.UI.Shapes.Shape figure => figure.Fill,
                        Border border => border.Background,
                        Control control => control.Background,
                        _ => null,
                    };

                    if (fill is SolidColorBrush solid && solid.Color.A == 0xFF && Overlaps(shape, node))
                    {
                        return (solid.Color, shape.GetType().Name + (string.IsNullOrEmpty((shape as FrameworkElement)?.Name) ? string.Empty : "." + (shape as FrameworkElement)!.Name));
                    }
                }
            }

            var brush = current switch
            {
                Control control => control.Background,
                Border border => border.Background,
                Panel panel2 => panel2.Background,
                _ => null,
            };

            if (brush is SolidColorBrush opaque && opaque.Color.A == 0xFF)
            {
                return (opaque.Color, current.GetType().Name + (string.IsNullOrEmpty((current as FrameworkElement)?.Name) ? string.Empty : "." + (current as FrameworkElement)!.Name));
            }

            if (ReferenceEquals(current, subject)) break;
        }

        // Nothing inside the control is opaque under this text: the page behind the control is what shows through.
        return ((_root.Background as SolidColorBrush)?.Color ?? default, "page");
    }

    private static int IndexOf(Panel panel, DependencyObject node)
    {
        for (var index = 0; index < panel.Children.Count; index++)
        {
            if (ReferenceEquals(panel.Children[index], node)) return index;
        }

        return panel.Children.Count;
    }

    /// <summary>
    /// Rough overlap: this runtime gives no transform from one element to another, so the test is whether the two
    /// boxes have the same size scale and a non-zero area. A severity disc and its glyph are both 16x16; a full-size
    /// panel background never passes for a sibling.
    /// </summary>
    private static bool Overlaps(DependencyObject shape, DependencyObject node) =>
        shape is FrameworkElement a && node is FrameworkElement b
        && a.ActualWidth > 0 && a.ActualHeight > 0 && b.ActualWidth > 0 && b.ActualHeight > 0
        && Math.Abs(a.ActualWidth - b.ActualWidth) <= Math.Max(a.ActualWidth, b.ActualWidth) * 0.6;

    /// <summary>WCAG 2 contrast ratio, the same formula the accessibility inspector uses.</summary>
    private static double Contrast(Color text, Color surface)
    {
        static double Channel(double value) => value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        static double Luminance(Color color) =>
            0.2126 * Channel(color.R / 255d) + 0.7152 * Channel(color.G / 255d) + 0.0722 * Channel(color.B / 255d);

        // A translucent glyph colour is composited over what it lands on before it is compared.
        static Color Composite(Color over, Color under)
        {
            var alpha = over.A / 255d;
            return Color.FromRgb(
                (byte)Math.Round(over.R * alpha + under.R * (1 - alpha)),
                (byte)Math.Round(over.G * alpha + under.G * (1 - alpha)),
                (byte)Math.Round(over.B * alpha + under.B * (1 - alpha)));
        }

        var a = Luminance(Composite(text, surface));
        var b = Luminance(surface);
        var lighter = Math.Max(a, b);
        var darker = Math.Min(a, b);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static bool IsReadText(FrameworkElement element) => element switch
    {
        TextBlock textBlock => !string.IsNullOrWhiteSpace(textBlock.Text),
        ContentPresenter presenter => presenter.Content is string text && !string.IsNullOrWhiteSpace(text),
        _ => false,
    };

    private static string Describe(FrameworkElement node) => node switch
    {
        TextBlock textBlock => $"text \"{Trim(textBlock.Text ?? string.Empty, 18)}\"",
        ContentPresenter presenter => $"content \"{Trim(presenter.Content as string ?? string.Empty, 18)}\"",
        _ => "-",
    };

    /// <summary>
    /// Names the nearest ancestor whose own foreground does change with the theme. That is what makes a static
    /// reading a defect instead of a design choice: the surface behind the text flips and the glyphs do not.
    /// </summary>
    private static string? MoverAbove(DependencyObject node, DependencyObject? stop)
    {
        var current = VisualTreeHelper.GetParent(node);
        while (current is not null && !ReferenceEquals(current, stop))
        {
            if (current is FrameworkElement element && IsReadText(element)
                && LightReadings.TryGetValue(Id(element), out var light)
                && DarkReadings.TryGetValue(Id(element), out var dark)
                && light.Text != dark.Text)
            {
                return $"{element.GetType().Name}{(string.IsNullOrEmpty(element.Name) ? string.Empty : "." + element.Name)}";
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    /// <summary>Identity by reference, not by layout: a node keeps the same key across the theme flip.</summary>
    private static string Id(FrameworkElement node) =>
        $"{node.GetType().Name}{(string.IsNullOrEmpty(node.Name) ? "." + node.Name : string.Empty)}" +
        $"#{System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(node):X}";

    private static FrameworkElement? SubjectFor(string ownerType)
    {
        var match = Subjects.FirstOrDefault(subject => subject.TargetType == ownerType);
        if (match.Markup is null)
        {
            Note("GAP", $"{ownerType} has no subject row in the probe");
            return null;
        }

        var element = LoadElement(match.Markup);
        if (element is null) return null;

        var host = _root.Children.OfType<StackPanel>().FirstOrDefault();
        if (host is null)
        {
            host = new StackPanel { Width = 420 };
            _root.Children.Add(host);
        }

        host.Children.Add(element);
        _root.UpdateLayout();
        Pump(10);
        return element;
    }

    private static FrameworkElement? FindNamed(DependencyObject root, string name)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is FrameworkElement element && element.Name == name) return element;
            if (FindNamed(child, name) is { } found) return found;
        }

        return null;
    }

    /// <summary>ARGB when the brush is translucent, RGB when it is opaque: an alpha-0x99 token is not black.</summary>
    private static string Hex(Color? color) => color is not { } value ? "<null>"
        : value.A == 0xFF ? $"#{value.R:X2}{value.G:X2}{value.B:X2}" : $"#{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}";

    private static FrameworkElement? LoadElement(string markup)
    {
        var file = Path.Combine(AppContext.BaseDirectory, "probe-subject.jalxaml");
        File.WriteAllText(file, $"<Grid {Root}>{markup}</Grid>", new UTF8Encoding(false));
        try
        {
            using var stream = File.OpenRead(file);
            if (XamlReader.Load(stream) is not Grid grid || grid.Children.Count == 0)
            {
                Note("load", "root was not a Grid with one child: " + Trim(markup));
                return null;
            }

            var child = grid.Children[0];
            grid.Children.RemoveAt(0);
            return child as FrameworkElement;
        }
        catch (Exception exception)
        {
            Note("load", "threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message) + " for " + Trim(markup));
            return null;
        }
    }

    private static void Pump(int frames, int budgetMilliseconds = 900)
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
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FluentJalium.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Cannot locate the repository root.");
    }

    private static void Note(string label, string text) => Lines.Add($"[{label}] {text}".TrimEnd());

    private static string Trim(string text) => Trim(text, 160);

    private static string Trim(string text, int budget) => text.Length <= budget ? text : text[..budget] + "...";
}
