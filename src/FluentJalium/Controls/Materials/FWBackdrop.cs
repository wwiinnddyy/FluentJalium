using FluentJalium.Controls.Themes;
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
/// <remarks>
/// Jalium.UI does not expose a blur / sampling backdrop yet, so every material is rendered as a
/// solid, opaque approximation: an opaque base surface (<see cref="FallbackColor"/>) with the
/// theme tint (<see cref="TintColor"/>) composited over it. The control always resolves a
/// non-transparent base and a theme-correct tint, so it can never wash the window with an
/// opaque white or black rectangle regardless of the active theme.
/// </remarks>
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
            new PropertyMetadata(Colors.Transparent, OnVisualPropertyChanged));

    public static readonly DependencyProperty AlwaysUseFallbackProperty =
        DependencyProperty.Register(nameof(AlwaysUseFallback), typeof(bool), typeof(FWBackdrop),
            new PropertyMetadata(false, OnTypeChanged));

    /// <summary>
    /// Initializes a new instance of the <see cref="FWBackdrop"/> class.
    /// </summary>
    public FWBackdrop()
    {
        IsHitTestVisible = false;
        FluentThemeManager.ThemeChanged += OnThemeChanged;
        Unloaded += OnUnloaded;
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
    /// <remarks>
    /// A fully transparent color (the default, <see cref="Colors.Transparent"/>) means
    /// "use the active theme tint" (<c>LayerFillColorDefault</c>) instead of painting a
    /// transparent — and therefore meaningless — overlay.
    /// </remarks>
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
    /// <remarks>
    /// The default is <see cref="Colors.Transparent"/>, which means "use the active theme base"
    /// (<c>SolidBackgroundFillColorBase</c>, or <c>WindowBackground</c> when the active variant does
    /// not define it). A transparent default is deliberate: the property is rendered opaque, so a
    /// baked-in light default would wash out a dark window.
    /// </remarks>
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

        var color = AlwaysUseFallback || Type == FWBackdropType.None
            ? ResolveBaseColor()
            : ResolveBackdropColor();

        drawingContext.DrawRectangle(new SolidColorBrush(color), null, rect);
    }

    /// <summary>
    /// Resolves the opaque base surface. Never returns a transparent color: a "solid" backdrop
    /// that let the desktop show through was the source of the washed-out/muddy window.
    /// </summary>
    private Color ResolveBaseColor()
    {
        var color = FallbackColor;
        if (color.A == 0)
        {
            color = TryThemeColor("SolidBackgroundFillColorBase")
                ?? TryThemeColorFromBrush("SolidBackgroundFillColorBaseBrush")
                ?? TryThemeColorFromBrush("WindowBackground")
                ?? color;
        }

        return color.A == 255 ? color : Color.FromArgb(255, color.R, color.G, color.B);
    }

    /// <summary>
    /// Resolves the tint overlay. A transparent <see cref="TintColor"/> means "follow the theme";
    /// that is also the dependency property default, so an unstyled <see cref="FWBackdrop"/>
    /// cannot fall back to painting <see cref="Colors.Transparent"/>'s white RGB as a full-strength
    /// overlay (which used to render as ~86% opaque white in every theme).
    /// </summary>
    private Color ResolveTintColor()
    {
        var tint = TintColor;
        if (tint.A != 0)
            return tint;

        return TryThemeColor("LayerFillColorDefault")
            ?? TryThemeColorFromBrush("LayerFillColorDefaultBrush")
            ?? tint;
    }

    private Color ResolveBackdropColor()
    {
        var baseColor = ResolveBaseColor();
        var tint = ResolveTintColor();
        var tintAlpha = tint.A / 255.0;
        if (tintAlpha <= 0)
            return baseColor;

        var tintRgb = AdjustTintForMaterial(tint);

        // WinUI Mica/Acrylic composite a tint layer (TintOpacity) and a luminosity layer
        // (LuminosityOpacity) over the base. Without a sampling backdrop both layers reduce to
        // alpha blends, so compose them with source-over math instead of choosing one and
        // leaving the other property dead.
        var tintLayer = Math.Clamp(TintOpacity, 0.0, 1.0) * tintAlpha;
        var luminosityLayer = Math.Clamp(LuminosityOpacity, 0.0, 1.0) * tintAlpha;
        var strength = 1.0 - ((1.0 - tintLayer) * (1.0 - luminosityLayer));

        return Blend(baseColor, tintRgb, strength);
    }

    private Color AdjustTintForMaterial(Color tint)
    {
        return Type switch
        {
            FWBackdropType.MicaAlt => Color.FromArgb(tint.A, Scale(tint.R, 0.9), Scale(tint.G, 0.9), Scale(tint.B, 0.9)),
            FWBackdropType.Tabbed => Color.FromArgb(tint.A, Offset(tint.R, 10), Offset(tint.G, 10), Offset(tint.B, 10)),
            _ => tint
        };
    }

    private static byte Scale(byte channel, double factor) =>
        (byte)Math.Clamp((int)Math.Round(channel * factor), 0, 255);

    private static byte Offset(byte channel, int delta) =>
        (byte)Math.Clamp(channel + delta, 0, 255);

    private static Color? TryThemeColor(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color)
            return color;

        return null;
    }

    private static Color? TryThemeColorFromBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is SolidColorBrush brush)
            return brush.Color;

        return null;
    }

    private static Color Blend(Color baseColor, Color tint, double factor)
    {
        factor = Math.Clamp(factor, 0.0, 1.0);

        static byte Lerp(byte from, byte to, double t) =>
            (byte)Math.Clamp((int)Math.Round(from + ((to - from) * t)), 0, 255);

        return Color.FromArgb(
            255,
            Lerp(baseColor.R, tint.R, factor),
            Lerp(baseColor.G, tint.G, factor),
            Lerp(baseColor.B, tint.B, factor));
    }

    private void OnThemeChanged() => InvalidateVisual();

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        FluentThemeManager.ThemeChanged -= OnThemeChanged;
        Unloaded -= OnUnloaded;
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
