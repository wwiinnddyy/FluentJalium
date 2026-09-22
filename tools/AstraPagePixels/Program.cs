using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using FluentJalium.Gallery;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

namespace AstraPagePixels;

/// <summary>
/// "Every gallery page renders to pixels", measured in a process of its own. The subject is the shipping
/// <see cref="MainWindow" /> shown at its design size, and each page is captured twice per theme variant: once as
/// mounted, once with the page host emptied. The second frame is what makes the claim "this page painted these
/// pixels" rather than "the navigation pane painted a Fluent frame", which any page would satisfy.
///
/// Exit code is the verdict: 0 = every page passed, 1 = at least one did not, 2 = the harness could not measure.
/// Pass <c>--report</c> to print the readings without judging them, which is how the thresholds below were set.
/// </summary>
internal static class Program
{
    /// <summary>The framework's focused-border green, the colour this library is not allowed to paint with.</summary>
    private static readonly Color BrandFocusGreen = Color.FromRgb(0x1E, 0x79, 0x3F);

    private static readonly Color LightBase = Color.FromRgb(0xF3, 0xF3, 0xF3);
    private static readonly Color DarkBase = Color.FromRgb(0x20, 0x20, 0x20);

    /// <summary>
    /// Floors under the minima of the 2026-09-22 <c>--report</c> run of the shown window (26 legs):
    /// window-lit 848,082..848,166 of 848,166, own base fill 197,022..257,809, slot painted 182,017..1,531,859,
    /// slot colours 48..404, empty slot 1 colour and 0 lit pixels.
    /// </summary>
    private const int MinPaintedPixels = 800_000;
    private const int MinBaseFillPixels = 150_000;
    private const int MinSlotPaintedPixels = 150_000;
    private const int MinSlotDistinctColors = 40;

    [STAThread]
    private static int Main(string[] args)
    {
        var report = args.Any(static argument =>
            string.Equals(argument, "--report", StringComparison.OrdinalIgnoreCase));

        var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
        renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
        ThemeLoader.Initialize();

        var application = new Application();
        FluentThemeManager.Apply(application, FluentThemeVariant.Light);

        var pages = PageIds();
        if (pages.Count == 0)
        {
            Console.Error.WriteLine("Catalog.json listed no pages, so there is nothing to measure.");
            return 2;
        }

        var window = new MainWindow();
        application.MainWindow = window;
        window.Show();
        window.Activate();
        Pump(30);

        var failures = new List<string>();
        foreach (var variant in new[] { FluentThemeVariant.Light, FluentThemeVariant.Dark })
        {
            FluentThemeManager.ApplyTheme(variant);
            foreach (var id in pages)
            {
                window.NavigateToPage(id);
                Pump(24);
                var host = PageHostOf(window);
                var pageElement = host.Children.Count > 0 ? host.Children[0] : null;
                var whole = CaptureStable((FrameworkElement)window.Content!);
                var slot = CaptureStable(host);

                if (pageElement is not null) host.Children.Remove(pageElement);
                Pump(24);
                var emptySlot = CaptureStable(host);
                if (pageElement is not null) host.Children.Add(pageElement);

                Report(id, variant, whole, slot, emptySlot);
                if (!report) Judge(id, variant, whole, slot, emptySlot, failures);
            }
        }

        window.Close();
        foreach (var failure in failures) Console.Error.WriteLine(failure);
        Console.WriteLine($"{(failures.Count == 0 ? "PASS" : "FAIL")} {pages.Count} pages x 2 variants" +
            (report ? " (report only)" : $", {failures.Count} offender(s)"));
        return failures.Count == 0 ? 0 : 1;
    }

    private static void Report(string id, FluentThemeVariant variant, Sample whole, Sample slot, Sample emptySlot)
    {
        Console.WriteLine($"{id} {variant}: stable={whole.Stable}/{slot.Stable} painted={Painted(whole.Histogram)} " +
            $"base={Count(whole.Histogram, BaseOf(variant))} other-base={Count(whole.Histogram, BaseOf(Other(variant)))} " +
            $"green={Count(whole.Histogram, BrandFocusGreen)} | slot {slot.Histogram.Count} colours " +
            $"{Painted(slot.Histogram)}px over {emptySlot.Histogram.Count} colours {Painted(emptySlot.Histogram)}px " +
            $"(slot {slot.Width}x{slot.Height})");
    }

    private static void Judge(string id, FluentThemeVariant variant, Sample whole, Sample slot, Sample emptySlot,
        List<string> failures)
    {
        if (!whole.Stable) failures.Add($"{id} {variant}: the frame never settled");
        if (Painted(whole.Histogram) < MinPaintedPixels)
            failures.Add($"{id} {variant}: only {Painted(whole.Histogram)} of the frame is lit");
        if (Count(whole.Histogram, BaseOf(variant)) < MinBaseFillPixels)
            failures.Add($"{id} {variant}: the variant's own base fill covers {Count(whole.Histogram, BaseOf(variant))} pixels");
        if (Count(whole.Histogram, BaseOf(Other(variant))) != 0)
            failures.Add($"{id} {variant}: the other variant's base fill is still in the frame " +
                $"({Count(whole.Histogram, BaseOf(Other(variant)))} pixels)");
        if (Count(whole.Histogram, BrandFocusGreen) != 0)
            failures.Add($"{id} {variant}: {Count(whole.Histogram, BrandFocusGreen)} pixels of the framework's brand green");

        // The slot is the scroll extent, not the viewport: it carries content the user cannot see, so a page with a
        // running animation there never repeats a frame. Measured on the status page in both variants - its
        // ProgressRing ticks below the fold, and the same window capture does settle because the ring is off-screen.
        // Stability is therefore claimed for the viewport only, and the slot is judged on what it paints.
        if (slot.Width <= 0 || slot.Height <= 0) failures.Add($"{id} {variant}: the page slot has no size");
        if (Painted(emptySlot.Histogram) != 0)
            failures.Add($"{id} {variant}: the slot was not actually emptied ({Painted(emptySlot.Histogram)} lit pixels left)");
        if (Painted(slot.Histogram) < MinSlotPaintedPixels)
            failures.Add($"{id} {variant}: the page painted {Painted(slot.Histogram)} pixels into its slot");
        if (slot.Histogram.Count <= emptySlot.Histogram.Count)
            failures.Add($"{id} {variant}: the page printed nothing its empty slot does not already print " +
                $"({slot.Histogram.Count} colours against {emptySlot.Histogram.Count})");
        if (slot.Histogram.Count < MinSlotDistinctColors)
            failures.Add($"{id} {variant}: the page slot holds {slot.Histogram.Count} colours, a plate not a page");
    }

    private static Sample CaptureStable(FrameworkElement element)
    {
        Dictionary<uint, int>? previous = null;
        var width = 0;
        var height = 0;
        for (var round = 0; round < MaxSettleRounds; round++)
        {
            Pump(12);
            var current = Capture(element);
            width = current.Width;
            height = current.Height;
            if (previous is not null && SamePicture(previous, current.Histogram))
                return new Sample(current.Histogram, width, height, true);
            previous = current.Histogram;
        }

        return new Sample(previous ?? [], width, height, false);
    }

    private readonly record struct Sample(Dictionary<uint, int> Histogram, int Width, int Height, bool Stable);

    private const int MaxSettleRounds = 10;

    private static bool SamePicture(Dictionary<uint, int> left, Dictionary<uint, int> right) =>
        left.Count == right.Count && !left.Except(right).Any();

    private static (Dictionary<uint, int> Histogram, int Width, int Height) Capture(FrameworkElement element)
    {
        var width = (int)element.ActualWidth;
        var height = (int)element.ActualHeight;
        if (width <= 0 || height <= 0) return ([], 0, 0);

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(element);
        var buffer = new byte[width * 4 * height];
        bitmap.CopyPixels(new Int32Rect(0, 0, width, height), buffer, width * 4, 0);

        var histogram = new Dictionary<uint, int>();
        for (var offset = 0; offset + 3 < buffer.Length; offset += 4)
        {
            var key = (uint)(buffer[offset + 2] << 16 | buffer[offset + 1] << 8 | buffer[offset]);
            histogram[key] = histogram.GetValueOrDefault(key) + 1;
        }

        return (histogram, width, height);
    }

    private static int Painted(Dictionary<uint, int> histogram) => histogram.Where(static entry => entry.Key != 0).Sum(static entry => entry.Value);

    private static int Count(Dictionary<uint, int> histogram, Color color) =>
        histogram.GetValueOrDefault((uint)(color.R << 16 | color.G << 8 | color.B));

    private static Color BaseOf(FluentThemeVariant variant) => variant == FluentThemeVariant.Dark ? DarkBase : LightBase;

    private static FluentThemeVariant Other(FluentThemeVariant variant) =>
        variant == FluentThemeVariant.Dark ? FluentThemeVariant.Light : FluentThemeVariant.Dark;

    /// <summary>The panel the Gallery mounts pages on, through its code-behind's own private view.</summary>
    private static Panel PageHostOf(MainWindow window) =>
        (Panel)(typeof(MainWindow).GetProperty("ContentHost", BindingFlags.Instance | BindingFlags.NonPublic)?
            .GetValue(window) ?? throw new InvalidOperationException("MainWindow lost its ContentHost view."));

    private static List<string> PageIds()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Catalog.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.GetProperty("pages").EnumerateArray()
            .Select(static page => page.GetProperty("id").GetString()!)
            .ToList();
    }

    /// <summary>Real rendered frames, because nothing paints until the dispatcher produces them.</summary>
    private static void Pump(int frames)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var seen = 0;
        var deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * PumpBudgetMilliseconds / 1000;
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames || Stopwatch.GetTimestamp() > deadline) frame.Continue = false;
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        using var watchdog = new System.Threading.Timer(_ => dispatcher.InvokeAsync(() => frame.Continue = false));
        watchdog.Change(PumpBudgetMilliseconds, System.Threading.Timeout.Infinite);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
    }

    /// <summary>
    /// A static scene stops raising <see cref="CompositionTarget.Rendering"/> on this runtime, so a pump that asks
    /// for frames it will never get has to be released by the clock. The harness uses a budget of the same order;
    /// a 20 s one turned every settle loop into a minute and the whole probe into a timeout.
    /// </summary>
    private const int PumpBudgetMilliseconds = 900;
}
