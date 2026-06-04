using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

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
}

/// <summary>
/// FluentJalium RichTextBox control.
/// </summary>
public class FWRichTextBox : RichTextBox, IFluentJaliumControl
{
}
