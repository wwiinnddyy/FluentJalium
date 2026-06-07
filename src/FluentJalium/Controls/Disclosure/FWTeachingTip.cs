using System.Windows.Input;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Input;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium TeachingTip control for contextual learning experiences.
/// </summary>
public class FWTeachingTip : ContentControl, IFluentJaliumControl
{
    private Popup? _popup;
    private Button? _closeButton;
    private Button? _actionButton;

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(FWTeachingTip),
            new PropertyMetadata(false, OnIsOpenChanged));

    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(nameof(Target), typeof(FrameworkElement), typeof(FWTeachingTip),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(FWTeachingTip),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(FWTeachingTip),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconSourceProperty =
        DependencyProperty.Register(nameof(IconSource), typeof(object), typeof(FWTeachingTip),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ActionButtonContentProperty =
        DependencyProperty.Register(nameof(ActionButtonContent), typeof(object), typeof(FWTeachingTip),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ActionButtonCommandProperty =
        DependencyProperty.Register(nameof(ActionButtonCommand), typeof(ICommand), typeof(FWTeachingTip),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ActionButtonCommandParameterProperty =
        DependencyProperty.Register(nameof(ActionButtonCommandParameter), typeof(object), typeof(FWTeachingTip),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CloseButtonContentProperty =
        DependencyProperty.Register(nameof(CloseButtonContent), typeof(object), typeof(FWTeachingTip),
            new PropertyMetadata("Close"));

    public static readonly DependencyProperty IsLightDismissEnabledProperty =
        DependencyProperty.Register(nameof(IsLightDismissEnabled), typeof(bool), typeof(FWTeachingTip),
            new PropertyMetadata(true));

    public static readonly DependencyProperty PreferredPlacementProperty =
        DependencyProperty.Register(nameof(PreferredPlacement), typeof(TeachingTipPlacementMode), typeof(FWTeachingTip),
            new PropertyMetadata(TeachingTipPlacementMode.Auto));

    public static readonly DependencyProperty TailVisibilityProperty =
        DependencyProperty.Register(nameof(TailVisibility), typeof(TeachingTipTailVisibility), typeof(FWTeachingTip),
            new PropertyMetadata(TeachingTipTailVisibility.Auto));

    public static readonly DependencyProperty HeroContentProperty =
        DependencyProperty.Register(nameof(HeroContent), typeof(object), typeof(FWTeachingTip),
            new PropertyMetadata(null));

    public static readonly RoutedEvent ClosedEvent =
        EventManager.RegisterRoutedEvent(nameof(Closed), RoutingStrategy.Bubble,
            typeof(EventHandler<TeachingTipClosedEventArgs>), typeof(FWTeachingTip));

    public static readonly RoutedEvent ClosingEvent =
        EventManager.RegisterRoutedEvent(nameof(Closing), RoutingStrategy.Bubble,
            typeof(EventHandler<TeachingTipClosingEventArgs>), typeof(FWTeachingTip));

    public static readonly RoutedEvent ActionButtonClickEvent =
        EventManager.RegisterRoutedEvent(nameof(ActionButtonClick), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(FWTeachingTip));

    /// <summary>
    /// Initializes a new instance of the <see cref="FWTeachingTip"/> class.
    /// </summary>
    public FWTeachingTip()
    {
    }

    /// <summary>
    /// Gets or sets a value indicating whether the teaching tip is open.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty)!;
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets the target element to which the teaching tip is anchored.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public FrameworkElement? Target
    {
        get => (FrameworkElement?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>
    /// Gets or sets the title of the teaching tip.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string Title
    {
        get => (string)GetValue(TitleProperty)!;
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the subtitle of the teaching tip.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty)!;
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon displayed in the teaching tip.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the content of the action button.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? ActionButtonContent
    {
        get => GetValue(ActionButtonContentProperty);
        set => SetValue(ActionButtonContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the command for the action button.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public ICommand? ActionButtonCommand
    {
        get => (ICommand?)GetValue(ActionButtonCommandProperty);
        set => SetValue(ActionButtonCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command parameter for the action button.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? ActionButtonCommandParameter
    {
        get => GetValue(ActionButtonCommandParameterProperty);
        set => SetValue(ActionButtonCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the content of the close button.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object CloseButtonContent
    {
        get => GetValue(CloseButtonContentProperty)!;
        set => SetValue(CloseButtonContentProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the teaching tip can be dismissed by clicking outside.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public bool IsLightDismissEnabled
    {
        get => (bool)GetValue(IsLightDismissEnabledProperty)!;
        set => SetValue(IsLightDismissEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the preferred placement of the teaching tip relative to its target.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public TeachingTipPlacementMode PreferredPlacement
    {
        get => (TeachingTipPlacementMode)GetValue(PreferredPlacementProperty)!;
        set => SetValue(PreferredPlacementProperty, value);
    }

    /// <summary>
    /// Gets or sets the visibility of the tail pointer.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public TeachingTipTailVisibility TailVisibility
    {
        get => (TeachingTipTailVisibility)GetValue(TailVisibilityProperty)!;
        set => SetValue(TailVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the hero content displayed at the top of the teaching tip.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? HeroContent
    {
        get => GetValue(HeroContentProperty);
        set => SetValue(HeroContentProperty, value);
    }

    /// <summary>
    /// Occurs when the teaching tip is closed.
    /// </summary>
    public event EventHandler<TeachingTipClosedEventArgs> Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    /// <summary>
    /// Occurs when the teaching tip is closing.
    /// </summary>
    public event EventHandler<TeachingTipClosingEventArgs> Closing
    {
        add => AddHandler(ClosingEvent, value);
        remove => RemoveHandler(ClosingEvent, value);
    }

    /// <summary>
    /// Occurs when the action button is clicked.
    /// </summary>
    public event RoutedEventHandler ActionButtonClick
    {
        add => AddHandler(ActionButtonClickEvent, value);
        remove => RemoveHandler(ActionButtonClickEvent, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_closeButton != null)
            _closeButton.Click -= OnCloseButtonClick;
        if (_actionButton != null)
            _actionButton.Click -= OnActionButtonClick;

        _popup = GetTemplateChild("PART_Popup") as Popup;
        _closeButton = GetTemplateChild("PART_CloseButton") as Button;
        _actionButton = GetTemplateChild("PART_ActionButton") as Button;

        if (_closeButton != null)
            _closeButton.Click += OnCloseButtonClick;
        if (_actionButton != null)
            _actionButton.Click += OnActionButtonClick;

        UpdatePopupState();
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTeachingTip tip)
        {
            tip.UpdatePopupState();
        }
    }

    private void UpdatePopupState()
    {
        if (_popup != null)
        {
            _popup.IsOpen = IsOpen;

            if (IsOpen && Target != null)
            {
                _popup.PlacementTarget = Target;
                _popup.Placement = ConvertPlacement(PreferredPlacement);
            }
        }
    }

    private PlacementMode ConvertPlacement(TeachingTipPlacementMode mode)
    {
        return mode switch
        {
            TeachingTipPlacementMode.Top => PlacementMode.Top,
            TeachingTipPlacementMode.Bottom => PlacementMode.Bottom,
            TeachingTipPlacementMode.Left => PlacementMode.Left,
            TeachingTipPlacementMode.Right => PlacementMode.Right,
            TeachingTipPlacementMode.TopLeft => PlacementMode.Top,
            TeachingTipPlacementMode.TopRight => PlacementMode.Top,
            TeachingTipPlacementMode.BottomLeft => PlacementMode.Bottom,
            TeachingTipPlacementMode.BottomRight => PlacementMode.Bottom,
            TeachingTipPlacementMode.LeftTop => PlacementMode.Left,
            TeachingTipPlacementMode.LeftBottom => PlacementMode.Left,
            TeachingTipPlacementMode.RightTop => PlacementMode.Right,
            TeachingTipPlacementMode.RightBottom => PlacementMode.Right,
            TeachingTipPlacementMode.Center => PlacementMode.Center,
            _ => PlacementMode.Bottom
        };
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        var closingArgs = new TeachingTipClosingEventArgs(ClosingEvent, this)
        {
            Reason = TeachingTipCloseReason.CloseButton
        };
        RaiseEvent(closingArgs);

        if (!closingArgs.Cancel)
        {
            IsOpen = false;
            RaiseEvent(new TeachingTipClosedEventArgs(ClosedEvent, this)
            {
                Reason = TeachingTipCloseReason.CloseButton
            });
        }
    }

    private void OnActionButtonClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ActionButtonClickEvent, this));
    }
}

/// <summary>
/// Defines placement modes for teaching tips.
/// </summary>
public enum TeachingTipPlacementMode
{
    Auto,
    Top,
    Bottom,
    Left,
    Right,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    LeftTop,
    LeftBottom,
    RightTop,
    RightBottom,
    Center
}

/// <summary>
/// Defines tail visibility modes for teaching tips.
/// </summary>
public enum TeachingTipTailVisibility
{
    Auto,
    Visible,
    Collapsed
}

/// <summary>
/// Defines reasons for closing a teaching tip.
/// </summary>
public enum TeachingTipCloseReason
{
    CloseButton,
    LightDismiss,
    Programmatic
}

/// <summary>
/// Event arguments for teaching tip closed event.
/// </summary>
public class TeachingTipClosedEventArgs : RoutedEventArgs
{
    public TeachingTipClosedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }

    public TeachingTipCloseReason Reason { get; init; }
}

/// <summary>
/// Event arguments for teaching tip closing event.
/// </summary>
public class TeachingTipClosingEventArgs : RoutedEventArgs
{
    public TeachingTipClosingEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }

    public TeachingTipCloseReason Reason { get; init; }
    public bool Cancel { get; set; }
}
