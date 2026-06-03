using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Media;

namespace FluentJalium.Icon;

/// <summary>
/// Displays a Fluent UI System Icons or Segoe Fluent Icons glyph.
/// </summary>
public class FluentIcon : FontIcon
{
    /// <summary>
    /// Default icon size in device-independent pixels.
    /// </summary>
    public const double DefaultSize = 20.0;

    /// <summary>
    /// Font family used for regular Fluent UI System Icons.
    /// </summary>
    public const string RegularFontFamily = "FluentSystemIcons-Regular";

    /// <summary>
    /// Font family used for filled Fluent UI System Icons.
    /// </summary>
    public const string FilledFontFamily = "FluentSystemIcons-Filled";

    /// <summary>
    /// Font family used for Windows Segoe Fluent Icons compatibility glyphs.
    /// </summary>
    public const string SegoeFontFamily = "Segoe Fluent Icons";

    private bool _isUpdating;

    /// <summary>
    /// Identifies the <see cref="Icon"/> dependency property.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Other)]
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(FluentIcon),
            new PropertyMetadata(FluentIconRegular.Empty, OnIconPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="Filled"/> dependency property.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Other)]
    public static readonly DependencyProperty FilledProperty =
        DependencyProperty.Register(nameof(Filled), typeof(bool), typeof(FluentIcon),
            new PropertyMetadata(false, OnIconPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="IconSet"/> dependency property.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Other)]
    public static readonly DependencyProperty IconSetProperty =
        DependencyProperty.Register(nameof(IconSet), typeof(FluentIconSet), typeof(FluentIcon),
            new PropertyMetadata(FluentIconSet.Regular, OnIconPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="Size"/> dependency property.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(double), typeof(FluentIcon),
            new PropertyMetadata(DefaultSize, OnSizePropertyChanged, CoerceSize));

    /// <summary>
    /// Gets or sets the icon enum value. Supports <see cref="FluentIconRegular"/>,
    /// <see cref="FluentIconFilled"/>, and <see cref="SegoeFluentIcon"/>.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Other)]
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets whether a regular system icon should use the filled glyph set.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Other)]
    public bool Filled
    {
        get => (bool)GetValue(FilledProperty)!;
        set => SetValue(FilledProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon set used when <see cref="Icon"/> does not already imply one.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Other)]
    public FluentIconSet IconSet
    {
        get => (FluentIconSet)GetValue(IconSetProperty)!;
        set => SetValue(IconSetProperty, value);
    }

    /// <summary>
    /// Gets or sets the rendered icon size.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double Size
    {
        get => (double)GetValue(SizeProperty)!;
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentIcon"/> class.
    /// </summary>
    public FluentIcon()
    {
        UpdateIcon();
        UpdateSize();
    }

    private static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentIcon icon)
        {
            icon.UpdateIcon();
        }
    }

    private static void OnSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentIcon icon)
        {
            icon.UpdateSize();
        }
    }

    private static object CoerceSize(DependencyObject d, object? baseValue)
    {
        if (baseValue is double size && double.IsFinite(size) && size > 0)
        {
            return size;
        }

        return DefaultSize;
    }

    private void UpdateIcon()
    {
        if (_isUpdating)
        {
            return;
        }

        _isUpdating = true;
        try
        {
            var (glyph, fontFamily) = ResolveIcon();
            SetCurrentValue(GlyphProperty, glyph);
            SetCurrentValue(FontFamilyProperty, new FontFamily(fontFamily));
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void UpdateSize()
    {
        var size = Size;
        SetCurrentValue(FontSizeProperty, size);
        SetCurrentValue(WidthProperty, size);
        SetCurrentValue(HeightProperty, size);
    }

    private (string Glyph, string FontFamily) ResolveIcon()
    {
        return Icon switch
        {
            FluentIconFilled filledIcon => (filledIcon.GetString(), FilledFontFamily),
            SegoeFluentIcon segoeIcon => (segoeIcon.GetString(), SegoeFontFamily),
            FluentIconRegular regularIcon => ResolveRegularIcon(regularIcon),
            int codePoint => (FluentIconExtensions.ConvertCodePoint(codePoint), ResolveFontFamily(IconSet, Filled)),
            _ => (string.Empty, ResolveFontFamily(IconSet, Filled))
        };
    }

    private (string Glyph, string FontFamily) ResolveRegularIcon(FluentIconRegular regularIcon)
    {
        if (Filled || IconSet == FluentIconSet.Filled)
        {
            var filledIcon = Enum.TryParse(regularIcon.ToString(), out FluentIconFilled parsed)
                ? parsed
                : FluentIconFilled.Empty;
            return (filledIcon.GetString(), FilledFontFamily);
        }

        return (regularIcon.GetString(), RegularFontFamily);
    }

    private static string ResolveFontFamily(FluentIconSet iconSet, bool filled)
        => iconSet switch
        {
            FluentIconSet.Segoe => SegoeFontFamily,
            FluentIconSet.Filled => FilledFontFamily,
            _ when filled => FilledFontFamily,
            _ => RegularFontFamily
        };
}
