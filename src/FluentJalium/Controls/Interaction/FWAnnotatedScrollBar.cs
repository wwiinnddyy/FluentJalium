using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// Snapshot of the annotation state hosted by <see cref="FWAnnotatedScrollBar"/>.
/// </summary>
public readonly record struct FWAnnotatedScrollBarDiagnostics(
    bool HasDetailsCanvas,
    int SourceLabelCount,
    int RegisteredLabelCount,
    bool HasLabels,
    Orientation Orientation,
    double Minimum,
    double Maximum,
    double Value,
    double ViewportSize,
    double TrackLength,
    double LastRequestedScrollOffset,
    object? LastRequestedContent,
    ScrollBarLabelType? LastRequestedLabelType);

/// <summary>
/// FluentJalium AnnotatedScrollBar control for enhanced scroll visualization.
/// </summary>
public class FWAnnotatedScrollBar : ScrollBar, IFluentJaliumControl
{
    private Canvas? _detailsCanvas;
    private readonly List<ScrollBarLabel> _labels = new();
    private double _lastRequestedScrollOffset;
    private object? _lastRequestedContent;
    private ScrollBarLabelType? _lastRequestedLabelType;

    public static readonly DependencyProperty LabelsProperty =
        DependencyProperty.Register(nameof(Labels), typeof(IList<ScrollBarLabel>), typeof(FWAnnotatedScrollBar),
            new PropertyMetadata(null, OnLabelsChanged));

    public static readonly RoutedEvent DetailLabelRequestedEvent =
        EventManager.RegisterRoutedEvent(nameof(DetailLabelRequested), RoutingStrategy.Bubble,
            typeof(EventHandler<DetailLabelRequestedEventArgs>), typeof(FWAnnotatedScrollBar));

    /// <summary>
    /// Initializes a new instance of the <see cref="FWAnnotatedScrollBar"/> class.
    /// </summary>
    public FWAnnotatedScrollBar()
    {
    }

    /// <summary>
    /// Gets or sets the collection of scroll bar labels.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public IList<ScrollBarLabel>? Labels
    {
        get => (IList<ScrollBarLabel>?)GetValue(LabelsProperty);
        set => SetValue(LabelsProperty, value);
    }

    /// <summary>
    /// Occurs when a detail label is requested.
    /// </summary>
    public event EventHandler<DetailLabelRequestedEventArgs> DetailLabelRequested
    {
        add => AddHandler(DetailLabelRequestedEvent, value);
        remove => RemoveHandler(DetailLabelRequestedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _detailsCanvas = GetTemplateChild("PART_DetailsCanvas") as Canvas;
        UpdateLabels();
    }

    /// <summary>
    /// Gets a snapshot of the current annotation and scroll state.
    /// </summary>
    public FWAnnotatedScrollBarDiagnostics GetDiagnostics()
    {
        var sourceLabelCount = Labels?.Count ?? 0;
        var trackLength = Orientation == Orientation.Vertical ? ActualHeight : ActualWidth;

        return new FWAnnotatedScrollBarDiagnostics(
            _detailsCanvas != null,
            sourceLabelCount,
            _labels.Count,
            sourceLabelCount > 0,
            Orientation,
            Minimum,
            Maximum,
            Value,
            ViewportSize,
            trackLength,
            _lastRequestedScrollOffset,
            _lastRequestedContent,
            _lastRequestedLabelType);
    }

    private static void OnLabelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWAnnotatedScrollBar scrollBar)
        {
            scrollBar.UpdateLabels();
        }
    }

    private void UpdateLabels()
    {
        _labels.Clear();
        _detailsCanvas?.Children.Clear();

        if (Labels == null)
            return;

        foreach (var label in Labels)
        {
            _labels.Add(label);

            var element = CreateLabelElement(label);
            if (_detailsCanvas != null && element != null)
            {
                _detailsCanvas.Children.Add(element);
            }
        }
    }

    private FrameworkElement? CreateLabelElement(ScrollBarLabel label)
    {
        var border = new Border
        {
            Background = label.Background ?? new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4)),
            Width = 4,
            Height = 8,
            CornerRadius = new CornerRadius(2),
            ToolTip = label.Content
        };

        // Position based on scroll offset
        var position = CalculateLabelPosition(label.ScrollOffset);
        Canvas.SetTop(border, position);
        Canvas.SetLeft(border, Orientation == Orientation.Vertical ? 0 : position);

        return border;
    }

    private double CalculateLabelPosition(double scrollOffset)
    {
        if (Maximum <= 0)
            return 0;

        var trackLength = Orientation == Orientation.Vertical ? ActualHeight : ActualWidth;
        var ratio = scrollOffset / Maximum;
        return ratio * trackLength;
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);

        // Check if we need to show detail label
        foreach (var label in _labels)
        {
            if (Math.Abs(newValue - label.ScrollOffset) < 10)
            {
                _lastRequestedScrollOffset = label.ScrollOffset;
                _lastRequestedContent = label.Content;
                _lastRequestedLabelType = label.Type;

                var args = new DetailLabelRequestedEventArgs(DetailLabelRequestedEvent, this)
                {
                    ScrollOffset = label.ScrollOffset,
                    Content = label.Content,
                    LabelType = label.Type
                };
                RaiseEvent(args);
                break;
            }
        }
    }
}

/// <summary>
/// Represents a label for an annotated scroll bar.
/// </summary>
public class ScrollBarLabel
{
    /// <summary>
    /// Gets or sets the scroll offset where this label appears.
    /// </summary>
    public double ScrollOffset { get; set; }

    /// <summary>
    /// Gets or sets the content displayed for this label.
    /// </summary>
    public object? Content { get; set; }

    /// <summary>
    /// Gets or sets the background brush for the label indicator.
    /// </summary>
    public Brush? Background { get; set; }

    /// <summary>
    /// Gets or sets the label type.
    /// </summary>
    public ScrollBarLabelType Type { get; set; } = ScrollBarLabelType.Default;
}

/// <summary>
/// Defines types of scroll bar labels.
/// </summary>
public enum ScrollBarLabelType
{
    Default,
    Warning,
    Error,
    Info
}

/// <summary>
/// Event arguments for detail label requested event.
/// </summary>
public class DetailLabelRequestedEventArgs : RoutedEventArgs
{
    public DetailLabelRequestedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }

    /// <summary>
    /// Gets the scroll offset of the label.
    /// </summary>
    public double ScrollOffset { get; init; }

    /// <summary>
    /// Gets the content of the label.
    /// </summary>
    public object? Content { get; init; }

    /// <summary>
    /// Gets the type of label that requested detail content.
    /// </summary>
    public ScrollBarLabelType LabelType { get; init; }
}
