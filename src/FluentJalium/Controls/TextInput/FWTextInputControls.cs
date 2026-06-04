using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium text input density presets.
/// </summary>
public enum FWTextInputDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium numeric input density presets.
/// </summary>
public enum FWNumberBoxDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium TextBox control.
/// </summary>
public class FWTextBox : TextBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWTextInputDensity), typeof(FWTextBox),
            new PropertyMetadata(FWTextInputDensity.Comfortable, OnDensityChanged));

    public FWTextBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWTextInputDensity Density
    {
        get => (FWTextInputDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWTextInputDensity density)
    {
        return density switch
        {
            FWTextInputDensity.Compact => (30.0, new Thickness(8, 4, 8, 5)),
            FWTextInputDensity.Spacious => (40.0, new Thickness(12, 8, 12, 8)),
            _ => (34.0, new Thickness(10, 6, 10, 6))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTextBox textBox && e.NewValue is FWTextInputDensity density)
        {
            ApplyDensity(textBox, density);
        }
    }

    private static void ApplyDensity(FWTextBox textBox, FWTextInputDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        textBox.MinHeight = minHeight;
        textBox.Padding = padding;
    }
}

/// <summary>
/// FluentJalium PasswordBox control.
/// </summary>
public class FWPasswordBox : PasswordBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWTextInputDensity), typeof(FWPasswordBox),
            new PropertyMetadata(FWTextInputDensity.Comfortable, OnDensityChanged));

    public FWPasswordBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWTextInputDensity Density
    {
        get => (FWTextInputDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWPasswordBox passwordBox && e.NewValue is FWTextInputDensity density)
        {
            ApplyDensity(passwordBox, density);
        }
    }

    private static void ApplyDensity(FWPasswordBox passwordBox, FWTextInputDensity density)
    {
        var (minHeight, padding) = FWTextBox.GetDensityMetrics(density);
        passwordBox.MinHeight = minHeight;
        passwordBox.Padding = padding;
    }
}

/// <summary>
/// FluentJalium NumberBox control.
/// </summary>
public class FWNumberBox : NumberBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWNumberBoxDensity), typeof(FWNumberBox),
            new PropertyMetadata(FWNumberBoxDensity.Comfortable, OnDensityChanged));

    public FWNumberBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWNumberBoxDensity Density
    {
        get => (FWNumberBoxDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWNumberBox numberBox && e.NewValue is FWNumberBoxDensity density)
        {
            ApplyDensity(numberBox, density);
        }
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWNumberBoxDensity density)
    {
        return density switch
        {
            FWNumberBoxDensity.Compact => (30.0, new Thickness(8, 4, 8, 5)),
            FWNumberBoxDensity.Spacious => (40.0, new Thickness(12, 8, 12, 8)),
            _ => (34.0, new Thickness(10, 6, 10, 6))
        };
    }

    private static void ApplyDensity(FWNumberBox numberBox, FWNumberBoxDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        numberBox.MinHeight = minHeight;
        numberBox.Padding = padding;
    }
}

/// <summary>
/// FluentJalium AutoCompleteBox control.
/// </summary>
public class FWAutoCompleteBox : AutoCompleteBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWTextInputDensity), typeof(FWAutoCompleteBox),
            new PropertyMetadata(FWTextInputDensity.Comfortable, OnDensityChanged));

    public FWAutoCompleteBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWTextInputDensity Density
    {
        get => (FWTextInputDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double MinHeight, Thickness Padding, double MaxDropDownHeight) GetDensityMetrics(FWTextInputDensity density)
    {
        var (minHeight, padding) = FWTextBox.GetDensityMetrics(density);
        var maxDropDownHeight = density switch
        {
            FWTextInputDensity.Compact => 192.0,
            FWTextInputDensity.Spacious => 288.0,
            _ => 224.0
        };

        return (minHeight, padding, maxDropDownHeight);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWAutoCompleteBox autoCompleteBox && e.NewValue is FWTextInputDensity density)
        {
            ApplyDensity(autoCompleteBox, density);
        }
    }

    private static void ApplyDensity(FWAutoCompleteBox autoCompleteBox, FWTextInputDensity density)
    {
        var (minHeight, padding, maxDropDownHeight) = GetDensityMetrics(density);
        autoCompleteBox.MinHeight = minHeight;
        autoCompleteBox.Padding = padding;
        autoCompleteBox.MaxDropDownHeight = maxDropDownHeight;
    }
}

/// <summary>
/// FluentJalium RichTextBox control.
/// </summary>
public class FWRichTextBox : RichTextBox, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWTextInputDensity), typeof(FWRichTextBox),
            new PropertyMetadata(FWTextInputDensity.Comfortable, OnDensityChanged));

    public FWRichTextBox()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWTextInputDensity Density
    {
        get => (FWTextInputDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWTextInputDensity density)
    {
        return density switch
        {
            FWTextInputDensity.Compact => (80.0, new Thickness(8)),
            FWTextInputDensity.Spacious => (128.0, new Thickness(12)),
            _ => (96.0, new Thickness(10))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRichTextBox richTextBox && e.NewValue is FWTextInputDensity density)
        {
            ApplyDensity(richTextBox, density);
        }
    }

    private static void ApplyDensity(FWRichTextBox richTextBox, FWTextInputDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        richTextBox.MinHeight = minHeight;
        richTextBox.Padding = padding;
    }
}
