using System.Reflection;
using System.Text;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace AutoCompleteProbe;

/// <summary>
/// Throwaway probe for the AutoSuggestBox batch. Answers four questions before any markup is
/// written: does AutoCompleteBox carry a framework-built template (NumberBox proved that
/// Style == null does not mean Template == null), what is its real property surface, what does a
/// mounted instance build and paint under our theme, and does filtering/dropdown actually work.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "autocomplete-probe.txt");

    private static readonly string[] LocalCandidates =
    [
        "Background", "Foreground", "BorderBrush", "BorderThickness", "CornerRadius", "Padding",
        "Margin", "MinHeight", "Height", "Width", "FontSize", "Visibility", "HorizontalContentAlignment",
    ];

    [STAThread]
    private static int Main()
    {
        try
        {
            Surface();
        }
        catch (Exception exception)
        {
            Note("SURFACE threw " + exception.GetType().Name + ": " + Trim(exception.Message));
        }

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

        File.WriteAllText(LogPath, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, full log: {LogPath}");
        return 0;
    }

    private static void Note(string line) => Lines.Add(line);

    private static string Trim(string text) =>
        text.Replace('\r', ' ').Replace('\n', ' ').Trim();

    // ---------- offline surface ----------

    private static void Surface()
    {
        var type = typeof(AutoCompleteBox);
        Note("=== A. type chain ===");
        for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
        {
            Note("  " + current.FullName);
        }

        Note("=== B. dependency properties (public static DP fields) ===");
        var dps = type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(DependencyProperty))
            .Select(field => (field.Name, dp: (DependencyProperty)field.GetValue(null)!))
            .OrderBy(item => item.Name, StringComparer.Ordinal)
            .ToList();
        foreach (var (fieldName, dp) in dps)
        {
            var extra = dp.PropertyType.IsEnum ? " enum{" + string.Join(",", Enum.GetNames(dp.PropertyType)) + "}" : string.Empty;
            Note($"  {fieldName} -> Name={dp.Name} Type={dp.PropertyType.Name} Owner={dp.OwnerType?.Name}{extra}");
        }

        Note($"  count={dps.Count}");

        Note("=== C. public CLR properties (declared only) ===");
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                     .OrderBy(property => property.Name, StringComparer.Ordinal))
        {
            Note($"  {property.PropertyType.Name} {property.Name} {(property.CanRead ? "get" : "")}{(property.CanWrite ? ";set" : "")}");
        }

        Note("=== D. public events (declared only) ===");
        foreach (var eventInfo in type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                     .OrderBy(eventInfo => eventInfo.Name, StringComparer.Ordinal))
        {
            Note($"  {eventInfo.EventHandlerType?.Name} {eventInfo.Name}");
        }

        Note("=== E. related public types in the assembly ===");
        foreach (var candidate in type.Assembly.GetExportedTypes()
                     .Where(candidate => candidate.Name.Contains("AutoComplete", StringComparison.Ordinal)
                                         || candidate.Name.Contains("AutoSuggest", StringComparison.Ordinal)
                                         || candidate.Name.Contains("TextSearch", StringComparison.Ordinal))
                     .OrderBy(candidate => candidate.Name, StringComparer.Ordinal))
        {
            Note($"  {candidate.FullName} {(candidate.IsEnum ? "enum{" + string.Join(",", Enum.GetNames(candidate)) + "}" : "type")}");
        }

        Note("=== F. unmounted instance ===");
        var box = new AutoCompleteBox();
        Note($"  Style == null: {box.Style is null}");
        Note($"  Template == null: {box.Template is null}");
        Note($"  local Template: {box.ReadLocalValue(Control.TemplateProperty) != DependencyProperty.UnsetValue}");
        if (box.Template is not null)
        {
            Note($"  Template type: {box.Template.GetType().FullName}");
            var xaml = ReadVisualTreeXaml(box.Template);
            Note($"  VisualTreeXaml length: {(xaml?.Length ?? -1)}");
            if (!string.IsNullOrEmpty(xaml))
            {
                Note("  --- framework template markup ---");
                foreach (var line in xaml!.Split('\n'))
                {
                    Note("  | " + Trim(line));
                }

                var keys = System.Text.RegularExpressions.Regex.Matches(xaml, @"\{ThemeResource\s+([^}\s]+)\}")
                    .Select(match => match.Groups[1].Value)
                    .ToList();
                Note("  --- {ThemeResource} names consumed (" + keys.Count + ", " + keys.Distinct().Count() + " distinct) ---");
                foreach (var group in keys.GroupBy(key => key, StringComparer.Ordinal).OrderBy(group => group.Key, StringComparer.Ordinal))
                {
                    Note($"    {group.Key} x{group.Count()}");
                }
            }
        }
    }

    private static string? ReadVisualTreeXaml(object template)
    {
        var property = template.GetType().GetProperty("VisualTreeXaml", BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(template) as string;
    }

    // ---------- mounted behaviour ----------

    private static void Run(Application application)
    {
        var box = new AutoCompleteBox { Width = 320, Margin = new Thickness(0, 0, 0, 12) };
        var probe = new AutoCompleteBox { Width = 320, Margin = new Thickness(0, 0, 0, 12) };
        var root = new StackPanel { Margin = new Thickness(24) };
        root.Children.Add(box);
        root.Children.Add(probe);

        var window = new Window { Content = root, Width = 900, Height = 700, Title = "AutoComplete probe" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Note("=== G. mounted (framework template, our theme applied) ===");
                BuildPalette();
                box.PlaceholderText = "framework placeholder";
                Pump();
                Note($"  mounted Style == null: {box.Style is null} | Template == null: {box.Template is null}");
                Note($"  IsKeyboardFocusable={box.Focusable} MinHeight={box.MinHeight} Padding={box.Padding} FontSize={box.FontSize} MinPrefix={box.MinimumPrefixLength} PopulateDelay={box.MinimumPopulateDelay} MaxDropDownHeight={box.MaxDropDownHeight} TextCompletion={box.IsTextCompletionEnabled}");
                Dump("  ", box);

                Note("=== H. control-level local values after mounting ===");
                foreach (var name in LocalCandidates)
                {
                    var dp = DependencyProperty.FromName(typeof(AutoCompleteBox), name);
                    if (dp is null)
                    {
                        continue;
                    }

                    var value = box.ReadLocalValue(dp);
                    if (value != DependencyProperty.UnsetValue)
                    {
                        Note($"  local {name} = {Format(value)}");
                    }
                }

                Note("=== I. items + filtering ===");
                box.ItemsSource = new[] { "Apple", "Banana", "Cherry", "Blueberry", "apricot" };
                Note($"  FilterMode default = {box.FilterMode}");
                box.Text = "ba";
                Pump();
                Note($"  after Text=\"ba\": IsDropDownOpen={box.IsDropDownOpen} Items.Count={CountItems(box)}");
                box.IsDropDownOpen = true;
                Pump();
                Note($"  forced open: IsDropDownOpen={box.IsDropDownOpen}");
                Note("  --- window tree with the dropdown open ---");
                Dump("  ", window, 6);

                Note("=== J. focus ===");
                Note($"  Focus() -> {box.Focus()} IsKeyboardFocused={box.IsKeyboardFocused}");
                Pump();
                Dump("  ", box);

                Note("=== K. selection ===");
                var filtered = box.FilteredItems;
                Note($"  FilteredItems.Count={filtered.Count} -> {string.Join(", ", filtered.Select(item => Format(item)))}");
                if (filtered.Count > 0)
                {
                    box.SelectedItem = filtered[0];
                    Pump();
                    Note($"  after SelectedItem=filtered[0]: Text=\"{box.Text}\"");
                }
                else
                {
                    Note("  nothing filtered");
                }

                Note("=== L. disabled + resting second instance ===");
                probe.IsEnabled = false;
                Pump();
                Dump("  ", probe);
                probe.IsEnabled = true;
                Pump();
                Dump("  ", probe);

                Note("=== M. our own template displaces it? ===");
                var ours = new AutoCompleteBox
                {
                    Width = 320,
                    Margin = new Thickness(0, 0, 0, 12),
                    PlaceholderText = "Pick a fruit",
                    Style = OurStyle(application),
                };
                if (ours.Style is null)
                {
                    Note("  our dictionary did not produce a style; skipped");
                }
                else
                {
                    root.Children.Add(ours);
                    Pump();
                    Note($"  template ours = {ours.Template is not null} | local Template = {ours.ReadLocalValue(Control.TemplateProperty) != DependencyProperty.UnsetValue}");
                    Dump("  ", ours);
                    ours.ItemsSource = new[] { "Apple", "Banana", "Cherry", "Blueberry", "apricot" };
                    ours.Text = "b";
                    Pump();
                    Note($"  after Text=\"b\": IsDropDownOpen={ours.IsDropDownOpen} FilteredItems=[{string.Join(", ", ours.FilteredItems.Select(Format))}]");
                    ours.IsDropDownOpen = true;
                    Pump(10);
                    Note($"  after forcing open + 10 frames: IsDropDownOpen={ours.IsDropDownOpen}");
                    Note("  --- window tree with our template + open dropdown ---");
                    Dump("  ", window, 6);
                    Note($"  Focus() -> {ours.Focus()} IsKeyboardFocused={ours.IsKeyboardFocused}");
                    Pump();
                    Dump("  ", ours);
                    ours.IsEnabled = false;
                    Pump();
                    Dump("  ", ours);
                    ours.IsEnabled = true;

                    Note("=== N. does selecting write the text? (WinUI UpdateTextOnSelect) ===");
                    foreach (var completion in new[] { false, true })
                    {
                        ours.Focus();
                        ours.IsTextCompletionEnabled = completion;
                        ours.Text = "bl";
                        Pump();
                        var items = ours.FilteredItems;
                        Note($"  focused={ours.IsKeyboardFocused} IsTextCompletionEnabled={completion}: FilteredItems=[{string.Join(", ", items.Select(Format))}]");
                        if (items.Count > 0)
                        {
                            ours.SelectedItem = items[0];
                            Pump();
                            Note($"    after selecting {Format(items[0])}: Text=\"{ours.Text}\" IsDropDownOpen={ours.IsDropDownOpen}");
                        }
                    }

                    Note("=== O. the same on the framework template ===");
                    box.IsTextCompletionEnabled = true;
                    box.Text = "ch";
                    Pump();
                    Note($"  FilteredItems=[{string.Join(", ", box.FilteredItems.Select(Format))}]");
                    if (box.FilteredItems.Count > 0)
                    {
                        box.SelectedItem = box.FilteredItems[0];
                        Pump();
                        Note($"  after selecting: Text=\"{box.Text}\"");
                    }

                    Note("=== P. our popup parts: did the framework fill them? ===");
                    Dump("  ", ours);
                    ours.IsDropDownOpen = true;
                    Pump(12);
                    Dump("  ", ours);

                    Note("=== Q. popup items host as an ItemsControl instead of a Panel ===");
                    var itemsHost = new AutoCompleteBox
                    {
                        Width = 320,
                        Style = OurStyle(application, "ProbeStyleItemsControl"),
                    };
                    root.Children.Add(itemsHost);
                    Pump();
                    itemsHost.ItemsSource = new[] { "Apple", "Banana", "Cherry" };
                    itemsHost.Text = "a";
                    itemsHost.IsDropDownOpen = true;
                    Pump(12);
                    Note($"  IsDropDownOpen={itemsHost.IsDropDownOpen} FilteredItems=[{string.Join(", ", itemsHost.FilteredItems.Select(Format))}]");
                    Dump("  ", itemsHost);
                    Note("  --- overlay after the ItemsControl variant ---");
                    Dump("  ", window, 6);
                }
            }
            catch (Exception exception)
            {
                Note("MEASURE threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            }

            window.Close();
        };
        application.Run(window);
    }

    private static int CountItems(AutoCompleteBox box)
    {
        var itemsProperty = box.GetType().GetProperty("Items");
        if (itemsProperty?.GetValue(box) is System.Collections.IEnumerable enumerable)
        {
            return enumerable.Cast<object?>().Count();
        }

        return -1;
    }

    private const string ProbeMarkup = """
        <ResourceDictionary xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
          <Style x:Key="ProbeStyle" TargetType="AutoCompleteBox">
            <Setter Property="Background" Value="{ThemeResource TextControlBackground}" />
            <Setter Property="Foreground" Value="{ThemeResource TextControlForeground}" />
            <Setter Property="BorderBrush" Value="{ThemeResource TextControlBorderBrush}" />
            <Setter Property="BorderThickness" Value="{ThemeResource TextControlBorderThemeThickness}" />
            <Setter Property="CornerRadius" Value="{ThemeResource ControlCornerRadius}" />
            <Setter Property="Padding" Value="{ThemeResource TextControlThemePadding}" />
            <Setter Property="MinHeight" Value="32" />
            <Setter Property="Template">
              <ControlTemplate TargetType="AutoCompleteBox">
                <Grid>
                  <Border Name="OuterBorder" Background="{TemplateBinding Background}"
                          BorderBrush="{TemplateBinding BorderBrush}"
                          BorderThickness="{TemplateBinding BorderThickness}"
                          CornerRadius="{TemplateBinding CornerRadius}">
                    <Grid>
                      <Grid Name="PART_ContentHost" Margin="{TemplateBinding Padding}" />
                      <TextBlock Name="ProbePlaceholder" Text="{TemplateBinding PlaceholderText}"
                                 Margin="{TemplateBinding Padding}" VerticalAlignment="Center"
                                 IsHitTestVisible="False" Visibility="Collapsed"
                                 Foreground="{ThemeResource TextControlForegroundDisabled}" />
                    </Grid>
                  </Border>
                  <Popup Name="PART_Popup" AllowsTransparency="True" Placement="Bottom">
                    <Grid Name="PART_DropDownBorder" Background="{ThemeResource FlyoutPresenterBackground}">
                      <ScrollViewer Name="PART_DropDownScrollViewer">
                        <StackPanel Name="PART_DropDownItemsHost" />
                      </ScrollViewer>
                    </Grid>
                  </Popup>
                </Grid>
                <ControlTemplate.Triggers>
                  <Trigger Property="Text" Value="">
                    <Setter TargetName="ProbePlaceholder" Property="Visibility" Value="Visible" />
                  </Trigger>
                  <Trigger Property="IsMouseOver" Value="True">
                    <Setter TargetName="OuterBorder" Property="Background" Value="{ThemeResource TextControlBackgroundPointerOver}" />
                    <Setter TargetName="OuterBorder" Property="BorderBrush" Value="{ThemeResource TextControlBorderBrushPointerOver}" />
                  </Trigger>
                  <Trigger Property="IsKeyboardFocusWithin" Value="True">
                    <Setter TargetName="OuterBorder" Property="Background" Value="{ThemeResource TextControlBackgroundFocused}" />
                    <Setter TargetName="OuterBorder" Property="BorderThickness" Value="{ThemeResource TextControlBorderThemeThicknessFocused}" />
                  </Trigger>
                  <Trigger Property="IsEnabled" Value="False">
                    <Setter TargetName="OuterBorder" Property="Background" Value="{ThemeResource TextControlBackgroundDisabled}" />
                    <Setter TargetName="OuterBorder" Property="BorderBrush" Value="{ThemeResource TextControlBorderBrushDisabled}" />
                    <Setter Property="Foreground" Value="{ThemeResource TextControlForegroundDisabled}" />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>
          <Style x:Key="ProbeStyleItemsControl" TargetType="AutoCompleteBox">
            <Setter Property="Template">
              <ControlTemplate TargetType="AutoCompleteBox">
                <Grid>
                  <Border Name="OuterBorder" Background="{ThemeResource TextControlBackground}" />
                  <Grid Name="PART_ContentHost" />
                  <Popup Name="PART_Popup" AllowsTransparency="True" Placement="Bottom">
                    <Grid Name="PART_DropDownBorder">
                      <ItemsControl Name="PART_DropDownItemsHost" />
                    </Grid>
                  </Popup>
                </Grid>
              </ControlTemplate>
            </Setter>
          </Style>
        </ResourceDictionary>
        """;

    private static Style? OurStyle(Application application, string key = "ProbeStyle")
    {
        try
        {
            var dictionary = (ResourceDictionary)Jalium.UI.Markup.XamlReader.Parse(ProbeMarkup);
            return dictionary[key] as Style;
        }
        catch (Exception exception)
        {
            Note("  our markup threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            return null;
        }
    }

    private static void Pump(int frames = 3)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var seen = 0;
        void OnRendering(object? sender, EventArgs arguments)
        {
            if (++seen >= frames)
            {
                dispatcher.InvokeAsync(() => frame.Continue = false);
            }
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        using var watchdog = new System.Threading.Timer(
            _ => dispatcher.InvokeAsync(() => frame.Continue = false), null, 4000, Timeout.Infinite);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
    }

    private static void Dump(string prefix, DependencyObject root, int maxDepth = 8)
    {
        Walk(prefix, root, 0, maxDepth);
    }

    private static void Walk(string prefix, DependencyObject node, int depth, int maxDepth)
    {
        if (depth > maxDepth)
        {
            return;
        }

        var visual = node as Visual;
        if (visual is null)
        {
            Note(prefix + new string(' ', depth * 2) + "(non-visual) " + node.GetType().Name);
            return;
        }

        var indent = prefix + new string(' ', depth * 2);
        var name = (node as FrameworkElement)?.Name;
        var builder = new StringBuilder($"{indent}{node.GetType().Name}");
        if (!string.IsNullOrEmpty(name))
        {
            builder.Append($" '{name}'");
        }

        if (node is Control control)
        {
            builder.Append($" bg={Mark(control.Background)} fg={Mark(control.Foreground)} bb={Mark(control.BorderBrush)}");
        }
        else if (node is Border border)
        {
            builder.Append($" bg={Mark(border.Background)} bb={Mark(border.BorderBrush)}");
        }
        else if (node is TextBlock textBlock)
        {
            builder.Append($" fg={Mark(textBlock.Foreground)} text=\"{Trim(textBlock.Text ?? string.Empty)}\"");
        }
        var typeName = node.GetType().Name;
        if (typeName is "Rectangle" or "Ellipse" or "Path" or "Line" or "Polygon")
        {
            builder.Append($" fill={Mark(Pick<Brush>(node, "Fill"))} stroke={Mark(Pick<Brush>(node, "Stroke"))}");
        }

        if (typeName == "Popup")
        {
            var child = Pick(node, "Child") as DependencyObject;
            builder.Append($" | popup IsOpen={Pick(node, "IsOpen")} placement={Pick(node, "Placement")} child={(child?.GetType().Name ?? "null")}");
        }

        var element = node;
        var locals = new List<string>();
        foreach (var candidate in LocalCandidates)
        {
            var dp = DependencyProperty.FromName(element.GetType(), candidate);
            if (dp is null)
            {
                continue;
            }

            var value = element.ReadLocalValue(dp);
            if (value != DependencyProperty.UnsetValue)
            {
                locals.Add($"{candidate}={Format(value)}");
            }
        }

        if (locals.Count > 0)
        {
            builder.Append(" | locals=" + string.Join(" ", locals));
        }

        if (node is UIElement uiElement && !uiElement.IsVisible)
        {
            builder.Append(" [hidden]");
        }

        Note(builder.ToString());
        var children = VisualTreeHelper.GetChildrenCount(visual);
        for (var index = 0; index < children; index++)
        {
            Walk(prefix, VisualTreeHelper.GetChild(visual, index), depth + 1, maxDepth);
        }
    }

    private static string Brush(Brush? brush) =>
        brush is SolidColorBrush solid ? $"#{solid.Color.A:X2}{solid.Color.R:X2}{solid.Color.G:X2}{solid.Color.B:X2}"
        : brush is null ? "-"
        : brush.GetType().Name;

    private static (string Key, Brush Brush)[] Palette = [];

    private static void BuildPalette()
    {
        string[] keys =
        [
            "ControlFillColorDefaultBrush", "ControlFillColorSecondaryBrush", "ControlFillColorTertiaryBrush",
            "ControlFillColorInputActiveBrush", "ControlFillColorDisabledBrush", "ControlFillColorTransparentBrush",
            "ControlStrokeColorDefaultBrush", "ControlStrongStrokeColorDefaultBrush",
            "TextFillColorPrimaryBrush", "TextFillColorSecondaryBrush", "TextFillColorDisabledBrush",
            "AccentFillColorDefaultBrush", "SolidBackgroundFillColorQuarternaryBrush",
            "FlyoutPresenterBackground", "SurfaceStrokeColorFlyoutBrush", "ListViewBackgroundBrush",
            "ComboBoxItemBackgroundSelectedBrush", "ListCardBackgroundBrush",
        ];
        var found = new List<(string, Brush)>();
        foreach (var key in keys)
        {
            try
            {
                var brush = FluentThemeManager.GetBrush(key);
                if (brush is not null)
                {
                    found.Add((key, brush));
                }
            }
            catch (Exception exception)
            {
                found.Add((key + " (threw " + exception.GetType().Name + ")", null!));
            }
        }

        Palette = [.. found.Where(item => item.Item2 is not null)];
        Note("  palette lookups: " + string.Join(" ", found
            .Where(item => item.Item1.Contains("threw", StringComparison.Ordinal))
            .Select(item => item.Item1)));
    }

    private static T? Pick<T>(DependencyObject node, string propertyName) where T : class
    {
        var property = node.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return property is not null && property.PropertyType == typeof(T) ? property.GetValue(node) as T : null;
    }

    private static object? Pick(DependencyObject node, string propertyName)
    {
        var property = node.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(node);
    }

    /// <summary>Names the palette instance when the brush is ours, otherwise reports its colour.</summary>
    private static string Mark(Brush? brush)
    {
        foreach (var (key, ours) in Palette)
        {
            if (ReferenceEquals(brush, ours))
            {
                return "OURS:" + key;
            }
        }

        return Brush(brush);
    }

    private static string Format(object? value) =>
        value switch
        {
            null => "null",
            SolidColorBrush solid => $"#{solid.Color.A:X2}{solid.Color.R:X2}{solid.Color.G:X2}{solid.Color.B:X2}",
            Color color => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}",
            string text => "\"" + Trim(text) + '"',
            _ => Trim(value.ToString() ?? string.Empty),
        };
}
