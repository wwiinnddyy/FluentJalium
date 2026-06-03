using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FluentJalium.Icon;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium MenuBar control.
/// </summary>
public class FWMenuBar : MenuBar, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium MenuBarItem control.
/// </summary>
public class FWMenuBarItem : MenuBarItem, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Menu control.
/// </summary>
public class FWMenu : Menu, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item) => new FWMenuItem();

    protected override bool IsItemItsOwnContainer(object item) => item is MenuItem or Separator;

    protected override void PrepareContainerForItem(FrameworkElement element, object item)
    {
        base.PrepareContainerForItem(element, item);

        if (!ReferenceEquals(element, item) && element is FWMenuItem menuItem && menuItem.Header == null)
        {
            menuItem.Header = item;
        }
    }
}

/// <summary>
/// FluentJalium MenuItem control.
/// </summary>
public class FWMenuItem : MenuItem, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item) => new FWMenuItem();

    protected override bool IsItemItsOwnContainer(object item) => item is MenuItem or Separator;

    protected override void PrepareContainerForItem(FrameworkElement element, object item)
    {
        base.PrepareContainerForItem(element, item);

        if (!ReferenceEquals(element, item) && element is FWMenuItem menuItem && menuItem.Header == null)
        {
            menuItem.Header = item;
        }
    }
}

/// <summary>
/// FluentJalium ContextMenu control.
/// </summary>
public class FWContextMenu : ContextMenu, IFluentJaliumControl
{
    protected override FrameworkElement GetContainerForItem(object item) => new FWMenuItem();

    protected override bool IsItemItsOwnContainer(object item) => item is MenuItem or Separator;

    protected override void PrepareContainerForItem(FrameworkElement element, object item)
    {
        base.PrepareContainerForItem(element, item);

        if (!ReferenceEquals(element, item) && element is FWMenuItem menuItem && menuItem.Header == null)
        {
            menuItem.Header = item;
        }
    }
}

/// <summary>
/// FluentJalium MenuFlyoutItem control.
/// </summary>
public class FWMenuFlyoutItem : FluentMenuFlyoutItemBase, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ToggleMenuFlyoutItem control.
/// </summary>
public class FWToggleMenuFlyoutItem : FluentToggleMenuFlyoutItemBase, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium MenuFlyoutSeparator control.
/// </summary>
public class FWMenuFlyoutSeparator : MenuFlyoutSeparator, IFluentJaliumControl
{
}

public abstract class FluentMenuFlyoutItemBase : MenuFlyoutItem
{
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        FWMenuFlyoutIconRenderer.DrawFluentIcon(this, drawingContext, Icon);
    }
}

public abstract class FluentToggleMenuFlyoutItemBase : ToggleMenuFlyoutItem
{
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        FWMenuFlyoutIconRenderer.DrawFluentIcon(this, drawingContext, Icon);
    }
}

internal static class FWMenuFlyoutIconRenderer
{
    private const double LeftPadding = 12;
    private const double IconSize = 14;
    private static readonly SolidColorBrush s_fallbackTextBrush = new(Color.FromRgb(255, 255, 255));
    private static readonly SolidColorBrush s_fallbackDisabledTextBrush = new(Color.FromRgb(90, 90, 90));

    public static void DrawFluentIcon(Control owner, DrawingContext drawingContext, object? icon)
    {
        if (icon is not string iconText || string.IsNullOrEmpty(iconText))
        {
            return;
        }

        var foreground = owner.IsEnabled
            ? ResolveBrush(owner, "OnePopupText", "TextPrimary", s_fallbackTextBrush)
            : ResolveBrush(owner, "OneTextDisabled", "TextDisabled", s_fallbackDisabledTextBrush);
        var formatted = new FormattedText(iconText, FluentIcon.RegularFontFamily, IconSize)
        {
            Foreground = foreground
        };
        TextMeasurement.MeasureText(formatted);
        drawingContext.DrawText(formatted, new Point(LeftPadding, Math.Max(0, (owner.RenderSize.Height - formatted.Height) / 2)));
    }

    private static Brush ResolveBrush(Control owner, string primaryKey, string secondaryKey, Brush fallback)
    {
        if (owner.HasLocalValue(Control.ForegroundProperty) && owner.Foreground != null)
        {
            return owner.Foreground;
        }

        if (owner.TryFindResource(primaryKey) is Brush primary)
        {
            return primary;
        }

        if (owner.TryFindResource(secondaryKey) is Brush secondary)
        {
            return secondary;
        }

        return fallback;
    }
}
