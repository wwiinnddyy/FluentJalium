using System.Reflection;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

namespace RatingProbe;

/// <summary>
/// Three questions that decide the RatingControl slice before any of it is written, asked of the pinned 26.10.9
/// runtime in-process (offline reflection into the shipped Controls assembly reads zero types - it is a
/// type-forwarding shell).
/// </summary>
/// <remarks>
/// 1. Is there anything native to style? Upstream's `RatingControl` is a `Control` with two item panels and a
///    `RatingItemInfo` object graph; the census only recorded "no type with this name" for the control itself.
/// 2. Does `StackPanel.Spacing` exist and survive markup? Upstream's item pitch is `A + S`, and the audit's open
///    contradiction (§10 in audits/rating-control.md) is between a pitch of 16 (template margins cancelling an
///    8 spacing) and 24 (the width/pointer maths). Which side our template lands on depends on whether spacing is
///    even available - and an attribute this runtime does not have parses clean and does nothing, so the only
///    reading that counts is the value back off a mounted panel.
/// 3. Can the half-star be cut at all? Upstream clips the foreground layer with one `RectangleGeometry` per item
///    (`RatingControl.cpp:336-366`). Two routes are measured: `UIElement.Clip` with a rect geometry, and the
///    route the ProgressBar adaptation ended up using - a `ClipToBounds` host sized to the fraction. Glyph ink
///    reaches neither capture path (adaptation/00 S1-r clause 3), so the clip probe deliberately uses a solid
///    colour block: the question is whether *any* ink is removable, not whether a star shows.
/// </remarks>
internal static class Program
{
    private static readonly List<string> Lines = [];

    private static readonly Color Accent = Color.FromRgb(0x00, 0x78, 0xD4);
    private static readonly Color Rest = Color.FromRgb(0x11, 0x22, 0x33);

    private const string Root =
        "xmlns=\"http://schemas.jalium.ui/2024\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"";

    private static Grid _root = new();

    [STAThread]
    private static int Main()
    {
        try
        {
            var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
            renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            var application = new Application();
            FluentThemeManager.Apply(application);
            var window = new Window { Width = 320, Height = 240, Content = _root };
            window.Show();
            Pump(8);
            Types();
            Members();
            Spacing();
            Clip();
        }
        catch (Exception exception)
        {
            Note("BOOT", "threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }

        var path = Path.Combine(AppContext.BaseDirectory, "rating-probe.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        foreach (var line in Lines)
        {
            Console.WriteLine(line);
        }

        return 0;
    }

    private static void Types()
    {
        var controls = typeof(Button).Assembly;
        Note("types", $"Button assembly = {controls.GetName().Name} {controls.GetName().Version}, " +
            $"{SafeTypes(controls).Count(static type => type.IsPublic)} public types");
        foreach (var name in new[]
                 {
                     "RatingControl", "Rating", "RatingItemInfo", "RatingItemFontInfo", "RatingItemImageInfo",
                     "SymbolIcon", "FontIcon", "BitmapIcon", "PathIcon", "Image",
                 })
        {
            var found = controls.GetType("Jalium.UI.Controls." + name) ?? controls.GetType(name);
            Note("types", $"Jalium.UI.Controls.{name} -> {(found is null ? "ABSENT" : found.FullName)}");
        }

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var anywhere = assemblies.SelectMany(SafeTypes)
            .Where(static type => type.IsPublic && type.Name.Contains("Rating", StringComparison.OrdinalIgnoreCase))
            .Select(static type => type.FullName ?? type.Name)
            .OrderBy(static text => text)
            .ToList();
        Note("types", $"any public *Rating* type across {assemblies.Length} loaded assemblies: {anywhere.Count} " +
            $"[{string.Join(", ", anywhere.Take(12))}]");
    }

    private static void Members()
    {
        ProbeType(typeof(StackPanel), "Spacing", "Orientation");
        ProbeType(typeof(UIElement), "Clip", "ClipToBounds");
        ProbeType(typeof(RectangleGeometry), "Rect");
        ProbeType(typeof(Geometry), "Bounds");
        ProbeType(typeof(FrameworkElement), "RenderTransform");
        ProbeType(typeof(Control), "IsHitTestVisible", "Focusable", "FontFamily", "FontSize", "Foreground", "Template");
        ProbeType(typeof(Border), "Background", "Child");
        ProbeType(typeof(TextBlock), "Text", "Foreground");
        Note("members", $"typeof(UIElement).GetProperty(\"Clip\").PropertyType = " +
            $"{typeof(UIElement).GetProperty("Clip")?.PropertyType.Name ?? "<null>"}");
    }

    private static void ProbeType(Type type, params string[] names)
    {
        foreach (var name in names)
        {
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Note("members", $"{type.Name}.{name} -> " +
                (property is null ? "ABSENT" : property.DeclaringType?.Name + "." + name + ":" + property.PropertyType.Name));
        }
    }

    private static void Spacing()
    {
        // Read back off a mounted panel: an attribute this runtime does not have parses clean and does nothing, so
        // a green load is not evidence the spacing applied. Pitch is measured as the DELTA between two cells, not
        // as one cell's absolute offset - the first version of this line read cell1.X only and reported a pitch
        // that was really cell0's own margin.
        foreach (var (label, spacing) in new[] { ("unset", (double?)null), ("8", (double?)8), ("24", (double?)24), ("-10", (double?)-10) })
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            if (spacing is { } value)
            {
                panel.Spacing = value;
            }

            AddCells(panel);
            Mount(panel);
            var pitch = panel.Children.Count > 1
                ? panel.Children[1]!.TranslatePoint(new Point(0, 0), panel).X - panel.Children[0]!.TranslatePoint(new Point(0, 0), panel).X
                : double.NaN;
            Note("spacing", $"code {label}: read-back={panel.Spacing} desired={panel.DesiredSize.Width:0.##}x{panel.DesiredSize.Height:0.##} " +
                $"pitch={pitch:0.###}");
            Unmount(panel);
        }

        var parsed = LoadElement(
            "<StackPanel Orientation=\"Horizontal\" Spacing=\"8\">" +
            "<Border Width=\"16\" Height=\"16\" Background=\"#FF0078D4\"/>" +
            "<Border Width=\"16\" Height=\"16\" Background=\"#FF112233\"/>" +
            "<Border Width=\"16\" Height=\"16\" Background=\"#FF112233\"/>" +
            "</StackPanel>");
        if (parsed is StackPanel markupPanel)
        {
            Mount(markupPanel);
            var markupPitch = markupPanel.Children.Count > 1
                ? markupPanel.Children[1]!.TranslatePoint(new Point(0, 0), markupPanel).X -
                  markupPanel.Children[0]!.TranslatePoint(new Point(0, 0), markupPanel).X
                : double.NaN;
            Note("spacing", $"markup Orientation=Horizontal Spacing=\"8\": type={parsed.GetType().Name} " +
                $"orientation={markupPanel.Orientation} read-back={markupPanel.Spacing} " +
                $"desired={markupPanel.DesiredSize.Width:0.##}x{markupPanel.DesiredSize.Height:0.##} pitch={markupPitch:0.###}");
            Unmount(markupPanel);
        }
        else
        {
            Note("spacing", "markup Spacing=\"8\" -> " + (parsed is null ? "LOAD-FAIL/null" : parsed.GetType().FullName));
        }

        // The other half of upstream's pitch: negative margins on the cells, which is what the checked template
        // actually does (-8,-8,0,0 on the glyph). Does a negative margin pull the next cell in, or is it clamped?
        var marginPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        for (var index = 0; index < 3; index++)
        {
            marginPanel.Children.Add(new Border
            {
                Width = 16,
                Height = 16,
                Background = index == 0 ? new SolidColorBrush(Accent) : new SolidColorBrush(Rest),
                Margin = new Thickness(-8, -8, 0, 0),
            });
        }

        Mount(marginPanel);
        var marginPitch = marginPanel.Children[1]!.TranslatePoint(new Point(0, 0), marginPanel).X -
            marginPanel.Children[0]!.TranslatePoint(new Point(0, 0), marginPanel).X;
        Note("spacing", $"pitch with Spacing=8 + Margin=-8,-8,0,0: pitch={marginPitch:0.###} " +
            $"cell0.X={marginPanel.Children[0]!.TranslatePoint(new Point(0, 0), marginPanel).X:0.###} " +
            $"desired={marginPanel.DesiredSize.Width:0.##}");
        Unmount(marginPanel);

        // The shape the fix needs: keep the published positive spacing and take the excess back off with a left
        // margin on every cell after the first. Does the margin subtract from the pitch, and does the panel's own
        // desired width follow (a row that overhangs its host is a different answer)?
        var compensated = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        for (var index = 0; index < 5; index++)
        {
            compensated.Children.Add(new Border
            {
                Width = 34,
                Height = 34,
                Background = index == 0 ? new SolidColorBrush(Accent) : new SolidColorBrush(Rest),
                Margin = index == 0 ? new Thickness() : new Thickness(-18, 0, 0, 0),
            });
        }

        Mount(compensated);
        var compensatedPitch = compensated.Children[1]!.TranslatePoint(new Point(0, 0), compensated).X -
            compensated.Children[0]!.TranslatePoint(new Point(0, 0), compensated).X;
        Note("spacing", $"pitch with Spacing=8 + cells 34 wide, later cells Margin.Left=-18: pitch={compensatedPitch:0.###} " +
            $"cell4.X={compensated.Children[4]!.TranslatePoint(new Point(0, 0), compensated).X:0.###} " +
            $"desired={compensated.DesiredSize.Width:0.##} (5*34+4*8={5 * 34 + 4 * 8}, compensated={4 * 24 + 34:0})");
        Unmount(compensated);

        // What _itemAdvance really reads: the star run's own width at the rendering size, which is the number the
        // compensation is built from.
        foreach (var (glyph, name) in new[] { ("\uE735", "set"), ("\uE734", "unset") })
        {
            var run = new TextBlock
            {
                FontFamily = new FontFamily("Segoe Fluent Icons"),
                FontSize = 32,
                Text = glyph,
            };
            Mount(run);
            Note("spacing", $"Segoe Fluent Icons 32 {name} glyph: measured={run.DesiredSize.Width:0.###}x{run.DesiredSize.Height:0.###} " +
                $"arranged={run.ActualWidth:0.###}x{run.ActualHeight:0.###}");
            Unmount(run);
        }
    }

    private static void Clip()
    {
        TransformShrinksInk();
        ClipItems();
    }

    /// <summary>Upstream draws each star at 32 and lets its own "default scale down" bring it back to the 16 the
    /// width model counts in (RatingControl_themeresources.xaml:38-46 says so in the comments on the two rows). This
    /// runtime has no such step, so the adaptation is an explicit 0.5 RenderTransform - which is only a fix if a
    /// transform really moves ink. Glyph ink reaches no capture (S1-r clause 3), so the question is settled on a
    /// solid block.</summary>
    private static void TransformShrinksInk()
    {
        var plain = new Border { Width = 60, Height = 60, Background = new SolidColorBrush(Accent) };
        Mount(plain);
        Note("transform", $"control, no transform: accent={Count(plain, Accent)} painted={Painted(plain)}");
        Unmount(plain);

        var scaled = new Border
        {
            Width = 60,
            Height = 60,
            Background = new SolidColorBrush(Accent),
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new ScaleTransform(0.5, 0.5),
        };
        Mount(scaled);
        Note("transform",
            $"origin read-back={scaled.RenderTransformOrigin} type={scaled.RenderTransform?.GetType().Name}: " +
            $"accent={Count(scaled, Accent)} painted={Painted(scaled)} (900 if the 0.5 reaches the ink, 3600 if not)");
        Unmount(scaled);

        // A capture of the target is sized to the target, so it can be blind to a transform. The same block seen
        // through a wider ancestor settles which of the two readings above is the truth: at 0.5 about the centre the
        // ink of a box at 0..60 must span 15..45, and 0..60 means the transform never reached the render.
        var host = new Grid { Width = 200, Height = 200, Background = new SolidColorBrush(Rest) };
        host.Children.Add(new Border
        {
            Width = 60,
            Height = 60,
            Background = new SolidColorBrush(Accent),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new ScaleTransform(0.5, 0.5),
        });
        Mount(host);
        Note("transform", $"60x60 at 0.5 seen through a 200x200 ancestor: {ExtentOf(host, Accent)} accent={Count(host, Accent)}");
        Unmount(host);

        var unscaledHost = new Grid { Width = 200, Height = 200, Background = new SolidColorBrush(Rest) };
        unscaledHost.Children.Add(new Border
        {
            Width = 60,
            Height = 60,
            Background = new SolidColorBrush(Accent),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        });
        Mount(unscaledHost);
        Note("transform", $"same box with no transform, same ancestor: {ExtentOf(unscaledHost, Accent)} accent={Count(unscaledHost, Accent)}");
        Unmount(unscaledHost);
    }

    /// <summary>The ink box of one colour inside a captured ancestor: "x=min..max y=min..max", or "none".</summary>
    private static string ExtentOf(FrameworkElement ancestor, Color color)
    {
        var width = (int)Math.Round(ancestor.ActualWidth);
        var height = (int)Math.Round(ancestor.ActualHeight);
        var key = (uint)(color.R << 16 | color.G << 8 | color.B);
        var buffer = Grab(ancestor, width, height);
        var stride = width * 4;
        var minX = int.MaxValue;
        var maxX = -1;
        var minY = int.MaxValue;
        var maxY = -1;
        for (var offset = 0; offset + 3 < buffer.Length; offset += 4)
        {
            if ((uint)(buffer[offset] | buffer[offset + 1] << 8 | buffer[offset + 2] << 16) != key)
            {
                continue;
            }

            var x = offset % stride / 4;
            var y = offset / stride;
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);
        }

        return maxX < 0 ? "none" : $"x={minX}..{maxX} y={minY}..{maxY}";
    }

    private static void ClipItems()
    {
        var plain = new Border { Width = 60, Height = 60, Background = new SolidColorBrush(Accent) };
        Mount(plain);
        Note("clip", $"control, no clip: accent={Count(plain, Accent)}/{Count(plain, Rest)} painted={Painted(plain)}");

        var codeClipped = new Border
        {
            Width = 60,
            Height = 60,
            Background = new SolidColorBrush(Accent),
            Clip = new RectangleGeometry { Rect = new Rect(0, 0, 30, 60) },
        };
        Mount(codeClipped);
        Note("clip", $"code RectangleGeometry(0,0,30,60): accent={Count(codeClipped, Accent)} " +
            $"clip={codeClipped.Clip?.GetType().Name ?? "null"} " +
            $"rect={((RectangleGeometry?)codeClipped.Clip)?.Rect.ToString() ?? "-"}");

        var fromMarkup = LoadElement(
            "<Border Width=\"60\" Height=\"60\" Background=\"#FF0078D4\">" +
            "<Border.Clip><RectangleGeometry Rect=\"0,0,30,60\"/></Border.Clip></Border>");
        if (fromMarkup is Border markupBorder)
        {
            Mount(markupBorder);
            Note("clip", $"markup <RectangleGeometry Rect=\"0,0,30,60\"/>: accent={Count(markupBorder, Accent)} " +
                $"clip={markupBorder.Clip?.GetType().Name ?? "null"} " +
                $"rect={((RectangleGeometry?)markupBorder.Clip)?.Rect.ToString() ?? "-"}");
            Unmount(markupBorder);
        }
        else
        {
            Note("clip", "markup clip element -> " + (fromMarkup is null ? "LOAD-FAIL/null" : fromMarkup.GetType().FullName));
        }

        // Is the markup reading above a spelling artifact of the comma form? Space-separated is the other shape
        // the parser accepts elsewhere, and an attribute whose value never lands is a permanent no-op.
        var spaceForm = LoadElement(
            "<Border Width=\"60\" Height=\"60\" Background=\"#FF0078D4\">" +
            "<Border.Clip><RectangleGeometry Rect=\"0 0 30 60\"/></Border.Clip></Border>");
        if (spaceForm is Border spaceBorder)
        {
            Mount(spaceBorder);
            Note("clip", $"markup Rect=\"0 0 30 60\" (spaces): accent={Count(spaceBorder, Accent)} " +
                $"rect={((RectangleGeometry?)spaceBorder.Clip)?.Rect.ToString() ?? "-"}");
            Unmount(spaceBorder);
        }
        else
        {
            Note("clip", "markup space form -> " + (spaceForm is null ? "LOAD-FAIL/null" : spaceForm.GetType().FullName));
        }

        var clippedText = new Border
        {
            Width = 60,
            Height = 60,
            Background = new SolidColorBrush(Accent),
            Clip = new RectangleGeometry { Rect = new Rect(0, 0, 30, 60) },
            Child = new TextBlock { Text = "\uE735\uE735\uE735", FontSize = 16, Foreground = new SolidColorBrush(Rest) },
        };
        Mount(clippedText);
        Note("clip", $"glyph layer under the same clip: accent={Count(clippedText, Accent)} rest={Count(clippedText, Rest)} " +
            $"(S1-r clause 3 predicts the rest count is 0 either way)");
        Unmount(clippedText);

        // The route the ProgressBar adaptation uses: a ClipToBounds host sized to the fraction, child left full width.
        // The capture has to be of an ANCESTOR wider than the host: RenderTargetBitmap is sized to the target it is
        // handed, so capturing the host itself cuts the ink at the host's own edge and reads "clipped" whether or
        // not ClipToBounds did anything. First version of this line made that mistake.
        foreach (var (label, width, clipToBounds) in new[]
                 { ("clip=false 30 of 60", 30, false), ("clip=true 30 of 60", 30, true), ("clip=true 45 of 60", 45, true) })
        {
            var outer = new Grid { Width = 200, Height = 60 };
            var host = new Grid { Width = width, Height = 60, ClipToBounds = clipToBounds, HorizontalAlignment = HorizontalAlignment.Left };
            host.Children.Add(new Border
            {
                Width = 60,
                Height = 60,
                Background = new SolidColorBrush(Accent),
                HorizontalAlignment = HorizontalAlignment.Left,
            });
            outer.Children.Add(host);
            Mount(outer);
            Note("clip", $"ClipToBounds {label}: accentInAncestor={Count(outer, Accent)} hostActual={host.ActualWidth:0.#}");
            Unmount(outer);
        }

        // Same route, but with everything the template will do: ClipToBounds and the fractional width set from
        // markup, the inner StackPanel carrying the item pitch.
        var markupHost = LoadElement(
            "<Grid Width=\"20\" Height=\"16\" ClipToBounds=\"True\">" +
            "<StackPanel Orientation=\"Horizontal\" Spacing=\"0\">" +
            "<Border Width=\"16\" Height=\"16\" Background=\"#FF0078D4\"/>" +
            "<Border Width=\"16\" Height=\"16\" Background=\"#FF0078D4\"/>" +
            "</StackPanel></Grid>");
        if (markupHost is Grid parsedMarkupHost)
        {
            var markupOuter = new Grid { Width = 200, Height = 16 };
            markupOuter.Children.Add(parsedMarkupHost);
            Mount(markupOuter);
            Note("clip", $"markup ClipToBounds=True host 20 over two 16 cells: accentInAncestor={Count(markupOuter, Accent)} " +
                $"clipToBounds={parsedMarkupHost.ClipToBounds} actual={parsedMarkupHost.ActualWidth:0.#}x{parsedMarkupHost.ActualHeight:0.#} " +
                $"(32x16 of ink = 512, cut at 20 => 320 if the clip reaches)");
            Unmount(markupOuter);
        }
        else
        {
            Note("clip", "markup ClipToBounds host -> " + (markupHost is null ? "LOAD-FAIL/null" : markupHost.GetType().FullName));
        }

        Unmount(plain);
    }

    private static void AddCells(StackPanel panel)
    {
        for (var index = 0; index < 3; index++)
        {
            panel.Children.Add(new Border
            {
                Width = 16,
                Height = 16,
                Background = new SolidColorBrush(index == 0 ? Accent : Rest),
            });
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

    private static int Count(FrameworkElement target, Color color)
    {
        var width = (int)Math.Round(target.ActualWidth);
        var height = (int)Math.Round(target.ActualHeight);
        if (width <= 0 || height <= 0)
        {
            return -1;
        }

        var key = (uint)(color.R << 16 | color.G << 8 | color.B);
        var buffer = Grab(target, width, height);
        var count = 0;
        for (var offset = 0; offset + 3 < buffer.Length; offset += 4)
        {
            if ((uint)(buffer[offset + 2] << 16 | buffer[offset + 1] << 8 | buffer[offset]) == key)
            {
                count++;
            }
        }

        return count;
    }

    private static int Painted(FrameworkElement target)
    {
        var width = (int)Math.Round(target.ActualWidth);
        var height = (int)Math.Round(target.ActualHeight);
        if (width <= 0 || height <= 0)
        {
            return -1;
        }

        var buffer = Grab(target, width, height);
        var count = 0;
        for (var offset = 0; offset + 3 < buffer.Length; offset += 4)
        {
            if (buffer[offset] != 0 || buffer[offset + 1] != 0 || buffer[offset + 2] != 0)
            {
                count++;
            }
        }

        return count;
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

    private static FrameworkElement? LoadElement(string markup)
    {
        var file = Path.Combine(AppContext.BaseDirectory, "probe-element.jalxaml");
        File.WriteAllText(file, $"<Grid {Root}>{markup}</Grid>");
        try
        {
            using var stream = File.OpenRead(file);
            if (XamlReader.Load(stream) is not Grid grid || grid.Children.Count == 0)
            {
                Note("load", "root was not a Grid with one child");
                return null;
            }

            // Detach before handing it out: a child that already has a parent cannot be mounted again.
            var child = grid.Children[0];
            grid.Children.RemoveAt(0);
            return child as FrameworkElement;
        }
        catch (Exception exception)
        {
            Note("load", "threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            return null;
        }
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

    private static IEnumerable<Type> SafeTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(static type => type is not null)!;
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static string Trim(string? text) => (text ?? string.Empty).Replace('\n', ' ').Replace('\r', ' ').Trim();

    private static void Note(string tag, string message) => Lines.Add($"{tag}: {message}");
}
