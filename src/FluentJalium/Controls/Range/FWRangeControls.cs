using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium Slider control.
/// </summary>
public class FWSlider : Slider, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium RangeSlider control.
/// </summary>
public class FWRangeSlider : RangeSlider, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ProgressBar control.
/// </summary>
public class FWProgressBar : ProgressBar, IFluentJaliumControl
{
    public static readonly DependencyProperty ShowPausedProperty =
        DependencyProperty.Register(nameof(ShowPaused), typeof(bool), typeof(FWProgressBar),
            new PropertyMetadata(false));

    public static readonly DependencyProperty ShowErrorProperty =
        DependencyProperty.Register(nameof(ShowError), typeof(bool), typeof(FWProgressBar),
            new PropertyMetadata(false));

    /// <summary>
    /// Gets or sets whether the progress bar uses the paused status foreground.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public bool ShowPaused
    {
        get => (bool)GetValue(ShowPausedProperty)!;
        set => SetValue(ShowPausedProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the progress bar uses the error status foreground.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public bool ShowError
    {
        get => (bool)GetValue(ShowErrorProperty)!;
        set => SetValue(ShowErrorProperty, value);
    }
}
