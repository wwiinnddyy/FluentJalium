using System;

namespace FluentJalium.Controls;

/// <summary>
/// Frame pacing used by the FluentJalium controls that drive their own animations.
/// </summary>
internal static class FWFramePacing
{
    /// <summary>
    /// The interval frame-paced animations tick at. Jalium.UI keeps its detected
    /// composition refresh rate internal to the framework, so external animation
    /// loops fall back to the same 60 Hz default the framework itself uses.
    /// </summary>
    public static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(16);
}
