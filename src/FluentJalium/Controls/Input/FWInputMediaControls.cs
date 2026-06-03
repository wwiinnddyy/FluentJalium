using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium ColorPicker control.
/// </summary>
public class FWColorPicker : ColorPicker, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium InkCanvas control.
/// </summary>
public class FWInkCanvas : InkCanvas, IFluentJaliumControl
{
    /// <summary>
    /// Identifies the BorderBrush dependency property.
    /// </summary>
    public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(FWInkCanvas),
            new PropertyMetadata(null, OnSurfacePropertyChanged));

    /// <summary>
    /// Identifies the BorderThickness dependency property.
    /// </summary>
    public static readonly DependencyProperty BorderThicknessProperty =
        DependencyProperty.Register(nameof(BorderThickness), typeof(Thickness), typeof(FWInkCanvas),
            new PropertyMetadata(new Thickness(0), OnSurfacePropertyChanged));

    /// <summary>
    /// Identifies the CornerRadius dependency property.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(FWInkCanvas),
            new PropertyMetadata(new CornerRadius(0), OnSurfacePropertyChanged));

    /// <summary>
    /// Gets or sets the brush used to draw the Fluent surface border.
    /// </summary>
    public Brush? BorderBrush
    {
        get => (Brush?)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the Fluent surface border thickness.
    /// </summary>
    public Thickness BorderThickness
    {
        get => (Thickness)GetValue(BorderThicknessProperty)!;
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the Fluent surface corner radius.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty)!;
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPostRender(DrawingContext drawingContext)
    {
        base.OnPostRender(drawingContext);
        DrawSurfaceBorder(drawingContext, BorderBrush, BorderThickness, CornerRadius);
    }

    private static void OnSurfacePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWInkCanvas canvas)
        {
            canvas.InvalidateVisual();
        }
    }

    private void DrawSurfaceBorder(DrawingContext drawingContext, Brush? borderBrush, Thickness thickness, CornerRadius cornerRadius)
    {
        if (borderBrush == null || thickness.Left <= 0 || RenderSize.Width <= 0 || RenderSize.Height <= 0)
            return;

        var halfThickness = thickness.Left / 2;
        var bounds = new Rect(
            halfThickness,
            halfThickness,
            Math.Max(0, RenderSize.Width - thickness.Left),
            Math.Max(0, RenderSize.Height - thickness.Left));
        var pen = new Pen(borderBrush, thickness.Left);
        drawingContext.DrawRoundedRectangle(null, pen, bounds, cornerRadius);
    }
}

/// <summary>
/// FluentJalium InkPresenter control.
/// </summary>
public class FWInkPresenter : InkPresenter, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium MediaElement control.
/// </summary>
public class FWMediaElement : MediaElement, IFluentJaliumControl
{
    /// <summary>
    /// Identifies the Background dependency property.
    /// </summary>
    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(FWMediaElement),
            new PropertyMetadata(null, OnSurfacePropertyChanged));

    /// <summary>
    /// Identifies the BorderBrush dependency property.
    /// </summary>
    public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(FWMediaElement),
            new PropertyMetadata(null, OnSurfacePropertyChanged));

    /// <summary>
    /// Identifies the BorderThickness dependency property.
    /// </summary>
    public static readonly DependencyProperty BorderThicknessProperty =
        DependencyProperty.Register(nameof(BorderThickness), typeof(Thickness), typeof(FWMediaElement),
            new PropertyMetadata(new Thickness(0), OnSurfacePropertyChanged));

    /// <summary>
    /// Identifies the CornerRadius dependency property.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(FWMediaElement),
            new PropertyMetadata(new CornerRadius(0), OnSurfacePropertyChanged));

    /// <summary>
    /// Gets or sets the Fluent surface background brush.
    /// </summary>
    public Brush? Background
    {
        get => (Brush?)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used to draw the Fluent surface border.
    /// </summary>
    public Brush? BorderBrush
    {
        get => (Brush?)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the Fluent surface border thickness.
    /// </summary>
    public Thickness BorderThickness
    {
        get => (Thickness)GetValue(BorderThicknessProperty)!;
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the Fluent surface corner radius.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty)!;
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <inheritdoc />
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (Background != null && !HasVideo && !HasAudio && RenderSize.Width > 0 && RenderSize.Height > 0)
        {
            drawingContext.DrawRoundedRectangle(
                Background,
                null,
                new Rect(0, 0, RenderSize.Width, RenderSize.Height),
                CornerRadius);
        }

        if (BorderBrush == null || BorderThickness.Left <= 0 || RenderSize.Width <= 0 || RenderSize.Height <= 0)
            return;

        var halfThickness = BorderThickness.Left / 2;
        var bounds = new Rect(
            halfThickness,
            halfThickness,
            Math.Max(0, RenderSize.Width - BorderThickness.Left),
            Math.Max(0, RenderSize.Height - BorderThickness.Left));
        drawingContext.DrawRoundedRectangle(null, new Pen(BorderBrush, BorderThickness.Left), bounds, CornerRadius);
    }

    private static void OnSurfacePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWMediaElement element)
        {
            element.InvalidateVisual();
        }
    }
}
