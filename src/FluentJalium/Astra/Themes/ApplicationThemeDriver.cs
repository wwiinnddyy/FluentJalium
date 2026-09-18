using Jalium.UI;

// Jalium marks the whole theme-mode family [Experimental("WPF0001")] and tells us to suppress the
// diagnostic to use it. This file is where that suppression lives; nowhere else in Astra.
#pragma warning disable WPF0001

namespace FluentJalium.Themes;

/// <summary>
/// Astra's only contact point with Jalium's experimental theme-mode API.
/// </summary>
/// <remarks>
/// <c>Application.ThemeMode</c>, <c>Window.ThemeMode</c> and the <c>ThemeMode</c> type itself carry
/// <c>[Experimental("WPF0001")]</c> in 26.10.9, and the stage-0 measurements say this is the only
/// assignment that re-resolves <c>{ThemeResource}</c> consumers (brushes, corner radii, thicknesses
/// and durations) without rebuilding controls. Everything else that looked like a driver -
/// <c>ResourceDictionary.CurrentThemeKey</c>, <c>ThemeManager.ApplyTheme</c> - is a result of it.
/// Mode is carried as a string so no experimental type reaches Astra's public surface; the compile
/// breaks here, in one file, if Jalium removes the property, and
/// <c>FluentJalium.Tests/AstraThemeRuntimeTests.cs</c> fails if it stops working.
/// </remarks>
internal static class ApplicationThemeDriver
{
    internal const string Light = "Light";
    internal const string Dark = "Dark";
    internal const string System = "System";
    internal const string None = "None";

    internal static void Apply(Application application, string mode)
    {
        application.ThemeMode = mode switch
        {
            Light => ThemeMode.Light,
            Dark => ThemeMode.Dark,
            System => ThemeMode.System,
            None => ThemeMode.None,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown theme mode."),
        };
    }

    internal static string Read(Application application)
    {
        var mode = application.ThemeMode;
        if (mode == ThemeMode.Light) return Light;
        if (mode == ThemeMode.Dark) return Dark;
        if (mode == ThemeMode.System) return System;
        if (mode == ThemeMode.None) return None;
        throw new InvalidOperationException($"Unrecognised Jalium theme mode: {mode}");
    }
}
