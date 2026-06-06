using Jalium.UI.Media;

namespace FluentJalium.Icon;

/// <summary>
/// Factory helpers for object-valued icon properties.
/// </summary>
public static class FluentIconFactory
{
    /// <summary>
    /// Creates a regular Fluent-named icon element backed by Jalium's symbol font.
    /// </summary>
    public static FluentIcon Create(FluentIconRegular icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
        => Regular(icon, size, foreground);

    /// <summary>
    /// Creates a filled Fluent-named icon element backed by Jalium's symbol font.
    /// </summary>
    public static FluentIcon Create(FluentIconFilled icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
        => Filled(icon, size, foreground);

    /// <summary>
    /// Creates a Segoe Fluent Icons compatibility element.
    /// </summary>
    public static FluentIcon Create(SegoeFluentIcon icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
        => Segoe(icon, size, foreground);

    /// <summary>
    /// Creates a regular Fluent-named icon element backed by Jalium's symbol font.
    /// </summary>
    public static FluentIcon Regular(FluentIconRegular icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
        => CreateIcon(icon, FluentIconSet.Regular, false, size, foreground);

    /// <summary>
    /// Creates a filled Fluent-named icon element backed by Jalium's symbol font.
    /// </summary>
    public static FluentIcon Filled(FluentIconFilled icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
        => CreateIcon(icon, FluentIconSet.Filled, true, size, foreground);

    /// <summary>
    /// Creates a filled Fluent-named icon element from a regular icon name.
    /// </summary>
    public static FluentIcon Filled(FluentIconRegular icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
        => CreateIcon(icon, FluentIconSet.Filled, true, size, foreground);

    /// <summary>
    /// Creates a Segoe Fluent Icons compatibility element.
    /// </summary>
    public static FluentIcon Segoe(SegoeFluentIcon icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
        => CreateIcon(icon, FluentIconSet.Segoe, false, size, foreground);

    private static FluentIcon CreateIcon(object icon, FluentIconSet iconSet, bool filled, double size, Brush? foreground)
        => new()
        {
            Icon = icon,
            IconSet = iconSet,
            Filled = filled,
            Size = size,
            Foreground = foreground
        };
}
