using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;

namespace FluentJalium.Controls;

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
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(FWToggleSwitch),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets supporting text shown under the switch header in the FluentJalium template.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
}
