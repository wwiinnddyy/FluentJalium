using System.Runtime.InteropServices;
using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls.Themes;

/// <summary>
/// Keeps the native (DWM) window chrome in sync with the active FluentJalium theme.
/// </summary>
/// <remarks>
/// <para>
/// Jalium.UI applies the <em>dark</em> DWM caption/border color unconditionally when a window is
/// created and never revisits it when the application theme changes. Without this helper a
/// light-themed FluentJalium window keeps a dark caption, border and system menu — the classic
/// "dark title bar on a light app" mismatch (and it makes the light-theme caption text, which is
/// near-black, unreadable).
/// </para>
/// <para>
/// Call <see cref="Apply(Window)"/> once the window has a native handle (for example from its
/// Loaded event — <see cref="Window.Handle"/> is still zero in the constructor). FluentJalium also
/// re-applies the chrome to every open window whenever
/// <see cref="FluentThemeManager.ThemeChanged"/> fires, so runtime theme switches are covered.
/// </para>
/// </remarks>
public static partial class FluentWindowChrome
{
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaBorderColor = 34;
    private const int DwmwaCaptionColor = 35;

    // COLORREF format is 0x00BBGGRR; both defaults are neutral greys so byte order is irrelevant.
    private const int DarkChromeColor = 0x00202020;  // #202020 — matches SolidBackgroundFillColorBase (Dark)
    private const int LightChromeColor = 0x00F3F3F3; // #F3F3F3 — matches SolidBackgroundFillColorBase (Light)

    /// <summary>
    /// Applies the active FluentJalium theme to the specified window's native chrome.
    /// </summary>
    /// <param name="window">The window whose chrome should follow the theme.</param>
    public static void Apply(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (!OperatingSystem.IsWindows())
            return;

        var handle = window.Handle;
        if (handle == nint.Zero)
            return;

        Apply(handle);
    }

    /// <summary>
    /// Applies the active FluentJalium theme to the specified native window handle.
    /// </summary>
    /// <param name="handle">The native window handle, or <see cref="nint.Zero"/> to do nothing.</param>
    public static void Apply(nint handle)
    {
        if (!OperatingSystem.IsWindows() || handle == nint.Zero)
            return;

        var dark = FluentThemeManager.CurrentTheme is FluentThemeVariant.Dark or FluentThemeVariant.HighContrast;

        var useDarkMode = dark ? 1 : 0;
        _ = DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkMode, ref useDarkMode, sizeof(int));

        var chromeColor = dark ? DarkChromeColor : LightChromeColor;
        _ = DwmSetWindowAttribute(handle, DwmwaCaptionColor, ref chromeColor, sizeof(int));
        _ = DwmSetWindowAttribute(handle, DwmwaBorderColor, ref chromeColor, sizeof(int));
    }

    /// <summary>
    /// Re-applies the active theme to every open window of the specified application.
    /// </summary>
    internal static void ApplyToOpenWindows(Application? application)
    {
        if (application is null || !OperatingSystem.IsWindows())
            return;

        foreach (var item in application.Windows)
        {
            if (item is Window window)
            {
                Apply(window);
            }
        }
    }

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(nint hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
}
