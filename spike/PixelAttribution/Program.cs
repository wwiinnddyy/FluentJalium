using System.Diagnostics;
using System.Reflection;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;
using Control = Jalium.UI.Controls.Control;

namespace PixelAttribution;

/// <summary>
/// Decides whether Astra's styles reach rendered pixels, or whether every "no change" reading so far
/// was a capture artifact. Runs inside a real Application whose loop is pumping, because the test
/// harness shows a window but never lets the dispatcher render a frame.
/// <c>dump</c> prints the capture-relevant framework surface, which is the part the earlier probes
/// never exercised.
/// </summary>
internal static class Program
{
    /// <summary>The window surface colour. Anything neither this nor transparent came from the probe.</summary>
    private static readonly Color Root = Color.FromRgb(0x0A, 0x14, 0x0A);

    private static readonly Color Magenta = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color Lime = Color.FromRgb(0x00, 0xFF, 0x00);
    private static readonly Color Orange = Color.FromRgb(0xFF, 0xA5, 0x00);
    private static readonly Color Yellow = Color.FromRgb(0xFF, 0xFF, 0x00);

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "dump") return Dump.Run();
        var noLoop = args.Length > 0 && args[0] == "nofun";
        if (args.Length > 0 && args[0] == "force") Probe.Forced = true;
        if (args.Length > 0 && args[0] == "scrollkeys") Probe.ScrollKeys = true;
        if (args.Length > 0 && args[0] == "parts") Probe.PartsMode = true;

        RenderContext.GetOrCreateCurrent(RenderBackend.Auto).DefaultRenderingEngine = RenderingEngine.Impeller;
        ThemeLoader.Initialize();

        var application = new Application();
        FluentThemeManager.Apply(application, FluentThemeVariant.Light);

        var window = new Window
        {
            Title = "PixelAttribution",
            Width = 420,
            Height = 260,
            Background = new SolidColorBrush(Root),
            Content = new Border { Background = new SolidColorBrush(Root) },
        };

        var entered = false;
        window.ContentRendered += (_, _) =>
        {
            if (entered) return;
            entered = true;
            try
            {
                Probe.Run(application, window);
            }
            catch (Exception exception)
            {
                Console.WriteLine("!! " + exception);
            }
            finally
            {
                Console.Out.Flush();
                window.Close();
                application.Shutdown();
            }
        };

        if (noLoop)
        {
            // Mirrors the test fixture: a shown window and a nested dispatcher frame, but the
            // application's own loop never runs. If frames do not arrive here, pixel assertions
            // cannot live in the fixture.
            window.Show();
            try
            {
                Probe.Run(application, window);
            }
            finally
            {
                Console.Out.Flush();
                window.Close();
            }
            return 0;
        }

        window.Show();
        return application.Run();
    }

    /// <summary>Eight attribution cases, four capture timings each, under both themes.</summary>
    private static class Probe
    {
        internal static bool Forced;
        internal static bool ScrollKeys;
        internal static bool PartsMode;

        internal static void Run(Application application, Window window)
        {
            if (PartsMode)
            {
                Parts(application, window);
                return;
            }
            Note($"env dpiScale={window.DpiScale} window={window.ActualWidth}x{window.ActualHeight} theme={FluentThemeManager.NativeThemeMode}");
            Note($"lookup app[typeof(Button)] = {Describe(Lookup(application.Resources, typeof(Button)))}");
            Note($"lookup app[\"Button\"] = {Describe(Lookup(application.Resources, "Button"))}");
            Note($"lookup app[\"DefaultButtonStyle\"] = {Describe(Lookup(application.Resources, "DefaultButtonStyle"))}");

            if (ScrollKeys)
            {
                Hooks = application.Resources;
                // The palette token overrides are skipped here: the only subject is a scroll viewer,
                // and an override that itself reaches the scrollbar would be read as a hook hit.
                foreach (var theme in new[] { FluentThemeVariant.Light })
                {
                    FluentThemeManager.ApplyTheme(theme);
                    foreach (var (name, build) in ScrollBarCases()) RunCase(window, name, theme, build);
                }
                return;
            }

            // These are colours no framework default ever paints, so one pixel of them proves the
            // token was consumed by whichever template rendered.
            FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", Yellow);
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", Lime);

            foreach (var theme in new[] { FluentThemeVariant.Light, FluentThemeVariant.Dark })
            {
                FluentThemeManager.ApplyTheme(theme);
                foreach (var (name, build) in Cases()) RunCase(window, name, theme, build);
            }

            FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", null);
            FluentThemeManager.OverrideBrush("AccentFillColorDefaultBrush", null);
        }

        private static ResourceDictionary? Hooks;

        private static void RunCase(Window window, string name, FluentThemeVariant theme, Func<FrameworkElement> build)
        {
            try
            {
                var element = build();
                var root = new Grid { Background = new SolidColorBrush(Root) };
                root.Children.Add(element);
                window.Content = root;
                element.Width = 200;
                element.Height = 44;
                if (element is Control control) control.ApplyTemplate();
                window.UpdateLayout();

                for (var stage = 0; stage < 4; stage++)
                {
                    if (stage > 0)
                    {
                        if (Forced) ForceFrames(window, 1 << (stage + 1));
                        else Pump(1 << (stage + 1));
                    }
                    Capture(window, element, name, theme, stage);
                }

                var styled = element as Button ?? (element as Grid)?.Children.OfType<Button>().FirstOrDefault();
                Note($"meta {name}/{theme}: style={Describe(styled?.Style?.TargetType)} template={Describe(styled?.Template)} loaded={styled?.IsLoaded} bounds={styled?.ActualWidth}x{styled?.ActualHeight}");
            }
            catch (Exception exception)
            {
                Note($"case {name}/{theme} threw {exception.GetType().Name}: {exception.Message}");
            }
        }

        /// <summary>
        /// One case per candidate ScrollBar hook, with all the other hooks removed for that case.
        /// Installing all five at once produced cyan 144 px plus lime 64 px and no way to say which
        /// name owned the 144, so each reading here has exactly one candidate in the tree.
        /// </summary>
        private static IEnumerable<(string Name, Func<FrameworkElement> Build)> ScrollBarCases()
        {
            var candidates = new (string Name, Action<ResourceDictionary> Install)[]
            {
                ("h0-baseline", _ => { }),
                ("h1-style-background", r => r["ScrollBarStyle"] = ScrollBarStyle((Control.BackgroundProperty, Brush("#FF00FFFF")))),
                ("h2-style-foreground", r => r["ScrollBarStyle"] = ScrollBarStyle((Control.ForegroundProperty, Brush("#FF00FF00")))),
                ("h3-style-border", r => r["ScrollBarStyle"] = ScrollBarStyle((Control.BorderBrushProperty, Brush("#FFFF00FF")))),
                ("h4-style-thumbstyle", r => r["ScrollBarStyle"] = ScrollBarStyle((ScrollBar.ThumbStyleProperty, ScrollBarStyle((Control.BackgroundProperty, Brush("#FFFF0000")))))),
                ("h5-track", r => r["ScrollBarTrack"] = Brush("#FFFFFF00")),
                ("h6-thumb", r => r["ScrollBarThumb"] = Brush("#FFFFA500")),
                ("h7-arrow", r => r["ScrollBarArrow"] = Brush("#FFFF1493")),
                ("h8-implicit-template", r => r[typeof(ScrollBar)] = StyleWithTemplate(typeof(ScrollBar), "#FF0000FF")),
                ("h9-implicit-background", r => r[typeof(ScrollBar)] = ScrollBarStyle((Control.BackgroundProperty, Brush("#FF800080")))),
            };

            foreach (var (name, install) in candidates)
                yield return (name, () =>
                {
                    var hooks = Hooks!;
                    foreach (var key in new object[] { typeof(ScrollBar), "ScrollBarStyle", "ScrollBarTrack", "ScrollBarThumb", "ScrollBarArrow" })
                        hooks.Remove(key);
                    install(hooks);
                    return new ScrollViewer
                    {
                        VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                        Content = new Border { Height = 800, Background = new SolidColorBrush(Root) },
                    };
                });
        }

        private static Style ScrollBarStyle(params (DependencyProperty Property, object Value)[] setters)
        {
            var style = new Style(typeof(ScrollBar));
            foreach (var (property, value) in setters) style.Setters.Add(new Setter(property, value));
            return style;
        }

        private static IEnumerable<(string Name, Func<FrameworkElement> Build)> Cases()
        {
            yield return ("1-default", () => new Button { Content = "native" });
            yield return ("2-appimplicit", () => new Button { Content = "astra implicit" });
            yield return ("3-explicit-astra", () => new Button { Content = "astra explicit", Style = FluentThemeManager.GetStyle("AccentButtonStyle") });
            yield return ("4-explicit-local", () => new Button { Content = "local style", Style = TemplateStyle("MagentaStyle") });
            yield return ("5-implicit-local", () =>
            {
                var grid = new Grid();
                grid.Resources[typeof(Button)] = TemplateStyle("CyanStyle");
                grid.Children.Add(new Button { Content = "implicit local" });
                return grid;
            });
            yield return ("6-local-background", () => new Button { Content = "local bg", Background = new SolidColorBrush(Orange) });
            yield return ("7-token-border", () => new Border { Background = FluentThemeManager.GetBrush("SolidBackgroundFillColorBaseBrush") });
            yield return ("8-plain-border", () => new Border { Background = new SolidColorBrush(Magenta) });
        }

        /// <summary>
        /// Built from markup rather than FrameworkElementFactory because that is the same route Astra's
        /// own dictionaries take; the code-side factory API is not part of what this probe is about.
        /// </summary>
        private static readonly ResourceDictionary Local = (ResourceDictionary)XamlReader.Parse("""
            <ResourceDictionary xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
              <Style x:Key="MagentaStyle" TargetType="Button">
                <Setter Property="Template"><ControlTemplate TargetType="Button"><Border Background="#FFFF00FF" /></ControlTemplate></Setter>
              </Style>
              <Style x:Key="CyanStyle" TargetType="Button">
                <Setter Property="Template"><ControlTemplate TargetType="Button"><Border Background="#FF00FFFF" /></ControlTemplate></Setter>
              </Style>
            </ResourceDictionary>
            """)!;

        private static Style TemplateStyle(string key) => (Style)Local[key]!;

        private static SolidColorBrush Brush(string value) => (SolidColorBrush)new BrushConverter().ConvertFromString(value)!;

        private static Style StyleWithTemplate(Type target, string color)
        {
            var style = new Style(target);
            style.Setters.Add(new Setter(Control.TemplateProperty, (ControlTemplate)XamlReader.Parse(
                $"<ControlTemplate xmlns='http://schemas.jalium.ui/2024' TargetType='{target.Name}'>" +
                $"<Border Background='{color}' /></ControlTemplate>")!));
            return style;
        }

        /// <summary>
        /// The scroll bar turned out to have a real visual tree (two RepeatButtons, a Track holding a
        /// Thumb, Borders and Paths), so "self-drawn" was the wrong reading. This asks the question the
        /// ScrollBar batch now hinges on: which of those parts can a resource from our dictionaries
        /// reach - the named ScrollBarStyle's Template, or an implicit style on the part types?
        /// </summary>
        internal static void Parts(Application application, Window window)
        {
            Hooks = application.Resources;
            var cases = new (string Name, Action<ResourceDictionary> Install)[]
            {
                ("t0-baseline", _ => { }),
                ("t1-named-style-template", r => r["ScrollBarStyle"] = ScrollBarStyle((Control.TemplateProperty, BarTemplate()))),
                ("t2-named-style-background", r => r["ScrollBarStyle"] = ScrollBarStyle((Control.BackgroundProperty, Brush("#FF00FFFF")))),
                ("t3-implicit-scrollbar-template", r => r[typeof(ScrollBar)] = StyleWithTemplate(typeof(ScrollBar), "#FFFF00FF")),
                ("t4-implicit-repeatbutton", r => r[typeof(RepeatButton)] = StyleWithTemplate(typeof(RepeatButton), "#FF0000FF")),
                ("t5-implicit-thumb", r => r[typeof(Thumb)] = StyleWithTemplate(typeof(Thumb), "#FFFF0000")),
                ("t6-thumb-hook", r =>
                {
                    FluentThemeManager.OverrideBrush("ScrollBarThumb", Magenta);
                    FluentThemeManager.OverrideBrush("ScrollBarTrack", Lime);
                }),
                // If the scroll host takes an implicit style, a template of our own is still on the
                // table for the states the scroll bar itself cannot be given.
                ("t7-implicit-scrollviewer-template", r => r[typeof(ScrollViewer)] = StyleWithTemplate(typeof(ScrollViewer), "#FF0000FF")),
            };

            foreach (var (name, install) in cases)
            {
                ClearHooks(Hooks);
                FluentThemeManager.OverrideBrush("ScrollBarThumb", null);
                FluentThemeManager.OverrideBrush("ScrollBarTrack", null);
                install(Hooks);

                var scroller = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                    Content = new Border { Height = 800, Background = new SolidColorBrush(Root) },
                };
                var root = new Grid { Background = new SolidColorBrush(Root) };
                root.Children.Add(scroller);
                window.Content = root;
                scroller.Width = 200;
                scroller.Height = 44;
                window.UpdateLayout();
                Pump(16);
                Note($"case {name}");
                DumpTree(scroller, 1);
                foreach (var bar in Descendants(scroller).OfType<ScrollBar>()) Note($"  bar {bar.ActualWidth}x{bar.ActualHeight} {CaptureOf(bar)}");
                foreach (var thumb in Descendants(scroller).OfType<Thumb>()) Note($"  thumb {thumb.ActualWidth}x{thumb.ActualHeight} {CaptureOf(thumb)}");
                foreach (var button in Descendants(scroller).OfType<RepeatButton>()) Note($"  repeat {button.ActualWidth}x{button.ActualHeight} {CaptureOf(button)}");
            }
        }

        private static readonly object[] HookSlots = [typeof(ScrollBar), typeof(RepeatButton), typeof(Thumb), "ScrollBarStyle", "ScrollBarTrack", "ScrollBarThumb", "ScrollBarArrow"];

        private static void ClearHooks(ResourceDictionary hooks)
        {
            foreach (var key in HookSlots) hooks.Remove(key);
        }

        private static ControlTemplate BarTemplate() => (ControlTemplate)XamlReader.Parse(
            "<ControlTemplate xmlns='http://schemas.jalium.ui/2024' TargetType='ScrollBar'>" +
            "<Border Background='#FFFF00FF' /></ControlTemplate>")!;


        private static void DumpTree(Visual visual, int depth)
        {
            var text = new string(' ', depth * 2);
            var bounds = visual is FrameworkElement element ? $"{element.ActualWidth}x{element.ActualHeight}" : "";
            Note($"tree {text}{visual.GetType().Name} {bounds}");
            foreach (var child in Children(visual)) DumpTree(child, depth + 1);
        }

        private static IEnumerable<Visual> Children(Visual visual)
        {
            var count = VisualTreeHelper.GetChildrenCount(visual);
            for (var index = 0; index < count; index++)
                yield return (Visual)VisualTreeHelper.GetChild(visual, index);
        }

        private static IEnumerable<Visual> Descendants(Visual visual)
        {
            foreach (var child in Children(visual))
            {
                yield return child;
                foreach (var deeper in Descendants(child)) yield return deeper;
            }
        }

        private static string CaptureOf(Visual visual) => visual is FrameworkElement { ActualWidth: > 0, ActualHeight: > 0 } element
            ? CaptureRaw(element, (int)element.ActualWidth, (int)element.ActualHeight)
            : "no-bounds";

        private static void ForceFrames(Window window, int count)
        {
            for (var index = 0; index < count; index++) window.ForceRenderFrame();
        }

        private static void Pump(int frames)
        {
            var frame = new DispatcherFrame();
            var seen = 0;
            var deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 6;
            void OnRendering(object? sender, EventArgs arguments)
            {
                seen++;
                if (seen >= frames || Stopwatch.GetTimestamp() > deadline) frame.Continue = false;
            }

            EventHandler handler = OnRendering;
            CompositionTarget.Rendering += handler;
            using var watchdog = new System.Threading.Timer(_ => Dispatcher.CurrentDispatcher.InvokeAsync(() => frame.Continue = false));
            watchdog.Change(TimeSpan.FromSeconds(9), System.Threading.Timeout.InfiniteTimeSpan);
            Dispatcher.PushFrame(frame);
            CompositionTarget.Rendering -= handler;
            Console.WriteLine($"pumped {seen}/{frames}");
        }

        private static void Capture(Window window, FrameworkElement element, string name, FluentThemeVariant theme, int stage)
        {
            var scale = window.DpiScale == 0 ? 1 : window.DpiScale;
            var windowSample = CaptureRaw(window, (int)Math.Round(window.ActualWidth * scale), (int)Math.Round(window.ActualHeight * scale));
            var elementSample = CaptureRaw(element, (int)Math.Round(element.ActualWidth * scale), (int)Math.Round(element.ActualHeight * scale));
            Note($"px {name}/{theme}/s{stage} win={windowSample} | self={elementSample}");
        }

        private static string CaptureRaw(Visual target, int width, int height)
        {
            if (width <= 0 || height <= 0) return "-";
            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
            bitmap.Render(target);
            var stride = width * 4;
            var buffer = new byte[stride * height];
            bitmap.CopyPixels(new Int32Rect(0, 0, width, height), buffer, stride, 0);

            var histogram = new Dictionary<uint, int>();
            for (var index = 0; index + 3 < buffer.Length; index += 4)
            {
                var key = (uint)(buffer[3] << 24 | buffer[index + 2] << 16 | buffer[index + 1] << 8 | buffer[index]);
                histogram[key] = histogram.GetValueOrDefault(key) + 1;
            }

            return $"{bitmap.PixelWidth}x{bitmap.PixelHeight} " + string.Join(" ", histogram.OrderByDescending(static entry => entry.Value)
                .Take(5).Select(entry => $"#{entry.Key:X8}x{entry.Value}"));
        }

        private static object? Lookup(ResourceDictionary dictionary, object key)
        {
            try { return dictionary.Contains(key) ? dictionary[key] : null; }
            catch (Exception exception) { return exception.GetType().Name; }
        }

        private static string Describe(object? value) => value switch
        {
            null => "null",
            Style style => $"Style({style.TargetType?.Name})",
            ControlTemplate template => $"ControlTemplate({template.TargetType?.Name})",
            _ => value.GetType().Name,
        };

        private static void Note(string line) => Console.WriteLine(line);
    }
}

internal static class Dump
{
    private static readonly string[] TypeNames =
    [
        "CompositionTarget", "Visual", "UIElement", "FrameworkElement", "ResourceDictionary",
        "Window", "Application", "RenderTargetBitmap", "Dispatcher", "Style", "ControlTemplate",
        "FrameworkElementFactory", "RenderContext", "Button", "ContentControl", "Control",
        "DispatcherFrame", "Setter", "SetterBase", "Border", "Canvas", "Grid", "TextBlock",
        "BasedOnStyleCollection", "Condition", "Trigger", "MultiTrigger", "FrameworkTemplate",
        "ScrollBar", "ScrollViewer", "Thumb", "Primitives", "RangeBase", "ScrollChangedEventArgs",
    ];

    private static readonly string[] Ungated = ["ScrollBar", "ScrollViewer", "Thumb", "DispatcherFrame", "FrameworkElementFactory", "Setter", "SetterBase", "Trigger", "Style", "CompositionTarget", "RenderTargetBitmap", "Window"];

    private static readonly string[] MemberWords =
    [
        "Render", "Transform", "Point", "Actual", "Frame", "Tick", "Opacity", "Capture", "Bitmap",
        "Priority", "Invoke", "Begin", "Content", "Resources", "Theme", "Style", "Template",
        "Width", "Height", "Show", "Close", "Run", "Shutdown", "Size", "Scale", "Dpi", "Bounds",
        "Parent", "Child", "Apply", "Measure", "Arrange", "Update", "Background", "IsLoaded",
    ];

    internal static int Run()
    {
        _ = typeof(Jalium.UI.Application);
        _ = typeof(Jalium.UI.Controls.Button);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(static a => a.GetName().Name?.StartsWith("Jalium.UI", StringComparison.Ordinal) == true)
            .ToList();
        Console.WriteLine($"# assemblies: {string.Join(", ", assemblies.Select(static a => a.GetName().Name))}");

        foreach (var assembly in assemblies)
        {
            Type[] types;
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException exception) { types = exception.Types.OfType<Type>().ToArray(); }

            foreach (var type in types.Where(t => Array.IndexOf(TypeNames, t.Name) >= 0).OrderBy(static t => t.FullName, StringComparer.Ordinal))
            {
                Console.WriteLine($"\n## {type.FullName} : {type.BaseType?.Name}  [{assembly.GetName().Name}]");
                foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    if (member.Name.StartsWith("get_", StringComparison.Ordinal) || member.Name.StartsWith("set_", StringComparison.Ordinal)) continue;
                    if (Array.IndexOf(Ungated, type.Name) < 0 && !MemberWords.Any(word => member.Name.Contains(word, StringComparison.OrdinalIgnoreCase))) continue;
                    Console.WriteLine($"  {Describe(member)}");
                }
            }
        }

        return 0;
    }

    private static string Describe(MemberInfo member) => member switch
    {
        MethodInfo method => $"M {method.Name}({string.Join(", ", method.GetParameters().Select(static p => p.ParameterType.Name + " " + p.Name))}) : {method.ReturnType.Name}",
        PropertyInfo property => $"P {property.PropertyType.Name} {property.Name} {{{(property.CanRead ? " get" : "")}{(property.CanWrite ? " set" : "")} }}",
        FieldInfo field => $"F {field.FieldType.Name} {field.Name}",
        EventInfo evt => $"E {evt.Name} : {evt.EventHandlerType?.Name}",
        ConstructorInfo ctor => $"C ctor({string.Join(", ", ctor.GetParameters().Select(static p => p.ParameterType.Name))})",
        _ => member.MemberType + " " + member.Name,
    };
}
