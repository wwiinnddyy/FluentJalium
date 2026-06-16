using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Media;
using Jalium.UI.Media.Effects;

namespace FluentJalium.Controls;

/// <summary>
/// Fluent Design elevation levels mapped to soft drop shadow depth.
/// These mirror the WinUI/Fluent z-depth scale (0 = flush, 8 = floating).
/// </summary>
public enum FWElevationLevel
{
    /// <summary>
    /// No elevation. The element sits flat on its parent surface.
    /// </summary>
    None = 0,

    /// <summary>
    /// Subtle resting elevation for cards and list items.
    /// </summary>
    Resting = 1,

    /// <summary>
    /// Hover elevation for interactive cards.
    /// </summary>
    Hover = 2,

    /// <summary>
    /// Elevation for flyouts, menus, and command overflow.
    /// </summary>
    Flyout = 4,

    /// <summary>
    /// Elevation for dialogs and floating panels.
    /// </summary>
    Dialog = 8
}

/// <summary>
/// Describes the Fluent Reveal light behavior applied to a surface.
/// </summary>
public enum FWRevealMode
{
    /// <summary>
    /// No Reveal light is rendered.
    /// </summary>
    None,

    /// <summary>
    /// A subtle accent-tinted glow follows pointer proximity along the border edge.
    /// </summary>
    Border,

    /// <summary>
    /// The whole surface emits a soft accent halo while hovered.
    /// </summary>
    Halo
}

/// <summary>
/// A Fluent Design "Light &amp; Depth" surface that combines Jalium's rasterized
/// <see cref="DropShadowEffect"/> elevation with a pointer-driven <see cref="OuterGlowEffect"/>
/// Reveal light. This control is the missing fifth Fluent pillar (Light + Depth) for
/// FluentJalium and layers cleanly over Mica/Acrylic without touching Jalium itself.
/// </summary>
public class FWRevealSurface : Border, IFluentJaliumControl
{
    /// <summary>
    /// Identifies the <see cref="ElevationLevel"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ElevationLevelProperty =
        DependencyProperty.Register(nameof(ElevationLevel), typeof(FWElevationLevel), typeof(FWRevealSurface),
            new PropertyMetadata(FWElevationLevel.Resting, OnDepthOrRevealChanged));

    /// <summary>
    /// Identifies the <see cref="RevealMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty RevealModeProperty =
        DependencyProperty.Register(nameof(RevealMode), typeof(FWRevealMode), typeof(FWRevealSurface),
            new PropertyMetadata(FWRevealMode.Border, OnDepthOrRevealChanged));

    /// <summary>
    /// Identifies the <see cref="RevealColor"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty RevealColorProperty =
        DependencyProperty.Register(nameof(RevealColor), typeof(Color), typeof(FWRevealSurface),
            new PropertyMetadata(Color.FromRgb(0x00, 0x78, 0xD4), OnDepthOrRevealChanged));

    /// <summary>
    /// Identifies the <see cref="ShadowColor"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShadowColorProperty =
        DependencyProperty.Register(nameof(ShadowColor), typeof(Color), typeof(FWRevealSurface),
            new PropertyMetadata(Color.FromArgb(0xFF, 0, 0, 0), OnDepthOrRevealChanged));

    private double _revealIntensity;

    /// <summary>
    /// Initializes a new instance of the <see cref="FWRevealSurface"/> class.
    /// </summary>
    public FWRevealSurface()
    {
        _revealIntensity = 0;
        UpdateCompositeEffect();
    }

    /// <summary>
    /// Gets or sets the Fluent elevation level used to derive the drop shadow.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public FWElevationLevel ElevationLevel
    {
        get => (FWElevationLevel)GetValue(ElevationLevelProperty)!;
        set => SetValue(ElevationLevelProperty, value);
    }

    /// <summary>
    /// Gets or sets the Fluent Reveal light behavior.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public FWRevealMode RevealMode
    {
        get => (FWRevealMode)GetValue(RevealModeProperty)!;
        set => SetValue(RevealModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the accent-tinted color of the Reveal glow.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public Color RevealColor
    {
        get => (Color)GetValue(RevealColorProperty)!;
        set => SetValue(RevealColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color of the elevation drop shadow.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public Color ShadowColor
    {
        get => (Color)GetValue(ShadowColorProperty)!;
        set => SetValue(ShadowColorProperty, value);
    }

    /// <summary>
    /// Sets the animated Reveal intensity (0 = idle, 1 = fully lit).
    /// </summary>
    public double RevealIntensity
    {
        get => _revealIntensity;
        set
        {
            _revealIntensity = Math.Clamp(value, 0, 1);
            UpdateCompositeEffect();
        }
    }

    /// <summary>
    /// Returns the shadow depth (in DIPs) for a given Fluent elevation level.
    /// </summary>
    public static double GetShadowDepth(FWElevationLevel level) => level switch
    {
        FWElevationLevel.None => 0,
        FWElevationLevel.Resting => 2,
        FWElevationLevel.Hover => 6,
        FWElevationLevel.Flyout => 8,
        FWElevationLevel.Dialog => 16,
        _ => 0
    };

    /// <summary>
    /// Returns the shadow blur radius for a given Fluent elevation level.
    /// </summary>
    public static double GetShadowBlur(FWElevationLevel level) => level switch
    {
        FWElevationLevel.None => 0,
        FWElevationLevel.Resting => 8,
        FWElevationLevel.Hover => 16,
        FWElevationLevel.Flyout => 24,
        FWElevationLevel.Dialog => 40,
        _ => 0
    };

    /// <summary>
    /// Returns the shadow opacity for a given Fluent elevation level.
    /// </summary>
    public static double GetShadowOpacity(FWElevationLevel level) => level switch
    {
        FWElevationLevel.None => 0,
        FWElevationLevel.Resting => 0.12,
        FWElevationLevel.Hover => 0.18,
        FWElevationLevel.Flyout => 0.22,
        FWElevationLevel.Dialog => 0.28,
        _ => 0
    };

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        RevealIntensity = 1;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        RevealIntensity = 0;
    }

    private static void OnDepthOrRevealChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRevealSurface surface)
        {
            surface.UpdateCompositeEffect();
        }
    }

    private void UpdateCompositeEffect()
    {
        // Compose depth + Reveal from the shared FluentMaterials tokens so the surface,
        // the helper, and the theme dictionary all agree on a single elevation scale.
        var composite = FWFluentEffects.CreateComposite(ElevationLevel, RevealMode, _revealIntensity);
        if (composite is null)
        {
            Effect = null;
            return;
        }

        // Honor this surface's explicit color overrides on top of the theme defaults.
        foreach (var child in composite.Children)
        {
            if (child is DropShadowEffect shadow)
            {
                shadow.Color = ShadowColor;
            }
            else if (child is OuterGlowEffect glow)
            {
                glow.GlowColor = RevealColor;
            }
        }

        Effect = composite;
    }
}
