using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium selection density presets for compact data surfaces and spacious touch targets.
/// </summary>
public enum FWSelectionDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium CheckBox control.
/// </summary>
public class FWCheckBox : CheckBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWSelectionDensity), typeof(FWCheckBox),
            new PropertyMetadata(FWSelectionDensity.Comfortable, OnDensityChanged));

    public FWCheckBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSelectionDensity Density
    {
        get => (FWSelectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWSelectionDensity density)
    {
        return density switch
        {
            FWSelectionDensity.Compact => (22.0, new Thickness(6, 0, 0, 0)),
            FWSelectionDensity.Spacious => (32.0, new Thickness(10, 0, 0, 0)),
            _ => (24.0, new Thickness(8, 0, 0, 0))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWCheckBox checkBox && e.NewValue is FWSelectionDensity density)
        {
            ApplyDensity(checkBox, density);
        }
    }

    private static void ApplyDensity(FWCheckBox checkBox, FWSelectionDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        checkBox.MinHeight = minHeight;
        checkBox.Padding = padding;
    }
}

/// <summary>
/// FluentJalium RadioButton control.
/// </summary>
public class FWRadioButton : RadioButton, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWSelectionDensity), typeof(FWRadioButton),
            new PropertyMetadata(FWSelectionDensity.Comfortable, OnDensityChanged));

    public FWRadioButton()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSelectionDensity Density
    {
        get => (FWSelectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRadioButton radioButton && e.NewValue is FWSelectionDensity density)
        {
            ApplyDensity(radioButton, density);
        }
    }

    private static void ApplyDensity(FWRadioButton radioButton, FWSelectionDensity density)
    {
        var (minHeight, padding) = FWCheckBox.GetDensityMetrics(density);
        radioButton.MinHeight = minHeight;
        radioButton.Padding = padding;
    }
}

/// <summary>
/// FluentJalium ComboBox control.
/// </summary>
public class FWComboBox : ComboBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWSelectionDensity), typeof(FWComboBox),
            new PropertyMetadata(FWSelectionDensity.Comfortable, OnDensityChanged));

    public FWComboBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSelectionDensity Density
    {
        get => (FWSelectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    protected override FrameworkElement GetContainerForItem(object item)
    {
        return new FWComboBoxItem { Density = Density };
    }

    protected override void PrepareContainerForItem(FrameworkElement element, object item)
    {
        base.PrepareContainerForItem(element, item);

        if (element is FWComboBoxItem comboBoxItem && !ReferenceEquals(element, item))
        {
            comboBoxItem.Density = Density;
        }
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWComboBox comboBox && e.NewValue is FWSelectionDensity density)
        {
            ApplyDensity(comboBox, density);
            comboBox.ApplyGeneratedItemDensity(density);
        }
    }

    internal static (double MinHeight, double MinWidth, Thickness Padding) GetDensityMetrics(FWSelectionDensity density)
    {
        return density switch
        {
            FWSelectionDensity.Compact => (30.0, 120.0, new Thickness(8, 4, 8, 5)),
            FWSelectionDensity.Spacious => (40.0, 144.0, new Thickness(12, 8, 10, 8)),
            _ => (34.0, 120.0, new Thickness(10, 5, 8, 6))
        };
    }

    private static void ApplyDensity(FWComboBox comboBox, FWSelectionDensity density)
    {
        var (minHeight, minWidth, padding) = GetDensityMetrics(density);
        comboBox.MinHeight = minHeight;
        comboBox.MinWidth = minWidth;
        comboBox.Padding = padding;
    }

    private void ApplyGeneratedItemDensity(FWSelectionDensity density)
    {
        var host = ItemsHost;
        if (host == null)
        {
            return;
        }

        foreach (var child in host.Children)
        {
            if (child is FWComboBoxItem item && !ReferenceEquals(item, item.Tag))
            {
                item.Density = density;
            }
        }
    }
}

/// <summary>
/// FluentJalium ComboBoxItem control.
/// </summary>
public class FWComboBoxItem : ComboBoxItem, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWSelectionDensity), typeof(FWComboBoxItem),
            new PropertyMetadata(FWSelectionDensity.Comfortable, OnDensityChanged));

    public FWComboBoxItem()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSelectionDensity Density
    {
        get => (FWSelectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWComboBoxItem item && e.NewValue is FWSelectionDensity density)
        {
            ApplyDensity(item, density);
        }
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWSelectionDensity density)
    {
        return density switch
        {
            FWSelectionDensity.Compact => (28.0, new Thickness(8, 4, 8, 4)),
            FWSelectionDensity.Spacious => (38.0, new Thickness(12, 8, 12, 8)),
            _ => (32.0, new Thickness(9, 6, 10, 6))
        };
    }

    private static void ApplyDensity(FWComboBoxItem item, FWSelectionDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        item.MinHeight = minHeight;
        item.Padding = padding;
    }
}
