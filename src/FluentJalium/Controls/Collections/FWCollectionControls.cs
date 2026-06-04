using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium ListBox control.
/// </summary>
public class FWListBox : ListBox, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item) => new FWListBoxItem();
}

/// <summary>
/// FluentJalium ListBoxItem control.
/// </summary>
public class FWListBoxItem : ListBoxItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ListView control.
/// </summary>
public class FWListView : ListView, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item) => new FWListViewItem();
}

/// <summary>
/// FluentJalium ListViewItem control.
/// </summary>
public class FWListViewItem : ListViewItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium TreeView control.
/// </summary>
public class FWTreeView : TreeView, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item)
    {
        return item is TreeViewItem treeViewItem ? treeViewItem : new FWTreeViewItem();
    }
}

/// <summary>
/// FluentJalium TreeViewItem control.
/// </summary>
public class FWTreeViewItem : TreeViewItem, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item)
    {
        return item is TreeViewItem treeViewItem ? treeViewItem : new FWTreeViewItem();
    }
}

/// <summary>
/// FluentJalium table density presets for data-heavy surfaces.
/// </summary>
public enum FWDataGridDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium DataGrid control.
/// </summary>
public class FWDataGrid : DataGrid, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDataGridDensity), typeof(FWDataGrid),
            new PropertyMetadata(FWDataGridDensity.Comfortable, OnDensityChanged));

    public FWDataGrid()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDataGridDensity Density
    {
        get => (FWDataGridDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWDataGrid grid && e.NewValue is FWDataGridDensity density)
        {
            ApplyDensity(grid, density);
        }
    }

    internal static (double RowHeight, double ColumnHeaderHeight) GetDensityMetrics(FWDataGridDensity density)
    {
        return density switch
        {
            FWDataGridDensity.Compact => (26.0, 30.0),
            FWDataGridDensity.Spacious => (40.0, 44.0),
            _ => (32.0, 36.0)
        };
    }

    private static void ApplyDensity(FWDataGrid grid, FWDataGridDensity density)
    {
        var (rowHeight, headerHeight) = GetDensityMetrics(density);
        grid.RowHeight = rowHeight;
        grid.ColumnHeaderHeight = headerHeight;
    }
}

/// <summary>
/// FluentJalium TreeDataGrid control.
/// </summary>
public class FWTreeDataGrid : TreeDataGrid, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDataGridDensity), typeof(FWTreeDataGrid),
            new PropertyMetadata(FWDataGridDensity.Comfortable, OnDensityChanged));

    public FWTreeDataGrid()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDataGridDensity Density
    {
        get => (FWDataGridDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTreeDataGrid grid && e.NewValue is FWDataGridDensity density)
        {
            ApplyDensity(grid, density);
        }
    }

    private static void ApplyDensity(FWTreeDataGrid grid, FWDataGridDensity density)
    {
        var (rowHeight, headerHeight) = FWDataGrid.GetDensityMetrics(density);
        grid.RowHeight = rowHeight;
        grid.ColumnHeaderHeight = headerHeight;
    }
}
