using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Tests.Controls;

public class FWItemsRepeaterTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var repeater = new FWItemsRepeater();

        // Assert
        Assert.NotNull(repeater);
        Assert.Null(repeater.ItemsSource);
        Assert.Null(repeater.ItemTemplate);
        Assert.Null(repeater.Layout);
        Assert.Equal(0.0, repeater.HorizontalCacheLength);
        Assert.Equal(0.0, repeater.VerticalCacheLength);
        Assert.Null(repeater.Animator);
    }

    [Fact]
    public void ItemsSource_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var repeater = new FWItemsRepeater();
        var items = new List<string> { "Item1", "Item2", "Item3" };

        // Act
        repeater.ItemsSource = items;

        // Assert
        Assert.Equal(items, repeater.ItemsSource);
    }

    [Fact]
    public void HorizontalCacheLength_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var repeater = new FWItemsRepeater();
        const double cacheLength = 200.0;

        // Act
        repeater.HorizontalCacheLength = cacheLength;

        // Assert
        Assert.Equal(cacheLength, repeater.HorizontalCacheLength);
    }

    [Fact]
    public void VerticalCacheLength_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var repeater = new FWItemsRepeater();
        const double cacheLength = 200.0;

        // Act
        repeater.VerticalCacheLength = cacheLength;

        // Assert
        Assert.Equal(cacheLength, repeater.VerticalCacheLength);
    }

    [Fact]
    public void TryGetElement_WithInvalidIndex_ShouldReturnNull()
    {
        // Arrange
        var repeater = new FWItemsRepeater();

        // Act
        var element = repeater.TryGetElement(0);

        // Assert
        Assert.Null(element);
    }

    [Fact]
    public void GetElementIndex_WithNonExistentElement_ShouldReturnMinusOne()
    {
        // Arrange
        var repeater = new FWItemsRepeater();
        var element = new TextBlock();

        // Act
        var index = repeater.GetElementIndex(element);

        // Assert
        Assert.Equal(-1, index);
    }

    [Fact]
    public void ImplementsIFluentJaliumControl()
    {
        // Arrange
        var repeater = new FWItemsRepeater();

        // Assert
        Assert.IsAssignableFrom<IFluentJaliumControl>(repeater);
    }
}

public class StackLayoutTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var layout = new StackLayout();

        // Assert
        Assert.NotNull(layout);
        Assert.Equal(Orientation.Vertical, layout.Orientation);
        Assert.Equal(0.0, layout.Spacing);
    }

    [Theory]
    [InlineData(Orientation.Vertical)]
    [InlineData(Orientation.Horizontal)]
    public void Orientation_WhenSet_ShouldUpdateProperty(Orientation orientation)
    {
        // Arrange
        var layout = new StackLayout();

        // Act
        layout.Orientation = orientation;

        // Assert
        Assert.Equal(orientation, layout.Orientation);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(5.0)]
    [InlineData(10.0)]
    [InlineData(20.0)]
    public void Spacing_WhenSet_ShouldUpdateProperty(double spacing)
    {
        // Arrange
        var layout = new StackLayout();

        // Act
        layout.Spacing = spacing;

        // Assert
        Assert.Equal(spacing, layout.Spacing);
    }
}

public class UniformGridLayoutTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var layout = new UniformGridLayout();

        // Assert
        Assert.NotNull(layout);
        Assert.Equal(Orientation.Vertical, layout.Orientation);
        Assert.Equal(100.0, layout.MinItemWidth);
        Assert.Equal(100.0, layout.MinItemHeight);
        Assert.Equal(0.0, layout.MinColumnSpacing);
        Assert.Equal(0.0, layout.MinRowSpacing);
        Assert.Equal(-1, layout.MaximumRowsOrColumns);
    }

    [Theory]
    [InlineData(50.0)]
    [InlineData(100.0)]
    [InlineData(200.0)]
    public void MinItemWidth_WhenSet_ShouldUpdateProperty(double width)
    {
        // Arrange
        var layout = new UniformGridLayout();

        // Act
        layout.MinItemWidth = width;

        // Assert
        Assert.Equal(width, layout.MinItemWidth);
    }

    [Theory]
    [InlineData(50.0)]
    [InlineData(100.0)]
    [InlineData(200.0)]
    public void MinItemHeight_WhenSet_ShouldUpdateProperty(double height)
    {
        // Arrange
        var layout = new UniformGridLayout();

        // Act
        layout.MinItemHeight = height;

        // Assert
        Assert.Equal(height, layout.MinItemHeight);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void MaximumRowsOrColumns_WhenSet_ShouldUpdateProperty(int max)
    {
        // Arrange
        var layout = new UniformGridLayout();

        // Act
        layout.MaximumRowsOrColumns = max;

        // Assert
        Assert.Equal(max, layout.MaximumRowsOrColumns);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(5.0)]
    [InlineData(10.0)]
    public void MinColumnSpacing_WhenSet_ShouldUpdateProperty(double spacing)
    {
        // Arrange
        var layout = new UniformGridLayout();

        // Act
        layout.MinColumnSpacing = spacing;

        // Assert
        Assert.Equal(spacing, layout.MinColumnSpacing);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(5.0)]
    [InlineData(10.0)]
    public void MinRowSpacing_WhenSet_ShouldUpdateProperty(double spacing)
    {
        // Arrange
        var layout = new UniformGridLayout();

        // Act
        layout.MinRowSpacing = spacing;

        // Assert
        Assert.Equal(spacing, layout.MinRowSpacing);
    }
}
