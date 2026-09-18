using System.Diagnostics;
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
        using var watchdog = new System.Threading.Timer(_ => Dispatcher.CurrentDispatcher.InvokeAsync(() => frame.Continue = false));
        watchdog.Change(TimeSpan.FromMilliseconds(budgetMilliseconds * 2), System.Threading.Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
    }
}
