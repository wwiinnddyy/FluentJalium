using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;

namespace FluentJalium.Tests.Pixel;

/// <summary>
/// In-process pixel capture, so "the theme reached the pixels" is an assertion rather than a
/// screenshot somebody eyeballed. <see cref="RenderTargetBitmap"/> holds Bgr32, so bytes are B,G,R;
/// histograms are keyed by packed R,G,B to keep the test code readable.
/// </summary>
/// <remarks>
/// Measured on 26.10.9: capturing a visual directly works for shapes and for self-drawn controls
/// that are not in a window, but a templated control captured on its own comes back empty even
/// after it has been shown, loaded and laid out — only the window rasterises. Use
/// <see cref="Host"/> for anything with a template.
/// </remarks>
internal static class PixelHarness
{
    internal sealed record Sample(int Width, int Height, Dictionary<uint, int> Histogram)
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

    /// <summary>Lays the element out at the given size and captures it in its own coordinate space.</summary>
    internal static Sample Render(FrameworkElement element, int width, int height)
    {
        element.Width = width;
        element.Height = height;
        if (element is Control control) control.ApplyTemplate();
        element.Measure(new Size(width, height));
        element.Arrange(new Rect(0, 0, width, height));
        element.UpdateLayout();
        return Capture(element, width, height);
    }

    /// <summary>
    /// Captures a control through a throwaway window, which is the only way a templated control
    /// rasterises. The window is shown and closed again on the caller's thread; the sample covers
    /// the whole window, so assert on colours rather than on a cropped region.
    /// </summary>
    internal static Sample Host(Visual element, int width = 320, int height = 200)
    {
        var window = new Window { Content = element, Width = width, Height = height, Title = "Astra pixel host" };
        try
        {
            window.Show();
            window.UpdateLayout();
            return Capture(window, width, height);
        }
        finally
        {
            window.Close();
        }
    }

    private static Sample Capture(Visual target, int width, int height)
    {
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(target);
        const int bytesPerPixel = 4;
        var stride = width * bytesPerPixel;
        var buffer = new byte[stride * height];
        bitmap.CopyPixels(new Int32Rect(0, 0, width, height), buffer, stride, 0);

        var histogram = new Dictionary<uint, int>();
        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                var offset = (row * stride) + (column * bytesPerPixel);
                var key = (uint)(buffer[offset + 2] << 16 | buffer[offset + 1] << 8 | buffer[offset]);
                histogram[key] = histogram.TryGetValue(key, out var count) ? count + 1 : 1;
            }
        }

        return new Sample(width, height, histogram);
    }
}
