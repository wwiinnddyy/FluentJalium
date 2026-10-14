using System.Runtime.InteropServices;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using Jalium.UI.Threading;

namespace SystemColorProbe;

/// <summary>
/// #61's question, asked of the platform instead of of the test host. The shipped high-contrast fact
/// (<c>AstraThemeRuntimeTests.Every_high_contrast_row_drives_its_own_brush</c>) pins which palette key is driven
/// by which system-colour slot, and says plainly that it cannot pin the value a user sees: on the headless host
/// the unset slots read #FF00FF, and two of them resolved to the same colour, so swapping them was invisible.
/// <para>
/// The framework's own source names the mechanism to check first: <c>SystemColors.WindowColor</c> and friends are
/// <c>ResolveColor(brushResourceKey, colorResourceKey, ColorFromSysColor(index))</c> - a theme resource that
/// carries one of those <c>SystemColor*</c> keys <b>shadows</b> the Win32 value rather than the other way round
/// (<c>Jalium.UI.Controls/SystemColors.cs:306-316</c>). So "the slot is magenta" is a claim about who wrote the
/// resource, not about the platform.
/// </para>
/// This probe reads all three things side by side, per slot, in each theme state: what Win32 says
/// (<c>GetSysColor</c>), what the framework's <c>SystemColors</c> says, and whether a resource of that slot's own
/// name is present to shadow it. Plus the one fact no in-process read can produce: whether Windows currently has
/// a high-contrast theme switched on (<c>SPI_GETHIGHCONTRAST</c>).
/// </summary>
internal static class Program
{
    private static readonly List<string> Lines = [];

    /// <summary>slot name used by our checked-in table, the Win32 index behind it, the framework property that
    /// reads that index, and the two resource keys that can shadow it.</summary>
    private static readonly (string Slot, int Index, string Property, string BrushKey, string ColorKey)[] Slots =
    [
        ("SystemColorWindowColor", 1, nameof(SystemColors.WindowColor), "SystemColorWindowColorBrush", "SystemColorWindowColor"),
        ("SystemColorWindowTextColor", 8, nameof(SystemColors.WindowTextColor), "SystemColorWindowTextColorBrush", "SystemColorWindowTextColor"),
        ("SystemColorHighlightColor", 13, nameof(SystemColors.HighlightColor), "SystemColorHighlightColorBrush", "SystemColorHighlightColor"),
        ("SystemColorHighlightTextColor", 14, nameof(SystemColors.HighlightTextColor), "SystemColorHighlightTextColorBrush", "SystemColorHighlightTextColor"),
        ("SystemColorButtonFaceColor", 15, nameof(SystemColors.ControlColor), "SystemColorButtonFaceColorBrush", "SystemColorButtonFaceColor"),
        ("SystemColorButtonTextColor", 16, nameof(SystemColors.ControlTextColor), "SystemColorButtonTextColorBrush", "SystemColorButtonTextColor"),
        ("SystemColorGrayTextColor", 17, nameof(SystemColors.GrayTextColor), "SystemColorGrayTextColorBrush", "SystemColorGrayTextColor"),
        ("SystemColorHotlightColor", 26, nameof(SystemColors.HotTrackColor), "SystemColorHotlightColorBrush", "SystemColorHotlightColor"),
    ];

    [DllImport("user32.dll")]
    private static extern int GetSysColor(int nIndex);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(int uiAction, int uiParam, ref HIGHCONTRAST pvParam, int fWinIni);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SystemParametersInfoW(int uiAction, int uiParam, ref HIGHCONTRAST pvParam, int fWinIni);

    [StructLayout(LayoutKind.Sequential)]
    private struct HIGHCONTRAST
    {
        public int cbSize;
        public int dwFlags;
        public IntPtr lpszStatusAppend;
    }

    private const int SpiGetHighContrast = 0x1021;
    private const int HcfHighContrastOn = 0x1;

    [STAThread]
    private static int Main()
    {
        var renderContext = RenderContext.GetOrCreateCurrent(RenderBackend.Auto);
        renderContext.DefaultRenderingEngine = RenderingEngine.Impeller;

        Note("=== the OS fact no in-process colour read can stand in for ===");
        var hc = new HIGHCONTRAST { cbSize = Marshal.SizeOf<HIGHCONTRAST>() };
        var queried = SystemParametersInfo(SpiGetHighContrast, hc.cbSize, ref hc, 0);
        Note($"   SPI_GETHIGHCONTRAST (A, uiParam=cbSize) ok={queried} err={Marshal.GetLastWin32Error()} dwFlags=0x{hc.dwFlags:X} highContrastOn={(hc.dwFlags & HcfHighContrastOn) != 0}");
        hc = new HIGHCONTRAST { cbSize = Marshal.SizeOf<HIGHCONTRAST>() };
        queried = SystemParametersInfo(SpiGetHighContrast, 0, ref hc, 0);
        Note($"   SPI_GETHIGHCONTRAST (A, uiParam=0) ok={queried} err={Marshal.GetLastWin32Error()} dwFlags=0x{hc.dwFlags:X} highContrastOn={(hc.dwFlags & HcfHighContrastOn) != 0}");
        hc = new HIGHCONTRAST { cbSize = Marshal.SizeOf<HIGHCONTRAST>() };
        queried = SystemParametersInfoW(SpiGetHighContrast, hc.cbSize, ref hc, 0);
        Note($"   SPI_GETHIGHCONTRAST (W, uiParam=cbSize) ok={queried} err={Marshal.GetLastWin32Error()} dwFlags=0x{hc.dwFlags:X} highContrastOn={(hc.dwFlags & HcfHighContrastOn) != 0}");
        Note($"   GetSystemMetrics(SM_HIGCONTRASTMODE=21) -> {GetSystemMetrics(21)}");
        Note("   (if every one of these says off, this machine cannot show what a high-contrast user sees; the slot");
        Note("    values below are the classic-theme values, and any claim about HC pixels needs a machine with the theme on)");
        Note(string.Empty);

        Note("=== what Win32 answers for the eight slots our table names ===");
        foreach (var slot in Slots)
        {
            Note($"   {slot.Property,-20} COLORREF {slot.Index,3} -> {Rgb(slot.Index)}");
        }

        Note(string.Empty);
        Note("=== what the framework resolves, per theme state, and what shadows it ===");
        ThemeLoader.Initialize();
        var application = new Application();
        Read("no theme applied  ", application, applied: false);

        FluentThemeManager.Apply(application);
        Pump(6);
        Read("Light            ", application);

        FluentThemeManager.ApplyTheme(FluentThemeVariant.HighContrast);
        Pump(6);
        Read("HighContrast     ", application);

        // The decisive question, asked with the production write route and no reflection: our palette owns the
        // very key that SystemColors.ResolveColor looks up first. If a brush written through
        // FluentThemeManager.OverrideBrush moves SystemColors.<Slot>, then the "platform" side of the high-contrast
        // map is our own cell, and the sentinel in Light/Dark.jalxaml is what a High-contrast user would get.
        Note(string.Empty);
        Note("=== does a write to our own cell move the framework's system-colour read? ===");
        foreach (var slot in Slots)
        {
            var injected = Color.FromRgb((byte)(0x10 + slot.Index), 0xAB, 0xCD);
            FluentThemeManager.OverrideBrush(slot.BrushKey, injected);
            Pump(2);
            var after = ReadFramework(slot.Property);
            var resource = Describe(application.TryFindResource(slot.BrushKey));
            Note($"      {slot.Property,-20} override={injected} -> framework={after} brushResource={resource} " +
                 $"moved={after == injected.ToString()}");
        }

        FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
        Pump(6);
        Read("back to Light    ", application);

        Note(string.Empty);
        Note("=== are the framework's eight slots distinct, or do some collapse onto one value? ===");
        var byValue = Slots
            .Select(slot => (slot.Property, Colour: ReadFramework(slot.Property)))
            .GroupBy(entry => entry.Colour)
            .OrderByDescending(group => group.Count())
            .ToArray();
        foreach (var group in byValue)
        {
            Note($"   {group.Key} <- {string.Join(", ", group.Select(entry => entry.Property))}");
        }

        Note(string.Empty);
        Note("=== and the same collapse question asked of Win32 ===");
        var platformGroups = Slots
            .Select(slot => (slot.Property, Colour: Rgb(slot.Index)))
            .GroupBy(entry => entry.Colour)
            .Where(group => group.Count() > 1)
            .ToArray();
        if (platformGroups.Length == 0) Note("   every slot has its own Win32 value");
        foreach (var group in platformGroups)
        {
            Note($"   {group.Key} <- {string.Join(", ", group.Select(entry => entry.Property))}");
        }

        var path = Path.Combine(AppContext.BaseDirectory, "system-color-probe.txt");
        File.WriteAllLines(path, Lines);
        foreach (var line in Lines)
        {
            Console.WriteLine(line);
        }

        Console.WriteLine($"wrote {path}");
        return 0;
    }

    private static void Read(string state, Application application, bool applied = true)
    {
        Note($"   --- {state} ---");
        foreach (var slot in Slots)
        {
            var framework = ReadFramework(slot.Property);
            var brushResource = application.TryFindResource(slot.BrushKey);
            var colorResource = application.TryFindResource(slot.ColorKey);
            // Is the resource the framework answers with *our* palette instance? If yes, our cell is the
            // shadow, and whatever we write there is what SystemColors.<Slot> reports as the platform value.
            var ours = applied && brushResource is not null && ReferenceEquals(brushResource, FluentThemeManager.GetBrush(slot.BrushKey))
                ? "ours"
                : applied ? "not-ours" : "n/a";
            Note($"      {slot.Property,-20} win32={Rgb(slot.Index)} framework={framework} ours={ours} " +
                 $"brushResource={Describe(brushResource)} colorResource={Describe(colorResource)}");
        }

        // TryFindResource, not FluentThemeManager.GetBrush: the first state here is "no theme applied yet",
        // and the manager refuses every read until Apply has run - which is itself the answer for that state.
        var textPrimary = Describe(application.TryFindResource("TextFillColorPrimaryBrush"));
        Note($"      TextFillColorPrimaryBrush via TryFindResource in this state: {textPrimary}");
    }

    private static string ReadFramework(string propertyName) =>
        typeof(SystemColors).GetProperty(propertyName)?.GetValue(null) is Color colour
            ? colour.ToString()
            : throw new InvalidOperationException($"{propertyName} is not a Color property");

    private static string Describe(object? resource) => resource switch
    {
        null => "absent",
        SolidColorBrush brush => brush.Color.ToString(),
        Color colour => colour.ToString(),
        _ => resource?.GetType().Name ?? "null",
    };

    private static string Rgb(int index)
    {
        var ref0 = GetSysColor(index);
        // COLORREF is 0x00BBGGRR; System.Drawing-free conversion keeps the same #RRGGBB text the tests print.
        return $"#{(ref0 >> 0 & 0xFF):X2}{(ref0 >> 8 & 0xFF):X2}{(ref0 >> 16 & 0xFF):X2}";
    }

    private static void Note(string line) => Lines.Add(line);

    private static void Pump(int frames, int budgetMilliseconds = 800)
    {
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        var seen = 0;
        var deadline = Environment.TickCount64 + budgetMilliseconds;
        void OnRendering(object? sender, EventArgs arguments)
        {
            seen++;
            if (seen >= frames || Environment.TickCount64 > deadline) frame.Continue = false;
        }

        EventHandler handler = OnRendering;
        CompositionTarget.Rendering += handler;
        using var watchdog = new System.Threading.Timer(_ => dispatcher.InvokeAsync(() => frame.Continue = false));
        watchdog.Change(TimeSpan.FromMilliseconds(budgetMilliseconds * 2L), System.Threading.Timeout.InfiniteTimeSpan);
        Dispatcher.PushFrame(frame);
        CompositionTarget.Rendering -= handler;
    }
}
