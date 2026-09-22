using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

namespace PointerProbe;

/// <summary>
/// Does an OS-level cursor move reach the framework's hover state, and does the hover token then
/// reach the pixels? Injecting through user32 keeps the path honest: the framework sees a real
/// WM_MOUSEMOVE over a real window, not a property somebody set. Defaults to hover only; passing
/// "press" additionally sends a real left-button click, which moves the user's cursor and clicks on
/// whatever is under it, so it is never on the default path.
/// </summary>
internal static class Program
{
    private static readonly Color Rest = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color Hover = Color.FromRgb(0x00, 0xFF, 0x00);
    private static readonly Color Pressed = Color.FromRgb(0xFF, 0xA5, 0x00);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out PointWin32 point);

    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(IntPtr window, ref PointWin32 point);

    [StructLayout(LayoutKind.Sequential)]
    private struct PointWin32
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint type;
        public MouseInput mouse;
        public ulong extra;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint count, Input[] inputs, int size);

    private const uint InputMouse = 0;
    private const uint LeftDown = 0x0002;
    private const uint LeftUp = 0x0004;
    private const uint Absolute = 0x8000;
    private const uint VirtualDesktop = 0x4000;

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "swap") return Swap();

        var withPress = args.Length > 0 && args[0] == "press";
        RenderContext.GetOrCreateCurrent(RenderBackend.Auto).DefaultRenderingEngine = RenderingEngine.Impeller;
        ThemeLoader.Initialize();

        var application = new Application();
        FluentThemeManager.Apply(application, FluentThemeVariant.Light);
        if (args.Length > 0 && args[0] == "surface") return Surface();
        if (args.Length > 0 && args[0] == "route") return Route();
        if (args.Length > 0 && args[0] == "synth") return Synth();
        // Three unmistakable colours: the claim here is that the state transition repaints, and a
        // 6/255 difference in the real palette would not prove that from a histogram.
        FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", Rest);
        FluentThemeManager.OverrideBrush("ControlFillColorSecondaryBrush", Hover);
        FluentThemeManager.OverrideBrush("ControlFillColorTertiaryBrush", Pressed);

        var button = new Button { Content = "pointer", Width = 200, Height = 44 };
        var canvas = new Canvas();
        canvas.Children.Add(button);
        Canvas.SetLeft(button, 100);
        Canvas.SetTop(button, 140);

        var window = new Window
        {
            Title = "PointerProbe",
            Content = canvas,
            Width = 420,
            Height = 320,
            Topmost = true,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = 500,
            Top = 200,
        };
        window.Show();
        Pump(12);
        window.UpdateLayout();

        var original = new PointWin32();
        GetCursorPos(out original);
        var results = new List<string>();
        var failures = new List<string>();
        try
        {
            // What property do the style triggers actually watch? The input pipeline sets
            // UIElement.IsMouseOver; a trigger bound to a shadowing declaration never fires.
            foreach (var candidate in new[] { "DefaultButtonStyle", "AccentButtonStyle" })
            {
                var applied = FluentThemeManager.GetStyle(candidate);
                results.Add($"style {candidate}: setters={applied.Setters.Count} triggers={applied.Triggers.Count} basedOn={applied.BasedOn?.TargetType?.Name ?? "none"}");
                foreach (var trigger in applied.Triggers.OfType<Trigger>())
                {
                    results.Add($"  trigger {trigger.Property.Name} owner={trigger.Property.OwnerType?.Name} value={trigger.Value}");
                }
            }

            var implicitStyle = (Style?)button.Style;
            results.Add($"button.Style={(implicitStyle is null ? "null(implicit)" : implicitStyle.TargetType?.Name)} " +
                        $"triggers={implicitStyle?.Triggers.Count ?? -1}");
            var effective = button.GetValue(Control.BackgroundProperty);
            results.Add($"bg before: {effective}");

            // Step 1: cursor parked away from the window. Each step waits for the state itself
            // rather than a fixed frame count, then reads the property and captures twice - the
            // first capture flushes a pending pass, the second is the picture to claim.
            WaitFor(() => { SetCursorPos(4, 4); return !button.IsMouseOver; });
            var rest = CaptureStable(button);
            results.Add($"rest    : IsMouseOver={button.IsMouseOver} IsPressed={button.IsPressed} bg={Hex(button.GetValue(Control.BackgroundProperty))} {Describe(rest, button)}");
            Check(rest, Rest, "rest fill", failures);

            // Step 2: real cursor move onto the button centre.
            var clientOrigin = new PointWin32 { X = 0, Y = 0 };
            ClientToScreen(window.Handle, ref clientOrigin);
            var scale = window.DpiScale;
            var target = new PointWin32
            {
                X = clientOrigin.X + (int)Math.Round((100 + 100) * scale),
                Y = clientOrigin.Y + (int)Math.Round((140 + 22) * scale),
            };
            results.Add($"target  : client=({clientOrigin.X},{clientOrigin.Y}) scale={scale} -> screen=({target.X},{target.Y})");
            WaitFor(() => { SetCursorPos(target.X, target.Y); return button.IsMouseOver; });
            var hover = CaptureStable(button);
            results.Add($"hover   : IsMouseOver={button.IsMouseOver} IsPressed={button.IsPressed} bg={Hex(button.GetValue(Control.BackgroundProperty))} {Describe(hover, button)}");
            Pump(20);
            hover = CaptureStable(button);
            results.Add($"hover2  : IsMouseOver={button.IsMouseOver} bg={Hex(button.GetValue(Control.BackgroundProperty))} {Describe(hover, button)}");
            Check(hover, Hover, "hover fill", failures);

            // Where does the value stop? The control's own Background is already the hover brush, so
            // compare the template part's Background, and then force a repaint to see whether this is
            // a propagation gap or an invalidation gap.
            var surface = FirstBorder(button);
            results.Add($"part    : surfaceBorder bg={(surface is null ? "not found" : Hex(surface.Background))}");
            button.InvalidateVisual();
            Pump(20);
            var afterInvalidate = CaptureStable(button);
            results.Add($"invalid : IsMouseOver={button.IsMouseOver} bg={Hex(button.GetValue(Control.BackgroundProperty))} {Describe(afterInvalidate, button)}");
            var plain = new Button { Content = "plain", Width = 200, Height = 44, Template = NoTransitionTemplate() };
            var host = (Panel)window.Content;
            Canvas.SetLeft(plain, 100);
            Canvas.SetTop(plain, 200);
            host.Children.Add(plain);
            Pump(20);
            results.Add($"plain   : rest bg={Hex(plain.GetValue(Control.BackgroundProperty))} {Describe(Capture(plain), plain)}");
            SetCursorPos(target.X, target.Y + 66);
            WaitFor(() => plain.IsMouseOver);
            var plainHover = Capture(plain);
            results.Add($"plainHov: IsMouseOver={plain.IsMouseOver} bg={Hex(plain.GetValue(Control.BackgroundProperty))} {Describe(plainHover, plain)}");
            Check(plainHover, Hover, "no-transition template hover fill", failures);
            SetCursorPos(4, 4);
            var landed = new PointWin32();
            GetCursorPos(out landed);
            results.Add($"cursor  : asked=({target.X},{target.Y}) actual=({landed.X},{landed.Y})");

            if (withPress)
            {
                Send(target, LeftDown);
                Pump(12);
                var pressed = CaptureStable(button);
                results.Add($"pressed : IsMouseOver={button.IsMouseOver} IsPressed={button.IsPressed} {Describe(pressed, button)}");
                Check(pressed, Pressed, "pressed fill", failures);
                Send(target, LeftUp);
                Pump(12);
                var released = CaptureStable(button);
                results.Add($"released: IsMouseOver={button.IsMouseOver} IsPressed={button.IsPressed} {Describe(released, button)}");
                Check(released, Hover, "release returns to hover", failures);
            }

            // Step 3: move away again - the state has to leave, not latch.
            WaitFor(() => { SetCursorPos(4, 4); return !button.IsMouseOver; });
            Pump(20);
            var away = CaptureStable(button);
            results.Add($"away    : IsMouseOver={button.IsMouseOver} IsPressed={button.IsPressed} bg={Hex(button.GetValue(Control.BackgroundProperty))} {Describe(away, button)}");
            Check(away, Rest, "return to rest", failures);
        }
        finally
        {
            SetCursorPos(original.X, original.Y);
            FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", null);
            FluentThemeManager.OverrideBrush("ControlFillColorSecondaryBrush", null);
            FluentThemeManager.OverrideBrush("ControlFillColorTertiaryBrush", null);
            window.Close();
        }

        foreach (var line in results) Console.WriteLine(line);
        foreach (var line in failures) Console.WriteLine($"FAIL {line}");
        Console.WriteLine(failures.Count == 0 ? "RESULT pass" : "RESULT fail");
        return failures.Count == 0 ? 0 : 1;
    }

    /// <summary>
    /// Recon for the route the standing objective actually allows. The hover evidence this project has today is made
    /// by moving the real OS cursor with user32 (the mode above), which is screen-coordinate injection and explicitly
    /// out of bounds for a repeatable gate. This mode inventories what the shipped 26.10.9 surface offers to a test that
    /// stays inside the process: the types, the injection-shaped methods, the <c>On*Pointer*</c> virtuals, the routed
    /// events and dependency properties a synthetic raise would have to satisfy, and which event-arg constructors are
    /// reachable. Reads only - it opens no window and sends no input.
    /// </summary>
    private static int Surface()
    {
        var lines = new List<string>();
        void Say(string text) => lines.Add(text);

        var assemblies = new List<Assembly>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if ((assembly.GetName().Name ?? string.Empty).StartsWith("Jalium.UI", StringComparison.Ordinal))
            {
                assemblies.Add(assembly);
            }
        }

        var types = new List<Type>();
        foreach (var assembly in assemblies)
        {
            try
            {
                types.AddRange(assembly.GetTypes());
            }
            catch (Exception error)
            {
                Say($"types of {assembly.GetName().Name}: threw {error.GetType().Name}");
            }
        }

        Say($"=== A. pointer-shaped types ({types.Count} Jalium types scanned) ===");
        foreach (var type in types.Where(static type => type.Name.Contains("Pointer", StringComparison.Ordinal)
                || type.Name.Contains("Mouse", StringComparison.Ordinal)
                || type.Name.Contains("Touch", StringComparison.Ordinal)
                || type.Name.Contains("Stylus", StringComparison.Ordinal)
                || (type.Name.Contains("Input", StringComparison.Ordinal)
                    && (type.Name.EndsWith("Device", StringComparison.Ordinal) || type.Name.EndsWith("Manager", StringComparison.Ordinal)
                        || type.Name.EndsWith("Router", StringComparison.Ordinal) || type.Name.EndsWith("Source", StringComparison.Ordinal)))
                || type.Name.Contains("Inject", StringComparison.Ordinal)
                || type.Name.Contains("Simulat", StringComparison.Ordinal)
                || type.Name.Contains("Synthes", StringComparison.Ordinal))
            .OrderBy(static type => type.FullName, StringComparer.Ordinal).ToList())
        {
            Say($"{(type.IsPublic ? "public " : "interna")} {type.FullName} : {type.BaseType?.Name ?? "-"}");
        }

        static string Shape(MethodBase member) =>
            $"{(member.IsPublic ? "public " : member.IsFamily ? "protected " : member.IsAssembly ? "internal " : "private ")}"
            + $"{member.Name}({string.Join(",", member.GetParameters().Select(static p => p.ParameterType.Name))})"
            + (member.IsStatic ? " [static]" : string.Empty);

        Say("");
        Say("=== B. methods whose name says 'input arrives from inside' ===");
        var injected = new List<MethodBase>();
        foreach (var type in types)
        {
            foreach (var member in Declared(type))
            {
                if (member.Name.Contains("Inject", StringComparison.Ordinal)
                    || member.Name.Contains("Simulat", StringComparison.Ordinal)
                    || member.Name.Contains("Synthes", StringComparison.Ordinal)
                    || member.Name.Contains("Deliver", StringComparison.Ordinal)
                    || member.Name.Contains("Feed", StringComparison.Ordinal)
                    || member.Name.Contains("Enqueue", StringComparison.Ordinal))
                {
                    injected.Add(member);
                }
            }
        }

        foreach (var member in injected.Take(60))
        {
            Say($"{member.DeclaringType?.FullName}: {Shape(member)}");
        }

        Say($"(total {injected.Count})");

        Say("");
        Say("=== C. On*Pointer* / On*Mouse* virtuals on element and control types ===");
        var handlers = new List<MethodBase>();
        foreach (var type in types.Where(static type => type.Namespace is string ns && ns.StartsWith("Jalium.UI", StringComparison.Ordinal)
                && typeof(UIElement).IsAssignableFrom(type) && !type.IsInterface))
        {
            handlers.AddRange(Declared(type).Where(member => member.Name.StartsWith("On", StringComparison.Ordinal)
                && (member.Name.Contains("Pointer", StringComparison.Ordinal) || member.Name.Contains("Mouse", StringComparison.Ordinal)
                    || member.Name.Contains("Enter", StringComparison.Ordinal) || member.Name.Contains("Leave", StringComparison.Ordinal))));
        }

        foreach (var member in handlers.OrderBy(static m => m.DeclaringType?.FullName, StringComparer.Ordinal).Take(80))
        {
            Say($"{member.DeclaringType?.Name}: {Shape(member)}");
        }

        Say($"(total {handlers.Count})");

        Say("");
        Say("=== D. routed events a synthetic raise could target (Pointer / Mouse in the field name) ===");
        var events = new List<string>();
        var dps = new List<string>();
        foreach (var type in types)
        {
            foreach (var field in Fields(type))
            {
                var name = field.Name;
                if (!name.Contains("Pointer", StringComparison.Ordinal) && !name.Contains("Mouse", StringComparison.Ordinal)
                    && !name.Contains("Touch", StringComparison.Ordinal) && !name.Contains("Pressed", StringComparison.Ordinal)
                    && !name.Contains("Hover", StringComparison.Ordinal))
                {
                    continue;
                }

                if (field.FieldType.Name.Contains("RoutedEvent", StringComparison.Ordinal))
                {
                    events.Add($"{type.FullName}.{name} : {(field.IsPublic ? "public" : "non-public")}");
                }
                else if (field.FieldType.Name.Contains("DependencyProperty", StringComparison.Ordinal))
                {
                    dps.Add($"{type.FullName}.{name} : {(field.IsPublic ? "public" : "non-public")}");
                }
            }
        }

        foreach (var line in events.OrderBy(static text => text, StringComparer.Ordinal).Take(60))
        {
            Say(line);
        }

        Say($"(total routed events {events.Count})");

        Say("");
        Say("=== E. pointer-shaped dependency properties ===");
        foreach (var line in dps.OrderBy(static text => text, StringComparer.Ordinal).Take(60))
        {
            Say(line);
        }

        Say($"(total dependency properties {dps.Count})");

        Say("");
        Say("=== F. event-arg and device constructors: can a test build one? ===");
        foreach (var name in new[] { "PointerEventArgs", "MouseEventArgs", "RawPointerEventArgs", "PointerRoutedEventArgs",
            "PointerDevice", "MouseDevice", "PointerPoint", "PointerUpdateEventArgs", "InputDevice", "KeyboardDevice" })
        {
            Type? found = null;
            foreach (var candidate in types)
            {
                if (candidate.Name == name)
                {
                    found = candidate;
                    break;
                }
            }

            if (found is null)
            {
                Say($"{name}: absent");
                continue;
            }

            var constructors = found.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(Shape).OrderBy(static text => text, StringComparer.Ordinal).ToList();
            var statics = found.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(static member => member.MemberType is MemberTypes.Property or MemberTypes.Method or MemberTypes.Field)
                .Select(static member => member.Name).Distinct().OrderBy(static text => text, StringComparer.Ordinal).ToList();
            Say($"{found.FullName}{(found.IsPublic ? " (public)" : " (non-public)")}");
            Say($"   ctors: {(constructors.Count == 0 ? "none" : Trim(string.Join(" | ", constructors), 400))}");
            Say($"   statics: {Trim(string.Join(" ", statics), 400)}");
        }

        var path = Path.Combine(AppContext.BaseDirectory, "pointer-surface.txt");
        File.WriteAllLines(path, lines);
        Console.WriteLine($"wrote {path} ({lines.Count} lines)");
        return 0;
    }

    /// <summary>
    /// The second half of the #13 census, narrowed to the one question section F above leaves open: is there a seam
    /// inside this process where the framework itself ingests a platform event and does the hit test, the hover
    /// bookkeeping and the press tracking for me? Section F proved an <c>PointerEventArgs</c> and a
    /// <c>PointerPoint</c> are publicly constructible, but constructing one only fires handlers - it cannot make
    /// <c>IsMouseOver</c> true, because that name is written by the router through a non-public key. So this mode
    /// walks the other end: every method in the shipped assemblies whose signature mentions <c>PlatformEvent</c>
    /// (the platform's own event shape), the full member list of the router type, the device singletons a test could
    /// reach, and the setter accessibility of every press/hover property a trigger could read. Reads only.
    /// </summary>
    private static int Route()
    {
        var lines = new List<string>();
        void Say(string text) => lines.Add(text);

        static string Shape(MethodBase member) =>
            $"{(member.IsPublic ? "public " : member.IsFamily ? "protected " : member.IsAssembly ? "internal " : "private ")}"
            + $"{member.Name}({string.Join(",", member.GetParameters().Select(static p => p.ParameterType.Name))})"
            + (member.IsStatic ? " [static]" : string.Empty);

        var types = new List<Type>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!(assembly.GetName().Name ?? string.Empty).StartsWith("Jalium.UI", StringComparison.Ordinal))
            {
                continue;
            }

            try
            {
                types.AddRange(assembly.GetTypes());
            }
            catch (Exception error)
            {
                Say($"types of {assembly.GetName().Name}: threw {error.GetType().Name}");
            }
        }

        Say($"=== A. the platform event shape ({types.Count} Jalium types scanned) ===");
        Type? platformEvent = null;
        foreach (var type in types)
        {
            if (type.Name == "PlatformEvent")
            {
                platformEvent = type;
                break;
            }
        }

        if (platformEvent is null)
        {
            Say("PlatformEvent: absent from the loaded assemblies");
        }
        else
        {
            Say($"{platformEvent.FullName}{(platformEvent.IsPublic ? " (public)" : " (non-public)")} : {platformEvent.BaseType?.Name ?? "-"}");
            Say($"   ctors: {Trim(string.Join(" | ", Declared(platformEvent).Select(Shape)), 500)}");
            foreach (var member in platformEvent.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                     .Where(static m => m.MemberType is MemberTypes.Property or MemberTypes.Field)
                     .OrderBy(static m => m.Name, StringComparer.Ordinal))
            {
                if (member is PropertyInfo property)
                {
                    var setter = property.GetSetMethod(true);
                    var verb = setter is null
                        ? "readonly"
                        : setter.IsPublic ? "public set" : setter.IsAssembly ? "internal set" : "private set";
                    Say($"   prop {property.PropertyType.Name} {property.Name} : {verb}");
                }
                else if (member is FieldInfo field)
                {
                    Say($"   field {field.FieldType.Name} {field.Name} : {(field.IsPublic ? "public" : field.IsAssembly ? "internal" : "private")}");
                }
            }
        }

        Say("");
        Say("=== B. every Jalium method whose signature mentions PlatformEvent (the ingestion seam) ===");
        var seams = new List<string>();
        foreach (var type in types)
        {
            var typeShape = type.IsPublic ? "public" : "non-public";
            foreach (var member in Declared(type))
            {
                if (!member.GetParameters().Any(static p => p.ParameterType.Name.Contains("PlatformEvent", StringComparison.Ordinal)))
                {
                    continue;
                }

                seams.Add($"{type.FullName} ({typeShape}): {Shape(member)}");
            }
        }

        foreach (var line in seams.OrderBy(static text => text, StringComparer.Ordinal).Take(70))
        {
            Say(line);
        }

        Say($"(total {seams.Count})");

        Say("");
        Say("=== C. the router type, member by member ===");
        foreach (var type in types.Where(static t => t.Name.Contains("InputDispatcher", StringComparison.Ordinal)
                || t.Name.Contains("InputRouter", StringComparison.Ordinal)
                || t.Name.Contains("PointerRouter", StringComparison.Ordinal)
                || t.Name.Contains("MouseRouter", StringComparison.Ordinal)).OrderBy(static t => t.FullName, StringComparer.Ordinal))
        {
            Say($"{type.FullName}{(type.IsPublic ? " (public)" : " (non-public)")} : {type.BaseType?.Name ?? "-"}");
            foreach (var member in Declared(type).Select(Shape).OrderBy(static text => text, StringComparer.Ordinal).Take(90))
            {
                Say($"   {Trim(member, 220)}");
            }

            Say($"   (fields: {Trim(string.Join(" ", Fields(type).Select(static f => $"{(f.IsPublic ? "pub" : f.IsAssembly ? "int" : "priv")} {f.FieldType.Name} {f.Name}")), 500)})");
        }

        Say("");
        Say("=== D. device singletons a test could reach ===");
        foreach (var name in new[] { "InputManager", "MouseDevice", "KeyboardDevice", "StylusDevice", "TouchDevice", "PointerDevice" })
        {
            foreach (var type in types.Where(t => t.Name == name).OrderBy(static t => t.FullName, StringComparer.Ordinal))
            {
                var statics = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(static m => m.MemberType is MemberTypes.Property or MemberTypes.Method or MemberTypes.Field)
                    .Select(static m => $"{(m is PropertyInfo p ? "prop" : m is FieldInfo f ? "field" : "meth")} {m.Name}")
                    .Distinct()
                    .OrderBy(static text => text, StringComparer.Ordinal)
                    .ToList();
                Say($"{type.FullName}{(type.IsPublic ? " (public)" : " (non-public)")}: {Trim(string.Join(" | ", statics), 600)}");
            }
        }

        Say("");
        Say("=== E. hover/press bookkeeping methods on elements ===");
        var bookkeeping = new List<string>();
        foreach (var type in types.Where(static t => typeof(UIElement).IsAssignableFrom(t) && !t.IsInterface))
        {
            foreach (var member in Declared(type))
            {
                var name = member.Name;
                if (name.Contains("MouseOver", StringComparison.Ordinal) || name.Contains("DirectlyOver", StringComparison.Ordinal)
                    || name.Contains("Hover", StringComparison.Ordinal) || name.Contains("SetIsPressed", StringComparison.Ordinal)
                    || name.Contains("UpdatePressed", StringComparison.Ordinal))
                {
                    bookkeeping.Add($"{type.Name}: {Shape(member)}");
                }
            }
        }

        foreach (var line in bookkeeping.OrderBy(static text => text, StringComparer.Ordinal).Take(50))
        {
            Say(line);
        }

        Say($"(total {bookkeeping.Count})");

        Say("");
        Say("=== F. what a trigger reads: CLR setter accessibility of the press/hover names ===");
        foreach (var (typeName, propertyName) in new[]
                 {
                     ("UIElement", "IsMouseOver"), ("UIElement", "IsPressed"), ("UIElement", "IsMouseDirectlyOver"),
                     ("ContentElement", "IsMouseOver"),
                     ("Primitives.ButtonBase", "IsPressed"), ("Controls.MenuItem", "IsPressed"),
                     ("Controls.NavigationViewItem", "IsPressed"),
                 })
        {
            Type? found = null;
            foreach (var type in types)
            {
                if (type.FullName == $"Jalium.UI.{typeName}")
                {
                    found = type;
                    break;
                }
            }

            if (found is null)
            {
                Say($"{typeName}.{propertyName}: type absent");
                continue;
            }

            var property = found.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var declared = found.GetField($"{propertyName}Property", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var key = found.GetField($"{propertyName}PropertyKey", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var setter = property?.GetSetMethod(true);
            Say($"{found.FullName}.{propertyName}"
                + $" | clr: {(property is null ? "absent" : setter is null ? "getter only" : setter.IsPublic ? "public set" : setter.IsFamily ? "protected set" : setter.IsAssembly ? "internal set" : "private set")}"
                + $" | DP field: {(declared is null ? "absent" : declared.IsPublic ? "public" : "non-public")}"
                + $" | key field: {(key is null ? "none" : key.IsPublic ? "public" : "non-public")}");
        }

        Say("");
        Say("=== G. anything already shaped like a test harness ===");
        foreach (var type in types.Where(static t => t.Name.Contains("Headless", StringComparison.Ordinal)
                || t.Name.Contains("TestHost", StringComparison.Ordinal)
                || t.Name.Contains("Simulat", StringComparison.Ordinal)
                || t.Name.Contains("Fake", StringComparison.Ordinal)
                || t.Name.Contains("Stub", StringComparison.Ordinal)
                || t.Name.Contains("Mock", StringComparison.Ordinal))
            .OrderBy(static t => t.FullName, StringComparer.Ordinal).Take(40))
        {
            Say($"{(type.IsPublic ? "public " : "interna")} {type.FullName} : {type.BaseType?.Name ?? "-"}");
        }

        Say("");
        Say("=== H. arbitration: what Window actually declares, and what a PlatformEvent actually is ===");
        Say("   Why this section exists: section B above found no method mentioning PlatformEvent, yet the surface");
        Say("   census of the same assemblies listed Window.SynthesizeMouseFromTouch(PlatformEvent,...). Two");
        Say("   readings of one fact cannot both stand, so this prints the raw member rows for the two types that");
        Say("   matter and the full name of every parameter type, which is what B matched a short name against.");
        Say($"   assemblies loaded now: {string.Join(", ", AppDomain.CurrentDomain.GetAssemblies().Select(static a => a.GetName().Name))}");
        foreach (var typeName in new[] { "Jalium.UI.Window", "Jalium.UI.UIElement" })
        {
            Type? target = null;
            foreach (var type in types)
            {
                if (type.FullName == typeName)
                {
                    target = type;
                    break;
                }
            }

            if (target is null)
            {
                Say($"{typeName}: absent from the scanned set");
                continue;
            }

            var declared = Declared(target).ToList();
            Say($"{typeName}: {declared.Count} declared members, assembly {target.Assembly.GetName().Name}");
            foreach (var member in declared.Where(static m => m.Name.Contains("Mouse", StringComparison.Ordinal)
                    || m.Name.Contains("Pointer", StringComparison.Ordinal)
                    || m.Name.Contains("Synthes", StringComparison.Ordinal)
                    || m.Name.Contains("Input", StringComparison.Ordinal)).Take(45))
            {
                var parameters = string.Join(" | ", member.GetParameters().Select(static p =>
                    $"{p.Name}:{p.ParameterType.FullName}~{p.ParameterType.Assembly.GetName().Name}"));
                Say($"   {Shape(member)}");
                Say($"      <- {Trim(parameters, 300)}");
            }
        }

        Say("");
        Say("=== I. the same window surface as the framework exposes it to a subclass ===");
        var overridable = new List<string>();
        foreach (var type in types)
        {
            foreach (var member in Declared(type))
            {
                if (!member.IsFamily || member.DeclaringType is null || !typeof(UIElement).IsAssignableFrom(member.DeclaringType))
                {
                    continue;
                }

                if (member.Name.Contains("Mouse", StringComparison.Ordinal) || member.Name.Contains("Pointer", StringComparison.Ordinal))
                {
                    overridable.Add($"{type.Name}: {Shape(member)}");
                }
            }
        }

        foreach (var line in overridable.OrderBy(static text => text, StringComparer.Ordinal).Take(60))
        {
            Say(line);
        }

        Say($"(total protected pointer members {overridable.Count})");

        var path = Path.Combine(AppContext.BaseDirectory, "pointer-route.txt");
        File.WriteAllLines(path, lines);
        Console.WriteLine($"wrote {path} ({lines.Count} lines)");
        return 0;
    }

    /// <summary>
    /// The #13 attempt at an in-process pointer. The census in <see cref="Surface" /> and <see cref="Route" /> settled
    /// what the shipped 26.10.9 offers: <c>UIElement.SetIsMouseOver(Boolean)</c> is internal and the DP's key is
    /// non-public, so no test outside the framework assembly can write the hover state, and the only in-process door
    /// that leads to the real hit test is <c>WindowInputDispatcher</c>'s public <c>HandleMouse*</c> family - on a
    /// non-public type, reached through a field of <c>Window</c>. So this mode keeps the reflection to the narrowest
    /// possible job (find that field, then call the framework's own public handlers) and spends its effort on proving
    /// the state arrived through the pipeline rather than from a written property:
    /// <list type="number">
    /// <item>the physical cursor is parked off-window for the whole run and re-read at the end, so nothing here can be
    /// attributed to a real mouse - this mode calls no user32 function except that read;</item>
    /// <item><c>Click</c> must fire from the down/up pair. A hand-assigned <c>IsPressed</c> cannot raise it, which is
    /// what separates this evidence from the state write the public setters would allow;</item>
    /// <item>every step is followed by the two readings a style trigger consumes (<c>IsMouseOver</c>,
    /// <c>IsPressed</c>) and a pixel histogram, because the point of the route is the pixels, not the booleans.</item>
    /// </list>
    /// Stops loudly (exit 2) when the acquisition fails - a silent no-op here would look like "the trigger does not
    /// repaint" and be read as a product finding.
    /// </summary>
    private static int Synth()
    {
        var report = new List<string>();
        void Say(string text)
        {
            report.Add(text);
            // Printed as it is produced: a run that blocks part way through has to show which leg it reached.
            Console.WriteLine(text);
        }

        FluentThemeManager.OverrideBrush("ControlFillColorDefaultBrush", Rest);
        FluentThemeManager.OverrideBrush("ControlFillColorSecondaryBrush", Hover);
        FluentThemeManager.OverrideBrush("ControlFillColorTertiaryBrush", Pressed);

        var live = new Button { Content = "live", Width = 200, Height = 44 };
        var plain = new Button { Content = "plain", Width = 200, Height = 44, Template = NoTransitionTemplate() };
        var clicks = 0;
        live.Click += static (_, _) => { };
        plain.Click += (_, _) => clicks++;

        var canvas = new Canvas();
        Canvas.SetLeft(live, 100);
        Canvas.SetTop(live, 140);
        Canvas.SetLeft(plain, 100);
        Canvas.SetTop(plain, 200);
        canvas.Children.Add(live);
        canvas.Children.Add(plain);

        var window = new Window
        {
            Title = "PointerProbe synth",
            Content = canvas,
            Width = 420,
            Height = 320,
            Topmost = true,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = 500,
            Top = 200,
        };
        window.Show();
        Pump(12);
        window.UpdateLayout();

        GetCursorPos(out var cursorStart);
        Say($"cursor at start (untouched for the whole run): ({cursorStart.X},{cursorStart.Y})");
        Say($"window at ({window.Left},{window.Top}) size {window.Width}x{window.Height} dpi={window.DpiScale}");

        static string Shape(ParameterInfo[] parameters) =>
            string.Join(", ", parameters.Select(static p => $"{p.ParameterType.Name} {p.Name}"));

        // Acquisition. One field, one type - everything downstream is a public method call.
        FieldInfo? handle = null;
        foreach (var candidate in typeof(Window).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                     .Concat(typeof(Window).BaseType?.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) ?? []))
        {
            if (candidate.FieldType.Name.Contains("Dispatcher", StringComparison.Ordinal)
                && candidate.FieldType.Name.Contains("Input", StringComparison.Ordinal))
            {
                handle = candidate;
                break;
            }
        }

        var candidates = typeof(Window)
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(static field => field.Name.Contains("input", StringComparison.OrdinalIgnoreCase)
                || field.Name.Contains("dispatcher", StringComparison.OrdinalIgnoreCase))
            .Select(static field => $"{field.FieldType.Name} {field.Name}")
            .ToList();
        Say($"window fields that look input-shaped: {string.Join(" | ", candidates)}");

        if (handle is null)
        {
            Say("ACQUIRE=failed: no Window field whose type name mentions both Input and Dispatcher");
            return 2;
        }

        var dispatcher = handle.GetValue(window)!;
        var dispatcherType = dispatcher.GetType();
        Say($"ACQUIRE=ok via {handle.Name} : {dispatcherType.FullName} (type public? {dispatcherType.IsPublic})");
        Say($"the three calls this mode makes, as the framework declares them:");
        var methods = new Dictionary<string, MethodInfo>();
        foreach (var name in new[] { "HandleMouseMove", "HandleMouseDown", "HandleMouseUp" })
        {
            var method = dispatcherType.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method is null)
            {
                Say($"   {name}: ABSENT");
                continue;
            }

            methods[name] = method;
            Say($"   {(method.IsPublic ? "public" : "non-public")} {name}({Shape(method.GetParameters())})");
        }

        if (methods.Count < 3)
        {
            Say("CALL=failed: the router does not declare the HandleMouse* trio this mode was written against");
            return 2;
        }

        object[] Arguments(MethodInfo method, double x, double y, bool pressed, int clickCount)
        {
            var parameters = method.GetParameters();
            var arguments = new object[parameters.Length];
            var tick = Environment.TickCount;
            for (var index = 0; index < parameters.Length; index++)
            {
                var type = parameters[index].ParameterType;
                if (type == typeof(Point))
                {
                    arguments[index] = new Point(x, y);
                }
                else if (type.IsEnum && type.Name is "MouseButton" or "MouseButtonStates")
                {
                    // Left is the only member either enum needs here, and naming it by string keeps this mode from
                    // depending on which of the two shapes it is pointed at.
                    arguments[index] = pressed || type.Name == "MouseButton"
                        ? Enum.Parse(type, "Left")
                        : Enum.ToObject(type, 0);
                }
                else if (type.IsEnum)
                {
                    arguments[index] = Enum.ToObject(type, 0);
                }
                else if (type == typeof(int))
                {
                    // The lead Int32 on HandleMouseDown is the click count, the trailing one the event timestamp;
                    // the names printed above are what this mapping is actually claiming.
                    arguments[index] = parameters[index].Name is "clickCount" or "clicks" ? clickCount : tick;
                }
                else
                {
                    arguments[index] = type.IsValueType ? Activator.CreateInstance(type)! : null!;
                }
            }

            return arguments;
        }

        Say("");
        Say("leg 0: nothing fed yet - the resting picture is the control");
        var rest = CaptureStable(plain);
        Say($"   live : IsMouseOver={live.IsMouseOver} IsPressed={live.IsPressed}");
        Say($"   plain: IsMouseOver={plain.IsMouseOver} IsPressed={plain.IsPressed} {Describe(rest, plain)}");
        var failures = new List<string>();
        Check(rest, Rest, "rest fill (no input fed)", failures);

        Say("");
        Say("leg 1: HandleMouseMove - what position space does the router read?");
        var move = methods["HandleMouseMove"];
        var lastOver = dispatcherType.GetProperty("LastMouseOverElement", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var captureChanged = dispatcherType.GetMethod("HandleNativeCaptureChanged", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Say($"   diagnostics: LastMouseOverElement {(lastOver is null ? "absent" : "present")}, "
            + $"HandleNativeCaptureChanged {(captureChanged is null ? "absent" : "present")}");

        void Feed(double x, double y, bool pressed)
        {
            move.Invoke(dispatcher, Arguments(move, x, y, pressed, 1));
        }

        // Canvas attachment plus half the size, which is where a TranslatePoint out of the tree said the button
        // was NOT: translating the plain centre to the window returned (100,54) for a button the Canvas puts at
        // (100,200), i.e. that call subtracts the element's own offset instead of adding the chrome. The sweep
        // below is the measurement that decides the space, and it agrees with canvas coordinates.
        var plainCentre = (X: Canvas.GetLeft(plain) + plain.ActualWidth / 2, Y: Canvas.GetTop(plain) + plain.ActualHeight / 2);
        var liveCentre = (X: Canvas.GetLeft(live) + live.ActualWidth / 2, Y: Canvas.GetTop(live) + live.ActualHeight / 2);
        Say($"   plain centre in canvas numbers ({plainCentre.X:0.##},{plainCentre.Y:0.##}), live ({liveCentre.X:0.##},{liveCentre.Y:0.##})");

        (bool Arrived, string Where, double X, double Y) FindHover(Button target, (double X, double Y) canvasPoint, string label)
        {
            Feed(canvasPoint.X, canvasPoint.Y, false);
            if (target.IsMouseOver)
            {
                return (true, $"canvas ({canvasPoint.X:0.##},{canvasPoint.Y:0.##})", canvasPoint.X, canvasPoint.Y);
            }

            for (var y = 0d; y <= 400d; y += 12)
            {
                for (var x = 0d; x <= 400d; x += 12)
                {
                    Feed(x, y, false);
                    if (target.IsMouseOver)
                    {
                        return (true, $"swept ({x:0},{y:0})", x, y);
                    }
                }
            }

            Say($"   {label}: no point in 0..400 set IsMouseOver; router thought it was over "
                + (lastOver?.GetValue(dispatcher)?.GetType().Name ?? "-"));
            return (false, "never", 0, 0);
        }

        // One feed does not survive the pump: whatever reconciles the router with the native cursor wins within a
        // few frames, so the picture has to be taken with the pointer held - re-fed before every pump and again
        // immediately before the grab. That is the route's real shape, so a gate test built on it has to hold too.
        Dictionary<uint, int> HeldCapture(double x, double y, Button target, bool pressed)
        {
            Dictionary<uint, int>? previous = null;
            for (var round = 0; round < 8; round++)
            {
                Feed(x, y, pressed);
                Pump(4, 200);
                Feed(x, y, pressed);
                var current = Capture(target);
                if (previous is not null && current.Count == previous.Count && !current.Except(previous).Any()) return current;
                previous = current;
            }

            return previous ?? [];
        }

        var plainProbe = FindHover(plain, plainCentre, "plain");
        var hoverX = plainProbe.X;
        var hoverY = plainProbe.Y;
        if (!plainProbe.Arrived)
        {
            failures.Add("the in-process feed never produced hover on the plain template, so the route is not established");
        }
        else
        {
            Say($"HOVER=ok via {plainProbe.Where}");
            Say($"   plain's canvas rect is x {Canvas.GetLeft(plain):0}..{Canvas.GetLeft(plain) + plain.ActualWidth:0} "
                + $"y {Canvas.GetTop(plain):0}..{Canvas.GetTop(plain) + plain.ActualHeight:0}, and the first point the router "
                + $"agrees is inside it is ({plainProbe.X:0},{plainProbe.Y:0}): the router's space is neither the canvas "
                + "numbers nor those numbers times DPI, so the point has to be found by sweeping, not by arithmetic");
            var held = HeldCapture(hoverX, hoverY, plain, false);
            Say($"   held hover, plain template: IsMouseOver={plain.IsMouseOver} {Describe(held, plain)}");
            Check(held, Hover, "synthetic hover fill", failures);
            Feed(hoverX, hoverY, false);
            var instant = plain.IsMouseOver;
            Pump(1, 100);
            var oneFrame = plain.IsMouseOver;
            Pump(6, 300);
            var sixFrames = plain.IsMouseOver;
            Say($"   how long one feed lasts: immediate={instant} after 1 frame={oneFrame} after 6 frames={sixFrames}");
        }

        Say("");
        Say("leg 2: the real Fluent template - the style cells this library ships are the thing being measured");
        var liveProbe = FindHover(live, liveCentre, "live");
        if (!liveProbe.Arrived)
        {
            failures.Add("hover never arrived on the default-template button either");
        }
        else
        {
            var liveHeld = HeldCapture(liveProbe.X, liveProbe.Y, live, false);
            Say($"   held hover, default template: IsMouseOver={live.IsMouseOver} part bg={Hex(FirstBorder(live))} {Describe(liveHeld, live)}");
            if (live.IsMouseOver)
            {
                // The state arrived and the surface is still mid-blend after ~30 fed frames, which is the transition
                // not reaching its final value - the #35/#63 family - not the pointer failing to arrive.
                Say("   state on the default template is hover; the picture above is what the template transition "
                    + "settles to under a held pointer, and it is not the hover colour");
            }

            Check(liveHeld, Hover, "synthetic hover fill on the default template", failures);
        }

        Say("");
        Say("leg 3: HandleMouseDown - the press chain, then Click on release");
        methods["HandleMouseDown"].Invoke(dispatcher, Arguments(methods["HandleMouseDown"], hoverX, hoverY, true, 1));
        Feed(hoverX, hoverY, true);
        var pressedNow = plain.IsPressed;
        Say($"   immediately after down: plain.IsPressed={pressedNow} bg={Hex(plain.Background)} "
            + $"effective={Hex(plain.GetValue(Control.BackgroundProperty))}");
        if (!pressedNow)
        {
            failures.Add("HandleMouseDown never produced IsPressed at the point hover landed on");
        }
        else
        {
            // Press does not survive like hover does, so the picture is taken at three depths and all three are
            // reported. Claiming the one that came out orange, without the other two, would hide the shape.
            var depth0 = Capture(plain);
            Pump(1, 100);
            var depth1 = Capture(plain);
            var survivedOneFrame = plain.IsPressed;
            Feed(hoverX, hoverY, true);
            Pump(4, 200);
            Feed(hoverX, hoverY, true);
            var depthHeld = Capture(plain);
            Say($"   immediately:  IsPressed={pressedNow} {Describe(depth0, plain)}");
            Say($"   after 1 frame: IsPressed={survivedOneFrame} {Describe(depth1, plain)}");
            Say($"   re-fed +4:     IsPressed={plain.IsPressed} {Describe(depthHeld, plain)}");
            var best = new[] { depth0, depth1, depthHeld }
                .OrderByDescending(static histogram => histogram.GetValueOrDefault(0xFFA500u))
                .First();
            Check(best, Pressed, "synthetic press fill at its best depth", failures);
        }

        Say("");
        Say("leg 4: HandleMouseUp - the release has to land the click through the button's own logic");
        if (captureChanged is not null)
        {
            // Hypothesis on trial: Click needs the capture the native path grants, and an in-process feed cannot
            // make Win32 SetCapture happen. HandleNativeCaptureChanged is the public method the platform calls
            // when it does, so this feeds that edge too and says which of the two the click needed.
            captureChanged.Invoke(dispatcher, new object[] { window.Handle, window.Handle });
            Say($"   fed HandleNativeCaptureChanged({window.Handle},{window.Handle}) before releasing");
        }

        methods["HandleMouseUp"].Invoke(dispatcher, Arguments(methods["HandleMouseUp"], hoverX, hoverY, false, 1));
        Feed(hoverX, hoverY, false);
        Pump(6, 300);
        var release = HeldCapture(hoverX, hoverY, plain, false);
        Say($"   plain: IsPressed={plain.IsPressed} IsMouseOver={plain.IsMouseOver} clicks={clicks} {Describe(release, plain)}");
        if (plainProbe.Arrived)
        {
            Check(release, Hover, "fill after release is hover again", failures);
        }

        if (clicks == 0)
        {
            failures.Add("click never fired: the down/up pair reached the press state but not the button's own "
                + "logic, so a click cannot be evidenced this way");
        }

        Say("");
        Say("leg 5: move away - hover has to leave, or the state was never following the pointer");
        Feed(20, 20, false);
        Pump(6, 300);
        var away = CaptureStable(plain);
        Say($"   plain: IsMouseOver={plain.IsMouseOver} IsPressed={plain.IsPressed} {Describe(away, plain)}");
        if (plainProbe.Arrived)
        {
            Check(away, Rest, "fill after moving away is rest again", failures);
        }
        GetCursorPos(out var cursorEnd);
        Say("");
        Say($"cursor at end: ({cursorEnd.X},{cursorEnd.Y}) unchanged={cursorStart.X == cursorEnd.X && cursorStart.Y == cursorEnd.Y}");
        Say($"VERDICT failures={failures.Count}");
        foreach (var failure in failures) Say($"   FAIL {failure}");

        window.Close();
        var path = Path.Combine(AppContext.BaseDirectory, "pointer-synth.txt");
        File.WriteAllLines(path, report);
        Console.WriteLine($"wrote {path} ({report.Count} lines)");
        return failures.Count == 0 ? 0 : 1;
    }

    private static IEnumerable<MethodBase> Declared(Type type)
    {
        IEnumerable<MethodBase> all;
        try
        {
            all = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Cast<MethodBase>()
                .Concat(type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        }
        catch (Exception)
        {
            return [];
        }

        return all;
    }

    private static IEnumerable<FieldInfo> Fields(Type type)
    {
        try
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static string Trim(string text, int limit) =>
        text.Length <= limit ? text : text[..limit] + "...";

    private static void Send(PointWin32 target, uint buttonFlags)
    {
        var inputs = new[]
        {
            new Input
            {
                type = InputMouse,
                mouse = new MouseInput
                {
                    dx = target.X * 65535 / Math.Max(1, GetSystemMetrics(0) - 1),
                    dy = target.Y * 65535 / Math.Max(1, GetSystemMetrics(1) - 1),
                    dwFlags = Absolute | VirtualDesktop | buttonFlags,
                },
            },
        };
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
    }

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);

    /// <summary>A template with no TransitionProperty layer, parsed from markup.</summary>
    private static ControlTemplate NoTransitionTemplate() =>
        (ControlTemplate)XamlReader.Parse(
            "<ControlTemplate xmlns='http://schemas.jalium.ui/2024' TargetType='Button'>" +
            "<Border Background='{TemplateBinding Background}' BorderThickness='1' BorderBrush='Black' />" +
            "</ControlTemplate>")!;

    private static Border? FirstBorder(Visual root)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is Border border) return border;
            if (child is Visual visual && FirstBorder(visual) is { } deeper) return deeper;
        }

        return null;
    }

    /// <summary>
    /// Mouse-free: does a Border carrying TransitionProperty="Background" adopt a NEW brush object
    /// assigned after its first render? Compare against the same template without the transition.
    /// </summary>
    private static int Swap()
    {
        RenderContext.GetOrCreateCurrent(RenderBackend.Auto).DefaultRenderingEngine = RenderingEngine.Impeller;
        ThemeLoader.Initialize();
        var application = new Application();
        FluentThemeManager.Apply(application, FluentThemeVariant.Light);

        var transitioned = new Button { Content = "t", Width = 200, Height = 44, Background = new SolidColorBrush(Rest) };
        var plain = new Button { Content = "p", Width = 200, Height = 44, Template = NoTransitionTemplate(), Background = new SolidColorBrush(Rest) };
        var canvas = new Canvas();
        Canvas.SetLeft(transitioned, 40);
        Canvas.SetTop(transitioned, 40);
        Canvas.SetLeft(plain, 40);
        Canvas.SetTop(plain, 110);
        canvas.Children.Add(transitioned);
        canvas.Children.Add(plain);
        var window = new Window { Title = "PointerProbe swap", Content = canvas, Width = 320, Height = 300, Topmost = true, WindowStartupLocation = WindowStartupLocation.Manual, Left = 60, Top = 60 };
        window.Show();
        Pump(24);
        var report = new List<string>
        {
            $"transitioned rest : {Describe(CaptureStable(transitioned), transitioned)}",
            $"plain        rest : {Describe(CaptureStable(plain), plain)}",
        };
        transitioned.Background = new SolidColorBrush(Hover);
        plain.Background = new SolidColorBrush(Hover);
        Pump(30);
        var afterTransitioned = CaptureStable(transitioned);
        var afterPlain = CaptureStable(plain);
        report.Add($"transitioned swap: part bg={Hex(FirstBorder(transitioned)?.Background)} {Describe(afterTransitioned, transitioned)}");
        report.Add($"plain        swap: {Describe(afterPlain, plain)}");
        report.Add($"VERDICT transitionedAdopted={afterTransitioned.GetValueOrDefault(0x00FF00u) > 4_000} plainAdopted={afterPlain.GetValueOrDefault(0x00FF00u) > 4_000}");
        window.Close();
        foreach (var line in report) Console.WriteLine(line);
        return 0;
    }

    private static string Hex(object? brush) =>
        brush is SolidColorBrush solid ? $"#{solid.Color.R:X2}{solid.Color.G:X2}{solid.Color.B:X2}" : brush?.GetType().Name ?? "null";

    private static void WaitFor(Func<bool> condition, int milliseconds = 1500)
    {
        var deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * milliseconds / 1000;
        while (!condition() && Stopwatch.GetTimestamp() < deadline) Pump(4, 200);
    }

    private static void Check(Dictionary<uint, int> histogram, Color expected, string claim, List<string> failures)
    {
        var key = (uint)(expected.R << 16 | expected.G << 8 | expected.B);
        var count = histogram.GetValueOrDefault(key);
        if (count < 4_000) failures.Add($"{claim}: expected >=4000 pixels of #{key:X6}, got {count}");
    }

    private static string Describe(Dictionary<uint, int> histogram, FrameworkElement element) =>
        $"{(int)element.ActualWidth}x{(int)element.ActualHeight} distinct={histogram.Count} top=" +
        string.Join(" ", histogram.OrderByDescending(static entry => entry.Value).Take(4)
            .Select(entry => $"#{entry.Key:X6}x{entry.Value}"));

    /// <summary>A state change animates (the template transitions Background), so one grab can land
    /// mid-blend. Re-grab until two consecutive pictures agree.</summary>
    private static Dictionary<uint, int> CaptureStable(FrameworkElement element)
    {
        Dictionary<uint, int>? previous = null;
        for (var round = 0; round < 8; round++)
        {
            Pump(6, 300);
            var current = Capture(element);
            if (previous is not null && current.Count == previous.Count && !current.Except(previous).Any()) return current;
            previous = current;
        }

        return previous ?? [];
    }

    private static Dictionary<uint, int> Capture(FrameworkElement element)
    {
        var width = (int)element.ActualWidth;
        var height = (int)element.ActualHeight;
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(element);
        var stride = width * 4;
        var buffer = new byte[stride * height];
        bitmap.CopyPixels(new Int32Rect(0, 0, width, height), buffer, stride, 0);
        var histogram = new Dictionary<uint, int>();
        for (var offset = 0; offset + 3 < buffer.Length; offset += 4)
        {
            var key = (uint)(buffer[offset + 2] << 16 | buffer[offset + 1] << 8 | buffer[offset]);
            histogram[key] = histogram.GetValueOrDefault(key) + 1;
        }

        return histogram;
    }

    private static void Pump(int frames, int budgetMilliseconds = 800)
    {
        var frame = new DispatcherFrame();
        var seen = 0;
        var deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * budgetMilliseconds / 1000;
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames || Stopwatch.GetTimestamp() > deadline) frame.Continue = false;
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        // The watchdog has to speak to the dispatcher that is pumping, not to whichever one its timer thread
        // happens to own: Dispatcher.CurrentDispatcher on a pool thread silently creates a second, idle
        // dispatcher, the posted continuation runs there, and a render-starved pump never returns. Captured
        // before PushFrame because the frame is what makes this thread's dispatcher current.
        var pumping = Dispatcher.CurrentDispatcher;
        using var watchdog = new System.Threading.Timer(_ => pumping.InvokeAsync(() => frame.Continue = false));
        watchdog.Change(TimeSpan.FromMilliseconds(budgetMilliseconds * 2), System.Threading.Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
    }
}
