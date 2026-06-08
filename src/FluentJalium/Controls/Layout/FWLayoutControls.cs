using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Input;
using Jalium.UI.Media;
using Jalium.UI.Media.Animation;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Input;
using AnimationDuration = Jalium.UI.Media.Animation.Duration;
using AnimationTransitionMode = Jalium.UI.Media.Animation.TransitionMode;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium content transition presets inspired by WinUI navigation motion.
/// </summary>
public enum FWContentTransitionProfile
{
    /// <summary>
    /// Uses a calm crossfade for ordinary content replacement.
    /// </summary>
    Default,

    /// <summary>
    /// Uses a slide-in entrance profile for forward navigation.
    /// </summary>
    Entrance,

    /// <summary>
    /// Uses a stronger zoom-in profile for drill-in navigation.
    /// </summary>
    DrillIn,

    /// <summary>
    /// Uses a slide-from-left profile for back navigation.
    /// </summary>
    BackNavigation,

    /// <summary>
    /// Uses Jalium's HLSL liquid morph transition for high-material surfaces.
    /// </summary>
    LiquidMorph,

    /// <summary>
    /// Disables content transition animation.
    /// </summary>
    Suppress
}

/// <summary>
/// Resolved FluentJalium content transition settings.
/// </summary>
public readonly record struct FWContentTransitionRecipe(
    FWContentTransitionProfile Profile,
    AnimationTransitionMode? TransitionMode,
    AnimationDuration Duration,
    TransitionTimingFunction TimingFunction)
{
    /// <summary>
    /// Creates a transition recipe for the supplied profile.
    /// </summary>
    public static FWContentTransitionRecipe Create(FWContentTransitionProfile profile)
    {
        return Create(profile, Application.Current?.Resources);
    }

    /// <summary>
    /// Creates a transition recipe for the supplied profile from a resource scope.
    /// </summary>
    public static FWContentTransitionRecipe Create(FWContentTransitionProfile profile, ResourceDictionary? resources)
    {
        if (!Enum.IsDefined(profile))
        {
            throw new ArgumentOutOfRangeException(nameof(profile), "Unknown content transition profile.");
        }

        return profile switch
        {
            FWContentTransitionProfile.Default => new(
                profile,
                ResourceTransitionMode(resources, "FluentMotionContentTransitionDefaultMode", AnimationTransitionMode.Crossfade),
                ResourceDuration(resources, "FluentMotionContentTransitionDefaultDuration", TimeSpan.FromMilliseconds(280)),
                ResourceTimingFunction(resources, "FluentMotionContentTransitionDefaultTimingFunction", TransitionTimingFunction.Recommended)),
            FWContentTransitionProfile.Entrance => new(
                profile,
                ResourceTransitionMode(resources, "FluentMotionContentTransitionEntranceMode", AnimationTransitionMode.SlideLeft),
                ResourceDuration(resources, "FluentMotionContentTransitionEntranceDuration", TimeSpan.FromMilliseconds(320)),
                ResourceTimingFunction(resources, "FluentMotionContentTransitionEntranceTimingFunction", TransitionTimingFunction.EaseOut)),
            FWContentTransitionProfile.DrillIn => new(
                profile,
                ResourceTransitionMode(resources, "FluentMotionContentTransitionDrillInMode", AnimationTransitionMode.ZoomIn),
                ResourceDuration(resources, "FluentMotionContentTransitionDrillInDuration", TimeSpan.FromMilliseconds(360)),
                ResourceTimingFunction(resources, "FluentMotionContentTransitionDrillInTimingFunction", TransitionTimingFunction.EaseInOut)),
            FWContentTransitionProfile.BackNavigation => new(
                profile,
                ResourceTransitionMode(resources, "FluentMotionContentTransitionBackNavigationMode", AnimationTransitionMode.SlideRight),
                ResourceDuration(resources, "FluentMotionContentTransitionBackNavigationDuration", TimeSpan.FromMilliseconds(280)),
                ResourceTimingFunction(resources, "FluentMotionContentTransitionBackNavigationTimingFunction", TransitionTimingFunction.EaseOut)),
            FWContentTransitionProfile.LiquidMorph => new(
                profile,
                ResourceTransitionMode(resources, "FluentMotionContentTransitionLiquidMorphMode", AnimationTransitionMode.LiquidMorph),
                ResourceDuration(resources, "FluentMotionContentTransitionLiquidMorphDuration", TimeSpan.FromMilliseconds(420)),
                ResourceTimingFunction(resources, "FluentMotionContentTransitionLiquidMorphTimingFunction", TransitionTimingFunction.EaseInOut)),
            FWContentTransitionProfile.Suppress => new(
                profile,
                ResourceTransitionMode(resources, "FluentMotionContentTransitionSuppressMode", null),
                ResourceDuration(resources, "FluentMotionContentTransitionSuppressDuration", TimeSpan.Zero),
                ResourceTimingFunction(resources, "FluentMotionContentTransitionSuppressTimingFunction", TransitionTimingFunction.Linear)),
            _ => throw new ArgumentOutOfRangeException(nameof(profile), "Unknown content transition profile.")
        };
    }

    private static AnimationDuration ResourceDuration(ResourceDictionary? resources, string key, TimeSpan fallback)
    {
        if (resources?.TryGetValue(key, out var value) != true)
        {
            return new AnimationDuration(fallback);
        }

        return value switch
        {
            AnimationDuration duration when duration.HasTimeSpan && duration.TimeSpan >= TimeSpan.Zero => duration,
            TimeSpan timeSpan when timeSpan >= TimeSpan.Zero => new AnimationDuration(timeSpan),
            double milliseconds when double.IsFinite(milliseconds) && milliseconds >= 0 => new AnimationDuration(TimeSpan.FromMilliseconds(milliseconds)),
            int milliseconds when milliseconds >= 0 => new AnimationDuration(TimeSpan.FromMilliseconds(milliseconds)),
            string text when TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var timeSpan) && timeSpan >= TimeSpan.Zero => new AnimationDuration(timeSpan),
            _ => new AnimationDuration(fallback)
        };
    }

    private static AnimationTransitionMode? ResourceTransitionMode(ResourceDictionary? resources, string key, AnimationTransitionMode? fallback)
    {
        if (resources?.TryGetValue(key, out var value) != true)
        {
            return fallback;
        }

        return value switch
        {
            null => null,
            AnimationTransitionMode mode => mode,
            string text when string.Equals(text, "None", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "Suppress", StringComparison.OrdinalIgnoreCase) => null,
            string text when Enum.TryParse<AnimationTransitionMode>(text, ignoreCase: true, out var mode) => mode,
            _ => fallback
        };
    }

    private static TransitionTimingFunction ResourceTimingFunction(ResourceDictionary? resources, string key, TransitionTimingFunction fallback)
    {
        if (resources?.TryGetValue(key, out var value) != true)
        {
            return fallback;
        }

        return value switch
        {
            TransitionTimingFunction timingFunction => timingFunction,
            string text when Enum.TryParse<TransitionTimingFunction>(text, ignoreCase: true, out var timingFunction) => timingFunction,
            _ => fallback
        };
    }
}

/// <summary>
/// FluentJalium TextBlock control.
/// </summary>
public class FWTextBlock : TextBlock, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium AccessText control.
/// </summary>
public class FWAccessText : AccessText, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Canvas control.
/// </summary>
public class FWCanvas : Canvas, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Border control.
/// </summary>
public class FWBorder : Border, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ContentControl control.
/// </summary>
public class FWContentControl : ContentControl, IFluentJaliumControl
{
}

/// <summary>
/// Describes how a <see cref="FWTwoPaneView"/> arranges its panes.
/// </summary>
public enum FWTwoPaneViewMode
{
    SinglePane,
    Wide,
    Tall
}

/// <summary>
/// Describes which pane remains visible when <see cref="FWTwoPaneView"/> is in single-pane mode.
/// </summary>
public enum FWTwoPaneViewPriority
{
    Pane1,
    Pane2
}

/// <summary>
/// Describes which pane is currently visible in a <see cref="FWTwoPaneView"/>.
/// </summary>
public enum FWTwoPaneViewVisiblePane
{
    Pane1,
    Pane2,
    Both
}

/// <summary>
/// Resolved FluentJalium TwoPaneView state useful for Gallery diagnostics and adaptive shells.
/// </summary>
public readonly record struct FWTwoPaneViewDiagnostics(
    FWTwoPaneViewMode RequestedMode,
    FWTwoPaneViewMode ActualMode,
    FWTwoPaneViewPriority PanePriority,
    FWTwoPaneViewVisiblePane VisiblePane,
    bool ShowsPane1,
    bool ShowsPane2,
    object? ActivePane,
    double MinWideModeWidth,
    double MinTallModeHeight);

/// <summary>
/// FluentJalium TwoPaneView control for foldable and adaptive master-detail layouts.
/// </summary>
public class FWTwoPaneView : Control, IFluentJaliumControl
{
    private Size _lastAvailableSize = new(double.PositiveInfinity, double.PositiveInfinity);

    public static readonly DependencyProperty Pane1Property =
        DependencyProperty.Register(nameof(Pane1), typeof(object), typeof(FWTwoPaneView),
            new PropertyMetadata(null, OnLayoutStateChanged));

    public static readonly DependencyProperty Pane1TemplateProperty =
        DependencyProperty.Register(nameof(Pane1Template), typeof(DataTemplate), typeof(FWTwoPaneView),
            new PropertyMetadata(null, OnLayoutStateChanged));

    public static readonly DependencyProperty Pane2Property =
        DependencyProperty.Register(nameof(Pane2), typeof(object), typeof(FWTwoPaneView),
            new PropertyMetadata(null, OnLayoutStateChanged));

    public static readonly DependencyProperty Pane2TemplateProperty =
        DependencyProperty.Register(nameof(Pane2Template), typeof(DataTemplate), typeof(FWTwoPaneView),
            new PropertyMetadata(null, OnLayoutStateChanged));

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(nameof(Mode), typeof(FWTwoPaneViewMode), typeof(FWTwoPaneView),
            new PropertyMetadata(FWTwoPaneViewMode.Wide, OnLayoutStateChanged), IsValidMode);

    public static readonly DependencyProperty PanePriorityProperty =
        DependencyProperty.Register(nameof(PanePriority), typeof(FWTwoPaneViewPriority), typeof(FWTwoPaneView),
            new PropertyMetadata(FWTwoPaneViewPriority.Pane1, OnLayoutStateChanged), IsValidPriority);

    public static readonly DependencyProperty MinWideModeWidthProperty =
        DependencyProperty.Register(nameof(MinWideModeWidth), typeof(double), typeof(FWTwoPaneView),
            new PropertyMetadata(641.0, OnLayoutStateChanged), IsValidNonNegativeDouble);

    public static readonly DependencyProperty MinTallModeHeightProperty =
        DependencyProperty.Register(nameof(MinTallModeHeight), typeof(double), typeof(FWTwoPaneView),
            new PropertyMetadata(641.0, OnLayoutStateChanged), IsValidNonNegativeDouble);

    private static readonly DependencyPropertyKey ActualModePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ActualMode), typeof(FWTwoPaneViewMode), typeof(FWTwoPaneView),
            new PropertyMetadata(FWTwoPaneViewMode.Wide));

    public static readonly DependencyProperty ActualModeProperty = ActualModePropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey VisiblePanePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(VisiblePane), typeof(FWTwoPaneViewVisiblePane), typeof(FWTwoPaneView),
            new PropertyMetadata(FWTwoPaneViewVisiblePane.Both));

    public static readonly DependencyProperty VisiblePaneProperty = VisiblePanePropertyKey.DependencyProperty;

    public FWTwoPaneView()
    {
        UpdateActualPaneState(_lastAvailableSize);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Pane1
    {
        get => GetValue(Pane1Property);
        set => SetValue(Pane1Property, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? Pane1Template
    {
        get => (DataTemplate?)GetValue(Pane1TemplateProperty);
        set => SetValue(Pane1TemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Pane2
    {
        get => GetValue(Pane2Property);
        set => SetValue(Pane2Property, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? Pane2Template
    {
        get => (DataTemplate?)GetValue(Pane2TemplateProperty);
        set => SetValue(Pane2TemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWTwoPaneViewMode Mode
    {
        get => (FWTwoPaneViewMode)GetValue(ModeProperty)!;
        set => SetValue(ModeProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWTwoPaneViewPriority PanePriority
    {
        get => (FWTwoPaneViewPriority)GetValue(PanePriorityProperty)!;
        set => SetValue(PanePriorityProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double MinWideModeWidth
    {
        get => (double)GetValue(MinWideModeWidthProperty)!;
        set => SetValue(MinWideModeWidthProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double MinTallModeHeight
    {
        get => (double)GetValue(MinTallModeHeightProperty)!;
        set => SetValue(MinTallModeHeightProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public FWTwoPaneViewMode ActualMode => (FWTwoPaneViewMode)GetValue(ActualModeProperty)!;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public FWTwoPaneViewVisiblePane VisiblePane => (FWTwoPaneViewVisiblePane)GetValue(VisiblePaneProperty)!;

    public object? ActivePane => VisiblePane == FWTwoPaneViewVisiblePane.Pane2 || PanePriority == FWTwoPaneViewPriority.Pane2 ? Pane2 : Pane1;

    public FWTwoPaneViewDiagnostics GetDiagnostics()
    {
        return new FWTwoPaneViewDiagnostics(
            Mode,
            ActualMode,
            PanePriority,
            VisiblePane,
            VisiblePane is FWTwoPaneViewVisiblePane.Pane1 or FWTwoPaneViewVisiblePane.Both,
            VisiblePane is FWTwoPaneViewVisiblePane.Pane2 or FWTwoPaneViewVisiblePane.Both,
            ActivePane,
            MinWideModeWidth,
            MinTallModeHeight);
    }

    private static void OnLayoutStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTwoPaneView twoPaneView)
        {
            twoPaneView.UpdateActualPaneState(twoPaneView._lastAvailableSize);
            twoPaneView.InvalidateMeasure();
            twoPaneView.InvalidateVisual();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _lastAvailableSize = availableSize;
        UpdateActualPaneState(availableSize);

        return base.MeasureOverride(availableSize);
    }

    private void UpdateActualPaneState(Size availableSize)
    {
        var actualMode = ResolveActualMode(availableSize);
        var visiblePane = actualMode == FWTwoPaneViewMode.SinglePane
            ? PanePriority == FWTwoPaneViewPriority.Pane2
                ? FWTwoPaneViewVisiblePane.Pane2
                : FWTwoPaneViewVisiblePane.Pane1
            : FWTwoPaneViewVisiblePane.Both;

        SetValue(ActualModePropertyKey.DependencyProperty, actualMode);
        SetValue(VisiblePanePropertyKey.DependencyProperty, visiblePane);
    }

    private FWTwoPaneViewMode ResolveActualMode(Size availableSize)
    {
        return Mode switch
        {
            FWTwoPaneViewMode.Wide when double.IsFinite(availableSize.Width) && availableSize.Width < MinWideModeWidth => FWTwoPaneViewMode.SinglePane,
            FWTwoPaneViewMode.Tall when double.IsFinite(availableSize.Height) && availableSize.Height < MinTallModeHeight => FWTwoPaneViewMode.SinglePane,
            _ => Mode
        };
    }

    private static bool IsValidMode(object? value)
    {
        return value is FWTwoPaneViewMode mode && Enum.IsDefined(mode);
    }

    private static bool IsValidPriority(object? value)
    {
        return value is FWTwoPaneViewPriority priority && Enum.IsDefined(priority);
    }

    private static bool IsValidNonNegativeDouble(object? value)
    {
        return value is double number && double.IsFinite(number) && number >= 0;
    }
}

/// <summary>
/// FluentJalium ParallaxView control for expressing scroll-linked depth.
/// </summary>
public class FWParallaxView : ContentControl, IFluentJaliumControl
{
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(object), typeof(FWParallaxView),
            new PropertyMetadata(null, OnParallaxSourceChanged));

    public static readonly DependencyProperty SourceOrientationProperty =
        DependencyProperty.Register(nameof(SourceOrientation), typeof(Orientation), typeof(FWParallaxView),
            new PropertyMetadata(Orientation.Vertical, OnParallaxPropertyChanged), IsValidOrientation);

    public static readonly DependencyProperty HorizontalShiftProperty =
        DependencyProperty.Register(nameof(HorizontalShift), typeof(double), typeof(FWParallaxView),
            new PropertyMetadata(0.0, OnParallaxPropertyChanged), IsValidFiniteDouble);

    public static readonly DependencyProperty VerticalShiftProperty =
        DependencyProperty.Register(nameof(VerticalShift), typeof(double), typeof(FWParallaxView),
            new PropertyMetadata(32.0, OnParallaxPropertyChanged), IsValidFiniteDouble);

    public static readonly DependencyProperty StartOffsetProperty =
        DependencyProperty.Register(nameof(StartOffset), typeof(double), typeof(FWParallaxView),
            new PropertyMetadata(0.0, OnParallaxPropertyChanged), IsValidFiniteDouble);

    public static readonly DependencyProperty EndOffsetProperty =
        DependencyProperty.Register(nameof(EndOffset), typeof(double), typeof(FWParallaxView),
            new PropertyMetadata(1.0, OnParallaxPropertyChanged), IsValidFiniteDouble);

    public static readonly DependencyProperty IsVerticalShiftEnabledProperty =
        DependencyProperty.Register(nameof(IsVerticalShiftEnabled), typeof(bool), typeof(FWParallaxView),
            new PropertyMetadata(true, OnParallaxPropertyChanged));

    public static readonly DependencyProperty IsHorizontalShiftEnabledProperty =
        DependencyProperty.Register(nameof(IsHorizontalShiftEnabled), typeof(bool), typeof(FWParallaxView),
            new PropertyMetadata(false, OnParallaxPropertyChanged));

    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(nameof(Progress), typeof(double), typeof(FWParallaxView),
            new PropertyMetadata(0.0, OnParallaxPropertyChanged), IsValidProgress);

    private static readonly DependencyPropertyKey CurrentOffsetPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CurrentOffset), typeof(Point), typeof(FWParallaxView),
            new PropertyMetadata(new Point(0, 0)));

    public static readonly DependencyProperty CurrentOffsetProperty = CurrentOffsetPropertyKey.DependencyProperty;

    private ContentPresenter? _contentHost;
    private TranslateTransform? _contentTransform;
    private ScrollViewer? _sourceScrollViewer;
    private FWScroller? _sourceScroller;

    public FWParallaxView()
    {
        UseTemplateContentManagement();
        UpdateCurrentOffset();
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public object? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public Orientation SourceOrientation
    {
        get => (Orientation)GetValue(SourceOrientationProperty)!;
        set => SetValue(SourceOrientationProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double HorizontalShift
    {
        get => (double)GetValue(HorizontalShiftProperty)!;
        set => SetValue(HorizontalShiftProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double VerticalShift
    {
        get => (double)GetValue(VerticalShiftProperty)!;
        set => SetValue(VerticalShiftProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double StartOffset
    {
        get => (double)GetValue(StartOffsetProperty)!;
        set => SetValue(StartOffsetProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double EndOffset
    {
        get => (double)GetValue(EndOffsetProperty)!;
        set => SetValue(EndOffsetProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsVerticalShiftEnabled
    {
        get => (bool)GetValue(IsVerticalShiftEnabledProperty)!;
        set => SetValue(IsVerticalShiftEnabledProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsHorizontalShiftEnabled
    {
        get => (bool)GetValue(IsHorizontalShiftEnabledProperty)!;
        set => SetValue(IsHorizontalShiftEnabledProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public double Progress
    {
        get => (double)GetValue(ProgressProperty)!;
        set => SetValue(ProgressProperty, Math.Clamp(value, 0.0, 1.0));
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public Point CurrentOffset => (Point)GetValue(CurrentOffsetProperty)!;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public bool IsSourceAttached => _sourceScrollViewer != null || _sourceScroller != null;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public string SourceKind =>
        _sourceScroller != null ? nameof(FWScroller) :
        _sourceScrollViewer != null ? nameof(ScrollViewer) :
        Source is null ? "None" : Source.GetType().Name;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _contentHost = GetTemplateChild("PART_ContentHost") as ContentPresenter;
        EnsureContentTransform();
        UpdateContentTransform(CurrentOffset);
    }

    public Point GetParallaxOffset(double progress)
    {
        var normalized = Math.Clamp(progress, 0.0, 1.0);
        var span = EndOffset - StartOffset;
        var position = StartOffset + span * normalized;
        return new Point(
            IsHorizontalShiftEnabled ? HorizontalShift * position : 0.0,
            IsVerticalShiftEnabled ? VerticalShift * position : 0.0);
    }

    public FWParallaxViewDiagnostics GetDiagnostics()
    {
        return new FWParallaxViewDiagnostics(
            Source is not null,
            Progress,
            CurrentOffset,
            HorizontalShift,
            VerticalShift,
            StartOffset,
            EndOffset,
            IsHorizontalShiftEnabled,
            IsVerticalShiftEnabled,
            IsSourceAttached,
            SourceKind,
            SourceOrientation);
    }

    public void RefreshProgressFromSource()
    {
        if (_sourceScroller != null)
        {
            RefreshProgressFromMetrics(
                SourceOrientation == Orientation.Horizontal ? _sourceScroller.HorizontalOffset : _sourceScroller.VerticalOffset,
                SourceOrientation == Orientation.Horizontal ? _sourceScroller.ViewportWidth : _sourceScroller.ViewportHeight,
                SourceOrientation == Orientation.Horizontal ? _sourceScroller.ExtentWidth : _sourceScroller.ExtentHeight);
            return;
        }

        if (_sourceScrollViewer != null)
        {
            RefreshProgressFromMetrics(
                SourceOrientation == Orientation.Horizontal ? _sourceScrollViewer.HorizontalOffset : _sourceScrollViewer.VerticalOffset,
                SourceOrientation == Orientation.Horizontal ? _sourceScrollViewer.ViewportWidth : _sourceScrollViewer.ViewportHeight,
                SourceOrientation == Orientation.Horizontal ? _sourceScrollViewer.ExtentWidth : _sourceScrollViewer.ExtentHeight);
        }
    }

    private static void OnParallaxSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWParallaxView parallaxView)
        {
            parallaxView.DetachSource();
            parallaxView.AttachSource(e.NewValue);
            parallaxView.RefreshProgressFromSource();
            parallaxView.UpdateCurrentOffset();
            parallaxView.InvalidateMeasure();
            parallaxView.InvalidateVisual();
        }
    }

    private static void OnParallaxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWParallaxView parallaxView)
        {
            if (e.Property == SourceOrientationProperty)
            {
                parallaxView.RefreshProgressFromSource();
            }

            parallaxView.UpdateCurrentOffset();
            parallaxView.InvalidateMeasure();
            parallaxView.InvalidateVisual();
        }
    }

    private void UpdateCurrentOffset()
    {
        var offset = GetParallaxOffset(Progress);
        SetValue(CurrentOffsetPropertyKey.DependencyProperty, offset);
        UpdateContentTransform(offset);
    }

    private void AttachSource(object? source)
    {
        if (source is FWScroller scroller)
        {
            _sourceScroller = scroller;
            _sourceScroller.ViewChanged += OnScrollerViewChanged;
            return;
        }

        if (source is ScrollViewer scrollViewer)
        {
            _sourceScrollViewer = scrollViewer;
            _sourceScrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
        }
    }

    private void DetachSource()
    {
        if (_sourceScroller != null)
        {
            _sourceScroller.ViewChanged -= OnScrollerViewChanged;
            _sourceScroller = null;
        }

        if (_sourceScrollViewer != null)
        {
            _sourceScrollViewer.ScrollChanged -= OnScrollViewerScrollChanged;
            _sourceScrollViewer = null;
        }
    }

    private void OnScrollerViewChanged(object? sender, ScrollerViewChangedEventArgs e)
    {
        if (ReferenceEquals(sender, _sourceScroller))
        {
            RefreshProgressFromSource();
        }
    }

    private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (ReferenceEquals(sender, _sourceScrollViewer))
        {
            RefreshProgressFromSource();
        }
    }

    private void RefreshProgressFromMetrics(double offset, double viewportLength, double extentLength)
    {
        var scrollableLength = extentLength - viewportLength;
        Progress = scrollableLength <= 0 || !double.IsFinite(scrollableLength)
            ? 0
            : Math.Clamp(offset / scrollableLength, 0.0, 1.0);
    }

    private void EnsureContentTransform()
    {
        if (_contentHost is null)
        {
            return;
        }

        _contentTransform = _contentHost.RenderTransform as TranslateTransform;
        if (_contentTransform is null)
        {
            _contentTransform = new TranslateTransform();
            _contentHost.RenderTransform = _contentTransform;
        }
    }

    private void UpdateContentTransform(Point offset)
    {
        EnsureContentTransform();

        if (_contentTransform is not null)
        {
            _contentTransform.X = offset.X;
            _contentTransform.Y = offset.Y;
        }
    }

    private static bool IsValidFiniteDouble(object? value)
    {
        return value is double number && double.IsFinite(number);
    }

    private static bool IsValidProgress(object? value)
    {
        return value is double number && double.IsFinite(number) && number >= 0.0 && number <= 1.0;
    }

    private static bool IsValidOrientation(object? value)
    {
        return value is Orientation orientation && Enum.IsDefined(orientation);
    }
}

/// <summary>
/// Resolved FluentJalium ParallaxView state useful for Gallery diagnostics and visual QA.
/// </summary>
public readonly record struct FWParallaxViewDiagnostics(
    bool HasSource,
    double Progress,
    Point CurrentOffset,
    double HorizontalShift,
    double VerticalShift,
    double StartOffset,
    double EndOffset,
    bool IsHorizontalShiftEnabled,
    bool IsVerticalShiftEnabled,
    bool IsSourceAttached,
    string SourceKind,
    Orientation SourceOrientation);

/// <summary>
/// FluentJalium RelativePanel control for arranging children relative to the panel or to sibling elements.
/// </summary>
public class FWRelativePanel : Panel, IFluentJaliumControl
{
    public static readonly DependencyProperty RowSpacingProperty =
        DependencyProperty.Register(nameof(RowSpacing), typeof(double), typeof(FWRelativePanel),
            new PropertyMetadata(0.0, OnPanelLayoutPropertyChanged), IsValidSpacing);

    public static readonly DependencyProperty ColumnSpacingProperty =
        DependencyProperty.Register(nameof(ColumnSpacing), typeof(double), typeof(FWRelativePanel),
            new PropertyMetadata(0.0, OnPanelLayoutPropertyChanged), IsValidSpacing);

    public static readonly DependencyProperty AlignLeftWithPanelProperty =
        RegisterBoolAttached("AlignLeftWithPanel");

    public static readonly DependencyProperty AlignTopWithPanelProperty =
        RegisterBoolAttached("AlignTopWithPanel");

    public static readonly DependencyProperty AlignRightWithPanelProperty =
        RegisterBoolAttached("AlignRightWithPanel");

    public static readonly DependencyProperty AlignBottomWithPanelProperty =
        RegisterBoolAttached("AlignBottomWithPanel");

    public static readonly DependencyProperty AlignHorizontalCenterWithPanelProperty =
        RegisterBoolAttached("AlignHorizontalCenterWithPanel");

    public static readonly DependencyProperty AlignVerticalCenterWithPanelProperty =
        RegisterBoolAttached("AlignVerticalCenterWithPanel");

    public static readonly DependencyProperty RightOfProperty =
        RegisterTargetAttached("RightOf");

    public static readonly DependencyProperty LeftOfProperty =
        RegisterTargetAttached("LeftOf");

    public static readonly DependencyProperty BelowProperty =
        RegisterTargetAttached("Below");

    public static readonly DependencyProperty AboveProperty =
        RegisterTargetAttached("Above");

    public static readonly DependencyProperty AlignLeftWithProperty =
        RegisterTargetAttached("AlignLeftWith");

    public static readonly DependencyProperty AlignTopWithProperty =
        RegisterTargetAttached("AlignTopWith");

    public static readonly DependencyProperty AlignRightWithProperty =
        RegisterTargetAttached("AlignRightWith");

    public static readonly DependencyProperty AlignBottomWithProperty =
        RegisterTargetAttached("AlignBottomWith");

    public static readonly DependencyProperty AlignHorizontalCenterWithProperty =
        RegisterTargetAttached("AlignHorizontalCenterWith");

    public static readonly DependencyProperty AlignVerticalCenterWithProperty =
        RegisterTargetAttached("AlignVerticalCenterWith");

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double RowSpacing
    {
        get => (double)GetValue(RowSpacingProperty)!;
        set => SetValue(RowSpacingProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double ColumnSpacing
    {
        get => (double)GetValue(ColumnSpacingProperty)!;
        set => SetValue(ColumnSpacingProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static bool GetAlignLeftWithPanel(UIElement element) => GetBoolAttached(element, AlignLeftWithPanelProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetAlignLeftWithPanel(UIElement element, bool value) => SetBoolAttached(element, AlignLeftWithPanelProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static bool GetAlignTopWithPanel(UIElement element) => GetBoolAttached(element, AlignTopWithPanelProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetAlignTopWithPanel(UIElement element, bool value) => SetBoolAttached(element, AlignTopWithPanelProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static bool GetAlignRightWithPanel(UIElement element) => GetBoolAttached(element, AlignRightWithPanelProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetAlignRightWithPanel(UIElement element, bool value) => SetBoolAttached(element, AlignRightWithPanelProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static bool GetAlignBottomWithPanel(UIElement element) => GetBoolAttached(element, AlignBottomWithPanelProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetAlignBottomWithPanel(UIElement element, bool value) => SetBoolAttached(element, AlignBottomWithPanelProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static bool GetAlignHorizontalCenterWithPanel(UIElement element) => GetBoolAttached(element, AlignHorizontalCenterWithPanelProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetAlignHorizontalCenterWithPanel(UIElement element, bool value) => SetBoolAttached(element, AlignHorizontalCenterWithPanelProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static bool GetAlignVerticalCenterWithPanel(UIElement element) => GetBoolAttached(element, AlignVerticalCenterWithPanelProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetAlignVerticalCenterWithPanel(UIElement element, bool value) => SetBoolAttached(element, AlignVerticalCenterWithPanelProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static object? GetRightOf(UIElement element) => element.GetValue(RightOfProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetRightOf(UIElement element, object? value) => element.SetValue(RightOfProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static object? GetLeftOf(UIElement element) => element.GetValue(LeftOfProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetLeftOf(UIElement element, object? value) => element.SetValue(LeftOfProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static object? GetBelow(UIElement element) => element.GetValue(BelowProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetBelow(UIElement element, object? value) => element.SetValue(BelowProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static object? GetAbove(UIElement element) => element.GetValue(AboveProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetAbove(UIElement element, object? value) => element.SetValue(AboveProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static object? GetAlignLeftWith(UIElement element) => element.GetValue(AlignLeftWithProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetAlignLeftWith(UIElement element, object? value) => element.SetValue(AlignLeftWithProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static object? GetAlignTopWith(UIElement element) => element.GetValue(AlignTopWithProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetAlignTopWith(UIElement element, object? value) => element.SetValue(AlignTopWithProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static object? GetAlignRightWith(UIElement element) => element.GetValue(AlignRightWithProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetAlignRightWith(UIElement element, object? value) => element.SetValue(AlignRightWithProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static object? GetAlignBottomWith(UIElement element) => element.GetValue(AlignBottomWithProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetAlignBottomWith(UIElement element, object? value) => element.SetValue(AlignBottomWithProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static object? GetAlignHorizontalCenterWith(UIElement element) => element.GetValue(AlignHorizontalCenterWithProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetAlignHorizontalCenterWith(UIElement element, object? value) => element.SetValue(AlignHorizontalCenterWithProperty, value);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static object? GetAlignVerticalCenterWith(UIElement element) => element.GetValue(AlignVerticalCenterWithProperty);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public static void SetAlignVerticalCenterWith(UIElement element, object? value) => element.SetValue(AlignVerticalCenterWithProperty, value);

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var child in Children)
        {
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }

        var bounds = ResolveLayout(Size.Empty);
        var desiredWidth = 0.0;
        var desiredHeight = 0.0;
        foreach (var rect in bounds.Values)
        {
            desiredWidth = Math.Max(desiredWidth, rect.Right);
            desiredHeight = Math.Max(desiredHeight, rect.Bottom);
        }

        return new Size(desiredWidth, desiredHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var bounds = ResolveLayout(finalSize);
        foreach (var child in Children)
        {
            if (bounds.TryGetValue(child, out var rect))
            {
                child.Arrange(rect);
            }
        }

        return finalSize;
    }

    private static DependencyProperty RegisterBoolAttached(string name)
    {
        return DependencyProperty.RegisterAttached(name, typeof(bool), typeof(FWRelativePanel),
            new PropertyMetadata(false, OnRelativeConstraintChanged));
    }

    private static DependencyProperty RegisterTargetAttached(string name)
    {
        return DependencyProperty.RegisterAttached(name, typeof(object), typeof(FWRelativePanel),
            new PropertyMetadata(null, OnRelativeConstraintChanged));
    }

    private static bool GetBoolAttached(UIElement element, DependencyProperty property)
    {
        return (bool)(element.GetValue(property) ?? false);
    }

    private static void SetBoolAttached(UIElement element, DependencyProperty property, bool value)
    {
        element.SetValue(property, value);
    }

    private static void OnPanelLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRelativePanel panel)
        {
            panel.InvalidateMeasure();
            panel.InvalidateVisual();
        }
    }

    private static void OnRelativeConstraintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && element.VisualParent is FWRelativePanel panel)
        {
            panel.InvalidateMeasure();
            panel.InvalidateVisual();
        }
    }

    private static bool IsValidSpacing(object? value)
    {
        return value is double number && double.IsFinite(number) && number >= 0;
    }

    private Dictionary<UIElement, Rect> ResolveLayout(Size panelSize)
    {
        var bounds = new Dictionary<UIElement, Rect>();
        var unresolved = new List<UIElement>(Children);
        var passCount = Math.Max(1, unresolved.Count);

        for (var pass = 0; pass < passCount && unresolved.Count > 0; pass++)
        {
            for (var index = unresolved.Count - 1; index >= 0; index--)
            {
                var child = unresolved[index];
                if (TryResolveChild(child, panelSize, bounds, out var rect))
                {
                    bounds[child] = rect;
                    unresolved.RemoveAt(index);
                }
            }
        }

        foreach (var child in unresolved)
        {
            bounds[child] = CreateFallbackRect(child);
        }

        return bounds;
    }

    private bool TryResolveChild(
        UIElement child,
        Size panelSize,
        IReadOnlyDictionary<UIElement, Rect> resolved,
        out Rect rect)
    {
        var desired = child.DesiredSize;
        var x = 0.0;
        var y = 0.0;

        if (GetAlignLeftWithPanel(child))
        {
            x = 0;
        }
        else if (GetAlignRightWithPanel(child))
        {
            x = Math.Max(0, panelSize.Width - desired.Width);
        }
        else if (GetAlignHorizontalCenterWithPanel(child))
        {
            x = Math.Max(0, (panelSize.Width - desired.Width) / 2.0);
        }
        else if (!TryResolveHorizontalTarget(child, desired, resolved, ref x))
        {
            rect = default;
            return false;
        }

        if (GetAlignTopWithPanel(child))
        {
            y = 0;
        }
        else if (GetAlignBottomWithPanel(child))
        {
            y = Math.Max(0, panelSize.Height - desired.Height);
        }
        else if (GetAlignVerticalCenterWithPanel(child))
        {
            y = Math.Max(0, (panelSize.Height - desired.Height) / 2.0);
        }
        else if (!TryResolveVerticalTarget(child, desired, resolved, ref y))
        {
            rect = default;
            return false;
        }

        rect = new Rect(x, y, desired.Width, desired.Height);
        return true;
    }

    private bool TryResolveHorizontalTarget(
        UIElement child,
        Size desired,
        IReadOnlyDictionary<UIElement, Rect> resolved,
        ref double x)
    {
        if (!TryResolveTargetRect(GetRightOf(child), resolved, out var target, out var hasTarget))
        {
            return false;
        }

        if (hasTarget)
        {
            x = target.Right + ColumnSpacing;
            return true;
        }

        if (!TryResolveTargetRect(GetLeftOf(child), resolved, out target, out hasTarget))
        {
            return false;
        }

        if (hasTarget)
        {
            x = target.Left - ColumnSpacing - desired.Width;
            return true;
        }

        if (!TryResolveTargetRect(GetAlignLeftWith(child), resolved, out target, out hasTarget))
        {
            return false;
        }

        if (hasTarget)
        {
            x = target.Left;
            return true;
        }

        if (!TryResolveTargetRect(GetAlignRightWith(child), resolved, out target, out hasTarget))
        {
            return false;
        }

        if (hasTarget)
        {
            x = target.Right - desired.Width;
            return true;
        }

        if (!TryResolveTargetRect(GetAlignHorizontalCenterWith(child), resolved, out target, out hasTarget))
        {
            return false;
        }

        if (hasTarget)
        {
            x = target.Left + (target.Width - desired.Width) / 2.0;
        }

        return true;
    }

    private bool TryResolveVerticalTarget(
        UIElement child,
        Size desired,
        IReadOnlyDictionary<UIElement, Rect> resolved,
        ref double y)
    {
        if (!TryResolveTargetRect(GetBelow(child), resolved, out var target, out var hasTarget))
        {
            return false;
        }

        if (hasTarget)
        {
            y = target.Bottom + RowSpacing;
            return true;
        }

        if (!TryResolveTargetRect(GetAbove(child), resolved, out target, out hasTarget))
        {
            return false;
        }

        if (hasTarget)
        {
            y = target.Top - RowSpacing - desired.Height;
            return true;
        }

        if (!TryResolveTargetRect(GetAlignTopWith(child), resolved, out target, out hasTarget))
        {
            return false;
        }

        if (hasTarget)
        {
            y = target.Top;
            return true;
        }

        if (!TryResolveTargetRect(GetAlignBottomWith(child), resolved, out target, out hasTarget))
        {
            return false;
        }

        if (hasTarget)
        {
            y = target.Bottom - desired.Height;
            return true;
        }

        if (!TryResolveTargetRect(GetAlignVerticalCenterWith(child), resolved, out target, out hasTarget))
        {
            return false;
        }

        if (hasTarget)
        {
            y = target.Top + (target.Height - desired.Height) / 2.0;
        }

        return true;
    }

    private bool TryResolveTargetRect(
        object? target,
        IReadOnlyDictionary<UIElement, Rect> resolved,
        out Rect rect,
        out bool hasTarget)
    {
        rect = default;
        if (target == null)
        {
            hasTarget = false;
            return true;
        }

        hasTarget = true;
        var element = ResolveTargetElement(target);
        if (element == null)
        {
            hasTarget = false;
            return true;
        }

        return resolved.TryGetValue(element, out rect);
    }

    private UIElement? ResolveTargetElement(object target)
    {
        if (target is UIElement element)
        {
            return Children.Contains(element) ? element : null;
        }

        if (target is string name && !string.IsNullOrWhiteSpace(name))
        {
            return Children.OfType<FrameworkElement>().FirstOrDefault(child => child.Name == name);
        }

        return null;
    }

    private static Rect CreateFallbackRect(UIElement child)
    {
        var desired = child.DesiredSize;
        return new Rect(0, 0, desired.Width, desired.Height);
    }
}

/// <summary>
/// Lightweight state snapshot for <see cref="FWSettingsCard"/> interaction diagnostics.
/// </summary>
public readonly record struct FWSettingsCardDiagnostics(
    bool IsClickEnabled,
    bool IsEnabled,
    bool CanExecute,
    bool IsInvokable,
    bool IsPointerPressed,
    bool IsKeyboardPressed,
    bool IsInteractionPressed,
    bool HasCommand,
    ClickMode ClickMode);

/// <summary>
/// Lightweight state snapshot for <see cref="FWSettingsCard"/> automation metadata.
/// </summary>
public readonly record struct FWSettingsCardAutomationDiagnostics(
    string ClassName,
    AutomationControlType ControlType,
    string Name,
    string HelpText,
    bool IsInvokePatternAvailable,
    bool IsKeyboardFocusable);

/// <summary>
/// FluentJalium SettingsCard control for compact settings rows.
/// </summary>
public class FWSettingsCard : ContentControl, IFluentJaliumControl
{
    private bool _hasCommandCanExecuteOverride;
    private bool _isEnabledBeforeCommandCanExecute = true;

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty DescriptionTemplateProperty =
        DependencyProperty.Register(nameof(DescriptionTemplate), typeof(DataTemplate), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register(nameof(HeaderIcon), typeof(object), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty ActionIconProperty =
        DependencyProperty.Register(nameof(ActionIcon), typeof(object), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty IsClickEnabledProperty =
        DependencyProperty.Register(nameof(IsClickEnabled), typeof(bool), typeof(FWSettingsCard),
            new PropertyMetadata(false, OnIsClickEnabledChanged));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnCommandChanged));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnCommandParameterChanged));

    public static readonly DependencyProperty CommandTargetProperty =
        DependencyProperty.Register(nameof(CommandTarget), typeof(IInputElement), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnCommandParameterChanged));

    private static readonly DependencyPropertyKey CanExecutePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CanExecute), typeof(bool), typeof(FWSettingsCard),
            new PropertyMetadata(true));

    public static readonly DependencyProperty CanExecuteProperty = CanExecutePropertyKey.DependencyProperty;

    public static readonly DependencyProperty ClickModeProperty =
        DependencyProperty.Register(nameof(ClickMode), typeof(ClickMode), typeof(FWSettingsCard),
            new PropertyMetadata(ClickMode.Release));

    private static readonly DependencyPropertyKey IsPointerPressedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsPointerPressed), typeof(bool), typeof(FWSettingsCard),
            new PropertyMetadata(false, OnInteractionStateChanged));

    public static readonly DependencyProperty IsPointerPressedProperty = IsPointerPressedPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey IsKeyboardPressedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsKeyboardPressed), typeof(bool), typeof(FWSettingsCard),
            new PropertyMetadata(false, OnInteractionStateChanged));

    public static readonly DependencyProperty IsKeyboardPressedProperty = IsKeyboardPressedPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey IsInteractionPressedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsInteractionPressed), typeof(bool), typeof(FWSettingsCard),
            new PropertyMetadata(false, OnInteractionPressedChanged));

    public static readonly DependencyProperty IsInteractionPressedProperty = IsInteractionPressedPropertyKey.DependencyProperty;

    public static readonly RoutedEvent ClickEvent =
        EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(FWSettingsCard));

    public FWSettingsCard()
    {
        UseTemplateContentManagement();
        Focusable = false;
        AddHandler(MouseDownEvent, new MouseButtonEventHandler(OnMouseDownHandler));
        AddHandler(MouseUpEvent, new MouseButtonEventHandler(OnMouseUpHandler));
        AddHandler(MouseEnterEvent, new MouseEventHandler(OnMouseEnterHandler));
        AddHandler(MouseLeaveEvent, new MouseEventHandler(OnMouseLeaveHandler));
        AddHandler(KeyDownEvent, new KeyEventHandler(OnKeyDownHandler));
        AddHandler(KeyUpEvent, new KeyEventHandler(OnKeyUpHandler));
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? DescriptionTemplate
    {
        get => (DataTemplate?)GetValue(DescriptionTemplateProperty);
        set => SetValue(DescriptionTemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? ActionIcon
    {
        get => GetValue(ActionIconProperty);
        set => SetValue(ActionIconProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsClickEnabled
    {
        get => (bool)GetValue(IsClickEnabledProperty)!;
        set => SetValue(IsClickEnabledProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public IInputElement? CommandTarget
    {
        get => (IInputElement?)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool CanExecute
    {
        get => (bool)GetValue(CanExecuteProperty)!;
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public ClickMode ClickMode
    {
        get => (ClickMode)GetValue(ClickModeProperty)!;
        set => SetValue(ClickModeProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public bool IsPointerPressed => (bool)GetValue(IsPointerPressedProperty)!;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public bool IsKeyboardPressed => (bool)GetValue(IsKeyboardPressedProperty)!;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public bool IsInteractionPressed => (bool)GetValue(IsInteractionPressedProperty)!;

    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    public bool Invoke()
    {
        if (!CanInvoke())
        {
            return false;
        }

        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        ExecuteCommand();
        return true;
    }

    public bool PerformClick()
    {
        return Invoke();
    }

    public FWSettingsCardDiagnostics GetDiagnostics()
    {
        return new FWSettingsCardDiagnostics(
            IsClickEnabled,
            IsEnabled,
            CanExecute,
            CanInvoke(),
            IsPointerPressed,
            IsKeyboardPressed,
            IsInteractionPressed,
            Command != null,
            ClickMode);
    }

    public FWSettingsCardAutomationDiagnostics GetAutomationDiagnostics()
    {
        var peer = GetAutomationPeer();
        return new FWSettingsCardAutomationDiagnostics(
            peer?.GetClassName() ?? nameof(FWSettingsCard),
            peer?.GetAutomationControlType() ?? AutomationControlType.Button,
            peer?.GetName() ?? ResolveAutomationName(this),
            peer?.GetHelpText() ?? ResolveAutomationHelpText(this),
            peer?.GetPattern(PatternInterface.Invoke) is IInvokeProvider,
            peer?.IsKeyboardFocusable() ?? Focusable);
    }

    protected override AutomationPeer? OnCreateAutomationPeer()
    {
        return new FWSettingsCardAutomationPeer(this);
    }

    private static void OnSettingsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSettingsCard card)
        {
            card.InvalidateMeasure();
            card.InvalidateVisual();
        }
    }

    private static void OnIsClickEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSettingsCard card)
        {
            card.Focusable = card.IsClickEnabled;
            card.CoerceInteractionState();
            card.InvalidateVisual();
        }
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FWSettingsCard card)
        {
            return;
        }

        if (e.OldValue is ICommand oldCommand)
        {
            oldCommand.CanExecuteChanged -= card.OnCanExecuteChanged;
        }

        if (e.NewValue is ICommand newCommand)
        {
            newCommand.CanExecuteChanged += card.OnCanExecuteChanged;
        }

        card.UpdateCanExecute();
    }

    private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSettingsCard card)
        {
            card.UpdateCanExecute();
        }
    }

    private static void OnInteractionStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSettingsCard card)
        {
            card.UpdateInteractionPressed();
            card.InvalidateVisual();
        }
    }

    private static void OnInteractionPressedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSettingsCard card)
        {
            card.InvalidateVisual();
        }
    }

    private void OnCanExecuteChanged(object? sender, EventArgs e)
    {
        UpdateCanExecute();
    }

    private void UpdateCanExecute()
    {
        var command = Command;
        var canExecute = command == null || CanExecuteCommand(command);
        SetValue(CanExecutePropertyKey.DependencyProperty, canExecute);

        if (!canExecute)
        {
            if (!_hasCommandCanExecuteOverride)
            {
                _isEnabledBeforeCommandCanExecute = IsEnabled;
                _hasCommandCanExecuteOverride = true;
            }

            IsEnabled = false;
            CoerceInteractionState();
            return;
        }

        if (_hasCommandCanExecuteOverride)
        {
            IsEnabled = _isEnabledBeforeCommandCanExecute;
            _hasCommandCanExecuteOverride = false;
        }

        CoerceInteractionState();
    }

    private bool CanInvoke()
    {
        if (!IsClickEnabled || !IsEnabled)
        {
            return false;
        }

        var command = Command;
        return command == null || CanExecuteCommand(command);
    }

    private void SetPointerPressed(bool value)
    {
        if (IsPointerPressed != value)
        {
            SetValue(IsPointerPressedPropertyKey.DependencyProperty, value);
        }
    }

    private void SetKeyboardPressed(bool value)
    {
        if (IsKeyboardPressed != value)
        {
            SetValue(IsKeyboardPressedPropertyKey.DependencyProperty, value);
        }
    }

    private void UpdateInteractionPressed()
    {
        var value = IsEnabled && IsClickEnabled && CanExecute && (IsPointerPressed || IsKeyboardPressed);
        if (IsInteractionPressed != value)
        {
            SetValue(IsInteractionPressedPropertyKey.DependencyProperty, value);
        }
    }

    private void CoerceInteractionState()
    {
        if (!IsEnabled || !IsClickEnabled || !CanExecute)
        {
            SetPointerPressed(false);
            SetKeyboardPressed(false);
            ReleaseMouseCapture();
            return;
        }

        UpdateInteractionPressed();
    }

    private bool CanExecuteCommand(ICommand command)
    {
        var parameter = CommandParameter;
        if (command is RoutedCommand routedCommand)
        {
            return routedCommand.CanExecute(parameter, CommandTarget ?? this);
        }

        return command.CanExecute(parameter);
    }

    private void ExecuteCommand()
    {
        var command = Command;
        if (command == null)
        {
            return;
        }

        var parameter = CommandParameter;
        if (command is RoutedCommand routedCommand)
        {
            var target = CommandTarget ?? this;
            if (routedCommand.CanExecute(parameter, target))
            {
                routedCommand.Execute(parameter, target);
            }
        }
        else if (command.CanExecute(parameter))
        {
            command.Execute(parameter);
        }
    }

    private void OnMouseDownHandler(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || !CanInvoke())
        {
            return;
        }

        SetPointerPressed(true);
        CaptureMouse();
        Focus();

        if (ClickMode == ClickMode.Press)
        {
            Invoke();
        }

        e.Handled = true;
    }

    private void OnMouseUpHandler(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        var wasPressed = IsPointerPressed;
        SetPointerPressed(false);
        ReleaseMouseCapture();

        if (wasPressed && IsMouseOver && ClickMode == ClickMode.Release)
        {
            Invoke();
            e.Handled = true;
        }
    }

    private void OnMouseLeaveHandler(object sender, MouseEventArgs e)
    {
        if (!IsPointerPressed)
        {
            return;
        }

        SetPointerPressed(false);
        ReleaseMouseCapture();
    }

    private void OnMouseEnterHandler(object sender, MouseEventArgs e)
    {
        if (ClickMode == ClickMode.Hover && CanInvoke())
        {
            Invoke();
        }
    }

    private void OnKeyDownHandler(object sender, KeyEventArgs e)
    {
        if (!CanInvoke())
        {
            return;
        }

        if (e.Key == Key.Space)
        {
            SetKeyboardPressed(true);
            if (ClickMode == ClickMode.Press)
            {
                Invoke();
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            Invoke();
            e.Handled = true;
        }
    }

    private void OnKeyUpHandler(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Space || !IsKeyboardPressed)
        {
            return;
        }

        SetKeyboardPressed(false);
        if (ClickMode == ClickMode.Release)
        {
            Invoke();
        }

        e.Handled = true;
    }

    protected override void OnLostMouseCapture()
    {
        base.OnLostMouseCapture();
        SetPointerPressed(false);
    }

    protected override void OnIsKeyboardFocusedChanged(bool isFocused)
    {
        base.OnIsKeyboardFocusedChanged(isFocused);

        if (!isFocused)
        {
            SetKeyboardPressed(false);
        }
    }

    internal static string ResolveAutomationName(FWSettingsCard card)
    {
        var header = ResolveAutomationText(card.Header);
        if (!string.IsNullOrWhiteSpace(header))
        {
            return header;
        }

        var content = ResolveAutomationText(card.Content);
        return string.IsNullOrWhiteSpace(content) ? nameof(FWSettingsCard) : content;
    }

    internal static string ResolveAutomationHelpText(FWSettingsCard card)
    {
        var description = ResolveAutomationText(card.Description);
        if (!string.IsNullOrWhiteSpace(description))
        {
            return description;
        }

        var content = ResolveAutomationText(card.Content);
        return string.IsNullOrWhiteSpace(content) ? "Settings card" : content;
    }

    private static string ResolveAutomationText(object? value)
    {
        if (value is string text)
        {
            return string.IsNullOrWhiteSpace(text) ? string.Empty : text;
        }

        var resolved = value?.ToString();
        return string.IsNullOrWhiteSpace(resolved) ? string.Empty : resolved;
    }
}

/// <summary>
/// Automation peer for <see cref="FWSettingsCard"/>.
/// </summary>
public sealed class FWSettingsCardAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider
{
    public FWSettingsCardAutomationPeer(FWSettingsCard owner) : base(owner)
    {
    }

    private FWSettingsCard SettingsCardOwner => (FWSettingsCard)Owner;

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return SettingsCardOwner.IsClickEnabled ? AutomationControlType.Button : AutomationControlType.Group;
    }

    protected override string GetClassNameCore()
    {
        return nameof(FWSettingsCard);
    }

    protected override string GetNameCore()
    {
        return FWSettingsCard.ResolveAutomationName(SettingsCardOwner);
    }

    protected override string GetHelpTextCore()
    {
        return FWSettingsCard.ResolveAutomationHelpText(SettingsCardOwner);
    }

    protected override object? GetPatternCore(PatternInterface patternInterface)
    {
        return patternInterface == PatternInterface.Invoke && SettingsCardOwner.IsClickEnabled
            ? this
            : base.GetPatternCore(patternInterface);
    }

    public void Invoke()
    {
        if (!SettingsCardOwner.Invoke())
        {
            throw new InvalidOperationException("Cannot invoke a disabled settings card.");
        }
    }
}

/// <summary>
/// FluentJalium SettingsExpander control for grouped settings rows.
/// </summary>
[ContentProperty(nameof(Items))]
public class FWSettingsExpander : Expander, IFluentJaliumControl
{
    private readonly ObservableCollection<object> _items = new();
    private ItemsControl? _itemsControl;

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(FWSettingsExpander),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty DescriptionTemplateProperty =
        DependencyProperty.Register(nameof(DescriptionTemplate), typeof(DataTemplate), typeof(FWSettingsExpander),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register(nameof(HeaderIcon), typeof(object), typeof(FWSettingsExpander),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(FWSettingsExpander),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(FWSettingsExpander),
            new PropertyMetadata(null, OnItemsPresentationChanged));

    public static readonly DependencyProperty ItemsPanelProperty =
        DependencyProperty.Register(nameof(ItemsPanel), typeof(ItemsPanelTemplate), typeof(FWSettingsExpander),
            new PropertyMetadata(null, OnItemsPresentationChanged));

    private static readonly DependencyPropertyKey ItemCountPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ItemCount), typeof(int), typeof(FWSettingsExpander),
            new PropertyMetadata(0));

    public static readonly DependencyProperty ItemCountProperty = ItemCountPropertyKey.DependencyProperty;

    public static readonly DependencyProperty SettingsContentProperty =
        DependencyProperty.Register(nameof(SettingsContent), typeof(object), typeof(FWSettingsExpander),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty SettingsContentTemplateProperty =
        DependencyProperty.Register(nameof(SettingsContentTemplate), typeof(DataTemplate), typeof(FWSettingsExpander),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public FWSettingsExpander()
    {
        _items.CollectionChanged += OnItemsCollectionChanged;
        UpdateItemCount();
    }

    public event NotifyCollectionChangedEventHandler? ItemsChanged;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? DescriptionTemplate
    {
        get => (DataTemplate?)GetValue(DescriptionTemplateProperty);
        set => SetValue(DescriptionTemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public IList<object> Items => _items;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public ItemsPanelTemplate? ItemsPanel
    {
        get => (ItemsPanelTemplate?)GetValue(ItemsPanelProperty);
        set => SetValue(ItemsPanelProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public int ItemCount
    {
        get => (int)GetValue(ItemCountProperty)!;
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? SettingsContent
    {
        get => GetValue(SettingsContentProperty);
        set => SetValue(SettingsContentProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? SettingsContentTemplate
    {
        get => (DataTemplate?)GetValue(SettingsContentTemplateProperty);
        set => SetValue(SettingsContentTemplateProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _itemsControl = GetTemplateChild("PART_ItemsControl") as ItemsControl;
        UpdateItemsControl();
    }

    public void AddSetting(object item)
    {
        _items.Add(item);
    }

    public bool RemoveSetting(object item)
    {
        return _items.Remove(item);
    }

    public void ClearSettings()
    {
        _items.Clear();
    }

    private static void OnSettingsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSettingsExpander expander)
        {
            expander.InvalidateMeasure();
            expander.InvalidateVisual();
        }
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSettingsExpander expander)
        {
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= expander.OnItemsCollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += expander.OnItemsCollectionChanged;
            }

            expander.UpdateItemsControl();
            expander.ItemsChanged?.Invoke(expander, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    private static void OnItemsPresentationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSettingsExpander expander)
        {
            expander.UpdateItemsControl();
        }
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateItemsControl();
        ItemsChanged?.Invoke(this, e);
    }

    private void UpdateItemsControl()
    {
        UpdateItemCount();

        if (_itemsControl == null)
        {
            InvalidateMeasure();
            return;
        }

        _itemsControl.ItemsSource = ItemsSource ?? _items;
        _itemsControl.ItemTemplate = ItemTemplate;
        _itemsControl.ItemsPanel = ItemsPanel;
        InvalidateMeasure();
        InvalidateVisual();
    }

    private void UpdateItemCount()
    {
        SetValue(ItemCountPropertyKey.DependencyProperty, CountItems(ItemsSource ?? _items));
    }

    private static int CountItems(IEnumerable items)
    {
        if (items is ICollection collection)
        {
            return collection.Count;
        }

        if (items is IReadOnlyCollection<object> readOnlyCollection)
        {
            return readOnlyCollection.Count;
        }

        var count = 0;
        foreach (var _ in items)
        {
            count++;
        }

        return count;
    }
}

/// <summary>
/// FluentJalium TransitioningContentControl control.
/// </summary>
public class FWTransitioningContentControl : TransitioningContentControl, IFluentJaliumControl
{
    private bool _applyingTransitionRecipe;

    public static readonly DependencyProperty TransitionProfileProperty =
        DependencyProperty.Register(nameof(TransitionProfile), typeof(FWContentTransitionProfile), typeof(FWTransitioningContentControl),
            new PropertyMetadata(FWContentTransitionProfile.Default, OnTransitionProfileChanged), IsValidTransitionProfile);

    public FWTransitioningContentControl()
    {
        ApplyTransitionRecipeCore(FWContentTransitionRecipe.Create(FWContentTransitionProfile.Default), updateProfile: false, useCurrentValue: true);
    }

    /// <summary>
    /// Gets or sets the FluentJalium transition profile applied to content changes.
    /// </summary>
    public FWContentTransitionProfile TransitionProfile
    {
        get => (FWContentTransitionProfile)GetValue(TransitionProfileProperty)!;
        set => SetValue(TransitionProfileProperty, value);
    }

    /// <summary>
    /// Applies the supplied FluentJalium content transition profile.
    /// </summary>
    public void ApplyTransitionProfile(FWContentTransitionProfile profile)
    {
        ApplyTransitionRecipe(FWContentTransitionRecipe.Create(profile));
    }

    /// <summary>
    /// Applies the supplied FluentJalium content transition profile from a resource scope.
    /// </summary>
    public void ApplyTransitionProfile(FWContentTransitionProfile profile, ResourceDictionary? resources)
    {
        ApplyTransitionRecipe(FWContentTransitionRecipe.Create(profile, resources));
    }

    /// <summary>
    /// Applies an already resolved FluentJalium content transition recipe.
    /// </summary>
    public void ApplyTransitionRecipe(FWContentTransitionRecipe recipe)
    {
        ApplyTransitionRecipeCore(recipe, updateProfile: true, useCurrentValue: false);
    }

    private void ApplyTransitionRecipeCore(FWContentTransitionRecipe recipe, bool updateProfile, bool useCurrentValue)
    {
        _applyingTransitionRecipe = true;
        try
        {
            if (useCurrentValue)
            {
                SetCurrentValue(TransitionModeProperty, recipe.TransitionMode);
                SetCurrentValue(TransitionDurationProperty, recipe.Duration);
                SetCurrentValue(TransitionTimingFunctionProperty, recipe.TimingFunction);
                if (updateProfile)
                {
                    SetCurrentValue(TransitionProfileProperty, recipe.Profile);
                }
            }
            else
            {
                TransitionMode = recipe.TransitionMode;
                TransitionDuration = recipe.Duration;
                TransitionTimingFunction = recipe.TimingFunction;
                if (updateProfile)
                {
                    TransitionProfile = recipe.Profile;
                }
            }
        }
        finally
        {
            _applyingTransitionRecipe = false;
        }
    }

    private static void OnTransitionProfileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTransitioningContentControl control &&
            !control._applyingTransitionRecipe &&
            e.NewValue is FWContentTransitionProfile profile)
        {
            control.ApplyTransitionRecipeCore(FWContentTransitionRecipe.Create(profile), updateProfile: false, useCurrentValue: true);
        }
    }

    private static bool IsValidTransitionProfile(object? value)
    {
        return value is FWContentTransitionProfile profile && Enum.IsDefined(profile);
    }
}

/// <summary>
/// FluentJalium ContentPresenter control.
/// </summary>
public class FWContentPresenter : ContentPresenter, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium StackPanel control.
/// </summary>
public class FWStackPanel : StackPanel, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium WrapPanel control.
/// </summary>
public class FWWrapPanel : WrapPanel, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Grid control.
/// </summary>
public class FWGrid : Grid, IFluentJaliumControl
{
}
