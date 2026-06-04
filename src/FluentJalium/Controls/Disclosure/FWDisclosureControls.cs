using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium density presets for disclosure, flyout, and dialog surfaces.
/// </summary>
public enum FWDisclosureDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium Expander control.
/// </summary>
public class FWExpander : Expander, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDisclosureDensity), typeof(FWExpander),
            new PropertyMetadata(FWDisclosureDensity.Comfortable, OnDensityChanged));

    public FWExpander()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDisclosureDensity Density
    {
        get => (FWDisclosureDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (Thickness Padding, double MinHeight) GetExpanderMetrics(FWDisclosureDensity density)
    {
        return density switch
        {
            FWDisclosureDensity.Compact => (new Thickness(10), 36.0),
            FWDisclosureDensity.Spacious => (new Thickness(18), 48.0),
            _ => (new Thickness(14), 40.0)
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWExpander expander && e.NewValue is FWDisclosureDensity density)
        {
            ApplyDensity(expander, density);
        }
    }

    private static void ApplyDensity(FWExpander expander, FWDisclosureDensity density)
    {
        var (padding, minHeight) = GetExpanderMetrics(density);
        expander.Padding = padding;
        expander.MinHeight = minHeight;
    }
}

/// <summary>
/// FluentJalium ToolTip control.
/// </summary>
public class FWToolTip : ToolTip, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDisclosureDensity), typeof(FWToolTip),
            new PropertyMetadata(FWDisclosureDensity.Comfortable, OnDensityChanged));

    public FWToolTip()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDisclosureDensity Density
    {
        get => (FWDisclosureDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (Thickness Padding, double MinHeight) GetToolTipMetrics(FWDisclosureDensity density)
    {
        return density switch
        {
            FWDisclosureDensity.Compact => (new Thickness(6, 3, 6, 3), 24.0),
            FWDisclosureDensity.Spacious => (new Thickness(10, 7, 10, 7), 32.0),
            _ => (new Thickness(8, 5, 8, 5), 28.0)
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWToolTip toolTip && e.NewValue is FWDisclosureDensity density)
        {
            ApplyDensity(toolTip, density);
        }
    }

    private static void ApplyDensity(FWToolTip toolTip, FWDisclosureDensity density)
    {
        var (padding, minHeight) = GetToolTipMetrics(density);
        toolTip.Padding = padding;
        toolTip.MinHeight = minHeight;
    }
}

/// <summary>
/// FluentJalium ContentDialog control.
/// </summary>
public class FWContentDialog : ContentDialog, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDisclosureDensity), typeof(FWContentDialog),
            new PropertyMetadata(FWDisclosureDensity.Comfortable, OnDensityChanged));

    public FWContentDialog()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDisclosureDensity Density
    {
        get => (FWDisclosureDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (Thickness Padding, double MinWidth, double MaxWidth) GetDialogMetrics(FWDisclosureDensity density)
    {
        return density switch
        {
            FWDisclosureDensity.Compact => (new Thickness(20), 300.0, 520.0),
            FWDisclosureDensity.Spacious => (new Thickness(28), 340.0, 600.0),
            _ => (new Thickness(24), 320.0, 548.0)
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWContentDialog dialog && e.NewValue is FWDisclosureDensity density)
        {
            ApplyDensity(dialog, density);
        }
    }

    private static void ApplyDensity(FWContentDialog dialog, FWDisclosureDensity density)
    {
        var (padding, minWidth, maxWidth) = GetDialogMetrics(density);
        dialog.Padding = padding;
        dialog.MinWidth = minWidth;
        dialog.MaxWidth = maxWidth;
    }
}

/// <summary>
/// FluentJalium GroupBox control.
/// </summary>
public class FWGroupBox : GroupBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDisclosureDensity), typeof(FWGroupBox),
            new PropertyMetadata(FWDisclosureDensity.Comfortable, OnDensityChanged));

    public FWGroupBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDisclosureDensity Density
    {
        get => (FWDisclosureDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (Thickness Padding, double MinHeight) GetGroupBoxMetrics(FWDisclosureDensity density)
    {
        return density switch
        {
            FWDisclosureDensity.Compact => (new Thickness(10, 12, 10, 10), 48.0),
            FWDisclosureDensity.Spacious => (new Thickness(18, 20, 18, 18), 72.0),
            _ => (new Thickness(14, 16, 14, 14), 56.0)
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWGroupBox groupBox && e.NewValue is FWDisclosureDensity density)
        {
            ApplyDensity(groupBox, density);
        }
    }

    private static void ApplyDensity(FWGroupBox groupBox, FWDisclosureDensity density)
    {
        var (padding, minHeight) = GetGroupBoxMetrics(density);
        groupBox.Padding = padding;
        groupBox.MinHeight = minHeight;
    }
}
