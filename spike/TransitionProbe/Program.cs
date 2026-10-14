using System.Diagnostics;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Input;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace TransitionProbe;

/// <summary>
/// Task #22: does a transition's end value reach the composited frame, and which variable decides it?
/// The spacing batch concluded "nested Border + brush transition does not land" from one app-bar cell, but
/// that conclusion is load-bearing for 19 more sites in src/FluentJalium, so it gets a matrix instead of an
/// extrapolation. Each cell differs from its neighbour by exactly one variable and carries a unique colour,
/// so an external PrintWindow capture answers by counting that colour anywhere in the window - 0 means the
/// frame never got the value, the cell's area means it did.
///   1/2   highlight is the template root, with a brush transition vs without
///   3/4   highlight one level down, with a transition vs without   (3 is the shape the spacing batch killed)
///   5     nested, transition declared with zero duration
///   6     nested, value written after the window is shown instead of at load
///   7     nested, brush from {ThemeResource} instead of a literal
///   8     nested, a *size* transition (Width 30 -> 90) instead of a brush
///   9     nested, the same property fed by BOTH a TemplateBinding and a trigger setter - the app-bar cell
///   10    a nested border in plain parsed content, outside any template
///   11/12 the shipping Astra Button skin (nested Surface + 83 ms) written before vs after show
/// Every pixel reading is paired with a property read-back of the same border in the same run, because the
/// two channels disagreed once already - and the first run of this probe proved they still can, in the
/// other direction: its cells never fired, because a ControlTemplate that lives in a dictionary keeps its
/// state cells unresolved (audits/slider.md), so it read back the resting colour for all nine templates.
/// The cells are therefore keyed STYLES with the template inline, which is the shipping shape.
/// </summary>
internal static class Program
{
    private static readonly Color Rest = Color.FromRgb(0x1E, 0x1E, 0x1F);
    private static readonly List<Cell> Cells = [];
    private static readonly Stopwatch Watch = new();

    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "transition-probe.txt");
    private static readonly string MarkerPath = Path.Combine(AppContext.BaseDirectory, "transition-probe.marks");

    private static Application _application = null!;
    private static ResourceDictionary _probe = null!;
    private static bool _ran;

    private sealed record Cell(string Name, Color Expect, Func<Border?> Host, Action? AfterShow = null);

    [STAThread]
    private static int Main()
    {
        File.Delete(LogPath);
        File.Delete(MarkerPath);
        Watch.Start();
        try
        {
            var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
            renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
            ThemeLoader.Initialize();
            var application = new Application();
            _application = application;
            FluentThemeManager.Apply(application);
            Run(application);
        }
        catch (Exception exception)
        {
            Log("BOOT threw " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
        }

        Log($"EXIT at {Watch.ElapsedMilliseconds}ms");
        Mark("exit");
        return 0;
    }

    private static void Run(Application application)
    {
        _probe = (ResourceDictionary)XamlReader.Parse(Markup)!;
        application.Resources.MergedDictionaries.Add(_probe);

        var panel = new WrapPanel
        {
            Width = 1060,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(12),
        };

        Add(panel, "01 root+trans", 0xFF0001, "RootTrans", checkedAtLoad: true);
        Add(panel, "02 root none", 0x7F0001, "RootPlain", checkedAtLoad: true);
        Add(panel, "03 nested+trans", 0xFE00FE, "NestTrans", checkedAtLoad: true);
        Add(panel, "04 nested none", 0x0300FF, "NestPlain", checkedAtLoad: true);
        Add(panel, "05 nested zero", 0xB400B4, "NestZero", checkedAtLoad: true);
        Add(panel, "06 nested after", 0x00B4B4, "NestAfter", checkedAtLoad: false);
        Add(panel, "07 nested themed", 0x60CDFF, "NestTheme", checkedAtLoad: true);
        // Cell 8 drives a width, not a checked state, so its after-show write is its own.
        var sizedHost = Add(panel, "08 nested width", 0x7F7F01, "NestSize", checkedAtLoad: true);
        Cells[^1] = Cells[^1] with { AfterShow = () => { if (sizedHost() is { } border) { border.Width = 90; } } };
        Add(panel, "09 bound+cell", 0xFF0101, "NestBoundTrigger", checkedAtLoad: true);

        // 13-17: the first pass proved nested + transition + {ThemeResource} + TemplateBinding all land, so
        // the app-bar symptom has to come from a variable the first matrix never carried. Each cell below
        // differs from 03 by exactly one of them: a border on the transitioning element, the two-name
        // property list, the space inside that list, the list on a template root, and the app-bar's own
        // three-layer shape with all of them together.
        Add(panel, "13 single+border", 0x01FF01, "NestSingleBorder", checkedAtLoad: true);
        Add(panel, "14 two names", 0x0101FF, "NestTwoNames", checkedAtLoad: true);
        Add(panel, "15 no space", 0xEE00EE, "NestNoSpace", checkedAtLoad: true);
        Add(panel, "16 root two names", 0x00EEEE, "RootTwoNames", checkedAtLoad: true);
        Add(panel, "17 appbar shape", 0xC86400, "AppBarShape", checkedAtLoad: true);

        // 10: the same brush transition on a nested border nothing templated.
        var looseHost = (FrameworkElement)XamlReader.Parse(LooseMarkup)!;
        panel.Children.Add(looseHost);
        var looseExpect = Color.FromRgb(0xFF, 0x7F, 0x01);
        Border? Loose() => FirstBorder(looseHost);
        Cells.Add(new Cell("10 loose border", looseExpect, Loose, () =>
        {
            if (Loose() is { } border)
            {
                border.Background = new SolidColorBrush(looseExpect);
            }
        }));

        // 11/12: the shipping Astra button skin, which is itself a nested Surface + 83 ms transition.
        var stockBefore = new Button { Width = 90, Height = 90, Margin = new Thickness(8), Background = new SolidColorBrush(Color.FromRgb(0x0F, 0xFF, 0x5F)) };
        var stockAfter = new Button { Width = 90, Height = 90, Margin = new Thickness(8), Background = new SolidColorBrush(Rest) };
        panel.Children.Add(stockBefore);
        panel.Children.Add(stockAfter);
        Cells.Add(new Cell("11 stock button pre", Color.FromRgb(0x0F, 0xFF, 0x5F), () => Part(stockBefore, "Surface") as Border));
        Cells.Add(new Cell("12 stock button post", Color.FromRgb(0x5F, 0x0F, 0xFF), () => Part(stockAfter, "Surface") as Border, () => stockAfter.Background = new SolidColorBrush(Color.FromRgb(0x5F, 0x0F, 0xFF))));

        foreach (var cell in Cells)
        {
            Log($"EXPECT\t{cell.Name}\t{Hex(cell.Expect)}");
        }

        var accent = Res("AccentFillColorDefaultBrush") as SolidColorBrush;
        Log($"NOTE\taccent row\t{(accent is null ? "absent" : Hex(accent.Color))}");

        var root = new Grid { Background = new SolidColorBrush(Color.FromRgb(0x10, 0x10, 0x10)) };
        root.Children.Add(panel);
        var window = new Window { Content = root, Width = 1130, Height = 400, Title = "Transition probe" };

        window.Loaded += (_, _) =>
        {
            if (_ran)
            {
                return;
            }

            _ran = true;
            var phase = Stopwatch.StartNew();
            try
            {
                Hold(phase, 400);
                Read(phase, "settle 400ms");
                LogCensus();

                foreach (var cell in Cells)
                {
                    cell.AfterShow?.Invoke();
                }

                Phase(phase, 1200, "ready", root, panel);
                Phase(phase, 3000, "capture1", root, panel);
                Phase(phase, 5400, "capture2", root, panel);
                Phase(phase, 7800, "capture3", root, panel);
                Read(phase, "after-captures");
                Mark("final");

                // Leave the window up long enough for the last grab: the script reads the mark, then asks
                // the surface for a frame, and a window destroyed in between answers no rect at all.
                Hold(phase, 19000);
            }
            catch (Exception exception)
            {
                Log("THREW " + exception.GetType().Name + ": " + Trim(exception.InnerException?.Message ?? exception.Message));
            }
            finally
            {
                Mark("close");
                window.Close();
            }
        };

        application.Run(window);
    }

    private static Func<Border?> Add(Panel panel, string name, int expect, string styleKey, bool checkedAtLoad)
    {
        var color = Color.FromRgb((byte)(expect >> 16), (byte)(expect >> 8), (byte)expect);
        var toggle = new ToggleButton
        {
            Width = 90,
            Height = 90,
            Margin = new Thickness(8),
            Style = (Style)_probe[styleKey]!,
            Background = new SolidColorBrush(Rest),
            IsChecked = checkedAtLoad,
        };
        var host = new Func<Border?>(() => Part(toggle, "Fill") as Border);
        Cells.Add(new Cell(name, color, host, checkedAtLoad ? null : () => toggle.IsChecked = true));
        panel.Children.Add(toggle);
        return host;
    }

    /// <summary>What the tree under each control actually is, so a null part reading is a measurement and
    /// not a guess.</summary>
    private static void LogCensus()
    {
        Log("CENSUS");
        foreach (var cell in Cells)
        {
            var border = cell.Host();
            var owner = border?.Parent as DependencyObject ?? border;
            var size = border is null ? "-" : $"{border.ActualWidth:0}x{border.ActualHeight:0}";
            Log($"  {cell.Name}\tpart={(border is null ? "missing" : "ok")}\tsize={size}\tnames={NamesOf(owner)}");
        }
    }

    private static string NamesOf(DependencyObject? root)
    {
        if (root is Control control)
        {
            var style = control.Style?.GetType().Name ?? "no-style";
            var template = control.Template is null ? "no-template" : "template";
            return $"{style}/{template} :: {NamesUnder(control)}";
        }

        return NamesUnder(root as Visual);
    }

    private static string NamesUnder(Visual? root)
    {
        if (root is null)
        {
            return "-";
        }

        var names = new List<string>();
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            names.Add((child as FrameworkElement)?.Name is { Length: > 0 } name ? name : child.GetType().Name);
            names.Add(NamesUnder(child as Visual));
        }

        return string.Join(",", names.Where(part => part.Length > 0));
    }

    private static void Read(Stopwatch phase, string label)
    {
        Log($"READ\t{label}\t{phase.ElapsedMilliseconds}ms\t(wall {Watch.ElapsedMilliseconds}ms)");
        foreach (var cell in Cells)
        {
            var border = cell.Host();
            Log($"  {cell.Name}\t{(border is null ? "part missing" : Brush(border.Background))}");
        }
    }

    private static void Hold(Stopwatch phase, double milliseconds)
    {
        var remaining = Math.Max(0, milliseconds - phase.ElapsedMilliseconds);
        var budget = remaining + 3000;
        var frames = PumpUntil(phase, milliseconds, budget);
        Log($"HOLD\t{phase.ElapsedMilliseconds}ms\t(wall {Watch.ElapsedMilliseconds}ms)\tpump gave {frames} frames");
    }

    private static int _nudge;

    /// <summary>
    /// A capture phase the script can align to. Once a scene is static the framework stops raising
    /// CompositionTarget.Rendering, and the second and third PrintWindow grabs of the first pass came back
    /// with every cell at zero - a surface that has not drawn recently gives a blank frame, which would be
    /// indistinguishable from a value that never landed. So each phase nudges the layout, waits for real
    /// frames, and only then marks the capture.
    /// </summary>
    private static void Phase(Stopwatch phase, double milliseconds, string label, UIElement root, FrameworkElement panel)
    {
        // Nudge first and only then wait: the grab wants a surface that has drawn since its last request,
        // and a nudge that lands after the wait leaves the re-layout in flight when the script reads it.
        _nudge = _nudge == 0 ? 1 : 0;
        panel.Margin = new Thickness(12 + _nudge);
        root.InvalidateVisual();
        Hold(phase, milliseconds);
        var frames = PumpFrames(6, 900);
        Log($"PHASE	{label}	{phase.ElapsedMilliseconds}ms	(wall {Watch.ElapsedMilliseconds}ms)	nudge={_nudge}	frames={frames}");
        Mark(label);
    }

    private static object? Res(string key)
    {
        try
        {
            return _application.TryFindResource(key);
        }
        catch (Exception exception)
        {
            return "threw " + exception.GetType().Name;
        }
    }

    private static void Log(string line) => File.AppendAllText(LogPath, line + Environment.NewLine);

    private static void Mark(string label) => File.AppendAllText(MarkerPath, $"{Watch.ElapsedMilliseconds}\t{label}{Environment.NewLine}");

    private static Border? FirstBorder(Visual? root)
    {
        if (root is null)
        {
            return null;
        }

        if (root is Border border)
        {
            return border;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            if (FirstBorder(VisualTreeHelper.GetChild(root, index) as Visual) is { } match)
            {
                return match;
            }
        }

        return null;
    }

    private static FrameworkElement? Part(DependencyObject? root, string name)
    {
        if (root is not Visual visual)
        {
            return null;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(visual); index++)
        {
            var child = VisualTreeHelper.GetChild(visual, index);
            if (child is FrameworkElement element && element.Name == name)
            {
                return element;
            }

            if (Part(child, name) is { } match)
            {
                return match;
            }
        }

        return null;
    }

    private static string Brush(Brush? brush) =>
        brush is SolidColorBrush solid ? Hex(solid.Color) : brush is null ? "-" : brush.GetType().Name;

    private static string Hex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    private static string Trim(string text) => text.Replace('\r', ' ').Replace('\n', ' ').Trim();

    /// <summary>Real rendered frames, until the phase clock reaches its boundary: a transition only gets a
    /// chance to land if this thread actually lets the render loop run.</summary>
    private static int PumpUntil(Stopwatch phase, double milliseconds, double budgetMilliseconds)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var seen = 0;
        // Both limits are absolute on the phase clock: comparing an elapsed value against a budget measured
        // from now makes every later phase return at once, which is how one run packed three capture phases
        // into 160 ms and destroyed the window while the first grab was still reading it.
        var stop = Math.Max(milliseconds, phase.ElapsedMilliseconds + budgetMilliseconds);
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (phase.ElapsedMilliseconds >= milliseconds || phase.ElapsedMilliseconds > stop)
            {
                frame.Continue = false;
            }
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        using var watchdog = new System.Threading.Timer(_ => dispatcher.InvokeAsync(() => frame.Continue = false));
        watchdog.Change(TimeSpan.FromMilliseconds(budgetMilliseconds + 1500), System.Threading.Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
        return seen;
    }

    /// <summary>Frames, not wall clock: used to prove the surface is drawing again before a capture.</summary>
    private static int PumpFrames(int frames, double budgetMilliseconds)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var seen = 0;
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames)
            {
                frame.Continue = false;
            }
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        using var watchdog = new System.Threading.Timer(_ => dispatcher.InvokeAsync(() => frame.Continue = false));
        watchdog.Change(TimeSpan.FromMilliseconds(budgetMilliseconds), System.Threading.Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
        return seen;
    }

    private const string LooseMarkup = """
        <Border xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                Width='106' Height='106' Margin='8' Background='#FF1E1E1F'
                TransitionProperty='Background' TransitionDuration='0:0:0.083' />
        """;

    /// <summary>Every case is a keyed Style with the template inline: a keyed ControlTemplate would keep its
    /// own state cells unresolved and the matrix would measure nothing (audits/slider.md).</summary>
    private const string Markup = """
        <ResourceDictionary xmlns='https://schemas.jalium.dev/jalxaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>

          <!-- 1: the highlight IS the template root, with a brush transition (the shape the old committed
               command-bar capture proved lands). -->
          <Style x:Key='RootTrans' TargetType='ToggleButton'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ToggleButton'>
                <Border x:Name='Fill' Background='#FF1E1E1F' TransitionProperty='Background' TransitionDuration='0:0:0.083' />
                <ControlTemplate.Triggers>
                  <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Fill' Property='Background' Value='#FFFF0001' />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>

          <!-- 2: same, no transition. -->
          <Style x:Key='RootPlain' TargetType='ToggleButton'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ToggleButton'>
                <Border x:Name='Fill' Background='#FF1E1E1F' />
                <ControlTemplate.Triggers>
                  <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Fill' Property='Background' Value='#FF7F0001' />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>

          <!-- 3/4/6: a hit-test grid outside the highlight, i.e. the highlight is one level down - the
               spacing batch's suspect shape. -->
          <Style x:Key='NestTrans' TargetType='ToggleButton'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ToggleButton'>
                <Grid Background='Transparent'>
                  <Border x:Name='Fill' Background='#FF1E1E1F' TransitionProperty='Background' TransitionDuration='0:0:0.083' />
                </Grid>
                <ControlTemplate.Triggers>
                  <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Fill' Property='Background' Value='#FFFE00FE' />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>

          <Style x:Key='NestAfter' TargetType='ToggleButton'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ToggleButton'>
                <Grid Background='Transparent'>
                  <Border x:Name='Fill' Background='#FF1E1E1F' TransitionProperty='Background' TransitionDuration='0:0:0.083' />
                </Grid>
                <ControlTemplate.Triggers>
                  <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Fill' Property='Background' Value='#FF00B4B4' />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>

          <Style x:Key='NestPlain' TargetType='ToggleButton'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ToggleButton'>
                <Grid Background='Transparent'>
                  <Border x:Name='Fill' Background='#FF1E1E1F' />
                </Grid>
                <ControlTemplate.Triggers>
                  <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Fill' Property='Background' Value='#FF0300FF' />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>

          <!-- 5: transition declared, duration zero. -->
          <Style x:Key='NestZero' TargetType='ToggleButton'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ToggleButton'>
                <Grid Background='Transparent'>
                  <Border x:Name='Fill' Background='#FF1E1E1F' TransitionProperty='Background' TransitionDuration='0:0:0' />
                </Grid>
                <ControlTemplate.Triggers>
                  <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Fill' Property='Background' Value='#FFB400B4' />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>

          <!-- 7: the brush is a palette row, not a literal, as every shipping cell is. -->
          <Style x:Key='NestTheme' TargetType='ToggleButton'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ToggleButton'>
                <Grid Background='Transparent'>
                  <Border x:Name='Fill' Background='#FF1E1E1F' TransitionProperty='Background' TransitionDuration='0:0:0.083' />
                </Grid>
                <ControlTemplate.Triggers>
                  <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Fill' Property='Background' Value='{ThemeResource AccentFillColorDefaultBrush}' />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>

          <!-- 8: a size transition instead of a brush, so the frame answers by area not colour. -->
          <Style x:Key='NestSize' TargetType='ToggleButton'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ToggleButton'>
                <Grid Background='Transparent'>
                  <Border x:Name='Fill' Width='30' Height='90' HorizontalAlignment='Left' Background='#FF7F7F01'
                          TransitionProperty='Width' TransitionDuration='0:0:0.083' />
                </Grid>
              </ControlTemplate>
            </Setter>
          </Style>

          <!-- 9: the app-bar cell reproduced: one property on one nested border, fed by a TemplateBinding
               AND by a trigger setter. -->
          <Style x:Key='NestBoundTrigger' TargetType='ToggleButton'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ToggleButton'>
                <Grid Background='Transparent'>
                  <Border x:Name='Fill' Background='{TemplateBinding Background}' TransitionProperty='Background' TransitionDuration='0:0:0.083' />
                </Grid>
                <ControlTemplate.Triggers>
                  <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Fill' Property='Background' Value='#FFFF0101' />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>
          <!-- 13-17: the variables the first pass left out, each one step away from cell 3. -->
          <Style x:Key='NestSingleBorder' TargetType='ToggleButton'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ToggleButton'>
                <Grid Background='Transparent'>
                  <Border x:Name='Fill' Background='#FF1E1E1F' BorderBrush='#FF808082' BorderThickness='1'
                          Padding='2,6,2,6' TransitionProperty='Background' TransitionDuration='0:0:0.083' />
                </Grid>
                <ControlTemplate.Triggers>
                  <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Fill' Property='Background' Value='#FF01FF01' />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>

          <Style x:Key='NestTwoNames' TargetType='ToggleButton'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ToggleButton'>
                <Grid Background='Transparent'>
                  <Border x:Name='Fill' Background='#FF1E1E1F' TransitionProperty='Background, BorderBrush' TransitionDuration='0:0:0.083' />
                </Grid>
                <ControlTemplate.Triggers>
                  <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Fill' Property='Background' Value='#FF0101FF' />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>

          <Style x:Key='NestNoSpace' TargetType='ToggleButton'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ToggleButton'>
                <Grid Background='Transparent'>
                  <Border x:Name='Fill' Background='#FF1E1E1F' TransitionProperty='Background,BorderBrush' TransitionDuration='0:0:0.083' />
                </Grid>
                <ControlTemplate.Triggers>
                  <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Fill' Property='Background' Value='#FFEE00EE' />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>

          <Style x:Key='RootTwoNames' TargetType='ToggleButton'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ToggleButton'>
                <Border x:Name='Fill' Background='#FF1E1E1F' TransitionProperty='Background, BorderBrush' TransitionDuration='0:0:0.083' />
                <ControlTemplate.Triggers>
                  <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Fill' Property='Background' Value='#FF00EEEE' />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>

          <!-- 17: the shipping app-bar cell, all of it: hit grid, an inner highlight inset 2,6,2,6 with a
               border and padding, a content layer with the 64 minimum, and the two-name transition list.
               Its fill rect is 78x86 of the 90x90 cell, so the frame area to expect is smaller than the
               other cells - about 136x151 physical pixels at 175%. -->
          <Style x:Key='AppBarShape' TargetType='ToggleButton'>
            <Setter Property='Template'>
              <ControlTemplate TargetType='ToggleButton'>
                <Grid Name='Root' Background='Transparent'>
                  <Border Name='Fill' Margin='2,6,2,6' Background='{TemplateBinding Background}'
                          BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='1'
                          Padding='2,6,2,6' TransitionProperty='Background, BorderBrush' TransitionDuration='0:0:0.083' />
                  <Grid Name='ContentRoot' MinHeight='64'>
                    <TextBlock Name='LabelText' Text='bold' HorizontalAlignment='Center' VerticalAlignment='Bottom' />
                  </Grid>
                </Grid>
                <ControlTemplate.Triggers>
                  <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Fill' Property='Background' Value='#FFC86400' />
                  </Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate>
            </Setter>
          </Style>
        </ResourceDictionary>
        """;
}
