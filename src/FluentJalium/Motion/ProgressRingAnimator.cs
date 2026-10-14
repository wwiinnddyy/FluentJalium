using System.Diagnostics;
using Jalium.UI.Media;

namespace FluentJalium.Motion;

/// <summary>
/// Advances a ring's rotation once per frame. Adapted from the rotation keyframes of WinUI's indeterminate
/// Lottie asset (<c>ProgressRingIndeterminate.cpp:289-294</c> at microsoft-ui-xaml commit 19e3bdc3c), which runs
/// 0 -> 450 -> 900 degrees across a 2.0 s loop.
/// </summary>
/// <remarks>
/// A loop rather than a clock: a repeating animation attached to <c>RotateTransform.AngleProperty</c> never
/// ticks in 26.10.9 (<c>spike/ProgressProbe</c>, mode <c>anim</c>: 0 -> 0 -> 0), while the same property written
/// from a <c>CompositionTarget.Rendering</c> handler does move the picture (<c>spike/RingProbe</c>, mode
/// <c>E4</c>: different bounding boxes at frame 10 and frame 30). The asset's two keyframe halves are linear
/// against each other, so the loop is one constant rate rather than a segmented one.
/// </remarks>
internal sealed class ProgressRingAnimator
{
    /// <summary>Nine hundred degrees per two-second loop, the average of the asset's own keyframes.</summary>
    internal const double DegreesPerSecond = 900d / 2d;

    private readonly Action<double> _setAngle;

    private double _angle;
    private long _lastTimestamp;
    private bool _running;

    internal ProgressRingAnimator(Action<double> setAngle) => _setAngle = setAngle;

    internal bool IsRunning => _running;

    internal void Start()
    {
        if (_running)
        {
            return;
        }

        _running = true;
        _lastTimestamp = Stopwatch.GetTimestamp();
        CompositionTarget.Rendering += OnRendering;
    }

    internal void Stop()
    {
        if (!_running)
        {
            return;
        }

        _running = false;
        CompositionTarget.Rendering -= OnRendering;
    }

    private void OnRendering(object? sender, EventArgs args)
    {
        var now = Stopwatch.GetTimestamp();
        var elapsed = (now - _lastTimestamp) / (double)Stopwatch.Frequency;
        _lastTimestamp = now;
        if (elapsed <= 0)
        {
            return;
        }

        _angle = (_angle + (elapsed * DegreesPerSecond)) % 360d;
        _setAngle(_angle);
    }
}
