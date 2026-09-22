using System.Reflection;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace TypoProbe;

/// <summary>
/// Throwaway probe for the #72 candidate: the framework projects its body type at 12 (so 10 and 8 downstream), while
/// the upstream authority this library transcribes from sets control content at 14
/// (microsoft-ui-xaml @19e3bdc3, dxaml/xcp/dxaml/themes/generic.xaml:36 <c>ControlContentThemeFontSize=14</c>), and 31
/// rows of our own styles already name 14 literally. Where we do not name a size, text inherits the framework's 12, so
/// the two disagree inside the same app.
///
/// The question this run answers, against the shipped 26.10.9 NuGet and not the sibling source tree:
///
///   census  - what does the runtime's own typography surface look like? every PUBLIC static member of
///             Jalium.UI.Controls.Themes.ThemeManager whose name carries Font or Typograph, with the parameter types
///             spelled out, plus the values those properties currently return. Public members only: nothing here reads
///             a private field.
///   reach   - the measurement, in the order the mode names: resting numbers, then the same numbers after one call to
///             the four-arg entry point with the body size set to 14 and the families passed back unchanged. Reads
///             app-scope projections, what a mounted TextBlock actually resolves for FontSize, what a Button's content
///             presenter resolves, what SystemFonts reports, and whether a block that was already on screen follows.
///
/// Modes are "after" (apply once the Astra manifest is installed - the ordering a host would use if this shipped) and
/// "before" (apply first, then install the manifest - the ordering that would let a late framework refresh overwrite
/// rows the manifest aliases). Both legs run in their own process so neither contaminates the other.
///
/// Diagnostic-only: opens its own windows, closes them, writes nothing into the user's profile, raises no real input.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = new();

    private const double Target = 14d;

    private static int Main(string[] arguments)
    {
        var mode = arguments.Length > 0 ? arguments[0] : "reach-after";
        ThemeLoader.Initialize();
        var application = new Application();

        if (mode == "reach-before")
        {
            ApplyTarget(application, "before the manifest is installed");
        }

        if (mode != "raw")
        {
            FluentThemeManager.Apply(application, FluentThemeVariant.Light);
        }

        var results = mode switch
        {
            "census" => Census(),
            "reach-after" => Reach(application, applyFirst: false),
            "reach-before" => Reach(application, applyFirst: true),
            "raw" => Raw(application),
            _ => new[] { $"unknown mode {mode}" },
        };

        var path = Path.Combine(AppContext.BaseDirectory, $"typo-{mode}.txt");
        File.WriteAllLines(path, results);
        Console.WriteLine($"wrote {path} ({results.Length} lines)");
        return 0;
    }

    private static string[] Census()
    {
        Say("=== 1. public typography surface on the shipped runtime ===");
        var manager = FrameworkType("Jalium.UI.Controls.Themes.ThemeManager");
        if (manager is null)
        {
            Say("Jalium.UI.Controls.Themes.ThemeManager is absent from the loaded runtime - the instrument is broken, "
                + "not the answer");
            return Lines.ToArray();
        }

        Say($"holder: {manager.Assembly.GetName().Name} (loaded assembly identity, not the sibling source tree)");
        foreach (var method in manager.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                     .Where(static item => item.Name.Contains("Font", StringComparison.Ordinal)
                        || item.Name.Contains("Typograph", StringComparison.Ordinal))
                     .OrderBy(static item => item.Name, StringComparer.Ordinal))
        {
            var parameters = string.Join(", ", method.GetParameters().Select(static item => $"{item.ParameterType.Name} {item.Name}"));
            Say($"  method {method.Name}({parameters}) -> {method.ReturnType.Name}");
        }

        foreach (var property in manager.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                     .Where(static item => item.Name.Contains("Font", StringComparison.Ordinal))
                     .OrderBy(static item => item.Name, StringComparer.Ordinal))
        {
            object? value = null;
            try
            {
                value = property.GetValue(null);
            }
            catch (Exception error)
            {
                value = $"threw {error.GetType().Name}";
            }

            Say($"  property {property.PropertyType.Name} {property.Name} = {Trim(value?.ToString())}");
        }

        Say("");
        Say("=== 2. the four-arg overload's parameter shape, which is what the probe calls ===");
        var four = manager.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .FirstOrDefault(static item => item.Name == "ApplyTypography"
                && item.GetParameters().Length == 4
                && item.GetParameters()[3].ParameterType == typeof(double));
        Say(four is null
            ? "  absent: this runtime has no public ApplyTypography(string,string,string,double), so the number cannot "
                + "be moved by a call and #72 has no in-process lever"
            : $"  present: ({string.Join(", ", four.GetParameters().Select(static item => $"{item.ParameterType.Name} {item.Name}"))})");
        var three = manager.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .FirstOrDefault(static item => item.Name == "ApplyTypography" && item.GetParameters().Length == 3);
        Say($"  three-arg overload: {(three is null ? "absent" : "present")}");
        return Lines.ToArray();
    }

    /// <summary>
    /// Attribution control: the same readings with no Astra manifest installed, so a 14 here is a framework default and
    /// not something our rows put there.
    /// </summary>
    private static string[] Raw(Application application)
    {
        Say("=== no FluentJalium manifest installed: framework defaults only ===");
        Say($"FluentThemeManager.IsInitialized={FluentThemeManager.IsInitialized}");
        var live = Mount(application, "raw");
        Report(application, "raw", live);
        Close(live.Window);
        return Lines.ToArray();
    }

    private static string[] Reach(Application application, bool applyFirst)
    {
        Say($"=== leg order: apply the body size {(applyFirst ? "BEFORE" : "AFTER")} FluentThemeManager.Apply ===");
        Say($"FluentThemeManager.IsInitialized={FluentThemeManager.IsInitialized} "
            + $"native ThemeMode={FluentThemeManager.NativeThemeMode}");

        Say("");
        Say("=== 1. resting: what the app projects, and what mounted text resolves to ===");
        var live = Mount(application, "resting");
        Report(application, "resting", live);

        Say("");
        Say("=== 2. one call to the framework's own typography entry point, families passed back unchanged ===");
        if (!applyFirst)
        {
            ApplyTarget(application, "after the manifest");
        }
        else
        {
            Say("  (already applied before the manifest; nothing called in this step)");
        }

        var after = Mount(application, "after apply");
        Report(application, "after", after);
        Say($"  the resting block, still on screen and never re-mounted: FontSize={live.Block.FontSize} "
            + $"(a live instance that follows would read the same as a fresh mount)");
        Close(after.Window);
        Close(live.Window);

        Say("");
        Say("=== 3. did our own rows survive the framework refresh the call triggers? ===");
        foreach (var key in new[] { "TextPrimary", "TextSecondary", "SurfaceBackground", "ControlBackground", "BodyTextBlockStyle" })
        {
            Say($"  {key,-20} {Describe(application.TryFindResource(key))}");
        }

        return Lines.ToArray();
    }

    private static void Report(Application application, string label, Mounted mounted)
    {
        foreach (var name in new[] { "BodyFontSize", "CaptionFontSize", "SmallFontSize", "ControlContentThemeFontSize",
                                     "BodyFontFamily", "MonoFontFamily", "DisplayFontFamily" })
        {
            Say($"  {label,-7} app.{name,-27} {Describe(application.TryFindResource(name))}");
        }

        Say($"  {label,-7} TextBlock() default FontSize   {new TextBlock().FontSize}");
        Say($"  {label,-7} mounted (no style) FontSize    {mounted.Plain.FontSize}");
        Say($"  {label,-7} mounted via BodyFontSize       {mounted.Block.FontSize}");
        Say($"  {label,-7} Button content presenter       {PresenterFontSize(mounted.Button)}");
        Say($"  {label,-7} SystemFonts.CaptionFontSize    {SystemSize("CaptionFontSize")}");
        Say($"  {label,-7} SystemFonts.MessageFontSize    {SystemSize("MessageFontSize")}");
    }

    private static void ApplyTarget(Application application, string when)
    {
        var manager = FrameworkType("Jalium.UI.Controls.Themes.ThemeManager");
        if (manager is null)
        {
            Say("  ThemeManager absent - nothing applied, so every later reading is a resting reading");
            return;
        }

        var four = manager.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .FirstOrDefault(static item => item.Name == "ApplyTypography"
                && item.GetParameters().Length == 4
                && item.GetParameters()[3].ParameterType == typeof(double));
        if (four is null)
        {
            Say("  no public four-arg ApplyTypography on this runtime: the size has no call-level lever");
            return;
        }

        var families = new[] { "DisplayFontFamily", "BodyFontFamily", "MonoFontFamily" }
            .Select(name => (application.TryFindResource(name) as FontFamily)?.ToString() ?? string.Empty)
            .ToArray();
        Say($"  passing back the families as found: display='{families[0]}' body='{families[1]}' mono='{families[2]}' "
            + $"so only the number moves ({when})");
        try
        {
            four.Invoke(null, new object[] { families[0], families[1], families[2], Target });
            Say($"  invoked {manager.Name}.ApplyTypography(..., {Target}) with no exception");
        }
        catch (Exception error)
        {
            Say($"  invoke threw {error.GetType().Name}: {Trim(error.Message, 200)}");
        }
    }

    private sealed record Mounted(TextBlock Block, TextBlock Plain, Button Button, Window Window);

    private static Mounted Mount(Application application, string leg)
    {
        var dictionary = (ResourceDictionary)XamlReader.Parse(
            "<ResourceDictionary xmlns='http://schemas.jalium.ui/2024' " +
            "xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>" +
            "<Style x:Key='S' TargetType='TextBlock'>" +
            "<Setter Property='FontSize' Value='{ThemeResource BodyFontSize}' /></Style>" +
            "</ResourceDictionary>")!;
        var merged = application.Resources.MergedDictionaries;
        merged.Add(dictionary);
        var block = new TextBlock { Text = "body", Style = (Style)dictionary["S"]! };
        var plain = new TextBlock { Text = "plain" };
        var button = new Button { Content = "click" };
        var panel = new StackPanel();
        panel.Children.Add(block);
        panel.Children.Add(plain);
        panel.Children.Add(button);
        var window = new Window
        {
            Title = $"TypoProbe.{leg}",
            Content = panel,
            Width = 320,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = 420,
            Top = 240,
        };
        window.Show();
        Pump(12);
        window.UpdateLayout();
        Pump(4);
        Say($"  mounted '{leg}': keys in the probe dictionary={dictionary.Count}");
        return new Mounted(block, plain, button, window);
    }

    private static void Close(Window window)
    {
        window.Close();
        Pump(4);
    }

    private static string PresenterFontSize(Button button)
    {
        // ContentPresenter carries no FontSize on this runtime (measured by the compile failing on it), so the read
        // that matters for a button is the TextBlock the presenter produces.
        foreach (var node in Descendants(button))
        {
            if (node is TextBlock text)
            {
                return $"{text.FontSize} (TextBlock in the template, Button.FontSize={button.FontSize})";
            }
        }

        return $"no TextBlock in the built tree (Button.FontSize={button.FontSize})";
    }

    private static string SystemSize(string name)
    {
        var fonts = FrameworkType("Jalium.UI.SystemFonts");
        var member = fonts?.GetProperty(name, BindingFlags.Public | BindingFlags.Static)
            ?? (object?)fonts?.GetField(name, BindingFlags.Public | BindingFlags.Static);
        if (member is null)
        {
            return $"{name} absent on this runtime";
        }

        try
        {
            var value = member is PropertyInfo property ? property.GetValue(null)
                : ((FieldInfo)member).GetValue(null);
            return Trim(value?.ToString());
        }
        catch (Exception error)
        {
            return $"threw {error.GetType().Name}";
        }
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is null)
            {
                continue;
            }

            yield return child;
            foreach (var deeper in Descendants(child))
            {
                yield return deeper;
            }
        }
    }

    private static Type? FrameworkType(string full)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetType(full, throwOnError: false) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    private static string Describe(object? value) => value switch
    {
        null => "null",
        double number => $"double {number}",
        FontFamily family => $"FontFamily '{Trim(family.ToString(), 60)}'",
        _ => value.GetType().Name + " " + Trim(value.ToString()),
    };

    private static string Trim(string? text) => Trim(text, 120);

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
        var pumping = Dispatcher.CurrentDispatcher;
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
        using var watchdog = new System.Threading.Timer(
            _ => pumping.InvokeAsync(() => frame.Continue = false), null, TimeSpan.FromSeconds(8), Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
        return seen;
    }
}
