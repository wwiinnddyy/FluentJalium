using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium date and time picker density presets.
/// </summary>
public enum FWDateTimePickerDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium DatePicker control.
/// </summary>
public class FWDatePicker : DatePicker, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDateTimePickerDensity), typeof(FWDatePicker),
            new PropertyMetadata(FWDateTimePickerDensity.Comfortable, OnDensityChanged));

    public FWDatePicker()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDateTimePickerDensity Density
    {
        get => (FWDateTimePickerDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWDatePicker picker && e.NewValue is FWDateTimePickerDensity density)
        {
            ApplyDensity(picker, density);
        }
    }

    internal static (double MinHeight, double MinWidth, Thickness Padding) GetDensityMetrics(FWDateTimePickerDensity density)
    {
        return density switch
        {
            FWDateTimePickerDensity.Compact => (30.0, 180.0, new Thickness(8, 4, 30, 5)),
            FWDateTimePickerDensity.Spacious => (36.0, 240.0, new Thickness(12, 7, 38, 8)),
            _ => (32.0, 200.0, new Thickness(10, 5, 34, 6))
        };
    }

    private static void ApplyDensity(FWDatePicker picker, FWDateTimePickerDensity density)
    {
        var (minHeight, minWidth, padding) = GetDensityMetrics(density);
        picker.MinHeight = minHeight;
        picker.MinWidth = minWidth;
        picker.Padding = padding;
    }
}

/// <summary>
/// FluentJalium TimePicker control.
/// </summary>
public class FWTimePicker : TimePicker, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDateTimePickerDensity), typeof(FWTimePicker),
            new PropertyMetadata(FWDateTimePickerDensity.Comfortable, OnDensityChanged));

    public FWTimePicker()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDateTimePickerDensity Density
    {
        get => (FWDateTimePickerDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTimePicker picker && e.NewValue is FWDateTimePickerDensity density)
        {
            ApplyDensity(picker, density);
        }
    }

    private static (double MinHeight, double MinWidth, Thickness Padding) GetDensityMetrics(FWDateTimePickerDensity density)
    {
        return density switch
        {
            FWDateTimePickerDensity.Compact => (30.0, 140.0, new Thickness(8, 4, 30, 5)),
            FWDateTimePickerDensity.Spacious => (36.0, 200.0, new Thickness(12, 7, 38, 8)),
            _ => (32.0, 160.0, new Thickness(10, 5, 34, 6))
        };
    }

    private static void ApplyDensity(FWTimePicker picker, FWDateTimePickerDensity density)
    {
        var (minHeight, minWidth, padding) = GetDensityMetrics(density);
        picker.MinHeight = minHeight;
        picker.MinWidth = minWidth;
        picker.Padding = padding;
    }
}

/// <summary>
/// FluentJalium Calendar control.
/// </summary>
public class FWCalendar : Calendar, IFluentJaliumControl
{
}
