using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests.Controls;

public class FWAnimatedIconTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var icon = new FWAnimatedIcon();

        // Assert
        Assert.NotNull(icon);
        Assert.Equal(20, icon.Width);
        Assert.Equal(20, icon.Height);
        Assert.True(icon.AutoPlay);
        Assert.Equal(string.Empty, icon.State);
        Assert.Null(icon.Source);
        Assert.Null(icon.FallbackIconSource);
        Assert.False(icon.MirroredWhenRightToLeft);
    }

    [Fact]
    public void AutoPlay_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var icon = new FWAnimatedIcon();

        // Act
        icon.AutoPlay = false;

        // Assert
        Assert.False(icon.AutoPlay);
    }

    [Fact]
    public void State_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var icon = new FWAnimatedIcon();
        const string testState = "Hover";

        // Act
        icon.State = testState;

        // Assert
        Assert.Equal(testState, icon.State);
    }

    [Fact]
    public void MirroredWhenRightToLeft_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var icon = new FWAnimatedIcon();

        // Act
        icon.MirroredWhenRightToLeft = true;

        // Assert
        Assert.True(icon.MirroredWhenRightToLeft);
    }

    [Fact]
    public void FallbackIconSource_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var icon = new FWAnimatedIcon();
        var fallback = ""; // Example glyph

        // Act
        icon.FallbackIconSource = fallback;

        // Assert
        Assert.Equal(fallback, icon.FallbackIconSource);
    }

    [Fact]
    public void FallbackContent_ShouldUseFluentCompatibleFontForPrivateUseGlyphs()
    {
        var glyph = FluentIconRegular.Share24.GetString();
        var icon = new FWAnimatedIcon
        {
            FallbackIconSource = glyph
        };

        var fallback = Assert.IsType<TextBlock>(CreateFallbackContent(icon));

        Assert.Equal(glyph, fallback.Text);
        Assert.Equal(FluentIconFonts.Regular, fallback.FontFamily);
    }

    [Fact]
    public void FallbackContent_ShouldUseTextFontForPlainUnicodeGlyphs()
    {
        var icon = new FWAnimatedIcon
        {
            FallbackIconSource = ">"
        };

        var fallback = Assert.IsType<TextBlock>(CreateFallbackContent(icon));

        Assert.Equal(">", fallback.Text);
        Assert.Equal(FrameworkElement.DefaultFontFamilyName, fallback.FontFamily);
    }

    [Fact]
    public void FallbackContent_ShouldCreateFluentIconForFluentIconEnums()
    {
        var icon = new FWAnimatedIcon
        {
            FallbackIconSource = FluentIconRegular.Save24
        };

        var fallback = Assert.IsType<FluentIcon>(CreateFallbackContent(icon));

        Assert.Equal(FluentIconRegular.Save24.GetString(), fallback.Glyph);
        Assert.Equal(FluentIconFonts.Regular, fallback.FontFamily?.ToString());
        Assert.Equal(16, fallback.Size);
    }

    [Fact]
    public void Play_ShouldInvokeWithoutException()
    {
        // Arrange
        var icon = new FWAnimatedIcon();

        // Act & Assert
        var exception = Record.Exception(() => icon.Play());
        Assert.Null(exception);
    }

    [Fact]
    public void Stop_ShouldInvokeWithoutException()
    {
        // Arrange
        var icon = new FWAnimatedIcon();

        // Act & Assert
        var exception = Record.Exception(() => icon.Stop());
        Assert.Null(exception);
    }

    [Fact]
    public void ImplementsIFluentJaliumControl()
    {
        // Arrange
        var icon = new FWAnimatedIcon();

        // Assert
        Assert.IsAssignableFrom<IFluentJaliumControl>(icon);
    }

    private static FrameworkElement? CreateFallbackContent(FWAnimatedIcon icon)
    {
        var method = typeof(FWAnimatedIcon).GetMethod("CreateFallbackContent", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return (FrameworkElement?)method.Invoke(icon, null);
    }
}

public class FWAnimatedVisualPlayerTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var player = new FWAnimatedVisualPlayer();

        // Assert
        Assert.NotNull(player);
        Assert.Equal(100, player.Width);
        Assert.Equal(100, player.Height);
        Assert.True(player.AutoPlay);
        Assert.False(player.IsPlaying);
        Assert.Equal(1.0, player.PlaybackRate);
        Assert.Equal(Stretch.Uniform, player.Stretch);
        Assert.True(player.IsLooping);
        Assert.Equal(TimeSpan.Zero, player.Duration);
        Assert.False(player.IsAnimatedVisualLoaded);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(5.0)]
    public void PlaybackRate_WhenSetToValidValue_ShouldUpdateProperty(double rate)
    {
        // Arrange
        var player = new FWAnimatedVisualPlayer();

        // Act
        player.PlaybackRate = rate;

        // Assert
        Assert.Equal(rate, player.PlaybackRate);
    }

    [Fact]
    public void IsLooping_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var player = new FWAnimatedVisualPlayer();

        // Act
        player.IsLooping = false;

        // Assert
        Assert.False(player.IsLooping);
    }

    [Theory]
    [InlineData(Stretch.None)]
    [InlineData(Stretch.Fill)]
    [InlineData(Stretch.Uniform)]
    [InlineData(Stretch.UniformToFill)]
    public void Stretch_WhenSet_ShouldUpdateProperty(Stretch stretch)
    {
        // Arrange
        var player = new FWAnimatedVisualPlayer();

        // Act
        player.Stretch = stretch;

        // Assert
        Assert.Equal(stretch, player.Stretch);
    }

    [Fact]
    public void Play_ShouldInvokeWithoutException()
    {
        // Arrange
        var player = new FWAnimatedVisualPlayer();

        // Act & Assert
        var exception = Record.Exception(() => player.Play());
        Assert.Null(exception);
    }

    [Fact]
    public void Pause_ShouldInvokeWithoutException()
    {
        // Arrange
        var player = new FWAnimatedVisualPlayer();

        // Act & Assert
        var exception = Record.Exception(() => player.Pause());
        Assert.Null(exception);
    }

    [Fact]
    public void Stop_ShouldInvokeWithoutException()
    {
        // Arrange
        var player = new FWAnimatedVisualPlayer();

        // Act & Assert
        var exception = Record.Exception(() => player.Stop());
        Assert.Null(exception);
    }

    [Fact]
    public void Resume_ShouldInvokeWithoutException()
    {
        // Arrange
        var player = new FWAnimatedVisualPlayer();

        // Act & Assert
        var exception = Record.Exception(() => player.Resume());
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void SetProgress_WithValidValue_ShouldInvokeWithoutException(double progress)
    {
        // Arrange
        var player = new FWAnimatedVisualPlayer();

        // Act & Assert
        var exception = Record.Exception(() => player.SetProgress(progress));
        Assert.Null(exception);
    }

    [Fact]
    public void ImplementsIFluentJaliumControl()
    {
        // Arrange
        var player = new FWAnimatedVisualPlayer();

        // Assert
        Assert.IsAssignableFrom<IFluentJaliumControl>(player);
    }
}
