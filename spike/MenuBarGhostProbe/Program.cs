using System.Reflection;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Interop;
using Jalium.UI.Threading;
using Jalium.UI.Markup;

namespace MenuBarGhostProbe;

/// <summary>
/// Throwaway probe: who paints a menu-bar item twice? spike/VisualQA/out/menus.png shows "View" and "Help"
/// (a MenuBar of MenuBarItems, which our style templates) drawn twice a few pixels apart, while "File" and
/// "Edit" (a Menu of MenuItems, which our style only colours) are single. Both bars also carry a horizontal
/// rule no WinUI bar has. This run separates the three candidate painters: MenuBar's own OnRender,
/// MenuBarItem's own OnRender, and the Button our template puts in the item.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "menubar-probe3.txt");

    /// <summary>A colour no Astra row can produce: any pixel of it is the blank template's.</summary>
    private const uint Lime = 0x00FF00;

    private sealed class SilentMenuBar : MenuBar
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
        }
    }

    private sealed class SilentMenuBarItem : MenuBarItem
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
        }
    }

    private sealed class SilentMenu : Menu
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
        }
    }

    private sealed class SilentCommandBar : CommandBar
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
        }
    }

    [STAThread]
    private static int Main()
    {
        Shape();

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

        File.WriteAllText(LogPath, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, full log: {LogPath}");
        return 0;
    }

    private static void Shape()
    {
        Note("=== S. who declares the paint, and what names does OnApplyTemplate read ===");
        foreach (var type in new[] { typeof(MenuBar), typeof(MenuBarItem), typeof(Menu), typeof(MenuItem), typeof(CommandBar) })
        {
            var owner = type;
            string painter = "-";
            while (owner is not null)
            {
                var method = owner.GetMethod("OnRender", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (method is not null)
                {
                    painter = $"{owner.Name}#{(method.IsFamily ? "protected" : "public")}";
                    break;
                }

                owner = owner.BaseType;
            }

            Note($"  {type.Name,-12} sealed={type.IsSealed,-5} base={type.BaseType?.Name,-20} OnRender={painter}");
            Note($"      parts read by OnApplyTemplate: {string.Join(", ", PartNames(type))}");
            var declared = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(static method => method.Name)
                .Where(static name => name.StartsWith("On", StringComparison.Ordinal) || name.Contains("Render", StringComparison.Ordinal) || name.Contains("Draw", StringComparison.Ordinal))
                .Order(StringComparer.Ordinal);
            Note($"      On*/Draw* it declares: {string.Join(", ", declared)}");
            Strings(type, "OnRender");
        }

        // Is the bar's rule reachable without a subclass? The rule is one row of #1C1C1C the width of the bar,
        // and CommandBar's own code reads three rows - so ask whether painting one of them transparent erases it.
        Note("=== U. can the rule be steered by a row instead of by OnRender ===");
        Note($"  rows CommandBar's OnRender reads: {string.Join(", ", Strings(typeof(CommandBar), "OnRender"))}");
        Note($"  rows MenuBar's OnRender reads: {string.Join(", ", Strings(typeof(MenuBar), "OnRender"))}");
        Note($"  rows Menu's OnRender reads: {string.Join(", ", Strings(typeof(Menu), "OnRender"))}");
        Note($"  rows MenuBarItem's OnRender reads: {string.Join(", ", Strings(typeof(MenuBarItem), "OnRender"))}");
    }

    /// <summary>Decodes the string literals an IL body loads: ldstr is 0x72, its operand is a #US token.</summary>
    private static List<string> Strings(Type owner, string methodName)
    {
        var method = owner.GetMethod(methodName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method is null)
        {
            Note($"      {owner.Name}.{methodName}: not declared");
            return [];
        }

        var literals = new List<string>();
        if (method.GetMethodBody()?.GetILAsByteArray() is not { } il)
        {
            Note($"      {owner.Name}.{methodName}: no body");
            return literals;
        }

        for (var index = 0; index + 5 < il.Length; index++)
        {
            if (il[index] != 0x72)
            {
                continue;
            }

            try
            {
                literals.Add(owner.Module.ResolveString(BitConverter.ToInt32(il, index + 1)));
                index += 4;
            }
            catch
            {
                // An operand byte that happens to look like ldstr.
            }
        }

        var kept = literals.Distinct().Where(static text => text.Length is > 0 and < 48).ToList();
        Note($"      {owner.Name}.{methodName} literals: {string.Join(", ", kept)}");
        return kept;
    }

    private static void Run(Application application)
    {
        var blank = BlankTemplate();

        var shipped = Bar(new MenuBar(), "View", "Help");
        var silentBar = Bar(new SilentMenuBar(), "View", "Help");
        var silentItems = new MenuBar();
        silentItems.Items.Add(new SilentMenuBarItem { Title = "View" });
        silentItems.Items.Add(new SilentMenuBarItem { Title = "Help" });
        silentItems.Width = 320;
        silentItems.Height = 40;
        var blanked = Bar(new MenuBar(), "View", "Help");
        foreach (MenuBarItem item in blanked.Items)
        {
            item.Template = blank;
        }

        var menu = new Menu();
        menu.Items.Add(new MenuItem { Header = "File" });
        menu.Items.Add(new MenuItem { Header = "Edit" });
        menu.Width = 320;
        menu.Height = 40;
        var silentMenu = new SilentMenu();
        silentMenu.Items.Add(new MenuItem { Header = "File" });
        silentMenu.Items.Add(new MenuItem { Header = "Edit" });
        silentMenu.Width = 320;
        silentMenu.Height = 40;

        var bar = new CommandBar { Width = 320, Height = 48 };
        bar.PrimaryCommands.Add(new AppBarButton { Label = "Save", Icon = new SymbolIcon(Symbol.Save) });
        bar.PrimaryCommands.Add(new AppBarButton { Label = "Share", Icon = new SymbolIcon(Symbol.Share) });
        var silentBarBar = new SilentCommandBar { Width = 320, Height = 48 };
        silentBarBar.PrimaryCommands.Add(new AppBarButton { Label = "Save", Icon = new SymbolIcon(Symbol.Save) });
        silentBarBar.PrimaryCommands.Add(new AppBarButton { Label = "Share", Icon = new SymbolIcon(Symbol.Share) });

        // The rule is one row of #1C1C1C the width of the bar and CommandBar's OnRender loads no string at all,
        // so ask whether a local value on the two properties a Border would use is enough to erase it.
        var ruleCleared = new CommandBar { Width = 320, Height = 48, BorderBrush = null, BorderThickness = new Thickness(0) };
        ruleCleared.PrimaryCommands.Add(new AppBarButton { Label = "Save", Icon = new SymbolIcon(Symbol.Save) });
        ruleCleared.PrimaryCommands.Add(new AppBarButton { Label = "Share", Icon = new SymbolIcon(Symbol.Share) });

        var panel = new StackPanel();
        panel.Children.Add(shipped);
        panel.Children.Add(silentBar);
        panel.Children.Add(silentItems);
        panel.Children.Add(blanked);
        panel.Children.Add(menu);
        panel.Children.Add(silentMenu);
        panel.Children.Add(bar);
        panel.Children.Add(silentBarBar);
        panel.Children.Add(ruleCleared);

        var window = new Window
        {
            Title = "MenuBarGhostProbe",
            Content = panel,
            Width = 700,
            Height = 700,
            Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
        };
        window.Show();
        Pump(24, 1600);

        Note("=== T. what each bar built ===");
        foreach (var (label, control) in new (string, Control)[] { ("shipped", shipped), ("silentBar", silentBar), ("silentItems", silentItems), ("blanked", blanked), ("menu", menu), ("silentMenu", silentMenu) })
        {
            Note($"  {label}: actual={control.ActualWidth:0.#}x{control.ActualHeight:0.#}");
            if (label is "shipped" or "blanked")
            {
                Dump(string.Empty, control, 0);
            }
        }

        Note("=== P. pixel attribution (ink = pixels darker than #505050, i.e. text and rules) ===");
        foreach (var (label, control) in new (string, Control)[] { ("shipped", shipped), ("silentBar", silentBar), ("silentItems", silentItems), ("blanked", blanked), ("menu", menu), ("silentMenu", silentMenu), ("commandBar", bar), ("silentCommandBar", silentBarBar), ("ruleCleared", ruleCleared) })
        {
            var width = (int)Math.Round(control.ActualWidth);
            var height = (int)Math.Round(control.ActualHeight);
            if (width < 2 || height < 2)
            {
                Note($"  {label}: no surface ({width}x{height})");
                continue;
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
            bitmap.Render(control);
            var buffer = new byte[width * 4 * height];
            bitmap.CopyPixels(new Int32Rect(0, 0, width, height), buffer, width * 4, 0);

            var histogram = new Dictionary<uint, int>();
            var rowsWithInk = 0;
            for (var y = 0; y < height; y++)
            {
                var inkInRow = 0;
                for (var x = 0; x < width; x++)
                {
                    var offset = (y * width + x) * 4;
                    var key = (uint)(buffer[offset + 2] << 16 | buffer[offset + 1] << 8 | buffer[offset]);
                    histogram[key] = histogram.GetValueOrDefault(key) + 1;
                    if (key < 0x505050) inkInRow++;
                }

                if (inkInRow > 0) rowsWithInk++;
            }

            var ink = histogram.Where(static e => e.Key < 0x505050).Sum(static e => e.Value);
            var top = histogram.OrderByDescending(static e => e.Value).ThenBy(static e => e.Key).Take(6);
            Note($"  {label,-16} {width}x{height} distinct={histogram.Count} ink={ink} lime={histogram.GetValueOrDefault(Lime)} inkRows={rowsWithInk}");
            Note("      top: " + string.Join("  ", top.Select(e => $"#{e.Key:X6}x{e.Value}")));
        }

        WritePanelBitmap(panel, Path.Combine(AppContext.BaseDirectory, "menubar-panel.bmp"));

        // A control's own crop cannot show a template's text, so the live window stays up while
        // spike/VisualQA/capture-window.ps1 PrintWindows it from outside.
        Note("  holding the window for an outside capture");
        Pump(600, 10_000);
        window.Close();
        Pump(4, 400);
    }

    /// <summary>
    /// Renders the whole shown panel and writes it as a 24-bit BMP, so a doubled text run can be seen rather
    /// than inferred: a control's own crop is not enough, because a template's TextBlock contributes no pixels
    /// to that path while an OnRender-drawn glyph does.
    /// </summary>
    private static void WritePanelBitmap(Panel panel, string path)
    {
        var width = (int)Math.Round(panel.ActualWidth);
        var height = (int)Math.Round(panel.ActualHeight);
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(panel);
        var buffer = new byte[width * 4 * height];
        bitmap.CopyPixels(new Int32Rect(0, 0, width, height), buffer, width * 4, 0);

        var stride = width * 3;
        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream);
        writer.Write((ushort)0x4D42);
        writer.Write(54 + stride * height);
        writer.Write(0);
        writer.Write(54);
        writer.Write(40);
        writer.Write(width);
        writer.Write(height);
        writer.Write((ushort)1);
        writer.Write((ushort)24);
        writer.Write(0);
        writer.Write(stride * height);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        for (var y = height - 1; y >= 0; y--)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 4;
                writer.Write(buffer[offset]);
                writer.Write(buffer[offset + 1]);
                writer.Write(buffer[offset + 2]);
            }
        }

        Note($"  wrote {path} ({width}x{height})");
    }

    private static MenuBar Bar(MenuBar bar, params string[] titles)
    {
        foreach (var title in titles)
        {
            bar.Items.Add(new MenuBarItem { Title = title });
        }

        bar.Width = 320;
        bar.Height = 40;
        return bar;
    }

    private static ControlTemplate? BlankTemplate()
    {
        var dictionary = XamlReader.Parse(Markup) as ResourceDictionary;
        return dictionary?["Blank"] as ControlTemplate;
    }

    private static List<string> PartNames(Type type)
    {
        var literals = new List<string>();
        var method = type.GetMethod("OnApplyTemplate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (method?.GetMethodBody()?.GetILAsByteArray() is not { } il)
        {
            return literals;
        }

        for (var index = 0; index + 5 < il.Length; index++)
        {
            if (il[index] != 0x72)
            {
                continue;
            }

            try
            {
                literals.Add(type.Module.ResolveString(BitConverter.ToInt32(il, index + 1)));
                index += 4;
            }
            catch
            {
                // An operand byte that happens to look like ldstr.
            }
        }

        return literals.Distinct().Where(static text => text.Length < 40 && !text.Contains(' ')).ToList();
    }

    private static void Dump(string prefix, DependencyObject node, int depth)
    {
        if (depth > 8)
        {
            return;
        }

        var text = node is TextBlock textBlock ? $" text=\"{Trim(textBlock.Text ?? string.Empty)}\""
            : node is ContentPresenter presenter ? $" content={presenter.Content?.GetType().Name ?? "null"}"
            : string.Empty;
        var bounds = node is FrameworkElement element ? $" {element.ActualWidth:0.#}x{element.ActualHeight:0.#}" : string.Empty;
        Note($"    {prefix}{node.GetType().Name}{(Name(node) is { Length: > 0 } name ? $" \"{name}\"" : string.Empty)}{bounds}{text}");
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            Dump(prefix + "  ", VisualTreeHelper.GetChild(node, index)!, depth + 1);
        }
    }

    private static string Name(DependencyObject node) => node is FrameworkElement element ? element.Name ?? string.Empty : string.Empty;

    private static int Pump(int frames = 8, int budgetMilliseconds = 800)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var seen = 0;
        var deadline = DateTime.UtcNow.AddMilliseconds(budgetMilliseconds);
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames || DateTime.UtcNow > deadline) frame.Continue = false;
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
        return seen;
    }

    private static void Note(string line) => Lines.Add(line);

    private static string Trim(string text) => text.Replace('\r', ' ').Replace('\n', ' ').Trim();

    private const string Markup = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <ControlTemplate x:Key='Blank' TargetType='MenuBarItem'>
            <Border Background='#FF00FF00' />
          </ControlTemplate>
        </ResourceDictionary>
        """;
}
