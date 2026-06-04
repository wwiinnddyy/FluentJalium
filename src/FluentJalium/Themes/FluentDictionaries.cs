using Jalium.UI;

namespace FluentJalium.Controls.Themes;

/// <summary>
/// Describes the FluentJalium dictionary entry points exposed for application resource setup.
/// </summary>
public enum FluentJaliumDictionaryKind
{
    /// <summary>
    /// Includes Fluent tokens and Fluent control styles.
    /// </summary>
    Complete,

    /// <summary>
    /// Includes Fluent tokens only.
    /// </summary>
    Resources,

    /// <summary>
    /// Includes Fluent control styles only.
    /// </summary>
    Controls
}

/// <summary>
/// Base resource dictionary for FluentJalium package entry points.
/// </summary>
public abstract class FluentJaliumDictionary : ResourceDictionary
{
    protected FluentJaliumDictionary(FluentJaliumDictionaryKind kind)
    {
        Kind = kind;
        Source = GetSource(kind);
    }

    /// <summary>
    /// Gets which FluentJalium dictionary this instance loads.
    /// </summary>
    public FluentJaliumDictionaryKind Kind { get; }

    /// <summary>
    /// Gets the pack-style URI for a FluentJalium dictionary entry point.
    /// </summary>
    public static Uri GetSource(FluentJaliumDictionaryKind kind)
    {
        return kind switch
        {
            FluentJaliumDictionaryKind.Complete => new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentJaliumDictionaryKind.Resources => new Uri("/FluentJalium;component/Themes/FluentResources.jalxaml", UriKind.Relative),
            FluentJaliumDictionaryKind.Controls => new Uri("/FluentJalium;component/Themes/Controls/FluentControls.jalxaml", UriKind.Relative),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), "Unknown FluentJalium dictionary kind.")
        };
    }
}

/// <summary>
/// Resource dictionary entry point that includes FluentJalium resources and control styles.
/// </summary>
public sealed class FluentJaliumThemeDictionary : FluentJaliumDictionary
{
    public FluentJaliumThemeDictionary()
        : base(FluentJaliumDictionaryKind.Complete)
    {
    }
}

/// <summary>
/// Resource dictionary entry point for FluentJalium tokens such as colors, typography, geometry, materials, and motion.
/// </summary>
public sealed class FluentJaliumResourcesDictionary : FluentJaliumDictionary
{
    public FluentJaliumResourcesDictionary()
        : base(FluentJaliumDictionaryKind.Resources)
    {
    }
}

/// <summary>
/// Resource dictionary entry point for FluentJalium control styles.
/// </summary>
public sealed class FluentJaliumControlsDictionary : FluentJaliumDictionary
{
    public FluentJaliumControlsDictionary()
        : base(FluentJaliumDictionaryKind.Controls)
    {
    }
}
