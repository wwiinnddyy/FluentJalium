using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium density presets for list and tree collection surfaces.
/// </summary>
public enum FWCollectionDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium ListBox control.
/// </summary>
public class FWListBox : ListBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWCollectionDensity), typeof(FWListBox),
            new PropertyMetadata(FWCollectionDensity.Comfortable, OnDensityChanged));

    public FWListBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWCollectionDensity Density
    {
        get => (FWCollectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWListBox listBox && e.NewValue is FWCollectionDensity density)
        {
            ApplyDensity(listBox, density);
        }
    }

    internal static (Thickness Padding, double MinItemHeight, Thickness ItemPadding) GetListDensityMetrics(FWCollectionDensity density)
    {
        return density switch
        {
            FWCollectionDensity.Compact => (new Thickness(2), 32.0, new Thickness(10, 4, 10, 4)),
            FWCollectionDensity.Spacious => (new Thickness(4), 48.0, new Thickness(16, 8, 16, 8)),
            _ => (new Thickness(3), 40.0, new Thickness(14, 6, 12, 6))
        };
    }

    internal static (Thickness Padding, double MinItemHeight, Thickness ItemPadding) GetTreeDensityMetrics(FWCollectionDensity density)
    {
        return density switch
        {
            FWCollectionDensity.Compact => (new Thickness(2), 24.0, new Thickness(8, 3, 8, 3)),
            FWCollectionDensity.Spacious => (new Thickness(4), 36.0, new Thickness(12, 7, 12, 7)),
            _ => (new Thickness(3), 28.0, new Thickness(10, 5, 10, 5))
        };
    }

    internal static void ApplyListItemDensity(Control item, FWCollectionDensity density)
    {
        var (_, minHeight, itemPadding) = GetListDensityMetrics(density);
        item.MinHeight = minHeight;
        item.Padding = itemPadding;
    }

    internal static void ApplyTreeItemDensity(Control item, FWCollectionDensity density)
    {
        var (_, minHeight, itemPadding) = GetTreeDensityMetrics(density);
        item.MinHeight = minHeight;
        item.Padding = itemPadding;
    }

    private static void ApplyDensity(FWListBox listBox, FWCollectionDensity density)
    {
        var (padding, _, _) = GetListDensityMetrics(density);
        listBox.Padding = padding;
    }

    protected override FrameworkElement GetContainerForItem(object item)
    {
        return new FWListBoxItem { Density = Density };
    }
}

/// <summary>
/// FluentJalium ListBoxItem control.
/// </summary>
public class FWListBoxItem : ListBoxItem, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWCollectionDensity), typeof(FWListBoxItem),
            new PropertyMetadata(FWCollectionDensity.Comfortable, OnDensityChanged));

    public FWListBoxItem()
    {
        FWListBox.ApplyListItemDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWCollectionDensity Density
    {
        get => (FWCollectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWListBoxItem item && e.NewValue is FWCollectionDensity density)
        {
            FWListBox.ApplyListItemDensity(item, density);
        }
    }
}

/// <summary>
/// FluentJalium ListView control.
/// </summary>
public class FWListView : ListView, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWCollectionDensity), typeof(FWListView),
            new PropertyMetadata(FWCollectionDensity.Comfortable, OnDensityChanged));

    public FWListView()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWCollectionDensity Density
    {
        get => (FWCollectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWListView listView && e.NewValue is FWCollectionDensity density)
        {
            ApplyDensity(listView, density);
        }
    }

    private static void ApplyDensity(FWListView listView, FWCollectionDensity density)
    {
        var (padding, _, _) = FWListBox.GetListDensityMetrics(density);
        listView.Padding = padding;
    }

    protected override FrameworkElement GetContainerForItem(object item)
    {
        return new FWListViewItem { Density = Density };
    }
}

/// <summary>
/// FluentJalium ListViewItem control.
/// </summary>
public class FWListViewItem : ListViewItem, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWCollectionDensity), typeof(FWListViewItem),
            new PropertyMetadata(FWCollectionDensity.Comfortable, OnDensityChanged));

    public FWListViewItem()
    {
        FWListBox.ApplyListItemDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWCollectionDensity Density
    {
        get => (FWCollectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWListViewItem item && e.NewValue is FWCollectionDensity density)
        {
            FWListBox.ApplyListItemDensity(item, density);
        }
    }
}

/// <summary>
/// FluentJalium GridView control.
/// </summary>
public class FWGridView : ListView, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWCollectionDensity), typeof(FWGridView),
            new PropertyMetadata(FWCollectionDensity.Comfortable, OnDensityChanged));

    public FWGridView()
    {
        View = new Jalium.UI.Controls.GridView();
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWCollectionDensity Density
    {
        get => (FWCollectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWGridView gridView && e.NewValue is FWCollectionDensity density)
        {
            ApplyDensity(gridView, density);
        }
    }

    private static void ApplyDensity(FWGridView gridView, FWCollectionDensity density)
    {
        var (padding, _, _) = FWListBox.GetListDensityMetrics(density);
        gridView.Padding = padding;
    }

    protected override FrameworkElement GetContainerForItem(object item)
    {
        return new FWGridViewItem { Density = Density };
    }
}

/// <summary>
/// FluentJalium GridViewItem control.
/// </summary>
public class FWGridViewItem : ListViewItem, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWCollectionDensity), typeof(FWGridViewItem),
            new PropertyMetadata(FWCollectionDensity.Comfortable, OnDensityChanged));

    public FWGridViewItem()
    {
        FWListBox.ApplyListItemDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWCollectionDensity Density
    {
        get => (FWCollectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWGridViewItem item && e.NewValue is FWCollectionDensity density)
        {
            FWListBox.ApplyListItemDensity(item, density);
        }
    }
}

/// <summary>
/// FluentJalium TreeView control.
/// </summary>
public class FWTreeView : TreeView, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWCollectionDensity), typeof(FWTreeView),
            new PropertyMetadata(FWCollectionDensity.Comfortable, OnDensityChanged));

    public FWTreeView()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWCollectionDensity Density
    {
        get => (FWCollectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTreeView treeView && e.NewValue is FWCollectionDensity density)
        {
            ApplyDensity(treeView, density);
        }
    }

    private static void ApplyDensity(FWTreeView treeView, FWCollectionDensity density)
    {
        var (padding, _, _) = FWListBox.GetTreeDensityMetrics(density);
        treeView.Padding = padding;
    }

    protected override FrameworkElement GetContainerForItem(object item)
    {
        return item is TreeViewItem treeViewItem ? treeViewItem : new FWTreeViewItem { Density = Density };
    }
}

/// <summary>
/// FluentJalium TreeViewItem control.
/// </summary>
public class FWTreeViewItem : TreeViewItem, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWCollectionDensity), typeof(FWTreeViewItem),
            new PropertyMetadata(FWCollectionDensity.Comfortable, OnDensityChanged));

    public FWTreeViewItem()
    {
        FWListBox.ApplyTreeItemDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWCollectionDensity Density
    {
        get => (FWCollectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTreeViewItem item && e.NewValue is FWCollectionDensity density)
        {
            FWListBox.ApplyTreeItemDensity(item, density);
        }
    }

    protected override FrameworkElement GetContainerForItem(object item)
    {
        return item is TreeViewItem treeViewItem ? treeViewItem : new FWTreeViewItem { Density = Density };
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
