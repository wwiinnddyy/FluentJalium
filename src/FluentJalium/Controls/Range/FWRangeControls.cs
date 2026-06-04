using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium range density presets for sliders and progress indicators.
/// </summary>
public enum FWRangeDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium Slider control.
/// </summary>
public class FWSlider : Slider, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWRangeDensity), typeof(FWSlider),
            new PropertyMetadata(FWRangeDensity.Comfortable, OnDensityChanged));

    public FWSlider()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWRangeDensity Density
    {
        get => (FWRangeDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSlider slider && e.NewValue is FWRangeDensity density)
        {
            ApplyDensity(slider, density);
        }
    }

    internal static (double MinHeight, double Height) GetDensityMetrics(FWRangeDensity density)
    {
        return density switch
        {
            FWRangeDensity.Compact => (28.0, 28.0),
            FWRangeDensity.Spacious => (40.0, 40.0),
            _ => (32.0, 32.0)
        };
    }

    private static void ApplyDensity(FWSlider slider, FWRangeDensity density)
    {
        var (minHeight, height) = GetDensityMetrics(density);
        slider.MinHeight = minHeight;
        slider.Height = height;
    }
}

/// <summary>
/// FluentJalium RangeSlider control.
/// </summary>
public class FWRangeSlider : RangeSlider, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWRangeDensity), typeof(FWRangeSlider),
            new PropertyMetadata(FWRangeDensity.Comfortable, OnDensityChanged));

    public FWRangeSlider()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWRangeDensity Density
    {
        get => (FWRangeDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRangeSlider slider && e.NewValue is FWRangeDensity density)
        {
            ApplyDensity(slider, density);
        }
    }

    private static void ApplyDensity(FWRangeSlider slider, FWRangeDensity density)
    {
        var (minHeight, height) = FWSlider.GetDensityMetrics(density);
        slider.MinHeight = minHeight;
        slider.Height = height;
    }
}

/// <summary>
/// FluentJalium ProgressBar control.
/// </summary>
public class FWProgressBar : ProgressBar, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWRangeDensity), typeof(FWProgressBar),
            new PropertyMetadata(FWRangeDensity.Comfortable, OnDensityChanged));

    public static readonly DependencyProperty ShowPausedProperty =
        DependencyProperty.Register(nameof(ShowPaused), typeof(bool), typeof(FWProgressBar),
            new PropertyMetadata(false));

    public static readonly DependencyProperty ShowErrorProperty =
        DependencyProperty.Register(nameof(ShowError), typeof(bool), typeof(FWProgressBar),
            new PropertyMetadata(false));

    public FWProgressBar()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWRangeDensity Density
    {
        get => (FWRangeDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

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

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWProgressBar progressBar && e.NewValue is FWRangeDensity density)
        {
            ApplyDensity(progressBar, density);
        }
    }

    internal static (double MinHeight, double Height, CornerRadius CornerRadius) GetDensityMetrics(FWRangeDensity density)
    {
        return density switch
        {
            FWRangeDensity.Compact => (4.0, 4.0, new CornerRadius(2)),
            FWRangeDensity.Spacious => (8.0, 8.0, new CornerRadius(4)),
            _ => (6.0, 6.0, new CornerRadius(3))
        };
    }

    private static void ApplyDensity(FWProgressBar progressBar, FWRangeDensity density)
    {
        var (minHeight, height, cornerRadius) = GetDensityMetrics(density);
        progressBar.MinHeight = minHeight;
        progressBar.Height = height;
        progressBar.CornerRadius = cornerRadius;
    }
}
