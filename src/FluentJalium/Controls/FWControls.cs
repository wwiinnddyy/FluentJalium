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
/// FluentJalium TextBox control.
/// </summary>
public class FWTextBox : TextBox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium PasswordBox control.
/// </summary>
public class FWPasswordBox : PasswordBox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium NumberBox control.
/// </summary>
public class FWNumberBox : NumberBox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium AutoCompleteBox control.
/// </summary>
public class FWAutoCompleteBox : AutoCompleteBox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium RichTextBox control.
/// </summary>
public class FWRichTextBox : RichTextBox, IFluentJaliumControl
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
/// FluentJalium DatePicker control.
/// </summary>
public class FWDatePicker : DatePicker, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium TimePicker control.
/// </summary>
public class FWTimePicker : TimePicker, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Calendar control.
/// </summary>
public class FWCalendar : Calendar, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium InfoBar control.
/// </summary>
public class FWInfoBar : InfoBar, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium toast notification item control.
/// </summary>
public class FWToastNotificationItem : ToastNotificationItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium toast notification host control.
/// </summary>
public class FWToastNotificationHost : ToastNotificationHost, IFluentJaliumControl
{
    /// <summary>
    /// Shows a FluentJalium toast notification with the specified severity, title, and message.
    /// </summary>
    public new FWToastNotificationItem Show(ToastSeverity severity, string title, string? message = null, TimeSpan? duration = null)
    {
        var toast = new FWToastNotificationItem
        {
            Severity = severity,
            Title = title,
            Message = message,
            IsAutoDismissEnabled = true,
            Duration = duration ?? TimeSpan.FromSeconds(5)
        };

        ShowToast(toast);
        return toast;
    }

    public new FWToastNotificationItem ShowInformation(string title, string? message = null, TimeSpan? duration = null)
        => Show(ToastSeverity.Information, title, message, duration);

    public new FWToastNotificationItem ShowSuccess(string title, string? message = null, TimeSpan? duration = null)
        => Show(ToastSeverity.Success, title, message, duration);

    public new FWToastNotificationItem ShowWarning(string title, string? message = null, TimeSpan? duration = null)
        => Show(ToastSeverity.Warning, title, message, duration);

    public new FWToastNotificationItem ShowError(string title, string? message = null, TimeSpan? duration = null)
        => Show(ToastSeverity.Error, title, message, duration);
}

/// <summary>
/// FluentJalium StatusBar control.
/// </summary>
public class FWStatusBar : StatusBar, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item) => new FWStatusBarItem();
}

/// <summary>
/// FluentJalium StatusBarItem control.
/// </summary>
public class FWStatusBarItem : Jalium.UI.Controls.StatusBarItem, IFluentJaliumControl
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
