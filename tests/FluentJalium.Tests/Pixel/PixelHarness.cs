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

    internal sealed record Sample(int Width, int Height, Dictionary<uint, int> Histogram, int Rounds = 0, bool Stable = false, string Subject = "")
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
        element.Width = width;
        element.Height = height;
        var window = EnsureHost(width, height);
        var root = new Grid();
        root.Children.Add(element);
        window.Content = root;
        if (element is Control control) control.ApplyTemplate();
        window.UpdateLayout();

        var sample = Capture(() => CaptureRaw(element, (int)element.ActualWidth, (int)element.ActualHeight));
        return sample with { Subject = Describe(element) };
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
        return Capture(() => CaptureRaw(window, (int)window.ActualWidth, (int)window.ActualHeight));
    }

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
        var deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 4 / 10;
        Sample? previous = null;
        for (var round = 1; round <= MaxRounds; round++)
        {
            Pump(FramesPerRound);
            var current = grab();
            if (previous is not null && current.PaintedPixels > 0 && SamePicture(current, previous))
                return current with { Rounds = round, Stable = true };
            previous = current;
            if (Stopwatch.GetTimestamp() > deadline) break;
        }

        return previous ?? new Sample(0, 0, [], 0, false);
    }

    /// <summary>What the style pipeline actually resolved for the subject, recorded while it is still in the tree.</summary>
    private static string Describe(FrameworkElement element) =>
        $"{(element.Style?.TargetType?.Name ?? "implicit")}/{(element as Control)?.Template?.GetType().Name ?? "no-template"}@{element.ActualWidth}x{element.ActualHeight}";

    private static bool SamePicture(Sample left, Sample right) =>
        left.Histogram.Count == right.Histogram.Count && !left.Histogram.Except(right.Histogram).Any();

    /// <summary>
    /// Pumps real rendered frames on the caller's dispatcher and reports how many arrived. The budget
    /// matters: once a scene is static the framework stops raising
    /// <see cref="CompositionTarget.Rendering"/>, so waiting for frames alone can block until the
    /// watchdog releases the frame.
    /// </summary>
    private static int Pump(int frames, int budgetMilliseconds = 400)
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
        return seen;
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
