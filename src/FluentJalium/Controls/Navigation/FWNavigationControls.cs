using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium density presets for navigation and tab surfaces.
/// </summary>
public enum FWNavigationDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium NavigationView control.
/// </summary>
public class FWNavigationView : NavigationView, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWNavigationDensity), typeof(FWNavigationView),
            new PropertyMetadata(FWNavigationDensity.Comfortable, OnDensityChanged));

    public FWNavigationView()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWNavigationDensity Density
    {
        get => (FWNavigationDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double OpenPaneLength, double CompactPaneLength) GetPaneMetrics(FWNavigationDensity density)
    {
        return density switch
        {
            FWNavigationDensity.Compact => (240.0, 40.0),
            FWNavigationDensity.Spacious => (320.0, 56.0),
            _ => (280.0, 48.0)
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWNavigationView navigationView && e.NewValue is FWNavigationDensity density)
        {
            ApplyDensity(navigationView, density);
        }
    }

    private static void ApplyDensity(FWNavigationView navigationView, FWNavigationDensity density)
    {
        var (openPaneLength, compactPaneLength) = GetPaneMetrics(density);
        navigationView.OpenPaneLength = openPaneLength;
        navigationView.CompactPaneLength = compactPaneLength;
    }
}

/// <summary>
/// FluentJalium NavigationViewItem control.
/// </summary>
public class FWNavigationViewItem : NavigationViewItem, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWNavigationDensity), typeof(FWNavigationViewItem),
            new PropertyMetadata(FWNavigationDensity.Comfortable, OnDensityChanged));

    public FWNavigationViewItem()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWNavigationDensity Density
    {
        get => (FWNavigationDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double MinHeight, Thickness Margin) GetItemMetrics(FWNavigationDensity density)
    {
        return density switch
        {
            FWNavigationDensity.Compact => (32.0, new Thickness(4, 1, 4, 1)),
            FWNavigationDensity.Spacious => (44.0, new Thickness(8, 2, 8, 2)),
            _ => (36.0, new Thickness(6, 2, 6, 2))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWNavigationViewItem item && e.NewValue is FWNavigationDensity density)
        {
            ApplyDensity(item, density);
        }
    }

    private static void ApplyDensity(FWNavigationViewItem item, FWNavigationDensity density)
    {
        var (minHeight, margin) = GetItemMetrics(density);
        item.MinHeight = minHeight;
        item.Margin = margin;
    }
}

/// <summary>
/// FluentJalium NavigationViewItemHeader control.
/// </summary>
public class FWNavigationViewItemHeader : NavigationViewItemHeader, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium NavigationViewItemSeparator control.
/// </summary>
public class FWNavigationViewItemSeparator : NavigationViewItemSeparator, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium TabControl control.
/// </summary>
public class FWTabControl : TabControl, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWNavigationDensity), typeof(FWTabControl),
            new PropertyMetadata(FWNavigationDensity.Comfortable, OnDensityChanged));

    public FWTabControl()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWNavigationDensity Density
    {
        get => (FWNavigationDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static double GetTabStripHeight(FWNavigationDensity density)
    {
        return density switch
        {
            FWNavigationDensity.Compact => 36.0,
            FWNavigationDensity.Spacious => 48.0,
            _ => 40.0
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTabControl tabControl && e.NewValue is FWNavigationDensity density)
        {
            ApplyDensity(tabControl, density);
        }
    }

    private static void ApplyDensity(FWTabControl tabControl, FWNavigationDensity density)
    {
        tabControl.TabStripHeight = GetTabStripHeight(density);
    }
}

/// <summary>
/// FluentJalium TabItem control.
/// </summary>
public class FWTabItem : TabItem, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWNavigationDensity), typeof(FWTabItem),
            new PropertyMetadata(FWNavigationDensity.Comfortable, OnDensityChanged));

    public FWTabItem()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWNavigationDensity Density
    {
        get => (FWNavigationDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double MinHeight, Thickness Padding) GetTabItemMetrics(FWNavigationDensity density)
    {
        return density switch
        {
            FWNavigationDensity.Compact => (32.0, new Thickness(12, 7, 12, 7)),
            FWNavigationDensity.Spacious => (44.0, new Thickness(18, 12, 18, 12)),
            _ => (36.0, new Thickness(16, 9, 16, 9))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTabItem tabItem && e.NewValue is FWNavigationDensity density)
        {
            ApplyDensity(tabItem, density);
        }
    }

    private static void ApplyDensity(FWTabItem tabItem, FWNavigationDensity density)
    {
        var (minHeight, padding) = GetTabItemMetrics(density);
        tabItem.MinHeight = minHeight;
        tabItem.Padding = padding;
    }
}

/// <summary>
/// FluentJalium Frame control.
/// </summary>
public class FWFrame : Frame, IFluentJaliumControl
{
}
