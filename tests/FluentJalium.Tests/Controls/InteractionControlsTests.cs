using FluentJalium.Controls;
using Jalium.UI;
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
    public void ImplementsIFluentJaliumControl()
    {
        // Arrange
        var container = new FWRefreshContainer();

        // Assert
        Assert.IsAssignableFrom<IFluentJaliumControl>(container);
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
