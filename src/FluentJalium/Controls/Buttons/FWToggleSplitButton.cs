using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium toggle split button control.
/// </summary>
public class FWToggleSplitButton : SplitButton, IFluentJaliumControl
{
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(FWToggleSplitButton),
            new PropertyMetadata(false, OnIsCheckedChanged));

    /// <summary>
    /// Gets or sets whether the primary action is checked.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty)!;
        set => SetValue(IsCheckedProperty, value);
    }

    public event EventHandler<FWToggleSplitButtonIsCheckedChangedEventArgs>? IsCheckedChanged;

    /// <summary>
    /// Toggles the checked state.
    /// </summary>
    public void Toggle()
    {
        IsChecked = !IsChecked;
    }

    protected override void OnClick(SplitButtonClickEventArgs args)
    {
        Toggle();
        base.OnClick(args);
    }

    private void OnIsCheckedChanged(bool oldValue, bool newValue)
    {
        IsCheckedChanged?.Invoke(this, new FWToggleSplitButtonIsCheckedChangedEventArgs(oldValue, newValue));
    }

    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWToggleSplitButton button)
        {
            button.OnIsCheckedChanged((bool)e.OldValue!, (bool)e.NewValue!);
        }
    }
}

/// <summary>
/// Event data for <see cref="FWToggleSplitButton.IsCheckedChanged"/>.
/// </summary>
public sealed class FWToggleSplitButtonIsCheckedChangedEventArgs : EventArgs
{
    public FWToggleSplitButtonIsCheckedChangedEventArgs(bool oldValue, bool newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public bool OldValue { get; }

    public bool NewValue { get; }
}
