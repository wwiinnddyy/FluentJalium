using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium ScrollViewer control.
/// </summary>
public class FWScrollViewer : ScrollViewer, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium SwipeControl control.
/// </summary>
public class FWSwipeControl : SwipeControl, IFluentJaliumControl
{
    private Pen? _borderPen;
    private Brush? _borderPenBrush;
    private double _borderPenThickness;

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
}

/// <summary>
/// FluentJalium GridSplitter control.
/// </summary>
public class FWGridSplitter : GridSplitter, IFluentJaliumControl
{
}
