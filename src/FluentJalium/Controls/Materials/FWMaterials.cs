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
/// FluentJalium window-level material roles that map onto Jalium's DWM system backdrops.
/// </summary>
public enum FWFluentWindowBackdropKind
{
    /// <summary>
    /// Uses the normal window background brush without requesting a system backdrop.
    /// </summary>
    None,

    /// <summary>
    /// WinUI-style Mica backdrop for the primary app shell.
    /// </summary>
    Mica,

    /// <summary>
    /// WinUI-style Mica Alt backdrop for tabbed or layered app shells.
    /// </summary>
    MicaAlt,

    /// <summary>
    /// WinUI-style acrylic backdrop for transient or high-depth windows.
    /// </summary>
    Acrylic
}

/// <summary>
/// FluentJalium surface roles used to layer content over window and element materials.
/// </summary>
public enum FWFluentMaterialRole
{
    /// <summary>
    /// No Fluent surface role styling.
    /// </summary>
    None,

    /// <summary>
    /// Root window surface painted over a DWM backdrop.
    /// </summary>
    Window,

    /// <summary>
    /// Navigation, title, or shell pane surface that needs stronger separation from content.
    /// </summary>
    ShellPane,

    /// <summary>
    /// Main content layer surface above the window backdrop.
    /// </summary>
    ContentLayer,

    /// <summary>
    /// Repeated card or settings section surface.
    /// </summary>
    Card,

    /// <summary>
    /// Transient surface for flyouts, menus, popups, and command overflow.
    /// </summary>
    Flyout,

    /// <summary>
    /// High-focus glass surface for previews, selected media, or hero panels.
    /// </summary>
    FocusGlass
}

/// <summary>
/// Describes how a FluentJalium window material role is applied to a Jalium window.
/// </summary>
public readonly record struct FWFluentWindowBackdropRecipe(
    FWFluentWindowBackdropKind Kind,
    WindowBackdropType SystemBackdrop,
    string Role,
    string Description)
{
    /// <summary>
    /// Creates the default recipe for the requested Fluent window backdrop role.
    /// </summary>
    public static FWFluentWindowBackdropRecipe Create(FWFluentWindowBackdropKind kind)
    {
        return kind switch
        {
            FWFluentWindowBackdropKind.None => new FWFluentWindowBackdropRecipe(
                kind,
                WindowBackdropType.None,
                "Solid shell",
                "Use the FluentJalium window background brush without a DWM system backdrop."),
            FWFluentWindowBackdropKind.Mica => new FWFluentWindowBackdropRecipe(
                kind,
                WindowBackdropType.Mica,
                "Mica shell",
                "Use the WinUI-style primary shell backdrop for long-lived app windows."),
            FWFluentWindowBackdropKind.MicaAlt => new FWFluentWindowBackdropRecipe(
                kind,
                WindowBackdropType.MicaAlt,
                "Mica Alt shell",
                "Use the tabbed shell backdrop when navigation, tabs, or dense panes need more separation."),
            FWFluentWindowBackdropKind.Acrylic => new FWFluentWindowBackdropRecipe(
                kind,
                WindowBackdropType.Acrylic,
                "Acrylic shell",
                "Use the transient DWM backdrop when a window needs stronger depth behind its content."),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown Fluent window backdrop kind.")
        };
    }

    /// <summary>
    /// Applies this recipe to the supplied Jalium window.
    /// </summary>
    public void ApplyTo(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        window.SystemBackdrop = SystemBackdrop;
    }
}

/// <summary>
/// Describes the visual surface role used by a FluentJalium material host.
/// </summary>
public readonly record struct FWFluentMaterialSurfaceRecipe(
    FWFluentMaterialRole Role,
    FWFluentMaterialKind MaterialKind,
    Brush? Background,
    Brush? BorderBrush,
    Thickness BorderThickness,
    CornerRadius CornerRadius,
    Thickness Padding,
    BorderShape Shape,
    double SuperEllipseN,
    string Description)
{
    /// <summary>
    /// Creates the default surface recipe for the supplied Fluent role.
    /// </summary>
    public static FWFluentMaterialSurfaceRecipe Create(FWFluentMaterialRole role)
    {
        return Create(role, Application.Current?.Resources);
    }

    /// <summary>
    /// Creates the default surface recipe for the supplied Fluent role using a resource scope.
    /// </summary>
    public static FWFluentMaterialSurfaceRecipe Create(FWFluentMaterialRole role, ResourceDictionary? resources)
    {
        return role switch
        {
            FWFluentMaterialRole.None => new FWFluentMaterialSurfaceRecipe(
                role,
                FWFluentMaterialKind.None,
                null,
                null,
                new Thickness(0),
                new CornerRadius(0),
                new Thickness(0),
                BorderShape.RoundedRectangle,
                4,
                "No Fluent material surface role."),
            FWFluentMaterialRole.Window => new FWFluentMaterialSurfaceRecipe(
                role,
                FWFluentMaterialKind.Layer,
                ResourceBrush(resources, "FluentMaterialWindowBackdropBrush", Color.FromRgb(0x20, 0x20, 0x20)),
                null,
                new Thickness(0),
                ResourceCornerRadius(resources, "FluentMaterialWindowSurfaceCornerRadius", new CornerRadius(0)),
                ResourceThickness(resources, "FluentMaterialWindowSurfacePadding", new Thickness(0)),
                BorderShape.RoundedRectangle,
                4,
                "Root app shell surface for a WinUI-style system backdrop."),
            FWFluentMaterialRole.ShellPane => new FWFluentMaterialSurfaceRecipe(
                role,
                FWFluentMaterialKind.MicaAlt,
                ResourceBrush(resources, "FluentMaterialShellPaneBrush", Color.FromRgb(0x2C, 0x2C, 0x2C)),
                ResourceBrush(resources, "FluentMaterialLayerBorderBrush", Color.FromArgb(0x15, 0xFF, 0xFF, 0xFF)),
                ResourceThickness(resources, "FluentMaterialShellPaneBorderThickness", new Thickness(0, 0, 1, 0)),
                ResourceCornerRadius(resources, "FluentMaterialShellPaneCornerRadius", new CornerRadius(0)),
                ResourceThickness(resources, "FluentMaterialShellPanePadding", new Thickness(0)),
                BorderShape.RoundedRectangle,
                4,
                "Mica Alt shell pane surface for navigation and app chrome."),
            FWFluentMaterialRole.ContentLayer => new FWFluentMaterialSurfaceRecipe(
                role,
                FWFluentMaterialKind.Layer,
                ResourceBrush(resources, "FluentMaterialContentLayerBrush", Color.FromArgb(0x4C, 0x3A, 0x3A, 0x3A)),
                null,
                new Thickness(0),
                ResourceCornerRadius(resources, "FluentMaterialContentLayerCornerRadius", new CornerRadius(0)),
                ResourceThickness(resources, "FluentMaterialContentLayerPadding", new Thickness(0)),
                BorderShape.RoundedRectangle,
                4,
                "Main content layer surface above the window material."),
            FWFluentMaterialRole.Card => new FWFluentMaterialSurfaceRecipe(
                role,
                FWFluentMaterialKind.Layer,
                ResourceBrush(resources, "FluentMaterialCardBrush", Color.FromArgb(0x66, 0x3A, 0x3A, 0x3A)),
                ResourceBrush(resources, "FluentMaterialLayerBorderBrush", Color.FromArgb(0x15, 0xFF, 0xFF, 0xFF)),
                ResourceThickness(resources, "FluentMaterialCardBorderThickness", new Thickness(1)),
                ResourceCornerRadius(resources, "FluentMaterialCardCornerRadius", new CornerRadius(8)),
                ResourceThickness(resources, "FluentMaterialCardPadding", new Thickness(16)),
                BorderShape.RoundedRectangle,
                4,
                "Layered card surface for grouped content."),
            FWFluentMaterialRole.Flyout => new FWFluentMaterialSurfaceRecipe(
                role,
                FWFluentMaterialKind.Acrylic,
                ResourceBrush(resources, "FluentMaterialTransientAcrylicBrush", Color.FromArgb(0xB0, 0x20, 0x54, 0x8F)),
                ResourceBrush(resources, "FluentMaterialLayerBorderBrush", Color.FromArgb(0x15, 0xFF, 0xFF, 0xFF)),
                ResourceThickness(resources, "FluentMaterialFlyoutBorderThickness", new Thickness(1)),
                ResourceCornerRadius(resources, "FluentMaterialFlyoutCornerRadius", new CornerRadius(8)),
                ResourceThickness(resources, "FluentMaterialFlyoutPadding", new Thickness(8)),
                BorderShape.RoundedRectangle,
                4,
                "Transient acrylic surface for menus, popups, and command flyouts."),
            FWFluentMaterialRole.FocusGlass => new FWFluentMaterialSurfaceRecipe(
                role,
                FWFluentMaterialKind.LiquidGlass,
                ResourceBrush(resources, "FluentMaterialFocusedGlassBrush", Color.FromArgb(0x2C, 0x00, 0x78, 0xD4)),
                ResourceBrush(resources, "FluentMaterialLayerBorderBrush", Color.FromArgb(0x15, 0xFF, 0xFF, 0xFF)),
                ResourceThickness(resources, "FluentMaterialFocusGlassBorderThickness", new Thickness(1)),
                ResourceCornerRadius(resources, "FluentMaterialFocusGlassCornerRadius", new CornerRadius(12)),
                ResourceThickness(resources, "FluentMaterialFocusGlassPadding", new Thickness(12)),
                BorderShape.SuperEllipse,
                ResourceDouble(resources, "FluentMaterialFocusGlassSuperEllipseN", 4),
                "Interactive Jalium liquid glass focus surface."),
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown Fluent material surface role.")
        };
    }

    private static Brush ResourceBrush(ResourceDictionary? resources, string key, Color fallback)
    {
        if (resources?.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(fallback);
    }

    private static CornerRadius ResourceCornerRadius(ResourceDictionary? resources, string key, CornerRadius fallback)
    {
        if (resources?.TryGetValue(key, out var value) != true)
        {
            return fallback;
        }

        return value switch
        {
            CornerRadius cornerRadius => cornerRadius,
            double number when double.IsFinite(number) => new CornerRadius(Math.Max(0, number)),
            int number => new CornerRadius(Math.Max(0, number)),
            string text when double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) => new CornerRadius(Math.Max(0, number)),
            _ => fallback
        };
    }

    private static Thickness ResourceThickness(ResourceDictionary? resources, string key, Thickness fallback)
    {
        if (resources?.TryGetValue(key, out var value) != true)
        {
            return fallback;
        }

        return value switch
        {
            Thickness thickness => thickness,
            double number when double.IsFinite(number) => new Thickness(Math.Max(0, number)),
            int number => new Thickness(Math.Max(0, number)),
            string text when double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) => new Thickness(Math.Max(0, number)),
            _ => fallback
        };
    }

    private static double ResourceDouble(ResourceDictionary? resources, string key, double fallback)
    {
        if (resources?.TryGetValue(key, out var value) != true)
        {
            return fallback;
        }

        return value switch
        {
            double number when double.IsFinite(number) => number,
            int number => number,
            string text when double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) && double.IsFinite(number) => number,
            _ => fallback
        };
    }
}

/// <summary>
/// Describes the effect recipe used by a FluentJalium material surface.
/// </summary>
public readonly record struct FWFluentMaterialRecipe(
    FWFluentMaterialKind MaterialKind,
    Color TintColor,
    double TintOpacity,
    double BlurRadius,
    double NoiseIntensity,
    double RefractionAmount,
    double ChromaticAberration,
    double FusionRadius,
    bool IsInteractive)
{
    /// <summary>
    /// Creates the default recipe for the requested material kind.
    /// </summary>
    public static FWFluentMaterialRecipe Create(FWFluentMaterialKind materialKind)
    {
        return Create(materialKind, Application.Current?.Resources);
    }

    /// <summary>
    /// Creates the default recipe for the requested material kind using the supplied FluentJalium resource scope.
    /// </summary>
    public static FWFluentMaterialRecipe Create(FWFluentMaterialKind materialKind, ResourceDictionary? resources)
    {
        return materialKind switch
        {
            FWFluentMaterialKind.None => new FWFluentMaterialRecipe(
                materialKind,
                Color.FromArgb(0, 0, 0, 0),
                0,
                0,
                0,
                0,
                0,
                0,
                false),
            FWFluentMaterialKind.Layer => new FWFluentMaterialRecipe(
                materialKind,
                Color.FromArgb(0, 0, 0, 0),
                0,
                0,
                0,
                0,
                0,
                0,
                false),
            FWFluentMaterialKind.Mica => new FWFluentMaterialRecipe(
                materialKind,
                ResourceColor(resources, "FluentMaterialMicaTintBrush", Color.FromArgb(180, 20, 84, 145)),
                ResourceDouble(resources, "FluentMaterialMicaTintOpacity", 0.18),
                ResourceDouble(resources, "FluentMaterialMicaBlurRadius", 18),
                0,
                0,
                0,
                0,
                false),
            FWFluentMaterialKind.MicaAlt => new FWFluentMaterialRecipe(
                materialKind,
                ResourceColor(resources, "FluentMaterialMicaAltTintBrush", Color.FromArgb(190, 20, 84, 145)),
                ResourceDouble(resources, "FluentMaterialMicaAltTintOpacity", 0.26),
                ResourceDouble(resources, "FluentMaterialMicaAltBlurRadius", 22),
                0,
                0,
                0,
                0,
                false),
            FWFluentMaterialKind.Acrylic => new FWFluentMaterialRecipe(
                materialKind,
                ResourceColor(resources, "FluentMaterialAcrylicTintBrush", Color.FromArgb(180, 20, 84, 145)),
                ResourceDouble(resources, "FluentMaterialAcrylicTintOpacity", 0.46),
                ResourceDouble(resources, "FluentMaterialAcrylicBlurRadius", 28),
                ResourceDouble(resources, "FluentMaterialAcrylicNoiseIntensity", 0.035),
                0,
                0,
                0,
                false),
            FWFluentMaterialKind.FrostedGlass => new FWFluentMaterialRecipe(
                materialKind,
                ResourceColor(resources, "FluentMaterialFrostedGlassTintBrush", Color.FromArgb(160, 255, 255, 255)),
                ResourceDouble(resources, "FluentMaterialFrostedGlassTintOpacity", 0.32),
                ResourceDouble(resources, "FluentMaterialFrostedGlassBlurRadius", 34),
                ResourceDouble(resources, "FluentMaterialFrostedGlassNoiseIntensity", 0.045),
                0,
                0,
                0,
                false),
            FWFluentMaterialKind.LiquidGlass => new FWFluentMaterialRecipe(
                materialKind,
                ResourceColor(resources, "FluentMaterialLiquidGlassTintBrush", Color.FromArgb(180, 20, 84, 145)),
                ResourceDouble(resources, "FluentMaterialLiquidGlassTintOpacity", 0.22),
                ResourceDouble(resources, "FluentMaterialLiquidGlassBlurRadius", 14),
                0,
                ResourceDouble(resources, "FluentMaterialLiquidGlassRefractionAmount", 84),
                ResourceDouble(resources, "FluentMaterialLiquidGlassChromaticAberration", 0.55),
                ResourceDouble(resources, "FluentMaterialLiquidGlassFusionRadius", 24),
                true),
            _ => throw new ArgumentOutOfRangeException(nameof(materialKind), materialKind, "Unknown Fluent material kind.")
        };
    }

    private static Color ResourceColor(ResourceDictionary? resources, string key, Color fallback)
    {
        if (resources?.TryGetValue(key, out var value) != true)
        {
            return fallback;
        }

        return value switch
        {
            SolidColorBrush brush => brush.Color,
            Color color => color,
            _ => fallback
        };
    }

    private static double ResourceDouble(ResourceDictionary? resources, string key, double fallback)
    {
        if (resources?.TryGetValue(key, out var value) != true)
        {
            return fallback;
        }

        return value switch
        {
            double number when double.IsFinite(number) => number,
            int number => number,
            string text when double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) => number,
            _ => fallback
        };
    }
}

/// <summary>
/// A FluentJalium material surface that maps WinUI-style material names onto Jalium backdrop and liquid glass effects.
/// </summary>
public class FWFluentMaterialSurface : Border, IFluentJaliumControl
{
    private bool _suppressMaterialRoleRecipe;

    public static readonly DependencyProperty MaterialRoleProperty =
        DependencyProperty.Register(nameof(MaterialRole), typeof(FWFluentMaterialRole), typeof(FWFluentMaterialSurface),
            new PropertyMetadata(FWFluentMaterialRole.None, OnMaterialRolePropertyChanged), IsValidMaterialRole);

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
    /// Gets or sets the Fluent role used to configure this material surface.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public FWFluentMaterialRole MaterialRole
    {
        get => (FWFluentMaterialRole)GetValue(MaterialRoleProperty)!;
        set => SetValue(MaterialRoleProperty, value);
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

    /// <summary>
    /// Applies the default material recipe for the supplied kind.
    /// </summary>
    public void UseMaterialRecipe(FWFluentMaterialKind materialKind)
    {
        UseMaterialRecipe(FWFluentMaterialRecipe.Create(materialKind));
    }

    /// <summary>
    /// Applies the default surface recipe for the supplied Fluent role.
    /// </summary>
    public void UseMaterialRole(FWFluentMaterialRole materialRole)
    {
        UseMaterialSurfaceRecipe(FWFluentMaterialSurfaceRecipe.Create(materialRole));
    }

    /// <summary>
    /// Applies an explicit surface role recipe to this surface.
    /// </summary>
    public void UseMaterialSurfaceRecipe(FWFluentMaterialSurfaceRecipe recipe)
    {
        _suppressMaterialRoleRecipe = true;
        try
        {
            MaterialRole = recipe.Role;
            UseMaterialRecipe(recipe.MaterialKind);
            Background = recipe.Background;
            BorderBrush = recipe.BorderBrush;
            BorderThickness = recipe.BorderThickness;
            CornerRadius = recipe.CornerRadius;
            Padding = recipe.Padding;
            Shape = recipe.Shape;
            SuperEllipseN = recipe.SuperEllipseN;
        }
        finally
        {
            _suppressMaterialRoleRecipe = false;
        }
    }

    /// <summary>
    /// Applies an explicit material recipe to this surface.
    /// </summary>
    public void UseMaterialRecipe(FWFluentMaterialRecipe recipe)
    {
        MaterialKind = recipe.MaterialKind;
        TintColor = recipe.TintColor;
        TintOpacity = recipe.TintOpacity;
        BlurRadius = recipe.BlurRadius;
        NoiseIntensity = recipe.NoiseIntensity;
        RefractionAmount = recipe.RefractionAmount;
        ChromaticAberration = recipe.ChromaticAberration;
        FusionRadius = recipe.FusionRadius;
        IsInteractive = recipe.IsInteractive;
    }

    private static void OnMaterialRolePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWFluentMaterialSurface surface &&
            !surface._suppressMaterialRoleRecipe &&
            e.NewValue is FWFluentMaterialRole role)
        {
            surface.UseMaterialRole(role);
        }
    }

    protected void SetMaterialRoleSilently(FWFluentMaterialRole materialRole)
    {
        _suppressMaterialRoleRecipe = true;
        try
        {
            MaterialRole = materialRole;
        }
        finally
        {
            _suppressMaterialRoleRecipe = false;
        }
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

    private static bool IsValidMaterialRole(object? value) => value is FWFluentMaterialRole;

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

/// <summary>
/// A FluentJalium root surface that couples a WinUI-style window backdrop role with in-window material tokens.
/// </summary>
public class FWFluentWindowSurface : FWFluentMaterialSurface
{
    public static readonly DependencyProperty WindowBackdropKindProperty =
        DependencyProperty.Register(nameof(WindowBackdropKind), typeof(FWFluentWindowBackdropKind), typeof(FWFluentWindowSurface),
            new PropertyMetadata(FWFluentWindowBackdropKind.Mica, OnWindowBackdropPropertyChanged), IsValidWindowBackdropKind);

    public static readonly DependencyProperty AutoApplyWindowBackdropProperty =
        DependencyProperty.Register(nameof(AutoApplyWindowBackdrop), typeof(bool), typeof(FWFluentWindowSurface),
            new PropertyMetadata(true, OnWindowBackdropPropertyChanged));

    public FWFluentWindowSurface()
    {
        SetMaterialRoleSilently(FWFluentMaterialRole.Window);
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Gets or sets the DWM system backdrop role that should be applied to the host window.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public FWFluentWindowBackdropKind WindowBackdropKind
    {
        get => (FWFluentWindowBackdropKind)GetValue(WindowBackdropKindProperty)!;
        set => SetValue(WindowBackdropKindProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this surface should apply <see cref="WindowBackdropKind"/> when it is loaded.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool AutoApplyWindowBackdrop
    {
        get => (bool)GetValue(AutoApplyWindowBackdropProperty)!;
        set => SetValue(AutoApplyWindowBackdropProperty, value);
    }

    /// <summary>
    /// Applies the selected Fluent window backdrop to the supplied Jalium window.
    /// </summary>
    public void ApplyWindowBackdrop(Window window)
    {
        FWFluentWindowBackdropRecipe.Create(WindowBackdropKind).ApplyTo(window);
    }

    /// <summary>
    /// Applies the selected Fluent window backdrop to the current host window when one exists.
    /// </summary>
    public bool TryApplyWindowBackdrop()
    {
        var window = Window.GetWindow(this);
        if (window == null)
        {
            return false;
        }

        ApplyWindowBackdrop(window);
        return true;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (AutoApplyWindowBackdrop)
        {
            TryApplyWindowBackdrop();
        }
    }

    private static void OnWindowBackdropPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWFluentWindowSurface { AutoApplyWindowBackdrop: true } surface)
        {
            surface.TryApplyWindowBackdrop();
        }
    }

    private static bool IsValidWindowBackdropKind(object? value) => value is FWFluentWindowBackdropKind;
}
