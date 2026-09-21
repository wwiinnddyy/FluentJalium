using System.ComponentModel;
using System.Text;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

namespace MotionProbe;

/// <summary>
/// The four questions that decide "减动效资源键化" before any key is written, asked of the pinned 26.10.9 runtime
/// with mounted elements on a shown window.
/// </summary>
/// <remarks>
/// Upstream publishes its animation durations as eleven-plus <c>&lt;x:String&gt;</c> rows
/// (Common_themeresources_any.xaml:603-606: ControlNormalAnimationDuration=00:00:00.250,
/// ControlFastAnimationDuration=00:00:00.167, ControlFasterAnimationDuration=00:00:00.083). This runtime has no
/// Storyboard-driven template animation to feed: motion lives in <c>UIElement.TransitionDuration</c>, a
/// <c>Duration</c>-typed dependency property that the framework reads fresh at arm time and that suppresses the
/// transition outright when it resolves to nothing or to at or below zero. So a duration row is a plausible
/// reduce-motion lever
/// - and "plausible" is worth exactly nothing here, because the last three segments each found a typed resource row
/// that parsed clean and carried no value (x:Double to 0, an unknown enum name to a different member, FontFamily to
/// an empty Source). Hence:
/// 1. [0]/[1] Is the instrument even live? The framework gates automatic transitions on
///    SystemParameters.ClientAreaAnimation &amp;&amp; UIEffects; if either is off, every "no animation" reading below is
///    that setting and not the row, so both are printed before anything is claimed.
/// 2. [2] Can a duration reach TransitionDuration through each consumer shape, with the row written every way that
///    is available (p:Duration, x:String in both time forms, TimeSpan, and x:Double as a known-dead control)?
/// 3. [3] Does a runtime write of the same row reach an element that is already loaded in a shown window?
/// 4. [4] Does a zero duration really stop the animation, and does a long one really run? Same pump, same read-back
///    route, so the pair cannot both be explained by the instrument.
/// The discriminator that makes [2] safe to read: default(Duration) reports HasTimeSpan=false and ToString
/// "Automatic", while an intentional zero reports HasTimeSpan=true and "00:00:00" - so a dropped row and a
/// reduce-motion row are distinguishable, which is exactly the ambiguity a colour-blind pixel judge would leave.
/// </remarks>
internal static class Program
{
    private static readonly List<string> Lines = [];

    private const string Root =
        "xmlns=\"http://schemas.jalium.ui/2024\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" " +
        "xmlns:p=\"clr-namespace:Jalium.UI;assembly=Jalium.UI.Core\" " +
        "xmlns:m=\"clr-namespace:Jalium.UI;assembly=Jalium.UI.Managed\" " +
        "xmlns:sys=\"clr-namespace:System;assembly=System.Private.CoreLib\"";

    private static readonly Color StartColor = Color.FromRgb(0xFF, 0x00, 0x00);
    private static readonly Color EndColor = Color.FromRgb(0x00, 0x00, 0xFF);

    private static Application _application = null!;
    private static Grid _root = new();

    [STAThread]
    private static int Main()
    {
        try
        {
            var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
            renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            _application = new Application();
            FluentThemeManager.Apply(_application);
            var window = new Window { Width = 320, Height = 240, Content = _root };
            window.Show();
            Pump(8);

            Instrument();
            Surface();
            Matrix();
            Flip();
            Motion();
        }
        catch (Exception exception)
        {
            Note("BOOT", "threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }

        var path = Path.Combine(AppContext.BaseDirectory, "motion-probe.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        foreach (var line in Lines)
        {
            Console.WriteLine(line);
        }

        return 0;
    }

    // --- [0] is the judge live ----------------------------------------------------------------------------
    // Both settings must be true or nothing below means anything: the framework asks them once per arm attempt, so
    // a machine with animations off shows "no transition" for every row, landed or not.
    private static void Instrument()
    {
        Heading("[0] instrument");
        Note("SystemParameters", $"ClientAreaAnimation={SystemParameters.ClientAreaAnimation}, UIEffects={SystemParameters.UIEffects}");
        Note("Astra", $"FluentThemeManager.AnimationsEnabled={FluentThemeManager.AnimationsEnabled}, ReduceMotion={FluentThemeManager.ReduceMotion}");
        var main = _application.MainWindow is null ? "none" : _application.MainWindow.GetType().Name;
        var merged = _application.Resources.MergedDictionaries.Count;
        Note("application", "MainWindow named=" + main + ", merged dictionaries=" + merged);
    }

    // --- [1] the property the motion lives in -------------------------------------------------------------
    private static void Surface()
    {
        Heading("[1] property surface");
        var border = new Border();
        Note("TransitionDuration unset", Describe(border.TransitionDuration));
        Note("DP", Border.TransitionDurationProperty is { } dp
            ? $"name={dp.Name}, declared type={dp.PropertyType.Name}, registered on={dp.OwnerType.Name}"
            : "<no public identifier>");
        Note("Duration states", string.Join(", ", new[]
        {
            "default=" + Describe(default),
            "Automatic=" + Describe(Duration.Automatic),
            "zero=" + Describe(new Duration(TimeSpan.Zero)),
            "83ms=" + Describe(new Duration(TimeSpan.FromMilliseconds(83))),
        }));
        Note("Duration converter", typeof(Duration).GetCustomAttributes(typeof(System.ComponentModel.TypeConverterAttribute), false) is { Length: > 0 } attributes
            ? (attributes[0] as System.ComponentModel.TypeConverterAttribute)?.ConverterTypeName ?? "<unnamed>"
            : "<none>");

        // What the shipped runtime does with the converter, asked directly: the forms upstream writes and the form
        // our own markup writes are not the same string.
        var converter = TypeDescriptor.GetConverter(typeof(Duration));
        foreach (var text in new[] { "0:0:0.083", "00:00:00.083", "00:00:00.250", "0", "Auto", "" })
        {
            string result;
            try
            {
                result = Describe((Duration)converter.ConvertFromInvariantString(text)!);
            }
            catch (Exception exception)
            {
                result = "threw " + exception.GetType().Name;
            }

            Note($"DurationConverter \"{text}\"", result);
        }
    }

    // --- [2] row x consumer -------------------------------------------------------------------------------
    private static void Matrix()
    {
        Heading("[2] row x consumer");
        var rows = new (string Label, string Markup)[]
        {
            ("p:Duration via Core", "<p:Duration x:Key=\"K\">0:0:0.083</p:Duration>"),
            ("m:Duration via Managed", "<m:Duration x:Key=\"K\">0:0:0.083</m:Duration>"),
            ("Duration unqualified", "<Duration x:Key=\"K\">0:0:0.083</Duration>"),
            ("x:String 0:0:0.083", "<x:String x:Key=\"K\">0:0:0.083</x:String>"),
            ("x:String 00:00:00.083 upstream form", "<x:String x:Key=\"K\">00:00:00.083</x:String>"),
            ("sys:TimeSpan 00:00:00.083", "<sys:TimeSpan x:Key=\"K\">00:00:00.083</sys:TimeSpan>"),
            ("x:Double 0.083 known-dead control", "<x:Double x:Key=\"K\">0.083</x:Double>"),
        };

        var consumers = new (string Label, string DictionaryAddendum, string Element)[]
        {
            ("attribute {ThemeResource}", string.Empty,
                "<Border Width=\"20\" Height=\"20\" TransitionProperty=\"Background\" TransitionDuration=\"{ThemeResource K}\" />"),
            ("attribute {StaticResource}", string.Empty,
                "<Border Width=\"20\" Height=\"20\" TransitionProperty=\"Background\" TransitionDuration=\"{StaticResource K}\" />"),
            ("implicit style setter",
                "<Style TargetType=\"Border\"><Setter Property=\"TransitionDuration\" Value=\"{ThemeResource K}\"/></Style>",
                "<Border Width=\"20\" Height=\"20\" TransitionProperty=\"Background\" />"),
            ("inside a ControlTemplate", string.Empty,
                "<Button Width=\"60\" Height=\"40\"><Button.Template><ControlTemplate TargetType=\"Button\">" +
                "<Border Name=\"Inner\" Width=\"20\" Height=\"20\" Background=\"{ThemeResource AccentFillColorDefaultBrush}\" " +
                "TransitionProperty=\"Background\" TransitionDuration=\"{ThemeResource K}\" />" +
                "</ControlTemplate></Button.Template></Button>"),
        };

        foreach (var row in rows)
        {
            // One line per row either way: a row the reader cannot resolve makes every consumer report the same
            // parse error, which would bury the readings that matter.
            if (!PreFlight(row.Label, row.Markup))
            {
                continue;
            }

            foreach (var consumer in consumers)
            {
                RunCase(row.Label + " -> " + consumer.Label, row.Markup + consumer.DictionaryAddendum, consumer.Element);
            }
        }

        // The literal our styles use today, as the positive control for the whole matrix: if this does not read
        // 83ms the matrix is measuring a broken harness rather than a broken resource row.
        RunCase("literal 0:0:0.083 (control)", string.Empty,
            "<Border Width=\"20\" Height=\"20\" TransitionProperty=\"Background\" TransitionDuration=\"0:0:0.083\" />");
    }

    /// <summary>
    /// Asks the reader for the row on its own. Returning false means the row never became an object, so no consumer
    /// can be blamed for what follows and the consumers are skipped.
    /// </summary>
    private static bool PreFlight(string label, string markup)
    {
        try
        {
            var dictionary = LoadResourceDictionary(markup);
            var stored = dictionary is null ? null : dictionary["K"];
            if (stored is null)
            {
                Note(label, "row produced no value (<null>)");
                return false;
            }

            Note(label, $"row parses as {DescribeRow(stored)}");
            return true;
        }
        catch (Exception exception)
        {
            Note(label, "row does not parse: " + Trim(exception.Message));
            return false;
        }
    }

    private static void RunCase(string label, string dictionaryMarkup, string elementMarkup)
    {
        ResourceDictionary? dictionary = null;
        var stored = "<no row>";
        try
        {
            if (dictionaryMarkup.Length > 0)
            {
                dictionary = LoadResourceDictionary(dictionaryMarkup);
                stored = dictionary is null
                    ? "dictionary failed to load"
                    : DescribeRow(dictionary["K"]);
                if (dictionary is not null)
                {
                    Install(dictionary);
                }
            }

            var element = LoadElement(elementMarkup);
            if (element is null)
            {
                Note(label, $"row stored {stored}, element did not load");
                return;
            }

            Mount(element);
            var target = FindTarget(element);
            Note(label, $"row stored {stored}; {target.Label}.TransitionDuration={Describe(target.Value)}");
            Unmount(element);
        }
        catch (Exception exception)
        {
            Note(label, $"row stored {stored}; threw {exception.GetType().Name}: {Trim(exception.Message)}");
        }
        finally
        {
            if (dictionary is not null)
            {
                Uninstall(dictionary);
            }
        }
    }

    private static string DescribeRow(object? value) => value is null
        ? "<absent>"
        : $"{value.GetType().Name} \"{value}\"";

    // --- [3] does a runtime write reach a shown window ----------------------------------------------------
    private static void Flip()
    {
        Heading("[3] live flip");
        foreach (var (label, first, second) in new (string, Duration, Duration)[]
                 {
                     ("83ms -> 0", new Duration(TimeSpan.FromMilliseconds(83)), new Duration(TimeSpan.Zero)),
                     ("0 -> 250ms", new Duration(TimeSpan.Zero), new Duration(TimeSpan.FromMilliseconds(250))),
                 })
        {
            var dictionary = new ResourceDictionary();
            dictionary["K"] = first;
            Install(dictionary);
            try
            {
                var element = LoadElement("<Border Width=\"20\" Height=\"20\" TransitionProperty=\"Background\" TransitionDuration=\"{ThemeResource K}\" />");
                if (element is null)
                {
                    Note(label, "element did not load");
                    continue;
                }

                Mount(element);
                var before = Describe(((Border)element).TransitionDuration);
                dictionary["K"] = second;
                Pump(4);
                var afterWrite = Describe(((Border)element).TransitionDuration);
                FluentThemeManager.ApplyTheme(FluentThemeManager.CurrentTheme);
                Pump(4);
                var afterTheme = Describe(((Border)element).TransitionDuration);
                Note(label, $"mounted row={Describe(first)}: before={before}, after write={afterWrite}, after theme re-apply={afterTheme}");
                Unmount(element);
            }
            catch (Exception exception)
            {
                Note(label, "threw " + exception.GetType().Name + ": " + Trim(exception.Message));
            }
            finally
            {
                Uninstall(dictionary);
            }
        }

        // The shipping shape is not a bare element: the durations live on a Border inside a ControlTemplate, so the
        // flip has to be re-measured there. This is also the case the real templates will use, so a reading of
        // 180ms (the framework default) here would kill the plan rather than just a probe.
        Heading("[3b] live flip inside a ControlTemplate");
        var templateDictionary = new ResourceDictionary();
        templateDictionary["K"] = new Duration(TimeSpan.FromMilliseconds(83));
        Install(templateDictionary);
        try
        {
            var button = LoadElement(
                "<Button Width=\"60\" Height=\"40\"><Button.Template><ControlTemplate TargetType=\"Button\">" +
                "<Border Name=\"Inner\" Width=\"20\" Height=\"20\" TransitionProperty=\"Background\" TransitionDuration=\"{ThemeResource K}\" />" +
                "</ControlTemplate></Button.Template></Button>");
            if (button is null)
            {
                Note("template row 83ms -> 0", "button did not load");
            }
            else
            {
                Mount(button);
                Note("template row 83ms -> 0", "mounted: " + Describe(FindInTemplate((Button)button).Value));
                templateDictionary["K"] = new Duration(TimeSpan.Zero);
                Pump(4);
                Note("template row 83ms -> 0", "after write: " + Describe(FindInTemplate((Button)button).Value));
                Unmount(button);
            }
        }
        catch (Exception exception)
        {
            Note("template row 83ms -> 0", "threw " + exception.GetType().Name + ": " + Trim(exception.Message));
        }
        finally
        {
            Uninstall(templateDictionary);
        }
    }

    // --- [4] does the lever move anything ------------------------------------------------------------------
    private static void Motion()
    {
        Heading("[4] animation");
        Run("literal 0:0:0.400", "<Border Width=\"40\" Height=\"40\" Background=\"#FFFF0000\" TransitionProperty=\"Background\" TransitionDuration=\"0:0:0.400\" />", null);

        var dictionary = new ResourceDictionary();
        dictionary["K"] = new Duration(TimeSpan.FromMilliseconds(400));
        Install(dictionary);
        Run("row 400ms via {ThemeResource}", "<Border Width=\"40\" Height=\"40\" Background=\"#FFFF0000\" TransitionProperty=\"Background\" TransitionDuration=\"{ThemeResource K}\" />", dictionary);
        dictionary["K"] = new Duration(TimeSpan.Zero);
        Run("same row flipped to 0", "<Border Width=\"40\" Height=\"40\" Background=\"#FFFF0000\" TransitionProperty=\"Background\" TransitionDuration=\"{ThemeResource K}\" />", dictionary);
        Uninstall(dictionary);
    }

    /// <summary>
    /// Paints red, asks for blue, and reads what the element is showing a handful of frames later. An element whose
    /// transition is running reports neither end; one with no transition reports the asked-for colour immediately.
    /// The DP read-back and the rendered ink are both taken so neither one alone carries the claim.
    /// </summary>
    private static void Run(string label, string elementMarkup, ResourceDictionary? dictionary)
    {
        try
        {
            var element = LoadElement(elementMarkup);
            if (element is not Border border)
            {
                Note(label, "element did not load");
                return;
            }

            Mount(border);
            var starting = ((SolidColorBrush)border.Background).Color;
            border.Background = new SolidColorBrush(EndColor);
            Pump(6);
            var animated = ((SolidColorBrush)border.GetValue(Border.BackgroundProperty)).Color;
            var local = ((SolidColorBrush)border.Background).Color;
            var buffer = Grab(border, 40, 40);
            var (midInk, startInk, endInk) = CountInk(buffer);
            Note(label, $"duration={Describe(border.TransitionDuration)}, DP color=#{animated.R:X2}{animated.G:X2}{animated.B:X2} " +
                $"(start #{starting.R:X2}{starting.G:X2}{starting.B:X2} asked #{EndColor.R:X2}{EndColor.G:X2}{EndColor.B:X2}), " +
                $"ink: start-like={startInk}, end-like={endInk}, between={midInk} of 1600");
            if (!Equals(animated, local))
            {
                Note(label, $"  the plain getter returned #{local.R:X2}{local.G:X2}{local.B:X2} while GetValue returned #{animated.R:X2}{animated.G:X2}{animated.B:X2}");
            }

            Unmount(border);
        }
        catch (Exception exception)
        {
            Note(label, "threw " + exception.GetType().Name + ": " + Trim(exception.Message));
        }
    }

    private static (int Between, int Start, int End) CountInk(byte[] buffer)
    {
        var between = 0;
        var start = 0;
        var end = 0;
        for (var index = 0; index < buffer.Length; index += 4)
        {
            var b = buffer[index];
            var g = buffer[index + 1];
            var r = buffer[index + 2];
            var redness = Math.Abs(r - StartColor.R) + Math.Abs(g - StartColor.G) + Math.Abs(b - StartColor.B);
            var blueness = Math.Abs(r - EndColor.R) + Math.Abs(g - EndColor.G) + Math.Abs(b - EndColor.B);
            if (redness <= 16) start++;
            else if (blueness <= 16) end++;
            else if (redness > 16 && blueness > 16) between++;
        }

        return (between, start, end);
    }

    // --- shared plumbing ----------------------------------------------------------------------------------
    private static string Describe(Duration duration) => duration.HasTimeSpan
        ? $"{duration.TimeSpan.TotalMilliseconds:0.###}ms"
        : duration.ToString() ?? "<unprintable>";

    private static ResourceDictionary? LoadResourceDictionary(string content)
    {
        var file = Path.Combine(AppContext.BaseDirectory, "probe-rows.jalxaml");
        File.WriteAllText(file, $"<ResourceDictionary {Root}>{content}</ResourceDictionary>", new UTF8Encoding(false));
        using var stream = File.OpenRead(file);
        return XamlReader.Load(stream) as ResourceDictionary;
    }

    private static FrameworkElement? LoadElement(string markup)
    {
        var file = Path.Combine(AppContext.BaseDirectory, "probe-element.jalxaml");
        File.WriteAllText(file, $"<Grid {Root}>{markup}</Grid>", new UTF8Encoding(false));
        using var stream = File.OpenRead(file);
        if (XamlReader.Load(stream) is not Grid grid || grid.Children.Count == 0)
        {
            return null;
        }

        var child = grid.Children[0];
        grid.Children.RemoveAt(0);
        return child as FrameworkElement;
    }

    private static void Install(ResourceDictionary dictionary) => _application.Resources.MergedDictionaries.Add(dictionary);

    private static void Uninstall(ResourceDictionary dictionary) => _application.Resources.MergedDictionaries.Remove(dictionary);

    private static (string Label, Duration Value) FindTarget(FrameworkElement element) => element switch
    {
        Border border => ("Border", border.TransitionDuration),
        Button button => FindInTemplate(button),
        _ => (element.GetType().Name, default),
    };

    private static (string Label, Duration Value) FindInTemplate(Button button)
    {
        var borders = new List<Border>();
        foreach (var (_, candidate) in Descendants(button))
        {
            if (candidate is Border border)
            {
                borders.Add(border);
            }
        }

        // The template here declares exactly one Border, so the count is part of the reading: a template that
        // instantiated to two would make "the first one" an arbitrary choice.
        return borders.Count switch
        {
            0 => ("<no Border under the template>", default),
            1 => ("Border in template", borders[0].TransitionDuration),
            _ => ($"{borders.Count} Borders in template, first", borders[0].TransitionDuration),
        };
    }

    private static List<(string Label, DependencyObject Element)> Descendants(DependencyObject root)
    {
        var list = new List<(string, DependencyObject)>();
        Walk(root, list);
        return list;
    }

    private static void Walk(DependencyObject visual, List<(string, DependencyObject)> into)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
        {
            var child = VisualTreeHelper.GetChild(visual, index);
            into.Add((child.GetType().Name, child));
            Walk(child, into);
        }
    }

    private static void Mount(FrameworkElement element)
    {
        _root.Children.Clear();
        _root.Children.Add(element);
        _root.UpdateLayout();
        Pump(6);
    }

    private static void Unmount(FrameworkElement element)
    {
        _root.Children.Remove(element);
        Pump(2);
    }

    private static byte[] Grab(Visual target, int width, int height)
    {
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(target);
        var stride = width * 4;
        var buffer = new byte[stride * height];
        bitmap.CopyPixels(new Int32Rect(0, 0, width, height), buffer, stride, 0);
        return buffer;
    }

    private static void Pump(int frames, int budgetMilliseconds = 800)
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

    private static void Heading(string title) => Note(title, string.Empty);

    private static void Note(string label, string text) => Lines.Add($"[{label}] {text}".TrimEnd());

    private static string Trim(string text) => text.Length <= 220 ? text : text[..220] + "...";
}
