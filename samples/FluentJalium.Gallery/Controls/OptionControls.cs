using FluentJalium.Gallery.Services;
using Jalium.UI.Controls;
using Jalium.UI;

namespace FluentJalium.Gallery.Controls;

/// <summary>
/// Base class for interactive option controls in Gallery examples.
/// </summary>
public abstract class OptionControl : Control
{
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label),
        typeof(string),
        typeof(OptionControl),
        new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the label displayed for this option.
    /// </summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }
}

/// <summary>
/// Numeric option control with slider and text input.
/// </summary>
public sealed class NumericOption : OptionControl
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(NumericOption),
        new PropertyMetadata(0.0, OnValueChanged));

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum),
        typeof(double),
        typeof(NumericOption),
        new PropertyMetadata(0.0));

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum),
        typeof(double),
        typeof(NumericOption),
        new PropertyMetadata(100.0));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public event EventHandler<double>? ValueChanged;

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericOption option)
        {
            option.ValueChanged?.Invoke(option, (double)e.NewValue);
        }
    }
}

/// <summary>
/// Boolean option control with checkbox or toggle switch.
/// </summary>
public sealed class BooleanOption : OptionControl
{
    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        nameof(IsChecked),
        typeof(bool),
        typeof(BooleanOption),
        new PropertyMetadata(false, OnIsCheckedChanged));

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public event EventHandler<bool>? ValueChanged;

    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BooleanOption option)
        {
            option.ValueChanged?.Invoke(option, (bool)e.NewValue);
        }
    }
}

/// <summary>
/// Enum option control with combo box.
/// </summary>
public sealed class EnumOption : OptionControl
{
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(System.Collections.IEnumerable),
        typeof(EnumOption),
        new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem),
        typeof(object),
        typeof(EnumOption),
        new PropertyMetadata(null, OnSelectedItemChanged));

    public System.Collections.IEnumerable? ItemsSource
    {
        get => (System.Collections.IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public event EventHandler<object>? ValueChanged;

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EnumOption option && e.NewValue != null)
        {
            option.ValueChanged?.Invoke(option, e.NewValue);
        }
    }
}

/// <summary>
/// Helper class for binding option controls to target UI elements.
/// </summary>
public static class OptionBinding
{
    public static void BindNumeric(
        UIElement target,
        DependencyProperty property,
        NumericOption option)
    {
        // Set initial value
        target.SetValue(property, option.Value);

        // Bind to changes
        option.ValueChanged += (s, value) =>
        {
            target.SetValue(property, value);
        };
    }

    public static void BindBoolean(
        UIElement target,
        DependencyProperty property,
        BooleanOption option)
    {
        // Set initial value
        target.SetValue(property, option.IsChecked);

        // Bind to changes
        option.ValueChanged += (s, value) =>
        {
            target.SetValue(property, value);
        };
    }

    public static void BindEnum(
        UIElement target,
        DependencyProperty property,
        EnumOption option)
    {
        // Set initial value
        if (option.SelectedItem != null)
        {
            target.SetValue(property, option.SelectedItem);
        }

        // Bind to changes
        option.ValueChanged += (s, value) =>
        {
            target.SetValue(property, value);
        };
    }
}
