using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Media.Animation;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium AnimatedVisualPlayer control for playing Lottie-like animations.
/// </summary>
public class FWAnimatedVisualPlayer : Control, IFluentJaliumControl
{
    private IAnimatedVisual? _animatedVisual;
    private bool _isPlaying;
    private double _currentProgress;

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(IAnimatedVisualSource), typeof(FWAnimatedVisualPlayer),
            new PropertyMetadata(null, OnSourceChanged));

    public static readonly DependencyProperty AutoPlayProperty =
        DependencyProperty.Register(nameof(AutoPlay), typeof(bool), typeof(FWAnimatedVisualPlayer),
            new PropertyMetadata(true, OnAutoPlayChanged));

    public static readonly DependencyProperty IsPlayingProperty =
        DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(FWAnimatedVisualPlayer),
            new PropertyMetadata(false, OnIsPlayingChanged));

    public static readonly DependencyProperty PlaybackRateProperty =
        DependencyProperty.Register(nameof(PlaybackRate), typeof(double), typeof(FWAnimatedVisualPlayer),
            new PropertyMetadata(1.0, OnPlaybackRateChanged), ValidatePlaybackRate);

    public static readonly DependencyProperty StretchProperty =
        DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(FWAnimatedVisualPlayer),
            new PropertyMetadata(Stretch.Uniform));

    public static readonly DependencyProperty IsLoopingProperty =
        DependencyProperty.Register(nameof(IsLooping), typeof(bool), typeof(FWAnimatedVisualPlayer),
            new PropertyMetadata(true));

    public static readonly DependencyProperty FallbackContentProperty =
        DependencyProperty.Register(nameof(FallbackContent), typeof(object), typeof(FWAnimatedVisualPlayer),
            new PropertyMetadata(null));

    private static readonly DependencyPropertyKey DurationPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Duration), typeof(TimeSpan), typeof(FWAnimatedVisualPlayer),
            new PropertyMetadata(TimeSpan.Zero));

    public static readonly DependencyProperty DurationProperty = DurationPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey IsAnimatedVisualLoadedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsAnimatedVisualLoaded), typeof(bool), typeof(FWAnimatedVisualPlayer),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsAnimatedVisualLoadedProperty = IsAnimatedVisualLoadedPropertyKey.DependencyProperty;

    public static readonly RoutedEvent PlayingEvent =
        EventManager.RegisterRoutedEvent(nameof(Playing), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(FWAnimatedVisualPlayer));

    public static readonly RoutedEvent PausedEvent =
        EventManager.RegisterRoutedEvent(nameof(Paused), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(FWAnimatedVisualPlayer));

    public static readonly RoutedEvent StoppedEvent =
        EventManager.RegisterRoutedEvent(nameof(Stopped), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(FWAnimatedVisualPlayer));

    public static readonly RoutedEvent CompletedEvent =
        EventManager.RegisterRoutedEvent(nameof(Completed), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(FWAnimatedVisualPlayer));

    /// <summary>
    /// Initializes a new instance of the <see cref="FWAnimatedVisualPlayer"/> class.
    /// </summary>
    public FWAnimatedVisualPlayer()
    {
        Width = 100;
        Height = 100;
    }

    /// <summary>
    /// Gets or sets the animated visual source.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public IAnimatedVisualSource? Source
    {
        get => (IAnimatedVisualSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the animation plays automatically when loaded.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool AutoPlay
    {
        get => (bool)GetValue(AutoPlayProperty)!;
        set => SetValue(AutoPlayProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the animation is currently playing.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty)!;
        set => SetValue(IsPlayingProperty, value);
    }

    /// <summary>
    /// Gets or sets the playback rate (1.0 = normal speed).
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public double PlaybackRate
    {
        get => (double)GetValue(PlaybackRateProperty)!;
        set => SetValue(PlaybackRateProperty, value);
    }

    /// <summary>
    /// Gets or sets how the animation is stretched to fill the available space.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty)!;
        set => SetValue(StretchProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the animation loops.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsLooping
    {
        get => (bool)GetValue(IsLoopingProperty)!;
        set => SetValue(IsLoopingProperty, value);
    }

    /// <summary>
    /// Gets or sets the fallback content displayed when the animation cannot be loaded.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? FallbackContent
    {
        get => GetValue(FallbackContentProperty);
        set => SetValue(FallbackContentProperty, value);
    }

    /// <summary>
    /// Gets the duration of the animation.
    /// </summary>
    public TimeSpan Duration => (TimeSpan)GetValue(DurationProperty)!;

    /// <summary>
    /// Gets a value indicating whether the animated visual is loaded.
    /// </summary>
    public bool IsAnimatedVisualLoaded => (bool)GetValue(IsAnimatedVisualLoadedProperty)!;

    /// <summary>
    /// Occurs when the animation starts playing.
    /// </summary>
    public event RoutedEventHandler Playing
    {
        add => AddHandler(PlayingEvent, value);
        remove => RemoveHandler(PlayingEvent, value);
    }

    /// <summary>
    /// Occurs when the animation is paused.
    /// </summary>
    public event RoutedEventHandler Paused
    {
        add => AddHandler(PausedEvent, value);
        remove => RemoveHandler(PausedEvent, value);
    }

    /// <summary>
    /// Occurs when the animation is stopped.
    /// </summary>
    public event RoutedEventHandler Stopped
    {
        add => AddHandler(StoppedEvent, value);
        remove => RemoveHandler(StoppedEvent, value);
    }

    /// <summary>
    /// Occurs when the animation completes.
    /// </summary>
    public event RoutedEventHandler Completed
    {
        add => AddHandler(CompletedEvent, value);
        remove => RemoveHandler(CompletedEvent, value);
    }

    /// <summary>
    /// Starts playing the animation from the beginning.
    /// </summary>
    public void Play()
    {
        if (_animatedVisual == null)
            return;

        _isPlaying = true;
        IsPlaying = true;
        _currentProgress = 0;
        RaiseEvent(new RoutedEventArgs(PlayingEvent, this));
    }

    /// <summary>
    /// Pauses the animation at the current position.
    /// </summary>
    public void Pause()
    {
        _isPlaying = false;
        IsPlaying = false;
        RaiseEvent(new RoutedEventArgs(PausedEvent, this));
    }

    /// <summary>
    /// Stops the animation and resets to the beginning.
    /// </summary>
    public void Stop()
    {
        _isPlaying = false;
        IsPlaying = false;
        _currentProgress = 0;
        RaiseEvent(new RoutedEventArgs(StoppedEvent, this));
    }

    /// <summary>
    /// Resumes playing from the current position.
    /// </summary>
    public void Resume()
    {
        if (_animatedVisual == null)
            return;

        _isPlaying = true;
        IsPlaying = true;
        RaiseEvent(new RoutedEventArgs(PlayingEvent, this));
    }

    /// <summary>
    /// Sets the progress position (0.0 to 1.0).
    /// </summary>
    public void SetProgress(double progress)
    {
        _currentProgress = Math.Clamp(progress, 0.0, 1.0);
        _animatedVisual?.SetProgress(_currentProgress);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        LoadAnimatedVisual();
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWAnimatedVisualPlayer player)
        {
            player.LoadAnimatedVisual();
        }
    }

    private static void OnAutoPlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWAnimatedVisualPlayer player && e.NewValue is bool autoPlay && autoPlay && player.IsAnimatedVisualLoaded)
        {
            player.Play();
        }
    }

    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWAnimatedVisualPlayer player && e.NewValue is bool isPlaying)
        {
            if (isPlaying)
            {
                player.Resume();
            }
            else
            {
                player.Pause();
            }
        }
    }

    private static void OnPlaybackRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Playback rate change logic would be implemented here
    }

    private static bool ValidatePlaybackRate(object? value)
    {
        return value is double d && d > 0 && !double.IsInfinity(d) && !double.IsNaN(d);
    }

    private void LoadAnimatedVisual()
    {
        _animatedVisual = null;
        SetValue(IsAnimatedVisualLoadedPropertyKey.DependencyProperty, false);

        if (Source == null)
            return;

        var visual = Source.TryCreateAnimatedVisual();
        if (visual is IAnimatedVisual animatedVisual)
        {
            _animatedVisual = animatedVisual;
            SetValue(DurationPropertyKey.DependencyProperty, animatedVisual.Duration);
            SetValue(IsAnimatedVisualLoadedPropertyKey.DependencyProperty, true);

            if (AutoPlay)
            {
                Play();
            }
        }
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (_animatedVisual != null && _isPlaying)
        {
            // Animation rendering logic would be implemented here
            _currentProgress += 0.016 * PlaybackRate; // Approx 60fps frame

            if (_currentProgress >= 1.0)
            {
                if (IsLooping)
                {
                    _currentProgress = 0;
                }
                else
                {
                    _currentProgress = 1.0;
                    Stop();
                    RaiseEvent(new RoutedEventArgs(CompletedEvent, this));
                }
            }

            _animatedVisual.SetProgress(_currentProgress);
            InvalidateVisual();
        }
    }
}

/// <summary>
/// Interface for animated visuals.
/// </summary>
public interface IAnimatedVisual
{
    /// <summary>
    /// Gets the duration of the animation.
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// Gets the size of the animation.
    /// </summary>
    Size Size { get; }

    /// <summary>
    /// Sets the progress of the animation (0.0 to 1.0).
    /// </summary>
    void SetProgress(double progress);

    /// <summary>
    /// Renders the current frame to the drawing context.
    /// </summary>
    void Render(DrawingContext drawingContext, Size size);
}
