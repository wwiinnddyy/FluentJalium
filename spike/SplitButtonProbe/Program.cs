using System.Reflection;
using System.Text;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Automation.Peers;
using Jalium.UI.Automation.Provider;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace SplitButtonProbe;

/// <summary>
/// Throwaway probe for the stage-2 remainder. Answers four questions before any markup is written:
/// which button-family and surface types 26.10.9 actually ships (the objective assumes DropDownButton
/// needs an own type and SplitButton does not), what SplitButton's real surface is, what its
/// framework-built template looks like and which palette names it consumes, and what a mounted
/// instance builds under our theme - including whether a flyout can be opened at all.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "splitbutton-probe.txt");

    private static readonly string[] LocalCandidates =
    [
        "Background", "Foreground", "BorderBrush", "BorderThickness", "CornerRadius", "Padding",
        "Margin", "MinHeight", "MinWidth", "Height", "Width", "FontSize", "Visibility",
        "HorizontalContentAlignment", "VerticalContentAlignment",
    ];

    /// <summary>Names WinUI uses for the stage-2 / stage-4 / stage-5 surfaces we may have to substitute.</summary>
    private static readonly string[] WantedNames =
    [
        "SplitButton", "ToggleSplitButton", "DropDownButton", "Button", "RepeatButton", "HyperlinkButton",
        "Menu", "MenuBar", "MenuFlyout", "MenuFlyoutItem", "ToggleMenuFlyoutItem", "RadioMenuFlyoutItem",
        "Flyout", "FlyoutBase", "Popup", "Expander", "InfoBar", "CommandBar", "CommandBarFlyout",
        "AppBarButton", "AppBarToggleButton", "ContentDialog", "TeachingTip", "TabView", "TabControl",
        "ProgressRing", "ProgressBar", "InfoBadge", "RatingControl", "Divider", "PipsPager",
        "BreadcrumbBar", "RadioButtons", "Card", "ColorPicker", "NumberBox", "AutoCompleteBox",
        "TreeView", "DataGrid", "TreeDataGrid", "ListView", "GridView", "ListBox", "NavigationView",
        "ItemsRepeater", "ScrollViewer", "ToolTip", "Thumb", "ScrollBar", "ContentPresenter",
    ];

    private static readonly string[] Keywords =
    [
        "Split", "DropDown", "Dropdown", "Menu", "Flyout", "Expander", "InfoBar", "Command", "AppBar",
        "Dialog", "Tip", "Tab", "Ring", "Badge", "Rating", "Divider", "Pips", "Breadcrumb", "Radio",
        "Card", "Bar", "Popup",
    ];

    [STAThread]
    private static int Main()
    {
        try
        {
            Census();
            Surface(typeof(SplitButton));
            FlyoutSignatures();
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

    private static string Trim(string text) => text.Replace('\r', ' ').Replace('\n', ' ').Trim();

    // ---------- A. what does the runtime ship ----------

    private static void Census()
    {
        Note("=== A. exported Jalium types matching the stage-2/4/5 names ===");
        var exported = new Dictionary<string, Type>(StringComparer.Ordinal);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()
                     .Where(assembly => assembly.GetName().Name?.StartsWith("Jalium.UI", StringComparison.Ordinal) == true))
        {
            Type[] types;
            try
            {
                types = assembly.GetExportedTypes();
            }
            catch (Exception exception)
            {
                Note($"  {assembly.GetName().Name}: GetExportedTypes threw {exception.GetType().Name}");
                continue;
            }

            foreach (var type in types)
            {
                exported[type.Name] = type;
            }
        }

        Note($"  assemblies scanned, distinct exported type names={exported.Count}");

        foreach (var name in WantedNames.OrderBy(name => name, StringComparer.Ordinal))
        {
            if (exported.TryGetValue(name, out var type))
            {
                var chain = new List<string>();
                for (var current = type.BaseType; current is not null && current != typeof(object); current = current.BaseType)
                {
                    chain.Add(current.Name);
                }

                Note($"  YES {type.FullName}  : {string.Join(" < ", chain)}");
            }
            else
            {
                Note($"  no  {name}");
            }
        }

        Note("=== A2. any exported Control whose name carries a keyword ===");
        var controlBase = typeof(Control);
        foreach (var type in exported.Values
                     .Where(type => type.IsClass && !type.IsAbstract && controlBase.IsAssignableFrom(type))
                     .Where(type => Keywords.Any(keyword => type.Name.Contains(keyword, StringComparison.Ordinal)))
                     .OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            Note($"  {type.FullName}");
        }
    }

    // ---------- B. one type in depth ----------

    private static void Surface(Type type)
    {
        Note($"=== B. {type.Name}: type chain ===");
        for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
        {
            Note("  " + current.FullName);
        }

        Note($"=== B2. {type.Name}: dependency properties (public static DP fields, inherited included) ===");
        var dps = new List<(string Owner, string Field, DependencyProperty Dp)>();
        for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
        {
            foreach (var field in current.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(field => field.FieldType == typeof(DependencyProperty)))
            {
                dps.Add((current.Name, field.Name, (DependencyProperty)field.GetValue(null)!));
            }
        }

        foreach (var (owner, fieldName, dp) in dps.OrderBy(item => item.Field, StringComparer.Ordinal))
        {
            var extra = dp.PropertyType.IsEnum ? " enum{" + string.Join(",", Enum.GetNames(dp.PropertyType)) + "}" : string.Empty;
            Note($"  {fieldName} -> Name={dp.Name} Type={dp.PropertyType.Name} Owner={owner}{extra}");
        }

        Note($"  count={dps.Count}");

        Note($"=== B3. {type.Name}: public CLR properties (declared only) ===");
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                     .OrderBy(property => property.Name, StringComparer.Ordinal))
        {
            Note($"  {property.PropertyType.Name} {property.Name} {(property.CanRead ? "get" : "")}{(property.CanWrite ? ";set" : "")}");
        }

        Note($"=== B4. {type.Name}: public events (declared only) ===");
        foreach (var eventInfo in type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                     .OrderBy(eventInfo => eventInfo.Name, StringComparer.Ordinal))
        {
            Note($"  {eventInfo.EventHandlerType?.Name} {eventInfo.Name}");
        }

        Note($"=== B5. {type.Name}: public methods (declared only) ===");
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                     .Where(method => !method.IsSpecialName)
                     .OrderBy(method => method.Name, StringComparer.Ordinal))
        {
            Note($"  {method.ReturnType.Name} {method.Name}({string.Join(",", method.GetParameters().Select(parameter => parameter.ParameterType.Name))})");
        }

        Note($"=== B6. unmounted {type.Name} ===");
        var instance = (Control)Activator.CreateInstance(type)!;
        Note($"  Style == null: {instance.Style is null} | Template == null: {instance.Template is null}");
        Note($"  local Template: {instance.ReadLocalValue(Control.TemplateProperty) != DependencyProperty.UnsetValue}");
        Note($"  MinHeight={instance.MinHeight} Padding={instance.Padding} FontSize={instance.FontSize} BorderThickness={instance.BorderThickness}");
        if (instance.Template is not null)
        {
            DumpTemplate(instance.Template);
        }
    }

    private static void DumpTemplate(ControlTemplate template)
    {
        Note($"  Template type: {template.GetType().FullName}");
        var xaml = ReadVisualTreeXaml(template);
        Note($"  VisualTreeXaml length: {(xaml?.Length ?? -1)}");
        if (string.IsNullOrEmpty(xaml))
        {
            return;
        }

        Note("  --- framework template markup ---");
        foreach (var line in xaml!.Split('\n'))
        {
            Note("  | " + Trim(line));
        }

        var keys = System.Text.RegularExpressions.Regex.Matches(xaml, @"\{ThemeResource\s+([^}\s]+)\}")
            .Select(match => match.Groups[1].Value).ToList();
        Note($"  --- {{ThemeResource}} names consumed ({keys.Count}, {keys.Distinct().Count()} distinct) ---");
        foreach (var group in keys.GroupBy(key => key, StringComparer.Ordinal).OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            Note($"    {group.Key} x{group.Count()}");
        }

        var names = System.Text.RegularExpressions.Regex.Matches(xaml, "\\{x:Name=(?:\"|\')([^\"\'=]+)")
            .Select(match => match.Groups[1].Value).ToList();
        Note("  --- named parts: " + string.Join(" ", names.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)));
    }

    private static string? ReadVisualTreeXaml(object template)
    {
        var property = template.GetType().GetProperty("VisualTreeXaml", BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(template) as string;
    }

    // ---------- C. mounted ----------

    private static void Run(Application application)
    {
        var plain = new SplitButton { Content = "plain", Width = 260, Margin = new Thickness(0, 0, 0, 12) };
        var withFlyout = new SplitButton { Content = "flyout", Width = 260, Margin = new Thickness(0, 0, 0, 12) };
        var root = new StackPanel { Margin = new Thickness(24) };
        root.Children.Add(plain);
        root.Children.Add(withFlyout);

        var window = new Window { Content = root, Width = 900, Height = 700, Title = "SplitButton probe" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Note("=== C. mounted (framework template, our theme applied) ===");
                BuildPalette();
                Pump();
                Note($"  plain: Style==null {plain.Style is null} | Template==null {plain.Template is null} | local Template {plain.ReadLocalValue(Control.TemplateProperty) != DependencyProperty.UnsetValue}");
                Note($"  MinHeight={plain.MinHeight} MinWidth={plain.MinWidth} Padding={plain.Padding} FontSize={plain.FontSize} BorderThickness={plain.BorderThickness} CornerRadius={plain.CornerRadius} Focusable={plain.Focusable}");
                Dump("  ", plain, 10);

                Note("=== D. control-level local values after mounting ===");
                foreach (var name in LocalCandidates)
                {
                    var dp = DependencyProperty.FromName(typeof(SplitButton), name);
                    if (dp is null)
                    {
                        continue;
                    }

                    var value = plain.ReadLocalValue(dp);
                    if (value != DependencyProperty.UnsetValue)
                    {
                        Note($"  local {name} = {Format(value)}");
                    }
                }

                Note("=== E. our own style on SplitButton (does the button layout style fit?) ===");
                var ours = new SplitButton
                {
                    Content = "ours",
                    Width = 260,
                    Style = FluentThemeManager.GetStyle("DefaultButtonStyle"),
                };
                root.Children.Add(ours);
                window.UpdateLayout();
                Pump();
                Note($"  ours: bg={Mark(ours.Background)} fg={Mark(ours.Foreground)} bb={Mark(ours.BorderBrush)} bt={ours.BorderThickness} cr={ours.CornerRadius}");
                Dump("  ", ours, 10);

                Note("=== F. flyout ===");
                var flyoutProperty = typeof(SplitButton).GetProperty("Flyout");
                Note($"  Flyout property type: {flyoutProperty?.PropertyType.FullName ?? "MISSING"}");
                var flyoutType = FindType("Jalium.UI.Controls.MenuFlyout");
                Note($"  MenuFlyout type: {flyoutType?.FullName ?? "MISSING"}");
                if (flyoutProperty is not null && flyoutType is not null)
                {
                    var flyout = Activator.CreateInstance(flyoutType);
                    var itemsProperty = flyoutType.GetProperty("Items");
                    Note($"  Items type: {itemsProperty?.PropertyType.Name ?? "MISSING"}");
                    if (itemsProperty?.GetValue(flyout) is System.Collections.IList items)
                    {
                        var itemType = FindType("Jalium.UI.Controls.MenuFlyoutItem");
                        Note($"  MenuFlyoutItem type: {itemType?.FullName ?? "MISSING"}");
                        if (itemType is not null)
                        {
                            var first = Activator.CreateInstance(itemType)!;
                            itemType.GetProperty("Text")?.SetValue(first, "first item");
                            items.Add(first);
                            var second = Activator.CreateInstance(itemType)!;
                            itemType.GetProperty("Text")?.SetValue(second, "second item");
                            items.Add(second);
                        }
                    }

                    flyoutProperty.SetValue(withFlyout, flyout);
                    Pump();
                    Dump("  ", withFlyout, 10);

                    Note("  --- open levers on SplitButton ---");
                    foreach (var member in typeof(SplitButton).GetMembers(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(member => member.Name.Contains("Open", StringComparison.Ordinal)
                                                  || member.Name.Contains("Flyout", StringComparison.Ordinal)
                                                  || member.Name.Contains("Click", StringComparison.Ordinal))
                                 .OrderBy(member => member.Name, StringComparer.Ordinal))
                    {
                        Note($"    {member.MemberType} {member.Name}");
                    }

                    var open = typeof(SplitButton).GetProperty("IsDropDownOpen");
                    if (open is not null)
                    {
                        open.SetValue(withFlyout, true);
                        Pump();
                        Note($"  after IsDropDownOpen=true: {open.GetValue(withFlyout)}");
                        Note("  --- window tree with the flyout open ---");
                        Dump("  ", window, 8);
                    }
                    else
                    {
                        Note("  no IsDropDownOpen property on SplitButton");
                    }
                }

                    Note("=== K. implicit style lookup, per type ===");
                var probeButton = new Button { Content = "b", Width = 120 };
                var probeToggle = new ToggleButton { Content = "t", Width = 120 };
                var probeHyperlink = new HyperlinkButton { Content = "h", Width = 120 };
                var probeDropDown = new FluentJalium.Controls.FluentDropDownButton { Content = "d", Width = 120 };
                root.Children.Add(probeButton);
                root.Children.Add(probeToggle);
                root.Children.Add(probeHyperlink);
                root.Children.Add(probeDropDown);
                window.UpdateLayout();
                Pump();
                foreach (var probe in new FrameworkElement[] { probeButton, probeToggle, probeHyperlink, plain, probeDropDown })
                {
                    var byType = application.TryFindResource(probe.GetType());
                    Note($"  {probe.GetType().Name}: element.Style={(probe.Style is null ? "null" : "set")} " +
                         $"TryFindResource(Type)={(byType?.GetType().Name ?? "null")}");
                }

                var implicitKeys = new List<string>();
                void Collect(ResourceDictionary dictionary)
                {
                    foreach (var key in dictionary.Keys)
                    {
                        if (key is Type typed)
                        {
                            implicitKeys.Add(typed.Name);
                        }
                    }

                    foreach (var merged in dictionary.MergedDictionaries)
                    {
                        Collect(merged);
                    }
                }

                Collect(application.Resources);
                Note($"  type-keyed entries visible to the application: {string.Join(", ", implicitKeys.Order())}");

            Note("=== G. focus ===");
                Note($"  Focus() -> {plain.Focus()} IsKeyboardFocused={plain.IsKeyboardFocused}");
                Pump();
                Dump("  ", plain, 10);

                Note("=== H. FlyoutBase surface (what can a test observe or drive?) ===");
                var flyoutBaseType = FindType("Jalium.UI.Controls.Primitives.FlyoutBase");
                Note($"  {flyoutBaseType?.FullName ?? "MISSING"}");
                foreach (var member in (flyoutBaseType?.GetMembers(BindingFlags.Public | BindingFlags.Instance) ?? [])
                             .Where(static member => member is PropertyInfo or EventInfo or MethodInfo)
                             .OrderBy(static member => member.Name, StringComparer.Ordinal))
                {
                    if (member.Name.StartsWith("get_", StringComparison.Ordinal)
                        || member.Name.StartsWith("add_", StringComparison.Ordinal)
                        || member.Name.StartsWith("Remove", StringComparison.Ordinal)
                        || member.Name.StartsWith("remove_", StringComparison.Ordinal)
                        || member.Name.StartsWith("set_", StringComparison.Ordinal)
                        || member.Name.StartsWith("Add", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    Note($"    {member.MemberType} {member.Name}");
                }

                Note("=== I. part-name contract: does a template we wrote still open the flyout? ===");
                var probeDictionary = (ResourceDictionary)XamlReader.Parse(TemplateMarkup)!;
                var named = new SplitButton
                {
                    Content = "named",
                    Width = 260,
                    Template = (ControlTemplate)probeDictionary["ProbeSplitTemplate"]!,
                    Flyout = MakeFlyout(),
                };
                var renamed = new SplitButton
                {
                    Content = "renamed",
                    Width = 260,
                    Template = (ControlTemplate)probeDictionary["ProbeRenamedTemplate"]!,
                    Flyout = MakeFlyout(),
                };
                root.Children.Add(named);
                root.Children.Add(renamed);
                window.UpdateLayout();
                Pump();
                Dump("  ", named, 10);
                foreach (var (label, button) in new[] { ("named", named), ("renamed", renamed) })
                {
                    Note($"  --- {label}: Click subscription and the two halves ---");
                    var clicks = 0;
                    button.Click += (_, _) => clicks++;
                    var secondary = Descendant(button, "SecondaryButton") is { } sec
                        ? sec
                        : Descendant(button, "SecondaryButtonZ");
                    var primary = Descendant(button, "PrimaryButton") is { } pri ? pri : Descendant(button, "PrimaryButtonZ");
                    Note($"    parts found: primary={primary?.GetType().Name ?? "none"} ({primary?.Name}) secondary={secondary?.GetType().Name ?? "none"} ({secondary?.Name})");
                    if (secondary is Button secondaryButton)
                    {
                        Invoke(secondaryButton);
                        Pump(12);
                        Note($"    after invoking secondary: flyout.IsOpen={IsOpen(button)}");
                    }

                    if (primary is Button primaryButton)
                    {
                        Invoke(primaryButton);
                        Pump(12);
                        Note($"    after invoking primary: Click fired {clicks} time(s), flyout.IsOpen={IsOpen(button)}");
                    }

                    var peerFactory = typeof(SplitButton).GetMethod("CreateAutomationPeer", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    var peer = peerFactory?.Invoke(button, null) as AutomationPeer;
                    Note($"    control-level lever: CreateAutomationPeer -> {peer?.GetType().Name ?? "none"}");
                    if (peer is not null)
                    {
                        foreach (var pattern in new[] { PatternInterface.Invoke, PatternInterface.ExpandCollapse, PatternInterface.Toggle })
                        {
                            Note($"      {pattern}: {peer.GetPattern(pattern)?.GetType().Name ?? "null"}");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Note("MOUNT threw " + exception.GetType().Name + ": " + Trim(exception.ToString()));
            }
            finally
            {
                application.Shutdown();
            }
        };

        window.Show();
        application.Run(window);
    }

    /// <summary>
    /// Signatures for the own-type work: how a flyout is opened, closed and observed, and which of those are
    /// dependency properties a style can bind or a trigger can read.
    /// </summary>
    private static void FlyoutSignatures()
    {
        Note("=== J. FlyoutBase signatures ===");
        var type = typeof(FlyoutBase);
        foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                     .Where(member => member.Name is "ShowAt" or "Hide" or "IsOpen" or "Opened" or "Closed" or "Opening" or "Closing" or "Placement" or "ShowMode")
                     .OrderBy(member => member.Name, StringComparer.Ordinal))
        {
            switch (member)
            {
                case MethodInfo method:
                    Note($"  method {method.ReturnType.Name} {method.Name}({string.Join(", ", method.GetParameters().Select(parameter => $"{parameter.ParameterType.Name} {parameter.Name}"))})");
                    break;
                case EventInfo eventInfo:
                    Note($"  event {eventInfo.EventHandlerType?.Name} {eventInfo.Name}");
                    break;
                case PropertyInfo property:
                    Note($"  property {property.PropertyType.Name} {property.Name} {{ {(property.CanRead ? "get; " : "")}{(property.CanWrite ? "set; " : "")}}}".TrimEnd());
                    break;
            }
        }

        Note($"  IsOpen is a DP on {type.Name}: {DependencyProperty.FromName(type, "IsOpen") is not null}");
        Note($"  Placement is a DP: {DependencyProperty.FromName(type, "Placement") is not null}");
        Note($"  SplitButton.Flyout is a DP: {DependencyProperty.FromName(typeof(SplitButton), "Flyout") is not null}");
        Note($"  Button.OnClick: {typeof(Button).GetMethod("OnClick", BindingFlags.NonPublic | BindingFlags.Instance)?.ToString() ?? "none"}");
        Note($"  MenuFlyout base: {typeof(MenuFlyout).BaseType?.Name}");
    }

    private static MenuFlyout MakeFlyout()
    {
        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuFlyoutItem { Text = "one" });
        flyout.Items.Add(new MenuFlyoutItem { Text = "two" });
        return flyout;
    }

    private static FrameworkElement? Descendant(DependencyObject root, string name)
    {
        if (root is FrameworkElement element && element.Name == name)
        {
            return element;
        }

        var children = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < children; index++)
        {
            if (VisualTreeHelper.GetChild(root, index) is { } child && Descendant(child, name) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    private static void Invoke(Button button)
    {
        try
        {
            ((IInvokeProvider)new ButtonAutomationPeer(button)).Invoke();
        }
        catch (Exception exception)
        {
            Note($"    Invoke({button.Name}) threw {exception.GetType().Name}: {Trim(exception.Message)}");
        }
    }

    private static string IsOpen(SplitButton button)
    {
        if (button.Flyout is null)
        {
            return "no flyout";
        }

        var property = button.Flyout.GetType().GetProperty("IsOpen") ?? typeof(FlyoutBase).GetProperty("IsOpen");
        return property is null ? "no IsOpen property" : Convert.ToString(property.GetValue(button.Flyout)) ?? "null";
    }

    private const string TemplateMarkup = """
        <ResourceDictionary xmlns='http://schemas.jalium.ui/2024'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <ControlTemplate x:Key='ProbeSplitTemplate' TargetType='SplitButton'>
            <Grid>
              <Grid.ColumnDefinitions>
                <ColumnDefinition Width='*' />
                <ColumnDefinition Width='1' />
                <ColumnDefinition Width='35' />
              </Grid.ColumnDefinitions>
              <Button x:Name='PrimaryButton' Grid.Column='0' Content='{TemplateBinding Content}' />
              <Border x:Name='Divider' Grid.Column='1' Background='Orange' />
              <Button x:Name='SecondaryButton' Grid.Column='2' Content='v' />
            </Grid>
          </ControlTemplate>
          <ControlTemplate x:Key='ProbeRenamedTemplate' TargetType='SplitButton'>
            <Grid>
              <Grid.ColumnDefinitions>
                <ColumnDefinition Width='*' />
                <ColumnDefinition Width='1' />
                <ColumnDefinition Width='35' />
              </Grid.ColumnDefinitions>
              <Button x:Name='PrimaryButtonZ' Grid.Column='0' Content='{TemplateBinding Content}' />
              <Border x:Name='DividerZ' Grid.Column='1' Background='Orange' />
              <Button x:Name='SecondaryButtonZ' Grid.Column='2' Content='v' />
            </Grid>
          </ControlTemplate>
        </ResourceDictionary>
        """;

    private static Type? FindType(string fullName) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(fullName))
            .FirstOrDefault(type => type is not null);

    // ---------- helpers ----------

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

        var locals = new List<string>();
        foreach (var candidate in LocalCandidates)
        {
            var dp = DependencyProperty.FromName(node.GetType(), candidate);
            if (dp is null)
            {
                continue;
            }

            var value = node.ReadLocalValue(dp);
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
            if (VisualTreeHelper.GetChild(visual, index) is { } child)
            {
                Walk(prefix, child, depth + 1, maxDepth);
            }
        }
    }

    private static void Dump(string prefix, DependencyObject root, int maxDepth = 8) => Walk(prefix, root, 0, maxDepth);

    private static object? Pick(DependencyObject node, string propertyName) =>
        node.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(node);

    private static T? Pick<T>(DependencyObject node, string propertyName) where T : class =>
        node.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance) is { } property
        && property.PropertyType == typeof(T)
            ? property.GetValue(node) as T
            : null;

    private static void BuildPalette()
    {
        string[] keys =
        [
            "ControlFillColorDefaultBrush", "ControlFillColorSecondaryBrush", "ControlFillColorTertiaryBrush",
            "ControlFillColorDisabledBrush", "ControlFillColorTransparentBrush", "ControlStrokeColorDefaultBrush",
            "TextFillColorPrimaryBrush", "TextFillColorSecondaryBrush", "AccentFillColorDefaultBrush",
            "AccentTextFillColorPrimaryBrush", "SubtleFillColorSecondaryBrush", "CardBackgroundFillColorDefaultBrush",
            "FlyoutPresenterBackground", "SurfaceStrokeColorFlyoutBrush",
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
                Note($"  palette {key} threw {exception.GetType().Name}");
            }
        }

        Palette = [.. found];
    }

    private static (string Key, Brush Brush)[] Palette = [];

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

    private static string Brush(Brush? brush) =>
        brush is SolidColorBrush solid ? $"#{solid.Color.A:X2}{solid.Color.R:X2}{solid.Color.G:X2}{solid.Color.B:X2}"
        : brush is null ? "-"
        : brush.GetType().Name;

    private static string Format(object? value) =>
        value switch
        {
            null => "null",
            SolidColorBrush solid => $"#{solid.Color.A:X2}{solid.Color.R:X2}{solid.Color.G:X2}{solid.Color.B:X2}",
            Color color => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}",
            string text => "\"" + Trim(text) + '"',
            _ => Trim(value.ToString() ?? string.Empty),
        };

    /// <summary>
    /// Pumps real rendered frames, releasing on its own dispatcher - the watchdog has to be handed the
    /// dispatcher of the thread that pushed the frame (adaptation/06).
    /// </summary>
    private static int Pump(int frames = 6, int budgetMilliseconds = 600)
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
        watchdog.Change(TimeSpan.FromMilliseconds(budgetMilliseconds * 2), System.Threading.Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
        return seen;
    }
}
