using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium Backdrop material types for background effects.
/// </summary>
public enum FWBackdropType
{
    None,
    Acrylic,
    Mica,
    MicaAlt,
    Tabbed
}

/// <summary>
/// FluentJalium Backdrop control for background material effects.
/// </summary>
public class FWBackdrop : Control, IFluentJaliumControl
{
    public static readonly DependencyProperty TypeProperty =
        DependencyProperty.Register(nameof(Type), typeof(FWBackdropType), typeof(FWBackdrop),
            new PropertyMetadata(FWBackdropType.None, OnTypeChanged));

    public static readonly DependencyProperty TintColorProperty =
        DependencyProperty.Register(nameof(TintColor), typeof(Color), typeof(FWBackdrop),
            new PropertyMetadata(Colors.Transparent, OnVisualPropertyChanged));

    public static readonly DependencyProperty TintOpacityProperty =
        DependencyProperty.Register(nameof(TintOpacity), typeof(double), typeof(FWBackdrop),
            new PropertyMetadata(0.8, OnVisualPropertyChanged), ValidateOpacity);

    public static readonly DependencyProperty LuminosityOpacityProperty =
        DependencyProperty.Register(nameof(LuminosityOpacity), typeof(double), typeof(FWBackdrop),
            new PropertyMetadata(0.85, OnVisualPropertyChanged), ValidateOpacity);

    public static readonly DependencyProperty FallbackColorProperty =
        DependencyProperty.Register(nameof(FallbackColor), typeof(Color), typeof(FWBackdrop),
            new PropertyMetadata(Color.FromRgb(0xF3, 0xF3, 0xF3), OnVisualPropertyChanged));

    public static readonly DependencyProperty AlwaysUseFallbackProperty =
        DependencyProperty.Register(nameof(AlwaysUseFallback), typeof(bool), typeof(FWBackdrop),
            new PropertyMetadata(false, OnTypeChanged));

    /// <summary>
    /// Initializes a new instance of the <see cref="FWBackdrop"/> class.
    /// </summary>
    public FWBackdrop()
    {
        IsHitTestVisible = false;
    }

    /// <summary>
    /// Gets or sets the backdrop material type.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public FWBackdropType Type
    {
        get => (FWBackdropType)GetValue(TypeProperty)!;
        set => SetValue(TypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the tint color overlay.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public Color TintColor
    {
        get => (Color)GetValue(TintColorProperty)!;
        set => SetValue(TintColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the tint opacity (0.0 to 1.0).
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public double TintOpacity
    {
        get => (double)GetValue(TintOpacityProperty)!;
        set => SetValue(TintOpacityProperty, value);
    }

    /// <summary>
    /// Gets or sets the luminosity opacity for the backdrop effect.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public double LuminosityOpacity
    {
        get => (double)GetValue(LuminosityOpacityProperty)!;
        set => SetValue(LuminosityOpacityProperty, value);
    }

    /// <summary>
    /// Gets or sets the fallback color when backdrop effects are not available.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public Color FallbackColor
    {
        get => (Color)GetValue(FallbackColorProperty)!;
        set => SetValue(FallbackColorProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to always use the fallback color.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public bool AlwaysUseFallback
    {
        get => (bool)GetValue(AlwaysUseFallbackProperty)!;
        set => SetValue(AlwaysUseFallbackProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (RenderSize.Width <= 0 || RenderSize.Height <= 0)
            return;

        var rect = new Rect(0, 0, RenderSize.Width, RenderSize.Height);

        if (AlwaysUseFallback || Type == FWBackdropType.None)
        {
            drawingContext.DrawRectangle(new SolidColorBrush(FallbackColor), null, rect);
            return;
        }

        // Render backdrop effect
        var brush = CreateBackdropBrush();
        drawingContext.DrawRectangle(brush, null, rect);
    }

    private Brush CreateBackdropBrush()
    {
        return Type switch
        {
            FWBackdropType.Acrylic => CreateAcrylicBrush(),
            FWBackdropType.Mica => CreateMicaBrush(),
            FWBackdropType.MicaAlt => CreateMicaAltBrush(),
            FWBackdropType.Tabbed => CreateTabbedBrush(),
            _ => new SolidColorBrush(FallbackColor)
        };
    }

    private Brush CreateAcrylicBrush()
    {
        // Acrylic effect: semi-transparent with tint and blur
        // Note: Actual blur effect would require native platform support
        var color = Color.FromArgb(
            (byte)(255 * TintOpacity),
            TintColor.R,
            TintColor.G,
            TintColor.B);

        var brush = new SolidColorBrush(color);
        return brush;
    }

    private Brush CreateMicaBrush()
    {
        // Mica effect: subtle texture with tint
        var color = Color.FromArgb(
            (byte)(255 * TintOpacity),
            TintColor.R,
            TintColor.G,
            TintColor.B);

        return new SolidColorBrush(color);
    }

    private Brush CreateMicaAltBrush()
    {
        // Mica Alt: darker variant for contrast surfaces
        var color = Color.FromArgb(
            (byte)(255 * TintOpacity),
            (byte)(TintColor.R * 0.9),
            (byte)(TintColor.G * 0.9),
            (byte)(TintColor.B * 0.9));

        return new SolidColorBrush(color);
    }

    private Brush CreateTabbedBrush()
    {
        // Tabbed: lighter variant optimized for tabbed interfaces
        var color = Color.FromArgb(
            (byte)(255 * TintOpacity * 0.9),
            (byte)Math.Min(255, TintColor.R + 10),
            (byte)Math.Min(255, TintColor.G + 10),
            (byte)Math.Min(255, TintColor.B + 10));

        return new SolidColorBrush(color);
    }

    private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWBackdrop backdrop)
        {
            backdrop.InvalidateVisual();
        }
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWBackdrop backdrop)
        {
            backdrop.InvalidateVisual();
        }
    }

    private static bool ValidateOpacity(object? value)
    {
        return value is double d && d >= 0.0 && d <= 1.0;
    }
}

/// <summary>
/// FluentJalium AcrylicBrush for acrylic material effects.
/// Note: This is a configuration class, not a real Brush. Use CreateBrush() to get the actual brush.
/// </summary>
public class FWAcrylicBrush
{
    /// <summary>
    /// Gets or sets the tint color.
    /// </summary>
    public Color TintColor { get; set; } = Color.FromRgb(0xF3, 0xF3, 0xF3);

    /// <summary>
    /// Gets or sets the tint opacity.
    /// </summary>
    public double TintOpacity { get; set; } = 0.8;

    /// <summary>
    /// Gets or sets the tint luminosity opacity.
    /// </summary>
    public double? TintLuminosityOpacity { get; set; }

    /// <summary>
    /// Gets or sets the background source.
    /// </summary>
    public AcrylicBackgroundSource BackgroundSource { get; set; } = AcrylicBackgroundSource.Backdrop;

    /// <summary>
    /// Gets or sets the fallback color.
    /// </summary>
    public Color FallbackColor { get; set; } = Color.FromRgb(0xF3, 0xF3, 0xF3);

    /// <summary>
    /// Creates the actual brush based on current properties.
    /// </summary>
    public Brush CreateBrush()
    {
        var color = Color.FromArgb(
            (byte)(TintOpacity * 255),
            TintColor.R,
            TintColor.G,
            TintColor.B);

        return new SolidColorBrush(color);
    }
}

/// <summary>
/// Defines acrylic background sources.
/// </summary>
public enum AcrylicBackgroundSource
{
    Backdrop,
    HostBackdrop
}
