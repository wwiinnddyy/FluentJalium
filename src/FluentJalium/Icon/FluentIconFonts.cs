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
    /// Windows system icon font stack: prefers Win11 "Segoe Fluent Icons" glyphs and
    /// falls back to "Segoe MDL2 Assets" on older Windows. Both fonts share the
    /// same code points, so the Segoe compatibility glyph set renders with the
    /// modern Fluent look wherever the font exists.
    /// </summary>
    public static string Segoe => "Segoe Fluent Icons, Segoe MDL2 Assets";
}
