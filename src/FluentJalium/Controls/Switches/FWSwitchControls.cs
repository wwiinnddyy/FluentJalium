using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium switch density presets for command toggles and settings rows.
/// </summary>
public enum FWSwitchDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium ToggleButton control.
/// </summary>
public class FWToggleButton : ToggleButton, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWSwitchDensity), typeof(FWToggleButton),
            new PropertyMetadata(FWSwitchDensity.Comfortable, OnDensityChanged));

    public FWToggleButton()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSwitchDensity Density
    {
        get => (FWSwitchDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWToggleButton button && e.NewValue is FWSwitchDensity density)
        {
            ApplyDensity(button, density);
        }
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWSwitchDensity density)
    {
        return density switch
        {
            FWSwitchDensity.Compact => (30.0, new Thickness(10, 4, 10, 5)),
            FWSwitchDensity.Spacious => (36.0, new Thickness(14, 7, 14, 8)),
            _ => (32.0, new Thickness(12, 5, 12, 6))
        };
    }

    private static void ApplyDensity(FWToggleButton button, FWSwitchDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        button.MinHeight = minHeight;
        button.Padding = padding;
    }
}

/// <summary>
/// FluentJalium ToggleSwitch control.
/// </summary>
public class FWToggleSwitch : ToggleSwitch, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWSwitchDensity), typeof(FWToggleSwitch),
            new PropertyMetadata(FWSwitchDensity.Comfortable, OnDensityChanged));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(FWToggleSwitch),
            new PropertyMetadata(null));

    public FWToggleSwitch()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSwitchDensity Density
    {
        get => (FWSwitchDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    /// <summary>
    /// Gets or sets supporting text shown under the switch header in the FluentJalium template.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWToggleSwitch toggleSwitch && e.NewValue is FWSwitchDensity density)
        {
            ApplyDensity(toggleSwitch, density);
        }
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWSwitchDensity density)
    {
        return density switch
        {
            FWSwitchDensity.Compact => (40.0, new Thickness(8, 6, 8, 6)),
            FWSwitchDensity.Spacious => (52.0, new Thickness(12, 10, 12, 10)),
            _ => (44.0, new Thickness(10, 8, 10, 8))
        };
    }

    private static void ApplyDensity(FWToggleSwitch toggleSwitch, FWSwitchDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        toggleSwitch.MinHeight = minHeight;
        toggleSwitch.Padding = padding;
    }
}
