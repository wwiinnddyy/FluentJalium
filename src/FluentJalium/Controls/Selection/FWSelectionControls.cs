using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium selection density presets for compact data surfaces and spacious touch targets.
/// </summary>
public enum FWSelectionDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium CheckBox control.
/// </summary>
public class FWCheckBox : CheckBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWSelectionDensity), typeof(FWCheckBox),
            new PropertyMetadata(FWSelectionDensity.Comfortable, OnDensityChanged));

    public FWCheckBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSelectionDensity Density
    {
        get => (FWSelectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWSelectionDensity density)
    {
        return density switch
        {
            FWSelectionDensity.Compact => (22.0, new Thickness(6, 0, 0, 0)),
            FWSelectionDensity.Spacious => (32.0, new Thickness(10, 0, 0, 0)),
            _ => (24.0, new Thickness(8, 0, 0, 0))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWCheckBox checkBox && e.NewValue is FWSelectionDensity density)
        {
            ApplyDensity(checkBox, density);
        }
    }

    private static void ApplyDensity(FWCheckBox checkBox, FWSelectionDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        checkBox.MinHeight = minHeight;
        checkBox.Padding = padding;
    }
}

/// <summary>
/// FluentJalium RadioButton control.
/// </summary>
public class FWRadioButton : RadioButton, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWSelectionDensity), typeof(FWRadioButton),
            new PropertyMetadata(FWSelectionDensity.Comfortable, OnDensityChanged));

    public FWRadioButton()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSelectionDensity Density
    {
        get => (FWSelectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRadioButton radioButton && e.NewValue is FWSelectionDensity density)
        {
            ApplyDensity(radioButton, density);
        }
    }

    private static void ApplyDensity(FWRadioButton radioButton, FWSelectionDensity density)
    {
        var (minHeight, padding) = FWCheckBox.GetDensityMetrics(density);
        radioButton.MinHeight = minHeight;
        radioButton.Padding = padding;
    }
}

/// <summary>
/// FluentJalium RadioButtons grouped selection control.
/// </summary>
public class FWRadioButtons : Selector, IFluentJaliumControl
{
    private readonly string _groupName = $"FWRadioButtons_{Guid.NewGuid():N}";
    private bool _updatingCheckedState;

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(FWRadioButtons),
            new PropertyMetadata(null, OnHeaderChanged));

    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(FWRadioButtons),
            new PropertyMetadata(null, OnHeaderChanged));

    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWSelectionDensity), typeof(FWRadioButtons),
            new PropertyMetadata(FWSelectionDensity.Comfortable, OnDensityChanged));

    public FWRadioButtons()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSelectionDensity Density
    {
        get => (FWSelectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    protected override FrameworkElement GetContainerForItem(object item)
    {
        return new FWRadioButton { Density = Density };
    }

    protected override bool IsItemItsOwnContainer(object item)
    {
        return item is FWRadioButton;
    }

    protected override void PrepareContainerForItem(FrameworkElement element, object item)
    {
        base.PrepareContainerForItem(element, item);

        if (element is FWRadioButton radioButton)
        {
            radioButton.Checked -= OnRadioButtonChecked;
            radioButton.Density = Density;

            if (string.IsNullOrEmpty(radioButton.GroupName))
            {
                radioButton.GroupName = _groupName;
            }

            radioButton.Checked += OnRadioButtonChecked;
        }

        UpdateContainerSelection();
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        UpdateContainerSelection();
        base.OnSelectionChanged(e);
    }

    protected override void UpdateContainerSelection()
    {
        if (_updatingCheckedState)
        {
            return;
        }

        var host = ItemsHost;
        if (host == null)
        {
            return;
        }

        _updatingCheckedState = true;
        try
        {
            for (var index = 0; index < host.Children.Count; index++)
            {
                if (host.Children[index] is FWRadioButton radioButton)
                {
                    radioButton.IsChecked = index == SelectedIndex;
                }
            }
        }
        finally
        {
            _updatingCheckedState = false;
        }
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRadioButtons radioButtons && e.NewValue is FWSelectionDensity density)
        {
            ApplyDensity(radioButtons, density);
            radioButtons.ApplyGeneratedItemDensity(density);
        }
    }

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRadioButtons radioButtons)
        {
            radioButtons.InvalidateMeasure();
            radioButtons.InvalidateVisual();
        }
    }

    private static void ApplyDensity(FWRadioButtons radioButtons, FWSelectionDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        radioButtons.MinHeight = minHeight;
        radioButtons.Padding = padding;
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWSelectionDensity density)
    {
        return density switch
        {
            FWSelectionDensity.Compact => (22.0, new Thickness(0, 2, 0, 2)),
            FWSelectionDensity.Spacious => (32.0, new Thickness(0, 6, 0, 6)),
            _ => (24.0, new Thickness(0, 4, 0, 4))
        };
    }

    private void ApplyGeneratedItemDensity(FWSelectionDensity density)
    {
        var host = ItemsHost;
        if (host == null)
        {
            return;
        }

        foreach (var child in host.Children)
        {
            if (child is FWRadioButton radioButton)
            {
                radioButton.Density = density;
            }
        }
    }

    private void OnRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        if (_updatingCheckedState || sender is not FWRadioButton radioButton)
        {
            return;
        }

        var host = ItemsHost;
        if (host == null)
        {
            return;
        }

        var index = host.Children.IndexOf(radioButton);
        if (index >= 0 && index != SelectedIndex)
        {
            SelectedIndex = index;
        }
    }
}

/// <summary>
/// FluentJalium ComboBox control.
/// </summary>
public class FWComboBox : ComboBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWSelectionDensity), typeof(FWComboBox),
            new PropertyMetadata(FWSelectionDensity.Comfortable, OnDensityChanged));

    public FWComboBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSelectionDensity Density
    {
        get => (FWSelectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    protected override FrameworkElement GetContainerForItem(object item)
    {
        return new FWComboBoxItem { Density = Density };
    }

    protected override void PrepareContainerForItem(FrameworkElement element, object item)
    {
        base.PrepareContainerForItem(element, item);

        if (element is FWComboBoxItem comboBoxItem && !ReferenceEquals(element, item))
        {
            comboBoxItem.Density = Density;
        }
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWComboBox comboBox && e.NewValue is FWSelectionDensity density)
        {
            ApplyDensity(comboBox, density);
            comboBox.ApplyGeneratedItemDensity(density);
        }
    }

    internal static (double MinHeight, double MinWidth, Thickness Padding) GetDensityMetrics(FWSelectionDensity density)
    {
        return density switch
        {
            FWSelectionDensity.Compact => (30.0, 120.0, new Thickness(8, 4, 8, 5)),
            FWSelectionDensity.Spacious => (40.0, 144.0, new Thickness(12, 8, 10, 8)),
            _ => (34.0, 120.0, new Thickness(10, 5, 8, 6))
        };
    }

    private static void ApplyDensity(FWComboBox comboBox, FWSelectionDensity density)
    {
        var (minHeight, minWidth, padding) = GetDensityMetrics(density);
        comboBox.MinHeight = minHeight;
        comboBox.MinWidth = minWidth;
        comboBox.Padding = padding;
    }

    private void ApplyGeneratedItemDensity(FWSelectionDensity density)
    {
        var host = ItemsHost;
        if (host == null)
        {
            return;
        }

        foreach (var child in host.Children)
        {
            if (child is FWComboBoxItem item && !ReferenceEquals(item, item.Tag))
            {
                item.Density = density;
            }
        }
    }
}

/// <summary>
/// FluentJalium ComboBoxItem control.
/// </summary>
public class FWComboBoxItem : ComboBoxItem, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWSelectionDensity), typeof(FWComboBoxItem),
            new PropertyMetadata(FWSelectionDensity.Comfortable, OnDensityChanged));

    public FWComboBoxItem()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSelectionDensity Density
    {
        get => (FWSelectionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWComboBoxItem item && e.NewValue is FWSelectionDensity density)
        {
            ApplyDensity(item, density);
        }
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWSelectionDensity density)
    {
        return density switch
        {
            FWSelectionDensity.Compact => (28.0, new Thickness(8, 4, 8, 4)),
            FWSelectionDensity.Spacious => (38.0, new Thickness(12, 8, 12, 8)),
            _ => (32.0, new Thickness(9, 6, 10, 6))
        };
    }

    private static void ApplyDensity(FWComboBoxItem item, FWSelectionDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        item.MinHeight = minHeight;
        item.Padding = padding;
    }
}
