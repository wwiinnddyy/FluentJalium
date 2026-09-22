using System.Diagnostics;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;
using Xunit;

namespace FluentJalium.Tests.Pixel;

/// <summary>
/// In-process pixel capture, so "the theme reached the pixels" is an assertion rather than a
/// screenshot somebody eyeballed. <see cref="RenderTargetBitmap"/> holds Bgr32, so bytes are B,G,R;
/// histograms are keyed by packed R,G,B to keep the test code readable.
/// </summary>
/// <remarks>
/// Measured on 26.10.9 (docs/astra/adaptation/06-pixel-attribution.md): nothing paints until the
/// dispatcher has produced real rendered frames, so every capture hosts its subject in a shown
/// window and pumps <see cref="CompositionTarget.Rendering"/> until two consecutive captures agree.
/// An element captured without that comes back unlit black, which is what made the first round of
/// readings read as "the theme never reaches a native control". Two consequences stay in force:
/// a template's brush transition is animated, so a fixed frame count can sample mid-transition, and
/// the alpha byte of a direct element capture is not written for brushes that go through a
/// TransitionProperty layer - so a direct capture proves which colour a token drove, not that the
/// result is visible once composited. Use <see cref="Host"/> for a claim about the composited window.
/// </remarks>
internal static class PixelHarness
{
    private const int FramesPerRound = 4;
    private const int MaxRounds = 12;

    /// <summary>How long one round is willing to wait for its frames.</summary>
    private const int PumpBudgetMilliseconds = 400;

    /// <summary>
    /// The settle question needs <i>two</i> rounds to be answerable at all, and a round costs at most
    /// <see cref="PumpBudgetMilliseconds"/>, so a cap below two of those can only ever answer "not settled" about
    /// a picture that is perfectly still but slow to get frames. Three rounds' worth is the smallest cap that
    /// cannot fail for that reason; a picture that genuinely keeps changing still fails, just after being asked.
    /// </summary>
    private const int SettleBudgetMilliseconds = PumpBudgetMilliseconds * 3;

    internal sealed record Sample(int Width, int Height, Dictionary<uint, int> Histogram, int Rounds = 0, bool Stable = false, string Subject = "", int CaptureMilliseconds = 0)
    {
        internal int DistinctColors => Histogram.Count;

        internal int Count(Color color) => Histogram.GetValueOrDefault(PixelKey(color));

        internal int CountAny(params Color[] colors) => colors.Sum(Count);

        /// <summary>Pixels that are not the unlit black of an unpainted surface.</summary>
        internal int PaintedPixels => Histogram.Where(static entry => entry.Key != 0).Sum(static entry => entry.Value);

        internal string Top(int count) => string.Join(" ", Histogram.OrderByDescending(static entry => entry.Value)
            .Take(count).Select(static entry => $"#{entry.Key:X6}x{entry.Value}"));
    }

    internal static uint PixelKey(Color color) => (uint)(color.R << 16 | color.G << 8 | color.B);

    /// <summary>
    /// The one host window every capture goes through. Measured in the same probe: only the first
    /// shown window on a thread receives rendered frames, so a harness that opens a window per
    /// capture gets a black picture from the second themed control onwards.
    /// </summary>
    private static Window? _host;

    /// <summary>
    /// Captures one control at the requested size, in DIP pixels: the render target draws a visual at
    /// its DIP bounds regardless of the monitor scale, so a 40x40 element is exactly 1600 pixels.
    /// </summary>
    internal static Sample Render(FrameworkElement element, int width, int height)
    {
        Place(element, width, height);

        var sample = Capture(() => CaptureRaw(element, (int)element.ActualWidth, (int)element.ActualHeight));
        return sample with { Subject = Describe(element) };
    }

    /// <summary>
    /// Captures a part the subject built inside itself, at that part's own bounds, so a claim about a
    /// scroll bar or a thumb is not diluted by the pixels around it. Two measured consequences: the
    /// alpha byte of a direct capture is not written, and a part whose real colour is black therefore
    /// reads as unpainted - capture an ancestor that has non-black pixels, or drive the part with an
    /// opaque sentinel brush.
    /// </summary>
    internal static Sample RenderPart<T>(FrameworkElement root, int width, int height) where T : FrameworkElement
    {
        Place(root, width, height);
        var part = Descendant<T>(root) ?? throw new InvalidOperationException($"No {typeof(T).Name} inside {Describe(root)}.");

        var sample = Capture(() => CaptureRaw(part, (int)part.ActualWidth, (int)part.ActualHeight));
        return sample with { Subject = Describe(part) };
    }

    /// <summary>
    /// Depth-first search of the built tree. Walking it stays on the test side of the line: the gate
    /// that forbids <c>VisualTreeHelper</c> under src/ is what keeps product code from needing it.
    /// </summary>
    internal static T? Descendant<T>(Visual root) where T : Visual
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) return match;
            if (child is Visual visual && Descendant<T>(visual) is { } deeper) return deeper;
        }

        return null;
    }

    /// <summary>
    /// A template part by the name the template gave it. State claims are read back from parts rather
    /// than captured when the subject has no opaque surface of its own - a glyph, an opacity flag.
    /// </summary>
    internal static FrameworkElement? Named(DependencyObject root, string name)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is FrameworkElement element && element.Name == name) return element;
            if (child is not null && Named(child, name) is { } deeper) return deeper;
        }

        return null;
    }

    /// <summary>
    /// Builds a control's tree inside the shown host and pumps frames, without rasterising anything.
    /// For a claim about what the style pipeline produced (a part, an inherited brush) this is the
    /// cheap path - and the only safe one for a subject that has no opaque surface: capturing a
    /// text-only visual costs tens of seconds in this runtime and writes no pixels at all.
    /// </summary>
    internal static void Build(FrameworkElement element, int width, int height)
    {
        Place(element, width, height);
        Pump(FramesPerRound);
    }

    private static void Place(FrameworkElement element, int width, int height)
    {
        element.Width = width;
        element.Height = height;
        var window = EnsureHost(width, height);
        var root = new Grid();
        root.Children.Add(element);
        window.Content = root;
        if (element is Control control) control.ApplyTemplate();
        window.UpdateLayout();
    }

    /// <summary>
    /// Captures the whole host window with the subject in it. This is the path that still holds the
    /// alpha channel, so it is the one to use when the claim is about composited output rather than
    /// about the colour a template painted. Counts scale with the monitor, so assert on change.
    /// </summary>
    internal static Sample Host(Visual element, int width = 320, int height = 200)
    {
        var window = EnsureHost(width, height);
        window.Content = element;
        window.UpdateLayout();
        // The subject travels with the sample: a capture that gives up used to report "never settled" with an
        // empty description, which left no way to tell which of the two shapes it was without re-running.
        return Capture(() => CaptureRaw(window, (int)window.ActualWidth, (int)window.ActualHeight))
            with { Subject = $"host/{element.GetType().Name}@{window.ActualWidth}x{window.ActualHeight}" };
    }

    /// <summary>
    /// Captures a visual where it already sits, without reparenting it. Window chrome - the title bar
    /// the window builds around its content - has no slot a subject can be placed in, so this is the
    /// only way to ask what colour it actually painted.
    /// </summary>
    internal static Sample Chrome(FrameworkElement element)
    {
        var sample = Capture(() => CaptureRaw(element, (int)element.ActualWidth, (int)element.ActualHeight));
        return sample with { Subject = Describe(element) };
    }

    /// <summary>
    /// One pixel of a visual where it already sits, as the same #RRGGBB key <see cref="Sample"/> counts with.
    /// A histogram cannot answer a geometry question: "did the rounded corner keep its child out" is about
    /// which colour sits at (0,0), not how many of each colour the crop holds. Coordinates are DIP pixels in
    /// the crop's own space, top-left origin.
    /// </summary>
    internal static uint PixelAt(FrameworkElement element, int x, int y)
    {
        var width = (int)element.ActualWidth;
        var height = (int)element.ActualHeight;
        if (width <= 0 || height <= 0)
        {
            throw new InvalidOperationException($"{Describe(element)} has no layout to sample.");
        }

        if (x < 0 || y < 0 || x >= width || y >= height)
        {
            throw new ArgumentOutOfRangeException(nameof(x), $"({x},{y}) is outside {width}x{height}.");
        }

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(element);
        var buffer = new byte[width * height * 4];
        bitmap.CopyPixels(buffer, width * 4, 0);
        var offset = (y * width + x) * 4;
        return (uint)(buffer[offset + 2] << 16 | buffer[offset + 1] << 8 | buffer[offset]);
    }

    internal static string Hex(uint key) => $"#{key:X6}";

    internal static string Hex(Color colour) => $"#{colour.A:X2}{colour.R:X2}{colour.G:X2}{colour.B:X2}";

    /// <summary>The page the light branch is measured on: one DIP of the app's own background colour.</summary>
    internal static Color LightPage => Color.FromRgb(0xF3, 0xF3, 0xF3);

    /// <summary>The page the dark branch is measured on, chosen so no translucent-white surface composite can imitate it.</summary>
    internal static Color DarkPage => Color.FromRgb(0x20, 0x20, 0x20);

    /// <summary>For a control that carries its surface on itself rather than on a named part.</summary>
    internal static FrameworkElement Self(FrameworkElement element) => element;

    /// <summary>
    /// An opaque plate for a subject whose token is translucent. A capture keeps only the colour bytes
    /// (<c>Bgr32</c>), so over nothing a translucent brush reports its own RGB and the claim cannot be falsified;
    /// over a plate it lands as <see cref="Over"/>, which is nameable in advance. The measurement is in
    /// <c>docs/astra/adaptation/06-pixel-attribution.md</c>, last section.
    /// </summary>
    internal static Border Backdrop(FrameworkElement subject, Color plate) => new()
    {
        Background = new SolidColorBrush(plate),
        Child = subject,
    };

    /// <summary>The colour an ink lands on when composited source-over an opaque plate, byte-quantised like the renderer.</summary>
    internal static Color Over(Color plate, Color ink)
    {
        static byte Blend(byte back, byte front, double alpha) => (byte)Math.Round(back + (front - back) * alpha);
        var a = ink.A / 255d;
        return Color.FromRgb(Blend(plate.R, ink.R, a), Blend(plate.G, ink.G, a), Blend(plate.B, ink.B, a));
    }

    /// <summary>
    /// Captures a freshly built subject on an opaque page and asserts that <b>its own</b> surface brush lands as the
    /// composite <see cref="Over"/> predicts, then hands the composite back so the caller can add the cross-theme leg.
    /// A two-theme pixel claim without this shape is not falsifiable: a capture keeps only the colour bytes, so over
    /// nothing a translucent brush reports its own RGB, and a shared RGB between two tokens (<c>adaptation/06</c>
    /// clause 4) lets one element's ink be counted as another's.
    /// </summary>
    internal static (Color Ink, Sample Sample) AssertSurfaceLands(
        FrameworkElement subject,
        Func<FrameworkElement, FrameworkElement?> surfaceOf,
        Color plate,
        int width,
        int height,
        int floor,
        string claim)
    {
        var host = Backdrop(subject, plate);
        Build(host, width, height);
        Settle(60);

        var surface = surfaceOf(subject) ?? throw new InvalidOperationException($"No surface for '{claim}': {Describe(subject)}.");
        var brush = BrushOf(surface) ?? throw new InvalidOperationException($"{Describe(surface)} carries no background brush.");
        var ink = Over(plate, brush.Color);

        Assert.True(ink != plate, $"The page and {Describe(surface)}'s brush composite to the same colour, so '{claim}' cannot be falsified.");
        var sample = Render(host, width, height);
        Assert.True(sample.Count(ink) >= floor,
            $"'{claim}': {Describe(surface)} carries {Hex(brush.Color)} which composites to {Hex(ink)} over {Hex(plate)}, "
            + $"but that colour lands {sample.Count(ink)} pixels, under the floor of {floor}. top={sample.Top(5)}");
        return (ink, sample);
    }

    /// <summary>
    /// The other side of the same instrument: a subject that upstream paints no surface at all has to leave the page
    /// intact, so "no ink here" becomes an assertion instead of an unmeasured hope.
    /// </summary>
    internal static void AssertNoSurfaceLands(FrameworkElement subject, Color plate, int width, int height, string claim)
    {
        var host = Backdrop(subject, plate);
        Build(host, width, height);
        Settle(60);
        var sample = Render(host, width, height);
        var surfaces = SurfaceBrushes(subject);

        Assert.True(surfaces.Count == 0,
            $"'{claim}' expected no surface brush, but {surfaces.Count} element(s) carry one: "
            + $"{string.Join(", ", surfaces.Select(entry => $"{Describe(entry.Element)}={Hex(entry.Colour)}"))}.");
        Assert.True(sample.Count(plate) > (width * height) - (width * height / 100),
            $"'{claim}': the page was covered although nothing should paint over it; left={sample.Count(plate)} of {width * height}. top={sample.Top(5)}");
    }

    private static SolidColorBrush? BrushOf(FrameworkElement element) => element switch
    {
        Border border => border.Background as SolidColorBrush,
        Panel panel => panel.Background as SolidColorBrush,
        Control control => control.Background as SolidColorBrush,
        _ => null,
    };

    /// <summary>
    /// The first element carrying <paramref name="name"/> that actually has a surface to attribute. A template can
    /// hand the same part name to several elements - a tree has one <c>ContentBorder</c> per row as well as its own -
    /// and the ones that stay transparent carry no claim.
    /// </summary>
    internal static FrameworkElement? NamedSurface(Visual root, string name)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is FrameworkElement { Name: var partName } element
                && partName == name
                && BrushOf(element) is { Color.A: > 0 })
            {
                return element;
            }

            if (child is Visual visual && NamedSurface(visual, name) is { } deeper)
            {
                return deeper;
            }
        }

        return null;
    }

    /// <summary>
    /// Every background brush a <b>built</b> subject carries, with the element that carries it. A surface claim reads
    /// the receiver's own property instead of naming a token, so it cannot be satisfied by a colour some neighbour
    /// happens to supply - the failure that <c>adaptation/06</c> clause 4 records for the scroll bar thumb.
    /// </summary>
    internal static List<(FrameworkElement Element, Color Colour)> SurfaceBrushes(Visual root)
    {
        var hits = new List<(FrameworkElement, Color)>();
        Collect(root);
        return hits;

        void Collect(Visual node)
        {
            var brush = node switch
            {
                Border border => border.Background,
                Panel panel => panel.Background,
                Control control => control.Background,
                Jalium.UI.Shapes.Shape shape => shape.Fill,
                _ => null,
            };

            if (brush is SolidColorBrush solid && solid.Color.A > 0 && node is FrameworkElement host)
            {
                hits.Add((host, solid.Color));
            }

            var count = VisualTreeHelper.GetChildrenCount(node);
            for (var index = 0; index < count; index++)
            {
                if (VisualTreeHelper.GetChild(node, index) is Visual child)
                {
                    Collect(child);
                }
            }
        }
    }

    /// <summary>
    /// The one shown host window, for a claim about the chrome it builds itself. Callers must not
    /// resize or replace its content as a side effect of measuring the shell.
    /// </summary>
    internal static Window HostWindow() => EnsureHost(420, 300);

    /// <summary>Closes the host. The fixture that owns the UI thread must call this before it exits.</summary>
    internal static void ReleaseHost()
    {
        _host?.Close();
        _host = null;
    }

    private static Window EnsureHost(int width, int height)
    {
        if (_host is not null)
        {
            if (_host.Width < width + 64) _host.Width = width + 64;
            if (_host.Height < height + 64) _host.Height = height + 64;
            return _host;
        }

        var window = new Window
        {
            Content = new Grid(),
            Width = Math.Max(420, width + 64),
            Height = Math.Max(300, height + 64),
            Title = "Astra pixel host",
        };
        window.Show();
        Pump(8);
        // The framework resolves a ContentDialog's host from the calling thread's Win32 active window and only
        // then from Application.MainWindow, and closing whatever window held activation leaves that handle at 0
        // (spike/HostWindowProbe). A test run opens and closes surfaces - popups among them - so without this
        // assignment every later dialog in the process throws "could not resolve a host window", which is what
        // #35 met as a whole class failing mid-suite. Only when null: a window the suite names stays named.
        if (Application.Current is { } application && application.MainWindow is null)
        {
            application.MainWindow = window;
        }

        _host = window;
        return window;
    }

    /// <summary>
    /// Re-renders until the picture stops changing, because a themed brush animates into place. A
    /// blank capture never counts as settled: an unstyled-at-yet control reads as uniform black, and
    /// treating that as stable is how the first harness mistook "not painted" for "theme ignored".
    /// </summary>
    private static Sample Capture(Func<Sample> grab)
    {
        var started = Stopwatch.GetTimestamp();
        var deadline = started + Stopwatch.Frequency * SettleBudgetMilliseconds / 1000;
        Sample? previous = null;
        var round = 0;
        for (round = 1; round <= MaxRounds; round++)
        {
            Pump(FramesPerRound);
            var current = grab();
            if (previous is not null && current.PaintedPixels > 0 && SamePicture(current, previous))
                return current with { Rounds = round, Stable = true, CaptureMilliseconds = Elapsed(started) };
            previous = current;
            if (Stopwatch.GetTimestamp() > deadline) break;
        }

        // The round count and the clock survive the failure now, because the two shapes of "not settled" read
        // identically without them: a picture that really keeps changing spends many rounds, while a picture that
        // is perfectly still but starved of frames runs out of the budget after one or two - and the second one is
        // this harness's own fault, not the subject's.
        return (previous ?? new Sample(0, 0, [])) with { Rounds = round, Stable = false, CaptureMilliseconds = Elapsed(started) };
    }

    private static int Elapsed(long started) => (int)Stopwatch.GetElapsedTime(started).TotalMilliseconds;

    /// <summary>What the style pipeline actually resolved for the subject, recorded while it is still in the tree.</summary>
    private static string Describe(FrameworkElement element) =>
        $"{(element.Style?.TargetType?.Name ?? "implicit")}/{(element as Control)?.Template?.GetType().Name ?? "no-template"}@{element.ActualWidth}x{element.ActualHeight}";

    private static bool SamePicture(Sample left, Sample right) =>
        left.Histogram.Count == right.Histogram.Count && !left.Histogram.Except(right.Histogram).Any();

    /// <summary>
    /// Pumps real rendered frames on the caller's dispatcher and reports how many arrived. The budget
    /// matters: once a scene is static the framework stops raising
    /// <see cref="CompositionTarget.Rendering"/>, so waiting for frames alone can block until the
    /// watchdog releases the frame. The watchdog has to be handed the dispatcher of the thread that
    /// pushed the frame: <c>Dispatcher.CurrentDispatcher</c> inside the timer callback resolves on a
    /// thread-pool thread, where it is an unrun dispatcher nobody pumps, so the release was queued and
    /// never executed. Measured as a 60-second fixture timeout on any focus change that starts no
    /// animation (a focused Slider) while controls that do animate returned in milliseconds.
    /// </summary>
    private static int Pump(int frames, int budgetMilliseconds = PumpBudgetMilliseconds)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var seen = 0;
        var deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * budgetMilliseconds / 1000;
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames || Stopwatch.GetTimestamp() > deadline) frame.Continue = false;
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        using var watchdog = new System.Threading.Timer(_ => dispatcher.InvokeAsync(() => frame.Continue = false));
        watchdog.Change(TimeSpan.FromMilliseconds(budgetMilliseconds * 2), System.Threading.Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
        return seen;
    }

    /// <summary>
    /// Pumps real rendered frames so a <c>TransitionProperty</c> has landed before a value is read back.
    /// Such a transition animates the dependency value itself, so mid-flight <c>Border.Width</c> reports an
    /// interpolated length and <c>Background</c> reports a brand-new interpolated brush rather than the
    /// palette instance (docs/astra/adaptation/12).
    /// <para>
    /// The return value is how many frames <em>arrived</em>, which is not what the argument asks for: the pump
    /// also stops when its wall-clock budget expires, so a call for 24 frames can deliver one on a loaded
    /// machine. That is #47's other member - a claim of the shape "the value changed across 24 frames" is
    /// measuring a stalled pump when the delivery is nought - so a caller that compares two reads takes them
    /// through <see cref="SettleFrames"/> instead.
    /// </para>
    /// </summary>
    internal static int Settle(int frames = 30) => Pump(frames);

    /// <summary>
    /// Pumps until at least <paramref name="minimum"/> real frames have been delivered, up to the same
    /// three-round budget a capture gives its settle question, and reports the count it reached. Used where the
    /// claim is about two reads taken across rendered time: with nothing rendered in between the comparison is
    /// not about the subject at all, and a stalled pump deserves an instrument's own failure rather than a
    /// control's.
    /// </summary>
    internal static int SettleFrames(int minimum)
    {
        var delivered = 0;
        var deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * SettleBudgetMilliseconds / 1000;
        while (delivered < minimum && Stopwatch.GetTimestamp() < deadline)
        {
            delivered += Pump(minimum - delivered);
        }

        return delivered;
    }

    private static Sample CaptureRaw(Visual target, int width, int height)
    {
        if (width <= 0 || height <= 0) return new Sample(0, 0, [], 0, false);

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(target);
        const int bytesPerPixel = 4;
        var stride = width * bytesPerPixel;
        var buffer = new byte[stride * height];
        bitmap.CopyPixels(new Int32Rect(0, 0, width, height), buffer, stride, 0);

        var histogram = new Dictionary<uint, int>();
        for (var offset = 0; offset + 3 < buffer.Length; offset += bytesPerPixel)
        {
            var key = (uint)(buffer[offset + 2] << 16 | buffer[offset + 1] << 8 | buffer[offset]);
            histogram[key] = histogram.GetValueOrDefault(key) + 1;
        }

        return new Sample(width, height, histogram, 0, false);
    }
}
