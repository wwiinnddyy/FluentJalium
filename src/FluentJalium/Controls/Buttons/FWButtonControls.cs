using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium button density presets for command surfaces and forms.
/// </summary>
public enum FWButtonDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium Button control.
/// </summary>
public class FWButton : Button, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWButtonDensity), typeof(FWButton),
            new PropertyMetadata(FWButtonDensity.Comfortable, OnDensityChanged));

    public FWButton()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWButtonDensity Density
    {
        get => (FWButtonDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double MinHeight, double MinWidth, Thickness Padding) GetDensityMetrics(FWButtonDensity density)
    {
        return density switch
        {
            FWButtonDensity.Compact => (30.0, 56.0, new Thickness(10, 4, 10, 5)),
            FWButtonDensity.Spacious => (40.0, 72.0, new Thickness(14, 8, 14, 8)),
            _ => (32.0, 64.0, new Thickness(12, 5, 12, 6))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWButton button && e.NewValue is FWButtonDensity density)
        {
            ApplyDensity(button, density);
        }
    }

    private static void ApplyDensity(FWButton button, FWButtonDensity density)
    {
        var (minHeight, minWidth, padding) = GetDensityMetrics(density);
        button.MinHeight = minHeight;
        button.MinWidth = minWidth;
        button.Padding = padding;
    }
}

/// <summary>
/// FluentJalium RepeatButton control.
/// </summary>
public class FWRepeatButton : RepeatButton, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWButtonDensity), typeof(FWRepeatButton),
            new PropertyMetadata(FWButtonDensity.Comfortable, OnDensityChanged));

    public FWRepeatButton()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWButtonDensity Density
    {
        get => (FWButtonDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRepeatButton button && e.NewValue is FWButtonDensity density)
        {
            ApplyDensity(button, density);
        }
    }

    private static void ApplyDensity(FWRepeatButton button, FWButtonDensity density)
    {
        var (minHeight, minWidth, padding) = FWButton.GetDensityMetrics(density);
        button.MinHeight = minHeight;
        button.MinWidth = minWidth;
        button.Padding = padding;
    }
}

/// <summary>
/// FluentJalium HyperlinkButton control.
/// </summary>
public class FWHyperlinkButton : HyperlinkButton, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWButtonDensity), typeof(FWHyperlinkButton),
            new PropertyMetadata(FWButtonDensity.Comfortable, OnDensityChanged));

    public FWHyperlinkButton()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWButtonDensity Density
    {
        get => (FWButtonDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWButtonDensity density)
    {
        return density switch
        {
            FWButtonDensity.Compact => (20.0, new Thickness(0, 0, 0, 1)),
            FWButtonDensity.Spacious => (28.0, new Thickness(0, 3, 0, 3)),
            _ => (24.0, new Thickness(0, 1, 0, 2))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWHyperlinkButton button && e.NewValue is FWButtonDensity density)
        {
            ApplyDensity(button, density);
        }
    }

    private static void ApplyDensity(FWHyperlinkButton button, FWButtonDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        button.MinHeight = minHeight;
        button.Padding = padding;
    }
}

/// <summary>
/// FluentJalium SplitButton control.
/// </summary>
public class FWSplitButton : SplitButton, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWButtonDensity), typeof(FWSplitButton),
            new PropertyMetadata(FWButtonDensity.Comfortable, OnDensityChanged));

    public FWSplitButton()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWButtonDensity Density
    {
        get => (FWButtonDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSplitButton button && e.NewValue is FWButtonDensity density)
        {
            ApplyDensity(button, density);
        }
    }

    private static void ApplyDensity(FWSplitButton button, FWButtonDensity density)
    {
        var (minHeight, minWidth, padding) = FWButton.GetDensityMetrics(density);
        button.MinHeight = minHeight;
        button.MinWidth = minWidth;
        button.Padding = padding;
    }
}

/// <summary>
/// FluentJalium CommandBar control.
/// </summary>
public class FWCommandBar : CommandBar, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium AppBarButton control.
/// </summary>
public class FWAppBarButton : AppBarButton, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWButtonDensity), typeof(FWAppBarButton),
            new PropertyMetadata(FWButtonDensity.Comfortable, OnDensityChanged));

    public FWAppBarButton()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWButtonDensity Density
    {
        get => (FWButtonDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double MinHeight, double MinWidth, Thickness Padding) GetAppBarDensityMetrics(FWButtonDensity density)
    {
        return density switch
        {
            FWButtonDensity.Compact => (40.0, 40.0, new Thickness(4, 2, 4, 2)),
            FWButtonDensity.Spacious => (56.0, 48.0, new Thickness(6, 6, 6, 6)),
            _ => (48.0, 40.0, new Thickness(4, 4, 4, 4))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWAppBarButton button && e.NewValue is FWButtonDensity density)
        {
            ApplyDensity(button, density);
        }
    }

    private static void ApplyDensity(FWAppBarButton button, FWButtonDensity density)
    {
        var (minHeight, minWidth, padding) = GetAppBarDensityMetrics(density, button.IsCompact);
        button.MinHeight = minHeight;
        button.MinWidth = minWidth;
        button.Padding = padding;
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == AppBarButton.IsCompactProperty)
        {
            ApplyDensity(this, Density);
        }
    }

    private static (double MinHeight, double MinWidth, Thickness Padding) GetAppBarDensityMetrics(
        FWButtonDensity density,
        bool isCompact)
    {
        if (isCompact)
        {
            return density switch
            {
                FWButtonDensity.Spacious => (48.0, 48.0, new Thickness(6, 4, 6, 4)),
                _ => (40.0, 40.0, new Thickness(4, 2, 4, 2))
            };
        }

        return GetAppBarDensityMetrics(density);
    }
}

/// <summary>
/// FluentJalium AppBarToggleButton control.
/// </summary>
public class FWAppBarToggleButton : AppBarToggleButton, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWButtonDensity), typeof(FWAppBarToggleButton),
            new PropertyMetadata(FWButtonDensity.Comfortable, OnDensityChanged));

    public FWAppBarToggleButton()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWButtonDensity Density
    {
        get => (FWButtonDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWAppBarToggleButton button && e.NewValue is FWButtonDensity density)
        {
            ApplyDensity(button, density);
        }
    }

    private static void ApplyDensity(FWAppBarToggleButton button, FWButtonDensity density)
    {
        var (minHeight, minWidth, padding) = GetAppBarDensityMetrics(density, button.IsCompact);
        button.MinHeight = minHeight;
        button.MinWidth = minWidth;
        button.Padding = padding;
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == AppBarToggleButton.IsCompactProperty)
        {
            ApplyDensity(this, Density);
        }
    }

    private static (double MinHeight, double MinWidth, Thickness Padding) GetAppBarDensityMetrics(
        FWButtonDensity density,
        bool isCompact)
    {
        if (isCompact)
        {
            return density switch
            {
                FWButtonDensity.Spacious => (48.0, 48.0, new Thickness(6, 4, 6, 4)),
                _ => (40.0, 40.0, new Thickness(4, 2, 4, 2))
            };
        }

        return FWAppBarButton.GetAppBarDensityMetrics(density);
    }
}

/// <summary>
/// FluentJalium AppBarSeparator control.
/// </summary>
public class FWAppBarSeparator : AppBarSeparator, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWButtonDensity), typeof(FWAppBarSeparator),
            new PropertyMetadata(FWButtonDensity.Comfortable, OnDensityChanged));

    public FWAppBarSeparator()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWButtonDensity Density
    {
        get => (FWButtonDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(1, GetSeparatorHeight(Density, IsCompact));
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == AppBarSeparator.IsCompactProperty)
        {
            InvalidateMeasure();
        }
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWAppBarSeparator separator && e.NewValue is FWButtonDensity density)
        {
            ApplyDensity(separator, density);
        }
    }

    private static void ApplyDensity(FWAppBarSeparator separator, FWButtonDensity density)
    {
        separator.Width = 1;
        separator.Margin = density switch
        {
            FWButtonDensity.Compact => new Thickness(4, 6, 4, 6),
            FWButtonDensity.Spacious => new Thickness(8, 10, 8, 10),
            _ => new Thickness(6, 8, 6, 8)
        };

        separator.InvalidateMeasure();
    }

    private static double GetSeparatorHeight(FWButtonDensity density, bool isCompact)
    {
        return density switch
        {
            FWButtonDensity.Compact => isCompact ? 20.0 : 28.0,
            FWButtonDensity.Spacious => isCompact ? 28.0 : 40.0,
            _ => isCompact ? 24.0 : 32.0
        };
    }
}

/// <summary>
/// FluentJalium ToolBar control.
/// </summary>
public class FWToolBar : Jalium.UI.Controls.ToolBar, IFluentJaliumControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FWToolBar"/> class.
    /// </summary>
    public FWToolBar()
    {
        var template = new ItemsPanelTemplate();
        template.SetVisualTree(() => new StackPanel { Orientation = Orientation.Horizontal });
        template.Seal();
        ItemsPanel = template;
    }
}

/// <summary>
/// FluentJalium ToolBarTray control.
/// </summary>
public class FWToolBarTray : Jalium.UI.Controls.ToolBarTray, IFluentJaliumControl
{
}
