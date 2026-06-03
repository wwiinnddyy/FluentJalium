using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Icon;

/// <summary>
/// Helpers for converting FluentJalium icon enums into glyphs and icon elements.
/// </summary>
public static class FluentIconExtensions
{
    /// <summary>
    /// Gets the regular Fluent UI System Icons glyph for an enum value.
    /// </summary>
    public static string GetGlyph(this FluentIconRegular icon) => ConvertCodePoint((int)icon);

    /// <summary>
    /// Gets the filled Fluent UI System Icons glyph for an enum value.
    /// </summary>
    public static string GetGlyph(this FluentIconFilled icon) => ConvertCodePoint((int)icon);

    /// <summary>
    /// Gets the Segoe Fluent Icons glyph for an enum value.
    /// </summary>
    public static string GetGlyph(this SegoeFluentIcon icon) => ConvertCodePoint((int)icon);

    /// <summary>
    /// Gets the regular Fluent UI System Icons glyph for an enum value.
    /// </summary>
    public static string GetString(this FluentIconRegular icon) => icon.GetGlyph();

    /// <summary>
    /// Gets the filled Fluent UI System Icons glyph for an enum value.
    /// </summary>
    public static string GetString(this FluentIconFilled icon) => icon.GetGlyph();

    /// <summary>
    /// Gets the Segoe Fluent Icons glyph for an enum value.
    /// </summary>
    public static string GetString(this SegoeFluentIcon icon) => icon.GetGlyph();

    /// <summary>
    /// Converts a regular Fluent UI System Icon to a Jalium icon element.
    /// </summary>
    public static FluentIcon ToIcon(this FluentIconRegular icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
        => FluentIconFactory.Regular(icon, size, foreground);

    /// <summary>
    /// Converts a filled Fluent UI System Icon to a Jalium icon element.
    /// </summary>
    public static FluentIcon ToIcon(this FluentIconFilled icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
        => FluentIconFactory.Filled(icon, size, foreground);

    /// <summary>
    /// Converts a Segoe Fluent Icons glyph to a Jalium icon element.
    /// </summary>
    public static FluentIcon ToIcon(this SegoeFluentIcon icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
        => FluentIconFactory.Segoe(icon, size, foreground);

    /// <summary>
    /// Converts a known FluentJalium icon enum value to a Jalium icon element.
    /// </summary>
    public static FontIcon ToIcon(this Enum icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
        => icon switch
        {
            FluentIconRegular regular => regular.ToIcon(size, foreground),
            FluentIconFilled filled => filled.ToIcon(size, foreground),
            SegoeFluentIcon segoe => segoe.ToIcon(size, foreground),
            _ => throw new ArgumentException($"Unsupported FluentJalium icon enum '{icon.GetType()}'.", nameof(icon))
        };

    internal static string ConvertCodePoint(int codePoint)
        => codePoint <= 0 ? string.Empty : char.ConvertFromUtf32(codePoint);
}
