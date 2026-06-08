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
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.NotNull(repeater);
        Assert.Null(repeater.ItemsSource);
        Assert.Null(repeater.ItemTemplate);
        Assert.Null(repeater.Layout);
        Assert.Equal(0.0, repeater.HorizontalCacheLength);
        Assert.Equal(0.0, repeater.VerticalCacheLength);
        Assert.Null(repeater.Animator);
        Assert.Equal(FWItemsRepeaterRealizationMode.All, repeater.RealizationMode);
        Assert.Equal(0, repeater.ItemCount);
        Assert.Equal(0, repeater.RealizedElementCount);
        Assert.Equal(0, repeater.RecycledElementCount);
        Assert.Equal(-1, repeater.FirstRealizedIndex);
        Assert.Equal(-1, repeater.LastRealizedIndex);
        Assert.Equal(0, repeater.RequestedFirstRealizedIndex);
        Assert.Equal(0, repeater.RequestedRealizedItemCount);
        Assert.False(diagnostics.HasRealizedElements);
        Assert.False(diagnostics.HasRecycledElements);
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

    [Fact]
    public void ItemsSource_WithTemplate_ShouldRealizeAllItemsByDefault()
    {
        // Arrange
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = new[] { "Alpha", "Beta", "Gamma" },
            HorizontalCacheLength = 120,
            VerticalCacheLength = 240
        };

        // Act
        var element = repeater.TryGetElement(1);
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.NotNull(element);
        Assert.Equal(FWItemsRepeaterRealizationMode.All, diagnostics.RealizationMode);
        Assert.Equal(3, diagnostics.ItemCount);
        Assert.Equal(3, diagnostics.RealizedElementCount);
        Assert.Equal(0, diagnostics.RecycledElementCount);
        Assert.Equal(0, diagnostics.FirstRealizedIndex);
        Assert.Equal(2, diagnostics.LastRealizedIndex);
        Assert.Equal(0, diagnostics.RequestedFirstRealizedIndex);
        Assert.Equal(3, diagnostics.RequestedRealizedItemCount);
        Assert.Equal(120, diagnostics.HorizontalCacheLength);
        Assert.Equal(240, diagnostics.VerticalCacheLength);
        Assert.Equal(3, diagnostics.LastCreatedElementCount);
        Assert.Equal(0, diagnostics.LastReusedElementCount);
        Assert.True(diagnostics.HasRealizedElements);
        Assert.False(diagnostics.HasRecycledElements);
        Assert.Equal(1, repeater.GetElementIndex(element!));
    }

    [Fact]
    public void RealizeRange_ShouldOnlyRealizeRequestedItemWindow()
    {
        // Arrange
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = new[] { "A", "B", "C", "D", "E", "F" }
        };

        // Act
        repeater.RealizeRange(2, 3);
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.Equal(FWItemsRepeaterRealizationMode.Range, diagnostics.RealizationMode);
        Assert.Equal(6, diagnostics.ItemCount);
        Assert.Equal(3, diagnostics.RealizedElementCount);
        Assert.Equal(3, diagnostics.RecycledElementCount);
        Assert.Equal(2, diagnostics.FirstRealizedIndex);
        Assert.Equal(4, diagnostics.LastRealizedIndex);
        Assert.Equal(2, diagnostics.RequestedFirstRealizedIndex);
        Assert.Equal(3, diagnostics.RequestedRealizedItemCount);
        Assert.Equal(0, diagnostics.LastCreatedElementCount);
        Assert.Equal(3, diagnostics.LastReusedElementCount);
        Assert.Null(repeater.TryGetElement(1));
        Assert.NotNull(repeater.TryGetElement(2));
        Assert.NotNull(repeater.TryGetElement(4));
        Assert.Null(repeater.TryGetElement(5));
    }

    [Fact]
    public void RealizeRange_WhenRangeMoves_ShouldReuseElements()
    {
        // Arrange
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = new[] { "A", "B", "C", "D", "E", "F" }
        };
        repeater.RealizeRange(2, 3);
        var firstWindowElement = repeater.TryGetElement(2);

        // Act
        repeater.RealizeRange(3, 2);
        var movedDiagnostics = repeater.GetDiagnostics();
        repeater.ResetRealizationWindow();
        var resetDiagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.NotNull(firstWindowElement);
        Assert.Equal(FWItemsRepeaterRealizationMode.Range, movedDiagnostics.RealizationMode);
        Assert.Equal(2, movedDiagnostics.RealizedElementCount);
        Assert.Equal(4, movedDiagnostics.RecycledElementCount);
        Assert.Equal(3, movedDiagnostics.FirstRealizedIndex);
        Assert.Equal(4, movedDiagnostics.LastRealizedIndex);
        Assert.Equal(0, movedDiagnostics.LastCreatedElementCount);
        Assert.Equal(2, movedDiagnostics.LastReusedElementCount);
        Assert.Equal(FWItemsRepeaterRealizationMode.All, resetDiagnostics.RealizationMode);
        Assert.Equal(6, resetDiagnostics.RealizedElementCount);
        Assert.Equal(0, resetDiagnostics.RecycledElementCount);
        Assert.Equal(0, resetDiagnostics.FirstRealizedIndex);
        Assert.Equal(5, resetDiagnostics.LastRealizedIndex);
        Assert.Equal(0, resetDiagnostics.LastCreatedElementCount);
        Assert.Equal(6, resetDiagnostics.LastReusedElementCount);
    }

    [Fact]
    public void RealizeRange_WhenOutsideItems_ShouldRealizeNothing()
    {
        // Arrange
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = new[] { "A", "B", "C" }
        };

        // Act
        repeater.RealizeRange(99, 5);
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.Equal(FWItemsRepeaterRealizationMode.Range, diagnostics.RealizationMode);
        Assert.Equal(3, diagnostics.ItemCount);
        Assert.Equal(0, diagnostics.RealizedElementCount);
        Assert.Equal(3, diagnostics.RecycledElementCount);
        Assert.Equal(-1, diagnostics.FirstRealizedIndex);
        Assert.Equal(-1, diagnostics.LastRealizedIndex);
        Assert.Equal(99, diagnostics.RequestedFirstRealizedIndex);
        Assert.Equal(5, diagnostics.RequestedRealizedItemCount);
        Assert.Equal(0, diagnostics.LastCreatedElementCount);
        Assert.Equal(0, diagnostics.LastReusedElementCount);
        Assert.False(diagnostics.HasRealizedElements);
        Assert.True(diagnostics.HasRecycledElements);
    }

    [Fact]
    public void RealizeRange_WithNegativeArguments_ShouldThrow()
    {
        // Arrange
        var repeater = new FWItemsRepeater();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => repeater.RealizeRange(-1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => repeater.RealizeRange(0, -1));
    }

    [Fact]
    public void CollectionChanges_ShouldRefreshDiagnosticsAndReuseElements()
    {
        // Arrange
        var items = new System.Collections.ObjectModel.ObservableCollection<string> { "A", "B", "C" };
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = items
        };
        repeater.RealizeRange(0, 2);

        // Act
        items.Add("D");
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.Equal(FWItemsRepeaterRealizationMode.Range, diagnostics.RealizationMode);
        Assert.Equal(4, diagnostics.ItemCount);
        Assert.Equal(2, diagnostics.RealizedElementCount);
        Assert.Equal(1, diagnostics.RecycledElementCount);
        Assert.Equal(0, diagnostics.LastCreatedElementCount);
        Assert.Equal(2, diagnostics.LastReusedElementCount);
    }

    private static DataTemplate CreateTextTemplate()
    {
        var template = new DataTemplate();
        template.SetVisualTree(() => new TextBlock());
        template.Seal();
        return template;
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
