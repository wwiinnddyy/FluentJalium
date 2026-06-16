using Jalium.UI;
using Jalium.UI.Media;
using Jalium.UI.Media.Effects;

namespace FluentJalium.Controls;

/// <summary>
/// Builds Jalium raster effects from the FluentJalium depth and Reveal theme tokens.
/// This exposes the Fluent "Light" and "Depth" pillars through the same resource system
/// used by Mica/Acrylic, so control themes and Gallery samples can reference elevation
/// by semantic level instead of hard-coded shadow numbers.
/// </summary>
public static class FWFluentEffects
{
    /// <summary>
    /// Creates a <see cref="DropShadowEffect"/> for the requested Fluent elevation level,
    /// reading the shadow depth, blur radius, and opacity from the FluentMaterials tokens.
    /// </summary>
    public static DropShadowEffect CreateElevation(FWElevationLevel level)
    {
        var (depth, blur, opacity) = ReadElevationTokens(level);
        var shadowColor = ResolveColor("FluentDepthShadowBrush", Color.FromArgb(0xFF, 0, 0, 0));

        return new DropShadowEffect
        {
            BlurRadius = blur,
            ShadowDepth = depth,
            Direction = 270, // Fluent key light: straight down
            Opacity = opacity,
            Color = shadowColor,
            RenderingBias = RenderingBias.Quality
        };
    }

    /// <summary>
    /// Creates a pointer-driven Reveal <see cref="OuterGlowEffect"/> for the requested
    /// Reveal mode, reading the glow size and opacity from the FluentMaterials tokens.
    /// Pass a value in <paramref name="intensity"/> between 0 (idle) and 1 (fully lit).
    /// </summary>
    public static OuterGlowEffect? CreateReveal(FWRevealMode mode, double intensity = 1.0)
    {
        if (mode == FWRevealMode.None || intensity <= 0)
        {
            return null;
        }

        var isHalo = mode == FWRevealMode.Halo;
        var glowSize = ReadDouble(isHalo ? "FluentRevealHaloGlowSize" : "FluentRevealBorderGlowSize", isHalo ? 18 : 8);
        var baseOpacity = ReadDouble(isHalo ? "FluentRevealHaloOpacity" : "FluentRevealBorderOpacity", isHalo ? 0.65 : 0.55);
        var intensityMultiplier = ReadDouble("FluentRevealHaloIntensity", 1.2);
        var accent = ResolveColor("FluentRevealAccentBrush", Color.FromRgb(0x00, 0x78, 0xD4));

        return new OuterGlowEffect
        {
            GlowSize = glowSize * Math.Clamp(intensity, 0, 1),
            GlowColor = accent,
            Opacity = baseOpacity * Math.Clamp(intensity, 0, 1),
            Intensity = isHalo ? intensityMultiplier : 1.0
        };
    }

    /// <summary>
    /// Composes elevation and Reveal into a single <see cref="EffectGroup"/>.
    /// Returns <c>null</c> when neither effect is active.
    /// </summary>
    public static EffectGroup? CreateComposite(FWElevationLevel level, FWRevealMode revealMode, double revealIntensity = 0.0)
    {
        var hasDepth = level != FWElevationLevel.None;
        var reveal = CreateReveal(revealMode, revealIntensity);
        if (!hasDepth && reveal is null)
        {
            return null;
        }

        var group = new EffectGroup();
        if (hasDepth)
        {
            group.Children.Add(CreateElevation(level));
        }
        if (reveal is not null)
        {
            group.Children.Add(reveal);
        }
        return group;
    }

    private static (double depth, double blur, double opacity) ReadElevationTokens(FWElevationLevel level)
    {
        return level switch
        {
            FWElevationLevel.None => (0, 0, 0),
            FWElevationLevel.Resting => (
                ReadDouble("FluentDepthRestingShadowDepth", 2),
                ReadDouble("FluentDepthRestingBlurRadius", 8),
                ReadDouble("FluentDepthRestingOpacity", 0.12)),
            FWElevationLevel.Hover => (
                ReadDouble("FluentDepthHoverShadowDepth", 6),
                ReadDouble("FluentDepthHoverBlurRadius", 16),
                ReadDouble("FluentDepthHoverOpacity", 0.18)),
            FWElevationLevel.Flyout => (
                ReadDouble("FluentDepthFlyoutShadowDepth", 8),
                ReadDouble("FluentDepthFlyoutBlurRadius", 24),
                ReadDouble("FluentDepthFlyoutOpacity", 0.22)),
            FWElevationLevel.Dialog => (
                ReadDouble("FluentDepthDialogShadowDepth", 16),
                ReadDouble("FluentDepthDialogBlurRadius", 40),
                ReadDouble("FluentDepthDialogOpacity", 0.28)),
            _ => (0, 0, 0)
        };
    }

    private static double ReadDouble(string key, double fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is double number)
        {
            return number;
        }
        return fallback;
    }

    private static Color ResolveColor(string brushKey, Color fallback)
    {
        if (Application.Current?.Resources.TryGetValue(brushKey, out var value) == true && value is SolidColorBrush brush)
        {
            return brush.Color;
        }
        return fallback;
    }
}
