using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Media;

namespace FluentJalium.Tests.Controls;

public class FWBackdropTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var backdrop = new FWBackdrop();

        // Assert
        Assert.NotNull(backdrop);
        Assert.Equal(FWBackdropType.None, backdrop.Type);
        Assert.Equal(Colors.Transparent, backdrop.TintColor);
        Assert.Equal(0.8, backdrop.TintOpacity);
        Assert.Equal(0.85, backdrop.LuminosityOpacity);
        Assert.False(backdrop.AlwaysUseFallback);
        Assert.False(backdrop.IsHitTestVisible);
    }

    [Theory]
    [InlineData(FWBackdropType.None)]
    [InlineData(FWBackdropType.Acrylic)]
    [InlineData(FWBackdropType.Mica)]
    [InlineData(FWBackdropType.MicaAlt)]
    [InlineData(FWBackdropType.Tabbed)]
    public void Type_WhenSet_ShouldUpdateProperty(FWBackdropType type)
    {
        // Arrange
        var backdrop = new FWBackdrop();

        // Act
        backdrop.Type = type;

        // Assert
        Assert.Equal(type, backdrop.Type);
    }

    [Fact]
    public void TintColor_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var backdrop = new FWBackdrop();
        var color = Color.FromRgb(0xFF, 0x00, 0x00);

        // Act
        backdrop.TintColor = color;

        // Assert
        Assert.Equal(color, backdrop.TintColor);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(0.8)]
    [InlineData(1.0)]
    public void TintOpacity_WhenSetToValidValue_ShouldUpdateProperty(double opacity)
    {
        // Arrange
        var backdrop = new FWBackdrop();

        // Act
        backdrop.TintOpacity = opacity;

        // Assert
        Assert.Equal(opacity, backdrop.TintOpacity);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(0.85)]
    [InlineData(1.0)]
    public void LuminosityOpacity_WhenSetToValidValue_ShouldUpdateProperty(double opacity)
    {
        // Arrange
        var backdrop = new FWBackdrop();

        // Act
        backdrop.LuminosityOpacity = opacity;

        // Assert
        Assert.Equal(opacity, backdrop.LuminosityOpacity);
    }

    [Fact]
    public void FallbackColor_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var backdrop = new FWBackdrop();
        var color = Color.FromRgb(0xF3, 0xF3, 0xF3);

        // Act
        backdrop.FallbackColor = color;

        // Assert
        Assert.Equal(color, backdrop.FallbackColor);
    }

    [Fact]
    public void AlwaysUseFallback_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var backdrop = new FWBackdrop();

        // Act
        backdrop.AlwaysUseFallback = true;

        // Assert
        Assert.True(backdrop.AlwaysUseFallback);
    }

    [Fact]
    public void ImplementsIFluentJaliumControl()
    {
        // Arrange
        var backdrop = new FWBackdrop();

        // Assert
        Assert.IsAssignableFrom<IFluentJaliumControl>(backdrop);
    }
}

public class FWAcrylicBrushTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var brush = new FWAcrylicBrush();

        // Assert
        Assert.NotNull(brush);
        Assert.Equal(Color.FromRgb(0xF3, 0xF3, 0xF3), brush.TintColor);
        Assert.Equal(0.8, brush.TintOpacity);
        Assert.Null(brush.TintLuminosityOpacity);
        Assert.Equal(AcrylicBackgroundSource.Backdrop, brush.BackgroundSource);
        Assert.Equal(Color.FromRgb(0xF3, 0xF3, 0xF3), brush.FallbackColor);
    }

    [Fact]
    public void TintColor_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var brush = new FWAcrylicBrush();
        var color = Color.FromRgb(0x00, 0x78, 0xD4);

        // Act
        brush.TintColor = color;

        // Assert
        Assert.Equal(color, brush.TintColor);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(0.8)]
    [InlineData(1.0)]
    public void TintOpacity_WhenSet_ShouldUpdateProperty(double opacity)
    {
        // Arrange
        var brush = new FWAcrylicBrush();

        // Act
        brush.TintOpacity = opacity;

        // Assert
        Assert.Equal(opacity, brush.TintOpacity);
    }

    [Fact]
    public void TintLuminosityOpacity_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var brush = new FWAcrylicBrush();
        const double opacity = 0.7;

        // Act
        brush.TintLuminosityOpacity = opacity;

        // Assert
        Assert.Equal(opacity, brush.TintLuminosityOpacity);
    }

    [Theory]
    [InlineData(AcrylicBackgroundSource.Backdrop)]
    [InlineData(AcrylicBackgroundSource.HostBackdrop)]
    public void BackgroundSource_WhenSet_ShouldUpdateProperty(AcrylicBackgroundSource source)
    {
        // Arrange
        var brush = new FWAcrylicBrush();

        // Act
        brush.BackgroundSource = source;

        // Assert
        Assert.Equal(source, brush.BackgroundSource);
    }

    [Fact]
    public void FallbackColor_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var brush = new FWAcrylicBrush();
        var color = Color.FromRgb(0xFF, 0xFF, 0xFF);

        // Act
        brush.FallbackColor = color;

        // Assert
        Assert.Equal(color, brush.FallbackColor);
    }
}
