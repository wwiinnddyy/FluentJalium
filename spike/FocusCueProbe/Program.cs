using System.Diagnostics;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;


namespace FocusCueProbe;

/// <summary>
/// Why a mouse click raises the same ring that Tab does, measured inside the process.
///
/// Every ring in Astra is a double Border inside the control template, resting at Opacity 0 and raised by a
/// trigger on IsKeyboardFocused (Styles/Common.jalxaml and its siblings). On this runtime a pointer click takes
/// keyboard focus, so that condition is true for both paths and the two are indistinguishable. WinUI gates on
/// FocusState instead, and the framework here appears to carry its own version of that gate
/// (Jalium.UI.Controls.FocusVisualManager / FocusVisualAdorner, exported in the 26.10.9 type inventory). This
/// probe asks four things, in that order, and prints raw readings for each:
///
///   api      - what is actually public on the shipping assembly (FocusVisualStyle, ShowFocusCues, AdornerLayer)
///   cue      - does ShowFocusCues flip on an in-process PreviewMouseDown / PreviewKeyDown(Tab), and what does
///              our template ring do at each step - the defect as one number per line
///   visual   - if a FocusVisualStyle is handed to a mounted control, does a focus-visual visual appear in the
///              tree, and does it follow the same gate
///   controls - the four of our own types that call Focus() from a pointer path, read the same way
///   product  - the same three readings on the shipped styles after the rings moved onto FocusVisualStyle:
///              does the setter arrive per family, does a programmatic Focus() draw anything, does Tab, does a
///              pointer press take it away, and what size/brush does the hosted ring really carry
///
/// No OS input is injected anywhere: the events are raised on the element, which is the only route this repo is
/// allowed to use for input claims (docs/astra/ROADMAP.md, #13).
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = new();

    [STAThread]
    private static int Main(string[] args)
    {
        var mode = args.Length > 0 ? args[0] : "api";
        ThemeLoader.Initialize();
        var application = new Application();
        FluentThemeManager.Apply(application, FluentThemeVariant.Light);

        var results = mode switch
        {
            "api" => Api(),
            "cue" => Cue(application),
            "visual" => Visual(application),
            "controls" => Controls(application),
            "product" => Product(application),
            _ => new[] { $"unknown mode {mode}" },
        };

        var path = Path.Combine(AppContext.BaseDirectory, $"focuscue-{mode}.txt");
        File.WriteAllLines(path, results);
        Console.WriteLine($"wrote {path} ({results.Length} lines)");
        foreach (var line in results)
        {
            Console.WriteLine(line);
        }

        return 0;
    }

    private static string[] Api()
    {
        Say("framework assembly: " + typeof(FrameworkElement).Assembly.GetName().Version);
        Say("controls assembly : " + typeof(Button).Assembly.GetName().Version);

        var fe = typeof(FrameworkElement);
        var styleProperty = fe.GetProperty("FocusVisualStyle", BindingFlags.Public | BindingFlags.Instance);
        Say($"FrameworkElement.FocusVisualStyle: get={styleProperty?.CanRead} set={styleProperty?.CanWrite} type={styleProperty?.PropertyType?.Name ?? "ABSENT"}");
        var dpField = fe.GetField("FocusVisualStyleProperty", BindingFlags.Public | BindingFlags.Static);
        Say($"FrameworkElement.FocusVisualStyleProperty: {(dpField is null ? "ABSENT" : "public, " + ((DependencyProperty)dpField.GetValue(null)!).Name + " owner=" + ((DependencyProperty)dpField.GetValue(null)!).OwnerType.Name)}");
        foreach (var name in new[] { "FocusVisualMargin", "FocusVisualPadding" })
        {
            var p = fe.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Say($"FrameworkElement.{name}: {(p is null ? "ABSENT" : p.PropertyType.Name)}");
        }

        var manager = Type.GetType("Jalium.UI.Controls.FocusVisualManager, Jalium.UI.Controls", throwOnError: false)
            ?? typeof(Button).Assembly.GetType("Jalium.UI.Controls.FocusVisualManager");
        if (manager is null)
        {
            Say("FocusVisualManager: ABSENT from the shipping assembly");
        }
        else
        {
            Say($"FocusVisualManager: {(manager.IsPublic ? "public" : "not public")} static={manager.IsAbstract}");
            foreach (var member in manager.GetMembers(BindingFlags.Public | BindingFlags.Static)
                         .OrderBy(m => m.Name, StringComparer.Ordinal))
            {
                Say($"  .{member.MemberType} {member.Name}");
            }
        }

        var layer = Type.GetType("Jalium.UI.Controls.AdornerLayer, Jalium.UI.Controls", throwOnError: false)
            ?? typeof(Button).Assembly.GetType("Jalium.UI.Controls.AdornerLayer");
        Say($"AdornerLayer: {(layer is null ? "ABSENT" : (layer.IsPublic ? "public" : "not public"))}");
        if (layer is not null)
        {
            foreach (var m in layer.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                         .Where(m => m.Name.Contains("Adorner", StringComparison.Ordinal) || m.Name.Contains("Children", StringComparison.Ordinal) || m.Name == "Add" || m.Name == "Remove")
                         .OrderBy(m => m.Name, StringComparer.Ordinal))
            {
                Say($"  {m.MemberType} {m.Name}");
            }
        }

        var adorner = typeof(Button).Assembly.GetType("Jalium.UI.Controls.FocusVisualAdorner");
        Say($"FocusVisualAdorner: {(adorner is null ? "ABSENT" : (adorner.IsPublic ? "public" : "not public") + " base=" + adorner!.BaseType!.Name)}");

        foreach (var name in new[] { "PreviewMouseDownEvent", "PreviewKeyDownEvent", "PreviewGotKeyboardFocusEvent", "GotKeyboardFocusEvent" })
        {
            var f = typeof(UIElement).GetField(name, BindingFlags.Public | BindingFlags.Static);
            Say($"UIElement.{name}: {(f is null ? "ABSENT" : ((RoutedEvent)f.GetValue(null)!).RoutingStrategy + " on " + ((RoutedEvent)f.GetValue(null)!).OwnerType.Name)}");
        }

        Say($"UIElement.IsKeyboardFocused: {typeof(UIElement).GetProperty("IsKeyboardFocused")?.PropertyType.Name ?? "ABSENT"}");
        Say($"UIElement.IsKeyboardFocusWithin: {typeof(UIElement).GetProperty("IsKeyboardFocusWithin")?.PropertyType.Name ?? "ABSENT"}");
        var inputMode = typeof(Button).Assembly.GetType("Jalium.UI.Input.InputMode") ?? Type.GetType("Jalium.UI.Input.InputMode, Jalium.UI.Input");
        Say($"InputMode: {(inputMode is null ? "ABSENT" : inputMode.FullName + " members=" + string.Join(",", Enum.GetNames(inputMode)))}");
        foreach (var t in new[] { "Jalium.UI.Input.Keyboard", "Jalium.UI.Input.FocusManager", "Jalium.UI.Input.KeyboardNavigation" })
        {
            var type = typeof(UIElement).Assembly.GetType(t) ?? typeof(Button).Assembly.GetType(t) ?? Type.GetType(t + ", Jalium.UI.Input");
            Say($"{t}: {(type is null ? "ABSENT (as a type)" : "present, public=" + type.IsPublic)}");
        }

        // Does the framework carry a default focus visual style that an app-level lookup can reach? The tree
        // ships Primitives.jalxaml's DefaultFocusVisualStyle, but 26.10.9 publishes no generic theme, so the
        // answer decides whether a style of ours must BasedOn something or start from scratch.
        Say($"TryFindResource(\"DefaultFocusVisualStyle\") = {Describe(Application.Current!.TryFindResource("DefaultFocusVisualStyle"))}");
        return Lines.ToArray();
    }

    private static string[] Cue(Application application)
    {
        var (window, button) = Show(application, new Button { Content = "cue", Width = 200, Height = 44 });
        Pump(12);
        var ring = Named(button, "FocusOutline");
        Say($"mounted: FocusVisualStyle={Describe(button.FocusVisualStyle)} ringPart={(ring is null ? "ABSENT - our template names no FocusOutline" : ring.GetType().Name)}");
        Read("rest", button, ring);

        button.Focus();
        Pump(6);
        Read("Focus()", button, ring);

        RaisePointer(button, window);
        Pump(6);
        Read("PreviewMouseDown (in-process)", button, ring);

        RaiseKey(button, window, Key.Tab);
        Pump(6);
        Read("PreviewKeyDown Tab (in-process)", button, ring);

        RaisePointer(button, window);
        Pump(6);
        Read("PreviewMouseDown again", button, ring);

        window.Close();
        Pump(4);
        return Lines.ToArray();
    }

    private static string[] Visual(Application application)
    {
        var style = new Style(typeof(Control));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0xFF))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(2)));

        var button = new Button { Content = "visual", Width = 200, Height = 44, FocusVisualStyle = style };
        var (window, mounted) = Show(application, button);
        Pump(12);
        Say($"FocusVisualStyle handed over: {Describe(mounted.FocusVisualStyle)} setters={style.Setters.Count}");
        Read("rest", mounted, null);
        Say($"focus-visual visuals in tree before focus: {CountFocusVisuals(window)}");

        RaiseKey(mounted, window, Key.Tab);
        mounted.Focus();
        Pump(10);
        Read("Tab + Focus()", mounted, null);
        Say($"focus-visual visuals in tree while gated on: {CountFocusVisuals(window)}");
        ReportRing(window, mounted);

        RaisePointer(mounted, window);
        Pump(10);
        Read("after a pointer press", mounted, null);
        Say($"focus-visual visuals in tree after the gate went off: {CountFocusVisuals(window)}");
        ReportRing(window, mounted);

        window.Close();
        Pump(4);
        return Lines.ToArray();
    }

    private static string[] Controls(Application application)
    {
        // The four types of ours that call Focus() from a pointer path. Each row is the same three numbers as
        // the Cue mode: keyboard focus, the framework's gate, and what our own ring part reports.
        var subjects = new (string Name, Func<Control> Make)[]
        {
            ("ToggleSwitch", () => new FluentToggleSwitch { Content = "t" }),
            ("PipsPager", () => new FluentPipsPager { NumberOfPages = 3 }),
            ("NavigationView", () => new FluentNavigationView()),
            ("RatingControl", () => new FluentRatingControl()),
        };

        foreach (var (name, make) in subjects)
        {
            Control subject;
            try
            {
                subject = make();
            }
            catch (Exception error)
            {
                Say($"{name}: could not construct - {error.GetType().Name}: {error.Message}");
                continue;
            }

            var (window, mounted) = Show(application, subject);
            Pump(10);
            Say($"{name}: FocusVisualStyle={Describe(mounted.FocusVisualStyle)}");
            Read($"{name} rest", mounted, null);
            mounted.Focus();
            Pump(6);
            Read($"{name} Focus()", mounted, null);
            RaisePointer(mounted, window);
            Pump(6);
            Read($"{name} + pointer", mounted, null);
            window.Close();
            Pump(4);
        }

        return Lines.ToArray();
    }

    private static string[] Product(Application application)
    {
        // The same three numbers as cue, now read on the shipped styles after the rings moved onto
        // FocusVisualStyle. One row per family that owns a ring: does the setter arrive, does a programmatic
        // Focus() draw anything, does Tab, and does a pointer press take it away again.
        var subjects = new (string Key, Func<Control> Make)[]
        {
            ("DefaultButtonStyle", () => new Button { Content = "b", Width = 200, Height = 44 }),
            ("DefaultDropDownButtonStyle", () => new FluentDropDownButton { Content = "d", Width = 200, Height = 44 }),
            ("SplitButtonStyle", () => new SplitButton { Content = "s", Width = 200, Height = 44 }),
            ("DefaultCheckBoxStyle", () => new CheckBox { Content = "c" }),
            ("DefaultRadioButtonStyle", () => new RadioButton { Content = "r" }),
            ("DefaultSliderStyle", () => new Slider { Width = 200 }),
            ("DefaultSliderStyle.vertical", () => new Slider { Height = 200, Orientation = Orientation.Vertical }),
            ("FluentToggleSwitchStyle", () => new FluentToggleSwitch { Content = "t" }),
            ("ExpanderStyle", () => new Expander { Header = "h", Width = 200 }),
            ("FluentNavigationItemStyle", () => new FluentNavigationItem { Content = "n" }),
            ("FluentNavigationPaneToggleButtonStyle", () => new Button { Content = "p", Width = 40, Height = 36 }),
            // An empty key means "hand over no style": these families never owned a ring part, so what they get
            // on the keyboard path is the framework's own DefaultFocusVisualStyle. Read it the same way, because
            // "the ring left the templates" is only a fair claim if it says what those controls now show.
            ("", () => new ComboBox { Width = 160 }),
            ("", () => new TextBox { Width = 160 }),
        };

        foreach (var (key, make) in subjects)
        {
            Control subject;
            try
            {
                subject = make();
                if (key.Length > 0)
                {
                    subject.Style = FluentThemeManager.GetStyle(key.Split('.')[0]);
                }
            }
            catch (Exception error)
            {
                Say($"{key}: could not construct - {error.GetType().Name}: {error.Message}");
                continue;
            }

            var (window, mounted) = Show(application, subject);
            Pump(10);
            Say(key.Length > 0 ? key : $"(nothing handed over: {mounted.GetType().Name})");
            var style = mounted.FocusVisualStyle;
            Say($"  FocusVisualStyle={Describe(style)} identity="
                + NameOf(style, "FocusVisualRingStyle") + "/"
                + NameOf(style, "FocusVisualCheckStyle") + "/"
                + NameOf(style, "FocusVisualSliderStyle") + "/"
                + NameOf(style, "FocusVisualSliderVerticalStyle"));
            Say($"  template parts named *Focus*: [{string.Join(",", RingParts(mounted))}] (empty means none)");

            mounted.Focus();
            Pump(6);
            Read("  Focus()", mounted, null);
            Say($"  focus-visual visuals while Focus() holds: {CountFocusVisuals(window)}");

            RaiseKey(mounted, window, Key.Tab);
            Pump(8);
            Read("  after Tab", mounted, null);
            Say($"  focus-visual visuals after Tab: {CountFocusVisuals(window)}");
            ReportRing(window, mounted);
            // Two families read 0x0 above. Either the adorner really is that size for them, or it was measured
            // before the subject had a render size and a later pass fixes it - which a live window always runs.
            window.UpdateLayout();
            Pump(8);
            ReportRing(window, mounted);

            RaisePointer(mounted, window);
            Pump(8);
            Read("  after pointer press", mounted, null);
            Say($"  focus-visual visuals after the pointer press: {CountFocusVisuals(window)}");
            ReportRing(window, mounted);

            window.Close();
            Pump(4);
        }

        return Lines.ToArray();
    }

    private static string NameOf(Style? candidate, string key)
        => ReferenceEquals(candidate, FluentThemeManager.GetStyle(key)) ? key : "no";

    private static string[] RingParts(Control subject)
    {
        var names = new List<string>();
        try
        {
            subject.ApplyTemplate();
        }
        catch (Exception error)
        {
            return new[] { "ApplyTemplate threw " + error.GetType().Name };
        }

        void Recurse(DependencyObject node)
        {
            if (node is FrameworkElement element && element.Name is { Length: > 0 } name
                && name.Contains("Focus", StringComparison.Ordinal))
            {
                names.Add(name);
            }

            var count = VisualTreeHelper.GetChildrenCount(node);
            for (var i = 0; i < count; i++)
            {
                Recurse(VisualTreeHelper.GetChild(node, i));
            }
        }

        Recurse(subject);
        return names.ToArray();
    }

    private static void Read(string label, Control subject, FrameworkElement? ring)
    {
        var gate = Gate();
        var opacity = ring switch
        {
            null => "(no ring part read)",
            UIElement element => element.Opacity.ToString(),
        };
        Say($"{label,-26}: IsKeyboardFocused={subject.IsKeyboardFocused} IsKeyboardFocusWithin={subject.IsKeyboardFocusWithin} ShowFocusCues={gate} ourRingOpacity={opacity}");
    }

    private static string Gate()
    {
        var manager = typeof(Button).Assembly.GetType("Jalium.UI.Controls.FocusVisualManager");
        var property = manager?.GetProperty("ShowFocusCues", BindingFlags.Public | BindingFlags.Static);
        if (property is null)
        {
            return "no public ShowFocusCues";
        }

        try
        {
            return property.GetValue(null)?.ToString() ?? "null";
        }
        catch (Exception error)
        {
            return "threw " + error.GetType().Name;
        }
    }

    private static void ReportRing(Window window, FrameworkElement subject)
    {
        // The per-family ring offsets today live as Margin on a Border inside the template (-2,1 for the
        // check/radio boxes, 0,2 and 2,0 for the two Slider layouts, 1 elsewhere). A FocusVisualStyle has to
        // reproduce those numbers from the adorner side, so print what the hosted ring actually measures.
        var founds = new List<FrameworkElement>();
        void Recurse(DependencyObject node)
        {
            if (node is FrameworkElement element
                && element.GetType().FullName!.Contains("FocusVisual", StringComparison.Ordinal)
                && !ReferenceEquals(element, subject))
            {
                founds.Add(element);
            }

            var count = VisualTreeHelper.GetChildrenCount(node);
            for (var i = 0; i < count; i++)
            {
                Recurse(VisualTreeHelper.GetChild(node, i));
            }
        }

        Recurse(window);
        if (founds.Count == 0)
        {
            Say("  ring: no focus-visual element in the tree");
            return;
        }

        foreach (var found in founds)
        {
            var parentChain = new List<string>();
            for (DependencyObject? node = found; node is not null && parentChain.Count < 4; node = VisualTreeHelper.GetParent(node))
            {
                parentChain.Add(node.GetType().Name);
            }

            Say($"  ring: {found.GetType().Name} margin={found.Margin} size={found.ActualWidth}x{found.ActualHeight} "
                + $"desired={found.DesiredSize} rendered={found.RenderSize} visible={found.IsVisible} "
                + $"subject={subject.ActualWidth}x{subject.ActualHeight} parents=[{string.Join(" < ", parentChain)}]"
                + (found.GetType().Name.Contains("Adorner", StringComparison.Ordinal) ? $" adornee={AdorneeOf(found)}" : ""));

            // The identity check: the stroke that reached the adorner has to be the palette instance, not a copy.
            Border? outer = null;
            void FindBorder(DependencyObject node)
            {
                if (outer is not null)
                {
                    return;
                }

                if (node is Border border)
                {
                    outer = border;
                    return;
                }

                var count = VisualTreeHelper.GetChildrenCount(node);
                for (var i = 0; i < count && outer is null; i++)
                {
                    FindBorder(VisualTreeHelper.GetChild(node, i));
                }
            }

            FindBorder(found);
            if (outer is null)
            {
                Say("    no Border under the focus visual");
                continue;
            }

            var palette = FluentThemeManager.GetBrush("FocusStrokeColorOuterBrush");
            var inner = FindInner(outer);
            Say($"    Border thickness={outer.BorderThickness} radius={outer.CornerRadius} margin={outer.Margin} "
                + $"stroke={outer.BorderBrush?.GetType().Name} sameAsPalette={ReferenceEquals(outer.BorderBrush, palette)} "
                + $"inner={(inner is null ? "none" : $"thickness={inner.BorderThickness} radius={inner.CornerRadius} sameAsInnerPalette={ReferenceEquals(inner.BorderBrush, FluentThemeManager.GetBrush("FocusStrokeColorInnerBrush"))}")}");
        }
    }

    private static string AdorneeOf(DependencyObject adorner)
    {
        for (var type = adorner.GetType(); type is not null; type = type.BaseType)
        {
            var property = type.GetProperty("AdornedElement", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (property is not null)
            {
                try
                {
                    return property.GetValue(adorner) is FrameworkElement element
                        ? $"{element.GetType().Name} actual={element.ActualWidth}x{element.ActualHeight} rendered={element.RenderSize}"
                        : "null";
                }
                catch (Exception error)
                {
                    return "threw " + error.GetType().Name;
                }
            }

            var field = type.GetField("AdornedElement", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (field is not null)
            {
                return field.GetValue(adorner) is FrameworkElement element
                    ? $"{element.GetType().Name} actual={element.ActualWidth}x{element.ActualHeight} rendered={element.RenderSize}"
                    : "null";
            }
        }

        return "no AdornedElement member";
    }

    private static Border? FindInner(Border outer)
    {
        var count = VisualTreeHelper.GetChildrenCount(outer);
        for (var i = 0; i < count; i++)
        {
            if (VisualTreeHelper.GetChild(outer, i) is Border border)
            {
                return border;
            }
        }

        return null;
    }

    private static int CountFocusVisuals(Window window)
    {
        var count = 0;
        Walk(window, visual =>
        {
            if (visual.GetType().FullName?.Contains("FocusVisual", StringComparison.Ordinal) == true)
            {
                count++;
            }
        });
        return count;
    }

    private static void Walk(DependencyObject root, Action<Visual> visit)
    {
        if (root is not Visual visual)
        {
            return;
        }

        visit(visual);
        var children = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < children; i++)
        {
            Walk(VisualTreeHelper.GetChild(root, i), visit);
        }
    }

    private static void RaisePointer(UIElement target, Window window)
    {
        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        {
            RoutedEvent = (RoutedEvent)typeof(UIElement)
                .GetField("PreviewMouseDownEvent", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!,
        };
        target.RaiseEvent(args);
    }

    private static void RaiseKey(UIElement target, Window window, Key key)
    {
        // The public KeyEventArgs constructor wants a PresentationSource, and this runtime publishes no way to
        // obtain one from a mounted element. The framework's own input path builds keyboard events through the
        // internal (RoutedEvent, Key, ModifierKeys, isDown, isRepeat, timestamp) constructor instead, so the probe
        // calls exactly that one - the same object the real pipeline would raise. Diagnostic only: no product code
        // reaches into the framework this way.
        var routed = (RoutedEvent)typeof(UIElement)
            .GetField("PreviewKeyDownEvent", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
        var ctor = typeof(KeyEventArgs).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(c => c.GetParameters().Length == 6 && c.GetParameters()[0].ParameterType == typeof(RoutedEvent));
        if (ctor is null)
        {
            Say($"  (no framework KeyEventArgs constructor found, so {key} was never raised)");
            return;
        }

        var args = (KeyEventArgs)ctor.Invoke(new object[] { routed, key, ModifierKeys.None, true, false, 0 });
        target.RaiseEvent(args);
    }

    private static int Pump(int frames)
    {
        var frame = new DispatcherFrame();
        var seen = 0;
        var deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 4;
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames || Stopwatch.GetTimestamp() > deadline)
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

    private static (Window, Control) Show(Application application, Control subject)
    {
        var canvas = new Canvas();
        canvas.Children.Add(subject);
        Canvas.SetLeft(subject, 40);
        Canvas.SetTop(subject, 40);
        var window = new Window
        {
            Title = "FocusCueProbe",
            Content = canvas,
            Width = 420,
            Height = 320,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = 520,
            Top = 220,
        };
        window.Show();
        Pump(12);
        window.UpdateLayout();
        return (window, subject);
    }

    private static FrameworkElement? Named(DependencyObject root, string name)
    {
        FrameworkElement? found = null;
        void Recurse(DependencyObject node)
        {
            if (found is not null)
            {
                return;
            }

            if (node is FrameworkElement element && element.Name == name)
            {
                found = element;
                return;
            }

            var count = VisualTreeHelper.GetChildrenCount(node);
            for (var i = 0; i < count && found is null; i++)
            {
                Recurse(VisualTreeHelper.GetChild(node, i));
            }
        }

        Recurse(root);
        return found;
    }

    private static string Describe(object? value) => value switch
    {
        null => "null",
        Style style => $"Style(TargetType={style.TargetType?.Name ?? "?"}, setters={style.Setters.Count}, basedOn={style.BasedOn?.TargetType?.Name ?? "none"})",
        _ => value.GetType().Name + " " + value,
    };

    private static void Say(string text) => Lines.Add(text);
}
