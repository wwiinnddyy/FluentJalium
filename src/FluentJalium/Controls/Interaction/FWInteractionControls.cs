using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium interaction density presets for scroll and drag surfaces.
/// </summary>
public enum FWInteractionDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// FluentJalium ScrollViewer control.
/// </summary>
public class FWScrollViewer : ScrollViewer, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ScrollBar control.
/// </summary>
public class FWScrollBar : ScrollBar, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWInteractionDensity), typeof(FWScrollBar),
            new PropertyMetadata(FWInteractionDensity.Comfortable, OnDensityChanged));

    public FWScrollBar()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWInteractionDensity Density
    {
        get => (FWInteractionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWScrollBar scrollBar && e.NewValue is FWInteractionDensity density)
        {
            ApplyDensity(scrollBar, density);
        }
    }

    internal static (double Thickness, Thickness Padding, CornerRadius CornerRadius) GetDensityMetrics(FWInteractionDensity density)
    {
        return density switch
        {
            FWInteractionDensity.Compact => (8.0, new Thickness(1), new CornerRadius(4)),
            FWInteractionDensity.Spacious => (16.0, new Thickness(3), new CornerRadius(8)),
            _ => (12.0, new Thickness(2), new CornerRadius(6))
        };
    }

    private static void ApplyDensity(FWScrollBar scrollBar, FWInteractionDensity density)
    {
        var (thickness, padding, cornerRadius) = GetDensityMetrics(density);
        scrollBar.MinWidth = thickness;
        scrollBar.MinHeight = thickness;
        scrollBar.Padding = padding;
        scrollBar.CornerRadius = cornerRadius;
    }
}

/// <summary>
/// FluentJalium SwipeControl control.
/// </summary>
public class FWSwipeControl : SwipeControl, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWInteractionDensity), typeof(FWSwipeControl),
            new PropertyMetadata(FWInteractionDensity.Comfortable, OnDensityChanged));

    private Pen? _borderPen;
    private Brush? _borderPenBrush;
    private double _borderPenThickness;

    public FWSwipeControl()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWInteractionDensity Density
    {
        get => (FWInteractionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    /// <inheritdoc />
    protected override void OnRender(DrawingContext drawingContext)
    {
        var bounds = new Rect(0, 0, RenderSize.Width, RenderSize.Height);

        if (Background != null)
        {
            drawingContext.DrawRoundedRectangle(Background, null, bounds, CornerRadius);
        }

        base.OnRender(drawingContext);

        if (BorderBrush != null && BorderThickness.Left > 0)
        {
            if (_borderPen == null || _borderPenBrush != BorderBrush || _borderPenThickness != BorderThickness.Left)
            {
                _borderPen = new Pen(BorderBrush, BorderThickness.Left);
                _borderPenBrush = BorderBrush;
                _borderPenThickness = BorderThickness.Left;
            }

            drawingContext.DrawRoundedRectangle(null, _borderPen, bounds, CornerRadius);
        }
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSwipeControl swipeControl && e.NewValue is FWInteractionDensity density)
        {
            ApplyDensity(swipeControl, density);
        }
    }

    internal static (double MinHeight, Thickness Padding, CornerRadius CornerRadius) GetDensityMetrics(FWInteractionDensity density)
    {
        return density switch
        {
            FWInteractionDensity.Compact => (40.0, new Thickness(10, 6, 10, 6), new CornerRadius(4)),
            FWInteractionDensity.Spacious => (56.0, new Thickness(16, 12, 16, 12), new CornerRadius(8)),
            _ => (48.0, new Thickness(12, 8, 12, 8), new CornerRadius(6))
        };
    }

    private static void ApplyDensity(FWSwipeControl swipeControl, FWInteractionDensity density)
    {
        var (minHeight, padding, cornerRadius) = GetDensityMetrics(density);
        swipeControl.MinHeight = minHeight;
        swipeControl.Padding = padding;
        swipeControl.CornerRadius = cornerRadius;
    }
}

/// <summary>
/// FluentJalium GridSplitter control.
/// </summary>
public class FWGridSplitter : GridSplitter, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWInteractionDensity), typeof(FWGridSplitter),
            new PropertyMetadata(FWInteractionDensity.Comfortable, OnDensityChanged));

    public FWGridSplitter()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWInteractionDensity Density
    {
        get => (FWInteractionDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWGridSplitter splitter && e.NewValue is FWInteractionDensity density)
        {
            ApplyDensity(splitter, density);
        }
    }

    internal static double GetDensityThickness(FWInteractionDensity density)
    {
        return density switch
        {
            FWInteractionDensity.Compact => 4.0,
            FWInteractionDensity.Spacious => 8.0,
            _ => 6.0
        };
    }

    private static void ApplyDensity(FWGridSplitter splitter, FWInteractionDensity density)
    {
        var thickness = GetDensityThickness(density);
        splitter.MinWidth = thickness;
        splitter.MinHeight = thickness;
    }
}
