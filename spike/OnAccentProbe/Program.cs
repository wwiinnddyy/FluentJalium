using System.Reflection;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Markup;
using Jalium.UI.Threading;

namespace OnAccentProbe;

/// <summary>
/// Throwaway census: who reads the framework's own <c>TextOnAccent</c> name on the shipped NuGet 26.10.9 authority,
/// and on which surface does the read land? That is the open half of the A2 alias layer's next row
/// (ThemeResources/FrameworkRetints.jalxaml), and the layer's standing rule is that a row ships only for a reader this
/// library cannot reach any other way: a surface whose template we own reads Astra rows and needs no name alias, while
/// a self-drawn or untouched framework surface has the name as its only lever.
///
/// The reference tree (/c/git/Jalium/Jalium.UI) is NOT the authority here - it may be newer than 26.10.9 - so every
/// claim below is read off the loaded assemblies. The tree's shape is still what the instrument calibrates against,
/// which is why the same names are censused side by side: TextPrimary / TextSecondary / TextDisabled / ControlBorder /
/// SurfaceBackground / AccentBrush already have shipped rows and known readers, so a run that finds none of them is a
/// broken instrument rather than a null reading. Same for the byte-level sanity check in spike/OnAccentProbe/grep-name.py
/// (a first pass grepped Jalium.UI.Controls.dll, which is a 55 KB type-forwarding shell that contains none of these
/// literals; the implementation is the 8 MB Jalium.UI.Managed.dll).
///
///   il      - every ldstr operand in every declared member of every loaded Jalium.UI* type, filtered to the target
///             names, plus each hit method's full literal list for context, plus whether the name is ALSO reachable as
///             a static ThemeColors member (a direct field/property read is not something an alias row can redirect,
///             so a surface painted that way must be reported as out of reach rather than as support).
///   reader  - mount each candidate surface twice, once resting and once with a probe brush installed under the name,
///             and report (a) what the name resolves to at that element's own resource scope and (b) which built
///             elements end up painted with the probe INSTANCE. Instance identity is the arrival proof; colour
///             equality is not, because the framework's projection and our token are different objects that can
///             coincide in value.
///   resolver  - call the four code readers the il pass names, resting and with the probe installed, so each one's
///             per-call shape is read off the assembly instead of inferred from the name it carries.
///   calendar - mount Calendar / DatePicker / TimePicker twice (resting, then with the probe already installed) and
///             count what the visual walk sees, with a Button mounted the same way in the same run as the instrument
///             check. The first surface pass walked fourteen families in their resting state and read 0/14, which is
///             the reason this leg drives a state at all: a uniform zero says the walk never asked the name, so it
///             cannot be reported as "nothing reads this".
///   style    - is the calendar family simply unstyled in this host? prints whether the framework's own theme
///             dictionary is merged, and each family's implicit-by-type style with its setter list, which is where the
///             missing Control.Template cell shows up.
///
/// Diagnostic-only: raises no real input, opens its own windows, writes nothing into the user's profile.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = new();

    /// <summary>A colour no token in this library paints with, so seeing it can only mean the name was read.</summary>
    private static readonly Color Probe = Color.FromRgb(0x11, 0x22, 0x33);

    private static readonly string[] Targets =
    [
        "TextOnAccent",
        "TextPrimary",
        "TextSecondary",
        "TextDisabled",
        "ControlBorder",
        "SurfaceBackground",
        "AccentBrush",
        "ControlBorderFocused",
    ];

    /// <summary>Candidate surfaces: name, assembly-qualified type, and how to make the on-accent state appear.</summary>
    private static readonly (string Label, string Type, string State)[] Surfaces =
    [
        ("Calendar", "Jalium.UI.Controls.Calendar, Jalium.UI.Controls", "resting"),
        ("DatePicker", "Jalium.UI.Controls.DatePicker, Jalium.UI.Controls", "resting"),
        ("TimePicker", "Jalium.UI.Controls.TimePicker, Jalium.UI.Controls", "resting"),
        ("SwipeControl", "Jalium.UI.Controls.SwipeControl, Jalium.UI.Controls", "resting"),
        ("TitleBar", "Jalium.UI.Controls.TitleBar, Jalium.UI.Controls", "resting"),
        ("CheckBox", "Jalium.UI.Controls.CheckBox, Jalium.UI.Controls", "checked"),
        ("RadioButton", "Jalium.UI.Controls.RadioButton, Jalium.UI.Controls", "checked"),
        ("ToggleButton", "Jalium.UI.Controls.Primitives.ToggleButton, Jalium.UI.Controls", "checked"),
        ("ToggleSwitch", "Jalium.UI.Controls.ToggleSwitch, Jalium.UI.Controls", "checked"),
        ("Button", "Jalium.UI.Controls.Button, Jalium.UI.Controls", "resting"),
        ("Expander", "Jalium.UI.Controls.Expander, Jalium.UI.Controls", "resting"),
        ("ComboBox", "Jalium.UI.Controls.ComboBox, Jalium.UI.Controls", "resting"),
        ("MenuItem", "Jalium.UI.Controls.MenuItem, Jalium.UI.Controls", "resting"),
        ("ContentDialog", "Jalium.UI.Controls.ContentDialog, Jalium.UI.Controls", "resting"),
    ];

    private static readonly string[] BrushProperties = ["Foreground", "Background", "Fill", "Stroke"];

    [STAThread]
    private static int Main(string[] arguments)
    {
        var mode = arguments.Length > 0 ? arguments[0] : "il";
        ThemeLoader.Initialize();
        var application = new Application();
        FluentThemeManager.Apply(application, FluentThemeVariant.Light);

        var results = mode switch
        {
            "il" => Il(),
            "resolver" => Resolver(application),
            "calendar" => SelectedDay(application),
            "style" => Styling(application),
            "reader" => Reader(application),
            _ => new[] { $"unknown mode {mode}" },
        };

        var path = Path.Combine(AppContext.BaseDirectory, $"onaccent-{mode}.txt");
        File.WriteAllLines(path, results);
        Console.WriteLine($"wrote {path} ({results.Length} lines)");
        return 0;
    }

    private static string[] Il()
    {
        Say("=== 1. every ldstr that loads one of the target names, on the shipped assemblies ===");
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(static assembly => (assembly.GetName().Name ?? string.Empty).StartsWith("Jalium.UI", StringComparison.Ordinal))
            .OrderBy(static assembly => assembly.GetName().Name, StringComparer.Ordinal)
            .ToList();
        Say($"assemblies: {string.Join(", ", assemblies.Select(static assembly => assembly.GetName().Name))}");

        var perTarget = new Dictionary<string, int>();
        foreach (var assembly in assemblies)
        {
            foreach (var type in Types(assembly))
            {
                foreach (var member in Members(type))
                {
                    var literals = Literals(member);
                    var matched = literals.Where(static text => Targets.Contains(text, StringComparer.Ordinal)).ToList();
                    if (matched.Count == 0)
                    {
                        continue;
                    }

                    foreach (var name in matched)
                    {
                        perTarget[name] = perTarget.GetValueOrDefault(name) + 1;
                    }

                    Say($"HIT {assembly.GetName().Name}!{type.FullName}.{Name(member)} -> {string.Join(" | ", matched)}");
                    Say($"     full literals: {Trim(string.Join(" , ", literals.Distinct().Take(24)))}");
                }
            }
        }

        Say("");
        Say("=== 2. per-target hit count (0 means no by-name read in code on this runtime) ===");
        foreach (var target in Targets)
        {
            Say($"{target,-24} {perTarget.GetValueOrDefault(target)}");
        }

        Say("");
        Say("=== 3. is the name ALSO a direct ThemeColors member? an alias row cannot reach that shape ===");
        foreach (var holder in new[] { "Jalium.UI.Controls.ThemeColors", "Jalium.UI.Controls.Themes.ThemeColors" })
        {
            Type? found = null;
            foreach (var assembly in assemblies)
            {
                found = SafeGetType(assembly, holder);
                if (found is not null)
                {
                    break;
                }
            }

            if (found is null)
            {
                Say($"holder {holder}: absent");
                continue;
            }

            Say($"holder {found.FullName}:");
            foreach (var target in Targets)
            {
                var property = found.GetProperty(target, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                var field = found.GetField(target, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                object? value = null;
                string read = "-";
                try
                {
                    value = property?.GetValue(null) ?? field?.GetValue(null);
                    read = value is null ? "null" : value.GetType().Name;
                }
                catch (Exception error)
                {
                    read = "threw " + error.GetType().Name;
                }

                Say($"  {target,-24} property={property is not null} field={field is not null} read={read} value={Trim(value?.ToString())}");
            }
        }

        return Lines.ToArray();
    }

    /// <summary>
    /// The four members the IL census found reading the name in code, invoked directly. A resolver that hands back the
    /// probe INSTANCE after an application-level entry was installed under the name is the arrival proof the alias layer
    /// uses for every row; a resolver that keeps returning its own fallback means the read is cached and the row would
    /// reach nothing.
    /// </summary>
    private static string[] Resolver(Application application)
    {
        Say("=== resolvers the IL census named, called resting and with a probe under TextOnAccent ===");
        var probe = new SolidColorBrush(Probe);
        foreach (var (owner, method) in new[]
                 {
                     ("Jalium.UI.Controls.Calendar", "ResolveSelectedTextBrush"),
                     ("Jalium.UI.Controls.Primitives.CalendarDayButton", "ResolveSelectedForegroundBrush"),
                     ("Jalium.UI.Controls.SwipeControl", "ResolveSwipeItemForeground"),
                     ("Jalium.UI.Controls.Primitives.DataGridRowHeader", "ResolveSelectionIndicatorBrush"),
                 })
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(owner, throwOnError: false))
                .FirstOrDefault(static found => found is not null);
            if (type is null)
            {
                Say($"{owner}: type absent");
                continue;
            }

            var info = type.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (info is null)
            {
                Say($"{owner}.{method}: method absent");
                continue;
            }

            object? instance = null;
            if (!info.IsStatic)
            {
                try
                {
                    instance = Activator.CreateInstance(type);
                }
                catch (Exception error)
                {
                    Say($"{owner}.{method}: ctor threw {error.GetType().Name} {Trim(error.InnerException?.Message ?? error.Message, 90)}");
                    continue;
                }
            }

            var arguments = info.GetParameters().Length == 0 ? Array.Empty<object?>() : null;
            if (arguments is null)
            {
                Say($"{owner}.{method}: takes {info.GetParameters().Length} argument(s), not invoked here");
                continue;
            }

            var resting = Invoke(info, instance);
            application.Resources["TextOnAccent"] = probe;
            var withProbe = Invoke(info, instance);
            application.Resources.Remove("TextOnAccent");
            var afterRemove = Invoke(info, instance);
            Say($"{owner}.{method}");
            Say($"   resting    = {Describe(resting)}");
            Say($"   withProbe  = {Describe(withProbe)} followed={ReferenceEquals(probe, withProbe)}");
            Say($"   afterRemove= {Describe(afterRemove)} backToResting={EqualsColor(afterRemove, resting)}");
        }

        return Lines.ToArray();
    }

    private static object? Invoke(MethodInfo info, object? instance)
    {
        try
        {
            return info.Invoke(instance, Array.Empty<object?>());
        }
        catch (Exception error)
        {
            return "threw " + (error.InnerException ?? error).GetType().Name;
        }
    }

    private static bool EqualsColor(object? left, object? right) =>
        left is SolidColorBrush a && right is SolidColorBrush b && a.Color == b.Color;

    /// <summary>
    /// The surface question the row turns on, answered at the element rather than the resolver. A mounted Calendar with
    /// today selected is asked for its day buttons' effective Foreground, once resting and once with the probe already
    /// installed under the name, so "the selected day paints with what this name resolves to" is read off a built tree
    /// instead of inferred from the resolver's name. The first pass over all fourteen mounted families walked in their
    /// resting state and came back 0/14, which is an instrument reading (no surface had entered a state where the name
    /// is asked for), not a null measurement - hence this leg drives the state.
    /// </summary>
    private static string[] SelectedDay(Application application)
    {
        Say("=== the calendar family, mounted resting and again with a probe installed under TextOnAccent ===");
        Say("calibration (does this walk see a tree at all?): " + MountButton(application));
        foreach (var family in new[] { "Calendar", "DatePicker", "TimePicker" })
        {
            var resting = MountFamily(application, family, probeInstalled: false);
            var withProbe = MountFamily(application, family, probeInstalled: true);
            application.Resources.Remove("TextOnAccent");
            Say($"{family,-12} resting : {resting}");
            Say($"{family,-12} withProbe: {withProbe}");
        }

        return Lines.ToArray();
    }

    /// <summary>
    /// The instrument check the 0/14 walk made necessary: mount a control whose template this library is known to ship
    /// (Button, via DefaultButtonStyle) the same way and count what the walk sees. A non-zero count here beside
    /// tree=0 for the Calendar says the Calendar has no template in this host; a zero here says the walk is blind and no
    /// surface reading from it means anything.
    /// </summary>
    private static string MountButton(Application application)
    {
        var button = new Button { Content = "calibrate", Width = 140, Height = 32, Style = FluentThemeManager.GetStyle("DefaultButtonStyle") };
        var host = new StackPanel();
        host.Children.Add(button);
        var window = new Window
        {
            Title = "OnAccentProbe.Calibrate",
            Content = host,
            Width = 320,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = 340,
            Top = 180,
        };
        window.Show();
        Pump(14);
        var applied = button.ApplyTemplate() ? "applied" : "no-template";
        window.UpdateLayout();
        Pump(6);
        var elements = new List<FrameworkElement>();
        Collect(button, string.Empty, elements);
        var text = $"Button: tpl={applied} tree={elements.Count} size={button.ActualWidth}x{button.ActualHeight} "
            + $"types[{Trim(string.Join(",", elements.Select(static element => element.GetType().Name).Distinct().Take(12)), 120)}]";
        window.Close();
        Pump(4);
        return text;
    }

    private static string MountFamily(Application application, string family, bool probeInstalled)
    {
        SolidColorBrush? probe = probeInstalled ? new SolidColorBrush(Probe) : null;
        if (probe is not null)
        {
            application.Resources["TextOnAccent"] = probe;
        }

        var calendarType = Type.GetType($"Jalium.UI.Controls.{family}, Jalium.UI.Controls", throwOnError: false);
        if (calendarType is null)
        {
            return $"{family} type absent";
        }

        var calendar = (FrameworkElement)Activator.CreateInstance(calendarType)!;
        Set(calendar, "SelectedDate", DateTime.Today);
        Set(calendar, "DisplayDate", DateTime.Today);
        Set(calendar, "Date", DateTime.Today);
        Set(calendar, "Value", DateTime.Today);
        Set(calendar, "SelectionMode", "Single");
        Set(calendar, "Width", 420d);
        Set(calendar, "Height", 360d);
        var panel = new StackPanel();
        panel.Children.Add(calendar);
        var window = new Window
        {
            Title = "OnAccentProbe.Calendar",
            Content = panel,
            Width = 460,
            Height = 420,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = 300,
            Top = 140,
        };
        window.Show();
        Pump(16);
        var template = calendar.ApplyTemplate() ? "applied" : "no-template";
        window.UpdateLayout();
        Pump(8);

        var days = new List<FrameworkElement>();
        Collect(calendar, "DayButton", days);
        var every = new List<FrameworkElement>();
        Collect(calendar, string.Empty, every);
        var shapes = every.Select(static element => element.GetType().Name).Distinct().OrderBy(static text => text);
        var report = new List<string>();
        foreach (var day in days)
        {
            var foreground = Get(day, "Foreground");
            var tag = $"{day.GetType().Name}{(Get(day, "IsSelected") is true ? "[selected]" : string.Empty)}="
                + $"{(foreground is SolidColorBrush brush ? "#" + brush.Color : Describe(foreground))}"
                + (probe is not null && ReferenceEquals(probe, foreground) ? "=PROBE" : string.Empty);
            report.Add(tag);
        }

        var selected = report.Count(static text => text.Contains("[selected]", StringComparison.Ordinal));
        var painted = report.Count(static text => text.Contains("=PROBE", StringComparison.Ordinal));
        var inked = new List<string>();
        CollectInk(calendar, probe, inked);
        var text = $"days={days.Count} tree={every.Count} size={calendar.ActualWidth}x{calendar.ActualHeight} tpl={template} selectedRead={selected} probePainted={painted} "
            + $"selectedInk={string.Join(" ", report.Where(static entry => entry.Contains("[selected]", StringComparison.Ordinal)))} "
            + $"probeOnAnyBrushProperty={inked.Count} {Trim(string.Join(" ; ", inked.Take(6)), 160)} "
            + $"types[{Trim(string.Join(",", shapes), 300)}]";
        Pump(3);
        window.Close();
        Pump(4);
        return Trim(text, 1500);
    }

    /// <summary>Walks the built tree for any brush-bearing property that ended up holding the probe instance.</summary>
    private static void CollectInk(DependencyObject node, Brush? probe, List<string> found)
    {
        if (node is FrameworkElement element && probe is not null)
        {
            foreach (var property in BrushProperties)
            {
                if (Get(element, property) is { } value && ReferenceEquals(value, probe))
                {
                    found.Add($"{element.GetType().Name}.{property}");
                }
            }
        }

        var count = VisualTreeHelper.GetChildrenCount(node);
        for (var index = 0; index < count; index++)
        {
            if (VisualTreeHelper.GetChild(node, index) is { } child)
            {
                CollectInk(child, probe, found);
            }
        }
    }

    private static void Collect(DependencyObject node, string typeName, List<FrameworkElement> found)
    {
        var count = VisualTreeHelper.GetChildrenCount(node);
        for (var index = 0; index < count; index++)
        {
            if (VisualTreeHelper.GetChild(node, index) is not { } child)
            {
                continue;
            }

            if (child is FrameworkElement element && element.GetType().Name.Contains(typeName, StringComparison.Ordinal))
            {
                found.Add(element);
            }

            Collect(child, typeName, found);
        }
    }

    /// <summary>
    /// What the tree=0 reading means. If the framework's own Generic dictionary were missing from this host, every
    /// control this library does not style would be blank and the reading would be about the host, not about the
    /// calendar family. So: is a framework-shipped key present at application scope, and does each family have an
    /// implicit style and a Template of its own?
    /// </summary>
    private static string[] Styling(Application application)
    {
        Say("=== is the framework's own theme dictionary in this host? ===");
        foreach (var key in new[] { "DefaultFocusVisualStyle", "FocusVisualRingStyle", "TextOnAccent", "Calendar", "DatePicker" })
        {
            Say($"app.TryFindResource(\"{key}\") = {Describe(application.TryFindResource(key))}");
        }

        foreach (var family in new[] { "Calendar", "DatePicker", "TimePicker", "Button" })
        {
            var type = Type.GetType($"Jalium.UI.Controls.{family}, Jalium.UI.Controls", throwOnError: false);
            if (type is null)
            {
                Say($"{family}: type absent");
                continue;
            }

            var implicitStyle = application.TryFindResource(type);
            var element = (FrameworkElement)Activator.CreateInstance(type)!;
            var ownStyle = Get(element, "Style");
            var template = Get(element, "Template");
            Say($"{family,-11} implicit-by-type={Describe(implicitStyle)} ownStyle={Describe(ownStyle)} template={Describe(template)}");
            if (implicitStyle is Style style)
            {
                Say($"   setters={style.Setters.Count} "
                    + Trim(string.Join(" | ", style.Setters.Cast<object>().OfType<Setter>()
                        .Select(static setter => $"{setter.Property}(value={setter.Value?.GetType().Name ?? "null"})")
                        .Take(14)), 500));
                var fromStyle = style.Setters.Cast<object>().OfType<Setter>()
                    .FirstOrDefault(static setter => setter.Property == Control.TemplateProperty);
                Say($"   Template-cell={Describe(fromStyle?.Value)}");
            }
        }

        return Lines.ToArray();
    }

    private static string[] Reader(Application application)
    {
        Say("=== app-scope resolution of each target name, resting vs with a probe installed ===");
        foreach (var target in Targets)
        {
            var resting = application.TryFindResource(target);
            Say($"{target,-24} resting={Describe(resting)}");
        }

        Say("");
        Say("=== mounted surfaces: probe brush installed under TextOnAccent, read by instance identity ===");
        var probe = new SolidColorBrush(Probe);
        var canvas = new StackPanel { Spacing = 8 };
        var built = new List<(string Label, FrameworkElement Element, string Note)>();
        foreach (var (label, qualified, state) in Surfaces)
        {
            var type = Type.GetType(qualified, throwOnError: false);
            if (type is null)
            {
                Say($"{label,-16} type absent on this runtime ({qualified})");
                continue;
            }

            FrameworkElement? element;
            string note;
            try
            {
                element = (FrameworkElement)Activator.CreateInstance(type)!;
                note = Configure(element, state);
            }
            catch (Exception error)
            {
                element = null;
                note = "ctor/state threw " + error.GetType().Name + ": " + Trim(error.InnerException?.Message ?? error.Message, 120);
            }

            if (element is null)
            {
                Say($"{label,-16} {note}");
                continue;
            }

            canvas.Children.Add(element);
            built.Add((label, element, note));
        }

        var window = new Window
        {
            Title = "OnAccentProbe",
            Content = canvas,
            Width = 700,
            Height = 620,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = 260,
            Top = 120,
        };
        window.Show();
        Pump(14);
        window.UpdateLayout();
        Pump(6);

        foreach (var (label, element, note) in built)
        {
            Say($"{label,-16} scope[{label}] {note}");
            Say($"{"",-16}  name-in-scope resting={Describe(element.TryFindResource("TextOnAccent"))}");
        }

        application.Resources["TextOnAccent"] = probe;
        Pump(6);
        Say("");
        Say("--- probe installed under TextOnAccent at application scope ---");
        foreach (var (label, element, _) in built)
        {
            Say($"{label,-16} name-in-scope with probe={Describe(element.TryFindResource("TextOnAccent"))}");
        }

        Say("");
        Say("--- re-mounted with the probe already in place, walked for the probe INSTANCE ---");
        application.Resources.Remove("TextOnAccent");
        window.Content = new StackPanel();
        var second = new StackPanel { Spacing = 8 };
        foreach (var (label, qualified, state) in Surfaces)
        {
            var type = Type.GetType(qualified, throwOnError: false);
            if (type is null)
            {
                continue;
            }

            try
            {
                var element = (FrameworkElement)Activator.CreateInstance(type)!;
                Configure(element, state);
                second.Children.Add(element);
            }
            catch (Exception error)
            {
                Say($"{label,-16} remount threw {error.GetType().Name}");
            }
        }

        application.Resources["TextOnAccent"] = probe;
        window.Content = second;
        Pump(16);
        window.UpdateLayout();
        Pump(8);
        foreach (var child in second.Children.Cast<FrameworkElement>())
        {
            var hits = new List<string>();
            Walk(child, probe, hits);
            Say($"{child.GetType().Name,-16} probePaintedElements={hits.Count} {Trim(string.Join(" ; ", hits.Take(8)), 200)}");
        }

        application.Resources.Remove("TextOnAccent");
        Pump(4);
        window.Close();
        Pump(4);
        return Lines.ToArray();
    }

    /// <summary>Puts a surface into the state where an on-accent colour would show, without any real input.</summary>
    private static string Configure(FrameworkElement element, string state)
    {
        if (state == "checked")
        {
            Set(element, "IsChecked", true);
            Set(element, "IsToggled", true);
        }

        return $"state={state} ischecked={Get(element, "IsChecked")} istoggled={Get(element, "IsToggled")}";
    }

    private static void Walk(DependencyObject node, Brush probe, List<string> hits)
    {
        if (node is FrameworkElement element)
        {
            foreach (var property in BrushProperties)
            {
                var value = Get(element, property);
                if (ReferenceEquals(value, probe))
                {
                    hits.Add($"{element.GetType().Name}{(element.Name is { Length: > 0 } n ? "#" + n : string.Empty)}.{property}=PROBE");
                }
                else if (value is SolidColorBrush brush && brush.Color == Probe)
                {
                    hits.Add($"{element.GetType().Name}.{property}=colour-only(not the instance)");
                }
            }
        }

        var count = VisualTreeHelper.GetChildrenCount(node);
        for (var index = 0; index < count; index++)
        {
            if (VisualTreeHelper.GetChild(node, index) is { } child)
            {
                Walk(child, probe, hits);
            }
        }
    }

    private static void Set(DependencyObject target, string property, object value)
    {
        var descriptor = target.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
        if (descriptor is not { CanWrite: true })
        {
            return;
        }

        try
        {
            var argument = descriptor.PropertyType.IsEnum && value is string text
                ? Enum.Parse(descriptor.PropertyType, text)
                : value;
            descriptor.SetValue(target, argument);
        }
        catch (Exception)
        {
            // A surface without that member is not the one under test; the read-back in the report says whether the
            // state actually landed.
        }
    }

    private static object? Get(DependencyObject target, string property)
    {
        try
        {
            return target.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance)?.GetValue(target);
        }
        catch (Exception)
        {
            return "(threw)";
        }
    }

    private static IEnumerable<Type> Types(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException error)
        {
            return error.Types.Where(static type => type is not null)!;
        }
        catch (Exception)
        {
            return Array.Empty<Type>();
        }
    }

    private static Type? SafeGetType(Assembly assembly, string name)
    {
        try
        {
            return assembly.GetType(name, throwOnError: false);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static IEnumerable<MethodBase> Members(Type type)
    {
        const BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        foreach (var method in type.GetMethods(flags))
        {
            yield return method;
        }

        foreach (var constructor in type.GetConstructors(flags))
        {
            yield return constructor;
        }
    }

    private static string Name(MethodBase member) =>
        member is MethodInfo method
            ? method.Name + (method.IsStatic ? " [static]" : string.Empty)
            : ".ctor";

    /// <summary>
    /// Decodes the string literals an IL body loads: ldstr is 0x72, its operand is a #US token. A byte scan can read an
    /// operand byte that happens to be 0x72 as an opcode, which is why ResolveString failures are skipped rather than
    /// reported - the risk is a stray literal, never a missing one, so a hit stands and a null reading needs the
    /// calibration names to confirm the instrument.
    /// </summary>
    private static List<string> Literals(MethodBase member)
    {
        var literals = new List<string>();
        if (Body(member) is not { } il)
        {
            return literals;
        }

        var module = member.Module;
        for (var index = 0; index + 4 < il.Length; index++)
        {
            if (il[index] != 0x72)
            {
                continue;
            }

            var token = BitConverter.ToInt32(il, index + 1);
            try
            {
                literals.Add(module.ResolveString(token));
            }
            catch (Exception)
            {
            }
        }

        return literals;
    }

    private static byte[]? Body(MethodBase member)
    {
        try
        {
            return member.GetMethodBody()?.GetILAsByteArray();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string Describe(object? value) => value switch
    {
        null => "null",
        SolidColorBrush brush => $"{brush.GetType().Name} #{brush.Color} instance=0x{System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value):X}",
        _ => value.GetType().Name + " " + Trim(value.ToString()),
    };

    private static string Trim(string? text) => Trim(text, 160);

    private static string Trim(string? text, int limit)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "-";
        }

        var flat = text!.Replace('\n', ' ').Replace('\r', ' ');
        return flat.Length <= limit ? flat : flat[..limit] + "...";
    }

    private static void Say(string text) => Lines.Add(text);

    private static int Pump(int frames)
    {
        var frame = new DispatcherFrame();
        var seen = 0;
        var deadline = System.Diagnostics.Stopwatch.GetTimestamp() + System.Diagnostics.Stopwatch.Frequency * 4;
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames || System.Diagnostics.Stopwatch.GetTimestamp() > deadline)
            {
                frame.Continue = false;
            }
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
        return seen;
    }
}
