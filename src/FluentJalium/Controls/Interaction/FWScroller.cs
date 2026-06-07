using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Input;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium Scroller control for advanced scrolling scenarios with snap points and chaining.
/// </summary>
public class FWScroller : ContentControl, IFluentJaliumControl
{
    private ScrollViewer? _scrollViewer;
    private Point _lastPosition;

    public static readonly DependencyProperty HorizontalScrollModeProperty =
        DependencyProperty.Register(nameof(HorizontalScrollMode), typeof(ScrollMode), typeof(FWScroller),
            new PropertyMetadata(ScrollMode.Auto, OnScrollModeChanged));

    public static readonly DependencyProperty VerticalScrollModeProperty =
        DependencyProperty.Register(nameof(VerticalScrollMode), typeof(ScrollMode), typeof(FWScroller),
            new PropertyMetadata(ScrollMode.Auto, OnScrollModeChanged));

    public static readonly DependencyProperty HorizontalScrollChainingModeProperty =
        DependencyProperty.Register(nameof(HorizontalScrollChainingMode), typeof(ChainingMode), typeof(FWScroller),
            new PropertyMetadata(ChainingMode.Auto));

    public static readonly DependencyProperty VerticalScrollChainingModeProperty =
        DependencyProperty.Register(nameof(VerticalScrollChainingMode), typeof(ChainingMode), typeof(FWScroller),
            new PropertyMetadata(ChainingMode.Auto));

    public static readonly DependencyProperty HorizontalScrollRailingModeProperty =
        DependencyProperty.Register(nameof(HorizontalScrollRailingMode), typeof(RailingMode), typeof(FWScroller),
            new PropertyMetadata(RailingMode.Enabled));

    public static readonly DependencyProperty VerticalScrollRailingModeProperty =
        DependencyProperty.Register(nameof(VerticalScrollRailingMode), typeof(RailingMode), typeof(FWScroller),
            new PropertyMetadata(RailingMode.Enabled));

    public static readonly DependencyProperty ZoomModeProperty =
        DependencyProperty.Register(nameof(ZoomMode), typeof(ZoomMode), typeof(FWScroller),
            new PropertyMetadata(ZoomMode.Disabled, OnZoomModeChanged));

    public static readonly DependencyProperty MinZoomFactorProperty =
        DependencyProperty.Register(nameof(MinZoomFactor), typeof(double), typeof(FWScroller),
            new PropertyMetadata(0.1, OnZoomFactorChanged), ValidateZoomFactor);

    public static readonly DependencyProperty MaxZoomFactorProperty =
        DependencyProperty.Register(nameof(MaxZoomFactor), typeof(double), typeof(FWScroller),
            new PropertyMetadata(10.0, OnZoomFactorChanged), ValidateZoomFactor);

    public static readonly DependencyProperty ZoomFactorProperty =
        DependencyProperty.Register(nameof(ZoomFactor), typeof(double), typeof(FWScroller),
            new PropertyMetadata(1.0, OnZoomFactorChanged), ValidateZoomFactor);

    public static readonly DependencyProperty HorizontalSnapPointsTypeProperty =
        DependencyProperty.Register(nameof(HorizontalSnapPointsType), typeof(SnapPointsType), typeof(FWScroller),
            new PropertyMetadata(SnapPointsType.None));

    public static readonly DependencyProperty VerticalSnapPointsTypeProperty =
        DependencyProperty.Register(nameof(VerticalSnapPointsType), typeof(SnapPointsType), typeof(FWScroller),
            new PropertyMetadata(SnapPointsType.None));

    public static readonly DependencyProperty IsAnchoredAtHorizontalExtentProperty =
        DependencyProperty.Register(nameof(IsAnchoredAtHorizontalExtent), typeof(bool), typeof(FWScroller),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsAnchoredAtVerticalExtentProperty =
        DependencyProperty.Register(nameof(IsAnchoredAtVerticalExtent), typeof(bool), typeof(FWScroller),
            new PropertyMetadata(false));

    public static readonly RoutedEvent ViewChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(ViewChanged), RoutingStrategy.Bubble,
            typeof(EventHandler<ScrollerViewChangedEventArgs>), typeof(FWScroller));

    public static readonly RoutedEvent ViewChangingEvent =
        EventManager.RegisterRoutedEvent(nameof(ViewChanging), RoutingStrategy.Bubble,
            typeof(EventHandler<ScrollerViewChangingEventArgs>), typeof(FWScroller));

    /// <summary>
    /// Initializes a new instance of the <see cref="FWScroller"/> class.
    /// </summary>
    public FWScroller()
    {
    }

    /// <summary>
    /// Gets or sets the horizontal scroll mode.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public ScrollMode HorizontalScrollMode
    {
        get => (ScrollMode)GetValue(HorizontalScrollModeProperty)!;
        set => SetValue(HorizontalScrollModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical scroll mode.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public ScrollMode VerticalScrollMode
    {
        get => (ScrollMode)GetValue(VerticalScrollModeProperty)!;
        set => SetValue(VerticalScrollModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal scroll chaining mode.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public ChainingMode HorizontalScrollChainingMode
    {
        get => (ChainingMode)GetValue(HorizontalScrollChainingModeProperty)!;
        set => SetValue(HorizontalScrollChainingModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical scroll chaining mode.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public ChainingMode VerticalScrollChainingMode
    {
        get => (ChainingMode)GetValue(VerticalScrollChainingModeProperty)!;
        set => SetValue(VerticalScrollChainingModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal scroll railing mode.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public RailingMode HorizontalScrollRailingMode
    {
        get => (RailingMode)GetValue(HorizontalScrollRailingModeProperty)!;
        set => SetValue(HorizontalScrollRailingModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical scroll railing mode.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public RailingMode VerticalScrollRailingMode
    {
        get => (RailingMode)GetValue(VerticalScrollRailingModeProperty)!;
        set => SetValue(VerticalScrollRailingModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the zoom mode.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public ZoomMode ZoomMode
    {
        get => (ZoomMode)GetValue(ZoomModeProperty)!;
        set => SetValue(ZoomModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum zoom factor.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public double MinZoomFactor
    {
        get => (double)GetValue(MinZoomFactorProperty)!;
        set => SetValue(MinZoomFactorProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum zoom factor.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public double MaxZoomFactor
    {
        get => (double)GetValue(MaxZoomFactorProperty)!;
        set => SetValue(MaxZoomFactorProperty, value);
    }

    /// <summary>
    /// Gets or sets the current zoom factor.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public double ZoomFactor
    {
        get => (double)GetValue(ZoomFactorProperty)!;
        set => SetValue(ZoomFactorProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal snap points type.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public SnapPointsType HorizontalSnapPointsType
    {
        get => (SnapPointsType)GetValue(HorizontalSnapPointsTypeProperty)!;
        set => SetValue(HorizontalSnapPointsTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical snap points type.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public SnapPointsType VerticalSnapPointsType
    {
        get => (SnapPointsType)GetValue(VerticalSnapPointsTypeProperty)!;
        set => SetValue(VerticalSnapPointsTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the content is anchored at the horizontal extent.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsAnchoredAtHorizontalExtent
    {
        get => (bool)GetValue(IsAnchoredAtHorizontalExtentProperty)!;
        set => SetValue(IsAnchoredAtHorizontalExtentProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the content is anchored at the vertical extent.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsAnchoredAtVerticalExtent
    {
        get => (bool)GetValue(IsAnchoredAtVerticalExtentProperty)!;
        set => SetValue(IsAnchoredAtVerticalExtentProperty, value);
    }

    /// <summary>
    /// Occurs when the view has changed.
    /// </summary>
    public event EventHandler<ScrollerViewChangedEventArgs> ViewChanged
    {
        add => AddHandler(ViewChangedEvent, value);
        remove => RemoveHandler(ViewChangedEvent, value);
    }

    /// <summary>
    /// Occurs when the view is changing.
    /// </summary>
    public event EventHandler<ScrollerViewChangingEventArgs> ViewChanging
    {
        add => AddHandler(ViewChangingEvent, value);
        remove => RemoveHandler(ViewChangingEvent, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged -= OnScrollChanged;
        }

        _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;

        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged += OnScrollChanged;
            UpdateScrollViewerProperties();
        }
    }

    /// <summary>
    /// Scrolls to the specified horizontal offset.
    /// </summary>
    public void ScrollTo(double horizontalOffset, double verticalOffset)
    {
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollToHorizontalOffset(horizontalOffset);
            _scrollViewer.ScrollToVerticalOffset(verticalOffset);
        }
    }

    /// <summary>
    /// Scrolls by the specified delta.
    /// </summary>
    public void ScrollBy(double horizontalDelta, double verticalDelta)
    {
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset + horizontalDelta);
            _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset + verticalDelta);
        }
    }

    /// <summary>
    /// Zooms to the specified factor.
    /// </summary>
    public void ZoomTo(double zoomFactor, Point? centerPoint = null)
    {
        ZoomFactor = Math.Clamp(zoomFactor, MinZoomFactor, MaxZoomFactor);
    }

    private static void OnScrollModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWScroller scroller)
        {
            scroller.UpdateScrollViewerProperties();
        }
    }

    private static void OnZoomModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWScroller scroller)
        {
            scroller.UpdateScrollViewerProperties();
        }
    }

    private static void OnZoomFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWScroller scroller && e.NewValue is double zoomFactor)
        {
            scroller.ApplyZoom(zoomFactor);
        }
    }

    private static bool ValidateZoomFactor(object? value)
    {
        return value is double d && d > 0 && !double.IsInfinity(d) && !double.IsNaN(d);
    }

    private void UpdateScrollViewerProperties()
    {
        if (_scrollViewer == null)
            return;

        _scrollViewer.HorizontalScrollBarVisibility = HorizontalScrollMode switch
        {
            ScrollMode.Disabled => ScrollBarVisibility.Disabled,
            ScrollMode.Enabled => ScrollBarVisibility.Visible,
            _ => ScrollBarVisibility.Auto
        };

        _scrollViewer.VerticalScrollBarVisibility = VerticalScrollMode switch
        {
            ScrollMode.Disabled => ScrollBarVisibility.Disabled,
            ScrollMode.Enabled => ScrollBarVisibility.Visible,
            _ => ScrollBarVisibility.Auto
        };
    }

    private void ApplyZoom(double zoomFactor)
    {
        if (Content is FrameworkElement element)
        {
            var scaleTransform = new ScaleTransform(zoomFactor, zoomFactor);
            element.RenderTransform = scaleTransform;
            element.RenderTransformOrigin = new Point(0.5, 0.5);
        }
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var currentPosition = new Point(e.HorizontalOffset, e.VerticalOffset);

        var changingArgs = new ScrollerViewChangingEventArgs(ViewChangingEvent, this)
        {
            NextHorizontalOffset = currentPosition.X,
            NextVerticalOffset = currentPosition.Y
        };
        RaiseEvent(changingArgs);

        var changedArgs = new ScrollerViewChangedEventArgs(ViewChangedEvent, this)
        {
            HorizontalOffset = currentPosition.X,
            VerticalOffset = currentPosition.Y
        };
        RaiseEvent(changedArgs);

        _lastPosition = currentPosition;
    }
}

/// <summary>
/// Defines scroll modes.
/// </summary>
public enum ScrollMode
{
    Disabled,
    Enabled,
    Auto
}

/// <summary>
/// Defines chaining modes for scroll behavior.
/// </summary>
public enum ChainingMode
{
    Auto,
    Always,
    Never
}

/// <summary>
/// Defines railing modes for scroll behavior.
/// </summary>
public enum RailingMode
{
    Enabled,
    Disabled
}

/// <summary>
/// Defines zoom modes.
/// </summary>
public enum ZoomMode
{
    Disabled,
    Enabled
}

/// <summary>
/// Defines snap points types.
/// </summary>
public enum SnapPointsType
{
    None,
    Optional,
    Mandatory,
    OptionalSingle,
    MandatorySingle
}

/// <summary>
/// Event arguments for view changed event.
/// </summary>
public class ScrollerViewChangedEventArgs : RoutedEventArgs
{
    public ScrollerViewChangedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }

    public double HorizontalOffset { get; init; }
    public double VerticalOffset { get; init; }
}

/// <summary>
/// Event arguments for view changing event.
/// </summary>
public class ScrollerViewChangingEventArgs : RoutedEventArgs
{
    public ScrollerViewChangingEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }

    public double NextHorizontalOffset { get; init; }
    public double NextVerticalOffset { get; init; }
}
