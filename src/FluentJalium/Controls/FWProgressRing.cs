using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium ring progress control.
/// </summary>
public class FWProgressRing : RangeBase, IFluentJaliumControl
{
    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(FWProgressRing),
            new PropertyMetadata(true, OnAnimationPropertyChanged));

    public static readonly DependencyProperty IsIndeterminateProperty =
        DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(FWProgressRing),
            new PropertyMetadata(true, OnAnimationPropertyChanged));

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(FWProgressRing),
            new PropertyMetadata(4.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty ProgressBrushProperty =
        DependencyProperty.Register(nameof(ProgressBrush), typeof(Brush), typeof(FWProgressRing),
            new PropertyMetadata(null, OnVisualPropertyChanged));

    private const double MinimumArcDegrees = 42.0;
    private const double IndeterminateArcDegrees = 96.0;
    private double _animationAngle;
    private bool _isAnimationSubscribed;
    private long _lastAnimationTickMs;

    public FWProgressRing()
    {
        Width = 32;
        Height = 32;
        Focusable = false;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty)!;
        set => SetValue(IsActiveProperty, value);
    }

    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty)!;
        set => SetValue(IsIndeterminateProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty)!;
        set => SetValue(StrokeThicknessProperty, value);
    }

    public Brush? ProgressBrush
    {
        get => (Brush?)GetValue(ProgressBrushProperty);
        set => SetValue(ProgressBrushProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var width = double.IsInfinity(availableSize.Width) ? Width : availableSize.Width;
        var height = double.IsInfinity(availableSize.Height) ? Height : availableSize.Height;

        if (double.IsNaN(width) || width <= 0)
            width = 32;
        if (double.IsNaN(height) || height <= 0)
            height = 32;

        return new Size(Math.Min(width, 64), Math.Min(height, 64));
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (!IsActive || RenderSize.Width <= 0 || RenderSize.Height <= 0)
            return;

        var stroke = Math.Clamp(StrokeThickness, 1.0, Math.Min(RenderSize.Width, RenderSize.Height) / 2.0);
        var radius = Math.Max(0, (Math.Min(RenderSize.Width, RenderSize.Height) - stroke) / 2.0);
        if (radius <= 0)
            return;

        var center = new Point(RenderSize.Width / 2.0, RenderSize.Height / 2.0);
        var trackBrush = ResolveTrackBrush();
        if (trackBrush != null)
        {
            drawingContext.DrawEllipse(null, new Pen(trackBrush, stroke), center, radius, radius);
        }

        var arcStart = IsIndeterminate ? _animationAngle : -90.0;
        var arcSweep = IsIndeterminate ? IndeterminateArcDegrees : GetDeterminateSweep();
        if (arcSweep <= 0)
            return;

        DrawArc(drawingContext, new Pen(ResolveProgressBrush(), stroke), center, radius, arcStart, arcSweep);
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        InvalidateVisual();
    }

    protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
    {
        base.OnMinimumChanged(oldMinimum, newMinimum);
        InvalidateVisual();
    }

    protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
    {
        base.OnMaximumChanged(oldMaximum, newMaximum);
        InvalidateVisual();
    }

    private double GetDeterminateSweep()
    {
        var range = Maximum - Minimum;
        if (range <= 0)
            return 0;

        var progress = Math.Clamp((Value - Minimum) / range, 0.0, 1.0);
        if (progress <= 0)
            return 0;

        return Math.Max(MinimumArcDegrees, 360.0 * progress);
    }

    private Brush ResolveProgressBrush()
    {
        return ProgressBrush
            ?? Foreground
            ?? TryFindResource("ProgressRingForeground") as Brush
            ?? TryFindResource("AccentBrush") as Brush
            ?? new SolidColorBrush(FluentThemeManager.DefaultAccentColor);
    }

    private Brush? ResolveTrackBrush()
    {
        return Background ?? TryFindResource("SliderTrack") as Brush;
    }

    private static void DrawArc(DrawingContext dc, Pen pen, Point center, double radius, double startAngle, double sweepAngle)
    {
        var steps = Math.Max(6, (int)Math.Ceiling(Math.Abs(sweepAngle) / 8.0));
        var previous = PointOnCircle(center, radius, startAngle);

        for (var i = 1; i <= steps; i++)
        {
            var angle = startAngle + (sweepAngle * i / steps);
            var current = PointOnCircle(center, radius, angle);
            dc.DrawLine(pen, previous, current);
            previous = current;
        }
    }

    private static Point PointOnCircle(Point center, double radius, double angleDegrees)
    {
        var radians = Math.PI * angleDegrees / 180.0;
        return new Point(
            center.X + (Math.Cos(radians) * radius),
            center.Y + (Math.Sin(radians) * radius));
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        UpdateAnimationSubscription();
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        StopAnimation();
    }

    private void UpdateAnimationSubscription()
    {
        if (IsActive && IsIndeterminate)
        {
            StartAnimation();
        }
        else
        {
            StopAnimation();
        }

        InvalidateVisual();
    }

    private void StartAnimation()
    {
        if (_isAnimationSubscribed)
            return;

        _lastAnimationTickMs = Environment.TickCount64;
        CompositionTarget.Rendering += OnRendering;
        CompositionTarget.Subscribe();
        _isAnimationSubscribed = true;
    }

    private void StopAnimation()
    {
        if (!_isAnimationSubscribed)
            return;

        CompositionTarget.Rendering -= OnRendering;
        CompositionTarget.Unsubscribe();
        _isAnimationSubscribed = false;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        var now = Environment.TickCount64;
        var elapsed = Math.Clamp((now - _lastAnimationTickMs) / 1000.0, 0.0, 0.1);
        _lastAnimationTickMs = now;
        _animationAngle = (_animationAngle + (elapsed * 360.0)) % 360.0;
        InvalidateVisual();
    }

    private static void OnAnimationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWProgressRing ring)
        {
            ring.UpdateAnimationSubscription();
        }
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWProgressRing ring)
        {
            ring.InvalidateVisual();
        }
    }
}
