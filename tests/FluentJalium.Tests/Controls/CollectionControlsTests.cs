using FluentJalium.Controls;
using FluentJalium.Gallery.Pages;
using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Tests.Controls;

public class CollectionControlsTests
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
        Assert.Null(repeater.AttachedViewportOrientation);
        Assert.Null(diagnostics.AttachedViewportOrientation);
        Assert.Equal(FWItemsRepeaterViewportSource.None, repeater.AttachedViewportSource);
        Assert.Equal(FWItemsRepeaterViewportSource.None, diagnostics.AttachedViewportSource);
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
    public void ApplyViewport_WithScroller_ShouldUseScrollerViewportMetrics()
    {
        // Arrange
        var scrollViewer = CreateScrollViewer(viewportHeight: 40, contentHeight: 240);
        scrollViewer.ScrollToVerticalOffset(50);
        var scroller = new FWScroller();
        scroller.AttachScrollViewer(scrollViewer);

        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = Enumerable.Range(0, 20).Select(index => $"Item {index}").ToArray(),
            EstimatedItemExtent = 10
        };

        // Act
        repeater.ApplyViewport(scroller);
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.Equal(FWItemsRepeaterRealizationSource.Viewport, diagnostics.RealizationSource);
        Assert.Equal(FWItemsRepeaterViewportSource.None, diagnostics.AttachedViewportSource);
        Assert.Equal(scrollViewer.VerticalOffset, diagnostics.ViewportStart);
        Assert.Equal(scrollViewer.ViewportHeight, diagnostics.ViewportLength);
        Assert.Equal(Orientation.Vertical, diagnostics.ViewportOrientation);
        Assert.Equal(5, diagnostics.FirstRealizedIndex);
        Assert.Equal(8, diagnostics.LastRealizedIndex);
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
        Assert.Equal(Orientation.Vertical, repeater.AttachedViewportOrientation);
        Assert.True(attachedDiagnostics.IsViewportAttached);
        Assert.Equal(Orientation.Vertical, attachedDiagnostics.AttachedViewportOrientation);
        Assert.Equal(FWItemsRepeaterRealizationSource.Viewport, attachedDiagnostics.RealizationSource);
        Assert.Equal(0, attachedDiagnostics.FirstRealizedIndex);
        Assert.Equal(2, attachedDiagnostics.LastRealizedIndex);
        Assert.Equal(40, scrolledDiagnostics.ViewportStart);
        Assert.Equal(30, scrolledDiagnostics.ViewportLength);
        Assert.Equal(4, scrolledDiagnostics.FirstRealizedIndex);
        Assert.Equal(6, scrolledDiagnostics.LastRealizedIndex);
        Assert.True(scrolledDiagnostics.IsViewportAttached);
        Assert.Equal(Orientation.Vertical, scrolledDiagnostics.AttachedViewportOrientation);
        Assert.Equal(FWItemsRepeaterViewportSource.ScrollViewer, scrolledDiagnostics.AttachedViewportSource);
    }

    [Fact]
    public void AttachViewport_WhenScrollerViewChanges_ShouldRefreshRealizationWindow()
    {
        // Arrange
        var scrollViewer = CreateScrollViewer(viewportHeight: 30, contentHeight: 200);
        var scroller = new FWScroller();
        scroller.AttachScrollViewer(scrollViewer);
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = Enumerable.Range(0, 20).Select(index => $"Item {index}").ToArray(),
            EstimatedItemExtent = 10
        };

        // Act
        repeater.AttachViewport(scroller);
        var attachedDiagnostics = repeater.GetDiagnostics();
        scroller.ScrollTo(0, 40);
        var scrolledDiagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.True(repeater.IsViewportAttached);
        Assert.Equal(FWItemsRepeaterViewportSource.Scroller, repeater.AttachedViewportSource);
        Assert.True(attachedDiagnostics.IsViewportAttached);
        Assert.Equal(FWItemsRepeaterViewportSource.Scroller, attachedDiagnostics.AttachedViewportSource);
        Assert.Equal(0, attachedDiagnostics.FirstRealizedIndex);
        Assert.Equal(2, attachedDiagnostics.LastRealizedIndex);
        Assert.Equal(40, scrolledDiagnostics.ViewportStart);
        Assert.Equal(30, scrolledDiagnostics.ViewportLength);
        Assert.Equal(4, scrolledDiagnostics.FirstRealizedIndex);
        Assert.Equal(6, scrolledDiagnostics.LastRealizedIndex);
        Assert.True(scrolledDiagnostics.IsViewportAttached);
        Assert.Equal(Orientation.Vertical, scrolledDiagnostics.AttachedViewportOrientation);
        Assert.Equal(FWItemsRepeaterViewportSource.Scroller, scrolledDiagnostics.AttachedViewportSource);
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
        Assert.Null(repeater.AttachedViewportOrientation);
        Assert.Null(diagnostics.AttachedViewportOrientation);
        Assert.Equal(FWItemsRepeaterViewportSource.None, diagnostics.AttachedViewportSource);
        Assert.Equal(FWItemsRepeaterRealizationSource.Viewport, diagnostics.RealizationSource);
        Assert.Equal(40, diagnostics.ViewportStart);
        Assert.Equal(4, diagnostics.FirstRealizedIndex);
        Assert.Equal(6, diagnostics.LastRealizedIndex);
    }

    [Fact]
    public void DetachViewport_WhenScrollerViewChanges_ShouldLeaveCurrentRealizationWindow()
    {
        // Arrange
        var scrollViewer = CreateScrollViewer(viewportHeight: 30, contentHeight: 200);
        var scroller = new FWScroller();
        scroller.AttachScrollViewer(scrollViewer);
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = Enumerable.Range(0, 20).Select(index => $"Item {index}").ToArray(),
            EstimatedItemExtent = 10
        };
        repeater.AttachViewport(scroller);
        scroller.ScrollTo(0, 40);

        // Act
        repeater.DetachViewport();
        scroller.ScrollTo(0, 80);
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.False(repeater.IsViewportAttached);
        Assert.False(diagnostics.IsViewportAttached);
        Assert.Null(repeater.AttachedViewportOrientation);
        Assert.Null(diagnostics.AttachedViewportOrientation);
        Assert.Equal(FWItemsRepeaterViewportSource.None, diagnostics.AttachedViewportSource);
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
        Assert.Equal(Orientation.Horizontal, repeater.AttachedViewportOrientation);
        Assert.Equal(Orientation.Horizontal, diagnostics.AttachedViewportOrientation);
        Assert.Equal(Orientation.Horizontal, diagnostics.ViewportOrientation);
        Assert.Equal(75, diagnostics.ViewportStart);
        Assert.Equal(50, diagnostics.ViewportLength);
        Assert.Equal(2, diagnostics.FirstRealizedIndex);
        Assert.Equal(5, diagnostics.LastRealizedIndex);
    }

    [Fact]
    public void AttachViewport_WhenSameScrollViewerReattachedWithNewOrientation_ShouldRefreshAttachmentDiagnostics()
    {
        // Arrange
        var scrollViewer = CreateScrollViewer(viewportWidth: 50, viewportHeight: 30, contentWidth: 240, contentHeight: 30);
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = Enumerable.Range(0, 20).Select(index => $"Item {index}").ToArray(),
            EstimatedItemExtent = 25
        };
        repeater.AttachViewport(scrollViewer);
        scrollViewer.ScrollToVerticalOffset(50);
        scrollViewer.ScrollToHorizontalOffset(75);

        // Act
        repeater.AttachViewport(scrollViewer, Orientation.Horizontal);
        var diagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.True(repeater.IsViewportAttached);
        Assert.Equal(Orientation.Horizontal, repeater.AttachedViewportOrientation);
        Assert.True(diagnostics.IsViewportAttached);
        Assert.Equal(Orientation.Horizontal, diagnostics.AttachedViewportOrientation);
        Assert.Equal(Orientation.Horizontal, diagnostics.ViewportOrientation);
        Assert.Equal(75, diagnostics.ViewportStart);
        Assert.Equal(50, diagnostics.ViewportLength);
        Assert.Equal(3, diagnostics.FirstRealizedIndex);
        Assert.Equal(4, diagnostics.LastRealizedIndex);
    }

    [Fact]
    public void AttachViewport_WhenReattachedToDifferentScrollViewer_ShouldIgnoreOldScrollViewerChanges()
    {
        // Arrange
        var oldScrollViewer = CreateScrollViewer(viewportHeight: 30, contentHeight: 200);
        var newScrollViewer = CreateScrollViewer(viewportHeight: 40, contentHeight: 200);
        var repeater = new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = Enumerable.Range(0, 20).Select(index => $"Item {index}").ToArray(),
            EstimatedItemExtent = 10
        };
        repeater.AttachViewport(oldScrollViewer);
        oldScrollViewer.ScrollToVerticalOffset(40);
        repeater.AttachViewport(newScrollViewer);
        var attachedDiagnostics = repeater.GetDiagnostics();

        // Act
        oldScrollViewer.ScrollToVerticalOffset(80);
        var afterOldScrollDiagnostics = repeater.GetDiagnostics();
        newScrollViewer.ScrollToVerticalOffset(50);
        var afterNewScrollDiagnostics = repeater.GetDiagnostics();

        // Assert
        Assert.Equal(0, attachedDiagnostics.ViewportStart);
        Assert.Equal(40, attachedDiagnostics.ViewportLength);
        Assert.Equal(0, afterOldScrollDiagnostics.ViewportStart);
        Assert.Equal(40, afterOldScrollDiagnostics.ViewportLength);
        Assert.Equal(50, afterNewScrollDiagnostics.ViewportStart);
        Assert.Equal(40, afterNewScrollDiagnostics.ViewportLength);
        Assert.Equal(5, afterNewScrollDiagnostics.FirstRealizedIndex);
        Assert.Equal(8, afterNewScrollDiagnostics.LastRealizedIndex);
        Assert.True(afterNewScrollDiagnostics.IsViewportAttached);
        Assert.Equal(Orientation.Vertical, afterNewScrollDiagnostics.AttachedViewportOrientation);
    }

    [Fact]
    public void AttachViewport_WithNullScrollViewer_ShouldThrow()
    {
        // Arrange
        var repeater = new FWItemsRepeater();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => repeater.AttachViewport((ScrollViewer)null!));
        Assert.Throws<ArgumentNullException>(() => repeater.AttachViewport((FWScroller)null!));
    }

    [Fact]
    public void AttachViewport_WithScrollerWithoutScrollViewer_ShouldThrow()
    {
        // Arrange
        var repeater = new FWItemsRepeater();
        var scroller = new FWScroller();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => repeater.ApplyViewport(scroller));
        Assert.Throws<InvalidOperationException>(() => repeater.AttachViewport(scroller));
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

    [Fact]
    public void GalleryProfileFactory_ShouldDescribeItemsRepeaterVisualQaScenarios()
    {
        // Arrange & Act
        var baseline = AdvancedCollectionsPage.CreateItemsRepeaterQaProfile(
            AdvancedCollectionsPage.ItemsRepeaterGalleryScenario.Baseline);
        var largeList = AdvancedCollectionsPage.CreateItemsRepeaterQaProfile(
            AdvancedCollectionsPage.ItemsRepeaterGalleryScenario.LargeListStress);
        var horizontal = AdvancedCollectionsPage.CreateItemsRepeaterQaProfile(
            AdvancedCollectionsPage.ItemsRepeaterGalleryScenario.HorizontalVirtualization);
        var cacheProfiles = AdvancedCollectionsPage.CreateItemsRepeaterCacheProfiles();

        // Assert
        Assert.Equal(20, baseline.ItemCount);
        Assert.Equal(FWItemsRepeaterViewportSource.Scroller, baseline.PreferredViewportSource);
        Assert.Equal(1500, largeList.ItemCount);
        Assert.Equal(Orientation.Vertical, largeList.Orientation);
        Assert.Equal(320, largeList.VerticalCacheLength);
        Assert.Equal(96, horizontal.ItemCount);
        Assert.Equal(Orientation.Horizontal, horizontal.Orientation);
        Assert.Equal(horizontal.HorizontalCacheLength, horizontal.ActiveCacheLength);
        Assert.Equal(FWItemsRepeaterViewportSource.ScrollViewer, horizontal.PreferredViewportSource);
        Assert.Collection(
            cacheProfiles,
            profile => Assert.Equal("Balanced", profile.Name),
            profile => Assert.Equal("Tight", profile.Name),
            profile => Assert.Equal("Stress buffer", profile.Name));
    }

    [Fact]
    public void GallerySampleItemFactory_ShouldCreateLargeListStressData()
    {
        // Arrange
        var profile = AdvancedCollectionsPage.CreateItemsRepeaterQaProfile(
            AdvancedCollectionsPage.ItemsRepeaterGalleryScenario.LargeListStress);

        // Act
        var items = AdvancedCollectionsPage.CreateItemsRepeaterSampleItems(profile);

        // Assert
        Assert.Equal(1500, items.Count);
        Assert.Equal("Stress Row 0001", items[0].Title);
        Assert.Equal("Stress Row 1500", items[^1].Title);
        Assert.Contains("Large-list stress QA item 1 of 1500", items[0].Description);
        Assert.Contains("Vertical", items[0].Status);
        Assert.Contains("cache H240/V320", items[0].Status);
    }

    [Fact]
    public void GalleryDiagnosticsText_ShouldSurfaceVirtualizationCacheAndAttachmentState()
    {
        // Arrange
        var profile = AdvancedCollectionsPage.CreateItemsRepeaterQaProfile(
            AdvancedCollectionsPage.ItemsRepeaterGalleryScenario.LargeListStress);
        var repeater = CreateRepeaterFromGalleryProfile(profile);

        // Act
        repeater.ApplyViewport(profile.ViewportStart, profile.ViewportLength, profile.Orientation);
        var diagnostics = repeater.GetDiagnostics();
        var scenarioText = AdvancedCollectionsPage.CreateItemsRepeaterScenarioText(
            profile,
            diagnostics,
            "Applied large-list stress");
        var diagnosticsText = AdvancedCollectionsPage.CreateItemsRepeaterDiagnosticsText(diagnostics);

        // Assert
        Assert.Equal(1500, diagnostics.ItemCount);
        Assert.True(diagnostics.RealizedElementCount < diagnostics.ItemCount);
        Assert.Equal(FWItemsRepeaterRealizationSource.Viewport, diagnostics.RealizationSource);
        Assert.Equal(320, diagnostics.ActiveCacheLength);
        Assert.Contains("Scenario: Large-list stress", scenarioText);
        Assert.Contains("Items: 1500", scenarioText);
        Assert.Contains("Last action: Applied large-list stress", scenarioText);
        Assert.Contains("QA: virtualized", diagnosticsText);
        Assert.Contains("Cache: active 320, H240/V320", diagnosticsText);
        Assert.Contains("Requested:", diagnosticsText);
        Assert.Contains("Range:", diagnosticsText);
    }

    [Fact]
    public void GalleryHorizontalProfile_ShouldUseHorizontalCacheAndLayout()
    {
        // Arrange
        var profile = AdvancedCollectionsPage.CreateItemsRepeaterQaProfile(
            AdvancedCollectionsPage.ItemsRepeaterGalleryScenario.HorizontalVirtualization);
        var repeater = CreateRepeaterFromGalleryProfile(profile);

        // Act
        repeater.ApplyViewport(profile.ViewportStart, profile.ViewportLength, profile.Orientation);
        var diagnostics = repeater.GetDiagnostics();
        var diagnosticsText = AdvancedCollectionsPage.CreateItemsRepeaterDiagnosticsText(diagnostics);

        // Assert
        var layout = Assert.IsType<StackLayout>(repeater.Layout);
        Assert.Equal(Orientation.Horizontal, layout.Orientation);
        Assert.Equal(Orientation.Horizontal, diagnostics.ViewportOrientation);
        Assert.Equal(profile.HorizontalCacheLength, diagnostics.ActiveCacheLength);
        Assert.Equal(FWItemsRepeaterRealizationSource.Viewport, diagnostics.RealizationSource);
        Assert.True(diagnostics.RealizedElementCount < diagnostics.ItemCount);
        Assert.Contains("Axis: Horizontal", diagnosticsText);
        Assert.Contains("Cache: active 360, H360/V80", diagnosticsText);
    }

    [Fact]
    public void GalleryReattachmentDiagnostics_ShouldReportLatestViewportSource()
    {
        // Arrange
        var profile = AdvancedCollectionsPage.CreateItemsRepeaterQaProfile(
            AdvancedCollectionsPage.ItemsRepeaterGalleryScenario.HorizontalVirtualization);
        var oldScrollViewer = CreateScrollViewer(viewportWidth: 60, viewportHeight: 30, contentWidth: 400, contentHeight: 30);
        var newScrollViewer = CreateScrollViewer(viewportWidth: 90, viewportHeight: 30, contentWidth: 800, contentHeight: 30);
        var scroller = new FWScroller();
        scroller.AttachScrollViewer(newScrollViewer);
        var repeater = CreateRepeaterFromGalleryProfile(profile);
        repeater.AttachViewport(oldScrollViewer, Orientation.Horizontal);
        oldScrollViewer.ScrollToHorizontalOffset(180);

        // Act
        repeater.AttachViewport(scroller, Orientation.Horizontal);
        newScrollViewer.ScrollToHorizontalOffset(360);
        oldScrollViewer.ScrollToHorizontalOffset(540);
        var diagnostics = repeater.GetDiagnostics();
        var scenarioText = AdvancedCollectionsPage.CreateItemsRepeaterScenarioText(
            profile,
            diagnostics,
            "Reattached ScrollViewer -> Scroller");

        // Assert
        Assert.True(diagnostics.IsViewportAttached);
        Assert.Equal(FWItemsRepeaterViewportSource.Scroller, diagnostics.AttachedViewportSource);
        Assert.Equal(Orientation.Horizontal, diagnostics.AttachedViewportOrientation);
        Assert.Equal(360, diagnostics.ViewportStart);
        Assert.Contains("Source: Scroller/Horizontal", scenarioText);
        Assert.Contains("Reattached ScrollViewer -> Scroller", scenarioText);
    }

    [Fact]
    public void GalleryItemsViewRecipe_ShouldTrackSelectionInvocationAndViewport()
    {
        // Arrange
        var state = AdvancedCollectionsPage.CreateCollectionRecipeState(
            AdvancedCollectionsPage.CollectionRecipeKind.ItemsViewSelection);

        // Act
        state = AdvancedCollectionsPage.ApplyCollectionRecipeCommand(
            state,
            AdvancedCollectionsPage.CollectionRecipeCommand.Next);
        state = AdvancedCollectionsPage.ApplyCollectionRecipeCommand(
            state,
            AdvancedCollectionsPage.CollectionRecipeCommand.Invoke);
        var repeater = CreateRepeaterFromRecipeState(state);
        repeater.ApplyViewport(
            AdvancedCollectionsPage.GetRecipeViewportStart(state),
            AdvancedCollectionsPage.GetRecipeViewportLength(state));
        var diagnostics = repeater.GetDiagnostics();
        var diagnosticsText = AdvancedCollectionsPage.CreateCollectionRecipeDiagnosticsText(state, diagnostics);

        // Assert
        Assert.Equal(18, state.ItemCount);
        Assert.Equal(1, state.SelectedIndex);
        Assert.Equal(1, state.InvokedIndex);
        Assert.Equal("Review item 02", AdvancedCollectionsPage.GetCollectionRecipeItem(state).Title);
        Assert.Equal(FWItemsRepeaterRealizationSource.Viewport, diagnostics.RealizationSource);
        Assert.Equal(64, diagnostics.ViewportStart);
        Assert.Equal(192, diagnostics.ViewportLength);
        Assert.Contains("ItemsView-like selection", diagnosticsText);
        Assert.Contains("Selected: 1", diagnosticsText);
        Assert.Contains("Invoked: 1", diagnosticsText);
        Assert.Contains("Last input: Invoke", diagnosticsText);
    }

    [Fact]
    public void GalleryFlipViewRecipe_ShouldSynchronizePageSelectionAndHorizontalWindow()
    {
        // Arrange
        var state = AdvancedCollectionsPage.CreateCollectionRecipeState(
            AdvancedCollectionsPage.CollectionRecipeKind.FlipViewPaging);

        // Act
        state = AdvancedCollectionsPage.ApplyCollectionRecipeCommand(
            state,
            AdvancedCollectionsPage.CollectionRecipeCommand.SelectIndex,
            4);
        state = AdvancedCollectionsPage.ApplyCollectionRecipeCommand(
            state,
            AdvancedCollectionsPage.CollectionRecipeCommand.Invoke);
        var repeater = CreateRepeaterFromRecipeState(state);
        repeater.ApplyViewport(
            AdvancedCollectionsPage.GetRecipeViewportStart(state),
            AdvancedCollectionsPage.GetRecipeViewportLength(state),
            Orientation.Horizontal);
        var diagnostics = repeater.GetDiagnostics();
        var diagnosticsText = AdvancedCollectionsPage.CreateCollectionRecipeDiagnosticsText(state, diagnostics);

        // Assert
        var layout = Assert.IsType<StackLayout>(AdvancedCollectionsPage.CreateCollectionRecipeLayout(state));
        Assert.Equal(Orientation.Horizontal, layout.Orientation);
        Assert.Equal(4, state.PageIndex);
        Assert.Equal(4, state.SelectedIndex);
        Assert.Equal(4, state.InvokedIndex);
        Assert.Equal(960, diagnostics.ViewportStart);
        Assert.Equal(240, diagnostics.ViewportLength);
        Assert.Equal(Orientation.Horizontal, diagnostics.ViewportOrientation);
        Assert.Contains("FlipView-like paging", diagnosticsText);
        Assert.Contains("Page: 5/6", diagnosticsText);
        Assert.Contains("Invoked: 4", diagnosticsText);
    }

    [Fact]
    public void GallerySemanticZoomRecipe_ShouldTrackGroupOverviewAndDetails()
    {
        // Arrange
        var state = AdvancedCollectionsPage.CreateCollectionRecipeState(
            AdvancedCollectionsPage.CollectionRecipeKind.SemanticZoomGrouping);

        // Act
        state = AdvancedCollectionsPage.ApplyCollectionRecipeCommand(
            state,
            AdvancedCollectionsPage.CollectionRecipeCommand.SelectNextGroup);
        var overviewState = state;
        state = AdvancedCollectionsPage.ApplyCollectionRecipeCommand(
            state,
            AdvancedCollectionsPage.CollectionRecipeCommand.ToggleZoom);
        state = AdvancedCollectionsPage.ApplyCollectionRecipeCommand(
            state,
            AdvancedCollectionsPage.CollectionRecipeCommand.Invoke);
        var repeater = CreateRepeaterFromRecipeState(state);
        repeater.ApplyViewport(
            AdvancedCollectionsPage.GetRecipeViewportStart(state),
            AdvancedCollectionsPage.GetRecipeViewportLength(state));
        var diagnostics = repeater.GetDiagnostics();
        var diagnosticsText = AdvancedCollectionsPage.CreateCollectionRecipeDiagnosticsText(state, diagnostics);

        // Assert
        Assert.True(overviewState.IsZoomedOut);
        Assert.Equal(1, overviewState.GroupIndex);
        Assert.Equal(1, overviewState.SelectedIndex);
        Assert.False(state.IsZoomedOut);
        Assert.Equal("Review item 02", AdvancedCollectionsPage.GetCollectionRecipeItem(state).Title);
        Assert.Equal(1, state.InvokedIndex);
        Assert.Equal(72, diagnostics.ViewportStart);
        Assert.Equal(216, diagnostics.ViewportLength);
        Assert.Contains("SemanticZoom-like grouping", diagnosticsText);
        Assert.Contains("Zoom: details group Review", diagnosticsText);
        Assert.Contains("Invoked: 1", diagnosticsText);
    }

    [Fact]
    public void GalleryCollectionNavigationEvaluation_ShouldKeepCandidatesAsRecipesUntilContractsAreProven()
    {
        // Act
        var evaluations = AdvancedCollectionsPage.CreateCollectionNavigationEvaluations();
        var summary = AdvancedCollectionsPage.CreateCollectionNavigationEvaluationSummary(evaluations);

        // Assert
        Assert.Equal(3, evaluations.Count);
        Assert.Contains("3 candidates", summary);
        Assert.Contains("3 recipe-first", summary);
        Assert.Contains("0 public-ready", summary);

        AssertEvaluation(evaluations, "FWItemsView", "multi-select selection model");
        AssertEvaluation(evaluations, "FWFlipView", "touch swipe gesture host");
        AssertEvaluation(evaluations, "FWSemanticZoom", "two-view synchronized source API");
    }

    private static DataTemplate CreateTextTemplate()
    {
        var template = new DataTemplate();
        template.SetVisualTree(() => new TextBlock());
        template.Seal();
        return template;
    }

    private static FWItemsRepeater CreateRepeaterFromGalleryProfile(
        AdvancedCollectionsPage.ItemsRepeaterGalleryProfile profile)
    {
        return new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = AdvancedCollectionsPage.CreateItemsRepeaterSampleItems(profile),
            Layout = AdvancedCollectionsPage.CreateItemsRepeaterLayout(profile),
            EstimatedItemExtent = profile.EstimatedItemExtent,
            HorizontalCacheLength = profile.HorizontalCacheLength,
            VerticalCacheLength = profile.VerticalCacheLength
        };
    }

    private static FWItemsRepeater CreateRepeaterFromRecipeState(
        AdvancedCollectionsPage.CollectionRecipeState state)
    {
        return new FWItemsRepeater
        {
            ItemTemplate = CreateTextTemplate(),
            ItemsSource = AdvancedCollectionsPage.CreateCollectionRecipeItems(state),
            Layout = AdvancedCollectionsPage.CreateCollectionRecipeLayout(state),
            EstimatedItemExtent = AdvancedCollectionsPage.GetRecipeEstimatedItemExtent(state),
            HorizontalCacheLength = state.Kind == AdvancedCollectionsPage.CollectionRecipeKind.FlipViewPaging ? 96 : 0,
            VerticalCacheLength = state.Kind == AdvancedCollectionsPage.CollectionRecipeKind.FlipViewPaging ? 0 : 96
        };
    }

    private static void AssertEvaluation(
        IReadOnlyList<AdvancedCollectionsPage.CollectionNavigationEvaluation> evaluations,
        string candidateControl,
        string expectedRisk)
    {
        var evaluation = Assert.Single(evaluations, evaluation => evaluation.CandidateControl == candidateControl);
        var text = AdvancedCollectionsPage.FormatCollectionNavigationEvaluation(evaluation);

        Assert.False(evaluation.IsPublicApiReady);
        Assert.Equal(4, evaluation.ProvenSemanticCount);
        Assert.Equal("Keep as Gallery recipe before public API", evaluation.RecommendedSurface);
        Assert.Contains(expectedRisk, evaluation.RemainingRisks);
        Assert.Contains(candidateControl, text);
        Assert.Contains("recipe/prototype", text);
        Assert.Contains("semantics keyboard on, selection on, viewport on, virtualization on", text);
        Assert.Contains(expectedRisk, text);
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
