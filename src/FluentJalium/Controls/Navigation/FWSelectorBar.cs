using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;

namespace FluentJalium.Controls;

/// <summary>
/// Describes where the selector bar draws its selected item indicator.
/// </summary>
public enum FWSelectorBarSelectionIndicatorPlacement
{
    Auto,
    Bottom,
    Top,
    Left,
    Right
}

/// <summary>
/// FluentJalium SelectorBar control for compact in-page view switching.
/// </summary>
public class FWSelectorBar : Selector, IFluentJaliumControl
{
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(FWSelectorBar),
            new PropertyMetadata(Orientation.Horizontal, OnLayoutStateChanged));

    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWNavigationDensity), typeof(FWSelectorBar),
            new PropertyMetadata(FWNavigationDensity.Comfortable, OnDensityChanged), IsValidNavigationDensity);

    public static readonly DependencyProperty SelectionIndicatorPlacementProperty =
        DependencyProperty.Register(nameof(SelectionIndicatorPlacement), typeof(FWSelectorBarSelectionIndicatorPlacement), typeof(FWSelectorBar),
            new PropertyMetadata(FWSelectorBarSelectionIndicatorPlacement.Auto, OnLayoutStateChanged), IsValidIndicatorPlacement);

    public FWSelectorBar()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty)!;
        set => SetValue(OrientationProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWNavigationDensity Density
    {
        get => (FWNavigationDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public FWSelectorBarSelectionIndicatorPlacement SelectionIndicatorPlacement
    {
        get => (FWSelectorBarSelectionIndicatorPlacement)GetValue(SelectionIndicatorPlacementProperty)!;
        set => SetValue(SelectionIndicatorPlacementProperty, value);
    }

    public void SelectItem(FWSelectorBarItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        SelectedItem = item;
    }

    public bool TrySelectIndex(int index)
    {
        if (index < 0 || index >= Items.Count)
        {
            return false;
        }

        SelectedIndex = index;
        return true;
    }

    public FWSelectorBarDiagnostics GetDiagnostics()
    {
        return new FWSelectorBarDiagnostics(
            Items.Count,
            SelectedIndex,
            SelectedItem,
            SelectedItem is FWSelectorBarItem selectorItem ? selectorItem.Text : SelectedItem?.ToString() ?? string.Empty,
            SelectedItem != null,
            Orientation,
            Density,
            SelectionIndicatorPlacement,
            MinHeight,
            Padding);
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        UpdateContainerSelection();
        base.OnSelectionChanged(e);
        InvalidateVisual();
    }

    protected override void UpdateContainerSelection()
    {
        foreach (var item in Items)
        {
            if (item is FWSelectorBarItem selectorItem)
            {
                selectorItem.IsSelected = ReferenceEquals(selectorItem, SelectedItem);
            }
        }
    }

    protected override Panel CreateItemsPanel()
    {
        if (ItemsPanel != null)
        {
            return base.CreateItemsPanel();
        }

        return new StackPanel
        {
            Orientation = Orientation,
            Spacing = 4
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSelectorBar selectorBar && e.NewValue is FWNavigationDensity density)
        {
            ApplyDensity(selectorBar, density);
        }
    }

    private static void ApplyDensity(FWSelectorBar selectorBar, FWNavigationDensity density)
    {
        var (minHeight, padding) = GetSelectorBarMetrics(density);
        selectorBar.MinHeight = minHeight;
        selectorBar.Padding = padding;
    }

    internal static (double MinHeight, Thickness Padding) GetSelectorBarMetrics(FWNavigationDensity density)
    {
        return density switch
        {
            FWNavigationDensity.Compact => (32.0, new Thickness(4)),
            FWNavigationDensity.Spacious => (48.0, new Thickness(8)),
            _ => (40.0, new Thickness(6))
        };
    }

    private static void OnLayoutStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSelectorBar selectorBar)
        {
            selectorBar.UpdateItemsPanelOrientation();
            selectorBar.InvalidateMeasure();
            selectorBar.InvalidateVisual();
        }
    }

    private void UpdateItemsPanelOrientation()
    {
        if (ItemsPanel == null && ItemsHost is StackPanel panel)
        {
            panel.Orientation = Orientation;
            panel.InvalidateMeasure();
        }
    }

    private static bool IsValidNavigationDensity(object? value)
    {
        return value is FWNavigationDensity density && Enum.IsDefined(density);
    }

    private static bool IsValidIndicatorPlacement(object? value)
    {
        return value is FWSelectorBarSelectionIndicatorPlacement placement && Enum.IsDefined(placement);
    }
}

/// <summary>
/// Snapshot of selector bar state for Gallery diagnostics and tests.
/// </summary>
public readonly record struct FWSelectorBarDiagnostics(
    int ItemCount,
    int SelectedIndex,
    object? SelectedItem,
    string SelectedText,
    bool HasSelection,
    Orientation Orientation,
    FWNavigationDensity Density,
    FWSelectorBarSelectionIndicatorPlacement SelectionIndicatorPlacement,
    double MinHeight,
    Thickness Padding);

/// <summary>
/// FluentJalium SelectorBar item.
/// </summary>
public class FWSelectorBarItem : ContentControl, IFluentJaliumControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(FWSelectorBarItem),
            new PropertyMetadata(string.Empty, OnContentStateChanged));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(FWSelectorBarItem),
            new PropertyMetadata(null, OnContentStateChanged));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(FWSelectorBarItem),
            new PropertyMetadata(false, OnContentStateChanged));

    public FWSelectorBarItem()
    {
        UseTemplateContentManagement();
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string Text
    {
        get => (string)(GetValue(TextProperty) ?? string.Empty);
        set => SetValue(TextProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty)!;
        set => SetValue(IsSelectedProperty, value);
    }

    private static void OnContentStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSelectorBarItem item)
        {
            item.InvalidateMeasure();
            item.InvalidateVisual();
        }
    }
}
