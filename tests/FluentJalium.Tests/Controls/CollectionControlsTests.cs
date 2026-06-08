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
        Assert.Equal(FWItemsRepeaterRealizationSource.All, repeater.RealizationSource);
        Assert.Equal(0, repeater.ItemCount);
        Assert.Equal(0, repeater.RealizedElementCount);
        Assert.Equal(0, repeater.RecycledElementCount);
        Assert.Equal(-1, repeater.FirstRealizedIndex);
        Assert.Equal(-1, repeater.LastRealizedIndex);
        Assert.Equal(0, repeater.RequestedFirstRealizedIndex);
        Assert.Equal(0, repeater.RequestedRealizedItemCount);
        Assert.Equal(0, repeater.ViewportStart);
        Assert.Equal(0, repeater.ViewportLength);
        Assert.Equal(Orientation.Vertical, repeater.ViewportOrientation);
        Assert.Equal(0, repeater.EstimatedItemExtent);
        Assert.Equal(Orientation.Vertical, diagnostics.ViewportOrientation);
        Assert.Equal(0, diagnostics.ActiveCacheLength);
        Assert.False(repeater.IsViewportAttached);
        Assert.False(diagnostics.IsViewportAttached);
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
        Assert.Equal(FWItemsRepeaterRealizationSource.All, diagnostics.RealizationSource);
        Assert.Equal(3, diagnostics.ItemCount);
        Assert.Equal(3, diagnostics.RealizedElementCount);
        Assert.Equal(0, diagnostics.RecycledElementCount);
        Assert.Equal(0, diagnostics.FirstRealizedIndex);
        Assert.Equal(2, diagnostics.LastRealizedIndex);
        Assert.Equal(0, diagnostics.RequestedFirstRealizedIndex);
        Assert.Equal(3, diagnostics.RequestedRealizedItemCount);
        Assert.Equal(0, diagnostics.ViewportStart);
        Assert.Equal(0, diagnostics.ViewportLength);
        Assert.Equal(Orientation.Vertical, diagnostics.ViewportOrientation);
        Assert.Equal(0, diagnostics.EstimatedItemExtent);
        Assert.Equal(240, diagnostics.ActiveCacheLength);
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
        Assert.Equal(FWItemsRepeaterRealizationSource.Manual, diagnostics.RealizationSource);
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
        Assert.Equal(FWItemsRepeaterRealizationSource.Manual, movedDiagnostics.RealizationSource);
        Assert.Equal(2, movedDiagnostics.RealizedElementCount);
        Assert.Equal(4, movedDiagnostics.RecycledElementCount);
        Assert.Equal(3, movedDiagnostics.FirstRealizedIndex);
        Assert.Equal(4, movedDiagnostics.LastRealizedIndex);
        Assert.Equal(0, movedDiagnostics.LastCreatedElementCount);
        Assert.Equal(2, movedDiagnostics.LastReusedElementCount);
        Assert.Equal(FWItemsRepeaterRealizationMode.All, resetDiagnostics.RealizationMode);
        Assert.Equal(FWItemsRepeaterRealizationSource.All, resetDiagnostics.RealizationSource);
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
        Assert.Equal(FWItemsRepeaterRealizationSource.Manual, diagnostics.RealizationSource);
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
        Assert.Equal(FWItemsRepeaterRealizationSource.Manual, diagnostics.RealizationSource);
        Assert.Equal(4, diagnostics.ItemCount);
        Assert.Equal(2, diagnostics.RealizedElementCount);
        Assert.Equal(1, diagnostics.RecycledElementCount);
        Assert.Equal(0, diagnostics.LastCreatedElementCount);
        Assert.Equal(2, diagnostics.LastReusedElementCount);
    }

    [Fact]
    public void ApplyViewport_ShouldRealizeViewportWindowWithCache()
    {
        // Arrange
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = Enumerable.Range(0, 20).Select(index => $"Item {index}").ToArray(),
            EstimatedItemExtent = 20,
            VerticalCacheLength = 20
        };

        // Act
        repeater.ApplyViewport(50, 60);
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.Equal(FWItemsRepeaterRealizationMode.Range, diagnostics.RealizationMode);
        Assert.Equal(FWItemsRepeaterRealizationSource.Viewport, diagnostics.RealizationSource);
        Assert.Equal(1, diagnostics.FirstRealizedIndex);
        Assert.Equal(6, diagnostics.LastRealizedIndex);
        Assert.Equal(1, diagnostics.RequestedFirstRealizedIndex);
        Assert.Equal(6, diagnostics.RequestedRealizedItemCount);
        Assert.Equal(50, diagnostics.ViewportStart);
        Assert.Equal(60, diagnostics.ViewportLength);
        Assert.Equal(Orientation.Vertical, diagnostics.ViewportOrientation);
        Assert.Equal(20, diagnostics.EstimatedItemExtent);
        Assert.Equal(20, diagnostics.ActiveCacheLength);
        Assert.Equal(20, diagnostics.VerticalCacheLength);
        Assert.Null(repeater.TryGetElement(0));
        Assert.NotNull(repeater.TryGetElement(1));
        Assert.NotNull(repeater.TryGetElement(6));
        Assert.Null(repeater.TryGetElement(7));
    }

    [Fact]
    public void ApplyViewport_WhenCacheLengthChanges_ShouldRecalculateViewportWindow()
    {
        // Arrange
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = Enumerable.Range(0, 20).Select(index => $"Item {index}").ToArray(),
            EstimatedItemExtent = 10,
            VerticalCacheLength = 0
        };
        repeater.ApplyViewport(30, 20);

        // Act
        repeater.VerticalCacheLength = 10;
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.Equal(FWItemsRepeaterRealizationSource.Viewport, diagnostics.RealizationSource);
        Assert.Equal(2, diagnostics.FirstRealizedIndex);
        Assert.Equal(5, diagnostics.LastRealizedIndex);
        Assert.Equal(2, diagnostics.RequestedFirstRealizedIndex);
        Assert.Equal(4, diagnostics.RequestedRealizedItemCount);
        Assert.Equal(10, diagnostics.ActiveCacheLength);
    }

    [Fact]
    public void ApplyViewport_WithoutEstimatedItemExtent_ShouldRealizeAllItems()
    {
        // Arrange
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = new[] { "A", "B", "C" }
        };

        // Act
        repeater.ApplyViewport(40, 120);
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.Equal(FWItemsRepeaterRealizationMode.All, diagnostics.RealizationMode);
        Assert.Equal(FWItemsRepeaterRealizationSource.All, diagnostics.RealizationSource);
        Assert.Equal(3, diagnostics.RealizedElementCount);
        Assert.Equal(0, diagnostics.FirstRealizedIndex);
        Assert.Equal(2, diagnostics.LastRealizedIndex);
        Assert.Equal(40, diagnostics.ViewportStart);
        Assert.Equal(120, diagnostics.ViewportLength);
        Assert.Equal(Orientation.Vertical, diagnostics.ViewportOrientation);
    }

    [Fact]
    public void ApplyViewport_WithInvalidMetrics_ShouldThrow()
    {
        // Arrange
        var repeater = new FWItemsRepeater();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => repeater.ApplyViewport(-1, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => repeater.ApplyViewport(0, double.NaN));
    }

    [Fact]
    public void ApplyViewport_WithScrollViewer_ShouldUseVerticalViewportMetrics()
    {
        // Arrange
        var scrollViewer = CreateScrollViewer(viewportHeight: 30, contentHeight: 200);
        scrollViewer.ScrollToVerticalOffset(20);

        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = Enumerable.Range(0, 10).Select(index => $"Item {index}").ToArray(),
            EstimatedItemExtent = 10
        };

        // Act
        repeater.ApplyViewport(scrollViewer);
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.Equal(FWItemsRepeaterRealizationSource.Viewport, diagnostics.RealizationSource);
        Assert.Equal(scrollViewer.VerticalOffset, diagnostics.ViewportStart);
        Assert.Equal(scrollViewer.ViewportHeight, diagnostics.ViewportLength);
        Assert.Equal(Orientation.Vertical, diagnostics.ViewportOrientation);
    }

    [Fact]
    public void AttachViewport_WhenScrollViewerScrolls_ShouldRefreshRealizationWindow()
    {
        // Arrange
        var scrollViewer = CreateScrollViewer(viewportHeight: 30, contentHeight: 200);
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = Enumerable.Range(0, 20).Select(index => $"Item {index}").ToArray(),
            EstimatedItemExtent = 10
        };

        // Act
        repeater.AttachViewport(scrollViewer);
        var attachedDiagnostics = repeater.GetDiagnostics();
        scrollViewer.ScrollToVerticalOffset(40);
        var scrolledDiagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.True(repeater.IsViewportAttached);
        Assert.True(attachedDiagnostics.IsViewportAttached);
        Assert.Equal(FWItemsRepeaterRealizationSource.Viewport, attachedDiagnostics.RealizationSource);
        Assert.Equal(0, attachedDiagnostics.FirstRealizedIndex);
        Assert.Equal(2, attachedDiagnostics.LastRealizedIndex);
        Assert.Equal(40, scrolledDiagnostics.ViewportStart);
        Assert.Equal(30, scrolledDiagnostics.ViewportLength);
        Assert.Equal(4, scrolledDiagnostics.FirstRealizedIndex);
        Assert.Equal(6, scrolledDiagnostics.LastRealizedIndex);
        Assert.True(scrolledDiagnostics.IsViewportAttached);
    }

    [Fact]
    public void DetachViewport_WhenScrollViewerScrolls_ShouldLeaveCurrentRealizationWindow()
    {
        // Arrange
        var scrollViewer = CreateScrollViewer(viewportHeight: 30, contentHeight: 200);
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = Enumerable.Range(0, 20).Select(index => $"Item {index}").ToArray(),
            EstimatedItemExtent = 10
        };
        repeater.AttachViewport(scrollViewer);
        scrollViewer.ScrollToVerticalOffset(40);

        // Act
        repeater.DetachViewport();
        scrollViewer.ScrollToVerticalOffset(80);
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.False(repeater.IsViewportAttached);
        Assert.False(diagnostics.IsViewportAttached);
        Assert.Equal(FWItemsRepeaterRealizationSource.Viewport, diagnostics.RealizationSource);
        Assert.Equal(40, diagnostics.ViewportStart);
        Assert.Equal(4, diagnostics.FirstRealizedIndex);
        Assert.Equal(6, diagnostics.LastRealizedIndex);
    }

    [Fact]
    public void AttachViewport_WithHorizontalOrientation_ShouldRefreshFromHorizontalScroll()
    {
        // Arrange
        var scrollViewer = CreateScrollViewer(viewportWidth: 50, viewportHeight: 30, contentWidth: 240, contentHeight: 30);
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = Enumerable.Range(0, 20).Select(index => $"Item {index}").ToArray(),
            EstimatedItemExtent = 25,
            HorizontalCacheLength = 25
        };

        // Act
        repeater.AttachViewport(scrollViewer, Orientation.Horizontal);
        scrollViewer.ScrollToHorizontalOffset(75);
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.True(diagnostics.IsViewportAttached);
        Assert.Equal(Orientation.Horizontal, diagnostics.ViewportOrientation);
        Assert.Equal(75, diagnostics.ViewportStart);
        Assert.Equal(50, diagnostics.ViewportLength);
        Assert.Equal(2, diagnostics.FirstRealizedIndex);
        Assert.Equal(5, diagnostics.LastRealizedIndex);
    }

    [Fact]
    public void AttachViewport_WithNullScrollViewer_ShouldThrow()
    {
        // Arrange
        var repeater = new FWItemsRepeater();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => repeater.AttachViewport(null!));
    }

    [Fact]
    public void ApplyViewport_WithHorizontalOrientation_ShouldUseHorizontalCacheLength()
    {
        // Arrange
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = Enumerable.Range(0, 20).Select(index => $"Item {index}").ToArray(),
            EstimatedItemExtent = 25,
            HorizontalCacheLength = 50,
            VerticalCacheLength = 0
        };

        // Act
        repeater.ApplyViewport(100, 50, Orientation.Horizontal);
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.Equal(FWItemsRepeaterRealizationSource.Viewport, diagnostics.RealizationSource);
        Assert.Equal(Orientation.Horizontal, diagnostics.ViewportOrientation);
        Assert.Equal(50, diagnostics.ActiveCacheLength);
        Assert.Equal(2, diagnostics.FirstRealizedIndex);
        Assert.Equal(7, diagnostics.LastRealizedIndex);
        Assert.Equal(2, diagnostics.RequestedFirstRealizedIndex);
        Assert.Equal(6, diagnostics.RequestedRealizedItemCount);
    }

    private static DataTemplate CreateTextTemplate()
    {
        var template = new DataTemplate();
        template.SetVisualTree(() => new TextBlock());
        template.Seal();
        return template;
    }

    private static ScrollViewer CreateScrollViewer(
        double viewportWidth = 100,
        double viewportHeight = 100,
        double contentWidth = 100,
        double contentHeight = 100)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = new Border
            {
                Width = contentWidth,
                Height = contentHeight
            }
        };
        var viewportSize = new Size(viewportWidth, viewportHeight);
        scrollViewer.Measure(viewportSize);
        scrollViewer.Arrange(new Rect(0, 0, viewportWidth, viewportHeight));
        return scrollViewer;
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
