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
/// FluentJalium SplitButton control.
/// </summary>
public class FWSplitButton : SplitButton, IFluentJaliumControl
{
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
