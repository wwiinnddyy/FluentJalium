using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Input;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// Snapshot of the pull-to-refresh state hosted by <see cref="FWRefreshContainer"/>.
/// </summary>
public readonly record struct FWRefreshContainerDiagnostics(
    RefreshPullDirection PullDirection,
    bool IsRefreshing,
    bool IsPulling,
    double PullDistance,
    double PullThreshold,
    double MaxPullDistance,
    double PullProgress,
    bool HasScrollViewer,
    bool HasRefreshVisualizerBorder,
    bool HasRefreshIndicator,
    bool HasCustomVisualizer,
    RefreshVisualizerState VisualizerState);

/// <summary>
/// FluentJalium RefreshContainer control for pull-to-refresh functionality.
/// </summary>
public class FWRefreshContainer : ContentControl, IFluentJaliumControl
{
    private ScrollViewer? _scrollViewer;
    private Border? _refreshVisualizerBorder;
    private Control? _refreshIndicator; // Changed from ProgressRing to Control
    private double _pullDistance;
    private bool _isRefreshing;
    private Point _startPoint;
    private bool _isPulling;

    public static readonly DependencyProperty PullDirectionProperty =
        DependencyProperty.Register(nameof(PullDirection), typeof(RefreshPullDirection), typeof(FWRefreshContainer),
            new PropertyMetadata(RefreshPullDirection.TopToBottom));

    public static readonly DependencyProperty VisualizerProperty =
        DependencyProperty.Register(nameof(Visualizer), typeof(RefreshVisualizer), typeof(FWRefreshContainer),
            new PropertyMetadata(null));

    public static readonly RoutedEvent RefreshRequestedEvent =
        EventManager.RegisterRoutedEvent(nameof(RefreshRequested), RoutingStrategy.Bubble,
            typeof(EventHandler<RefreshRequestedEventArgs>), typeof(FWRefreshContainer));

    private const double PullThreshold = 100.0;
    private const double MaxPullDistance = 150.0;

    /// <summary>
    /// Initializes a new instance of the <see cref="FWRefreshContainer"/> class.
    /// </summary>
    public FWRefreshContainer()
    {
        AddHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown), true);
        AddHandler(UIElement.MouseMoveEvent, new MouseEventHandler(OnMouseMove), true);
        AddHandler(UIElement.MouseUpEvent, new MouseButtonEventHandler(OnMouseUp), true);
    }

    /// <summary>
    /// Gets or sets the pull direction for the refresh gesture.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public RefreshPullDirection PullDirection
    {
        get => (RefreshPullDirection)GetValue(PullDirectionProperty)!;
        set => SetValue(PullDirectionProperty, value);
    }

    /// <summary>
    /// Gets or sets the custom refresh visualizer.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public RefreshVisualizer? Visualizer
    {
        get => (RefreshVisualizer?)GetValue(VisualizerProperty);
        set => SetValue(VisualizerProperty, value);
    }

    /// <summary>
    /// Occurs when a refresh is requested.
    /// </summary>
    public event EventHandler<RefreshRequestedEventArgs> RefreshRequested
    {
        add => AddHandler(RefreshRequestedEvent, value);
        remove => RemoveHandler(RefreshRequestedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
        _refreshVisualizerBorder = GetTemplateChild("PART_RefreshVisualizerBorder") as Border;
        _refreshIndicator = GetTemplateChild("PART_RefreshIndicator") as Control;

        if (_refreshVisualizerBorder != null)
        {
            _refreshVisualizerBorder.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Programmatically triggers a refresh.
    /// </summary>
    public void RequestRefresh()
    {
        if (!_isRefreshing)
        {
            StartRefresh();
        }
    }

    /// <summary>
    /// Gets a snapshot of the current pull-to-refresh state.
    /// </summary>
    public FWRefreshContainerDiagnostics GetDiagnostics()
    {
        return new FWRefreshContainerDiagnostics(
            PullDirection,
            _isRefreshing,
            _isPulling,
            _pullDistance,
            PullThreshold,
            MaxPullDistance,
            Math.Clamp(_pullDistance / PullThreshold, 0, 1),
            _scrollViewer != null,
            _refreshVisualizerBorder != null,
            _refreshIndicator != null,
            Visualizer != null,
            Visualizer?.State ?? RefreshVisualizerState.Idle);
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_isRefreshing || _scrollViewer == null)
            return;

        // Check if we're at the top of the scroll viewer
        if (PullDirection == RefreshPullDirection.TopToBottom && _scrollViewer.VerticalOffset == 0)
        {
            _startPoint = e.GetPosition(this);
            _isPulling = true;
            _pullDistance = 0;
            CaptureMouse();
        }
        else if (PullDirection == RefreshPullDirection.BottomToTop &&
                 _scrollViewer.VerticalOffset >= _scrollViewer.ScrollableHeight)
        {
            _startPoint = e.GetPosition(this);
            _isPulling = true;
            _pullDistance = 0;
            CaptureMouse();
        }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isPulling || _isRefreshing)
            return;

        var currentPoint = e.GetPosition(this);
        double delta = 0;

        if (PullDirection == RefreshPullDirection.TopToBottom)
        {
            delta = currentPoint.Y - _startPoint.Y;
        }
        else if (PullDirection == RefreshPullDirection.BottomToTop)
        {
            delta = _startPoint.Y - currentPoint.Y;
        }

        if (delta > 0)
        {
            _pullDistance = Math.Min(delta, MaxPullDistance);
            UpdateVisualizerPosition(_pullDistance);
        }
        else
        {
            _pullDistance = 0;
            ResetVisualizer();
        }
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isPulling)
            return;

        _isPulling = false;
        ReleaseMouseCapture();

        if (_pullDistance >= PullThreshold && !_isRefreshing)
        {
            StartRefresh();
        }
        else
        {
            ResetVisualizer();
        }
    }

    private void UpdateVisualizerPosition(double distance)
    {
        if (_refreshVisualizerBorder == null)
            return;

        _refreshVisualizerBorder.Visibility = Visibility.Visible;

        // Calculate opacity based on pull distance
        double progress = Math.Min(distance / PullThreshold, 1.0);
        _refreshVisualizerBorder.Opacity = progress;

        // Update position
        var transform = new TranslateTransform(0, PullDirection == RefreshPullDirection.TopToBottom ? distance : -distance);
        _refreshVisualizerBorder.RenderTransform = transform;

        // Update visualizer state
        if (Visualizer != null)
        {
            Visualizer.UpdateProgress(progress);
        }
        else if (_refreshIndicator != null)
        {
            // Control doesn't have IsIndeterminate or Value properties
            // These would be set if the actual control supports them
        }
    }

    private void ResetVisualizer()
    {
        if (_refreshVisualizerBorder == null)
            return;

        _refreshVisualizerBorder.RenderTransform = null;
        _refreshVisualizerBorder.Visibility = Visibility.Collapsed;
        _pullDistance = 0;
    }

    private void StartRefresh()
    {
        _isRefreshing = true;

        if (_refreshVisualizerBorder != null)
        {
            _refreshVisualizerBorder.Visibility = Visibility.Visible;
            _refreshVisualizerBorder.Opacity = 1.0;
        }

        if (_refreshIndicator != null)
        {
            // Control doesn't have IsIndeterminate property
        }

        if (Visualizer != null)
        {
            Visualizer.State = RefreshVisualizerState.Refreshing;
        }

        var deferral = new RefreshRequestedDeferral(() => CompleteRefresh());
        var args = new RefreshRequestedEventArgs(RefreshRequestedEvent, this, deferral);
        RaiseEvent(args);

        // If no one took the deferral, complete immediately
        if (!args.DeferralTaken)
        {
            CompleteRefresh();
        }
    }

    private void CompleteRefresh()
    {
        _isRefreshing = false;
        ResetVisualizer();

        if (Visualizer != null)
        {
            Visualizer.State = RefreshVisualizerState.Idle;
        }
    }
}

/// <summary>
/// Defines the pull direction for refresh.
/// </summary>
public enum RefreshPullDirection
{
    TopToBottom,
    BottomToTop,
    LeftToRight,
    RightToLeft
}

/// <summary>
/// Event arguments for refresh requested event.
/// </summary>
public class RefreshRequestedEventArgs : RoutedEventArgs
{
    private readonly RefreshRequestedDeferral _deferral;

    public RefreshRequestedEventArgs(RoutedEvent routedEvent, object source, RefreshRequestedDeferral deferral)
        : base(routedEvent, source)
    {
        _deferral = deferral;
    }

    internal bool DeferralTaken { get; private set; }

    /// <summary>
    /// Gets a deferral to allow async refresh operations.
    /// </summary>
    public RefreshRequestedDeferral GetDeferral()
    {
        DeferralTaken = true;
        return _deferral;
    }
}

/// <summary>
/// Deferral for async refresh operations.
/// </summary>
public class RefreshRequestedDeferral
{
    private readonly Action _completeAction;
    private bool _completed;

    internal RefreshRequestedDeferral(Action completeAction)
    {
        _completeAction = completeAction;
    }

    /// <summary>
    /// Signals that the refresh operation is complete.
    /// </summary>
    public void Complete()
    {
        if (!_completed)
        {
            _completed = true;
            _completeAction();
        }
    }
}

/// <summary>
/// Base class for custom refresh visualizers.
/// </summary>
public abstract class RefreshVisualizer : Control
{
    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(RefreshVisualizerState), typeof(RefreshVisualizer),
            new PropertyMetadata(RefreshVisualizerState.Idle, OnStateChanged));

    /// <summary>
    /// Gets or sets the current state of the visualizer.
    /// </summary>
    public RefreshVisualizerState State
    {
        get => (RefreshVisualizerState)GetValue(StateProperty)!;
        set => SetValue(StateProperty, value);
    }

    /// <summary>
    /// Updates the visualizer progress during pull gesture.
    /// </summary>
    public abstract void UpdateProgress(double progress);

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RefreshVisualizer visualizer && e.NewValue is RefreshVisualizerState state)
        {
            visualizer.OnStateChanged(state);
        }
    }

    protected virtual void OnStateChanged(RefreshVisualizerState newState)
    {
    }
}

/// <summary>
/// Defines the state of a refresh visualizer.
/// </summary>
public enum RefreshVisualizerState
{
    Idle,
    Peeking,
    Interacting,
    Pending,
    Refreshing
}
