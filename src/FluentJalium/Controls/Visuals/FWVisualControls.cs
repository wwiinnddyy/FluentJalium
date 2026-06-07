using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Shapes;
using ShapePath = Jalium.UI.Controls.Shapes.Path;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium Image control.
/// </summary>
public class FWImage : Image, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium BitmapIcon compatibility control.
/// </summary>
public class FWBitmapIcon : Image, IFluentJaliumControl
{
    public static readonly DependencyProperty ShowAsMonochromeProperty =
        DependencyProperty.Register(nameof(ShowAsMonochrome), typeof(bool), typeof(FWBitmapIcon),
            new PropertyMetadata(true, OnIconPropertyChanged));

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public bool ShowAsMonochrome
    {
        get => (bool)GetValue(ShowAsMonochromeProperty)!;
        set => SetValue(ShowAsMonochromeProperty, value);
    }

    private static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWBitmapIcon icon)
        {
            icon.InvalidateVisual();
        }
    }
}

/// <summary>
/// FluentJalium ImageIcon compatibility control.
/// </summary>
public class FWImageIcon : Image, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Markdown control.
/// </summary>
public class FWMarkdown : Markdown, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium RichTextBlock compatibility control.
/// </summary>
public class FWRichTextBlock : TextBlock, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium QRCode control.
/// </summary>
public class FWQRCode : QRCode, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium FontIcon control.
/// </summary>
public class FWFontIcon : FontIcon, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium SymbolIcon control.
/// </summary>
public class FWSymbolIcon : SymbolIcon, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium PathIcon control.
/// </summary>
public class FWPathIcon : PathIcon, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Viewbox control.
/// </summary>
public class FWViewbox : Viewbox, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Label control.
/// </summary>
public class FWLabel : Label, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Separator control.
/// </summary>
public class FWSeparator : Separator, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Rectangle shape.
/// </summary>
public class FWRectangle : Rectangle, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Ellipse shape.
/// </summary>
public class FWEllipse : Ellipse, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Line shape.
/// </summary>
public class FWLine : Line, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Polyline shape.
/// </summary>
public class FWPolyline : Polyline, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Polygon shape.
/// </summary>
public class FWPolygon : Polygon, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Path shape.
/// </summary>
public class FWPath : ShapePath, IFluentJaliumControl
{
}
