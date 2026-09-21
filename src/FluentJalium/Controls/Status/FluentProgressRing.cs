using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FluentJalium.Motion;

// The implicit System.IO using wins over the shape namespace for this one name.
using Path = Jalium.UI.Shapes.Path;

namespace FluentJalium.Controls;

/// <summary>
/// A circular progress indicator: WinUI's ProgressRing. The arc is drawn by this control rather than played
/// out of an animated-visual asset, because 26.10.9 has no <c>ProgressRing</c> to retemplate and no Lottie
/// player to host the asset upstream ships.
/// </summary>
/// <remarks>
/// <para>
/// The runtime census (<c>spike/ProgressProbe</c>, mode <c>census</c>) reports <c>ProgressRing</c> and
/// <c>LottiePlayer</c> both absent, so this is an own type by the same rule as <see cref="FluentPipsPager"/>.
/// Three routes to the ring were measured before one was picked (<c>spike/RingProbe</c>):
/// </para>
/// <para>
/// A dashed stroke is the cheap arc and it is a dead end here. <c>StrokeDashArray="20 200"</c> reads back on the
/// shape as <c>[20, 200]</c> and prints the same 556 inked pixels as an undashed ring, at the same bounding box
/// and the same row profile (mode <c>ink</c>), and writing <c>StrokeDashOffset</c> to 0, 40 and 95 leaves the
/// picture byte-for-byte identical (mode <c>anim</c>). The dash is parsed, stored and ignored.
/// </para>
/// <para>
/// A geometry authored in markup is a dead end too, and quietly: <c>&lt;PathFigure&gt;&lt;ArcSegment/&gt;&lt;/PathFigure&gt;</c>
/// hydrates a <c>PathGeometry</c> whose <c>Figures</c> collection reads back with <c>Count=0</c> and prints nothing,
/// and spelling the collection out as <c>&lt;PathFigure.Segments&gt;</c> changes nothing (mode <c>E5</c>). Only the
/// <c>Data="M ... A ..."</c> mini-language reaches an actual figure from markup.
/// </para>
/// <para>
/// What does work is a geometry the control builds in code: the same <c>PathGeometry</c>/<c>PathFigure</c>/
/// <c>ArcSegment</c> triple assigned from C# inked exactly what the equivalent parsed string inked (169 pixels for
/// a quarter arc), and re-assigning <c>Data</c> repaints - a quarter arc, a 135° arc and a 359.9° arc measure
/// 169, 248 and 620 inked pixels in the same 60x60 cell (modes <c>E2</c>, <c>E3</c>). That is the determinate ring.
/// </para>
/// <para>
/// The spin cannot ride an animation clock, because a clock refuses to tick on a transform (adaptation/00 S1-o
/// clause 4); a <c>CompositionTarget.Rendering</c> handler is what does advance a value frame by frame (mode
/// <c>E4</c>). Where the angle goes is the other half of the decision: it is folded into the figure's start angle
/// rather than handed to a <c>RenderTransform</c>, because regenerating the figure is the one repaint route this
/// runtime is measured to honour.
/// </para>
/// <para>
/// The base type follows upstream rather than <c>ProgressBar</c>: <c>Control</c> with its own
/// <see cref="Minimum"/>/<see cref="Maximum"/>/<see cref="Value"/> rows (<c>ProgressRing.idl:17</c> and
/// <c>ProgressRing.properties.cpp</c>), not <c>RangeBase</c>. The runtime does export a <c>RangeBase</c>, but
/// taking it would add <c>SmallChange</c> and <c>LargeChange</c> to a control upstream says nothing about.
/// </para>
/// <para>
/// Upstream's own numbers are carried where they are measurable: the radius and stroke factors come out of the two
/// Lottie canvases (they are different assets, with different geometry), the loop is 2.0 s, and the indeterminate
/// arc is half the circle because the asset's trim ends at 0.5. The <c>TrimStart</c> that chases it - the taper at
/// the trailing end - is not reproduced; see <c>docs/astra/audits/progress-ring.md</c>.
/// </para>
/// </remarks>
public class FluentProgressRing : Control
{
    private const string DefaultStyleResourceKey = "DefaultProgressRingStyle";
    private const string ArcPartName = "ProgressRingArc";

    /// <summary>
    /// The determinate asset is a 32x32 canvas holding an ellipse of radius 8 at shape scale 1.77, stroked at 1.5
    /// (<c>ProgressRingDeterminate.cpp:171,183,194</c> and <c>:207</c>). The stroke factor is the same arithmetic:
    /// the asset's own thickness, not the <c>ProgressRingStrokeThickness</c> row, which upstream never reads.
    /// </summary>
    private const double DeterminateRadiusFactor = 8 * 1.77 / 32;

    private const double DeterminateStrokeFactor = 1.5 * 1.77 / 32;

    /// <summary>
    /// The indeterminate asset is a different file: an 80x80 canvas, radius 7, scale 5, stroke 1.5
    /// (<c>ProgressRingIndeterminate.cpp:178,190,202</c> and <c>:216</c>). ModernWpf draws both states from the
    /// determinate numbers; this one keeps the distinction the assets carry.
    /// </summary>
    private const double IndeterminateRadiusFactor = 7d * 5 / 80;

    private const double IndeterminateStrokeFactor = 1.5 * 5 / 80;

    /// <summary>The indeterminate arc: the asset's <c>TrimEnd</c> reaches 0.5 of the circle and stays there.</summary>
    private const double IndeterminateSweepAngle = 180;

    /// <summary>
    /// A full circle cannot be drawn as one arc - start and end would coincide - so 100 % stops just short of
    /// closing. The value is ModernWpf's (<c>ProgressRingIndicator.cs:189</c>); upstream's equivalent is a Lottie
    /// trim of 1.0, which needs no cap because the player closes the path itself.
    /// </summary>
    private const double MaximumSweepAngle = 359.9;

    /// <summary>Both assets start the arc at 12 o'clock and grow it clockwise.</summary>
    private const double StartAngleDegrees = -90;

    /// <summary>Identifies the <see cref="IsActive"/> dependency property.</summary>
    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
        nameof(IsActive), typeof(bool), typeof(FluentProgressRing),
        new PropertyMetadata(true, OnAppearanceConditionChanged));

    /// <summary>Identifies the <see cref="IsIndeterminate"/> dependency property.</summary>
    public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register(
        nameof(IsIndeterminate), typeof(bool), typeof(FluentProgressRing),
        new PropertyMetadata(true, OnAppearanceConditionChanged));

    /// <summary>Identifies the <see cref="Minimum"/> dependency property.</summary>
    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum), typeof(double), typeof(FluentProgressRing),
        new PropertyMetadata(0d, OnAppearanceConditionChanged));

    /// <summary>Identifies the <see cref="Maximum"/> dependency property.</summary>
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum), typeof(double), typeof(FluentProgressRing),
        new PropertyMetadata(100d, OnAppearanceConditionChanged));

    /// <summary>Identifies the <see cref="Value"/> dependency property.</summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(double), typeof(FluentProgressRing),
        new PropertyMetadata(0d, OnAppearanceConditionChanged));

    private readonly ProgressRingAnimator _animator;

    private Path? _arc;
    private double _spin;
    private Style? _appliedDefaultStyle;

    /// <summary>Creates a ring and applies its named style when one is available.</summary>
    public FluentProgressRing()
    {
        _animator = new ProgressRingAnimator(SetAngle);
        ApplyDefaultStyle();
        Loaded += (_, _) =>
        {
            ApplyDefaultStyle();
            UpdateRing();
        };
        Unloaded += (_, _) => _animator.Stop();
        SizeChanged += (_, _) => UpdateRing();
    }

    /// <summary>Gets or sets whether the ring shows. <c>false</c> empties the picture, like upstream's <c>Inactive</c> state.</summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty)!;
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the ring spins (the default, like upstream) or tracks <see cref="Value"/>.
    /// </summary>
    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty)!;
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>Gets or sets the value that maps to an empty arc. Upstream's own default, 0.</summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty)!;
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>Gets or sets the value that maps to a full circle. Upstream's own default, 100.</summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty)!;
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>Gets or sets how far the determinate arc has travelled. Upstream's own default, 0.</summary>
    public double Value
    {
        get => (double)GetValue(ValueProperty)!;
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The sweep angle the ring is currently drawing, in degrees clockwise from 12 o'clock.</summary>
    /// <remarks>Read by the tests to separate "the value moved the arc" from "the arc was rebuilt at the same size".
    /// Upstream has no counterpart: the determinate asset is driven by a playback position, not an angle.</remarks>
    public double CurrentSweepAngle { get; private set; }

    private static void OnAppearanceConditionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) =>
        ((FluentProgressRing)sender).UpdateRing();

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _arc = GetTemplateChild(ArcPartName) as Path;
        UpdateRing();
    }

    private void UpdateRing()
    {
        if (_arc is null)
        {
            return;
        }

        var side = Side();
        if (side <= 0)
        {
            return;
        }

        var radiusFactor = IsIndeterminate ? IndeterminateRadiusFactor : DeterminateRadiusFactor;
        var strokeFactor = IsIndeterminate ? IndeterminateStrokeFactor : DeterminateStrokeFactor;
        var radius = side * radiusFactor;
        var stroke = side * strokeFactor;

        CurrentSweepAngle = IsIndeterminate ? IndeterminateSweepAngle : SweepForValue();
        _arc.Data = BuildArc(side, radius, StartAngleDegrees + (IsIndeterminate ? _spin : 0), CurrentSweepAngle);
        _arc.StrokeThickness = stroke;

        UpdateMotion();
    }

    private void UpdateMotion()
    {
        if (IsActive && !IsIndeterminate)
        {
            _animator.Stop();
            SetAngle(0);
            return;
        }

        if (IsActive)
        {
            _animator.Start();
        }
        else
        {
            _animator.Stop();
        }
    }

    private double SweepForValue()
    {
        var range = Maximum - Minimum;
        if (range <= 0 || double.IsNaN(range))
        {
            return 0;
        }

        var progress = Math.Clamp((Value - Minimum) / range, 0d, 1d);
        return Math.Min(progress * 360d, MaximumSweepAngle);
    }

    /// <summary>The box the ring draws into: the measured side, or the authored one before the first measure.</summary>
    private double Side()
    {
        var width = ActualWidth > 0 ? ActualWidth : Width;
        var height = ActualHeight > 0 ? ActualHeight : Height;
        if (!double.IsFinite(width) || !double.IsFinite(height) || width <= 0 || height <= 0)
        {
            return 0;
        }

        return Math.Min(width, height);
    }

    private void SetAngle(double degrees)
    {
        // The spin is a start angle written into the figure, not a RenderTransform aimed at the part. Rotating the
        // arc that way was the first cut and it is not claimed here: the picture it produced turned out to be a
        // zero-radius arc (an integer-division constant, caught by AstraProgressRingTests), so the pivot question
        // was never isolated and remains unmeasured. Regenerating the figure is the route RingProbe mode E2
        // measured to repaint - a quarter, 135 degree and 359.9 degree arc inked 169, 248 and 620 pixels through
        // the same assignment - so it is the route the spin rides.
        _spin = degrees;
        if (IsIndeterminate)
        {
            UpdateRing();
        }
    }

    private static Geometry BuildArc(double side, double radius, double startDegrees, double sweepDegrees)
    {
        if (sweepDegrees <= 0)
        {
            // A zero-length arc is not an empty picture: a round cap on it still inks a dot at the start point
            // (two accent pixels in a 60x60 capture at the authored 32 DIP box). Upstream's 0 % is the asset's
            // trim of zero, which draws nothing, so the guard is what keeps 0 % looking like 0 %.
            return new PathGeometry();
        }

        var center = side / 2;
        var start = PointOnCircle(center, radius, startDegrees);
        var end = PointOnCircle(center, radius, startDegrees + sweepDegrees);
        var geometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = start, IsClosed = false };
        figure.Segments.Add(new ArcSegment
        {
            Point = end,
            Size = new Size(radius, radius),
            SweepDirection = SweepDirection.Clockwise,
            IsLargeArc = sweepDegrees > 180,
        });
        geometry.Figures.Add(figure);
        return geometry;
    }

    /// <summary>Screen angles: 0 degrees is 12 o'clock once the caller's start angle folds in -90, and the axis
    /// grows clockwise because the y axis points down.</summary>
    private static Point PointOnCircle(double center, double radius, double degrees)
    {
        var radians = degrees * Math.PI / 180d;
        return new Point(center + (radius * Math.Cos(radians)), center + (radius * Math.Sin(radians)));
    }

    private void ApplyDefaultStyle()
    {
        // Same named-style fallback as FluentPipsPager: an implicit derived-control style is not guaranteed to
        // resolve, and an application-assigned style - including an explicit null - is never replaced.
        if ((Style is not null || _appliedDefaultStyle is not null) &&
            !ReferenceEquals(Style, _appliedDefaultStyle))
        {
            return;
        }

        if (_appliedDefaultStyle is null &&
            !ReferenceEquals(ReadLocalValue(StyleProperty), DependencyProperty.UnsetValue))
        {
            return;
        }

        if (TryFindResource(DefaultStyleResourceKey) is not Style style)
        {
            return;
        }

        _appliedDefaultStyle = style;
        SetCurrentValue(StyleProperty, style);
    }
}
