namespace FluentJalium.Icon;

/// <summary>
/// Font family names used by FluentJalium icon controls.
/// </summary>
public static class FluentIconFonts
{
    /// <summary>
    /// FluentSystemIcons Regular font for FluentJalium icons.
    /// Triggers font extraction and registration on first access.
    /// </summary>
    public static string Regular => FluentIconFontLoader.Regular;

    /// <summary>
    /// FluentSystemIcons Filled font for FluentJalium icons.
    /// Triggers font extraction and registration on first access.
    /// </summary>
    public static string Filled => FluentIconFontLoader.Filled;

    /// <summary>
    /// Segoe MDL2 Assets fallback for compatibility.
    /// </summary>
    public const string Segoe = "Segoe MDL2 Assets";
}
