using Jalium.UI;
using Jalium.UI.Media;
using Jalium.UI.Media.Animation;
using Jalium.UI.Threading;

namespace FluentJalium.Controls;

/// <summary>
/// Describes how a prepared FluentJalium shared element transition starts.
/// </summary>
public readonly record struct FWConnectedAnimationPlan(
    double TranslateX,
    double TranslateY,
    double ScaleX,
    double ScaleY,
    double InitialOpacity,
    FWConnectedAnimationConfiguration Configuration);

/// <summary>
/// Describes how a prepared shared element aligns with the destination surface.
/// </summary>
public enum FWConnectedAnimationConfiguration
{
    /// <summary>
    /// Aligns the destination to the source top-left corner and scales from the source bounds.
    /// </summary>
    Direct,

    /// <summary>
    /// Aligns the destination around the source center while keeping the destination natural size.
    /// </summary>
    Gravity
}

/// <summary>
/// Options for <see cref="FWConnectedAnimationService"/>.
/// </summary>
public sealed class FWConnectedAnimationOptions
{
    private TimeSpan _duration = TimeSpan.FromMilliseconds(320);
    private IEasingFunction _easingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
    private double _initialOpacity = 0.72;
    private FWConnectedAnimationConfiguration _configuration;

    /// <summary>
    /// Gets or sets the transition duration. Defaults to 320ms, matching the snappy WinUI motion family.
    /// </summary>
    public TimeSpan Duration
    {
        get => _duration;
        set
        {
            if (value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Duration cannot be negative.");
            }

            _duration = value;
        }
    }

    /// <summary>
    /// Gets or sets the easing function used for the shared element transform.
    /// </summary>
    public IEasingFunction EasingFunction
    {
        get => _easingFunction;
        set => _easingFunction = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the destination opacity at the first animation frame.
    /// </summary>
    public double InitialOpacity
    {
        get => _initialOpacity;
        set => _initialOpacity = Math.Clamp(value, 0, 1);
    }

    /// <summary>
    /// Gets or sets whether the destination element scales from the prepared source bounds.
    /// </summary>
    public bool AnimateScale { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the destination fades in while it moves into place.
    /// </summary>
    public bool AnimateOpacity { get; set; } = true;

    /// <summary>
    /// Gets or sets how the source bounds align with the destination. Defaults to <see cref="FWConnectedAnimationConfiguration.Direct"/>.
    /// </summary>
    public FWConnectedAnimationConfiguration Configuration
    {
        get => _configuration;
        set
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Unknown connected animation configuration.");
            }

            _configuration = value;
        }
    }
}

/// <summary>
/// A lightweight FluentJalium shared element animation service inspired by WinUI ConnectedAnimation.
/// </summary>
public sealed class FWConnectedAnimationService
{
    private readonly Dictionary<string, PreparedAnimation> _preparedAnimations = new(StringComparer.Ordinal);

    /// <summary>
    /// Prepares a source element for a later shared element transition.
    /// </summary>
    /// <param name="key">The animation key that links the source and destination elements.</param>
    /// <param name="source">The source element to animate from.</param>
    /// <param name="options">Optional transition settings.</param>
    /// <returns><see langword="true"/> when the source bounds are usable.</returns>
    public bool PrepareToAnimate(string key, UIElement source, FWConnectedAnimationOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(source);

        if (!TryGetRootBounds(source, out var sourceBounds))
        {
            return false;
        }

        _preparedAnimations[key] = new PreparedAnimation(sourceBounds, CreateOptionsSnapshot(options));
        return true;
    }

    /// <summary>
    /// Starts the prepared transition against a destination element.
    /// </summary>
    /// <param name="key">The animation key passed to <see cref="PrepareToAnimate"/>.</param>
    /// <param name="destination">The destination element to animate into place.</param>
    /// <returns><see langword="true"/> when a prepared animation was consumed and started.</returns>
    public bool TryStart(string key, UIElement destination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(destination);

        if (!_preparedAnimations.TryGetValue(key, out var prepared) ||
            !TryGetRootBounds(destination, out var destinationBounds) ||
            !TryCreatePlan(prepared.SourceBounds, destinationBounds, prepared.Options, out var plan))
        {
            return false;
        }

        _preparedAnimations.Remove(key);
        StartTransformAnimation(destination, plan, prepared.Options);
        return true;
    }

    /// <summary>
    /// Creates a non-destructive plan for a prepared transition without consuming or starting it.
    /// </summary>
    /// <param name="key">The animation key passed to <see cref="PrepareToAnimate"/>.</param>
    /// <param name="destination">The destination element to plan against.</param>
    /// <param name="plan">The calculated shared element transform plan.</param>
    /// <returns><see langword="true"/> when a prepared source and destination bounds can create a plan.</returns>
    public bool TryCreatePreparedPlan(string key, UIElement destination, out FWConnectedAnimationPlan plan)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(destination);

        plan = default;
        return _preparedAnimations.TryGetValue(key, out var prepared) &&
            TryGetRootBounds(destination, out var destinationBounds) &&
            TryCreatePlan(prepared.SourceBounds, destinationBounds, prepared.Options, out plan);
    }

    /// <summary>
    /// Clears a prepared transition without starting it.
    /// </summary>
    public bool Cancel(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _preparedAnimations.Remove(key);
    }

    /// <summary>
    /// Gets whether a key has a prepared source element.
    /// </summary>
    public bool IsPrepared(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _preparedAnimations.ContainsKey(key);
    }

    /// <summary>
    /// Creates the transform plan used by the runtime service. Exposed for deterministic tests.
    /// </summary>
    public static bool TryCreatePlan(
        Rect sourceBounds,
        Rect destinationBounds,
        FWConnectedAnimationOptions? options,
        out FWConnectedAnimationPlan plan)
    {
        var resolvedOptions = options ?? new FWConnectedAnimationOptions();
        plan = default;

        if (!IsUsableBounds(sourceBounds) || !IsUsableBounds(destinationBounds))
        {
            return false;
        }

        var configuration = resolvedOptions.Configuration;
        var useScale = resolvedOptions.AnimateScale && configuration == FWConnectedAnimationConfiguration.Direct;
        var scaleX = useScale ? sourceBounds.Width / destinationBounds.Width : 1.0;
        var scaleY = useScale ? sourceBounds.Height / destinationBounds.Height : 1.0;

        if (!double.IsFinite(scaleX) || !double.IsFinite(scaleY) || scaleX <= 0 || scaleY <= 0)
        {
            return false;
        }

        var translateX = configuration == FWConnectedAnimationConfiguration.Gravity
            ? GetCenterX(sourceBounds) - GetCenterX(destinationBounds)
            : sourceBounds.X - destinationBounds.X;
        var translateY = configuration == FWConnectedAnimationConfiguration.Gravity
            ? GetCenterY(sourceBounds) - GetCenterY(destinationBounds)
            : sourceBounds.Y - destinationBounds.Y;

        plan = new FWConnectedAnimationPlan(
            translateX,
            translateY,
            scaleX,
            scaleY,
            resolvedOptions.AnimateOpacity ? resolvedOptions.InitialOpacity : 1.0,
            configuration);
        return true;
    }

    private static bool TryGetRootBounds(UIElement element, out Rect bounds)
    {
        bounds = default;

        var size = element.RenderSize;
        if (size.Width <= 0 || size.Height <= 0 || !double.IsFinite(size.Width) || !double.IsFinite(size.Height))
        {
            return false;
        }

        var transform = element.TransformToVisual(null);
        if (transform == null || !transform.TryTransform(new Point(0, 0), out var origin))
        {
            return false;
        }

        bounds = new Rect(origin.X, origin.Y, size.Width, size.Height);
        return IsUsableBounds(bounds);
    }

    private static void StartTransformAnimation(
        UIElement destination,
        FWConnectedAnimationPlan plan,
        FWConnectedAnimationOptions options)
    {
        var originalTransform = destination.RenderTransform;
        var originalTransformOrigin = destination.RenderTransformOrigin;
        var originalOpacity = destination.Opacity;
        var transform = new CompositeTransform
        {
            ScaleX = plan.ScaleX,
            ScaleY = plan.ScaleY,
            TranslateX = plan.TranslateX,
            TranslateY = plan.TranslateY
        };
        var durationMs = Math.Max(1.0, options.Duration.TotalMilliseconds);
        var startTime = Environment.TickCount64;
        var timer = new DispatcherTimer { Interval = CompositionTarget.FrameInterval };

        destination.RenderTransformOrigin = new Point(0, 0);
        destination.RenderTransform = originalTransform == null
            ? transform
            : new TransformGroup(transform, originalTransform);
        if (options.AnimateOpacity)
        {
            destination.Opacity = plan.InitialOpacity;
        }

        timer.Tick += (_, _) =>
        {
            var elapsed = Environment.TickCount64 - startTime;
            var rawProgress = Math.Min(1.0, elapsed / durationMs);
            var easedProgress = Math.Clamp(options.EasingFunction.Ease(rawProgress), 0, 1);
            var remaining = 1.0 - easedProgress;

            transform.ScaleX = Lerp(plan.ScaleX, 1.0, easedProgress);
            transform.ScaleY = Lerp(plan.ScaleY, 1.0, easedProgress);
            transform.TranslateX = plan.TranslateX * remaining;
            transform.TranslateY = plan.TranslateY * remaining;
            if (options.AnimateOpacity)
            {
                destination.Opacity = Lerp(plan.InitialOpacity, originalOpacity, easedProgress);
            }

            if (rawProgress >= 1.0)
            {
                timer.Stop();
                destination.RenderTransform = originalTransform;
                destination.RenderTransformOrigin = originalTransformOrigin;
                destination.Opacity = originalOpacity;
            }
        };
        timer.Start();
    }

    private static bool IsUsableBounds(Rect bounds)
    {
        return double.IsFinite(bounds.X) &&
            double.IsFinite(bounds.Y) &&
            double.IsFinite(bounds.Width) &&
            double.IsFinite(bounds.Height) &&
            bounds.Width > 0 &&
            bounds.Height > 0;
    }

    private static double Lerp(double from, double to, double progress)
    {
        return from + ((to - from) * progress);
    }

    private static FWConnectedAnimationOptions CreateOptionsSnapshot(FWConnectedAnimationOptions? options)
    {
        if (options == null)
        {
            return new FWConnectedAnimationOptions();
        }

        return new FWConnectedAnimationOptions
        {
            Duration = options.Duration,
            EasingFunction = options.EasingFunction,
            InitialOpacity = options.InitialOpacity,
            AnimateScale = options.AnimateScale,
            AnimateOpacity = options.AnimateOpacity,
            Configuration = options.Configuration
        };
    }

    private static double GetCenterX(Rect bounds)
    {
        return bounds.X + (bounds.Width / 2.0);
    }

    private static double GetCenterY(Rect bounds)
    {
        return bounds.Y + (bounds.Height / 2.0);
    }

    private sealed record PreparedAnimation(Rect SourceBounds, FWConnectedAnimationOptions Options);
}
