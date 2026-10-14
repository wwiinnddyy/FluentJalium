using System.Text.Json.Serialization;
using Jalium.UI.Hosting;
using Jalium.UI.Interop;

namespace FluentJalium.Localization;

/// <summary>Which theme the host asks for, independent of any theme manager's own enum.</summary>
public enum AppThemeMode
{
    /// <summary>Follow the operating system.</summary>
    System,

    /// <summary>Force the light palette.</summary>
    Light,

    /// <summary>Force the dark palette.</summary>
    Dark,
}

/// <summary>
/// Everything the launcher persists between runs. Kept free of UI types so the store can be exercised without a
/// window, and so the Gallery can read the one field it cares about (<see cref="Language"/>) without pulling in
/// the launcher's renderer choices.
/// </summary>
public sealed class HostSettings
{
    /// <summary>The graphics backend requested for the next window's render target.</summary>
    public RenderBackend Backend { get; set; } = RenderBackend.Auto;

    /// <summary>The 2D path engine requested for the next window's render target.</summary>
    public RenderingEngine Engine { get; set; } = RenderingEngine.Auto;

    /// <summary>The catalog language.</summary>
    public UiLanguage Language { get; set; } = UiLanguage.English;

    /// <summary>
    /// Path anti-aliasing mode. Read by the runtime once per render target, so a change lands on the next window
    /// rather than on the running one.
    /// </summary>
    public PathAntiAliasing PathAntiAliasing { get; set; } = PathAntiAliasing.Msaa8x;

    /// <summary>The requested theme.</summary>
    public AppThemeMode Theme { get; set; } = AppThemeMode.System;

    /// <summary>Whether animations are suppressed.</summary>
    public bool ReduceMotion { get; set; }

    /// <summary>A copy, so a caller can stage a change and commit it once the handoff succeeds.</summary>
    public HostSettings Clone() => new()
    {
        Backend = Backend,
        Engine = Engine,
        Language = Language,
        PathAntiAliasing = PathAntiAliasing,
        Theme = Theme,
        ReduceMotion = ReduceMotion,
    };

    /// <summary>
    /// Forces every field back to the shipped default. Bound to the launcher's "restore defaults" affordance and
    /// used by the settings tests as the round-trip baseline.
    /// </summary>
    public void ResetToDefaults()
    {
        Backend = RenderBackend.Auto;
        Engine = RenderingEngine.Auto;
        Language = UiLanguage.English;
        PathAntiAliasing = PathAntiAliasing.Msaa8x;
        Theme = AppThemeMode.System;
        ReduceMotion = false;
    }
}

/// <summary>The on-disk shape. Strings rather than enum numbers so a hand-edited file stays readable and a
/// reordered enum cannot silently repoint a saved value.</summary>
internal sealed class HostSettingsDocument
{
    [JsonPropertyName("backend")] public string? Backend { get; set; }
    [JsonPropertyName("engine")] public string? Engine { get; set; }
    [JsonPropertyName("language")] public string? Language { get; set; }
    [JsonPropertyName("pathAntiAliasing")] public string? PathAntiAliasing { get; set; }
    [JsonPropertyName("theme")] public string? Theme { get; set; }
    [JsonPropertyName("reduceMotion")] public bool? ReduceMotion { get; set; }
}
