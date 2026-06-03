using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

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
/// FluentJalium ComboBox control.
/// </summary>
public class FWComboBox : ComboBox, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item) => new FWComboBoxItem();
}

/// <summary>
/// FluentJalium ComboBoxItem control.
/// </summary>
public class FWComboBoxItem : ComboBoxItem, IFluentJaliumControl
{
}
