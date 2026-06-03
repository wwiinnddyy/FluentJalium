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
/// FluentJalium PropertyGrid control.
/// </summary>
public class FWPropertyGrid : PropertyGrid, IFluentJaliumControl
{
}
