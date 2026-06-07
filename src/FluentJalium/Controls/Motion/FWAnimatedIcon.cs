using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Media.Animation;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium AnimatedIcon control for displaying animated Fluent icons.
/// </summary>
public class FWAnimatedIcon : Control, IFluentJaliumControl
{
    private Panel? _rootPanel;
    private bool _isPlaying;

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(IAnimatedVisualSource), typeof(FWAnimatedIcon),
            new PropertyMetadata(null, OnSourceChanged));

    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(string), typeof(FWAnimatedIcon),
            new PropertyMetadata(string.Empty, OnStateChanged));

    public static readonly DependencyProperty FallbackIconSourceProperty =
        DependencyProperty.Register(nameof(FallbackIconSource), typeof(object), typeof(FWAnimatedIcon),
            new PropertyMetadata(null));

    public static readonly DependencyProperty MirroredWhenRightToLeftProperty =
        DependencyProperty.Register(nameof(MirroredWhenRightToLeft), typeof(bool), typeof(FWAnimatedIcon),
            new PropertyMetadata(false));

    public static readonly DependencyProperty AutoPlayProperty =
        DependencyProperty.Register(nameof(AutoPlay), typeof(bool), typeof(FWAnimatedIcon),
            new PropertyMetadata(true, OnAutoPlayChanged));

    /// <summary>
    /// Initializes a new instance of the <see cref="FWAnimatedIcon"/> class.
    /// </summary>
    public FWAnimatedIcon()
    {
        Width = 20;
        Height = 20;
    }

    /// <summary>
    /// Gets or sets the animated visual source for the icon.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public IAnimatedVisualSource? Source
    {
        get => (IAnimatedVisualSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the current animation state.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string State
    {
        get => (string)GetValue(StateProperty)!;
        set => SetValue(StateProperty, value);
    }

    /// <summary>
    /// Gets or sets the fallback icon displayed when animation is not available.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? FallbackIconSource
    {
        get => GetValue(FallbackIconSourceProperty);
        set => SetValue(FallbackIconSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the icon is mirrored in right-to-left layouts.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public bool MirroredWhenRightToLeft
    {
        get => (bool)GetValue(MirroredWhenRightToLeftProperty)!;
        set => SetValue(MirroredWhenRightToLeftProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the animation plays automatically on state changes.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool AutoPlay
    {
        get => (bool)GetValue(AutoPlayProperty)!;
        set => SetValue(AutoPlayProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _rootPanel = GetTemplateChild("PART_RootPanel") as Panel;
        UpdateVisual();
    }

    /// <summary>
    /// Plays the animation from the current state to the specified state.
    /// </summary>
    public void Play()
    {
        if (!_isPlaying && Source != null)
        {
            _isPlaying = true;
            PlayAnimation();
        }
    }

    /// <summary>
    /// Stops the current animation.
    /// </summary>
    public void Stop()
    {
        _isPlaying = false;
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWAnimatedIcon icon)
        {
            icon.UpdateVisual();
        }
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWAnimatedIcon icon && icon.AutoPlay)
        {
            icon.Play();
        }
    }

    private static void OnAutoPlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWAnimatedIcon icon && e.NewValue is bool autoPlay && autoPlay)
        {
            icon.Play();
        }
    }

    private void UpdateVisual()
    {
        if (_rootPanel == null)
            return;

        _rootPanel.Children.Clear();

        if (Source == null)
        {
            // Show fallback icon if available
            if (FallbackIconSource != null)
            {
                var fallback = CreateFallbackContent();
                if (fallback != null)
                {
                    _rootPanel.Children.Add(fallback);
                }
            }
        }
        else
        {
            // Create animated visual
            var visual = Source.TryCreateAnimatedVisual();
            if (visual != null)
            {
                _rootPanel.Children.Add(visual);
                if (AutoPlay)
                {
                    Play();
                }
            }
        }
    }

    private FrameworkElement? CreateFallbackContent()
    {
        if (FallbackIconSource is string glyph)
        {
            return new TextBlock
            {
                Text = glyph,
                FontFamily = "Segoe Fluent Icons",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }
        else if (FallbackIconSource is FrameworkElement element)
        {
            return element;
        }

        return null;
    }

    private void PlayAnimation()
    {
        if (Source == null || _rootPanel == null || _rootPanel.Children.Count == 0)
        {
            _isPlaying = false;
            return;
        }

        // Animation logic would be implemented here
        // For now, this is a placeholder that marks the animation as complete
        _isPlaying = false;
    }
}

/// <summary>
/// Interface for animated visual sources.
/// </summary>
public interface IAnimatedVisualSource
{
    /// <summary>
    /// Tries to create an animated visual element.
    /// </summary>
    /// <returns>The animated visual element, or null if creation fails.</returns>
    FrameworkElement? TryCreateAnimatedVisual();
}
