using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Media;
using FluentJalium.Icon;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium density presets for menu bars, menu items, and flyout surfaces.
/// </summary>
public enum FWMenuDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium MenuBar control.
/// </summary>
public class FWMenuBar : MenuBar, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWMenuDensity), typeof(FWMenuBar),
            new PropertyMetadata(FWMenuDensity.Comfortable, OnDensityChanged));

    public FWMenuBar()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWMenuDensity Density
    {
        get => (FWMenuDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWMenuBar menuBar && e.NewValue is FWMenuDensity density)
        {
            ApplyDensity(menuBar, density);
        }
    }

    private static void ApplyDensity(FWMenuBar menuBar, FWMenuDensity density)
    {
        var (minHeight, padding) = FWMenuDensityMetrics.GetMenuBarMetrics(density);
        menuBar.MinHeight = minHeight;
        menuBar.Padding = padding;
    }
}

/// <summary>
/// FluentJalium MenuBarItem control.
/// </summary>
public class FWMenuBarItem : MenuBarItem, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWMenuDensity), typeof(FWMenuBarItem),
            new PropertyMetadata(FWMenuDensity.Comfortable, OnDensityChanged));

    public FWMenuBarItem()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWMenuDensity Density
    {
        get => (FWMenuDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWMenuBarItem item && e.NewValue is FWMenuDensity density)
        {
            ApplyDensity(item, density);
        }
    }

    private static void ApplyDensity(FWMenuBarItem item, FWMenuDensity density)
    {
        var (height, padding) = FWMenuDensityMetrics.GetMenuBarItemMetrics(density);
        item.Height = height;
        item.MinHeight = height;
        item.Padding = padding;
    }
}

/// <summary>
/// FluentJalium Menu control.
/// </summary>
public class FWMenu : Menu, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWMenuDensity), typeof(FWMenu),
            new PropertyMetadata(FWMenuDensity.Comfortable, OnDensityChanged));

    public FWMenu()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWMenuDensity Density
    {
        get => (FWMenuDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    protected override FrameworkElement GetContainerForItem(object item) => new FWMenuItem { Density = Density };

    protected override bool IsItemItsOwnContainer(object item) => item is MenuItem or Separator;

    protected override void PrepareContainerForItem(FrameworkElement element, object item)
    {
        base.PrepareContainerForItem(element, item);

        if (!ReferenceEquals(element, item) && element is FWMenuItem menuItem)
        {
            menuItem.Density = Density;
            menuItem.Header ??= item;
        }
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWMenu menu && e.NewValue is FWMenuDensity density)
        {
            ApplyDensity(menu, density);
        }
    }

    private static void ApplyDensity(FWMenu menu, FWMenuDensity density)
    {
        var (height, padding) = FWMenuDensityMetrics.GetMenuMetrics(density);
        menu.Height = height;
        menu.MinHeight = height;
        menu.Padding = padding;
    }
}

/// <summary>
/// FluentJalium MenuItem control.
/// </summary>
public class FWMenuItem : MenuItem, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWMenuDensity), typeof(FWMenuItem),
            new PropertyMetadata(FWMenuDensity.Comfortable, OnDensityChanged));

    public FWMenuItem()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWMenuDensity Density
    {
        get => (FWMenuDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    protected override FrameworkElement GetContainerForItem(object item) => new FWMenuItem { Density = Density };

    protected override bool IsItemItsOwnContainer(object item) => item is MenuItem or Separator;

    protected override void PrepareContainerForItem(FrameworkElement element, object item)
    {
        base.PrepareContainerForItem(element, item);

        if (!ReferenceEquals(element, item) && element is FWMenuItem menuItem)
        {
            menuItem.Density = Density;
            menuItem.Header ??= item;
        }
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWMenuItem item && e.NewValue is FWMenuDensity density)
        {
            ApplyDensity(item, density);
        }
    }

    private static void ApplyDensity(FWMenuItem item, FWMenuDensity density)
    {
        FWMenuDensityMetrics.ApplyMenuItemMetrics(item, density);
    }
}

/// <summary>
/// FluentJalium ContextMenu control.
/// </summary>
public class FWContextMenu : ContextMenu, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWMenuDensity), typeof(FWContextMenu),
            new PropertyMetadata(FWMenuDensity.Comfortable, OnDensityChanged));

    public FWContextMenu()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWMenuDensity Density
    {
        get => (FWMenuDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    protected override FrameworkElement GetContainerForItem(object item) => new FWMenuItem { Density = Density };

    protected override bool IsItemItsOwnContainer(object item) => item is MenuItem or Separator;

    protected override void PrepareContainerForItem(FrameworkElement element, object item)
    {
        base.PrepareContainerForItem(element, item);

        if (!ReferenceEquals(element, item) && element is FWMenuItem menuItem)
        {
            menuItem.Density = Density;
            menuItem.Header ??= item;
        }
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWContextMenu contextMenu && e.NewValue is FWMenuDensity density)
        {
            ApplyDensity(contextMenu, density);
        }
    }

    private static void ApplyDensity(FWContextMenu contextMenu, FWMenuDensity density)
    {
        var (padding, cornerRadius) = FWMenuDensityMetrics.GetMenuFlyoutSurfaceMetrics(density);
        contextMenu.Padding = padding;
        contextMenu.CornerRadius = cornerRadius;
    }
}

/// <summary>
/// FluentJalium MenuFlyoutItem control.
/// </summary>
public class FWMenuFlyoutItem : FluentMenuFlyoutItemBase, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ToggleMenuFlyoutItem control.
/// </summary>
public class FWToggleMenuFlyoutItem : FluentToggleMenuFlyoutItemBase, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium MenuFlyoutSeparator control.
/// </summary>
public class FWMenuFlyoutSeparator : MenuFlyoutSeparator, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWMenuDensity), typeof(FWMenuFlyoutSeparator),
            new PropertyMetadata(FWMenuDensity.Comfortable, OnDensityChanged));

    public FWMenuFlyoutSeparator()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWMenuDensity Density
    {
        get => (FWMenuDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWMenuFlyoutSeparator separator && e.NewValue is FWMenuDensity density)
        {
            ApplyDensity(separator, density);
        }
    }

    private static void ApplyDensity(FWMenuFlyoutSeparator separator, FWMenuDensity density)
    {
        FWMenuDensityMetrics.ApplyMenuFlyoutSeparatorMetrics(separator, density);
    }
}

public abstract class FluentMenuFlyoutItemBase : MenuFlyoutItem
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWMenuDensity), typeof(FluentMenuFlyoutItemBase),
            new PropertyMetadata(FWMenuDensity.Comfortable, OnDensityChanged));

    protected FluentMenuFlyoutItemBase()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWMenuDensity Density
    {
        get => (FWMenuDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        FWMenuFlyoutIconRenderer.DrawFluentIcon(this, drawingContext, Icon);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentMenuFlyoutItemBase item && e.NewValue is FWMenuDensity density)
        {
            ApplyDensity(item, density);
        }
    }

    private static void ApplyDensity(FluentMenuFlyoutItemBase item, FWMenuDensity density)
    {
        FWMenuDensityMetrics.ApplyMenuFlyoutItemMetrics(item, density);
    }
}

public abstract class FluentToggleMenuFlyoutItemBase : ToggleMenuFlyoutItem
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWMenuDensity), typeof(FluentToggleMenuFlyoutItemBase),
            new PropertyMetadata(FWMenuDensity.Comfortable, OnDensityChanged));

    protected FluentToggleMenuFlyoutItemBase()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWMenuDensity Density
    {
        get => (FWMenuDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        FWMenuFlyoutIconRenderer.DrawFluentIcon(this, drawingContext, Icon);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentToggleMenuFlyoutItemBase item && e.NewValue is FWMenuDensity density)
        {
            ApplyDensity(item, density);
        }
    }

    private static void ApplyDensity(FluentToggleMenuFlyoutItemBase item, FWMenuDensity density)
    {
        FWMenuDensityMetrics.ApplyMenuFlyoutItemMetrics(item, density);
    }
}

internal static class FWMenuDensityMetrics
{
    public static (double MinHeight, Thickness Padding) GetMenuBarMetrics(FWMenuDensity density)
    {
        return density switch
        {
            FWMenuDensity.Compact => (28.0, new Thickness(2, 0, 2, 0)),
            FWMenuDensity.Spacious => (40.0, new Thickness(6, 0, 6, 0)),
            _ => (32.0, new Thickness(4, 0, 4, 0))
        };
    }

    public static (double Height, Thickness Padding) GetMenuMetrics(FWMenuDensity density)
    {
        return density switch
        {
            FWMenuDensity.Compact => (28.0, new Thickness(2, 0, 2, 0)),
            FWMenuDensity.Spacious => (40.0, new Thickness(6, 0, 6, 0)),
            _ => (32.0, new Thickness(4, 0, 4, 0))
        };
    }

    public static (double Height, Thickness Padding) GetMenuBarItemMetrics(FWMenuDensity density)
    {
        return density switch
        {
            FWMenuDensity.Compact => (28.0, new Thickness(10, 2, 10, 2)),
            FWMenuDensity.Spacious => (40.0, new Thickness(16, 8, 16, 8)),
            _ => (32.0, new Thickness(12, 4, 12, 4))
        };
    }

    public static (double Height, Thickness Padding) GetMenuItemMetrics(FWMenuDensity density)
    {
        return density switch
        {
            FWMenuDensity.Compact => (28.0, new Thickness(10, 0, 10, 0)),
            FWMenuDensity.Spacious => (40.0, new Thickness(14, 4, 14, 4)),
            _ => (32.0, new Thickness(12, 0, 12, 0))
        };
    }

    public static (Thickness Padding, CornerRadius CornerRadius) GetMenuFlyoutSurfaceMetrics(FWMenuDensity density)
    {
        return density switch
        {
            FWMenuDensity.Compact => (new Thickness(3), new CornerRadius(6)),
            FWMenuDensity.Spacious => (new Thickness(6), new CornerRadius(10)),
            _ => (new Thickness(4), new CornerRadius(8))
        };
    }

    public static Thickness GetMenuFlyoutSeparatorMargin(FWMenuDensity density)
    {
        return density switch
        {
            FWMenuDensity.Compact => new Thickness(0, 3, 0, 3),
            FWMenuDensity.Spacious => new Thickness(0, 6, 0, 6),
            _ => new Thickness(0, 4, 0, 4)
        };
    }

    public static void ApplyMenuItemMetrics(Control item, FWMenuDensity density)
    {
        var (height, padding) = GetMenuItemMetrics(density);
        item.Height = height;
        item.MinHeight = height;
        item.Padding = padding;
    }

    public static void ApplyMenuFlyoutItemMetrics(Control item, FWMenuDensity density)
    {
        var (height, _) = GetMenuItemMetrics(density);
        item.Height = height;
        item.MinHeight = height;
    }

    public static void ApplyMenuFlyoutSeparatorMetrics(Control separator, FWMenuDensity density)
    {
        separator.Margin = GetMenuFlyoutSeparatorMargin(density);
    }
}

internal static class FWMenuFlyoutIconRenderer
{
    private const double LeftPadding = 12;
    private const double IconSize = 14;
    private static readonly SolidColorBrush s_fallbackTextBrush = new(Color.FromRgb(255, 255, 255));
    private static readonly SolidColorBrush s_fallbackDisabledTextBrush = new(Color.FromRgb(90, 90, 90));

    public static void DrawFluentIcon(Control owner, DrawingContext drawingContext, object? icon)
    {
        if (icon is not string iconText || string.IsNullOrEmpty(iconText))
        {
            return;
        }

        var foreground = owner.IsEnabled
            ? ResolveBrush(owner, "OnePopupText", "TextPrimary", s_fallbackTextBrush)
            : ResolveBrush(owner, "OneTextDisabled", "TextDisabled", s_fallbackDisabledTextBrush);
        var formatted = new FormattedText(iconText, FluentIcon.RegularFontFamily, IconSize)
        {
            Foreground = foreground
        };
        TextMeasurement.MeasureText(formatted);
        drawingContext.DrawText(formatted, new Point(LeftPadding, Math.Max(0, (owner.RenderSize.Height - formatted.Height) / 2)));
    }

    private static Brush ResolveBrush(Control owner, string primaryKey, string secondaryKey, Brush fallback)
    {
        if (owner.HasLocalValue(Control.ForegroundProperty) && owner.Foreground != null)
        {
            return owner.Foreground;
        }

        if (owner.TryFindResource(primaryKey) is Brush primary)
        {
            return primary;
        }

        if (owner.TryFindResource(secondaryKey) is Brush secondary)
        {
            return secondary;
        }

        return fallback;
    }
}
