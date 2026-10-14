using System.Runtime.InteropServices;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace HostWindowProbe;

/// <summary>
/// Why does a ContentDialog resolve its host window when its class runs alone and stop resolving it part-way
/// through a sequential suite run (#35)? The shipped resolver is a two-step lookup: the Win32 active window of the
/// calling thread, then <c>Application.Current.MainWindow</c>. The test harness shows its host window and never
/// assigns MainWindow (only <c>Application.Run(window)</c> does), so every dialog in this repository hangs on the
/// first step - and a thread's active window goes away when the window that held it closes.
///
/// Three readings, one process, in the order the suite would meet them:
///   A  host window shown, nothing else                - what a single-class run looks like
///   B  a second window shown and then closed          - what a neighbour's teardown does to this thread
///   C  B, then Application.Current.MainWindow assigned - the candidate fix
///
/// Each reading prints the ambient handles and the outcome of a real ShowAsync, so the cause and the fix are
/// measured on 26.10.9 rather than inferred from a reference source tree that may be newer.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];

    [DllImport("user32.dll")]
    private static extern nint GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [STAThread]
    private static int Main()
    {
        var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
        renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;
        ThemeLoader.Initialize();
        var application = new Application();
        FluentThemeManager.Apply(application);

        var host = new Window { Content = new Grid(), Width = 420, Height = 300, Title = "Probe host" };
        host.Show();
        Pump(8);
        Note(string.Empty);
        Note($"A  host shown, MainWindow unassigned: host={Hex(HandleOf(host))} active={Hex(GetActiveWindow())} " +
             $"foreground={Hex(GetForegroundWindow())} app.MainWindow={Describe(application.MainWindow)}");
        TryDialog(application);

        var neighbour = new Window { Content = new Grid(), Width = 200, Height = 120, Title = "Probe neighbour" };
        neighbour.Show();
        Pump(8);
        Note($"   neighbour shown: neighbour={Hex(HandleOf(neighbour))} active={Hex(GetActiveWindow())} " +
             $"app.MainWindow={Describe(application.MainWindow)}");
        TryDialog(application);
        neighbour.Close();
        Pump(8);
        Note($"B  neighbour closed: active={Hex(GetActiveWindow())} foreground={Hex(GetForegroundWindow())} " +
             $"app.MainWindow={Describe(application.MainWindow)}");
        TryDialog(application);

        application.MainWindow = host;
        Note($"C  app.MainWindow = host: active={Hex(GetActiveWindow())} " +
             $"app.MainWindow={Describe(application.MainWindow)}");
        TryDialog(application);

        host.Close();
        Pump(4);
        var path = Path.Combine(AppContext.BaseDirectory, "hostwindow-probe.txt");
        File.WriteAllLines(path, Lines);
        foreach (var line in Lines)
        {
            Console.WriteLine(line);
        }

        Console.WriteLine($"wrote {path}");
        return 0;
    }

    private static void TryDialog(Application application)
    {
        var dialog = new ContentDialog { Title = "T", Content = "C", PrimaryButtonText = "OK" };
        try
        {
            dialog.ShowAsync();
            Pump(8);
            Note($"   dialog opened: Visibility={dialog.Visibility}");
            try
            {
                dialog.Hide();
            }
            catch (Exception hide)
            {
                Note($"   dialog Hide threw: {hide.GetType().Name}: {hide.Message}");
            }

            Pump(8);
        }
        catch (Exception failure)
        {
            Note($"   dialog threw: {failure.GetType().Name}: {failure.Message}");
        }
    }

    private static nint HandleOf(Window window) => window.Handle;

    private static string Describe(Window? window) => window is null ? "null" : window.Title ?? "unnamed";

    private static string Hex(nint handle) => handle == nint.Zero ? "0" : $"0x{handle:X}";

    private static void Note(string line)
    {
        Lines.Add(line);
    }

    private static void Pump(int frames = 4, int budgetMilliseconds = 800)
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
