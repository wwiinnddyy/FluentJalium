using System.Reflection;
using System.Text;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace GlyphInkProbe;

/// <summary>
/// Lays every <see cref="Symbol"/> value on a white grid inside a red box so a monitor grab can be read cell by cell,
/// and writes the cell map next to the exe.
///
/// Two claims ride on it. #62/#50 said glyph ink cannot be measured at all - that was written when every capture path
/// was in-process, and an in-process capture re-runs the render pass. A monitor grab does see the ink, so this prints a
/// per-cell ink count with two controls (a text cell that must carry ink, a blank cell that must not): a grid-wide
/// zero now reads as a broken instrument rather than as a finding about fonts.
///
/// #97 asks which font paints the symbol set, and the three variants answer it against pixels instead of against an
/// impression: "symbol" is the element our templates use, "fluent" and "mdl2" are the same codepoints handed to
/// FontIcon with that family set from code. Cell i lands in the same rect in all three runs, so the shapes can be
/// compared cell by cell with the signature each run writes.
/// </summary>
internal static class Program
{
    private const int Cell = 24;
    private const int Columns = 40;

    private const string FluentFamily = "Segoe Fluent Icons";
    private const string Mdl2Family = "Segoe MDL2 Assets";
    private const string Root = "xmlns=\"https://schemas.jalium.dev/jalxaml/presentation\"";

    private static readonly SolidColorBrush White = new(Color.FromRgb(0xFF, 0xFF, 0xFF));
    private static readonly SolidColorBrush Black = new(Color.FromRgb(0x00, 0x00, 0x00));
    private static readonly SolidColorBrush Red = new(Color.FromRgb(0xFF, 0x00, 0x00));

    private static readonly List<string> Rows = [];

    private static string _variant = "symbol";

    [STAThread]
    private static int Main(string[] arguments)
    {
        for (var index = 0; index + 1 < arguments.Length; index += 2)
        {
            if (arguments[index] == "--variant") _variant = arguments[index + 1];
        }

        if (_variant is not ("symbol" or "fluent" or "mdl2" or "markup"))
        {
            Console.WriteLine($"unknown --variant '{_variant}' (symbol, fluent, mdl2, markup)");
            return 2;
        }

        var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
        renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
        ThemeLoader.Initialize();
        var application = new Application();

        var symbols = Enum.GetValues<Symbol>();
        var total = symbols.Length + 2;
        var usedRows = (total + Columns - 1) / Columns;

        var canvas = new Canvas { Background = White };
        Place(canvas, 0, "text", "CONTROL-TEXT", "0", new TextBlock
        {
            Text = "MW",
            FontSize = 16,
            Foreground = Black,
        });
        Place(canvas, 1, "blank", "CONTROL-BLANK", "0", new Border { Background = White });
        for (var index = 0; index < symbols.Length; index++)
        {
            var symbol = symbols[index];
            Place(canvas, index + 2, "symbol", symbol.ToString(), $"U+{(int)symbol:X4}", Paint(symbols[index]));
        }

        var box = new Border
        {
            Width = Columns * Cell,
            Height = usedRows * Cell,
            Background = White,
            BorderBrush = Red,
            BorderThickness = new Thickness(2),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Child = canvas,
        };
        // Window.Width/Height takes the same units the content lays out in, so the box only needs slack of its own; the
        // grabber is DPI-aware and measures the stretched picture (this display paints 1.75 px per DIP, which the
        // counter checks against the box before it trusts a single cell).
        var window = new Window
        {
            Title = "GlyphInkProbe",
            Width = Columns * Cell + 80,
            Height = usedRows * Cell + 120,
            Background = White,
            Content = box,
        };
        window.Show();
        Pump(20, 2000);
        var pixelsPerDip = window.DpiScale;
        if (pixelsPerDip <= 0 || double.IsNaN(pixelsPerDip)) pixelsPerDip = 1;

        // Geometry self-witness: the grabber refuses a box whose width and height disagree about the scale, and this
        // line is what names the layout pass's own account when that guard fires.
        Console.WriteLine($"variant={_variant} geom dpi={pixelsPerDip:0.###} window={window.Width:0.##}x{window.Height:0.##} " +
            $"actual={window.ActualWidth:0.##}x{window.ActualHeight:0.##}");
        Console.WriteLine($"geom box requested={box.Width}x{box.Height} actual={box.ActualWidth}x{box.ActualHeight}");
        var toWindow = box.TransformToVisual(window);
        if (toWindow is not null && toWindow.TryTransform(new Point(0, 0), out var origin))
            Console.WriteLine($"geom box origin in window=DIP {origin.X:0.##},{origin.Y:0.##}");
        else
            Console.WriteLine("geom no box-to-window transform");

        var map = Path.Combine(AppContext.BaseDirectory, $"glyph-cells-{_variant}.tsv");
        File.WriteAllText(map,
            $"# variant={_variant} cell-dip={Cell} columns={Columns} rows={usedRows} " +
            $"box-dip={Columns * Cell}x{usedRows * Cell} px-per-dip={pixelsPerDip:0.###}" +
            Environment.NewLine +
            "index\tkind\tname\tcodepoint\tcol\trow" + Environment.NewLine +
            string.Join(Environment.NewLine, Rows) + Environment.NewLine);
        Console.WriteLine($"glyph-cells written={Rows.Count} rows={usedRows} file={map}");

        FontWitness();
        MarkupWitness();

        // Hold the frame until the grabber drops the stop file, pumping so the window keeps painting. The grabber
        // owns the timing; a fixed sleep here would measure whenever the paint happened to be ready.
        var flag = Path.Combine(AppContext.BaseDirectory, "glyph-stop.flag");
        if (File.Exists(flag)) File.Delete(flag);
        var deadline = Environment.TickCount64 + 120_000;
        while (Environment.TickCount64 < deadline && !File.Exists(flag))
        {
            Pump(4, 250);
            Thread.Sleep(30);
        }

        window.Close();
        Pump(4, 500);
        Console.WriteLine(File.Exists(flag) ? "stopped by the grabber" : "hold budget exhausted, closing anyway");
        return 0;
    }

    /// <summary>Builds the element that carries one codepoint under the variant being measured.</summary>
    private static FrameworkElement Paint(Symbol symbol)
    {
        if (_variant == "symbol")
            return new SymbolIcon { Symbol = symbol, Width = 20, Height = 20, Foreground = Black };

        if (_variant == "markup")
        {
            // The same element the fluent row paints, but handed its family the way a template does - a read-back
            // proving a literal FontFamily survives parsing says nothing about ink until the picture says so too.
            var snippet = $"""<Grid {Root}><FontIcon Glyph="&#x{(int)symbol:X4};" FontSize="20" FontFamily="{FluentFamily}" /></Grid>""";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(snippet));
            if (XamlReader.Load(stream) is Grid { Children.Count: > 0 } grid && grid.Children[0] is FrameworkElement element)
            {
                // The parsed element still has its throwaway parent; the canvas cannot adopt it until that is dropped.
                grid.Children.RemoveAt(0);
                // Only the family is delivered by markup here. The ink colour comes from code because the other three
                // rows set it in code too, and leaving it unset sends IconElement's fallback looking for a
                // "TextPrimary" resource this bare window never resolved - which showed up as a grid with nothing
                // painted, an instrument reading, not a fact about markup.
                if (element is IconElement icon) icon.Foreground = Black;
                return element;
            }

            Console.WriteLine($"markup parse gave no element for U+{(int)symbol:X4}");
            return new Border { Background = White };
        }

        var glyph = char.ConvertFromUtf32((int)symbol);
        return new FontIcon
        {
            Glyph = glyph,
            FontFamily = new FontFamily(_variant == "fluent" ? FluentFamily : Mdl2Family),
            FontSize = 20,
            Width = 20,
            Height = 20,
            Foreground = Black,
        };
    }

    /// <summary>
    /// Prints what the shipped assembly - not the sibling source tree, which AGENTS.md warns may be newer - says about
    /// the font a symbol icon paints with. The private statics are read here because this is a probe; the same
    /// reflection in src/FluentJalium is what the gate text-checks for and rejects.
    /// </summary>
    private static void FontWitness()
    {
        Console.WriteLine($"assembly={typeof(SymbolIcon).Assembly.GetName().Name} " +
            $"version={typeof(SymbolIcon).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");
        Console.WriteLine("SymbolIcon public=" + string.Join(',', typeof(SymbolIcon)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(static property => property.Name)));
        Console.WriteLine("FontIcon public=" + string.Join(',', typeof(FontIcon)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(static property => property.Name)));
        foreach (var (type, field) in new[]
                 {
                     (typeof(SymbolIcon), "SymbolFontFamily"),
                     (typeof(FontIcon), "DefaultFontFamily"),
                 })
        {
            var privateField = type.GetField(field, BindingFlags.NonPublic | BindingFlags.Static);
            if (privateField is null)
            {
                Console.WriteLine($"witness {type.Name}.{field}=<no such field>");
                continue;
            }

            var family = privateField.GetValue(null) as FontFamily;
            Console.WriteLine($"witness {type.Name}.{field}='{family?.Source}'");
        }

        var icon = new FontIcon { Glyph = "a", FontFamily = new FontFamily(FluentFamily) };
        Console.WriteLine($"witness FontIcon.FontFamily after a code write='{icon.FontFamily?.Source}' " +
            $"default instance='{new FontIcon().FontFamily?.Source ?? "<null>"}'");

        // And whether the two families resolve to a face at all: a name that matches nothing paints through a fallback,
        // and the cell readings would then describe the fallback rather than the font named on screen.
        foreach (var name in new[] { FluentFamily, Mdl2Family, "Microsoft YaHei UI" })
        {
            var probe = new FormattedText("MW", name, 16) { MaxTextWidth = 200, MaxTextHeight = 40 };
            TextMeasurement.MeasureText(probe);
            Console.WriteLine($"witness measure '{name}' MW width={probe.Width:0.##}");
        }
    }

    /// <summary>
    /// Whether a font family can reach an icon from markup at all. The audit's table only tried resource-key
    /// spellings (x:Key rows and a {ThemeResource} hand-off) and every one of them came back an empty FontFamily;
    /// a literal attribute is a different parse path, and which way it goes decides whether a fix can sit in a
    /// template or has to be delivered from code.
    /// </summary>
    private static void MarkupWitness()
    {
        foreach (var (label, snippet, read) in new (string, string, Func<object?, string>)[]
                 {
                     (
                         "FontIcon literal FontFamily",
                         """
                         <FontIcon xmlns="https://schemas.jalium.dev/jalxaml/presentation" Glyph="&#xE700;" FontSize="20" FontFamily="Segoe Fluent Icons" />
                         """,
                         (Func<object?, string>)(value => value is FontIcon icon
                             ? $"'{icon.FontFamily?.Source ?? "<null>"}' glyph=U+{(int)char.ConvertToUtf32(icon.Glyph, 0):X4} size={icon.FontSize}"
                             : $"<not a FontIcon: {value?.GetType().Name ?? "null"}>")
                     ),
                     (
                         "TextBlock literal FontFamily",
                         """
                         <TextBlock xmlns="https://schemas.jalium.dev/jalxaml/presentation" Text="MW" FontSize="20" FontFamily="Segoe MDL2 Assets" />
                         """,
                         (Func<object?, string>)(value => value is TextBlock text
                             ? $"'{text.FontFamily?.Source ?? "<null>"}' text={text.Text} size={text.FontSize}"
                             : $"<not a TextBlock: {value?.GetType().Name ?? "null"}>")
                     ),
                 })
        {
            try
            {
                // Parse(string) is in the sibling source tree but not in the shipped 26.10.9 assembly, and the reader
                // this runtime carries sits under Jalium.UI.Markup - the stream overload is the reachable one.
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(snippet));
                Console.WriteLine($"markup {label} -> {read(XamlReader.Load(stream))}");
            }
            catch (Exception error)
            {
                Console.WriteLine($"markup {label} threw {error.GetType().Name}: {error.Message}");
            }
        }
    }

    private static void Place(Canvas canvas, int index, string kind, string name, string codepoint, FrameworkElement element)
    {
        var column = index % Columns;
        var row = index / Columns;
        element.Width = Cell;
        element.Height = Cell;
        element.HorizontalAlignment = HorizontalAlignment.Center;
        element.VerticalAlignment = VerticalAlignment.Center;
        Canvas.SetLeft(element, column * Cell);
        Canvas.SetTop(element, row * Cell);
        canvas.Children.Add(element);
        Rows.Add($"{index}\t{kind}\t{name}\t{codepoint}\t{column}\t{row}");
    }

    private static void Pump(int frames, int budgetMilliseconds)
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
}
