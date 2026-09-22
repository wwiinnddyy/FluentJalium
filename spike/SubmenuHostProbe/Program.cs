using System.Reflection;
using System.Runtime.InteropServices;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace SubmenuHostProbe;

/// <summary>
/// #47 asks why `AstraMenuTests.A_submenu_surface_wears_upstreams_radius_from_the_framework_itself` reads
/// `IsSubmenuOpen` back as false part-way through a sequential suite run while the same test is green alone.
/// The cause registered for the family (#35, `adaptation/06` "宿主窗口是谁给的") is a persistent thread state, not a
/// race: closing whatever window held Win32 activation leaves this thread's active window at 0 forever, and the
/// harness proved that a ContentDialog resolves its host through exactly that handle. A submenu popup is the same
/// kind of surface - and unlike the dialog it is light-dismiss, so it also has a reason to be closed from outside.
///
/// Four readings, one process, one host window that is never closed - the shape the suite is in:
///   P  nothing has been closed yet                    - what a single-class run looks like
///   F  a MenuFlyout shown and hidden on the host      - the teardown the suite really performs (popups build
///                                                      their own top-level window, and hiding it closes it)
///   N  a second Window shown and then closed          - the poison #35 measured
///   A  N, then host.Activate()                        - does taking activation back restore the read?
///
/// Each reading mounts a fresh Menu, drives the same synthetic mouse-down the test drives, and reports the
/// per-frame trajectory of `IsSubmenuOpen` plus, for every write, the framework frames that made it - so
/// "never opened" and "opened then closed by something else" cannot be confused, and the closer is named.
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];
    private static readonly List<string> Writes = [];

    private static Window _host = null!;

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

        // The one hook the probe needs: who writes IsSubmenuOpen, and from where. Registered against a private
        // subclass because the framework refuses a second registration for the owner type itself. Baseline reading
        // P is what proves the instrument is honest: if the derived callback replaced MenuItem's own, the submenu
        // would never open and P would read false.
        MenuItem.IsSubmenuOpenProperty.OverrideMetadata(typeof(ProbeMenuItem), new PropertyMetadata(false, OnSubmenuOpen));

        _host = new Window { Content = new Grid(), Width = 420, Height = 300, Title = "Probe host" };
        _host.Show();
        Pump(8);
        if (application.MainWindow is null) application.MainWindow = _host;

        Note("=== ambient handles, and the submenu read after each neighbour teardown ===");
        Note(string.Empty);
        Note($"P  baseline                 : {Ambient()}");
        Read("P");

        Note(string.Empty);
        Note($"F  flyout shown+hidden      : {Ambient()}");
        FlyoutOnHost();
        Note($"   after flyout teardown    : {Ambient()}");
        Read("F");

        Note(string.Empty);
        var neighbour = new Window { Content = new Grid(), Width = 200, Height = 120, Title = "Probe neighbour" };
        neighbour.Show();
        Pump(8);
        Note($"N  neighbour shown          : {Ambient()}");
        neighbour.Close();
        Pump(8);
        Note($"   neighbour closed         : {Ambient()}");
        Read("N");

        Note(string.Empty);
        _host.Activate();
        Pump(8);
        Note($"A  host.Activate()          : {Ambient()}");
        Read("A");

        // None of the neighbour shapes reproduced the red, so the remaining suspect is the pump itself:
        // `Pump(frames, budget)` stops at whichever comes first, so a build that asks for four frames can get one
        // when frames are late - which is what a loaded sequential run does to every fixed frame count in this
        // harness. The ladder below asks how much build time the click actually needs.
        Note(string.Empty);
        Note("=== how many frames between mounting the Menu and the click landing ===");
        Ladder();

        // The framework itself contains one call that closes every light-dismiss popup in a window: showing a
        // ContentDialog does it (`ContentDialog.cs:343` in the 26.10.9 source). A submenu popup is light-dismiss,
        // and its `IsSubmenuOpen` is written back from the popup's own Closed event - so a dialog that opens while
        // a submenu is up, including one a neighbouring test left resolving late, is a named closer.
        Note(string.Empty);
        Note("=== the submenu is up, then a ContentDialog is shown on the same thread ===");
        ReadWithDialog();

        // The dialog above is a call the suite can see coming. The other closer in the shipped framework is
        // `WindowInputDispatcher.CloseLightDismissPopupsOnDeactivate` - our window losing the foreground. A test run
        // keeps one host window alive for minutes, so anything that takes focus mid-run (another process, another
        // session's window, a hand on the mouse) is a closer no test can rule out. Reading E manufactures exactly
        // that change in-process, after the submenu is already up.
        Note(string.Empty);
        Note("=== the submenu is up, then another window takes the foreground ===");
        ReadWithDeactivate();

        // E's closer came from outside the process. The suite also has an in-process candidate: a flyout or context
        // menu realises its popup in a top-level window of its own, and hiding it destroys that window. If that
        // teardown closes a submenu that is still up, the cause is our own overlapping popups and housekeeping can
        // reach it; if it does not, only an external focus change can, and the read is environmental.
        Note(string.Empty);
        Note("=== the submenu is up, then a flyout's own window is torn down around it ===");
        ReadWithPopupTeardown();

        Note(string.Empty);
        _host.Close();
        Pump(4);
        var path = Path.Combine(AppContext.BaseDirectory, "submenu-host-probe.txt");
        File.WriteAllLines(path, Lines);
        foreach (var line in Lines)
        {
            Console.WriteLine(line);
        }

        Console.WriteLine($"wrote {path}");
        return 0;
    }

    /// <summary>The test's exact move: mount a Menu with one submenu-bearing item, click it, watch the read.</summary>
    private static void Read(string label)
    {
        Writes.Clear();
        var top = new ProbeMenuItem { Header = "edit" };
        top.Items.Add(new MenuItem { Header = "copy" });
        var menu = new Menu();
        menu.Items.Add(top);
        Mount(menu, 320, 40);

        var before = top.IsSubmenuOpen;
        RaiseMouseDown(top);
        Pump(30);
        var after = top.IsSubmenuOpen;

        Note($"   {label} read: IsSubmenuOpen before={before} after={after} popupOpen={PopupOpenOf(top)} " +
             $"writes={Writes.Count}");
        foreach (var write in Writes)
        {
            Note($"      {write}");
        }
    }

    private static void FlyoutOnHost()
    {
        var anchor = new Button { Content = "host", Width = 140 };
        Mount(anchor, 140, 32);
        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuFlyoutItem { Text = "cut" });
        flyout.ShowAt(anchor);
        Pump(20);
        flyout.Hide();
        Pump(20);
    }

    private static void OnSubmenuOpen(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ProbeMenuItem)
        {
            return;
        }

        if (e.NewValue is true)
        {
            Opened.Add((MenuItem)d);
        }

        var closer = Environment.StackTrace
            .Split('\n')
            .Skip(2)
            .Select(static line => line.Trim())
            .Where(static line => line.Contains("Jalium.UI"))
            .Take(6);
        Writes.Add($"IsSubmenuOpen {e.OldValue}->{e.NewValue} :: {string.Join(" <- ", closer)}");
    }

    /// <summary>Every submenu this probe opened and did not close - the same population our suite leaks.</summary>
    private static readonly List<MenuItem> Opened = [];

    /// <summary>Submenu up, then the host loses the foreground to another window while the read is still pending.</summary>
    private static void ReadWithDeactivate()
    {
        // Close what the earlier readings left up, so the write below belongs to this reading alone. The count is
        // itself a reading: every one of those is a light-dismiss root still registered on the shared host.
        var left = Opened.Count(static item => item.IsSubmenuOpen);
        foreach (var item in Opened)
        {
            item.IsSubmenuOpen = false;
        }

        Pump(6);
        Note($"   roots left open by earlier readings: {left} (closed before this reading)");

        Writes.Clear();
        var top = new ProbeMenuItem { Header = "edit" };
        top.Items.Add(new MenuItem { Header = "copy" });
        var menu = new Menu();
        menu.Items.Add(top);
        Mount(menu, 320, 40);
        RaiseMouseDown(top);
        Pump(10);
        Note($"   submenu up            : read={top.IsSubmenuOpen} popupOpen={PopupOpenOf(top)}");

        var neighbour = new Window { Content = new Grid(), Width = 200, Height = 120, Title = "Probe neighbour" };
        neighbour.Show();
        Pump(30);
        Note($"   neighbour takes focus  : {Ambient()} read={top.IsSubmenuOpen} popupOpen={PopupOpenOf(top)} " +
             $"writes={Writes.Count}");
        foreach (var write in Writes)
        {
            Note($"      {write}");
        }

        neighbour.Close();
        _host.Activate();
        Pump(8);
        Note($"   back on the host       : {Ambient()}");
    }

    /// <summary>Submenu up, then a flyout - which the framework realises in a top-level window of its own - is
    /// shown over it and hidden again, without reparenting anything the submenu is mounted on.</summary>
    private static void ReadWithPopupTeardown()
    {
        Note($"   roots left open by earlier readings: {CloseAllOpen()} (closed before this reading)");
        var top = SubmenuUp();
        Note($"   submenu up            : read={top.IsSubmenuOpen} popupOpen={PopupOpenOf(top)}");
        Writes.Clear();

        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuFlyoutItem { Text = "cut" });
        flyout.ShowAt(top);
        Pump(20);
        Note($"   flyout over the submenu: flyoutOpen={flyout.IsOpen} read={top.IsSubmenuOpen} writes={Writes.Count}");
        Writes.Clear();

        flyout.Hide();
        Pump(30);
        Note($"   flyout hidden         : read={top.IsSubmenuOpen} popupOpen={PopupOpenOf(top)} writes={Writes.Count}");
        foreach (var write in Writes)
        {
            Note($"      {write}");
        }
    }

    /// <summary>Closes what earlier readings left up and returns how many were up - each one is a light-dismiss
    /// root still registered on the shared host's overlay.</summary>
    private static int CloseAllOpen()
    {
        var left = Opened.Count(static item => item.IsSubmenuOpen);
        foreach (var item in Opened)
        {
            item.IsSubmenuOpen = false;
        }

        Opened.Clear();
        Pump(6);
        return left;
    }

    /// <summary>The test's move, on a fresh Menu: mount, click the one item that owns a submenu, pump, hand it back.</summary>
    private static MenuItem SubmenuUp()
    {
        Writes.Clear();
        var top = new ProbeMenuItem { Header = "edit" };
        top.Items.Add(new MenuItem { Header = "copy" });
        var menu = new Menu();
        menu.Items.Add(top);
        Mount(menu, 320, 40);
        RaiseMouseDown(top);
        Pump(10);
        return top;
    }

    /// <summary>Submenu up, then a dialog opened over it - the one call in the shipped framework that closes every
    /// light-dismiss popup in the window regardless of who owns it.</summary>
    private static void ReadWithDialog()
    {
        Writes.Clear();
        var top = new ProbeMenuItem { Header = "edit" };
        top.Items.Add(new MenuItem { Header = "copy" });
        var menu = new Menu();
        menu.Items.Add(top);
        Mount(menu, 320, 40);
        RaiseMouseDown(top);
        Pump(10);
        Note($"   submenu up            : read={top.IsSubmenuOpen} popupOpen={PopupOpenOf(top)}");

        var dialog = new ContentDialog { Title = "T", Content = "C", PrimaryButtonText = "OK" };
        try
        {
            dialog.ShowAsync();
            Pump(20);
            Note($"   dialog shown          : dialogVisible={dialog.Visibility} read={top.IsSubmenuOpen} " +
                 $"popupOpen={PopupOpenOf(top)} writes={Writes.Count}");
            dialog.Hide();
            Pump(10);
        }
        catch (Exception failure)
        {
            Note($"   dialog threw: {failure.GetType().Name}: {failure.Message}");
        }

        foreach (var write in Writes)
        {
            Note($"      {write}");
        }
    }

    private static void Mount(FrameworkElement element, int width, int height)
    {
        Place(element, width, height);
        Pump(4);
    }

    /// <summary>The harness's build step without the frame pump: size, hang on the host, apply the template, lay out.</summary>
    private static void Place(FrameworkElement element, int width, int height)
    {
        element.Width = width;
        element.Height = height;
        var root = new Grid();
        root.Children.Add(element);
        _host.Content = root;
        if (element is Control control) control.ApplyTemplate();
        _host.UpdateLayout();
    }

    /// <summary>The minimum build time for the click to land, in frames. Reads at the frame counts the harness
    /// actually asks for (4) and below it, so a test whose click needs more than a starved pump delivers is visible.</summary>
    private static void Ladder()
    {
        foreach (var frames in new[] { 0, 1, 2, 3, 4, 6 })
        {
            Writes.Clear();
            var top = new ProbeMenuItem { Header = "edit" };
            top.Items.Add(new MenuItem { Header = "copy" });
            var menu = new Menu();
            menu.Items.Add(top);
            Place(menu, 320, 40);
            Pump(frames);
            var before = RealizedOf(top);
            RaiseMouseDown(top);
            Pump(12);
            Note($"   {frames} frame(s) before the click: realized={before} read={top.IsSubmenuOpen} writes={Writes.Count}");
        }
    }

    private static string RealizedOf(MenuItem item) =>
        $"parent={item.VisualParent?.GetType().Name ?? "null"} children={VisualTreeHelper.GetChildrenCount(item)}";

    /// <summary>The submenu popup is a private field; read only its own IsOpen, which is what separates "never
    /// shown" from "shown then closed". Missing field says so instead of pretending.</summary>
    private static string PopupOpenOf(MenuItem item)
    {
        var field = typeof(MenuItem).GetField("_submenuPopup", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field?.GetValue(item) is not object popup) return "no-popup-field";
        var value = popup.GetType().GetProperty("IsOpen")?.GetValue(popup);
        return $"{value?.ToString()}";
    }

    private static void RaiseMouseDown(FrameworkElement element) => element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
    {
        RoutedEvent = UIElement.MouseDownEvent,
        Source = element,
    });

    private static string Ambient() => $"active={Hex(GetActiveWindow())} foreground={Hex(GetForegroundWindow())} " +
        $"host={Hex(_host.Handle)} mainWindow={(_host.Equals(Application.Current?.MainWindow) ? "host" : Application.Current?.MainWindow?.ToString() ?? "null")}";

    private static string Hex(nint handle) => handle == nint.Zero ? "0" : $"0x{handle:X}";

    private static void Note(string line) => Lines.Add(line);

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

    private sealed class ProbeMenuItem : MenuItem;
}
