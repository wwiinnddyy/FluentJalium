using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium Button control.
/// </summary>
public class FWButton : Button, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium RepeatButton control.
/// </summary>
public class FWRepeatButton : RepeatButton, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium HyperlinkButton control.
/// </summary>
public class FWHyperlinkButton : HyperlinkButton, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium CheckBox control.
/// </summary>
public class FWCheckBox : CheckBox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium RadioButton control.
/// </summary>
public class FWRadioButton : RadioButton, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ToggleButton control.
/// </summary>
public class FWToggleButton : ToggleButton, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ToggleSwitch control.
/// </summary>
public class FWToggleSwitch : ToggleSwitch, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Slider control.
/// </summary>
public class FWSlider : Slider, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium RangeSlider control.
/// </summary>
public class FWRangeSlider : RangeSlider, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ProgressBar control.
/// </summary>
public class FWProgressBar : ProgressBar, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ComboBox control.
/// </summary>
public class FWComboBox : ComboBox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ComboBoxItem control.
/// </summary>
public class FWComboBoxItem : ComboBoxItem, IFluentJaliumControl
{
}

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

/// <summary>
/// FluentJalium NavigationView control.
/// </summary>
public class FWNavigationView : NavigationView, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium NavigationViewItem control.
/// </summary>
public class FWNavigationViewItem : NavigationViewItem, IFluentJaliumControl
{
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
}

/// <summary>
/// FluentJalium TabItem control.
/// </summary>
public class FWTabItem : TabItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Frame control.
/// </summary>
public class FWFrame : Frame, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium SplitButton control.
/// </summary>
public class FWSplitButton : SplitButton, IFluentJaliumControl
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
