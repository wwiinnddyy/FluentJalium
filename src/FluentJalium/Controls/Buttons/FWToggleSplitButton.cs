using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium toggle split button control.
/// </summary>
public class FWToggleSplitButton : SplitButton, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWButtonDensity), typeof(FWToggleSplitButton),
            new PropertyMetadata(FWButtonDensity.Comfortable, OnDensityChanged));

    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(FWToggleSplitButton),
            new PropertyMetadata(false, OnIsCheckedChanged));

    public FWToggleSplitButton()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWButtonDensity Density
    {
        get => (FWButtonDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

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

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWToggleSplitButton button && e.NewValue is FWButtonDensity density)
        {
            ApplyDensity(button, density);
        }
    }

    private static void ApplyDensity(FWToggleSplitButton button, FWButtonDensity density)
    {
        var (minHeight, minWidth, padding) = density switch
        {
            FWButtonDensity.Compact => (30.0, 104.0, new Thickness(10, 4, 10, 5)),
            FWButtonDensity.Spacious => (40.0, 136.0, new Thickness(14, 8, 14, 8)),
            _ => (32.0, 120.0, new Thickness(12, 5, 12, 6))
        };

        button.MinHeight = minHeight;
        button.MinWidth = minWidth;
        button.Padding = padding;
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
