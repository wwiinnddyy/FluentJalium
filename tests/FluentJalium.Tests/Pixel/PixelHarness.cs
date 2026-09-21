using System.Diagnostics;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

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
    /// </summary>
    internal static void Settle(int frames = 30) => Pump(frames);

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
