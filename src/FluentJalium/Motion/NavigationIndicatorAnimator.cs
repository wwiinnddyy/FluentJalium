using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media.Animation;

namespace FluentJalium.Motion;

/// <summary>
/// One indicator spans item boundaries. Adapted from NavigationView.cpp's 200/600 ms
/// stretch/settle keyframes at microsoft-ui-xaml commit 19e3bdc3c.
/// </summary>
internal sealed class NavigationIndicatorAnimator : IDisposable
{
    internal const double RestingHeight = 16;
    private readonly Border _indicator;
    private bool _positioned;
    private bool _disposed;
    private double _targetTop;
    private int _generation;

    internal NavigationIndicatorAnimator(Border indicator) => _indicator = indicator;

    internal void MoveTo(double left, double top, bool animate)
    {
        if (_disposed || !double.IsFinite(left) || !double.IsFinite(top)) return;
        Canvas.SetLeft(_indicator, left);

        // LayoutUpdated fires during motion. An unchanged destination must not restart it.
        if (_positioned && Math.Abs(top - _targetTop) < 0.01) return;

        var fromTop = Canvas.GetTop(_indicator);
        var fromHeight = _indicator.Height;
        var canAnimate = animate && _positioned && double.IsFinite(fromTop) &&
            double.IsFinite(fromHeight) && fromHeight > 0;
        _positioned = true;
        _targetTop = top;

        // Capture displayed values before removing old clocks so fast retargets remain continuous.
        Complete();
        _indicator.Opacity = 1;
        if (!canAnimate) return;

        var extendedTop = Math.Min(fromTop, top);
        var extendedBottom = Math.Max(fromTop + fromHeight, top + RestingHeight);
        var offsetAnimation = CreateAnimation(fromTop, extendedTop, top);
        var heightAnimation = CreateAnimation(fromHeight, extendedBottom - extendedTop, RestingHeight);
        var generation = _generation;
        offsetAnimation.Completed += (_, _) =>
        {
            if (!_disposed && generation == _generation) Complete();
        };
        _indicator.BeginAnimation(Canvas.TopProperty, offsetAnimation);
        _indicator.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);
    }

    internal void Complete()
    {
        _generation++;
        _indicator.BeginAnimation(Canvas.TopProperty, (AnimationTimeline?)null);
        _indicator.BeginAnimation(FrameworkElement.HeightProperty, (AnimationTimeline?)null);
        Canvas.SetTop(_indicator, _targetTop);
        _indicator.Height = RestingHeight;
    }

    internal void Hide()
    {
        Complete();
        _positioned = false;
        _indicator.Opacity = 0;
    }

    public void Dispose()
    {
        if (_disposed) return;
        Hide();
        _disposed = true;
    }

    private static DoubleAnimationUsingKeyFrames CreateAnimation(double from, double extended, double to)
    {
        var animation = new DoubleAnimationUsingKeyFrames
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(600)),
            FillBehavior = FillBehavior.Stop,
        };
        animation.KeyFrames.Add(new SplineDoubleKeyFrame(from, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        animation.KeyFrames.Add(new SplineDoubleKeyFrame(extended,
            KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)), new KeySpline(0.9, 0.1, 1, 0.2)));
        animation.KeyFrames.Add(new SplineDoubleKeyFrame(to,
            KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(600)), new KeySpline(0.1, 0.9, 0.2, 1)));
        return animation;
    }
}
