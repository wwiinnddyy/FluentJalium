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
/// FluentJalium DataGrid control.
/// </summary>
public class FWDataGrid : DataGrid, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium TreeDataGrid control.
/// </summary>
public class FWTreeDataGrid : TreeDataGrid, IFluentJaliumControl
{
}
