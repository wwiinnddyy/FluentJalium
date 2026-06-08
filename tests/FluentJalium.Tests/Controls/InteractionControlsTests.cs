using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;

namespace FluentJalium.Tests.Controls;

public class FWRefreshContainerTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var container = new FWRefreshContainer();

        // Assert
        Assert.NotNull(container);
        Assert.Equal(RefreshPullDirection.TopToBottom, container.PullDirection);
        Assert.Null(container.Visualizer);
        Assert.Equal(RefreshPullDirection.TopToBottom, container.GetDiagnostics().PullDirection);
        Assert.False(container.GetDiagnostics().IsRefreshing);
        Assert.False(container.GetDiagnostics().IsPulling);
        Assert.Equal(0, container.GetDiagnostics().PullDistance);
        Assert.Equal(100, container.GetDiagnostics().PullThreshold);
        Assert.Equal(150, container.GetDiagnostics().MaxPullDistance);
        Assert.Equal(0, container.GetDiagnostics().PullProgress);
        Assert.False(container.GetDiagnostics().HasCustomVisualizer);
        Assert.Equal(RefreshVisualizerState.Idle, container.GetDiagnostics().VisualizerState);
    }

    [Theory]
    [InlineData(RefreshPullDirection.TopToBottom)]
    [InlineData(RefreshPullDirection.BottomToTop)]
    [InlineData(RefreshPullDirection.LeftToRight)]
    [InlineData(RefreshPullDirection.RightToLeft)]
    public void PullDirection_WhenSet_ShouldUpdateProperty(RefreshPullDirection direction)
    {
        // Arrange
        var container = new FWRefreshContainer();

        // Act
        container.PullDirection = direction;

        // Assert
        Assert.Equal(direction, container.PullDirection);
    }

    [Fact]
    public void RequestRefresh_ShouldInvokeWithoutException()
    {
        // Arrange
        var container = new FWRefreshContainer();

        // Act & Assert
        var exception = Record.Exception(() => container.RequestRefresh());
        Assert.Null(exception);
    }

    [Fact]
    public void RequestRefresh_WithDeferral_ShouldExposeRefreshingDiagnosticsUntilCompleted()
    {
        // Arrange
        var container = new FWRefreshContainer();
        RefreshRequestedDeferral? deferral = null;
        container.RefreshRequested += (_, args) =>
        {
            deferral = args.GetDeferral();
        };

        // Act
        container.RequestRefresh();
        var refreshingDiagnostics = container.GetDiagnostics();
        deferral?.Complete();
        var completedDiagnostics = container.GetDiagnostics();

        // Assert
        Assert.NotNull(deferral);
        Assert.True(refreshingDiagnostics.IsRefreshing);
        Assert.Equal(RefreshVisualizerState.Idle, refreshingDiagnostics.VisualizerState);
        Assert.False(completedDiagnostics.IsRefreshing);
        Assert.Equal(0, completedDiagnostics.PullDistance);
    }

    [Fact]
    public void GetDiagnostics_WithVisualizer_ShouldExposeVisualizerState()
    {
        // Arrange
        var visualizer = new TestRefreshVisualizer
        {
            State = RefreshVisualizerState.Pending
        };
        var container = new FWRefreshContainer
        {
            Visualizer = visualizer
        };

        // Act
        var diagnostics = container.GetDiagnostics();

        // Assert
        Assert.True(diagnostics.HasCustomVisualizer);
        Assert.Equal(RefreshVisualizerState.Pending, diagnostics.VisualizerState);
    }

    [Fact]
    public void ImplementsIFluentJaliumControl()
    {
        // Arrange
        var container = new FWRefreshContainer();

        // Assert
        Assert.IsAssignableFrom<IFluentJaliumControl>(container);
    }

    private sealed class TestRefreshVisualizer : RefreshVisualizer
    {
        public override void UpdateProgress(double progress)
        {
        }
    }
}

public class FWScrollerTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var scroller = new FWScroller();

        // Assert
        Assert.NotNull(scroller);
        Assert.Equal(ScrollMode.Auto, scroller.HorizontalScrollMode);
        Assert.Equal(ScrollMode.Auto, scroller.VerticalScrollMode);
        Assert.Equal(ChainingMode.Auto, scroller.HorizontalScrollChainingMode);
        Assert.Equal(ChainingMode.Auto, scroller.VerticalScrollChainingMode);
        Assert.Equal(RailingMode.Enabled, scroller.HorizontalScrollRailingMode);
        Assert.Equal(RailingMode.Enabled, scroller.VerticalScrollRailingMode);
        Assert.Equal(ZoomMode.Disabled, scroller.ZoomMode);
        Assert.Equal(0.1, scroller.MinZoomFactor);
        Assert.Equal(10.0, scroller.MaxZoomFactor);
        Assert.Equal(1.0, scroller.ZoomFactor);
        Assert.Equal(SnapPointsType.None, scroller.HorizontalSnapPointsType);
        Assert.Equal(SnapPointsType.None, scroller.VerticalSnapPointsType);
        Assert.False(scroller.IsAnchoredAtHorizontalExtent);
        Assert.False(scroller.IsAnchoredAtVerticalExtent);
        Assert.Null(scroller.ScrollViewer);
        Assert.Equal(0, scroller.HorizontalOffset);
        Assert.Equal(0, scroller.VerticalOffset);
        Assert.False(scroller.GetViewportDiagnostics().HasScrollViewer);
    }

    [Theory]
    [InlineData(ScrollMode.Disabled)]
    [InlineData(ScrollMode.Enabled)]
    [InlineData(ScrollMode.Auto)]
    public void HorizontalScrollMode_WhenSet_ShouldUpdateProperty(ScrollMode mode)
    {
        // Arrange
        var scroller = new FWScroller();

        // Act
        scroller.HorizontalScrollMode = mode;

        // Assert
        Assert.Equal(mode, scroller.HorizontalScrollMode);
    }

    [Theory]
    [InlineData(ScrollMode.Disabled)]
    [InlineData(ScrollMode.Enabled)]
    [InlineData(ScrollMode.Auto)]
    public void VerticalScrollMode_WhenSet_ShouldUpdateProperty(ScrollMode mode)
    {
        // Arrange
        var scroller = new FWScroller();

        // Act
        scroller.VerticalScrollMode = mode;

        // Assert
        Assert.Equal(mode, scroller.VerticalScrollMode);
    }

    [Theory]
    [InlineData(ChainingMode.Auto)]
    [InlineData(ChainingMode.Always)]
    [InlineData(ChainingMode.Never)]
    public void HorizontalScrollChainingMode_WhenSet_ShouldUpdateProperty(ChainingMode mode)
    {
        // Arrange
        var scroller = new FWScroller();

        // Act
        scroller.HorizontalScrollChainingMode = mode;

        // Assert
        Assert.Equal(mode, scroller.HorizontalScrollChainingMode);
    }

    [Theory]
    [InlineData(RailingMode.Enabled)]
    [InlineData(RailingMode.Disabled)]
    public void HorizontalScrollRailingMode_WhenSet_ShouldUpdateProperty(RailingMode mode)
    {
        // Arrange
        var scroller = new FWScroller();

        // Act
        scroller.HorizontalScrollRailingMode = mode;

        // Assert
        Assert.Equal(mode, scroller.HorizontalScrollRailingMode);
    }

    [Theory]
    [InlineData(ZoomMode.Disabled)]
    [InlineData(ZoomMode.Enabled)]
    public void ZoomMode_WhenSet_ShouldUpdateProperty(ZoomMode mode)
    {
        // Arrange
        var scroller = new FWScroller();

        // Act
        scroller.ZoomMode = mode;

        // Assert
        Assert.Equal(mode, scroller.ZoomMode);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void MinZoomFactor_WhenSetToValidValue_ShouldUpdateProperty(double factor)
    {
        // Arrange
        var scroller = new FWScroller();

        // Act
        scroller.MinZoomFactor = factor;

        // Assert
        Assert.Equal(factor, scroller.MinZoomFactor);
    }

    [Theory]
    [InlineData(2.0)]
    [InlineData(5.0)]
    [InlineData(10.0)]
    public void MaxZoomFactor_WhenSetToValidValue_ShouldUpdateProperty(double factor)
    {
        // Arrange
        var scroller = new FWScroller();

        // Act
        scroller.MaxZoomFactor = factor;

        // Assert
        Assert.Equal(factor, scroller.MaxZoomFactor);
    }

    [Theory]
    [InlineData(SnapPointsType.None)]
    [InlineData(SnapPointsType.Optional)]
    [InlineData(SnapPointsType.Mandatory)]
    [InlineData(SnapPointsType.OptionalSingle)]
    [InlineData(SnapPointsType.MandatorySingle)]
    public void HorizontalSnapPointsType_WhenSet_ShouldUpdateProperty(SnapPointsType type)
    {
        // Arrange
        var scroller = new FWScroller();

        // Act
        scroller.HorizontalSnapPointsType = type;

        // Assert
        Assert.Equal(type, scroller.HorizontalSnapPointsType);
    }

    [Fact]
    public void ScrollTo_ShouldInvokeWithoutException()
    {
        // Arrange
        var scroller = new FWScroller();

        // Act & Assert
        var exception = Record.Exception(() => scroller.ScrollTo(100, 100));
        Assert.Null(exception);
    }

    [Fact]
    public void AttachScrollViewer_ShouldExposeViewportDiagnosticsAndRaiseViewEvents()
    {
        // Arrange
        var scrollViewer = CreateScrollViewer(viewportWidth: 50, viewportHeight: 40, contentWidth: 160, contentHeight: 220);
        var scroller = new FWScroller();
        var viewChanging = 0;
        var viewChanged = 0;
        scroller.ViewChanging += (_, e) =>
        {
            viewChanging++;
        };
        scroller.ViewChanged += (_, e) =>
        {
            viewChanged++;
        };

        // Act
        scroller.AttachScrollViewer(scrollViewer);
        scroller.ScrollTo(30, 60);
        var diagnostics = scroller.GetViewportDiagnostics();

        // Assert
        Assert.Same(scrollViewer, scroller.ScrollViewer);
        Assert.True(diagnostics.HasScrollViewer);
        Assert.Equal(30, diagnostics.HorizontalOffset);
        Assert.Equal(60, diagnostics.VerticalOffset);
        Assert.Equal(scrollViewer.ViewportWidth, diagnostics.ViewportWidth);
        Assert.Equal(scrollViewer.ViewportHeight, diagnostics.ViewportHeight);
        Assert.Equal(1.0, diagnostics.ZoomFactor);
        Assert.True(viewChanging > 0);
        Assert.True(viewChanged > 0);
    }

    [Fact]
    public void ScrollBy_ShouldInvokeWithoutException()
    {
        // Arrange
        var scroller = new FWScroller();

        // Act & Assert
        var exception = Record.Exception(() => scroller.ScrollBy(10, 10));
        Assert.Null(exception);
    }

    [Fact]
    public void ZoomTo_ShouldInvokeWithoutException()
    {
        // Arrange
        var scroller = new FWScroller();

        // Act & Assert
        var exception = Record.Exception(() => scroller.ZoomTo(1.5));
        Assert.Null(exception);
    }

    [Fact]
    public void ImplementsIFluentJaliumControl()
    {
        // Arrange
        var scroller = new FWScroller();

        // Assert
        Assert.IsAssignableFrom<IFluentJaliumControl>(scroller);
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

public class FWAnnotatedScrollBarTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var scrollBar = new FWAnnotatedScrollBar();

        // Assert
        Assert.NotNull(scrollBar);
        Assert.Null(scrollBar.Labels);
        Assert.False(scrollBar.GetDiagnostics().HasLabels);
        Assert.Equal(0, scrollBar.GetDiagnostics().SourceLabelCount);
        Assert.Equal(0, scrollBar.GetDiagnostics().RegisteredLabelCount);
        Assert.Equal(Orientation.Vertical, scrollBar.GetDiagnostics().Orientation);
    }

    [Fact]
    public void Labels_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var scrollBar = new FWAnnotatedScrollBar();
        var labels = new List<ScrollBarLabel>
        {
            new ScrollBarLabel { ScrollOffset = 100, Content = "Test" }
        };

        // Act
        scrollBar.Labels = labels;

        // Assert
        Assert.Equal(labels, scrollBar.Labels);
        Assert.Equal(1, scrollBar.GetDiagnostics().SourceLabelCount);
        Assert.Equal(1, scrollBar.GetDiagnostics().RegisteredLabelCount);
        Assert.True(scrollBar.GetDiagnostics().HasLabels);
    }

    [Fact]
    public void ValueNearLabel_ShouldRaiseDetailLabelRequestedAndExposeDiagnostics()
    {
        // Arrange
        var scrollBar = new FWAnnotatedScrollBar
        {
            Minimum = 0,
            Maximum = 500,
            Labels = new List<ScrollBarLabel>
            {
                new ScrollBarLabel { ScrollOffset = 100, Content = "Important", Type = ScrollBarLabelType.Warning }
            }
        };
        DetailLabelRequestedEventArgs? requested = null;
        scrollBar.DetailLabelRequested += (_, args) =>
        {
            requested = args;
        };

        // Act
        scrollBar.Value = 104;
        var diagnostics = scrollBar.GetDiagnostics();

        // Assert
        Assert.NotNull(requested);
        Assert.Equal(100, requested.ScrollOffset);
        Assert.Equal("Important", requested.Content);
        Assert.Equal(ScrollBarLabelType.Warning, requested.LabelType);
        Assert.Equal(100, diagnostics.LastRequestedScrollOffset);
        Assert.Equal("Important", diagnostics.LastRequestedContent);
        Assert.Equal(ScrollBarLabelType.Warning, diagnostics.LastRequestedLabelType);
    }

    [Fact]
    public void ImplementsIFluentJaliumControl()
    {
        // Arrange
        var scrollBar = new FWAnnotatedScrollBar();

        // Assert
        Assert.IsAssignableFrom<IFluentJaliumControl>(scrollBar);
    }
}

public class ScrollBarLabelTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var label = new ScrollBarLabel();

        // Assert
        Assert.NotNull(label);
        Assert.Equal(0.0, label.ScrollOffset);
        Assert.Null(label.Content);
        Assert.Null(label.Background);
        Assert.Equal(ScrollBarLabelType.Default, label.Type);
    }

    [Fact]
    public void ScrollOffset_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var label = new ScrollBarLabel();
        const double offset = 150.0;

        // Act
        label.ScrollOffset = offset;

        // Assert
        Assert.Equal(offset, label.ScrollOffset);
    }

    [Fact]
    public void Content_WhenSet_ShouldUpdateProperty()
    {
        // Arrange
        var label = new ScrollBarLabel();
        const string content = "Test Label";

        // Act
        label.Content = content;

        // Assert
        Assert.Equal(content, label.Content);
    }

    [Theory]
    [InlineData(ScrollBarLabelType.Default)]
    [InlineData(ScrollBarLabelType.Warning)]
    [InlineData(ScrollBarLabelType.Error)]
    [InlineData(ScrollBarLabelType.Info)]
    public void Type_WhenSet_ShouldUpdateProperty(ScrollBarLabelType type)
    {
        // Arrange
        var label = new ScrollBarLabel();

        // Act
        label.Type = type;

        // Assert
        Assert.Equal(type, label.Type);
    }
}
