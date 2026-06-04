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
}

/// <summary>
/// FluentJalium AppBarToggleButton control.
/// </summary>
public class FWAppBarToggleButton : AppBarToggleButton, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium AppBarSeparator control.
/// </summary>
public class FWAppBarSeparator : AppBarSeparator, IFluentJaliumControl
{
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
