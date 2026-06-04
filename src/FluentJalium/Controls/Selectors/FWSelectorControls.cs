using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium TreeSelector control.
/// </summary>
public class FWTreeSelector : TreeSelector, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item)
    {
        return item is TreeSelectorItem treeSelectorItem ? treeSelectorItem : new FWTreeSelectorItem();
    }
}

/// <summary>
/// FluentJalium TreeSelectorItem control.
/// </summary>
public class FWTreeSelectorItem : TreeSelectorItem, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item)
    {
        return item is TreeSelectorItem treeSelectorItem ? treeSelectorItem : new FWTreeSelectorItem();
    }
}

/// <summary>
/// FluentJalium PropertyGrid density presets for inspector and settings surfaces.
/// </summary>
public enum FWPropertyGridDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium PropertyGrid control.
/// </summary>
public class FWPropertyGrid : PropertyGrid, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWPropertyGridDensity), typeof(FWPropertyGrid),
            new PropertyMetadata(FWPropertyGridDensity.Comfortable, OnDensityChanged));

    public FWPropertyGrid()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWPropertyGridDensity Density
    {
        get => (FWPropertyGridDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWPropertyGrid grid && e.NewValue is FWPropertyGridDensity density)
        {
            ApplyDensity(grid, density);
        }
    }

    internal static double GetNameColumnWidth(FWPropertyGridDensity density)
    {
        return density switch
        {
            FWPropertyGridDensity.Compact => 128.0,
            FWPropertyGridDensity.Spacious => 176.0,
            _ => 150.0
        };
    }

    private static void ApplyDensity(FWPropertyGrid grid, FWPropertyGridDensity density)
    {
        grid.NameColumnWidth = GetNameColumnWidth(density);
    }
}
