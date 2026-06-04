using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium density presets for code, binary, and structured data inspector surfaces.
/// </summary>
public enum FWDataInspectorDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium DiffViewer control.
/// </summary>
public class FWDiffViewer : DiffViewer, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDataInspectorDensity), typeof(FWDiffViewer),
            new PropertyMetadata(FWDataInspectorDensity.Comfortable, OnDensityChanged));

    public FWDiffViewer()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDataInspectorDensity Density
    {
        get => (FWDataInspectorDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double FontSize, double GutterWidth, double MinWidth, double MinHeight, CornerRadius CornerRadius) GetDensityMetrics(FWDataInspectorDensity density)
    {
        return density switch
        {
            FWDataInspectorDensity.Compact => (12.0, 48.0, 220.0, 120.0, new CornerRadius(4)),
            FWDataInspectorDensity.Spacious => (14.0, 72.0, 280.0, 180.0, new CornerRadius(8)),
            _ => (13.0, 60.0, 240.0, 140.0, new CornerRadius(6))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWDiffViewer viewer && e.NewValue is FWDataInspectorDensity density)
        {
            ApplyDensity(viewer, density);
        }
    }

    private static void ApplyDensity(FWDiffViewer viewer, FWDataInspectorDensity density)
    {
        var (fontSize, gutterWidth, minWidth, minHeight, cornerRadius) = GetDensityMetrics(density);
        viewer.FontSize = fontSize;
        viewer.GutterWidth = gutterWidth;
        viewer.MinWidth = minWidth;
        viewer.MinHeight = minHeight;
        viewer.CornerRadius = cornerRadius;
    }
}

/// <summary>
/// FluentJalium HexEditor control.
/// </summary>
public class FWHexEditor : HexEditor, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDataInspectorDensity), typeof(FWHexEditor),
            new PropertyMetadata(FWDataInspectorDensity.Comfortable, OnDensityChanged));

    public FWHexEditor()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDataInspectorDensity Density
    {
        get => (FWDataInspectorDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double FontSize, Thickness Padding, double MinWidth, double MinHeight, CornerRadius CornerRadius) GetDensityMetrics(FWDataInspectorDensity density)
    {
        return density switch
        {
            FWDataInspectorDensity.Compact => (12.0, new Thickness(4), 220.0, 120.0, new CornerRadius(4)),
            FWDataInspectorDensity.Spacious => (14.0, new Thickness(8), 280.0, 180.0, new CornerRadius(8)),
            _ => (13.0, new Thickness(6), 240.0, 140.0, new CornerRadius(6))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWHexEditor editor && e.NewValue is FWDataInspectorDensity density)
        {
            ApplyDensity(editor, density);
        }
    }

    private static void ApplyDensity(FWHexEditor editor, FWDataInspectorDensity density)
    {
        var (fontSize, padding, minWidth, minHeight, cornerRadius) = GetDensityMetrics(density);
        editor.FontSize = fontSize;
        editor.Padding = padding;
        editor.MinWidth = minWidth;
        editor.MinHeight = minHeight;
        editor.CornerRadius = cornerRadius;
    }
}

/// <summary>
/// FluentJalium JsonTreeViewer control.
/// </summary>
public class FWJsonTreeViewer : JsonTreeViewer, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWDataInspectorDensity), typeof(FWJsonTreeViewer),
            new PropertyMetadata(FWDataInspectorDensity.Comfortable, OnDensityChanged));

    public FWJsonTreeViewer()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWDataInspectorDensity Density
    {
        get => (FWDataInspectorDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double FontSize, double IndentSize, double MinWidth, double MinHeight, CornerRadius CornerRadius) GetDensityMetrics(FWDataInspectorDensity density)
    {
        return density switch
        {
            FWDataInspectorDensity.Compact => (12.0, 16.0, 220.0, 160.0, new CornerRadius(4)),
            FWDataInspectorDensity.Spacious => (14.0, 24.0, 300.0, 220.0, new CornerRadius(8)),
            _ => (13.0, 20.0, 240.0, 180.0, new CornerRadius(6))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWJsonTreeViewer viewer && e.NewValue is FWDataInspectorDensity density)
        {
            ApplyDensity(viewer, density);
        }
    }

    private static void ApplyDensity(FWJsonTreeViewer viewer, FWDataInspectorDensity density)
    {
        var (fontSize, indentSize, minWidth, minHeight, cornerRadius) = GetDensityMetrics(density);
        viewer.FontSize = fontSize;
        viewer.IndentSize = indentSize;
        viewer.MinWidth = minWidth;
        viewer.MinHeight = minHeight;
        viewer.CornerRadius = cornerRadius;
    }
}
