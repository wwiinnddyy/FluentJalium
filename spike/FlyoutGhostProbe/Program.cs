using System.Reflection;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;
using Jalium.UI.Interop;
using Jalium.UI.Threading;

namespace FlyoutGhostProbe;

/// <summary>
/// Throwaway probe: what does an opened flyout look like once our template stops drawing what the control
/// already draws itself? spike/MenuBarGhostProbe measured that a MenuBarItem paints its own Title (our
/// template's second label cost 828 bright pixels) and the IL here shows the same for every flyout row type:
/// MenuFlyoutItem.OnRender carries the label and the hover surface, ToggleMenuFlyoutItem carries the check
/// mark, MenuFlyoutSubItem carries its label, MenuFlyoutSeparator carries its line. This holds one real
/// MenuFlyout open under a chosen pass so spike/VisualQA/capture-window.ps1 can PrintWindow the popup's own
/// top-level window - the only path that answers "is this row painted twice".
///   pass shipped  : the templates as shipped (both painters).
///   pass stripped : a local Template value that keeps only the surface and the icon slot, so the control's
///                   own OnRender is the sole painter of label, accelerator, check and chevron.
///   pass silent   : our shipped templates with the control's OnRender suppressed (the other half of the pair).
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];

    private sealed class SilentFlyoutItem : MenuFlyoutItem
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
        }
    }

    private sealed class SilentToggleItem : ToggleMenuFlyoutItem
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
        }
    }

    private sealed class SilentSeparator : MenuFlyoutSeparator
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
        }
    }

    [STAThread]
    private static int Main(string[] arguments)
    {
        var pass = arguments.FirstOrDefault(static text => !text.StartsWith("-", StringComparison.Ordinal)) ?? "shipped";
        var hold = arguments.Contains("--hold");

        Shape();

        try
        {
            var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
            renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            var application = new Application();
            FluentThemeManager.Apply(application);
            Run(pass, hold);
        }
        catch (Exception exception)
        {
            Note("BOOT threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }

        var logPath = Path.Combine(AppContext.BaseDirectory, $"flyout-{pass}.txt");
        File.WriteAllText(logPath, string.Join(Environment.NewLine, Lines) + Environment.NewLine);
        Console.WriteLine($"{Lines.Count} lines, full log: {logPath}");
        return 0;
    }

    private static void Shape()
    {
        Note("=== S. who declares the paint, and what does it load ===");
        foreach (var type in new[] { typeof(MenuFlyoutItem), typeof(ToggleMenuFlyoutItem), typeof(MenuFlyoutSubItem), typeof(MenuFlyoutSeparator) })
        {
            var owner = type;
            string painter = "-";
            while (owner is not null)
            {
                var method = owner.GetMethod("OnRender", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (method is not null)
                {
                    painter = owner.Name;
                    break;
                }

                owner = owner.BaseType;
            }

            Note($"  {type.Name,-22} sealed={type.IsSealed,-5} base={type.BaseType?.Name,-22} OnRender={painter}");
            Literals(type, "OnRender");
            Literals(type, "MeasureOverride");
        }

        Note($"  MenuFlyoutItem.Icon type: {typeof(MenuFlyoutItem).GetProperty("Icon")?.PropertyType.Name ?? "-"}");
    }

    private static void Run(string pass, bool hold)
    {
        var target = new Button { Content = "target", Width = 120, Height = 32, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        var root = new Grid();
        root.Children.Add(target);

        var window = new Window
        {
            Content = root,
            Width = 760,
            Height = 620,
            Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
        };
        window.Show();
        Pump(20, 1200);

        Measure(target, pass, hold);

        window.Close();
        Pump(4, 400);
    }

    private static void Measure(Button target, string label, bool hold)
    {
        Note($"=== {label} ===");
        var silent = label == "silent";
        var flyout = new MenuFlyout();
        flyout.Items.Add(Item<SilentFlyoutItem>(silent, text: "Rename", accelerator: "F2", icon: true));
        flyout.Items.Add(Item<SilentFlyoutItem>(silent, text: "Archive", accelerator: "Ctrl+Shift+A"));
        flyout.Items.Add(silent
            ? new SilentToggleItem { Text = "Show read receipts", IsChecked = true }
            : new ToggleMenuFlyoutItem { Text = "Show read receipts", IsChecked = true });
        flyout.Items.Add(silent ? new SilentSeparator() : new MenuFlyoutSeparator());
        // MenuFlyoutSubItem is sealed, so its own paint cannot be suppressed; its row is read from the capture.
        flyout.Items.Add(new MenuFlyoutSubItem { Text = "Share with" });
        flyout.Items.Add(Item<SilentFlyoutItem>(silent, text: "Delete", disabled: true));

        if (label is "stripped" or "bare" or "tint")
        {
            var dictionary = (ResourceDictionary)XamlReader.Parse(label == "stripped" ? StrippedMarkup : BareMarkup)!;
            foreach (var item in flyout.Items.OfType<Control>())
            {
                if (dictionary[item.GetType().Name] is ControlTemplate template)
                {
                    item.Template = template;
                }
            }

            if (label == "tint")
            {
                // The bare surface answers one more thing from the same capture: does the framework's own
                // painter read the control's Foreground/FontSize, or only its palette? If a magenta label
                // appears, MenuFlyoutItemForeground is live through the property and the row stays honest;
                // if it stays grey, the row is dead and has to be withdrawn.
                var first = flyout.Items.OfType<MenuFlyoutItem>().First();
                first.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0xFF));
                first.FontSize = 20;
                var toggle = flyout.Items.OfType<ToggleMenuFlyoutItem>().First();
                toggle.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0xFF));
                Note("  tint: row 1 and the toggle row got Foreground=#FF00FF and row 1 FontSize=20");
            }
        }

        flyout.ShowAt(target);
        Pump(30, 2000);
        if (hold)
        {
            Note("  holding the flyout for an outside capture");
            Hold(14);
        }

        // The popped-up rows are not under the Window: the framework hosts them in their own visual root, so
        // the way in is to walk up from a realized item.
        var realized = (DependencyObject)flyout.Items[0]!;
        DependencyObject root = realized;
        for (var parent = VisualTreeHelper.GetParent(realized); parent is not null; parent = VisualTreeHelper.GetParent(root))
        {
            root = parent;
        }

        var presenter = FindFirst(root, static node => node.GetType().Name.Contains("Popup", StringComparison.Ordinal)) ?? root;
        Note($"  walked up from the item to {root.GetType().Name}; presenter found as {presenter.GetType().Name}");

        Note($"  presenter: {presenter.GetType().Name} \"{Name(presenter)}\" {Size(presenter)}");
        Dump(string.Empty, presenter, 0);

        var textNodes = new List<TextBlock>();
        Collect(presenter, textNodes);
        Note($"  TextBlocks in the grafted tree: {textNodes.Count} -> {string.Join(" | ", textNodes.Select(static node => Trim(node.Text ?? string.Empty)))}");

        if (presenter is not FrameworkElement surface)
        {
            return;
        }

        var width = (int)Math.Round(surface.ActualWidth);
        var height = (int)Math.Round(surface.ActualHeight);
        if (width < 4 || height < 4)
        {
            Note($"  no surface to render ({width}x{height})");
            return;
        }

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormat.Bgr32);
        bitmap.Render(surface);
        var buffer = new byte[width * 4 * height];
        bitmap.CopyPixels(new Int32Rect(0, 0, width, height), buffer, width * 4, 0);

        var bright = 0;
        var histogram = new Dictionary<uint, int>();
        for (var offset = 0; offset + 3 < buffer.Length; offset += 4)
        {
            var key = (uint)(buffer[offset + 2] << 16 | buffer[offset + 1] << 8 | buffer[offset]);
            histogram[key] = histogram.GetValueOrDefault(key) + 1;
            if (buffer[offset] > 150 && buffer[offset + 1] > 150 && buffer[offset + 2] > 150) bright++;
        }

        var top = histogram.OrderByDescending(static e => e.Value).ThenBy(static e => e.Key).Take(6);
        Note($"  render {width}x{height} bright(label-ish)={bright} distinct={histogram.Count}");
        Note("      top: " + string.Join("  ", top.Select(e => $"#{e.Key:X6}x{e.Value}")));
        WriteBitmap(buffer, width, height, Path.Combine(AppContext.BaseDirectory, $"flyout-{label}.bmp"));
    }

    private static MenuFlyoutItem Item<TSilent>(bool silent, string text, string? accelerator = null, bool disabled = false, bool icon = false)
        where TSilent : MenuFlyoutItem, new()
    {
        MenuFlyoutItem item = silent ? new TSilent() : new MenuFlyoutItem();
        item.Text = text;
        item.IsEnabled = !disabled;
        if (accelerator is not null)
        {
            item.KeyboardAcceleratorTextOverride = accelerator;
        }

        if (icon)
        {
            // MenuFlyoutItem.Icon is typed System.Object on this runtime, so the skin's ContentPresenter is
            // what presents it - the framework paints no icon at all. A Segoe MDL2 glyph in a TextBlock is
            // what a caller would hand it, and it is the reading the icon slot has to survive.
            item.Icon = new TextBlock
            {
                Text = ((char)0xE710).ToString(),
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0xFF)),
            };
        }

        return item;
    }

    private static DependencyObject? FindFirst(DependencyObject root, Func<DependencyObject, bool> matches)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index)!;
            if (matches(child))
            {
                return child;
            }

            if (FindFirst(child, matches) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    private static void Collect(DependencyObject root, List<TextBlock> into)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index)!;
            if (child is TextBlock textBlock)
            {
                into.Add(textBlock);
            }

            Collect(child, into);
        }
    }

    private static void WriteBitmap(byte[] buffer, int width, int height, string path)
    {
        var stride = width * 3;
        using var writer = new BinaryWriter(File.Create(path));
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
    }

    private static void Dump(string prefix, DependencyObject node, int depth)
    {
        if (depth > 10)
        {
            return;
        }

        var text = node is TextBlock textBlock ? $" text=\"{Trim(textBlock.Text ?? string.Empty)}\"" : string.Empty;
        Note($"    {prefix}{node.GetType().Name}{(Name(node) is { Length: > 0 } name ? $" \"{name}\"" : string.Empty)} {Size(node)}{text}");
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            Dump(prefix + "  ", VisualTreeHelper.GetChild(node, index)!, depth + 1);
        }
    }

    private static string Size(DependencyObject node) => node is FrameworkElement element ? $"{element.ActualWidth:0.#}x{element.ActualHeight:0.#}" : string.Empty;

    private static string Name(DependencyObject node) => node is FrameworkElement element ? element.Name ?? string.Empty : string.Empty;

    private static void Literals(Type owner, string methodName)
    {
        var method = owner.GetMethod(methodName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method?.GetMethodBody()?.GetILAsByteArray() is not { } il)
        {
            return;
        }

        var literals = new List<string>();
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
        if (kept.Count > 0)
        {
            Note($"      {owner.Name}.{methodName} literals: {string.Join(", ", kept)}");
        }
    }

    /// <summary>
    /// Keeps the current visual state on screen for a while. The release is posted from a thread timer onto the
    /// dispatcher captured here, because a held popup answers no frames and a deadline checked only in a
    /// Rendering handler would then never fire (docs/astra/adaptation/06, the Slider batch's watchdog defect).
    /// </summary>
    private static void Hold(int seconds)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var deadline = DateTime.UtcNow.AddSeconds(seconds);
        void OnRendering(object? sender, EventArgs arguments)
        {
            if (DateTime.UtcNow > deadline) frame.Continue = false;
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        using var timer = new System.Threading.Timer(
            _ => dispatcher.InvokeAsync(() => frame.Continue = false),
            null,
            TimeSpan.FromSeconds(seconds + 1),
            Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
    }

    private static int Pump(int frames = 8, int budgetMilliseconds = 800)
    {
        var frame = new DispatcherFrame();
        var seen = 0;
        var deadline = DateTime.UtcNow.AddMilliseconds(budgetMilliseconds);
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames || DateTime.UtcNow > deadline) frame.Continue = false;
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        // A static scene stops answering frames, so the budget has to be enforced from outside the pump as
        // well - a deadline only the Rendering handler can see is a hang once nothing renders (the same
        // watchdog defect adaptation/06 records for the Slider batch).
        using var timer = new System.Threading.Timer(
            _ => Dispatcher.CurrentDispatcher.InvokeAsync(() => frame.Continue = false),
            null,
            TimeSpan.FromMilliseconds(budgetMilliseconds + 250),
            Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
        return seen;
    }

    private static void Note(string line) => Lines.Add(line);

    private static string Trim(string text) => text.Replace('\r', ' ').Replace('\n', ' ').Trim();

    /// <summary>
    /// The surface-only skin: everything the four row types paint themselves is gone from here - no label, no
    /// accelerator text, no check mark, no chevron, no separator line - so the control's own OnRender is the
    /// single painter of those, and the capture says whether its geometry is usable.
    /// </summary>
    private const string StrippedMarkup = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <ControlTemplate x:Key='MenuFlyoutItem' TargetType='MenuFlyoutItem'>
            <Border Name='LayoutRoot' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}'
                    BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='{TemplateBinding CornerRadius}'
                    Padding='{TemplateBinding Padding}' Margin='{ThemeResource MenuFlyoutItemMargin}'>
              <Border Name='IconRoot' Width='16' Height='16' Margin='0,0,12,0' HorizontalAlignment='Left' VerticalAlignment='Center'>
                <ContentPresenter Name='IconContent' Content='{TemplateBinding Icon}' Foreground='{TemplateBinding Foreground}' />
              </Border>
            </Border>
          </ControlTemplate>
          <ControlTemplate x:Key='ToggleMenuFlyoutItem' TargetType='ToggleMenuFlyoutItem'>
            <Border Name='LayoutRoot' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}'
                    BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='{TemplateBinding CornerRadius}'
                    Padding='{TemplateBinding Padding}' Margin='{ThemeResource MenuFlyoutItemMargin}'>
              <Border Name='IconRoot' Width='16' Height='16' Margin='0,0,12,0' HorizontalAlignment='Left' VerticalAlignment='Center'>
                <ContentPresenter Name='IconContent' Content='{TemplateBinding Icon}' Foreground='{TemplateBinding Foreground}' />
              </Border>
            </Border>
          </ControlTemplate>
          <ControlTemplate x:Key='MenuFlyoutSubItem' TargetType='MenuFlyoutSubItem'>
            <Border Name='LayoutRoot' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}'
                    BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='{TemplateBinding CornerRadius}'
                    Padding='{TemplateBinding Padding}' Margin='{ThemeResource MenuFlyoutItemMargin}'>
              <Border Name='IconRoot' Width='16' Height='16' Margin='0,0,12,0' HorizontalAlignment='Left' VerticalAlignment='Center'>
                <ContentPresenter Name='IconContent' Content='{TemplateBinding Icon}' Foreground='{TemplateBinding Foreground}' />
              </Border>
            </Border>
          </ControlTemplate>
          <ControlTemplate x:Key='MenuFlyoutSeparator' TargetType='MenuFlyoutSeparator'>
            <Border Background='Transparent' Margin='{TemplateBinding Padding}' Height='1' />
          </ControlTemplate>
        </ResourceDictionary>
        """;

    /// <summary>
    /// The surface with nothing inside it, not even the icon slot: the reading this gives is whether the
    /// control paints the icon as well, which is the one flyout element the shipped template still owns.
    /// </summary>
    private const string BareMarkup = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
          <ControlTemplate x:Key='MenuFlyoutItem' TargetType='MenuFlyoutItem'>
            <Border Name='LayoutRoot' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}'
                    BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='{TemplateBinding CornerRadius}'
                    Padding='{TemplateBinding Padding}' Margin='{ThemeResource MenuFlyoutItemMargin}' />
          </ControlTemplate>
          <ControlTemplate x:Key='ToggleMenuFlyoutItem' TargetType='ToggleMenuFlyoutItem'>
            <Border Name='LayoutRoot' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}'
                    BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='{TemplateBinding CornerRadius}'
                    Padding='{TemplateBinding Padding}' Margin='{ThemeResource MenuFlyoutItemMargin}' />
          </ControlTemplate>
          <ControlTemplate x:Key='MenuFlyoutSubItem' TargetType='MenuFlyoutSubItem'>
            <Border Name='LayoutRoot' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}'
                    BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='{TemplateBinding CornerRadius}'
                    Padding='{TemplateBinding Padding}' Margin='{ThemeResource MenuFlyoutItemMargin}' />
          </ControlTemplate>
          <ControlTemplate x:Key='MenuFlyoutSeparator' TargetType='MenuFlyoutSeparator'>
            <Border Background='Transparent' Margin='{TemplateBinding Padding}' Height='1' />
          </ControlTemplate>
        </ResourceDictionary>
        """;
}
