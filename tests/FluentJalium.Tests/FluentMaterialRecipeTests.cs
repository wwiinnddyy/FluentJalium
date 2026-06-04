using FluentJalium.Controls;
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
}
