using Jalium.UI;
using Jalium.UI.Media;
using Jalium.UI.Shapes;

namespace FluentJalium.Controls;

/// <summary>
/// Shared rendering base for the FluentJalium shape primitives.
/// </summary>
/// <remarks>
/// Jalium.UI seals <c>Rectangle</c>, <c>Ellipse</c>, <c>Line</c>, <c>Polyline</c>, <c>Polygon</c>
/// and <c>Path</c> for WPF API parity and leaves <see cref="Shape"/> open, so the themed
/// FluentJalium variants extend <see cref="Shape"/> and reproduce the leaf geometry contract
/// (<see cref="Shape.DefiningGeometry"/> plus an explicit <c>OnRender</c>).
/// </remarks>
public abstract class FWShape : Shape, IFluentJaliumControl
{
    /// <summary>
    /// Builds the stroke <see cref="Pen"/> from the inherited stroke properties,
    /// or <see langword="null"/> when there is nothing to stroke.
    /// </summary>
    protected Pen? BuildStrokePen()
    {
        var stroke = Stroke;
        if (stroke is null || StrokeThickness <= 0)
        {
            return null;
        }

        var pen = new Pen(stroke, StrokeThickness)
        {
            StartLineCap = StrokeStartLineCap,
            EndLineCap = StrokeEndLineCap,
            DashCap = StrokeDashCap,
            LineJoin = StrokeLineJoin,
            MiterLimit = StrokeMiterLimit,
        };

        var dashArray = StrokeDashArray;
        if (dashArray is { Count: > 0 })
        {
            pen.DashStyle = new DashStyle(dashArray, StrokeDashOffset);
        }

        return pen;
    }

    /// <summary>
    /// The render box inset by the stroke thickness so the outline stays inside the element bounds.
    /// </summary>
    protected Rect StrokeBox
    {
        get
        {
            var thickness = StrokeThickness;
            var half = thickness / 2;
            return new Rect(
                half,
                half,
                Math.Max(0, RenderSize.Width - thickness),
                Math.Max(0, RenderSize.Height - thickness));
        }
    }

    /// <summary>
    /// Resolves a stretch mode against geometry bounds into the matrix that maps the bounds
    /// into the stroked render box. Mirrors <c>Shape.GetStretchMetrics</c>, including the
    /// thin-dimension rule that keeps axis-aligned lines visible.
    /// </summary>
    protected static Matrix CreateStretchMatrix(
        Stretch mode,
        double strokeThickness,
        Size renderSize,
        Rect bounds)
    {
        if (mode == Stretch.None)
        {
            return Matrix.Identity;
        }

        bool boundsUsable =
            !double.IsNaN(bounds.Width) && !double.IsNaN(bounds.Height) &&
            !double.IsInfinity(bounds.Width) && !double.IsInfinity(bounds.Height) &&
            bounds.Width >= 0 && bounds.Height >= 0 &&
            (bounds.Width > 0 || bounds.Height > 0);

        if (!boundsUsable)
        {
            return Matrix.Identity;
        }

        var margin = strokeThickness / 2;
        var xScale = Math.Max(0, renderSize.Width - strokeThickness);
        var yScale = Math.Max(0, renderSize.Height - strokeThickness);
        var hasThinDimension = false;

        if (bounds.Width > xScale * double.Epsilon)
        {
            xScale /= bounds.Width;
        }
        else
        {
            xScale = 1;
            hasThinDimension |= bounds.Width == 0;
        }

        if (bounds.Height > yScale * double.Epsilon)
        {
            yScale /= bounds.Height;
        }
        else
        {
            yScale = 1;
            hasThinDimension |= bounds.Height == 0;
        }

        if (mode != Stretch.Fill && !hasThinDimension)
        {
            if (mode == Stretch.Uniform)
            {
                yScale = Math.Min(xScale, yScale);
                xScale = yScale;
            }
            else
            {
                xScale = Math.Max(xScale, yScale);
                yScale = xScale;
            }
        }

        var matrix = Matrix.Identity;
        matrix.Scale(xScale, yScale);
        matrix.Translate(margin - (bounds.X * xScale), margin - (bounds.Y * yScale));
        return matrix;
    }

    /// <summary>
    /// Sizes the shape to its explicit <see cref="FrameworkElement.Width"/> /
    /// <see cref="FrameworkElement.Height"/>, collapsing to zero when <see cref="Shape.Stretch"/>
    /// is <see cref="Stretch.None"/>.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Stretch == Stretch.None)
        {
            return default;
        }

        var width = double.IsNaN(Width) ? availableSize.Width : Width;
        var height = double.IsNaN(Height) ? availableSize.Height : Height;

        if (double.IsPositiveInfinity(width))
        {
            width = 0;
        }

        if (double.IsPositiveInfinity(height))
        {
            height = 0;
        }

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize) => finalSize;
}

/// <summary>
/// Shared vertex-list rendering for <see cref="FWPolyline"/> and <see cref="FWPolygon"/>.
/// </summary>
public abstract class FWShapeWithPoints : FWShape
{
    public static readonly DependencyProperty PointsProperty =
        DependencyProperty.Register(nameof(Points), typeof(PointCollection), typeof(FWShapeWithPoints),
            new PropertyMetadata(null, OnGeometryPropertyChanged));

    public static readonly DependencyProperty FillRuleProperty =
        DependencyProperty.Register(nameof(FillRule), typeof(FillRule), typeof(FWShapeWithPoints),
            new PropertyMetadata(FillRule.EvenOdd, OnGeometryPropertyChanged));

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Data)]
    public PointCollection? Points
    {
        get => (PointCollection?)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FillRule FillRule
    {
        get => (FillRule)(GetValue(FillRuleProperty) ?? FillRule.EvenOdd);
        set => SetValue(FillRuleProperty, value);
    }

    /// <summary>Lowest number of vertices that still produces a shape.</summary>
    protected abstract int MinimumPointCount { get; }

    /// <summary>Whether the figure closes back onto its start point.</summary>
    protected abstract bool IsFigureClosed { get; }

    private PathGeometry? _cachedGeometry;

    protected override Geometry? DefiningGeometry => EnsureGeometry();

    protected override Size MeasureOverride(Size availableSize)
    {
        var points = Points;
        if (points is null || points.Count < MinimumPointCount)
        {
            return default;
        }

        var bounds = GetBounds(points);
        var width = bounds.Width + StrokeThickness;
        var height = bounds.Height + StrokeThickness;

        return new Size(
            double.IsInfinity(availableSize.Width) ? width : Math.Min(width, availableSize.Width),
            double.IsInfinity(availableSize.Height) ? height : Math.Min(height, availableSize.Height));
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var points = Points;
        if (points is null || points.Count < MinimumPointCount)
        {
            return;
        }

        if (Fill is null && Stroke is null)
        {
            return;
        }

        var geometry = EnsureGeometry();
        if (geometry is null)
        {
            return;
        }

        drawingContext.DrawGeometry(Fill, BuildStrokePen(), geometry);
    }

    private PathGeometry? EnsureGeometry()
    {
        if (_cachedGeometry is not null)
        {
            return _cachedGeometry;
        }

        var points = Points;
        if (points is null || points.Count < MinimumPointCount)
        {
            return null;
        }

        var geometry = new PathGeometry { FillRule = FillRule };
        var figure = new PathFigure
        {
            StartPoint = points[0],
            IsClosed = IsFigureClosed,
            IsFilled = true,
        };

        for (var i = 1; i < points.Count; i++)
        {
            figure.Segments.Add(new LineSegment(points[i]));
        }

        geometry.Figures.Add(figure);
        _cachedGeometry = geometry;
        return geometry;
    }

    private static Rect GetBounds(PointCollection points)
    {
        if (points.Count == 0)
        {
            return Rect.Empty;
        }

        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;

        foreach (var point in points)
        {
            minX = Math.Min(minX, point.X);
            minY = Math.Min(minY, point.Y);
            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private static void OnGeometryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWShapeWithPoints shape)
        {
            shape._cachedGeometry = null;
            shape.InvalidateMeasure();
            shape.InvalidateVisual();
        }
    }
}

/// <summary>
/// FluentJalium Rectangle shape.
/// </summary>
public class FWRectangle : FWShape
{
    public static readonly DependencyProperty RadiusXProperty =
        DependencyProperty.Register(nameof(RadiusX), typeof(double), typeof(FWRectangle),
            new PropertyMetadata(0.0, OnRadiusChanged));

    public static readonly DependencyProperty RadiusYProperty =
        DependencyProperty.Register(nameof(RadiusY), typeof(double), typeof(FWRectangle),
            new PropertyMetadata(0.0, OnRadiusChanged));

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double RadiusX
    {
        get => (double)GetValue(RadiusXProperty)!;
        set => SetValue(RadiusXProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double RadiusY
    {
        get => (double)GetValue(RadiusYProperty)!;
        set => SetValue(RadiusYProperty, value);
    }

    protected override Geometry? DefiningGeometry
    {
        get
        {
            if (RenderSize.Width <= 0 || RenderSize.Height <= 0)
            {
                return null;
            }

            var box = StrokeBox;
            return (RadiusX > 0 || RadiusY > 0)
                ? new RectangleGeometry(box, RadiusX, RadiusY)
                : new RectangleGeometry(box);
        }
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (RenderSize.Width <= 0 || RenderSize.Height <= 0)
        {
            return;
        }

        var box = StrokeBox;
        var pen = BuildStrokePen();

        if (RadiusX > 0 || RadiusY > 0)
        {
            drawingContext.DrawRoundedRectangle(Fill, pen, box, RadiusX, RadiusY);
        }
        else
        {
            drawingContext.DrawRectangle(Fill, pen, box);
        }
    }

    private static void OnRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRectangle rectangle)
        {
            rectangle.InvalidateVisual();
        }
    }
}

/// <summary>
/// FluentJalium Ellipse shape.
/// </summary>
public class FWEllipse : FWShape
{
    protected override Geometry? DefiningGeometry
    {
        get
        {
            if (RenderSize.Width <= 0 || RenderSize.Height <= 0)
            {
                return null;
            }

            var thickness = StrokeThickness;
            return new EllipseGeometry
            {
                Center = new Point(RenderSize.Width / 2, RenderSize.Height / 2),
                RadiusX = Math.Max(0, (RenderSize.Width - thickness) / 2),
                RadiusY = Math.Max(0, (RenderSize.Height - thickness) / 2),
            };
        }
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (RenderSize.Width <= 0 || RenderSize.Height <= 0)
        {
            return;
        }

        var geometry = DefiningGeometry;
        if (geometry is null)
        {
            return;
        }

        drawingContext.DrawGeometry(Fill, BuildStrokePen(), geometry);
    }
}

/// <summary>
/// FluentJalium Line shape.
/// </summary>
public class FWLine : FWShape
{
    public static readonly DependencyProperty X1Property =
        DependencyProperty.Register(nameof(X1), typeof(double), typeof(FWLine),
            new PropertyMetadata(0.0, OnLinePointsChanged));

    public static readonly DependencyProperty Y1Property =
        DependencyProperty.Register(nameof(Y1), typeof(double), typeof(FWLine),
            new PropertyMetadata(0.0, OnLinePointsChanged));

    public static readonly DependencyProperty X2Property =
        DependencyProperty.Register(nameof(X2), typeof(double), typeof(FWLine),
            new PropertyMetadata(0.0, OnLinePointsChanged));

    public static readonly DependencyProperty Y2Property =
        DependencyProperty.Register(nameof(Y2), typeof(double), typeof(FWLine),
            new PropertyMetadata(0.0, OnLinePointsChanged));

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double X1
    {
        get => (double)GetValue(X1Property)!;
        set => SetValue(X1Property, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double Y1
    {
        get => (double)GetValue(Y1Property)!;
        set => SetValue(Y1Property, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double X2
    {
        get => (double)GetValue(X2Property)!;
        set => SetValue(X2Property, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double Y2
    {
        get => (double)GetValue(Y2Property)!;
        set => SetValue(Y2Property, value);
    }

    protected override Geometry? DefiningGeometry =>
        new LineGeometry(new Point(X1, Y1), new Point(X2, Y2));

    protected override Size MeasureOverride(Size availableSize)
    {
        var minX = Math.Min(X1, X2);
        var minY = Math.Min(Y1, Y2);
        var maxX = Math.Max(X1, X2);
        var maxY = Math.Max(Y1, Y2);

        var width = maxX - minX + StrokeThickness;
        var height = maxY - minY + StrokeThickness;

        return new Size(
            double.IsInfinity(availableSize.Width) ? width : Math.Min(width, availableSize.Width),
            double.IsInfinity(availableSize.Height) ? height : Math.Min(height, availableSize.Height));
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var pen = BuildStrokePen();
        if (pen is null)
        {
            return;
        }

        drawingContext.DrawLine(pen, new Point(X1, Y1), new Point(X2, Y2));
    }

    private static void OnLinePointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWLine line)
        {
            line.InvalidateMeasure();
            line.InvalidateVisual();
        }
    }
}

/// <summary>
/// FluentJalium Polyline shape.
/// </summary>
public class FWPolyline : FWShapeWithPoints
{
    protected override int MinimumPointCount => 2;

    protected override bool IsFigureClosed => false;
}

/// <summary>
/// FluentJalium Polygon shape.
/// </summary>
public class FWPolygon : FWShapeWithPoints
{
    protected override int MinimumPointCount => 3;

    protected override bool IsFigureClosed => true;
}

/// <summary>
/// FluentJalium Path shape.
/// </summary>
public class FWPath : FWShape
{
    static FWPath()
    {
        // As in WPF, a Path normalises its own bounds only when asked to. Defaulting to
        // Stretch.Fill made sibling paths inside one design-space container scale apart,
        // so icon layers lost their shared coordinate system.
        StretchProperty.OverrideMetadata(typeof(FWPath), new PropertyMetadata(Stretch.None, OnStretchChanged));
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(Geometry), typeof(FWPath),
            new PropertyMetadata(null, OnDataChanged));

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Data)]
    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    private Matrix _stretchMatrix = Matrix.Identity;

    protected override Geometry? DefiningGeometry => Data;

    public override Transform GeometryTransform =>
        _stretchMatrix.IsIdentity ? Transform.Identity : new MatrixTransform(_stretchMatrix);

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Data is null)
        {
            return default;
        }

        var bounds = Data.Bounds;
        if (bounds.IsEmpty)
        {
            return default;
        }

        var halfStroke = StrokeThickness / 2;
        var natural = new Size(
            Math.Max(bounds.Right + halfStroke, 0),
            Math.Max(bounds.Bottom + halfStroke, 0));

        if (Stretch == Stretch.None)
        {
            return natural;
        }

        return new Size(
            double.IsInfinity(availableSize.Width) ? natural.Width : Math.Min(natural.Width, availableSize.Width),
            double.IsInfinity(availableSize.Height) ? natural.Height : Math.Min(natural.Height, availableSize.Height));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _stretchMatrix = Data is { } data
            ? CreateStretchMatrix(Stretch, StrokeThickness, finalSize, data.Bounds)
            : Matrix.Identity;

        return finalSize;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (Data is null || RenderSize.Width <= 0 || RenderSize.Height <= 0)
        {
            return;
        }

        drawingContext.DrawGeometry(Fill, BuildStrokePen(), RenderedGeometry);
    }

    private static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWPath path)
        {
            path.InvalidateMeasure();
            path.InvalidateVisual();
        }
    }

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWPath path)
        {
            path.InvalidateMeasure();
            path.InvalidateVisual();
        }
    }
}
