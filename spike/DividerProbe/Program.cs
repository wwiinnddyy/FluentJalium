using System.Reflection;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Threading;

namespace DividerProbe;

/// <summary>
/// The readings the Divider slice cannot pick a host for without. WinUI's `Divider` control does not exist at the
/// pinned commit (zero files by name, zero `Divider` types in .idl/.h), so the only native candidate is Jalium's own
/// `Separator`, and the only geometry authority is the seven divider-role templates other controls carry.
///   surface: what `Separator` (and the sibling separator/icon types) actually declares - base chain, own dependency
///            properties, and whether the type draws itself in OnRender the way `MenuItem` and `MenuFlyoutSeparator`
///            do (audits/menu-flyout.md 2, 3). A self-drawing type stores a Template without ever realizing it, so
///            this decides whether "retemplate the native control" is even a live option.
///   ink    : does a 1 DIP line survive an in-process capture at all, and through which element - Rectangle,
///            Border, or a Separator's template. The row profile is printed because a line that lands on a row
///            boundary can half-cover two rows instead of filling one.
///   alpha  : the real token is translucent (#0F000000 on Light, #15FFFFFF on Dark). Prints the exact sampled RGB of
///            the line row over a white card in both themes, so the pixel assertion picks a predicate that is not
///            sitting on a rounding boundary.
///   mount  : a Separator whose Style/Template arrives from markup, checked for whether the named part realizes.
/// Modes: surface | ink | alpha | mount | all.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];

    private static string _mode = "all";

    [STAThread]
    private static int Main(string[] arguments)
    {
        _mode = arguments.Length > 0 ? arguments[0].ToLowerInvariant() : "all";
        try
        {
            var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
            renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            var application = new Application();
            FluentThemeManager.Apply(application);
            Run(application);
        }
        catch (Exception exception)
        {
            Note("BOOT threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }

        var path = System.IO.Path.Combine(AppContext.BaseDirectory, $"divider-probe-{_mode}.txt");
        File.WriteAllText(path, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, mode={_mode}, full log: {path}");
        return 0;
    }

    private static void Run(Application application)
    {
        var root = new StackPanel { Margin = new Thickness(24) };
        var window = new Window { Content = root, Width = 900, Height = 900, Title = "Divider probe" };
        window.Loaded += (_, _) =>
        {
            try
            {
                Pump();
                Note($"theme dictionaries={FluentThemeManager.DictionaryNames.Count}");
                if (_mode is "all" or "surface")
                {
                    Surface();
                }

                if (_mode is "all" or "ink")
                {
                    Ink(root);
                }

                if (_mode is "all" or "alpha")
                {
                    Alpha(root);
                }

                if (_mode is "all" or "mount")
                {
                    Mount(root);
                }

                if (_mode is "all" or "shape")
                {
                    Shape(root);
                }
            }
            catch (Exception exception)
            {
                Note("PROBE threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            }
            finally
            {
                window.Close();
                application.Shutdown();
            }
        };

        application.Run(window);
    }

    private const string Cells = """
        <StackPanel xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
          <Grid Name="RectFixed" Width="320" Height="24" Background="#FFFFFFFF">
            <Rectangle Height="1" Width="300" Fill="#FF101010" HorizontalAlignment="Center" VerticalAlignment="Center" />
          </Grid>
          <Grid Name="BorderFixed" Width="320" Height="24" Background="#FFFFFFFF">
            <Border Height="1" Width="300" Background="#FF101010" HorizontalAlignment="Center" VerticalAlignment="Center" />
          </Grid>
          <Grid Name="RectStretch" Width="320" Height="24" Background="#FFFFFFFF">
            <Rectangle Height="1" Fill="#FF101010" VerticalAlignment="Center" />
          </Grid>
          <Grid Name="RectHalf" Width="320" Height="24" Background="#FFFFFFFF">
            <Rectangle Height="0.5" Fill="#FF101010" VerticalAlignment="Center" />
          </Grid>
          <Grid Name="BareSeparator" Width="320" Height="24" Background="#FFFFFFFF">
            <Separator />
          </Grid>
          <Grid Name="TemplatedSeparator" Width="320" Height="24" Background="#FFFFFFFF">
            <Separator>
              <Separator.Template>
                <ControlTemplate TargetType="Separator">
                  <Border Name="DividerLine" Height="1" Background="#FF101010" VerticalAlignment="Center" />
                </ControlTemplate>
              </Separator.Template>
            </Separator>
          </Grid>
          <Grid Name="VerticalBorder" Width="24" Height="120" Background="#FFFFFFFF">
            <Border Width="1" Height="100" Background="#FF101010" HorizontalAlignment="Center" VerticalAlignment="Center" />
          </Grid>
          <Grid Name="TokenLineLight" Width="320" Height="24" Background="#FFFFFFFF">
            <Rectangle Height="1" Fill="{ThemeResource DividerStrokeColorDefaultBrush}" VerticalAlignment="Center" />
          </Grid>
        </StackPanel>
        """;

    // ---------- A. what the runtime exports for a separator, and who draws it ----------

    private static void Surface()
    {
        Note("");
        Note("=== A. separator and icon candidates: chain, own DPs, and who owns OnRender ===");
        foreach (var name in new[]
                 {
                     "Separator", "MenuFlyoutSeparator", "AppBarSeparator", "GridViewItemSeparator",
                     "Divider", "Line", "Rectangle", "Border", "GridSplitter", "ToolBar",
                     "SymbolIcon", "FontIcon", "PathIcon", "BitmapIcon", "IconElement", "Symbol",
                 })
        {
            var type = TypeByName(name);
            if (type is null)
            {
                Note($"  {name,-22} ABSENT");
                continue;
            }

            var ownsRender = type
                .GetMethod("OnRender", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                is not null;
            var ownFields = type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(field => typeof(DependencyProperty).IsAssignableFrom(field.FieldType))
                .Select(field => $"{field.Name.Replace("Property", string.Empty)}={PropType(field)}")
                .OrderBy(static text => text, StringComparer.Ordinal)
                .ToList();
            var hasBackground = type.GetProperty("Background", BindingFlags.Public | BindingFlags.Instance) is not null;
            var hasTemplate = type.GetProperty("Template", BindingFlags.Public | BindingFlags.Instance) is not null;
            Note($"  {name,-22} {Chain(type)} | onRender={type.Name}.{(ownsRender ? "DECLARES OnRender" : "no own OnRender")}" +
                 $" | background={hasBackground} template={hasTemplate}" +
                 $" | ownDPs=[{string.Join(", ", ownFields)}]");
        }

        Note("  --- a bare Separator's own read-backs, through its own typed properties ---");
        var bare = new Separator();
        Note($"  fresh: Style={Show(bare.Style)} Template={Show(bare.Template)} Height={Show(bare.Height)}" +
             $" MinHeight={Show(bare.MinHeight)} Margin={Show(bare.Margin)} IsHitTestVisible={Show(bare.IsHitTestVisible)}");
    }

    // ---------- B. does a 1 DIP line reach the picture, and through which element ----------

    private static void Ink(Panel root)
    {
        Note("");
        Note("=== B. 1 DIP line candidates: parsed, mounted, then counted for ink ===");
        var host = ParseInto(root, Cells);
        if (host is null)
        {
            return;
        }

        foreach (var name in new[]
                 {
                     "RectFixed", "BorderFixed", "RectStretch", "RectHalf", "BareSeparator",
                     "TemplatedSeparator", "VerticalBorder", "TokenLineLight",
                 })
        {
            var cell = FindNamed(host, name);
            if (cell is null)
            {
                Note($"  {name,-20} not in tree");
                continue;
            }

            var inner = ChildrenOf(cell).OfType<FrameworkElement>().FirstOrDefault();
            Note($"  {name,-20} cell={cell.ActualWidth:0}x{cell.ActualHeight:0} " +
                 $"inner={inner?.GetType().Name ?? "none"} innerSize={inner?.ActualWidth:0}x{inner?.ActualHeight:0} " +
                 $"innerHeight={Show(Prop(inner, "Height"))}");
            if (name == "TemplatedSeparator")
            {
                var line = FindNamed(cell, "DividerLine");
                Note($"    part DividerLine realized={line?.GetType().Name ?? "null"}" +
                     $" size={line?.ActualWidth:0}x{line?.ActualHeight:0}");
            }

            Profile(cell, name, white: 250);
        }
    }

    // ---------- C. what the translucent token actually leaves on a white card ----------

    private static void Alpha(Panel root)
    {
        Note("");
        Note("=== C. DividerStrokeColorDefaultBrush as painted, both themes, on a white card ===");
        const string TokenCell = """
            <Grid xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                  Name="TokenCell" Width="320" Height="24" Background="#FFFFFFFF">
              <Rectangle Height="1" Fill="{ThemeResource DividerStrokeColorDefaultBrush}" VerticalAlignment="Center" />
            </Grid>
            """;

        foreach (var variant in new[] { FluentThemeVariant.Light, FluentThemeVariant.Dark })
        {
            FluentThemeManager.ApplyTheme(variant);
            var cell = XamlReader.Parse(TokenCell) as FrameworkElement;
            if (cell is null)
            {
                Note($"  {variant}: parse produced null");
                continue;
            }

            root.Children.Add(cell);
            Pump(8);
            Note($"  {variant}: token row reads {Show(Application.Current?.TryFindResource("DividerStrokeColorDefaultBrush") is SolidColorBrush brush ? brush.Color.ToString() : "not a brush")}" +
                 $" (Color row: {Show(Application.Current?.TryFindResource("DividerStrokeColorDefault"))})");
            SampleRow(cell, 320, 24, variant.ToString());
            root.Children.Remove(cell);
            Pump(2);
        }

        FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
    }

    // ---------- D. can a markup template land on the native control ----------

    private static void Mount(Panel root)
    {
        Note("");
        Note("=== D. a Separator styled from markup: does the part realize, and who owns the box ===");
        const string StyleMarkup = """
            <Style xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                   TargetType="Separator">
              <Setter Property="Background" Value="#FF202020" />
              <Setter Property="Height" Value="9" />
              <Setter Property="Template">
                <Setter.Value>
                  <ControlTemplate TargetType="Separator">
                    <Grid Name="DividerRoot">
                      <Border Name="DividerLine" Height="1" Background="{TemplateBinding Background}" VerticalAlignment="Center" />
                    </Grid>
                  </ControlTemplate>
                </Setter.Value>
              </Setter>
            </Style>
            """;

        var style = XamlReader.Parse(StyleMarkup) as Style;
        Note($"  style parsed={style is not null}");
        if (style is null)
        {
            return;
        }

        var separator = new Separator { Style = style, Width = 300 };
        var card = new Grid
        {
            Width = 320,
            Height = 40,
            Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
            Children = { separator },
        };
        root.Children.Add(card);
        Pump(8);

        Note($"  separator: box={separator.ActualWidth:0}x{separator.ActualHeight:0}" +
             $" Height={Show(separator.Height)} Background={Show(separator.Background?.GetType().Name)}");
        var line = FindNamed(separator, "DividerLine");
        Note($"  part DividerLine realized={line?.GetType().Name ?? "null"} size={line?.ActualWidth:0}x{line?.ActualHeight:0}");
        var box = FindNamed(separator, "DividerRoot");
        Note($"  part DividerRoot realized={box?.GetType().Name ?? "null"} size={box?.ActualWidth:0}x{box?.ActualHeight:0}");
        Profile(card, "StyledSeparator", white: 250);

        // The local-value question the ProgressBar slice had to learn twice: does anything write over the
        // control's own Height once it sits in a panel?
        separator.Height = 20;
        Pump(6);
        Note($"  after Height=20: box={separator.ActualHeight:0} line={line?.ActualHeight:0}");
        Profile(card, "StyledSeparator(taller)", white: 250);
    }

    // ---------- E. is it "1 DIP" that kills the rectangle, or the rectangle? ----------

    private const string ShapeCells = """
        <StackPanel xmlns="http://schemas.jalium.ui/2024" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
          <Grid Name="Rect1" Width="320" Height="24" Background="#FFFFFFFF">
            <Rectangle Height="1" Width="300" Fill="#FF101010" VerticalAlignment="Center" />
          </Grid>
          <Grid Name="Rect2" Width="320" Height="24" Background="#FFFFFFFF">
            <Rectangle Height="2" Width="300" Fill="#FF101010" VerticalAlignment="Center" />
          </Grid>
          <Grid Name="Rect3" Width="320" Height="24" Background="#FFFFFFFF">
            <Rectangle Height="3" Width="300" Fill="#FF101010" VerticalAlignment="Center" />
          </Grid>
          <Grid Name="Rect1Top" Width="320" Height="24" Background="#FFFFFFFF">
            <Rectangle Height="1" Width="300" Fill="#FF101010" VerticalAlignment="Top" />
          </Grid>
          <Grid Name="Rect4" Width="320" Height="24" Background="#FFFFFFFF">
            <Rectangle Height="4" Width="300" Fill="#FF101010" VerticalAlignment="Center" />
          </Grid>
          <Grid Name="Rect1Bottom" Width="320" Height="24" Background="#FFFFFFFF">
            <Rectangle Height="1" Width="300" Fill="#FF101010" VerticalAlignment="Bottom" />
          </Grid>
          <Grid Name="Rect1Stroke" Width="320" Height="24" Background="#FFFFFFFF">
            <Rectangle Height="1" Width="300" Stroke="#FF101010" StrokeThickness="1" VerticalAlignment="Center" />
          </Grid>
          <Grid Name="Line1" Width="320" Height="24" Background="#FFFFFFFF">
            <Line X1="10" Y1="12" X2="310" Y2="12" Stroke="#FF101010" StrokeThickness="1" />
          </Grid>
          <Grid Name="Border1" Width="320" Height="24" Background="#FFFFFFFF">
            <Border Height="1" Width="300" Background="#FF101010" VerticalAlignment="Center" />
          </Grid>
          <Grid Name="Border1Token" Width="320" Height="24" Background="#FFFFFFFF">
            <Border Height="1" Width="300" Background="{ThemeResource DividerStrokeColorDefaultBrush}" VerticalAlignment="Center" />
          </Grid>
        </StackPanel>
        """;

    private static void Shape(Panel root)
    {
        Note("");
        Note("=== E. control experiment: which thin element actually paints ===");
        var host = ParseInto(root, ShapeCells);
        if (host is null)
        {
            return;
        }

        foreach (var name in new[] { "Rect1", "Rect2", "Rect3", "Rect1Top", "Rect1Bottom", "Rect4", "Rect1Stroke", "Line1", "Border1", "Border1Token" })
        {
            var cell = FindNamed(host, name);
            if (cell is null)
            {
                Note($"  {name,-14} not in tree");
                continue;
            }

            var inner = ChildrenOf(cell).OfType<FrameworkElement>().FirstOrDefault();
            Note($"  {name,-14} inner={inner?.GetType().Name ?? "none"} size={inner?.ActualWidth:0}x{inner?.ActualHeight:0}");
            Profile(cell, name, white: 250);
            if (name == "Border1Token")
            {
                SampleRow(cell, 320, 24, name);
            }
        }

        Note("  --- the native control's own drawing, driven by its own properties (no template) ---");
        foreach (var setup in new Action<Separator>[]
                 {
                     _ => { },
                     separator => separator.StrokeThickness = 4,
                     separator =>
                     {
                         separator.StrokeBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00));
                         separator.StrokeThickness = 3;
                     },
                 })
        {
            var separator = new Separator { Width = 300 };
            setup(separator);
            var card = new Grid { Width = 320, Height = 24, Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)) };
            card.Children.Add(separator);
            root.Children.Add(card);
            Pump(8);
            Note($"  bare Separator (stroke={separator.StrokeBrush?.GetType().Name ?? "null"} x{Show(separator.StrokeThickness)}):" +
                 $" box={separator.ActualWidth:0}x{separator.ActualHeight:0}");
            Profile(card, $"bare/stroke{separator.StrokeThickness:0}", white: 250);
            SampleRow(card, 320, 24, $"bare/stroke{separator.StrokeThickness:0}");
            root.Children.Remove(card);
            Pump(2);
        }

        Note("  --- the vertical axis ---");
        var vertical = new Separator { Orientation = Jalium.UI.Controls.Orientation.Vertical, Height = 100 };
        var vCard = new Grid { Width = 24, Height = 120, Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)) };
        vCard.Children.Add(vertical);
        root.Children.Add(vCard);
        Pump(8);
        Note($"  vertical Separator: box={vertical.ActualWidth:0}x{vertical.ActualHeight:0}");
        Profile(vCard, "vertical", white: 250);
        SampleRow(vCard, 24, 120, "vertical");
        root.Children.Remove(vCard);
        Pump(2);

        // The loose-snippet question: a {ThemeResource} read inside a standalone parsed element is not the same
        // scope as a row resolved from a merged dictionary (S1-p's negative-reading trap). Here the brush is
        // resolved in code from Application.Current, so a silent row is a paint failure, not a lookup failure.
        Note("  --- the token brush handed over in code, both themes (Border and the native stroke) ---");
        foreach (var variant in new[] { FluentThemeVariant.Light, FluentThemeVariant.Dark })
        {
            FluentThemeManager.ApplyTheme(variant);
            var brush = Application.Current?.TryFindResource("DividerStrokeColorDefaultBrush") as SolidColorBrush;
            Note($"  {variant}: resolved brush={brush?.Color.ToString() ?? "null"}");

            var border = new Border
            {
                Width = 300,
                Height = 1,
                Background = brush,
                VerticalAlignment = Jalium.UI.VerticalAlignment.Center,
            };
            var borderCard = new Grid { Width = 320, Height = 24, Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)) };
            borderCard.Children.Add(border);
            root.Children.Add(borderCard);
            Pump(8);
            Profile(borderCard, $"{variant}/Border", white: 250);
            SampleRow(borderCard, 320, 24, $"{variant}/Border");
            root.Children.Remove(borderCard);
            Pump(2);

            var separator = new Separator { Width = 300, StrokeBrush = brush };
            var strokeCard = new Grid { Width = 320, Height = 24, Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)) };
            strokeCard.Children.Add(separator);
            root.Children.Add(strokeCard);
            Pump(8);
            Profile(strokeCard, $"{variant}/SeparatorStroke", white: 250);
            SampleRow(strokeCard, 320, 24, $"{variant}/SeparatorStroke");
            root.Children.Remove(strokeCard);
            Pump(2);
        }

        FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
    }

    // ---------- harness ----------

    private static FrameworkElement? ParseInto(Panel root, string markup)
    {
        object? parsed;
        try
        {
            parsed = XamlReader.Parse(markup);
        }
        catch (Exception exception)
        {
            Note($"  parse threw {exception.GetType().Name}: {Trim(exception.InnerException?.Message ?? exception.Message)}");
            return null;
        }

        if (parsed is not FrameworkElement element)
        {
            Note($"  parse produced {parsed?.GetType().Name ?? "null"}, not an element");
            return null;
        }

        root.Children.Add(element);
        Pump(8);
        return element;
    }

    /// <summary>Counts every pixel that is not within <paramref name="white"/> of pure white, and prints the rows.</summary>
    private static void Profile(FrameworkElement element, string label, byte white = 240)
    {
        var width = (int)Math.Round(element.ActualWidth);
        var height = (int)Math.Round(element.ActualHeight);
        if (width <= 0 || height <= 0)
        {
            Note($"    {label}: no box ({element.ActualWidth}x{element.ActualHeight}), nothing to read");
            return;
        }

        var buffer = Capture(element, width, height);
        var ink = 0;
        var rows = new int[height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 4;
                if (buffer[offset] >= white && buffer[offset + 1] >= white && buffer[offset + 2] >= white)
                {
                    continue;
                }

                ink++;
                rows[y]++;
            }
        }

        var profile = string.Join(",", rows.Select((count, index) => count == 0 ? null : $"{index}:{count}").Where(static part => part is not null)!);
        Note($"    {label}: ink={ink} rows=[{Trim(profile)}]");
    }

    /// <summary>Prints the darkest pixel of every non-white row, so a translucent line can be read as a colour.</summary>
    private static void SampleRow(FrameworkElement element, int width, int height, string label)
    {
        var buffer = Capture(element, width, height);
        for (var y = 0; y < height; y++)
        {
            var darkest = int.MaxValue;
            string? picked = null;
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 4;
                var blue = buffer[offset];
                var green = buffer[offset + 1];
                var red = buffer[offset + 2];
                var luminance = red + green + blue;
                if (luminance < darkest)
                {
                    darkest = luminance;
                    picked = $"#{red:X2}{green:X2}{blue:X2}";
                }
            }

            if (darkest < 3 * 255)
            {
                Note($"    {label}: row {y} darkest={picked}");
            }
        }
    }

    private static byte[] Capture(FrameworkElement element, int width, int height)
    {
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(element);
        var buffer = new byte[width * height * 4];
        bitmap.CopyPixels(buffer, width * 4, 0);
        return buffer;
    }

    private static FrameworkElement? FindNamed(DependencyObject root, string name)
    {
        foreach (var child in ChildrenOf(root))
        {
            if (child is FrameworkElement element && element.Name == name)
            {
                return element;
            }

            if (FindNamed(child, name) is { } nested)
            {
                return nested;
            }
        }

        return null;
    }

    private static List<DependencyObject> ChildrenOf(DependencyObject root)
    {
        var list = new List<DependencyObject>();

        // The panel route is the honest one here; the VisualTreeHelper route is only a fallback, and an empty walk
        // must never read as "no descendants" (that mistake already cost a whole probe pass once).
        var children = root.GetType().GetProperty("Children", BindingFlags.Public | BindingFlags.Instance)?.GetValue(root);
        if (children is System.Collections.IEnumerable items)
        {
            foreach (var item in items)
            {
                if (item is DependencyObject child)
                {
                    list.Add(child);
                }
            }

            if (list.Count > 0)
            {
                return list;
            }
        }

        var template = Prop(root, "Template") is null ? null : Read(() => root.GetType()
            .GetMethod("GetTemplateChild", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
            ?.Invoke(root, ["DividerLine"]));
        if (template is DependencyObject part)
        {
            list.Add(part);
        }

        return list;
    }

    private static string PropType(FieldInfo field)
    {
        var property = field.DeclaringType?.GetProperty(field.Name.Replace("Property", string.Empty),
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        return property?.PropertyType.Name ?? "?";
    }

    private static string Chain(Type type)
    {
        var chain = new List<string> { type.Name };
        for (var baseType = type.BaseType; baseType is not null && baseType != typeof(object); baseType = baseType.BaseType)
        {
            chain.Add(baseType.Name);
        }

        return string.Join(" < ", chain);
    }

    private static object? Prop(object? target, string name) => target is null
        ? null
        : Read(() => target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(target));

    private static object? Read(Func<object?> reader)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            return " threw " + exception.GetType().Name;
        }
    }

    private static string Show(object? value) => value is null ? "null" : Trim(value.ToString() ?? "?");

    private static void Note(string line) => Lines.Add(line);

    private static string Trim(string text) => text.Replace('\r', ' ').Replace('\n', ' ').Trim();

    private static int Pump(int frames = 6, int budgetMilliseconds = 1500)
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
        return seen;
    }

    private static List<Assembly> LoadedAssemblies()
    {
        var seen = new Dictionary<string, Assembly>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.GetName().Name ?? "?";
            if (name.StartsWith("Jalium", StringComparison.Ordinal) || name.StartsWith("FluentJalium", StringComparison.Ordinal))
            {
                seen.TryAdd(name, assembly);
            }
        }

        return seen.Values.ToList();
    }

    private static Type? TypeByName(string name) => LoadedAssemblies()
        .SelectMany(SafeTypes)
        .FirstOrDefault(type => type.Name == name && !type.IsNested);

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
}
