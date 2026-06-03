using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium material presets for <see cref="FWFluentMaterialSurface"/>.
/// </summary>
public enum FWFluentMaterialKind
{
    /// <summary>
    /// No generated material effect.
    /// </summary>
    None,

    /// <summary>
    /// A regular Fluent layer surface that relies on inherited background and border styling.
    /// </summary>
    Layer,

    /// <summary>
    /// WinUI-style Mica element backdrop material.
    /// </summary>
    Mica,

    /// <summary>
    /// WinUI-style alternate Mica element backdrop material.
    /// </summary>
    MicaAlt,

    /// <summary>
    /// WinUI-style acrylic element backdrop material.
    /// </summary>
    Acrylic,

    /// <summary>
    /// Frosted glass backdrop material.
    /// </summary>
    FrostedGlass,

    /// <summary>
    /// Jalium HLSL liquid glass material with refraction, highlight, and inner shadow.
    /// </summary>
    LiquidGlass
}

/// <summary>
/// A FluentJalium material surface that maps WinUI-style material names onto Jalium backdrop and liquid glass effects.
/// </summary>
public class FWFluentMaterialSurface : Border, IFluentJaliumControl
{
    public static readonly DependencyProperty MaterialKindProperty =
        DependencyProperty.Register(nameof(MaterialKind), typeof(FWFluentMaterialKind), typeof(FWFluentMaterialSurface),
            new PropertyMetadata(FWFluentMaterialKind.Layer, OnMaterialPropertyChanged), IsValidMaterialKind);

    public static readonly DependencyProperty TintColorProperty =
        DependencyProperty.Register(nameof(TintColor), typeof(Color), typeof(FWFluentMaterialSurface),
            new PropertyMetadata(Color.FromArgb(180, 0, 120, 212), OnMaterialPropertyChanged));

    public static readonly DependencyProperty TintOpacityProperty =
        DependencyProperty.Register(nameof(TintOpacity), typeof(double), typeof(FWFluentMaterialSurface),
            new PropertyMetadata(0.42, OnMaterialPropertyChanged), IsValidOpacity);

    public static readonly DependencyProperty BlurRadiusProperty =
        DependencyProperty.Register(nameof(BlurRadius), typeof(double), typeof(FWFluentMaterialSurface),
            new PropertyMetadata(30.0, OnMaterialPropertyChanged), IsValidNonNegativeDouble);

    public static readonly DependencyProperty NoiseIntensityProperty =
        DependencyProperty.Register(nameof(NoiseIntensity), typeof(double), typeof(FWFluentMaterialSurface),
            new PropertyMetadata(0.03, OnMaterialPropertyChanged), IsValidOpacity);

    public static readonly DependencyProperty RefractionAmountProperty =
        DependencyProperty.Register(nameof(RefractionAmount), typeof(double), typeof(FWFluentMaterialSurface),
            new PropertyMetadata(60.0, OnMaterialPropertyChanged), IsValidNonNegativeDouble);

    public static readonly DependencyProperty ChromaticAberrationProperty =
        DependencyProperty.Register(nameof(ChromaticAberration), typeof(double), typeof(FWFluentMaterialSurface),
            new PropertyMetadata(0.35, OnMaterialPropertyChanged), IsValidNonNegativeDouble);

    public static readonly DependencyProperty FusionRadiusProperty =
        DependencyProperty.Register(nameof(FusionRadius), typeof(double), typeof(FWFluentMaterialSurface),
            new PropertyMetadata(30.0, OnMaterialPropertyChanged), IsValidNonNegativeDouble);

    public static readonly DependencyProperty IsInteractiveProperty =
        DependencyProperty.Register(nameof(IsInteractive), typeof(bool), typeof(FWFluentMaterialSurface),
            new PropertyMetadata(true, OnMaterialPropertyChanged));

    public FWFluentMaterialSurface()
    {
        ApplyMaterial();
    }

    /// <summary>
    /// Gets or sets the Fluent material preset.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public FWFluentMaterialKind MaterialKind
    {
        get => (FWFluentMaterialKind)GetValue(MaterialKindProperty)!;
        set => SetValue(MaterialKindProperty, value);
    }

    /// <summary>
    /// Gets or sets the tint color used by acrylic, frosted glass, and Mica backdrop materials.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public Color TintColor
    {
        get => (Color)GetValue(TintColorProperty)!;
        set => SetValue(TintColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the tint opacity used by generated material effects.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public double TintOpacity
    {
        get => (double)GetValue(TintOpacityProperty)!;
        set => SetValue(TintOpacityProperty, value);
    }

    /// <summary>
    /// Gets or sets the blur radius used by generated material effects.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public double BlurRadius
    {
        get => (double)GetValue(BlurRadiusProperty)!;
        set => SetValue(BlurRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the frosted or acrylic noise intensity.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public double NoiseIntensity
    {
        get => (double)GetValue(NoiseIntensityProperty)!;
        set => SetValue(NoiseIntensityProperty, value);
    }

    /// <summary>
    /// Gets or sets the liquid glass refraction strength.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public double RefractionAmount
    {
        get => (double)GetValue(RefractionAmountProperty)!;
        set => SetValue(RefractionAmountProperty, value);
    }

    /// <summary>
    /// Gets or sets the liquid glass chromatic aberration amount.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public double ChromaticAberration
    {
        get => (double)GetValue(ChromaticAberrationProperty)!;
        set => SetValue(ChromaticAberrationProperty, value);
    }

    /// <summary>
    /// Gets or sets the liquid glass fusion radius for adjacent glass panels.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public double FusionRadius
    {
        get => (double)GetValue(FusionRadiusProperty)!;
        set => SetValue(FusionRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets whether liquid glass reacts to pointer and press interaction.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsInteractive
    {
        get => (bool)GetValue(IsInteractiveProperty)!;
        set => SetValue(IsInteractiveProperty, value);
    }

    private static void OnMaterialPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWFluentMaterialSurface surface)
        {
            surface.ApplyMaterial();
        }
    }

    private void ApplyMaterial()
    {
        var kind = MaterialKind;
        var isLiquidGlass = kind == FWFluentMaterialKind.LiquidGlass;

        LiquidGlass = isLiquidGlass;
        LiquidGlassInteractive = isLiquidGlass && IsInteractive;
        LiquidGlassBlurRadius = BlurRadius;
        LiquidGlassRefractionAmount = RefractionAmount;
        LiquidGlassChromaticAberration = ChromaticAberration;
        LiquidGlassFusionRadius = FusionRadius;

        BackdropEffect = isLiquidGlass ? null : CreateBackdropEffect(kind);
    }

    private IBackdropEffect? CreateBackdropEffect(FWFluentMaterialKind kind)
    {
        return kind switch
        {
            FWFluentMaterialKind.Mica => CreateMica(useAlt: false),
            FWFluentMaterialKind.MicaAlt => CreateMica(useAlt: true),
            FWFluentMaterialKind.Acrylic => CreateAcrylic(),
            FWFluentMaterialKind.FrostedGlass => CreateFrostedGlass(),
            _ => null
        };
    }

    private MicaEffect CreateMica(bool useAlt)
    {
        return new MicaEffect(useAlt)
        {
            TintColor = TintColor,
            TintOpacity = (float)TintOpacity,
            BlurRadius = (float)BlurRadius,
            BlurSigma = (float)BlurRadius / 3.0f
        };
    }

    private AcrylicEffect CreateAcrylic()
    {
        return new AcrylicEffect(TintColor, (float)TintOpacity, (float)BlurRadius)
        {
            NoiseIntensity = (float)NoiseIntensity
        };
    }

    private FrostedGlassEffect CreateFrostedGlass()
    {
        return new FrostedGlassEffect((float)BlurRadius, (float)NoiseIntensity, TintColor, (float)TintOpacity);
    }

    private static bool IsValidMaterialKind(object? value) => value is FWFluentMaterialKind;

    private static bool IsValidOpacity(object? value)
    {
        return value is double number && double.IsFinite(number) && number >= 0 && number <= 1;
    }

    private static bool IsValidNonNegativeDouble(object? value)
    {
        return value is double number && double.IsFinite(number) && number >= 0;
    }
}
