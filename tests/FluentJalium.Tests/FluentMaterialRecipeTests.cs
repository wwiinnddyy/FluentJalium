using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentMaterialRecipeTests
{
    [Theory]
    [InlineData(FWFluentWindowBackdropKind.None, WindowBackdropType.None, "Solid shell")]
    [InlineData(FWFluentWindowBackdropKind.Mica, WindowBackdropType.Mica, "Mica shell")]
    [InlineData(FWFluentWindowBackdropKind.MicaAlt, WindowBackdropType.MicaAlt, "Mica Alt shell")]
    [InlineData(FWFluentWindowBackdropKind.Acrylic, WindowBackdropType.Acrylic, "Acrylic shell")]
    public void WindowBackdropRecipe_ShouldMapFluentRolesToJaliumSystemBackdrops(
        FWFluentWindowBackdropKind kind,
        WindowBackdropType expectedBackdrop,
        string expectedRole)
    {
        var recipe = FWFluentWindowBackdropRecipe.Create(kind);

        Assert.Equal(kind, recipe.Kind);
        Assert.Equal(expectedBackdrop, recipe.SystemBackdrop);
        Assert.Equal(expectedRole, recipe.Role);
        Assert.False(string.IsNullOrWhiteSpace(recipe.Description));
    }

    [Fact]
    public void WindowBackdropRecipe_ShouldApplyToJaliumWindow()
    {
        var window = new Window();
        var recipe = FWFluentWindowBackdropRecipe.Create(FWFluentWindowBackdropKind.MicaAlt);

        recipe.ApplyTo(window);

        Assert.Equal(WindowBackdropType.MicaAlt, window.SystemBackdrop);
    }

    [Theory]
    [InlineData(FWFluentWindowMaterialProfile.Solid, FWFluentWindowBackdropKind.None, WindowBackdropType.None, FWFluentMaterialRole.Window, FWFluentMaterialKind.Layer, "Solid shell")]
    [InlineData(FWFluentWindowMaterialProfile.MicaShell, FWFluentWindowBackdropKind.Mica, WindowBackdropType.Mica, FWFluentMaterialRole.Window, FWFluentMaterialKind.Layer, "Mica shell")]
    [InlineData(FWFluentWindowMaterialProfile.TabbedMicaAlt, FWFluentWindowBackdropKind.MicaAlt, WindowBackdropType.MicaAlt, FWFluentMaterialRole.ShellPane, FWFluentMaterialKind.MicaAlt, "Tabbed Mica Alt shell")]
    [InlineData(FWFluentWindowMaterialProfile.TransientAcrylic, FWFluentWindowBackdropKind.Acrylic, WindowBackdropType.Acrylic, FWFluentMaterialRole.Flyout, FWFluentMaterialKind.Acrylic, "Transient acrylic shell")]
    [InlineData(FWFluentWindowMaterialProfile.FocusGlassShell, FWFluentWindowBackdropKind.MicaAlt, WindowBackdropType.MicaAlt, FWFluentMaterialRole.FocusGlass, FWFluentMaterialKind.LiquidGlass, "Focus glass shell")]
    public void WindowMaterialProfileRecipe_ShouldComposeBackdropAndSurfaceRoles(
        FWFluentWindowMaterialProfile profile,
        FWFluentWindowBackdropKind expectedBackdropKind,
        WindowBackdropType expectedBackdrop,
        FWFluentMaterialRole expectedSurfaceRole,
        FWFluentMaterialKind expectedMaterialKind,
        string expectedRole)
    {
        var recipe = FWFluentWindowMaterialProfileRecipe.Create(profile);

        Assert.Equal(profile, recipe.Profile);
        Assert.Equal(expectedBackdropKind, recipe.WindowBackdropKind);
        Assert.Equal(expectedBackdrop, recipe.SystemBackdrop);
        Assert.Equal(expectedSurfaceRole, recipe.SurfaceRole);
        Assert.Equal(expectedMaterialKind, recipe.MaterialKind);
        Assert.Equal(expectedSurfaceRole, recipe.Surface.Role);
        Assert.Equal(expectedMaterialKind, recipe.Surface.MaterialKind);
        Assert.Equal(expectedRole, recipe.Role);
        Assert.False(string.IsNullOrWhiteSpace(recipe.Description));
    }

    [Fact]
    public void WindowMaterialProfileRecipe_ShouldReadDefaultProfileFromResourceDictionary()
    {
        var resources = new ResourceDictionary
        {
            ["FluentMaterialWindowDefaultProfile"] = "FocusGlassShell"
        };

        var recipe = FWFluentWindowMaterialProfileRecipe.CreateDefault(resources);

        Assert.Equal(FWFluentWindowMaterialProfile.FocusGlassShell, recipe.Profile);
        Assert.Equal(FWFluentWindowBackdropKind.MicaAlt, recipe.WindowBackdropKind);
        Assert.Equal(FWFluentMaterialRole.FocusGlass, recipe.SurfaceRole);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, recipe.MaterialKind);
    }

    [Fact]
    public void WindowMaterialProfileRecipe_ShouldFallBackWhenDefaultProfileResourceIsInvalid()
    {
        var resources = new ResourceDictionary
        {
            ["FluentMaterialWindowDefaultProfile"] = "UnknownProfile"
        };

        var recipe = FWFluentWindowMaterialProfileRecipe.CreateDefault(resources);

        Assert.Equal(FWFluentWindowMaterialProfile.MicaShell, recipe.Profile);
        Assert.Equal(FWFluentWindowBackdropKind.Mica, recipe.WindowBackdropKind);
    }

    [Theory]
    [InlineData(FWFluentMaterialRole.None, FWFluentMaterialKind.None, 0)]
    [InlineData(FWFluentMaterialRole.Window, FWFluentMaterialKind.Layer, 0)]
    [InlineData(FWFluentMaterialRole.ShellPane, FWFluentMaterialKind.MicaAlt, 0)]
    [InlineData(FWFluentMaterialRole.ContentLayer, FWFluentMaterialKind.Layer, 0)]
    [InlineData(FWFluentMaterialRole.Card, FWFluentMaterialKind.Layer, 8)]
    [InlineData(FWFluentMaterialRole.Flyout, FWFluentMaterialKind.Acrylic, 8)]
    [InlineData(FWFluentMaterialRole.FocusGlass, FWFluentMaterialKind.LiquidGlass, 12)]
    public void SurfaceRecipe_ShouldExposeDefaultsForEveryMaterialRole(
        FWFluentMaterialRole role,
        FWFluentMaterialKind expectedMaterialKind,
        double expectedCornerRadius)
    {
        var recipe = FWFluentMaterialSurfaceRecipe.Create(role);

        Assert.Equal(role, recipe.Role);
        Assert.Equal(expectedMaterialKind, recipe.MaterialKind);
        Assert.Equal(expectedCornerRadius, recipe.CornerRadius.TopLeft);
        Assert.False(string.IsNullOrWhiteSpace(recipe.Description));
    }

    [Fact]
    public void SurfaceRecipe_ShouldReadRoleTokensFromResourceDictionary()
    {
        var background = new SolidColorBrush(Color.FromRgb(12, 34, 56));
        var border = new SolidColorBrush(Color.FromRgb(90, 80, 70));
        var resources = new ResourceDictionary
        {
            ["FluentMaterialCardBrush"] = background,
            ["FluentMaterialLayerBorderBrush"] = border,
            ["FluentMaterialCardBorderThickness"] = new Thickness(2),
            ["FluentMaterialCardCornerRadius"] = new CornerRadius(10),
            ["FluentMaterialCardPadding"] = new Thickness(18)
        };

        var recipe = FWFluentMaterialSurfaceRecipe.Create(FWFluentMaterialRole.Card, resources);

        Assert.Same(background, recipe.Background);
        Assert.Same(border, recipe.BorderBrush);
        Assert.Equal(new Thickness(2), recipe.BorderThickness);
        Assert.Equal(new CornerRadius(10), recipe.CornerRadius);
        Assert.Equal(new Thickness(18), recipe.Padding);
    }

    [Theory]
    [InlineData(FWFluentMaterialKind.None)]
    [InlineData(FWFluentMaterialKind.Layer)]
    [InlineData(FWFluentMaterialKind.Mica)]
    [InlineData(FWFluentMaterialKind.MicaAlt)]
    [InlineData(FWFluentMaterialKind.Acrylic)]
    [InlineData(FWFluentMaterialKind.FrostedGlass)]
    [InlineData(FWFluentMaterialKind.LiquidGlass)]
    public void Recipe_ShouldExposeDefaultsForEveryMaterialKind(FWFluentMaterialKind materialKind)
    {
        var recipe = FWFluentMaterialRecipe.Create(materialKind);

        Assert.Equal(materialKind, recipe.MaterialKind);
        Assert.InRange(recipe.TintOpacity, 0, 1);
        Assert.InRange(recipe.NoiseIntensity, 0, 1);
        Assert.True(recipe.BlurRadius >= 0);
        Assert.True(recipe.RefractionAmount >= 0);
        Assert.True(recipe.ChromaticAberration >= 0);
        Assert.True(recipe.FusionRadius >= 0);
    }

    [Fact]
    public void Recipe_ShouldDefineWinUiStyleAcrylicAndMicaDefaults()
    {
        var mica = FWFluentMaterialRecipe.Create(FWFluentMaterialKind.Mica);
        var micaAlt = FWFluentMaterialRecipe.Create(FWFluentMaterialKind.MicaAlt);
        var acrylic = FWFluentMaterialRecipe.Create(FWFluentMaterialKind.Acrylic);

        Assert.Equal(0.18, mica.TintOpacity);
        Assert.Equal(18, mica.BlurRadius);
        Assert.Equal(0.26, micaAlt.TintOpacity);
        Assert.Equal(22, micaAlt.BlurRadius);
        Assert.Equal(0.46, acrylic.TintOpacity);
        Assert.Equal(28, acrylic.BlurRadius);
        Assert.Equal(0.035, acrylic.NoiseIntensity);
    }

    [Fact]
    public void Recipe_ShouldDefineLiquidGlassHlslDefaults()
    {
        var recipe = FWFluentMaterialRecipe.Create(FWFluentMaterialKind.LiquidGlass);

        Assert.True(recipe.IsInteractive);
        Assert.Equal(0.22, recipe.TintOpacity);
        Assert.Equal(14, recipe.BlurRadius);
        Assert.Equal(84, recipe.RefractionAmount);
        Assert.Equal(0.55, recipe.ChromaticAberration);
        Assert.Equal(24, recipe.FusionRadius);
    }

    [Fact]
    public void Recipe_ShouldReadFluentMaterialTokensFromResourceDictionary()
    {
        var resources = new ResourceDictionary
        {
            ["FluentMaterialAcrylicTintBrush"] = new SolidColorBrush(Color.FromArgb(222, 12, 34, 56)),
            ["FluentMaterialAcrylicTintOpacity"] = 0.61,
            ["FluentMaterialAcrylicBlurRadius"] = 31.0,
            ["FluentMaterialAcrylicNoiseIntensity"] = 0.052,
            ["FluentMaterialLiquidGlassTintBrush"] = new SolidColorBrush(Color.FromArgb(180, 70, 80, 90)),
            ["FluentMaterialLiquidGlassTintOpacity"] = 0.27,
            ["FluentMaterialLiquidGlassBlurRadius"] = 18.0,
            ["FluentMaterialLiquidGlassRefractionAmount"] = 96.0,
            ["FluentMaterialLiquidGlassChromaticAberration"] = 0.68,
            ["FluentMaterialLiquidGlassFusionRadius"] = 30.0
        };

        var acrylic = FWFluentMaterialRecipe.Create(FWFluentMaterialKind.Acrylic, resources);
        var liquidGlass = FWFluentMaterialRecipe.Create(FWFluentMaterialKind.LiquidGlass, resources);

        Assert.Equal(Color.FromArgb(222, 12, 34, 56), acrylic.TintColor);
        Assert.Equal(0.61, acrylic.TintOpacity);
        Assert.Equal(31.0, acrylic.BlurRadius);
        Assert.Equal(0.052, acrylic.NoiseIntensity);
        Assert.Equal(Color.FromArgb(180, 70, 80, 90), liquidGlass.TintColor);
        Assert.Equal(0.27, liquidGlass.TintOpacity);
        Assert.Equal(18.0, liquidGlass.BlurRadius);
        Assert.Equal(96.0, liquidGlass.RefractionAmount);
        Assert.Equal(0.68, liquidGlass.ChromaticAberration);
        Assert.Equal(30.0, liquidGlass.FusionRadius);
    }

    [Fact]
    public void Surface_ShouldApplyAcrylicRecipeToBackdropEffect()
    {
        var surface = new FWFluentMaterialSurface();
        var recipe = FWFluentMaterialRecipe.Create(FWFluentMaterialKind.Acrylic);

        surface.UseMaterialRecipe(recipe);

        var acrylic = Assert.IsType<AcrylicEffect>(surface.BackdropEffect);
        Assert.Equal(FWFluentMaterialKind.Acrylic, surface.MaterialKind);
        Assert.False(surface.LiquidGlass);
        Assert.Equal(recipe.TintColor, surface.TintColor);
        Assert.Equal(recipe.TintOpacity, surface.TintOpacity);
        Assert.Equal(recipe.BlurRadius, surface.BlurRadius);
        Assert.Equal(recipe.NoiseIntensity, surface.NoiseIntensity);
        Assert.Equal((float)recipe.TintOpacity, acrylic.TintOpacity);
        Assert.Equal((float)recipe.BlurRadius, acrylic.BlurRadius);
        Assert.Equal((float)recipe.NoiseIntensity, acrylic.NoiseIntensity);
    }

    [Fact]
    public void Surface_ShouldApplyLiquidGlassRecipeToHlslProperties()
    {
        var surface = new FWFluentMaterialSurface();
        var recipe = FWFluentMaterialRecipe.Create(FWFluentMaterialKind.LiquidGlass);

        surface.UseMaterialRecipe(recipe);

        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.True(surface.LiquidGlassInteractive);
        Assert.Null(surface.BackdropEffect);
        Assert.Equal(recipe.BlurRadius, surface.LiquidGlassBlurRadius);
        Assert.Equal(recipe.RefractionAmount, surface.LiquidGlassRefractionAmount);
        Assert.Equal(recipe.ChromaticAberration, surface.LiquidGlassChromaticAberration);
        Assert.Equal(recipe.FusionRadius, surface.LiquidGlassFusionRadius);
    }

    [Fact]
    public void Surface_ShouldApplyFluentRoleRecipeWithoutBreakingManualMaterialSelection()
    {
        var surface = new FWFluentMaterialSurface();

        surface.UseMaterialRole(FWFluentMaterialRole.FocusGlass);

        Assert.Equal(FWFluentMaterialRole.FocusGlass, surface.MaterialRole);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(12, surface.CornerRadius.TopLeft);

        surface.MaterialKind = FWFluentMaterialKind.Acrylic;

        Assert.Equal(FWFluentMaterialRole.FocusGlass, surface.MaterialRole);
        Assert.Equal(FWFluentMaterialKind.Acrylic, surface.MaterialKind);
        Assert.False(surface.LiquidGlass);
        Assert.IsType<AcrylicEffect>(surface.BackdropEffect);
    }

    [Fact]
    public void WindowSurface_ShouldApplySelectedBackdropToJaliumWindow()
    {
        var window = new Window();
        var surface = new FWFluentWindowSurface
        {
            WindowBackdropKind = FWFluentWindowBackdropKind.Acrylic
        };

        surface.ApplyWindowBackdrop(window);

        Assert.Equal(FWFluentMaterialRole.Window, surface.MaterialRole);
        Assert.Equal(FWFluentWindowBackdropKind.Acrylic, surface.WindowBackdropKind);
        Assert.Equal(WindowBackdropType.Acrylic, window.SystemBackdrop);
    }

    [Fact]
    public void WindowSurface_ShouldApplyProfileRecipeToBackdropAndMaterialSurface()
    {
        var window = new Window();
        var surface = new FWFluentWindowSurface();

        surface.ApplyWindowMaterialProfile(FWFluentWindowMaterialProfile.FocusGlassShell);
        surface.ApplyWindowBackdrop(window);

        Assert.Equal(FWFluentWindowMaterialProfile.FocusGlassShell, surface.WindowMaterialProfile);
        Assert.Equal(FWFluentWindowBackdropKind.MicaAlt, surface.WindowBackdropKind);
        Assert.Equal(FWFluentMaterialRole.FocusGlass, surface.MaterialRole);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(WindowBackdropType.MicaAlt, window.SystemBackdrop);
    }

    [Fact]
    public void WindowSurface_ShouldApplyDefaultProfileFromResourceDictionary()
    {
        var resources = new ResourceDictionary
        {
            ["FluentMaterialWindowDefaultProfile"] = FWFluentWindowMaterialProfile.TransientAcrylic
        };
        var surface = new FWFluentWindowSurface();

        surface.ApplyDefaultWindowMaterialProfile(resources);

        Assert.Equal(FWFluentWindowMaterialProfile.TransientAcrylic, surface.WindowMaterialProfile);
        Assert.Equal(FWFluentWindowBackdropKind.Acrylic, surface.WindowBackdropKind);
        Assert.Equal(FWFluentMaterialRole.Flyout, surface.MaterialRole);
        Assert.Equal(FWFluentMaterialKind.Acrylic, surface.MaterialKind);
        Assert.IsType<AcrylicEffect>(surface.BackdropEffect);
    }
}
